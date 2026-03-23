using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class BudgetService : IBudgetService
{
    private readonly ApplicationDbContext _context;
    private static readonly string[] AyAdlari = { "", "Ocak", "Þubat", "Mart", "Nisan", "Mayýs", "Haziran", 
                                                   "Temmuz", "Aðustos", "Eylül", "Ekim", "Kasým", "Aralýk" };

    public BudgetService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Ödeme Ýþlemleri

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

    public async Task<BudgetOdeme?> GetOdemeByIdAsync(int id)
    {
        return await _context.BudgetOdemeler.FindAsync(id);
    }

    public async Task<BudgetOdeme> CreateOdemeAsync(BudgetOdeme odeme)
    {
        odeme.OdemeAy = odeme.OdemeTarihi.Month;
        odeme.OdemeYil = odeme.OdemeTarihi.Year;

        _context.BudgetOdemeler.Add(odeme);
        await _context.SaveChangesAsync();
        return odeme;
    }

    public async Task<BudgetOdeme> UpdateOdemeAsync(BudgetOdeme odeme)
    {
        odeme.OdemeAy = odeme.OdemeTarihi.Month;
        odeme.OdemeYil = odeme.OdemeTarihi.Year;

        _context.BudgetOdemeler.Update(odeme);
        await _context.SaveChangesAsync();
        return odeme;
    }

    public async Task DeleteOdemeAsync(int id)
    {
        var odeme = await _context.BudgetOdemeler.FindAsync(id);
        if (odeme != null)
        {
            odeme.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Taksitli Ödeme Ýþlemleri

    public async Task<List<BudgetOdeme>> CreateTaksitliOdemeAsync(TaksitliOdemeRequest request)
    {
        var taksitGrupId = Guid.NewGuid();
        var taksitTutari = Math.Round(request.ToplamTutar / request.TaksitSayisi, 2);
        var taksitler = new List<BudgetOdeme>();

        // Yuvarlama farkýný son taksitte düzelt
        var toplamHesaplanan = taksitTutari * request.TaksitSayisi;
        var fark = request.ToplamTutar - toplamHesaplanan;

        for (int i = 0; i < request.TaksitSayisi; i++)
        {
            var taksitTarihi = request.BaslangicTarihi.AddMonths(i);
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
                TaksitBaslangicAy = request.BaslangicTarihi,
                TaksitBitisAy = request.BaslangicTarihi.AddMonths(request.TaksitSayisi - 1),
                Notlar = request.Notlar,
                Durum = OdemeDurum.Bekliyor
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
            taksit.OdemeAy = taksit.OdemeTarihi.Month;
            taksit.OdemeYil = taksit.OdemeTarihi.Year;
            _context.BudgetOdemeler.Update(taksit);
        }
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Toplu Ýþlemler (Excel)

    public async Task<List<BudgetOdeme>> CreateBulkOdemeAsync(List<BudgetOdeme> odemeler)
    {
        foreach (var odeme in odemeler)
        {
            odeme.OdemeAy = odeme.OdemeTarihi.Month;
            odeme.OdemeYil = odeme.OdemeTarihi.Year;
        }
        
        _context.BudgetOdemeler.AddRange(odemeler);
        await _context.SaveChangesAsync();
        return odemeler;
    }

    public byte[] GenerateExcelTemplate()
    {
        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Ödeme Þablonu");

        // Baþlýklar
        var headers = new[] { "Ödeme Tarihi*", "Masraf Kalemi*", "Açýklama", "Miktar*", "Durum", "Notlar" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;
        }

        // Örnek satýrlar
        worksheet.Cell(2, 1).Value = DateTime.Today.ToString("dd.MM.yyyy");
        worksheet.Cell(2, 2).Value = "Kira";
        worksheet.Cell(2, 3).Value = "Ocak ayý kirasý";
        worksheet.Cell(2, 4).Value = 5000;
        worksheet.Cell(2, 5).Value = "Bekliyor";
        worksheet.Cell(2, 6).Value = "";

        worksheet.Cell(3, 1).Value = DateTime.Today.AddDays(5).ToString("dd.MM.yyyy");
        worksheet.Cell(3, 2).Value = "Elektrik";
        worksheet.Cell(3, 3).Value = "Ocak faturasý";
        worksheet.Cell(3, 4).Value = 850;
        worksheet.Cell(3, 5).Value = "Bekliyor";
        worksheet.Cell(3, 6).Value = "";

        // Açýklama satýrlarý
        worksheet.Cell(5, 1).Value = "AÇIKLAMALAR:";
        worksheet.Cell(5, 1).Style.Font.Bold = true;
        worksheet.Cell(6, 1).Value = "* Ödeme Tarihi: GG.AA.YYYY formatýnda";
        worksheet.Cell(7, 1).Value = "* Masraf Kalemi: Kira, Elektrik, Su, Doðalgaz, Personel Maaþ vb.";
        worksheet.Cell(8, 1).Value = "* Miktar: Sayýsal deðer (virgül veya nokta kullanabilirsiniz)";
        worksheet.Cell(9, 1).Value = "* Durum: Bekliyor, Ödendi, Ertelendi, Ýptal";

        // Masraf kalemleri listesi
        worksheet.Cell(11, 1).Value = "MASRAF KALEMLERÝ:";
        worksheet.Cell(11, 1).Style.Font.Bold = true;
        
        var masrafKalemleri = _context.BudgetMasrafKalemleri.Where(m => m.Aktif).OrderBy(m => m.SiraNo).ToList();
        int row = 12;
        foreach (var kalem in masrafKalemleri)
        {
            worksheet.Cell(row, 1).Value = kalem.KalemAdi;
            worksheet.Cell(row, 2).Value = kalem.Kategori;
            row++;
        }

        // Sütun geniþlikleri
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
                
                // Boþ satýr veya açýklama satýrlarýný atla
                var tarihStr = row.Cell(1).GetString().Trim();
                if (string.IsNullOrEmpty(tarihStr) || tarihStr.StartsWith("AÇIKLAMA") || tarihStr.StartsWith("MASRAF") || tarihStr.StartsWith("*"))
                    continue;

                try
                {
                    // Tarih parse
                    if (!DateTime.TryParse(tarihStr, out var tarih))
                    {
                        // GG.AA.YYYY formatýný dene
                        if (!DateTime.TryParseExact(tarihStr, new[] { "dd.MM.yyyy", "d.M.yyyy", "dd/MM/yyyy", "yyyy-MM-dd" }, 
                            null, System.Globalization.DateTimeStyles.None, out tarih))
                        {
                            result.Errors.Add($"Satýr {rowNum}: Geçersiz tarih formatý - '{tarihStr}'");
                            result.ErrorCount++;
                            continue;
                        }
                    }

                    var masrafKalemi = row.Cell(2).GetString().Trim();
                    if (string.IsNullOrEmpty(masrafKalemi))
                    {
                        result.Errors.Add($"Satýr {rowNum}: Masraf kalemi boþ olamaz");
                        result.ErrorCount++;
                        continue;
                    }

                    var aciklama = row.Cell(3).GetString().Trim();
                    
                    // Miktar parse
                    var miktarStr = row.Cell(4).GetString().Trim().Replace(",", ".");
                    if (!decimal.TryParse(miktarStr, System.Globalization.NumberStyles.Any, 
                        System.Globalization.CultureInfo.InvariantCulture, out var miktar) || miktar <= 0)
                    {
                        result.Errors.Add($"Satýr {rowNum}: Geçersiz miktar - '{miktarStr}'");
                        result.ErrorCount++;
                        continue;
                    }

                    // Durum parse
                    var durumStr = row.Cell(5).GetString().Trim().ToLower();
                    var durum = durumStr switch
                    {
                        "ödendi" or "odendi" => OdemeDurum.Odendi,
                        "ertelendi" => OdemeDurum.Ertelendi,
                        "iptal" => OdemeDurum.Iptal,
                        _ => OdemeDurum.Bekliyor
                    };

                    var notlar = row.Cell(6).GetString().Trim();

                    var odeme = new BudgetOdeme
                    {
                        OdemeTarihi = tarih,
                        OdemeAy = tarih.Month,
                        OdemeYil = tarih.Year,
                        MasrafKalemi = masrafKalemi,
                        Aciklama = string.IsNullOrEmpty(aciklama) ? null : aciklama,
                        Miktar = miktar,
                        Durum = durum,
                        Notlar = string.IsNullOrEmpty(notlar) ? null : notlar,
                        TaksitliMi = false,
                        ToplamTaksitSayisi = 1,
                        KacinciTaksit = 1
                    };

                    odemeler.Add(odeme);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Satýr {rowNum}: {ex.Message}");
                    result.ErrorCount++;
                }
            }

            if (odemeler.Any())
            {
                await CreateBulkOdemeAsync(odemeler);
                result.ImportedCount = odemeler.Count;
                result.ImportedItems = odemeler;
            }

            result.Success = result.ErrorCount == 0 || result.ImportedCount > 0;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add($"Excel dosyasý okunamadý: {ex.Message}");
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
        var odemeler = await _context.BudgetOdemeler
            .Where(o => o.OdemeYil == yil && o.OdemeAy == ay)
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
}
