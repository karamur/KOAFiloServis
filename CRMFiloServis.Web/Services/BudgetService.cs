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

    public async Task<List<BudgetOdeme>> GetOdemelerAsync(int yil, int? ay = null, int? firmaId = null)
    {
        var query = _context.BudgetOdemeler
            .Where(o => o.OdemeYil == yil);

        if (ay.HasValue)
            query = query.Where(o => o.OdemeAy == ay.Value);

        if (firmaId.HasValue)
            query = query.Where(o => o.FirmaId == firmaId.Value);

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
                        o.Durum == OdemeDurum.Bekliyor);

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
        // Önce Change Tracker'daki mevcut entity'yi temizle
        var trackedEntity = _context.ChangeTracker.Entries<BudgetOdeme>()
            .FirstOrDefault(e => e.Entity.Id == odeme.Id);
        
        if (trackedEntity != null)
        {
            trackedEntity.State = EntityState.Detached;
        }
        
        // DateTime'i UTC olarak ayarla
        odeme.OdemeTarihi = DateTime.SpecifyKind(odeme.OdemeTarihi, DateTimeKind.Utc);
        odeme.OdemeAy = odeme.OdemeTarihi.Month;
        odeme.OdemeYil = odeme.OdemeTarihi.Year;
        odeme.UpdatedAt = DateTime.UtcNow;
        
        // Entity'yi Update et (Attach + Modified)
        _context.BudgetOdemeler.Update(odeme);

        await _context.SaveChangesAsync();
        
        // Kaydettikten sonra detach et (sonraki işlemler için temiz başlasın)
        _context.Entry(odeme).State = EntityState.Detached;
        
        return odeme;
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
    /// Kalici silme - veritabanindan tamamen siler
    /// </summary>
    public async Task HardDeleteOdemeAsync(int id)
    {
        var odeme = await _context.BudgetOdemeler
            .IgnoreQueryFilters() // Soft delete filtresini atla
            .FirstOrDefaultAsync(o => o.Id == id);
            
        if (odeme != null)
        {
            _context.BudgetOdemeler.Remove(odeme);
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
                Aciklama = $"Butce Odemesi: {odeme.MasrafKalemi}" + 
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
        var taksitler = new List<BudgetOdeme>();

        // Baslangic tarihini UTC olarak ayarla
        var baslangicUtc = DateTime.SpecifyKind(request.BaslangicTarihi, DateTimeKind.Utc);

        if (request.TaksitPlani != null && request.TaksitPlani.Any())
        {
            // Kullanicinin ozel taksit plani varsa onu kullan
            foreach (var plan in request.TaksitPlani.OrderBy(x => x.Sira))
            {
                var taksitTarihi = DateTime.SpecifyKind(plan.Tarih, DateTimeKind.Utc);
                
                var odeme = new BudgetOdeme
                {
                    OdemeTarihi = taksitTarihi,
                    OdemeAy = taksitTarihi.Month,
                    OdemeYil = taksitTarihi.Year,
                    MasrafKalemi = request.MasrafKalemi,
                    Aciklama = request.Aciklama,
                    Miktar = plan.Tutar,
                    TaksitliMi = true,
                    ToplamTaksitSayisi = request.TaksitSayisi,
                    KacinciTaksit = plan.Sira,
                    TaksitGrupId = taksitGrupId,
                    TaksitBaslangicAy = baslangicUtc,
                    TaksitBitisAy = baslangicUtc.AddMonths(request.TaksitSayisi - 1), // Yaklasik bitis tarihi
                    Notlar = request.Notlar,
                    FirmaId = request.FirmaId,
                    Durum = OdemeDurum.Bekliyor,
                    CreatedAt = DateTime.UtcNow
                };

                taksitler.Add(odeme);
            }
        }
        else
        {
            // Otomatik hesaplama (Eski yontem - fallback olarak birakiyorum)
            var taksitTutari = Math.Round(request.ToplamTutar / request.TaksitSayisi, 2);
            var toplamHesaplanan = taksitTutari * request.TaksitSayisi;
            var fark = request.ToplamTutar - toplamHesaplanan;

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
                    FirmaId = request.FirmaId,
                    Durum = OdemeDurum.Bekliyor,
                    CreatedAt = DateTime.UtcNow
                };

                taksitler.Add(odeme);
            }
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

    #region Excel Islemleri

    public async Task<byte[]> GetExcelSablonAsync(List<Firma> firmalar)
    {
        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Odeme Sablonu");

        // Basliklar
        var headers = new[] { "Odeme Tarihi*", "Masraf Kalemi*", "Aciklama", "Miktar*", "Durum", "Firma", "Notlar" };
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
        worksheet.Cell(2, 6).Value = firmalar.FirstOrDefault()?.FirmaAdi ?? "";
        worksheet.Cell(2, 7).Value = "";

        // Aciklama
        worksheet.Cell(5, 1).Value = "ACIKLAMALAR:";
        worksheet.Cell(5, 1).Style.Font.Bold = true;
        worksheet.Cell(6, 1).Value = "* Odeme Tarihi: GG.AA.YYYY formatinda";
        worksheet.Cell(7, 1).Value = "* Durum: Bekliyor, Odendi, Ertelendi, Iptal";
        
        // Firma listesi
        worksheet.Cell(9, 1).Value = "FIRMALAR:";
        worksheet.Cell(9, 1).Style.Font.Bold = true;
        int row = 10;
        foreach (var firma in firmalar)
        {
            worksheet.Cell(row++, 1).Value = firma.FirmaAdi;
        }

        // Masraf kalemleri
        row += 2;
        worksheet.Cell(row, 1).Value = "MASRAF KALEMLERI:";
        worksheet.Cell(row++, 1).Style.Font.Bold = true;
        
        var masrafKalemleri = await _context.BudgetMasrafKalemleri.Where(m => m.Aktif).OrderBy(m => m.KalemAdi).ToListAsync();
        foreach (var kalem in masrafKalemleri)
        {
            worksheet.Cell(row++, 1).Value = kalem.KalemAdi;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<int> ImportFromExcelAsync(byte[] fileContent)
    {
        var odemeler = new List<BudgetOdeme>();

        using var stream = new MemoryStream(fileContent);
        using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1);

        var rows = worksheet.RowsUsed().Skip(1);

        foreach (var row in rows)
        {
            var tarihStr = row.Cell(1).GetString().Trim();
            if (string.IsNullOrEmpty(tarihStr) || tarihStr.StartsWith("ACIKLAMA") || tarihStr.StartsWith("FIRMA") || tarihStr.StartsWith("MASRAF") || tarihStr.StartsWith("*"))
                continue;

            DateTime tarih;
            if (row.Cell(1).DataType == ClosedXML.Excel.XLDataType.DateTime)
                tarih = row.Cell(1).GetDateTime();
            else if (row.Cell(1).DataType == ClosedXML.Excel.XLDataType.Number)
                tarih = DateTime.FromOADate(row.Cell(1).GetDouble());
            else if (!DateTime.TryParse(tarihStr, new System.Globalization.CultureInfo("tr-TR"), out tarih))
                continue;

            var masrafKalemi = row.Cell(2).GetString().Trim();
            if (string.IsNullOrEmpty(masrafKalemi)) continue;

            decimal miktar;
            if (row.Cell(4).DataType == ClosedXML.Excel.XLDataType.Number)
                miktar = (decimal)row.Cell(4).GetDouble();
            else
            {
                var miktarStr = row.Cell(4).GetString().Replace(".", "").Replace(",", ".");
                if (!decimal.TryParse(miktarStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out miktar))
                    continue;
            }

            if (miktar <= 0) continue;

            var durumStr = row.Cell(5).GetString().Trim().ToLower();
            var durum = durumStr switch
            {
                "odendi" => OdemeDurum.Odendi,
                "ertelendi" => OdemeDurum.Ertelendi,
                "iptal" => OdemeDurum.Iptal,
                _ => OdemeDurum.Bekliyor
            };

            // Firma bul
            var firmaAdi = row.Cell(6).GetString().Trim();
            int? firmaId = null;
            if (!string.IsNullOrEmpty(firmaAdi))
            {
                var firma = await _context.Firmalar.FirstOrDefaultAsync(f => f.FirmaAdi == firmaAdi);
                firmaId = firma?.Id;
            }

            var tarihUtc = DateTime.SpecifyKind(tarih, DateTimeKind.Utc);

            var odeme = new BudgetOdeme
            {
                OdemeTarihi = tarihUtc,
                OdemeAy = tarih.Month,
                OdemeYil = tarih.Year,
                MasrafKalemi = masrafKalemi,
                Aciklama = row.Cell(3).GetString().Trim(),
                Miktar = miktar,
                Durum = durum,
                FirmaId = firmaId,
                Notlar = row.Cell(7).GetString().Trim(),
                TaksitliMi = false,
                ToplamTaksitSayisi = 1,
                KacinciTaksit = 1,
                CreatedAt = DateTime.UtcNow
            };

            odemeler.Add(odeme);
        }

        if (odemeler.Any())
        {
            _context.BudgetOdemeler.AddRange(odemeler);
            await _context.SaveChangesAsync();
        }

        return odemeler.Count;
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
            kalem.Aktif = false;
            await _context.SaveChangesAsync();
        }
    }

    public async Task SeedMasrafKalemleriAsync()
    {
        // Varsayılan masraf kalemleri
        var varsayilanKalemler = new List<(string Adi, string Kategori, string Icon, string Renk, int SiraNo)>
        {
            ("Kira", "Sabit Giderler", "bi-house", "#6c757d", 1),
            ("Elektrik", "Faturalar", "bi-lightning", "#ffc107", 2),
            ("Su", "Faturalar", "bi-droplet", "#0dcaf0", 3),
            ("Doğalgaz", "Faturalar", "bi-fire", "#fd7e14", 4),
            ("İnternet", "Faturalar", "bi-wifi", "#6610f2", 5),
            ("Telefon", "Faturalar", "bi-telephone", "#20c997", 6),
            ("Personel Maaş", "Personel", "bi-people", "#0d6efd", 7),
            ("SGK", "Personel", "bi-shield-check", "#198754", 8),
            ("Vergi", "Vergiler", "bi-bank", "#dc3545", 9),
            ("Akaryakıt", "Araç Giderleri", "bi-fuel-pump", "#fd7e14", 10),
            ("Sigorta", "Sigorta", "bi-shield", "#6f42c1", 11),
            ("Bakım/Onarım", "Araç Giderleri", "bi-tools", "#6c757d", 12),
            ("Kredi Kartı", "Finans", "bi-credit-card", "#dc3545", 13),
            ("Banka Kredisi", "Finans", "bi-bank2", "#0d6efd", 14),
            ("Araç Kredisi", "Finans", "bi-car-front", "#198754", 15),
            ("Diğer", "Diğer", "bi-three-dots", "#6c757d", 99)
        };

        foreach (var (adi, kategori, icon, renk, siraNo) in varsayilanKalemler)
        {
            var mevcutMu = await _context.BudgetMasrafKalemleri
                .IgnoreQueryFilters()
                .AnyAsync(m => m.KalemAdi == adi);

            if (!mevcutMu)
            {
                _context.BudgetMasrafKalemleri.Add(new BudgetMasrafKalemi
                {
                    KalemAdi = adi,
                    Kategori = kategori,
                    Icon = icon,
                    Renk = renk,
                    SiraNo = siraNo,
                    Aktif = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    #endregion

    #region Raporlar

    public async Task<BudgetOzet> GetAylikOzetAsync(int yil, int ay, int? firmaId = null)
    {
        var query = _context.BudgetOdemeler
            .Where(o => o.OdemeYil == yil && o.OdemeAy == ay);

        if (firmaId.HasValue)
            query = query.Where(o => o.FirmaId == firmaId.Value);

        var odemeler = await query.ToListAsync();

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

    public async Task<BudgetYillikOzet> GetYillikOzetAsync(int yil, int? firmaId = null)
    {
        // Add this to ensure payments are automatically generated for all months in the year view
        for (int m = 1; m <= 12; m++)
        {
            await TekrarlayanOdemelerdenKayitOlusturAsync(yil, m, firmaId);
        }

        var query = _context.BudgetOdemeler.Where(o => o.OdemeYil == yil);

        if (firmaId.HasValue)
            query = query.Where(o => o.FirmaId == firmaId.Value);

        var odemeler = await query.ToListAsync();

        var ozet = new BudgetYillikOzet
        {
            Yil = yil,
            ToplamOdeme = odemeler.Sum(o => o.Miktar)
        };

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

    public async Task<List<BudgetGunlukOzet>> GetTakvimDataAsync(int yil, int ay, int? firmaId = null)
    {
        // Tekrarlayan odemelerden bu ay icin otomatik kayit olustur
        await TekrarlayanOdemelerdenKayitOlusturAsync(yil, ay, firmaId);

        var query = _context.BudgetOdemeler
            .Where(o => o.OdemeYil == yil && o.OdemeAy == ay);

        if (firmaId.HasValue)
            query = query.Where(o => o.FirmaId == firmaId.Value);

        var odemeler = await query.OrderBy(o => o.OdemeTarihi).ToListAsync();

        var gunlukOzetler = new List<BudgetGunlukOzet>();
        var gunSayisi = DateTime.DaysInMonth(yil, ay);

        for (int gun = 1; gun <= gunSayisi; gun++)
        {
            var tarih = new DateTime(yil, ay, gun);
            var gunOdemeleri = odemeler.Where(o => o.OdemeTarihi.Day == gun).ToList();
            var bekleyenOdemeler = gunOdemeleri.Where(o => o.Durum == OdemeDurum.Bekliyor).ToList();

            gunlukOzetler.Add(new BudgetGunlukOzet
            {
                Tarih = tarih,
                Gun = gun,
                ToplamOdeme = gunOdemeleri.Sum(o => o.Miktar),
                OdemeSayisi = gunOdemeleri.Count,
                BekleyenToplamOdeme = bekleyenOdemeler.Sum(o => o.Miktar),
                BekleyenOdemeSayisi = bekleyenOdemeler.Count,
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

    public async Task<List<KrediOzet>> GetAktifKredilerAsync(int? firmaId = null)
    {
        var query = _context.BudgetOdemeler
            .Where(o => o.TaksitliMi && o.TaksitGrupId.HasValue);

        if (firmaId.HasValue)
            query = query.Where(o => o.FirmaId == firmaId.Value);

        var taksitliOdemeler = await query.ToListAsync();

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

        if (periyod == "gunluk")
        {
            var gunler = odemeler.GroupBy(o => o.OdemeTarihi.Date);
            foreach (var gun in gunler.OrderBy(g => g.Key))
            {
                trendData.Add(new BudgetTrendData
                {
                    Etiket = gun.Key.ToString("dd.MM"),
                    Tarih = gun.Key,
                    Toplam = gun.Sum(o => o.Miktar),
                    Odenen = gun.Where(o => o.Durum == OdemeDurum.Odendi).Sum(o => o.Miktar),
                    Bekleyen = gun.Where(o => o.Durum == OdemeDurum.Bekliyor).Sum(o => o.Miktar),
                    OdemeSayisi = gun.Count()
                });
            }
        }
        else
        {
            var aylar = odemeler.GroupBy(o => new { o.OdemeYil, o.OdemeAy });
            foreach (var ay in aylar.OrderBy(a => a.Key.OdemeYil).ThenBy(a => a.Key.OdemeAy))
            {
                trendData.Add(new BudgetTrendData
                {
                    Etiket = AyAdlari[ay.Key.OdemeAy],
                    Tarih = new DateTime(ay.Key.OdemeYil, ay.Key.OdemeAy, 1),
                    Toplam = ay.Sum(o => o.Miktar),
                    Odenen = ay.Where(o => o.Durum == OdemeDurum.Odendi).Sum(o => o.Miktar),
                    Bekleyen = ay.Where(o => o.Durum == OdemeDurum.Bekliyor).Sum(o => o.Miktar),
                    OdemeSayisi = ay.Count()
                });
            }
        }

        return trendData;
    }

    #endregion

    #region Tekrarlayan Odeme Islemleri

    public async Task<List<TekrarlayanOdeme>> GetTekrarlayanOdemelerAsync(int? firmaId = null)
    {
        var query = _context.TekrarlayanOdemeler
            .Where(t => !t.IsDeleted)
            .AsQueryable();

        if (firmaId.HasValue)
            query = query.Where(t => t.FirmaId == firmaId.Value);

        return await query
            .Include(t => t.Firma)
            .OrderBy(t => t.Aktif ? 0 : 1) // Aktifler önce
            .ThenBy(t => t.OdemeGunu)
            .ThenBy(t => t.OdemeAdi)
            .ToListAsync();
    }

    public async Task<List<TekrarlayanOdeme>> GetAktifTekrarlayanOdemelerAsync(int? firmaId = null)
    {
        var bugun = DateTime.Today;
        var query = _context.TekrarlayanOdemeler
            .Where(t => !t.IsDeleted && t.Aktif);

        if (firmaId.HasValue)
            query = query.Where(t => t.FirmaId == firmaId.Value);

        return await query
            .Include(t => t.Firma)
            .OrderBy(t => t.OdemeGunu)
            .ThenBy(t => t.OdemeAdi)
            .ToListAsync();
    }

    public async Task<TekrarlayanOdeme?> GetTekrarlayanOdemeByIdAsync(int id)
    {
        return await _context.TekrarlayanOdemeler
            .Include(t => t.Firma)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
    }

    public async Task<TekrarlayanOdeme> CreateTekrarlayanOdemeAsync(TekrarlayanOdeme odeme)
    {
        odeme.BaslangicTarihi = DateTime.SpecifyKind(odeme.BaslangicTarihi, DateTimeKind.Utc);
        if (odeme.BitisTarihi.HasValue)
            odeme.BitisTarihi = DateTime.SpecifyKind(odeme.BitisTarihi.Value, DateTimeKind.Utc);
        odeme.CreatedAt = DateTime.UtcNow;

        _context.TekrarlayanOdemeler.Add(odeme);
        await _context.SaveChangesAsync();
        
        // Tracking'den cikar - ayni context uzerinde tekrar islem yapilabilsin
        _context.Entry(odeme).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        
        return odeme;
    }

    public async Task<TekrarlayanOdeme> UpdateTekrarlayanOdemeAsync(TekrarlayanOdeme odeme)
    {
        var existing = await _context.TekrarlayanOdemeler.FindAsync(odeme.Id);
        if (existing == null)
            throw new Exception("Tekrarlayan odeme bulunamadi");

        existing.OdemeAdi = odeme.OdemeAdi;
        existing.MasrafKalemi = odeme.MasrafKalemi;
        existing.Aciklama = odeme.Aciklama;
        existing.Tutar = odeme.Tutar;
        existing.Periyod = odeme.Periyod;
        existing.OdemeGunu = odeme.OdemeGunu;
        existing.BaslangicTarihi = DateTime.SpecifyKind(odeme.BaslangicTarihi, DateTimeKind.Utc);
        existing.BitisTarihi = odeme.BitisTarihi.HasValue
            ? DateTime.SpecifyKind(odeme.BitisTarihi.Value, DateTimeKind.Utc)
            : null;
        existing.HatirlatmaGunSayisi = odeme.HatirlatmaGunSayisi;
        existing.FirmaId = odeme.FirmaId;
        existing.Aktif = odeme.Aktif;
        existing.Renk = odeme.Renk;
        existing.Icon = odeme.Icon;
        existing.Notlar = odeme.Notlar;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        // Tracking'den cikar
        _context.Entry(existing).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        
        return existing;
    }

    public async Task DeleteTekrarlayanOdemeAsync(int id)
    {
        var odeme = await _context.TekrarlayanOdemeler.FindAsync(id);
        if (odeme != null)
        {
            odeme.IsDeleted = true;
            odeme.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            _context.Entry(odeme).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        }
    }

    /// <summary>
    /// Tekrarlayan odeme tanimlarindan, belirtilen ay icin BudgetOdeme kayitlari olusturur.
    /// Ayni plan + ayni ay icin daha once kayit varsa tekrar olusturmaz.
    /// </summary>
    public async Task<int> TekrarlayanOdemelerdenKayitOlusturAsync(int yil, int ay, int? firmaId = null)
    {
        var aktifPlanlar = await GetAktifTekrarlayanOdemelerAsync(firmaId);
        var olusturulanSayisi = 0;

        foreach (var plan in aktifPlanlar)
        {
            // Periyod kontrolu - bu ay odeme gunu olup olmadigini kontrol et
            if (!PeriyodaUygunMu(plan, yil, ay))
                continue;

            // Odeme gunu - ayin gun sayisindan buyukse son gunu al
            var gunSayisi = DateTime.DaysInMonth(yil, ay);
            var odemeGunu = Math.Min(plan.OdemeGunu, gunSayisi);

            // Bu plan + bu ay icin kayit var mi kontrol et
            var mevcutKayit = await _context.BudgetOdemeler
                .AnyAsync(o => o.OdemeYil == yil &&
                               o.OdemeAy == ay &&
                               o.MasrafKalemi == plan.MasrafKalemi &&
                               o.Aciklama != null && o.Aciklama.StartsWith("[Tekrarlayan") &&
                               o.Aciklama.Contains($"#{plan.Id}]"));

            if (!mevcutKayit)
            {
                var odemeTarihi = DateTime.SpecifyKind(new DateTime(yil, ay, odemeGunu), DateTimeKind.Utc);

                var yeniOdeme = new BudgetOdeme
                {
                    OdemeTarihi = odemeTarihi,
                    OdemeAy = ay,
                    OdemeYil = yil,
                    MasrafKalemi = plan.MasrafKalemi,
                    Aciklama = $"[Tekrarlayan#{plan.Id}] {plan.OdemeAdi}",
                    Miktar = plan.Tutar,
                    FirmaId = plan.FirmaId,
                    Durum = OdemeDurum.Bekliyor,
                    TaksitliMi = false,
                    ToplamTaksitSayisi = 1,
                    KacinciTaksit = 1,
                    Notlar = plan.Notlar,
                    CreatedAt = DateTime.UtcNow
                };

                _context.BudgetOdemeler.Add(yeniOdeme);
                olusturulanSayisi++;
            }
        }

        if (olusturulanSayisi > 0)
            await _context.SaveChangesAsync();

        return olusturulanSayisi;
    }

    /// <summary>
    /// Belirtilen tekrarlayan odeme planinin, verilen yil/ay icin gecerli olup olmadigini kontrol eder.
    /// </summary>
    private bool PeriyodaUygunMu(TekrarlayanOdeme plan, int yil, int ay)
    {
        var kontrolTarihi = new DateTime(yil, ay, 1);
        var baslangic = new DateTime(plan.BaslangicTarihi.Year, plan.BaslangicTarihi.Month, 1);

        if (kontrolTarihi < baslangic)
            return false;

        if (plan.BitisTarihi.HasValue)
        {
            var bitis = new DateTime(plan.BitisTarihi.Value.Year, plan.BitisTarihi.Value.Month, 1);
            if (kontrolTarihi > bitis)
                return false;
        }

        // Periyod kontrolu
        var ayFarki = ((yil - plan.BaslangicTarihi.Year) * 12) + (ay - plan.BaslangicTarihi.Month);
        var periyodAySayisi = (int)plan.Periyod;

        return ayFarki % periyodAySayisi == 0;
    }

    #endregion

    #region Kredi/Taksit Detay Metodları

    public async Task<List<KrediOzet>> GetKrediOzetleriAsync(int? yil = null, int? firmaId = null)
    {
        var query = _context.BudgetOdemeler
            .Where(o => o.TaksitliMi && o.TaksitGrupId.HasValue && !o.IsDeleted);

        if (firmaId.HasValue)
            query = query.Where(o => o.FirmaId == firmaId.Value);

        var taksitliOdemeler = await query.ToListAsync();

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
            .ToList();

        // Yıl filtresi
        if (yil.HasValue && yil > 0)
        {
            krediler = krediler
                .Where(k => k.BaslangicTarihi.Year <= yil && k.BitisTarihi.Year >= yil)
                .ToList();
        }

        return krediler.OrderBy(k => k.MasrafKalemi).ToList();
    }

    public async Task<List<KrediTaksitDetay>> GetKrediTaksitDetaylariAsync(Guid taksitGrupId)
    {
        var taksitler = await _context.BudgetOdemeler
            .Where(o => o.TaksitGrupId == taksitGrupId && !o.IsDeleted)
            .OrderBy(o => o.KacinciTaksit)
            .ToListAsync();

        return taksitler.Select(t => new KrediTaksitDetay
        {
            MasrafKalemi = t.MasrafKalemi,
            Aciklama = t.Aciklama,
            KacinciTaksit = t.KacinciTaksit,
            ToplamTaksitSayisi = t.ToplamTaksitSayisi,
            Tutar = t.Miktar,
            Durum = t.Durum,
            OdemeTarihi = t.OdemeTarihi
        }).ToList();
    }

    public async Task<BudgetOdeme?> GetTaksitOdemeAsync(Guid taksitGrupId, int taksitNo)
    {
        return await _context.BudgetOdemeler
            .FirstOrDefaultAsync(o => o.TaksitGrupId == taksitGrupId && 
                                      o.KacinciTaksit == taksitNo && 
                                      !o.IsDeleted);
    }

    public async Task OdemeYapAsync(int odemeId, int bankaHesapId, DateTime odemeTarihi)
    {
        var odeme = await _context.BudgetOdemeler.FindAsync(odemeId);
        if (odeme == null)
            throw new Exception("Ödeme bulunamadı");

        odeme.Durum = OdemeDurum.Odendi;
        odeme.OdemeTarihi = DateTime.SpecifyKind(odemeTarihi, DateTimeKind.Utc);
        odeme.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
    }

    public async Task TaksitliOdemeOlusturAsync(object request)
    {
        // Request'i dynamic olarak işle
        var requestType = request.GetType();
        var masrafKalemi = requestType.GetProperty("MasrafKalemi")?.GetValue(request)?.ToString() ?? "";
        var aciklama = requestType.GetProperty("Aciklama")?.GetValue(request)?.ToString();
        var baslangicTarihi = (DateTime)(requestType.GetProperty("BaslangicTarihi")?.GetValue(request) ?? DateTime.Today);
        var taksitSayisi = (int)(requestType.GetProperty("TaksitSayisi")?.GetValue(request) ?? 1);
        var toplamTutar = (decimal)(requestType.GetProperty("ToplamTutar")?.GetValue(request) ?? 0);

        var taksitliRequest = new TaksitliOdemeRequest
        {
            MasrafKalemi = masrafKalemi,
            Aciklama = aciklama,
            BaslangicTarihi = baslangicTarihi,
            TaksitSayisi = taksitSayisi,
            ToplamTutar = toplamTutar
        };

        await CreateTaksitliOdemeAsync(taksitliRequest);
    }

    #endregion
}
