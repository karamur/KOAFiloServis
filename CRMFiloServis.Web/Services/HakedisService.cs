using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace CRMFiloServis.Web.Services;

/// <summary>
/// Hakedis/Puantaj kayıt istatistikleri
/// </summary>
public class HakedisIstatistikleri
{
    public int ToplamKayit { get; set; }
    public int TaslakKayit { get; set; }
    public int OnayliKayit { get; set; }
    public decimal ToplamGelir { get; set; }
    public decimal ToplamAlinacak { get; set; }
    public decimal ToplamGider { get; set; }
    public decimal ToplamOdenecek { get; set; }
    public decimal ToplamFark { get; set; }
    public int FaturasizGelir { get; set; }
    public int FaturasizGider { get; set; }
    public int OdenmemisGelir { get; set; }
    public int OdenmemisGider { get; set; }
}

/// <summary>
/// Excel import önizleme satırı
/// </summary>
public class HakedisExcelSatiri
{
    public int SatirNo { get; set; }
    public string? KurumAdi { get; set; }
    public string? GuzergahAdi { get; set; }
    public string? Yon { get; set; }
    public string? Plaka { get; set; }
    public string? SoforAdi { get; set; }
    public string? SoforTelefon { get; set; }
    public string? FaturaKesiciAdi { get; set; }
    public string? FaturaKesiciTelefon { get; set; }
    public decimal Gun { get; set; }

    // Gelir alanları
    public decimal Gelir { get; set; }
    public decimal ToplamGelir { get; set; }
    public decimal GelirKdv20 { get; set; }
    public decimal GelirKdv10 { get; set; }
    public decimal GelirKesinti { get; set; }
    public decimal Alinacak { get; set; }

    // Gider alanları
    public decimal Gider { get; set; }
    public decimal ToplamGider { get; set; }
    public decimal GiderKdv20 { get; set; }
    public decimal GiderKdv10 { get; set; }
    public decimal GiderKesinti { get; set; }
    public decimal Odenecek { get; set; }

    // Fark
    public decimal Fark => Alinacak - Odenecek;

    // Eşleştirme durumları
    public int? KurumCariId { get; set; }
    public int? GuzergahId { get; set; }
    public int? AracId { get; set; }
    public int? SoforId { get; set; }
    public int? FaturaKesiciCariId { get; set; }

    public bool KurumEslesti { get; set; }
    public bool GuzergahEslesti { get; set; }
    public bool AracEslesti { get; set; }
    public bool SoforEslesti { get; set; }
    public bool FaturaKesiciEslesti { get; set; }

    public bool Gecerli => !string.IsNullOrEmpty(KurumAdi) && !string.IsNullOrEmpty(GuzergahAdi);
    public string? HataMesaji { get; set; }
}

/// <summary>
/// Excel kolon eşleştirme
/// </summary>
public class HakedisKolonEslestirme
{
    public int KurumKolonu { get; set; } = 1;
    public int GuzergahKolonu { get; set; } = 2;
    public int GelirKolonu { get; set; } = 3;
    public int GiderKolonu { get; set; } = 4;
    public int YonKolonu { get; set; } = 5;
    public int PlakaKolonu { get; set; } = 6;
    public int SoforKolonu { get; set; } = 7;
    public int SoforTelefonKolonu { get; set; } = 8;
    public int FaturaKesiciKolonu { get; set; } = 9;
    public int FaturaKesiciTelefonKolonu { get; set; } = 10;
    public int GunKolonu { get; set; } = 11;

    // Gider detay kolonları
    public int ToplamGiderKolonu { get; set; } = 12;
    public int GiderKdv20Kolonu { get; set; } = 13;
    public int GiderKdv10Kolonu { get; set; } = 14;
    public int GiderKesintiKolonu { get; set; } = 15;
    public int OdenecekKolonu { get; set; } = 16;

    // Gelir detay kolonları
    public int ToplamGelirKolonu { get; set; } = 17;
    public int GelirKdv20Kolonu { get; set; } = 18;
    public int GelirKdv10Kolonu { get; set; } = 19;
    public int GelirKesintiKolonu { get; set; } = 20;
    public int AlinacakKolonu { get; set; } = 21;

    // Fark kolonu (opsiyonel)
    public int FarkKolonu { get; set; } = 22;

    public int BaslangicSatiri { get; set; } = 2;
}

public interface IHakedisService
{
    // Puantaj/Hakedis kayıtları
    Task<List<PuantajKayit>> GetHakedislerAsync(int yil, int ay, int? kurumId = null, int? guzergahId = null);
    Task<PuantajKayit?> GetHakedisByIdAsync(int id);
    Task<PuantajKayit> CreateHakedisAsync(PuantajKayit hakedis);
    Task<PuantajKayit> UpdateHakedisAsync(PuantajKayit hakedis);
    Task DeleteHakedisAsync(int id);
    Task<HakedisIstatistikleri> GetIstatistiklerAsync(int yil, int ay);
    
    // Toplu işlemler
    Task TopluOnaylaAsync(List<int> hakedisIdleri, string onaylayanKullanici);
    Task TopluFaturaIsaretle(List<int> hakedisIdleri, bool gelir, string faturaNo);
    Task TopluOdemeIsaretle(List<int> hakedisIdleri, bool gelir, decimal tutar);
    
    // Excel import
    Task<List<HakedisExcelSatiri>> ExcelOnizlemeAsync(Stream excelStream, HakedisKolonEslestirme eslestirme);
    Task<PuantajExcelImport> ExcelImportAsync(
        List<HakedisExcelSatiri> satirlar, 
        int yil, 
        int ay, 
        string dosyaAdi,
        string kullanici,
        bool otomatikOlustur = true);
    Task<List<PuantajExcelImport>> GetImportGecmisiAsync(int? yil = null, int? ay = null);
    
    // Eşleştirme yardımcıları
    Task<List<Cari>> AraKurumAsync(string arama);
    Task<List<Guzergah>> AraGuzergahAsync(string arama, int? kurumId = null);
    Task<List<Sofor>> AraSoforAsync(string arama);
    Task<List<Arac>> AraAracAsync(string arama);
}

public class HakedisService : IHakedisService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public HakedisService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<PuantajKayit>> GetHakedislerAsync(int yil, int ay, int? kurumId = null, int? guzergahId = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.PuantajKayitlar
            .Include(p => p.KurumCari)
            .Include(p => p.Guzergah)
            .Include(p => p.Arac)
            .Include(p => p.Sofor)
            .Include(p => p.OdemeYapilacakCari)
            .Include(p => p.FaturaKesiciCari)
            .Where(p => p.Yil == yil && p.Ay == ay);

        if (kurumId.HasValue)
            query = query.Where(p => p.KurumCariId == kurumId);

        if (guzergahId.HasValue)
            query = query.Where(p => p.GuzergahId == guzergahId);

        return await query.OrderBy(p => p.KurumAdi).ThenBy(p => p.GuzergahAdi).ToListAsync();
    }

    public async Task<PuantajKayit?> GetHakedisByIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.PuantajKayitlar
            .Include(p => p.KurumCari)
            .Include(p => p.Guzergah)
            .Include(p => p.Arac)
            .Include(p => p.Sofor)
            .Include(p => p.OdemeYapilacakCari)
            .Include(p => p.FaturaKesiciCari)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PuantajKayit> CreateHakedisAsync(PuantajKayit hakedis)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        hakedis.HesaplaGelir();
        hakedis.HesaplaGider();
        context.PuantajKayitlar.Add(hakedis);
        await context.SaveChangesAsync();
        return hakedis;
    }

    public async Task<PuantajKayit> UpdateHakedisAsync(PuantajKayit hakedis)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.PuantajKayitlar.FindAsync(hakedis.Id);
        if (existing == null) throw new Exception("Hakedis kaydı bulunamadı.");

        // Tüm alanları güncelle
        existing.Yil = hakedis.Yil;
        existing.Ay = hakedis.Ay;
        existing.KurumCariId = hakedis.KurumCariId;
        existing.KurumAdi = hakedis.KurumAdi;
        existing.GuzergahId = hakedis.GuzergahId;
        existing.GuzergahAdi = hakedis.GuzergahAdi;
        existing.Yon = hakedis.Yon;
        existing.AracId = hakedis.AracId;
        existing.Plaka = hakedis.Plaka;
        existing.SoforId = hakedis.SoforId;
        existing.SoforAdi = hakedis.SoforAdi;
        existing.SoforTelefon = hakedis.SoforTelefon;
        existing.SoforOdemeTipi = hakedis.SoforOdemeTipi;
        existing.OdemeYapilacakCariId = hakedis.OdemeYapilacakCariId;
        existing.FaturaKesiciCariId = hakedis.FaturaKesiciCariId;
        existing.FaturaKesiciAdi = hakedis.FaturaKesiciAdi;
        existing.FaturaKesiciTelefon = hakedis.FaturaKesiciTelefon;
        existing.Gun = hakedis.Gun;
        existing.SeferSayisi = hakedis.SeferSayisi;

        // Gelir alanları
        existing.BirimGelir = hakedis.BirimGelir;
        existing.ToplamGelir = hakedis.ToplamGelir;
        existing.GelirKdvOrani = hakedis.GelirKdvOrani;
        existing.GelirKdvOrani20 = hakedis.GelirKdvOrani20;
        existing.GelirKdv20Tutari = hakedis.GelirKdv20Tutari;
        existing.GelirKdvOrani10 = hakedis.GelirKdvOrani10;
        existing.GelirKdv10Tutari = hakedis.GelirKdv10Tutari;
        existing.GelirKesinti = hakedis.GelirKesinti;
        existing.Alinacak = hakedis.Alinacak;

        // Gider alanları
        existing.BirimGider = hakedis.BirimGider;
        existing.GiderKdvOrani20 = hakedis.GiderKdvOrani20;
        existing.GiderKdv20Tutari = hakedis.GiderKdv20Tutari;
        existing.GiderKdvOrani10 = hakedis.GiderKdvOrani10;
        existing.GiderKdv10Tutari = hakedis.GiderKdv10Tutari;
        existing.GiderKesinti = hakedis.GiderKesinti;
        existing.Odenecek = hakedis.Odenecek;

        // Fatura durumları
        existing.GelirFaturaKesildi = hakedis.GelirFaturaKesildi;
        existing.GelirFaturaNo = hakedis.GelirFaturaNo;
        existing.GelirFaturaTarihi = hakedis.GelirFaturaTarihi;
        existing.GiderFaturaAlindi = hakedis.GiderFaturaAlindi;
        existing.GiderFaturaNo = hakedis.GiderFaturaNo;
        existing.GiderFaturaTarihi = hakedis.GiderFaturaTarihi;

        // Ödeme durumları
        existing.GelirOdemeDurumu = hakedis.GelirOdemeDurumu;
        existing.GelirOdemeTarihi = hakedis.GelirOdemeTarihi;
        existing.GelirOdenenTutar = hakedis.GelirOdenenTutar;
        existing.GiderOdemeDurumu = hakedis.GiderOdemeDurumu;
        existing.GiderOdemeTarihi = hakedis.GiderOdemeTarihi;
        existing.GiderOdenenTutar = hakedis.GiderOdenenTutar;

        existing.OnayDurum = hakedis.OnayDurum;
        existing.Notlar = hakedis.Notlar;

        existing.HesaplaGelir();
        existing.HesaplaGider();
        
        await context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteHakedisAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var hakedis = await context.PuantajKayitlar.FindAsync(id);
        if (hakedis != null)
        {
            hakedis.IsDeleted = true;
            await context.SaveChangesAsync();
        }
    }

    public async Task<HakedisIstatistikleri> GetIstatistiklerAsync(int yil, int ay)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var kayitlar = await context.PuantajKayitlar
            .Where(p => p.Yil == yil && p.Ay == ay)
            .ToListAsync();

        return new HakedisIstatistikleri
        {
            ToplamKayit = kayitlar.Count,
            TaslakKayit = kayitlar.Count(k => k.OnayDurum == PuantajOnayDurum.Taslak),
            OnayliKayit = kayitlar.Count(k => k.OnayDurum == PuantajOnayDurum.Onaylandi),
            ToplamGelir = kayitlar.Sum(k => k.GelirToplam),
            ToplamAlinacak = kayitlar.Sum(k => k.Alinacak),
            ToplamGider = kayitlar.Sum(k => k.ToplamGider),
            ToplamOdenecek = kayitlar.Sum(k => k.Odenecek),
            ToplamFark = kayitlar.Sum(k => k.FarkTutari),
            FaturasizGelir = kayitlar.Count(k => !k.GelirFaturaKesildi && k.GelirToplam > 0),
            FaturasizGider = kayitlar.Count(k => !k.GiderFaturaAlindi && k.ToplamGider > 0),
            OdenmemisGelir = kayitlar.Count(k => k.GelirOdemeDurumu != PuantajOdemeDurum.Odendi && k.GelirToplam > 0),
            OdenmemisGider = kayitlar.Count(k => k.GiderOdemeDurumu != PuantajOdemeDurum.Odendi && k.Odenecek > 0)
        };
    }

    public async Task TopluOnaylaAsync(List<int> hakedisIdleri, string onaylayanKullanici)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var kayitlar = await context.PuantajKayitlar
            .Where(p => hakedisIdleri.Contains(p.Id))
            .ToListAsync();

        foreach (var kayit in kayitlar)
        {
            kayit.OnayDurum = PuantajOnayDurum.Onaylandi;
            kayit.OnaylayanKullanici = onaylayanKullanici;
            kayit.OnayTarihi = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
    }

    public async Task TopluFaturaIsaretle(List<int> hakedisIdleri, bool gelir, string faturaNo)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var kayitlar = await context.PuantajKayitlar
            .Where(p => hakedisIdleri.Contains(p.Id))
            .ToListAsync();

        foreach (var kayit in kayitlar)
        {
            if (gelir)
            {
                kayit.GelirFaturaKesildi = true;
                kayit.GelirFaturaNo = faturaNo;
                kayit.GelirFaturaTarihi = DateTime.UtcNow;
            }
            else
            {
                kayit.GiderFaturaAlindi = true;
                kayit.GiderFaturaNo = faturaNo;
                kayit.GiderFaturaTarihi = DateTime.UtcNow;
            }
        }

        await context.SaveChangesAsync();
    }

    public async Task TopluOdemeIsaretle(List<int> hakedisIdleri, bool gelir, decimal tutar)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var kayitlar = await context.PuantajKayitlar
            .Where(p => hakedisIdleri.Contains(p.Id))
            .ToListAsync();

        foreach (var kayit in kayitlar)
        {
            if (gelir)
            {
                kayit.GelirOdemeDurumu = PuantajOdemeDurum.Odendi;
                kayit.GelirOdemeTarihi = DateTime.UtcNow;
                kayit.GelirOdenenTutar = tutar > 0 ? tutar : kayit.GelirToplam;
            }
            else
            {
                kayit.GiderOdemeDurumu = PuantajOdemeDurum.Odendi;
                kayit.GiderOdemeTarihi = DateTime.UtcNow;
                kayit.GiderOdenenTutar = tutar > 0 ? tutar : kayit.Odenecek;
            }
        }

        await context.SaveChangesAsync();
    }

    public async Task<List<HakedisExcelSatiri>> ExcelOnizlemeAsync(Stream excelStream, HakedisKolonEslestirme eslestirme)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var satirlar = new List<HakedisExcelSatiri>();
        
        using var package = new ExcelPackage(excelStream);
        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
        if (worksheet == null) return satirlar;

        var rowCount = worksheet.Dimension?.Rows ?? 0;
        
        // Tüm cari, güzergah, araç ve şoförleri cache'le
        var cariler = await context.Cariler.Where(c => c.Aktif).ToListAsync();
        var guzergahlar = await context.Guzergahlar.Where(g => g.Aktif).ToListAsync();
        var araclar = await context.Araclar.Where(a => a.Aktif).Include(a => a.PlakaGecmisi).ToListAsync();
        var soforler = await context.Soforler.Where(s => s.Aktif).ToListAsync();

        for (int row = eslestirme.BaslangicSatiri; row <= rowCount; row++)
        {
            var satir = new HakedisExcelSatiri
            {
                SatirNo = row,
                KurumAdi = GetCellValue(worksheet, row, eslestirme.KurumKolonu),
                GuzergahAdi = GetCellValue(worksheet, row, eslestirme.GuzergahKolonu),
                Yon = GetCellValue(worksheet, row, eslestirme.YonKolonu),
                Plaka = GetCellValue(worksheet, row, eslestirme.PlakaKolonu),
                SoforAdi = GetCellValue(worksheet, row, eslestirme.SoforKolonu),
                SoforTelefon = GetCellValue(worksheet, row, eslestirme.SoforTelefonKolonu),
                FaturaKesiciAdi = GetCellValue(worksheet, row, eslestirme.FaturaKesiciKolonu),
                FaturaKesiciTelefon = GetCellValue(worksheet, row, eslestirme.FaturaKesiciTelefonKolonu),
                Gun = GetCellDecimal(worksheet, row, eslestirme.GunKolonu),

                // Gelir alanları
                Gelir = GetCellDecimal(worksheet, row, eslestirme.GelirKolonu),
                ToplamGelir = GetCellDecimal(worksheet, row, eslestirme.ToplamGelirKolonu),
                GelirKdv20 = GetCellDecimal(worksheet, row, eslestirme.GelirKdv20Kolonu),
                GelirKdv10 = GetCellDecimal(worksheet, row, eslestirme.GelirKdv10Kolonu),
                GelirKesinti = GetCellDecimal(worksheet, row, eslestirme.GelirKesintiKolonu),
                Alinacak = GetCellDecimal(worksheet, row, eslestirme.AlinacakKolonu),

                // Gider alanları
                Gider = GetCellDecimal(worksheet, row, eslestirme.GiderKolonu),
                ToplamGider = GetCellDecimal(worksheet, row, eslestirme.ToplamGiderKolonu),
                GiderKdv20 = GetCellDecimal(worksheet, row, eslestirme.GiderKdv20Kolonu),
                GiderKdv10 = GetCellDecimal(worksheet, row, eslestirme.GiderKdv10Kolonu),
                GiderKesinti = GetCellDecimal(worksheet, row, eslestirme.GiderKesintiKolonu),
                Odenecek = GetCellDecimal(worksheet, row, eslestirme.OdenecekKolonu)
            };

            // Boş satırları atla
            if (string.IsNullOrWhiteSpace(satir.KurumAdi) && string.IsNullOrWhiteSpace(satir.GuzergahAdi))
                continue;

            // Kurum eşleştirme
            if (!string.IsNullOrEmpty(satir.KurumAdi))
            {
                var kurum = cariler.FirstOrDefault(c => 
                    c.Unvan.Equals(satir.KurumAdi, StringComparison.OrdinalIgnoreCase) ||
                    c.Unvan.Contains(satir.KurumAdi, StringComparison.OrdinalIgnoreCase) ||
                    satir.KurumAdi.Contains(c.Unvan, StringComparison.OrdinalIgnoreCase));
                if (kurum != null)
                {
                    satir.KurumCariId = kurum.Id;
                    satir.KurumEslesti = true;
                }
            }

            // Güzergah eşleştirme
            if (!string.IsNullOrEmpty(satir.GuzergahAdi))
            {
                var guzergah = guzergahlar.FirstOrDefault(g =>
                    g.GuzergahAdi.Equals(satir.GuzergahAdi, StringComparison.OrdinalIgnoreCase) ||
                    g.GuzergahKodu.Equals(satir.GuzergahAdi, StringComparison.OrdinalIgnoreCase) ||
                    g.GuzergahAdi.Contains(satir.GuzergahAdi, StringComparison.OrdinalIgnoreCase));
                if (guzergah != null)
                {
                    satir.GuzergahId = guzergah.Id;
                    satir.GuzergahEslesti = true;
                    // Güzergahtan kurum al
                    if (!satir.KurumEslesti && guzergah.CariId > 0)
                    {
                        satir.KurumCariId = guzergah.CariId;
                        satir.KurumEslesti = true;
                    }
                }
            }

            // Araç eşleştirme (plaka ile)
            if (!string.IsNullOrEmpty(satir.Plaka))
            {
                var plakaNormalize = NormalizePlaka(satir.Plaka);
                var arac = araclar.FirstOrDefault(a =>
                    NormalizePlaka(a.AktifPlaka) == plakaNormalize ||
                    a.PlakaGecmisi.Any(p => NormalizePlaka(p.Plaka) == plakaNormalize && !p.IsDeleted));
                if (arac != null)
                {
                    satir.AracId = arac.Id;
                    satir.AracEslesti = true;
                }
            }

            // Şoför eşleştirme
            if (!string.IsNullOrEmpty(satir.SoforAdi))
            {
                var sofor = soforler.FirstOrDefault(s =>
                    s.TamAd.Equals(satir.SoforAdi, StringComparison.OrdinalIgnoreCase) ||
                    s.TamAd.Contains(satir.SoforAdi, StringComparison.OrdinalIgnoreCase) ||
                    satir.SoforAdi.Contains(s.TamAd, StringComparison.OrdinalIgnoreCase));
                if (sofor != null)
                {
                    satir.SoforId = sofor.Id;
                    satir.SoforEslesti = true;
                }
            }

            // Fatura kesici eşleştirme
            if (!string.IsNullOrEmpty(satir.FaturaKesiciAdi))
            {
                var faturaKesici = cariler.FirstOrDefault(c =>
                    c.Unvan.Equals(satir.FaturaKesiciAdi, StringComparison.OrdinalIgnoreCase) ||
                    c.Unvan.Contains(satir.FaturaKesiciAdi, StringComparison.OrdinalIgnoreCase));
                if (faturaKesici != null)
                {
                    satir.FaturaKesiciCariId = faturaKesici.Id;
                    satir.FaturaKesiciEslesti = true;
                }
            }

            satirlar.Add(satir);
        }

        return satirlar;
    }

    public async Task<PuantajExcelImport> ExcelImportAsync(
        List<HakedisExcelSatiri> satirlar, 
        int yil, 
        int ay, 
        string dosyaAdi,
        string kullanici,
        bool otomatikOlustur = true)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var import = new PuantajExcelImport
        {
            DosyaAdi = dosyaAdi,
            ImportTarihi = DateTime.UtcNow,
            ImportEdenKullanici = kullanici,
            Yil = yil,
            Ay = ay,
            ToplamSatir = satirlar.Count,
            Durum = ImportDurum.Isleniyor
        };
        context.PuantajExcelImportlar.Add(import);
        await context.SaveChangesAsync();

        int basarili = 0, hatali = 0, olusturulanFirma = 0, olusturulanGuzergah = 0, olusturulanSofor = 0;

        foreach (var satir in satirlar)
        {
            try
            {
                // Otomatik oluşturma açıksa eksik kayıtları oluştur
                if (otomatikOlustur)
                {
                    // Kurum oluştur
                    if (!satir.KurumEslesti && !string.IsNullOrEmpty(satir.KurumAdi))
                    {
                        var yeniCari = new Cari
                        {
                            CariKodu = await GenerateCariKodu(context),
                            Unvan = satir.KurumAdi,
                            CariTipi = CariTipi.Musteri,
                            Aktif = true
                        };
                        context.Cariler.Add(yeniCari);
                        await context.SaveChangesAsync();
                        satir.KurumCariId = yeniCari.Id;
                        satir.KurumEslesti = true;
                        olusturulanFirma++;
                    }

                    // Güzergah oluştur
                    if (!satir.GuzergahEslesti && !string.IsNullOrEmpty(satir.GuzergahAdi) && satir.KurumCariId.HasValue)
                    {
                        var yeniGuzergah = new Guzergah
                        {
                            GuzergahKodu = await GenerateGuzergahKodu(context),
                            GuzergahAdi = satir.GuzergahAdi,
                            CariId = satir.KurumCariId.Value,
                            BirimFiyat = satir.Gelir,
                            Aktif = true,
                            SeferTipi = ParseYonToSeferTipi(satir.Yon)
                        };
                        context.Guzergahlar.Add(yeniGuzergah);
                        await context.SaveChangesAsync();
                        satir.GuzergahId = yeniGuzergah.Id;
                        satir.GuzergahEslesti = true;
                        olusturulanGuzergah++;
                    }

                    // Şoför oluştur
                    if (!satir.SoforEslesti && !string.IsNullOrEmpty(satir.SoforAdi))
                    {
                        var adSoyad = ParseAdSoyad(satir.SoforAdi);
                        var yeniSofor = new Sofor
                        {
                            SoforKodu = await GenerateSoforKodu(context),
                            Ad = adSoyad.Item1,
                            Soyad = adSoyad.Item2,
                            Telefon = satir.SoforTelefon,
                            Gorev = PersonelGorev.Sofor,
                            Aktif = true
                        };
                        context.Soforler.Add(yeniSofor);
                        await context.SaveChangesAsync();
                        satir.SoforId = yeniSofor.Id;
                        satir.SoforEslesti = true;
                        olusturulanSofor++;
                    }

                    // Fatura kesici oluştur
                    if (!satir.FaturaKesiciEslesti && !string.IsNullOrEmpty(satir.FaturaKesiciAdi))
                    {
                        var yeniCari = new Cari
                        {
                            CariKodu = await GenerateCariKodu(context),
                            Unvan = satir.FaturaKesiciAdi,
                            CariTipi = CariTipi.Tedarikci,
                            Telefon = satir.FaturaKesiciTelefon,
                            Aktif = true
                        };
                        context.Cariler.Add(yeniCari);
                        await context.SaveChangesAsync();
                        satir.FaturaKesiciCariId = yeniCari.Id;
                        satir.FaturaKesiciEslesti = true;
                        olusturulanFirma++;
                    }
                }

                // Puantaj kaydı oluştur
                var puantaj = new PuantajKayit
                {
                    Yil = yil,
                    Ay = ay,
                    KurumCariId = satir.KurumCariId,
                    KurumAdi = satir.KurumAdi,
                    GuzergahId = satir.GuzergahId,
                    GuzergahAdi = satir.GuzergahAdi,
                    Yon = ParseYonToPuantajYon(satir.Yon),
                    AracId = satir.AracId,
                    Plaka = satir.Plaka,
                    SoforId = satir.SoforId,
                    SoforAdi = satir.SoforAdi,
                    SoforTelefon = satir.SoforTelefon,
                    FaturaKesiciCariId = satir.FaturaKesiciCariId,
                    FaturaKesiciAdi = satir.FaturaKesiciAdi,
                    FaturaKesiciTelefon = satir.FaturaKesiciTelefon,
                    Gun = satir.Gun > 0 ? satir.Gun : 1,

                    // Gelir alanları
                    BirimGelir = satir.Gelir,
                    ToplamGelir = satir.ToplamGelir > 0 ? satir.ToplamGelir : satir.Gelir * (satir.Gun > 0 ? satir.Gun : 1),
                    GelirKdv20Tutari = satir.GelirKdv20,
                    GelirKdv10Tutari = satir.GelirKdv10,
                    GelirKesinti = satir.GelirKesinti,
                    Alinacak = satir.Alinacak,

                    // Gider alanları
                    BirimGider = satir.Gider,
                    ToplamGider = satir.ToplamGider > 0 ? satir.ToplamGider : satir.Gider * (satir.Gun > 0 ? satir.Gun : 1),
                    GiderKdv20Tutari = satir.GiderKdv20,
                    GiderKdv10Tutari = satir.GiderKdv10,
                    GiderKesinti = satir.GiderKesinti,
                    Odenecek = satir.Odenecek,

                    Kaynak = PuantajKaynak.ExcelImport,
                    ExcelImportId = import.Id,
                    ExcelSatirNo = satir.SatirNo,
                    OnayDurum = PuantajOnayDurum.Taslak
                };
                puantaj.HesaplaGelir();
                // Gider hesaplaması Excel'den geldiği için override etme
                if (satir.Odenecek <= 0)
                    puantaj.HesaplaGider();

                context.PuantajKayitlar.Add(puantaj);
                basarili++;
            }
            catch (Exception ex)
            {
                satir.HataMesaji = ex.Message;
                hatali++;
            }
        }

        await context.SaveChangesAsync();

        import.BasariliSatir = basarili;
        import.HataliSatir = hatali;
        import.OtoOlusturulanFirma = olusturulanFirma;
        import.OtoOlusturulanGuzergah = olusturulanGuzergah;
        import.OtoOlusturulanSofor = olusturulanSofor;
        import.Durum = hatali > 0 ? ImportDurum.Hata : ImportDurum.Tamamlandi;
        await context.SaveChangesAsync();

        return import;
    }

    public async Task<List<PuantajExcelImport>> GetImportGecmisiAsync(int? yil = null, int? ay = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.PuantajExcelImportlar.AsQueryable();
        
        if (yil.HasValue)
            query = query.Where(i => i.Yil == yil);
        if (ay.HasValue)
            query = query.Where(i => i.Ay == ay);

        return await query.OrderByDescending(i => i.ImportTarihi).ToListAsync();
    }

    public async Task<List<Cari>> AraKurumAsync(string arama)
    {
        if (string.IsNullOrEmpty(arama)) return new List<Cari>();
        
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Cariler
            .Where(c => c.Aktif && (c.CariTipi == CariTipi.Musteri || c.CariTipi == CariTipi.MusteriTedarikci))
            .Where(c => c.Unvan.ToLower().Contains(arama.ToLower()) || c.CariKodu.ToLower().Contains(arama.ToLower()))
            .Take(20)
            .ToListAsync();
    }

    public async Task<List<Guzergah>> AraGuzergahAsync(string arama, int? kurumId = null)
    {
        if (string.IsNullOrEmpty(arama)) return new List<Guzergah>();
        
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Guzergahlar.Where(g => g.Aktif);
        
        if (kurumId.HasValue)
            query = query.Where(g => g.CariId == kurumId);
        
        return await query
            .Where(g => g.GuzergahAdi.ToLower().Contains(arama.ToLower()) || g.GuzergahKodu.ToLower().Contains(arama.ToLower()))
            .Take(20)
            .ToListAsync();
    }

    public async Task<List<Sofor>> AraSoforAsync(string arama)
    {
        if (string.IsNullOrEmpty(arama)) return new List<Sofor>();
        
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Soforler
            .Where(s => s.Aktif && s.Gorev == PersonelGorev.Sofor)
            .Where(s => (s.Ad + " " + s.Soyad).ToLower().Contains(arama.ToLower()) || s.SoforKodu.ToLower().Contains(arama.ToLower()))
            .Take(20)
            .ToListAsync();
    }

    public async Task<List<Arac>> AraAracAsync(string arama)
    {
        if (string.IsNullOrEmpty(arama)) return new List<Arac>();
        
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Araclar
            .Include(a => a.PlakaGecmisi)
            .Where(a => a.Aktif)
            .Where(a => a.AktifPlaka != null && a.AktifPlaka.ToLower().Contains(arama.ToLower()))
            .Take(20)
            .ToListAsync();
    }

    #region Helper Methods

    private string? GetCellValue(ExcelWorksheet ws, int row, int col)
    {
        if (col <= 0) return null;
        var cell = ws.Cells[row, col];
        return cell?.Value?.ToString()?.Trim();
    }

    private decimal GetCellDecimal(ExcelWorksheet ws, int row, int col)
    {
        if (col <= 0) return 0;
        var cell = ws.Cells[row, col];
        if (cell?.Value == null) return 0;
        if (decimal.TryParse(cell.Value.ToString(), out var result))
            return result;
        return 0;
    }

    private string NormalizePlaka(string? plaka)
    {
        if (string.IsNullOrEmpty(plaka)) return "";
        return plaka.Replace(" ", "").Replace("-", "").ToUpperInvariant();
    }

    private PuantajYon ParseYonToPuantajYon(string? yon)
    {
        if (string.IsNullOrEmpty(yon)) return PuantajYon.SabahAksam;
        
        var lower = yon.ToLowerInvariant();
        if (lower.Contains("sabah") && lower.Contains("akşam")) return PuantajYon.SabahAksam;
        if (lower.Contains("sabah") && lower.Contains("aksam")) return PuantajYon.SabahAksam;
        if (lower.Contains("sabah")) return PuantajYon.Sabah;
        if (lower.Contains("akşam") || lower.Contains("aksam")) return PuantajYon.Aksam;
        return PuantajYon.Diger;
    }

    private SeferTipi ParseYonToSeferTipi(string? yon)
    {
        if (string.IsNullOrEmpty(yon)) return SeferTipi.SabahAksam;
        
        var lower = yon.ToLowerInvariant();
        if (lower.Contains("sabah") && lower.Contains("akşam")) return SeferTipi.SabahAksam;
        if (lower.Contains("sabah") && lower.Contains("aksam")) return SeferTipi.SabahAksam;
        if (lower.Contains("sabah")) return SeferTipi.Sabah;
        if (lower.Contains("akşam") || lower.Contains("aksam")) return SeferTipi.Aksam;
        return SeferTipi.Saatlik;
    }

    private (string, string) ParseAdSoyad(string tamAd)
    {
        var parcalar = tamAd.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parcalar.Length == 0) return ("", "");
        if (parcalar.Length == 1) return (parcalar[0], "");
        return (string.Join(" ", parcalar.Take(parcalar.Length - 1)), parcalar.Last());
    }

    private async Task<string> GenerateCariKodu(ApplicationDbContext context)
    {
        var sonKayit = await context.Cariler
            .IgnoreQueryFilters()
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync();
        var sonId = sonKayit?.Id ?? 0;
        return $"C{(sonId + 1):D5}";
    }

    private async Task<string> GenerateGuzergahKodu(ApplicationDbContext context)
    {
        var sonKayit = await context.Guzergahlar
            .IgnoreQueryFilters()
            .OrderByDescending(g => g.Id)
            .FirstOrDefaultAsync();
        var sonId = sonKayit?.Id ?? 0;
        return $"G{(sonId + 1):D5}";
    }

    private async Task<string> GenerateSoforKodu(ApplicationDbContext context)
    {
        var sonKayit = await context.Soforler
            .IgnoreQueryFilters()
            .OrderByDescending(s => s.Id)
            .FirstOrDefaultAsync();
        var sonId = sonKayit?.Id ?? 0;
        return $"P{(sonId + 1):D5}";
    }

    #endregion
}
