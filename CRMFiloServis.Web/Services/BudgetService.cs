using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class BudgetService : IBudgetService
{
    private readonly ApplicationDbContext _context;
    private static readonly string[] AyAdlari = { "", "Ocak", "Subat", "Mart", "Nisan", "Mayis", "Haziran", 
                                                   "Temmuz", "Agustos", "Eylul", "Ekim", "Kasim", "Aralik" };

    public BudgetService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Odeme Islemleri

    public async Task<List<BudgetOdeme>> GetOdemelerAsync(int yil, int? ay = null)
    {
        var query = _context.BudgetOdemeler
            .Where(o => o.OdemeYil == yil);

        if (ay.HasValue)
            query = query.Where(o => o.OdemeAy == ay.Value);

        return await query
            .OrderBy(o => o.OdemeAy)
            .ThenBy(o => o.OdemeTarihi)
            .ToListAsync();
    }

    /// <summary>
    /// Sadece bekleyen odemeleri getirir (Odenmis ve fatura ile kapatilmis olanlar haric)
    /// </summary>
    public async Task<List<BudgetOdeme>> GetBekleyenOdemelerAsync(int yil, int? ay = null)
    {
        var query = _context.BudgetOdemeler
            .Where(o => o.OdemeYil == yil && 
                        o.Durum != OdemeDurum.Odendi && 
                        !o.FaturaIleKapatildi);

        if (ay.HasValue)
            query = query.Where(o => o.OdemeAy == ay.Value);

        return await query
            .OrderBy(o => o.OdemeAy)
            .ThenBy(o => o.OdemeTarihi)
            .ToListAsync();
    }

    public async Task<List<BudgetOdeme>> GetOdemelerByDateRangeAsync(DateTime baslangic, DateTime bitis)
    {
        var baslangicUtc = DateTime.SpecifyKind(baslangic.Date, DateTimeKind.Utc);
        var bitisUtc = DateTime.SpecifyKind(bitis.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

        return await _context.BudgetOdemeler
            .Where(o => o.OdemeTarihi >= baslangicUtc && o.OdemeTarihi <= bitisUtc)
            .OrderBy(o => o.OdemeTarihi)
            .ToListAsync();
    }

    public async Task<BudgetOdeme?> GetOdemeByIdAsync(int id)
    {
        return await _context.BudgetOdemeler.FindAsync(id);
    }

    public async Task<BudgetOdeme> CreateOdemeAsync(BudgetOdeme odeme)
    {
        // DateTime'i UTC olarak ayarla
        odeme.OdemeTarihi = DateTime.SpecifyKind(odeme.OdemeTarihi, DateTimeKind.Utc);
        
        // Varsayilan degerler
        odeme.OdemeAy = odeme.OdemeTarihi.Month;
        odeme.OdemeYil = odeme.OdemeTarihi.Year;
        
        if (!odeme.TaksitliMi)
        {
            odeme.ToplamTaksitSayisi = 1;
            odeme.KacinciTaksit = 1;
        }
        
        odeme.CreatedAt = DateTime.UtcNow;

        _context.BudgetOdemeler.Add(odeme);
        await _context.SaveChangesAsync();
        return odeme;
    }

    public async Task<BudgetOdeme> UpdateOdemeAsync(BudgetOdeme odeme)
    {
        var existing = await _context.BudgetOdemeler.FindAsync(odeme.Id);
        if (existing == null)
            throw new Exception("Odeme bulunamadi");
        
        // DateTime'i UTC olarak ayarla
        existing.OdemeTarihi = DateTime.SpecifyKind(odeme.OdemeTarihi, DateTimeKind.Utc);
        existing.OdemeAy = odeme.OdemeTarihi.Month;
        existing.OdemeYil = odeme.OdemeTarihi.Year;
        existing.MasrafKalemi = odeme.MasrafKalemi;
        existing.Aciklama = odeme.Aciklama;
        existing.Miktar = odeme.Miktar;
        existing.Durum = odeme.Durum;
        existing.Notlar = odeme.Notlar;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    /// <summary>
    /// Soft delete - silinen hicbir yerde gorunmez
    /// </summary>
    public async Task DeleteOdemeAsync(int id)
    {
        var odeme = await _context.BudgetOdemeler.FindAsync(id);
        if (odeme != null)
        {
            odeme.IsDeleted = true;
            odeme.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Kasa'dan odeme yapildiginda:
    /// - Kasa = Borc (Cikis)
    /// - Odeme kaydi = Alacak olarak islenir
    /// </summary>
    public async Task<BudgetOdeme> OdemeYapAsync(int odemeId, OdemeYapRequest request)
    {
        var odeme = await _context.BudgetOdemeler.FindAsync(odemeId);
        if (odeme == null)
            throw new Exception("Odeme bulunamadi");

        var odemeTutari = request.KismiOdemeTutari ?? odeme.Miktar;
        var odemeTarihi = DateTime.SpecifyKind(request.OdemeTarihi, DateTimeKind.Utc);

        // Odeme durumunu guncelle
        odeme.Durum = OdemeDurum.Odendi;
        odeme.GercekOdemeTarihi = odemeTarihi;
        odeme.OdenenTutar = odemeTutari;
        odeme.OdemeYapildigiHesapId = request.BankaHesapId;
        odeme.OdemeNotu = request.Aciklama;
        odeme.UpdatedAt = DateTime.UtcNow;

        // Kasa/Banka hareketi olustur (Mahsup disinda)
        // KASA = BORC (Cikis hareket), ODEME = ALACAK
        if (request.OdemeTipi != OdemeTipi.Mahsup && request.BankaHesapId.HasValue)
        {
            var hareket = new BankaKasaHareket
            {
                IslemNo = $"BORC-{odeme.Id}-{DateTime.Now:yyyyMMddHHmmss}",
                IslemTarihi = odemeTarihi,
                HareketTipi = HareketTipi.Cikis, // Kasa = Borc (para cikiyor)
                BankaHesapId = request.BankaHesapId.Value,
                Tutar = odemeTutari,
                Aciklama = $"Butce Odemesi (Borc): {odeme.MasrafKalemi}" + 
                          (string.IsNullOrEmpty(request.Aciklama) ? "" : $" - {request.Aciklama}"),
                IslemKaynak = IslemKaynak.Manuel,
                CreatedAt = DateTime.UtcNow
            };

            _context.BankaKasaHareketleri.Add(hareket);
            await _context.SaveChangesAsync();
            
            // Hareket ID'sini kaydet
            odeme.BankaKasaHareketId = hareket.Id;
        }

        await _context.SaveChangesAsync();
        return odeme;
    }

    /// <summary>
    /// Fatura ile kapatma - fatura girildiginde hesaplari kapatir
    /// </summary>
    public async Task<BudgetOdeme> FaturaIleKapatAsync(int odemeId, int faturaId)
    {
        var odeme = await _context.BudgetOdemeler.FindAsync(odemeId);
        if (odeme == null)
            throw new Exception("Odeme bulunamadi");

        var fatura = await _context.Faturalar.FindAsync(faturaId);
        if (fatura == null)
            throw new Exception("Fatura bulunamadi");

        // Odeme fatura ile kapatildi
        odeme.FaturaId = faturaId;
        odeme.FaturaIleKapatildi = true;
        odeme.Durum = OdemeDurum.Odendi;
        odeme.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return odeme;
    }

    #endregion

    #region Taksitli Odeme Islemleri

    public async Task<List<BudgetOdeme>> CreateTaksitliOdemeAsync(TaksitliOdemeRequest request)
    {
        var taksitGrupId = Guid.NewGuid();
        var taksitTutari = Math.Round(request.ToplamTutar / request.TaksitSayisi, 2);
        var taksitler = new List<BudgetOdeme>();

        // Yuvarlama farkýný son taksitte düzelt
        var toplamHesaplanan = taksitTutari * request.TaksitSayisi;
        var fark = request.ToplamTutar - toplamHesaplanan;

        // Baslangic tarihini UTC olarak ayarla
        var baslangicUtc = DateTime.SpecifyKind(request.BaslangicTarihi, DateTimeKind.Utc);

        for (int i = 0; i < request.TaksitSayisi; i++)
        {
            var taksitTarihi = baslangicUtc.AddMonths(i);
            var tutar = i == request.TaksitSayisi - 1 ? taksitTutari + fark : taksitTutari;

            var odeme = new BudgetOdeme
            {
                OdemeTarihi = taksitTarihi,
                OdemeAy = taksitTarihi.Month,
                OdemeYil = taksitTarihi.Year,
                MasrafKalemi = request.MasrafKalemi,
                Aciklama = request.Aciklama,
                Miktar = tutar,
                TaksitliMi = true,
                ToplamTaksitSayisi = request.TaksitSayisi,
                KacinciTaksit = i + 1,
                TaksitGrupId = taksitGrupId,
                TaksitBaslangicAy = baslangicUtc,
                TaksitBitisAy = baslangicUtc.AddMonths(request.TaksitSayisi - 1),
                Notlar = request.Notlar,
                Durum = OdemeDurum.Bekliyor,
                CreatedAt = DateTime.UtcNow
            };

            taksitler.Add(odeme);
        }

        _context.BudgetOdemeler.AddRange(taksitler);
        await _context.SaveChangesAsync();
        return taksitler;
    }

    public async Task<List<BudgetOdeme>> GetTaksitGrubuAsync(Guid taksitGrupId)
    {
        return await _context.BudgetOdemeler
            .Where(o => o.TaksitGrupId == taksitGrupId)
            .OrderBy(o => o.KacinciTaksit)
            .ToListAsync();
    }

    public async Task UpdateTaksitGrubuAsync(List<BudgetOdeme> taksitler)
    {
        foreach (var taksit in taksitler)
        {
            var existing = await _context.BudgetOdemeler.FindAsync(taksit.Id);
            if (existing != null)
            {
                // DateTime'i UTC olarak ayarla
                existing.OdemeTarihi = DateTime.SpecifyKind(taksit.OdemeTarihi, DateTimeKind.Utc);
                existing.OdemeAy = taksit.OdemeTarihi.Month;
                existing.OdemeYil = taksit.OdemeTarihi.Year;
                existing.Miktar = taksit.Miktar;
                existing.Durum = taksit.Durum;
                existing.UpdatedAt = DateTime.UtcNow;
            }
        }
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Toplu Islemler (Excel)

    public async Task<List<BudgetOdeme>> CreateBulkOdemeAsync(List<BudgetOdeme> odemeler)
    {
        foreach (var odeme in odemeler)
        {
            // DateTime'i UTC olarak ayarla
            odeme.OdemeTarihi = DateTime.SpecifyKind(odeme.OdemeTarihi, DateTimeKind.Utc);
            odeme.OdemeAy = odeme.OdemeTarihi.Month;
            odeme.OdemeYil = odeme.OdemeTarihi.Year;
            odeme.TaksitliMi = false;
            odeme.ToplamTaksitSayisi = 1;
            odeme.KacinciTaksit = 1;
            odeme.CreatedAt = DateTime.UtcNow;
            
            _context.BudgetOdemeler.Add(odeme);
        }
        
        await _context.SaveChangesAsync();
        return odemeler;
    }

    public byte[] GenerateExcelTemplate()
    {
        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Odeme Sablonu");

        // Basliklar
        var headers = new[] { "Odeme Tarihi*", "Masraf Kalemi*", "Aciklama", "Miktar*", "Durum", "Notlar" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;
        }

        // Ornek satirlar
        worksheet.Cell(2, 1).Value = DateTime.Today.ToString("dd.MM.yyyy");
        worksheet.Cell(2, 2).Value = "Kira";
        worksheet.Cell(2, 3).Value = "Ocak ayi kirasi";
        worksheet.Cell(2, 4).Value = 5000;
        worksheet.Cell(2, 5).Value = "Bekliyor";
        worksheet.Cell(2, 6).Value = "";

        worksheet.Cell(3, 1).Value = DateTime.Today.AddDays(5).ToString("dd.MM.yyyy");
        worksheet.Cell(3, 2).Value = "Elektrik";
        worksheet.Cell(3, 3).Value = "Ocak faturasi";
        worksheet.Cell(3, 4).Value = 850;
        worksheet.Cell(3, 5).Value = "Bekliyor";
        worksheet.Cell(3, 6).Value = "";

        // Aciklama satirlari
        worksheet.Cell(5, 1).Value = "ACIKLAMALAR:";
        worksheet.Cell(5, 1).Style.Font.Bold = true;
        worksheet.Cell(6, 1).Value = "* Odeme Tarihi: GG.AA.YYYY formatinda";
        worksheet.Cell(7, 1).Value = "* Masraf Kalemi: Kira, Elektrik, Su, Dogalgaz, Personel Maas vb.";
        worksheet.Cell(8, 1).Value = "* Miktar: Sayisal deger (virgul veya nokta kullanabilirsiniz)";
        worksheet.Cell(9, 1).Value = "* Durum: Bekliyor, Odendi, Ertelendi, Iptal";

        // Masraf kalemleri listesi
        worksheet.Cell(11, 1).Value = "MASRAF KALEMLERI:";
        worksheet.Cell(11, 1).Style.Font.Bold = true;
        
        var masrafKalemleri = _context.BudgetMasrafKalemleri.Where(m => m.Aktif).OrderBy(m => m.SiraNo).ToList();
        int row = 12;
        foreach (var kalem in masrafKalemleri)
        {
            worksheet.Cell(row, 1).Value = kalem.KalemAdi;
            worksheet.Cell(row, 2).Value = kalem.Kategori;
            row++;
        }

        // Sutun genislikleri
        worksheet.Column(1).Width = 15;
        worksheet.Column(2).Width = 20;
        worksheet.Column(3).Width = 30;
        worksheet.Column(4).Width = 15;
        worksheet.Column(5).Width = 12;
        worksheet.Column(6).Width = 30;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<ExcelImportResult> ImportFromExcelAsync(byte[] fileContent)
    {
        var result = new ExcelImportResult();
        var odemeler = new List<BudgetOdeme>();

        try
        {
            using var stream = new MemoryStream(fileContent);
            using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);

            var rows = worksheet.RowsUsed().Skip(1); // Baþlýk satýrýný atla

            int rowNum = 1;
            foreach (var row in rows)
            {
                rowNum++;
                
                // Bos satir veya aciklama satirlarini atla
                var tarihStr = row.Cell(1).GetString().Trim();
                if (string.IsNullOrEmpty(tarihStr) || tarihStr.StartsWith("ACIKLAMA") || tarihStr.StartsWith("MASRAF") || tarihStr.StartsWith("*"))
                    continue;

                try
                {
                    // Tarih parse - Excel'den sayi olarak da gelebilir
                    DateTime tarih;
                    if (row.Cell(1).DataType == ClosedXML.Excel.XLDataType.DateTime)
                    {
                        tarih = row.Cell(1).GetDateTime();
                    }
                    else if (row.Cell(1).DataType == ClosedXML.Excel.XLDataType.Number)
                    {
                        tarih = DateTime.FromOADate(row.Cell(1).GetDouble());
                    }
                    else if (!DateTime.TryParse(tarihStr, new System.Globalization.CultureInfo("tr-TR"), out tarih))
                    {
                        // GG.AA.YYYY formatini dene
                        if (!DateTime.TryParseExact(tarihStr, new[] { "dd.MM.yyyy", "d.M.yyyy", "dd/MM/yyyy", "yyyy-MM-dd" }, 
                            new System.Globalization.CultureInfo("tr-TR"), System.Globalization.DateTimeStyles.None, out tarih))
                        {
                            result.Errors.Add($"Satir {rowNum}: Gecersiz tarih formati - '{tarihStr}'");
                            result.ErrorCount++;
                            continue;
                        }
                    }

                    var masrafKalemi = row.Cell(2).GetString().Trim();
                    if (string.IsNullOrEmpty(masrafKalemi))
                    {
                        result.Errors.Add($"Satir {rowNum}: Masraf kalemi bos olamaz");
                        result.ErrorCount++;
                        continue;
                    }

                    var aciklama = row.Cell(3).GetString().Trim();
                    
                    // Miktar parse - Excel'den sayi olarak da gelebilir
                    decimal miktar;
                    if (row.Cell(4).DataType == ClosedXML.Excel.XLDataType.Number)
                    {
                        miktar = (decimal)row.Cell(4).GetDouble();
                    }
                    else
                    {
                        var miktarStr = row.Cell(4).GetString().Trim()
                            .Replace(".", "")  // Binlik ayirici noktayi kaldir
                            .Replace(",", "."); // Virgulu noktaya cevir
                        
                        if (!decimal.TryParse(miktarStr, System.Globalization.NumberStyles.Any, 
                            System.Globalization.CultureInfo.InvariantCulture, out miktar))
                        {
                            result.Errors.Add($"Satir {rowNum}: Gecersiz miktar - '{row.Cell(4).GetString()}'");
                            result.ErrorCount++;
                            continue;
                        }
                    }
                    
                    if (miktar <= 0)
                    {
                        result.Errors.Add($"Satir {rowNum}: Miktar sifirdan buyuk olmali");
                        result.ErrorCount++;
                        continue;
                    }

                    // Durum parse
                    var durumStr = row.Cell(5).GetString().Trim().ToLower();
                    var durum = durumStr switch
                    {
                        "odendi" or " ödendi" => OdemeDurum.Odendi,
                        "ertelendi" => OdemeDurum.Ertelendi,
                        "iptal" => OdemeDurum.Iptal,
                        _ => OdemeDurum.Bekliyor
                    };

                    var notlar = row.Cell(6).GetString().Trim();

                    // DateTime'i UTC olarak ayarla
                    var tarihUtc = DateTime.SpecifyKind(tarih, DateTimeKind.Utc);

                    var odeme = new BudgetOdeme
                    {
                        OdemeTarihi = tarihUtc,
                        OdemeAy = tarih.Month,
                        OdemeYil = tarih.Year,
                        MasrafKalemi = masrafKalemi,
                        Aciklama = string.IsNullOrEmpty(aciklama) ? null : aciklama,
                        Miktar = miktar,
                        Durum = durum,
                        Notlar = string.IsNullOrEmpty(notlar) ? null : notlar,
                        TaksitliMi = false,
                        ToplamTaksitSayisi = 1,
                        KacinciTaksit = 1,
                        CreatedAt = DateTime.UtcNow
                    };

                    odemeler.Add(odeme);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Satir {rowNum}: {ex.Message}");
                    result.ErrorCount++;
                }
            }

            if (odemeler.Any())
            {
                try
                {
                    // Toplu ekleme
                    foreach (var odeme in odemeler)
                    {
                        _context.BudgetOdemeler.Add(odeme);
                    }
                    await _context.SaveChangesAsync();
                    result.ImportedCount = odemeler.Count;
                    result.ImportedItems = odemeler;
                }
                catch (Exception dbEx)
                {
                    var innerMsg = dbEx.InnerException?.Message ?? dbEx.Message;
                    result.Errors.Add($"Veritabani kayit hatasi: {innerMsg}");
                    result.Success = false;
                    return result;
                }
            }

            result.Success = result.ErrorCount == 0 || result.ImportedCount > 0;
        }
        catch (Exception ex)
        {
            result.Success = false;
            var innerMsg = ex.InnerException?.Message ?? ex.Message;
            result.Errors.Add($"Excel dosyasi okunamadi: {innerMsg}");
        }

        return result;
    }

    #endregion

    #region Masraf Kalemleri

    public async Task<List<BudgetMasrafKalemi>> GetMasrafKalemleriAsync()
    {
        return await _context.BudgetMasrafKalemleri
            .Where(m => m.Aktif)
            .OrderBy(m => m.SiraNo)
            .ThenBy(m => m.KalemAdi)
            .ToListAsync();
    }

    public async Task<BudgetMasrafKalemi> CreateMasrafKalemiAsync(BudgetMasrafKalemi kalem)
    {
        _context.BudgetMasrafKalemleri.Add(kalem);
        await _context.SaveChangesAsync();
        return kalem;
    }

    public async Task<BudgetMasrafKalemi> UpdateMasrafKalemiAsync(BudgetMasrafKalemi kalem)
    {
        _context.BudgetMasrafKalemleri.Update(kalem);
        await _context.SaveChangesAsync();
        return kalem;
    }

    public async Task DeleteMasrafKalemiAsync(int id)
    {
        var kalem = await _context.BudgetMasrafKalemleri.FindAsync(id);
        if (kalem != null)
        {
            kalem.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Raporlar

    public async Task<BudgetOzet> GetAylikOzetAsync(int yil, int ay)
    {
        var odemeler = await _context.BudgetOdemeler
            .Where(o => o.OdemeYil == yil && o.OdemeAy == ay)
            .ToListAsync();

        var ozet = new BudgetOzet
        {
            Yil = yil,
            Ay = ay,
            ToplamOdeme = odemeler.Sum(o => o.Miktar),
            OdenenToplam = odemeler.Where(o => o.Durum == OdemeDurum.Odendi).Sum(o => o.Miktar),
            BekleyenToplam = odemeler.Where(o => o.Durum == OdemeDurum.Bekliyor).Sum(o => o.Miktar),
            ToplamKayit = odemeler.Count,
            OdenenKayit = odemeler.Count(o => o.Durum == OdemeDurum.Odendi),
            BekleyenKayit = odemeler.Count(o => o.Durum == OdemeDurum.Bekliyor)
        };

        ozet.KategoriOzetleri = odemeler
            .GroupBy(o => o.MasrafKalemi)
            .Select(g => new BudgetKategoriOzet
            {
                MasrafKalemi = g.Key,
                Toplam = g.Sum(o => o.Miktar),
                Adet = g.Count(),
                Yuzde = ozet.ToplamOdeme > 0 ? Math.Round(g.Sum(o => o.Miktar) / ozet.ToplamOdeme * 100, 1) : 0
            })
            .OrderByDescending(k => k.Toplam)
            .ToList();

        return ozet;
    }

    public async Task<BudgetYillikOzet> GetYillikOzetAsync(int yil)
    {
        var odemeler = await _context.BudgetOdemeler
            .Where(o => o.OdemeYil == yil)
            .ToListAsync();

        var ozet = new BudgetYillikOzet
        {
            Yil = yil,
            ToplamOdeme = odemeler.Sum(o => o.Miktar)
        };

        // Aylýk toplamlar
        for (int ay = 1; ay <= 12; ay++)
        {
            var aylikOdemeler = odemeler.Where(o => o.OdemeAy == ay).ToList();
            ozet.AylikToplamlar.Add(new BudgetAylikToplam
            {
                Ay = ay,
                AyAdi = AyAdlari[ay],
                Toplam = aylikOdemeler.Sum(o => o.Miktar),
                Odenen = aylikOdemeler.Where(o => o.Durum == OdemeDurum.Odendi).Sum(o => o.Miktar),
                Bekleyen = aylikOdemeler.Where(o => o.Durum == OdemeDurum.Bekliyor).Sum(o => o.Miktar)
            });
        }

        // Kategori özetleri
        ozet.KategoriOzetleri = odemeler
            .GroupBy(o => o.MasrafKalemi)
            .Select(g => new BudgetKategoriOzet
            {
                MasrafKalemi = g.Key,
                Toplam = g.Sum(o => o.Miktar),
                Adet = g.Count(),
                Yuzde = ozet.ToplamOdeme > 0 ? Math.Round(g.Sum(o => o.Miktar) / ozet.ToplamOdeme * 100, 1) : 0
            })
            .OrderByDescending(k => k.Toplam)
            .ToList();

        return ozet;
    }

    public async Task<List<BudgetGunlukOzet>> GetTakvimDataAsync(int yil, int ay)
    {
        // SADECE BEKLEYEN odemeleri getir (Odenmis olanlar takvimde gozukmez)
        var odemeler = await _context.BudgetOdemeler
            .Where(o => o.OdemeYil == yil && o.OdemeAy == ay && o.Durum == OdemeDurum.Bekliyor)
            .OrderBy(o => o.OdemeTarihi)
            .ToListAsync();

        var gunlukOzetler = new List<BudgetGunlukOzet>();
        var gunSayisi = DateTime.DaysInMonth(yil, ay);

        for (int gun = 1; gun <= gunSayisi; gun++)
        {
            var tarih = new DateTime(yil, ay, gun);
            var gunOdemeleri = odemeler.Where(o => o.OdemeTarihi.Day == gun).ToList();

            gunlukOzetler.Add(new BudgetGunlukOzet
            {
                Tarih = tarih,
                Gun = gun,
                ToplamOdeme = gunOdemeleri.Sum(o => o.Miktar),
                OdemeSayisi = gunOdemeleri.Count,
                Odemeler = gunOdemeleri
            });
        }

        return gunlukOzetler;
    }

    public async Task<List<BudgetKategoriOzet>> GetKategoriOzetAsync(int yil, int? ay = null)
    {
        var query = _context.BudgetOdemeler.Where(o => o.OdemeYil == yil);

        if (ay.HasValue)
            query = query.Where(o => o.OdemeAy == ay.Value);

        var odemeler = await query.ToListAsync();
        var toplam = odemeler.Sum(o => o.Miktar);

        // Masraf kalemlerinin renklerini al
        var masrafKalemleri = await _context.BudgetMasrafKalemleri
            .ToDictionaryAsync(m => m.KalemAdi, m => m.Renk);

        return odemeler
            .GroupBy(o => o.MasrafKalemi)
            .Select(g => new BudgetKategoriOzet
            {
                MasrafKalemi = g.Key,
                Renk = masrafKalemleri.TryGetValue(g.Key, out var renk) ? renk : "#6c757d",
                Toplam = g.Sum(o => o.Miktar),
                Adet = g.Count(),
                Yuzde = toplam > 0 ? Math.Round(g.Sum(o => o.Miktar) / toplam * 100, 1) : 0
            })
            .OrderByDescending(k => k.Toplam)
            .ToList();
    }

    #endregion

    #region Kredi/Taksit Raporlari

    public async Task<List<KrediOzet>> GetAktifKredilerAsync()
    {
        var taksitliOdemeler = await _context.BudgetOdemeler
            .Where(o => o.TaksitliMi && o.TaksitGrupId.HasValue)
            .ToListAsync();

        var krediler = taksitliOdemeler
            .GroupBy(o => o.TaksitGrupId!.Value)
            .Select(g =>
            {
                var taksitler = g.OrderBy(t => t.KacinciTaksit).ToList();
                var ilkTaksit = taksitler.First();
                var sonTaksit = taksitler.Last();
                var odenenTaksitler = taksitler.Where(t => t.Durum == OdemeDurum.Odendi).ToList();
                var bekleyenTaksitler = taksitler.Where(t => t.Durum == OdemeDurum.Bekliyor).ToList();
                var sonrakiTaksit = bekleyenTaksitler.OrderBy(t => t.OdemeTarihi).FirstOrDefault();

                return new KrediOzet
                {
                    TaksitGrupId = g.Key,
                    MasrafKalemi = ilkTaksit.MasrafKalemi,
                    Aciklama = ilkTaksit.Aciklama,
                    BaslangicTarihi = ilkTaksit.OdemeTarihi,
                    BitisTarihi = sonTaksit.OdemeTarihi,
                    ToplamTaksitSayisi = taksitler.Count,
                    OdenenTaksitSayisi = odenenTaksitler.Count,
                    KalanTaksitSayisi = bekleyenTaksitler.Count,
                    TaksitTutari = taksitler.First().Miktar,
                    ToplamTutar = taksitler.Sum(t => t.Miktar),
                    OdenenTutar = odenenTaksitler.Sum(t => t.Miktar),
                    KalanTutar = bekleyenTaksitler.Sum(t => t.Miktar),
                    TamamlanmaYuzdesi = taksitler.Count > 0 
                        ? Math.Round((decimal)odenenTaksitler.Count / taksitler.Count * 100, 1) 
                        : 0,
                    SonrakiTaksitTarihi = sonrakiTaksit?.OdemeTarihi
                };
            })
            .Where(k => k.KalanTaksitSayisi > 0) // Sadece aktif (kalan taksiti olan) krediler
            .OrderBy(k => k.SonrakiTaksitTarihi)
            .ToList();

        return krediler;
    }

    public async Task<List<AylikKrediTaksitRapor>> GetAylikKrediTaksitRaporuAsync(int yil)
    {
        var taksitliOdemeler = await _context.BudgetOdemeler
            .Where(o => o.TaksitliMi && o.OdemeYil == yil)
            .OrderBy(o => o.OdemeAy)
            .ThenBy(o => o.OdemeTarihi)
            .ToListAsync();

        var rapor = new List<AylikKrediTaksitRapor>();

        for (int ay = 1; ay <= 12; ay++)
        {
            var aylikTaksitler = taksitliOdemeler.Where(o => o.OdemeAy == ay).ToList();

            rapor.Add(new AylikKrediTaksitRapor
            {
                Ay = ay,
                AyAdi = AyAdlari[ay],
                ToplamTaksitTutari = aylikTaksitler.Sum(t => t.Miktar),
                OdenenTutar = aylikTaksitler.Where(t => t.Durum == OdemeDurum.Odendi).Sum(t => t.Miktar),
                BekleyenTutar = aylikTaksitler.Where(t => t.Durum == OdemeDurum.Bekliyor).Sum(t => t.Miktar),
                TaksitSayisi = aylikTaksitler.Count,
                Taksitler = aylikTaksitler.Select(t => new KrediTaksitDetay
                {
                    MasrafKalemi = t.MasrafKalemi,
                    Aciklama = t.Aciklama,
                    KacinciTaksit = t.KacinciTaksit,
                    ToplamTaksitSayisi = t.ToplamTaksitSayisi,
                    Tutar = t.Miktar,
                    Durum = t.Durum,
                    OdemeTarihi = t.OdemeTarihi
                }).ToList()
            });
        }

        return rapor;
    }

    #endregion

    #region Periyod Bazli Raporlar

    public async Task<BudgetOzet> GetPeriyodOzetAsync(DateTime baslangic, DateTime bitis)
    {
        var odemeler = await GetOdemelerByDateRangeAsync(baslangic, bitis);

        var ozet = new BudgetOzet
        {
            BaslangicTarihi = baslangic,
            BitisTarihi = bitis,
            ToplamOdeme = odemeler.Sum(o => o.Miktar),
            OdenenToplam = odemeler.Where(o => o.Durum == OdemeDurum.Odendi).Sum(o => o.Miktar),
            BekleyenToplam = odemeler.Where(o => o.Durum == OdemeDurum.Bekliyor).Sum(o => o.Miktar),
            ToplamKayit = odemeler.Count,
            OdenenKayit = odemeler.Count(o => o.Durum == OdemeDurum.Odendi),
            BekleyenKayit = odemeler.Count(o => o.Durum == OdemeDurum.Bekliyor)
        };

        ozet.KategoriOzetleri = odemeler
            .GroupBy(o => o.MasrafKalemi)
            .Select(g => new BudgetKategoriOzet
            {
                MasrafKalemi = g.Key,
                Toplam = g.Sum(o => o.Miktar),
                Adet = g.Count(),
                Yuzde = ozet.ToplamOdeme > 0 ? Math.Round(g.Sum(o => o.Miktar) / ozet.ToplamOdeme * 100, 1) : 0
            })
            .OrderByDescending(k => k.Toplam)
            .ToList();

        return ozet;
    }

    public async Task<List<BudgetKategoriOzet>> GetKategoriOzetByDateRangeAsync(DateTime baslangic, DateTime bitis)
    {
        var odemeler = await GetOdemelerByDateRangeAsync(baslangic, bitis);
        var toplam = odemeler.Sum(o => o.Miktar);

        var masrafKalemleri = await _context.BudgetMasrafKalemleri
            .ToDictionaryAsync(m => m.KalemAdi, m => m.Renk);

        return odemeler
            .GroupBy(o => o.MasrafKalemi)
            .Select(g => new BudgetKategoriOzet
            {
                MasrafKalemi = g.Key,
                Renk = masrafKalemleri.TryGetValue(g.Key, out var renk) ? renk : "#6c757d",
                Toplam = g.Sum(o => o.Miktar),
                Adet = g.Count(),
                Yuzde = toplam > 0 ? Math.Round(g.Sum(o => o.Miktar) / toplam * 100, 1) : 0
            })
            .OrderByDescending(k => k.Toplam)
            .ToList();
    }

    public async Task<List<BudgetTrendData>> GetTrendDataAsync(DateTime baslangic, DateTime bitis, string periyod)
    {
        var odemeler = await GetOdemelerByDateRangeAsync(baslangic, bitis);
        var trendData = new List<BudgetTrendData>();

        switch (periyod.ToLower())
        {
            case "gunluk":
                var gunler = odemeler.GroupBy(o => o.OdemeTarihi.Date);
                foreach (var gun in gunler.OrderBy(g => g.Key))
                {
                    trendData.Add(new BudgetTrendData
                    {
                        Tarih = gun.Key,
                        Etiket = gun.Key.ToString("dd.MM"),
                        Toplam = gun.Sum(o => o.Miktar),
                        Odenen = gun.Where(o => o.Durum == OdemeDurum.Odendi).Sum(o => o.Miktar),
                        Bekleyen = gun.Where(o => o.Durum == OdemeDurum.Bekliyor).Sum(o => o.Miktar),
                        OdemeSayisi = gun.Count()
                    });
                }
                break;

            case "haftalik":
                var haftalar = odemeler.GroupBy(o => GetHaftaBaslangic(o.OdemeTarihi));
                foreach (var hafta in haftalar.OrderBy(h => h.Key))
                {
                    trendData.Add(new BudgetTrendData
                    {
                        Tarih = hafta.Key,
                        Etiket = $"{hafta.Key:dd.MM} - {hafta.Key.AddDays(6):dd.MM}",
                        Toplam = hafta.Sum(o => o.Miktar),
                        Odenen = hafta.Where(o => o.Durum == OdemeDurum.Odendi).Sum(o => o.Miktar),
                        Bekleyen = hafta.Where(o => o.Durum == OdemeDurum.Bekliyor).Sum(o => o.Miktar),
                        OdemeSayisi = hafta.Count()
                    });
                }
                break;

            case "15gunluk":
                var onbesGunler = odemeler.GroupBy(o => Get15GunBaslangic(o.OdemeTarihi));
                foreach (var donem in onbesGunler.OrderBy(d => d.Key))
                {
                    var bitisGun = donem.Key.Day <= 15 ? 15 : DateTime.DaysInMonth(donem.Key.Year, donem.Key.Month);
                    trendData.Add(new BudgetTrendData
                    {
                        Tarih = donem.Key,
                        Etiket = $"{donem.Key:dd.MM} - {bitisGun}.{donem.Key:MM}",
                        Toplam = donem.Sum(o => o.Miktar),
                        Odenen = donem.Where(o => o.Durum == OdemeDurum.Odendi).Sum(o => o.Miktar),
                        Bekleyen = donem.Where(o => o.Durum == OdemeDurum.Bekliyor).Sum(o => o.Miktar),
                        OdemeSayisi = donem.Count()
                    });
                }
                break;

            case "aylik":
            default:
                var aylar = odemeler.GroupBy(o => new DateTime(o.OdemeTarihi.Year, o.OdemeTarihi.Month, 1));
                foreach (var ay in aylar.OrderBy(a => a.Key))
                {
                    trendData.Add(new BudgetTrendData
                    {
                        Tarih = ay.Key,
                        Etiket = AyAdlari[ay.Key.Month].Substring(0, 3) + " " + ay.Key.Year,
                        Toplam = ay.Sum(o => o.Miktar),
                        Odenen = ay.Where(o => o.Durum == OdemeDurum.Odendi).Sum(o => o.Miktar),
                        Bekleyen = ay.Where(o => o.Durum == OdemeDurum.Bekliyor).Sum(o => o.Miktar),
                        OdemeSayisi = ay.Count()
                    });
                }
                break;
        }

        return trendData;
    }

    private DateTime GetHaftaBaslangic(DateTime tarih)
    {
        var gun = (int)tarih.DayOfWeek;
        var pazartesiOffset = gun == 0 ? -6 : 1 - gun;
        return tarih.Date.AddDays(pazartesiOffset);
    }

    private DateTime Get15GunBaslangic(DateTime tarih)
    {
        return tarih.Day <= 15 
            ? new DateTime(tarih.Year, tarih.Month, 1) 
            : new DateTime(tarih.Year, tarih.Month, 16);
    }

    #endregion
}
