using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public interface IFiloOperasyonService
{
    // Komisyonculuk İş
    Task<List<KomisyonculukIs>> GetKomisyonculukIslerAsync();
    Task<List<KomisyonculukIs>> GetAktifKomisyonculukIslerAsync();
    Task<KomisyonculukIs?> GetKomisyonculukIsAsync(int id);
    Task<KomisyonculukIs> CreateKomisyonculukIsAsync(KomisyonculukIs komisyonculukIs);
    Task<KomisyonculukIs> UpdateKomisyonculukIsAsync(KomisyonculukIs komisyonculukIs);
    Task DeleteKomisyonculukIsAsync(int id);
    Task<string> GenerateIsKoduAsync();
    
    // Komisyonculuk İş Atama
    Task<List<KomisyonculukIsAtama>> GetIsAtamalarAsync(int komisyonculukIsId);
    Task<KomisyonculukIsAtama?> GetIsAtamaAsync(int id);
    Task<KomisyonculukIsAtama> CreateIsAtamaAsync(KomisyonculukIsAtama atama);
    Task<KomisyonculukIsAtama> UpdateIsAtamaAsync(KomisyonculukIsAtama atama);
    Task DeleteIsAtamaAsync(int id);
    
    // Araç Alım/Satım
    Task<List<AracAlimSatim>> GetAracAlimSatimlarAsync();
    Task<List<AracAlimSatim>> GetAracAlimSatimlarAsync(int aracId);
    Task<AracAlimSatim?> GetAracAlimSatimAsync(int id);
    Task<AracAlimSatim> CreateAracAlimSatimAsync(AracAlimSatim alimSatim);
    Task<AracAlimSatim> UpdateAracAlimSatimAsync(AracAlimSatim alimSatim);
    Task DeleteAracAlimSatimAsync(int id);
    Task<List<AracAlimSatim>> GetFaturaKontrolBekleyenlerAsync();
    
    // Plaka Dönüşüm
    Task<List<PlakaDonusum>> GetPlakaDonusumlerAsync();
    Task<PlakaDonusum?> GetPlakaDonusumAsync(int id);
    Task<PlakaDonusum> CreatePlakaDonusumAsync(PlakaDonusum donusum);
    Task<PlakaDonusum> UpdatePlakaDonusumAsync(PlakaDonusum donusum);
    Task DeletePlakaDonusumAsync(int id);
    
    // Operasyon Durum
    Task<List<AracOperasyonDurum>> GetAracOperasyonDurumlariAsync(int yil, int ay);
    Task<AracOperasyonDurum?> GetAracOperasyonDurumAsync(int aracId, int yil, int ay);
    Task<AracOperasyonDurum> CreateOrUpdateOperasyonDurumAsync(AracOperasyonDurum durum);
    
    // Raporlar
    Task<KomisyonculukKazancRaporu> GetKomisyonculukKazancRaporuAsync(DateTime baslangic, DateTime bitis);
    Task<List<AracKarZararRaporu>> GetAracKarZararRaporuAsync(int yil, int ay);
    Task<FiloOzetRaporu> GetFiloOzetRaporuAsync(DateTime? tarih = null);
}

public class FiloOperasyonService : IFiloOperasyonService
{
    private readonly ApplicationDbContext _context;

    public FiloOperasyonService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Komisyonculuk İş

    public async Task<List<KomisyonculukIs>> GetKomisyonculukIslerAsync()
    {
        return await _context.KomisyonculukIsler
            .Include(k => k.MusteriCari)
            .Include(k => k.AlinanIsFatura)
            .Include(k => k.Atamalar)
                .ThenInclude(a => a.Arac)
            .Include(k => k.Atamalar)
                .ThenInclude(a => a.Sofor)
            .OrderByDescending(k => k.BaslangicTarihi)
            .ToListAsync();
    }

    public async Task<List<KomisyonculukIs>> GetAktifKomisyonculukIslerAsync()
    {
        return await _context.KomisyonculukIsler
            .Include(k => k.MusteriCari)
            .Include(k => k.Atamalar)
                .ThenInclude(a => a.Arac)
            .Where(k => k.Durum == KomisyonculukIsDurum.Aktif || k.Durum == KomisyonculukIsDurum.Beklemede)
            .OrderByDescending(k => k.BaslangicTarihi)
            .ToListAsync();
    }

    public async Task<KomisyonculukIs?> GetKomisyonculukIsAsync(int id)
    {
        return await _context.KomisyonculukIsler
            .Include(k => k.MusteriCari)
            .Include(k => k.AlinanIsFatura)
            .Include(k => k.Atamalar)
                .ThenInclude(a => a.Arac)
            .Include(k => k.Atamalar)
                .ThenInclude(a => a.Sofor)
            .Include(k => k.Atamalar)
                .ThenInclude(a => a.TedarikciCari)
            .Include(k => k.Atamalar)
                .ThenInclude(a => a.VerilenIsFatura)
            .FirstOrDefaultAsync(k => k.Id == id);
    }

    public async Task<KomisyonculukIs> CreateKomisyonculukIsAsync(KomisyonculukIs komisyonculukIs)
    {
        if (string.IsNullOrEmpty(komisyonculukIs.IsKodu))
        {
            komisyonculukIs.IsKodu = await GenerateIsKoduAsync();
        }
        
        komisyonculukIs.CreatedAt = DateTime.UtcNow;
        _context.KomisyonculukIsler.Add(komisyonculukIs);
        await _context.SaveChangesAsync();
        return komisyonculukIs;
    }

    public async Task<KomisyonculukIs> UpdateKomisyonculukIsAsync(KomisyonculukIs komisyonculukIs)
    {
        var existing = await _context.KomisyonculukIsler.FindAsync(komisyonculukIs.Id);
        if (existing == null)
            throw new InvalidOperationException("Komisyonculuk işi bulunamadı.");

        existing.MusteriCariId = komisyonculukIs.MusteriCariId;
        existing.IsAciklamasi = komisyonculukIs.IsAciklamasi;
        existing.BaslangicTarihi = komisyonculukIs.BaslangicTarihi;
        existing.BitisTarihi = komisyonculukIs.BitisTarihi;
        existing.IsTipi = komisyonculukIs.IsTipi;
        existing.FiyatlamaTipi = komisyonculukIs.FiyatlamaTipi;
        existing.BirimFiyat = komisyonculukIs.BirimFiyat;
        existing.ToplamGun = komisyonculukIs.ToplamGun;
        existing.ToplamSefer = komisyonculukIs.ToplamSefer;
        existing.ToplamTutar = komisyonculukIs.ToplamTutar;
        existing.Durum = komisyonculukIs.Durum;
        existing.Notlar = komisyonculukIs.Notlar;
        existing.AlinanIsFaturaId = komisyonculukIs.AlinanIsFaturaId;
        existing.FaturaKesildi = komisyonculukIs.FaturaKesildi;
        existing.FaturaKesimTarihi = komisyonculukIs.FaturaKesimTarihi;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteKomisyonculukIsAsync(int id)
    {
        var komisyonculukIs = await _context.KomisyonculukIsler.FindAsync(id);
        if (komisyonculukIs != null)
        {
            komisyonculukIs.IsDeleted = true;
            komisyonculukIs.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateIsKoduAsync()
    {
        var yil = DateTime.Today.Year;
        var prefix = $"KOM-{yil}-";
        
        var lastIs = await _context.KomisyonculukIsler
            .IgnoreQueryFilters()
            .Where(k => k.IsKodu.StartsWith(prefix))
            .OrderByDescending(k => k.IsKodu)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastIs != null)
        {
            var lastNumber = lastIs.IsKodu.Replace(prefix, "");
            if (int.TryParse(lastNumber, out int num))
            {
                nextNumber = num + 1;
            }
        }

        return $"{prefix}{nextNumber:D4}";
    }

    #endregion

    #region Komisyonculuk İş Atama

    public async Task<List<KomisyonculukIsAtama>> GetIsAtamalarAsync(int komisyonculukIsId)
    {
        return await _context.KomisyonculukIsAtamalar
            .Include(a => a.Arac)
            .Include(a => a.Sofor)
            .Include(a => a.TedarikciCari)
            .Include(a => a.VerilenIsFatura)
            .Where(a => a.KomisyonculukIsId == komisyonculukIsId)
            .OrderByDescending(a => a.BaslangicTarihi)
            .ToListAsync();
    }

    public async Task<KomisyonculukIsAtama?> GetIsAtamaAsync(int id)
    {
        return await _context.KomisyonculukIsAtamalar
            .Include(a => a.Arac)
            .Include(a => a.Sofor)
            .Include(a => a.TedarikciCari)
            .Include(a => a.VerilenIsFatura)
            .Include(a => a.KomisyonculukIs)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<KomisyonculukIsAtama> CreateIsAtamaAsync(KomisyonculukIsAtama atama)
    {
        await UygulaAtamaKurallariAsync(atama);
        atama.CreatedAt = DateTime.UtcNow;
        _context.KomisyonculukIsAtamalar.Add(atama);
        await _context.SaveChangesAsync();
        return atama;
    }

    public async Task<KomisyonculukIsAtama> UpdateIsAtamaAsync(KomisyonculukIsAtama atama)
    {
        await UygulaAtamaKurallariAsync(atama);
        var existing = await _context.KomisyonculukIsAtamalar.FindAsync(atama.Id);
        if (existing == null)
            throw new InvalidOperationException("İş ataması bulunamadı.");

        existing.AracId = atama.AracId;
        existing.TedarikciCariId = atama.TedarikciCariId;
        existing.DisAracPlaka = atama.DisAracPlaka;
        existing.AtamaTipi = atama.AtamaTipi;
        existing.SoforId = atama.SoforId;
        existing.DisSoforAdSoyad = atama.DisSoforAdSoyad;
        existing.DisSoforTelefon = atama.DisSoforTelefon;
        existing.AracKiraBedeli = atama.AracKiraBedeli;
        existing.SoforMaliyeti = atama.SoforMaliyeti;
        existing.YakitMaliyeti = atama.YakitMaliyeti;
        existing.OtoyolMaliyeti = atama.OtoyolMaliyeti;
        existing.DigerMasraflar = atama.DigerMasraflar;
        existing.VerilenIsFaturaId = atama.VerilenIsFaturaId;
        existing.TedarikciOdendi = atama.TedarikciOdendi;
        existing.TedarikciOdemeTarihi = atama.TedarikciOdemeTarihi;
        existing.BaslangicTarihi = atama.BaslangicTarihi;
        existing.BitisTarihi = atama.BitisTarihi;
        existing.Notlar = atama.Notlar;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteIsAtamaAsync(int id)
    {
        var atama = await _context.KomisyonculukIsAtamalar.FindAsync(id);
        if (atama != null)
        {
            atama.IsDeleted = true;
            atama.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private async Task UygulaAtamaKurallariAsync(KomisyonculukIsAtama atama)
    {
        if (!atama.AracId.HasValue)
            return;

        var arac = await _context.Araclar
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == atama.AracId.Value && !a.IsDeleted);

        if (arac == null)
            throw new InvalidOperationException("Atama için seçilen araç bulunamadı.");

        switch (arac.SahiplikTipi)
        {
            case AracSahiplikTipi.Ozmal:
                if (!atama.SoforId.HasValue)
                    throw new InvalidOperationException("Özmal araç atamalarında firma şoförü seçilmelidir.");

                atama.AtamaTipi = AracAtamaTipi.OzmalSoforlu;
                atama.TedarikciCariId = null;
                atama.DisAracPlaka = null;
                atama.DisSoforAdSoyad = null;
                atama.DisSoforTelefon = null;
                atama.AracKiraBedeli = 0;
                break;

            case AracSahiplikTipi.Kiralik:
                if (!atama.SoforId.HasValue)
                    throw new InvalidOperationException("Kiralık araç atamalarında firma şoförü seçilmelidir.");

                atama.AtamaTipi = AracAtamaTipi.KiralikKendiSofor;
                atama.TedarikciCariId ??= arac.KiralikCariId;
                atama.DisAracPlaka = null;
                atama.DisSoforAdSoyad = null;
                atama.DisSoforTelefon = null;

                if (atama.AracKiraBedeli <= 0)
                    atama.AracKiraBedeli = arac.SeferBasinaKiraBedeli ?? arac.GunlukKiraBedeli ?? 0;
                break;

            case AracSahiplikTipi.Komisyon:
                atama.AtamaTipi = AracAtamaTipi.KiralikDisSofor;
                atama.SoforId = null;
                atama.TedarikciCariId ??= arac.KomisyoncuCariId;

                if (!atama.TedarikciCariId.HasValue)
                    throw new InvalidOperationException("Komisyon araç atamalarında komisyoncu cari tanımlı olmalıdır.");

                atama.DisAracPlaka ??= arac.AktifPlaka ?? arac.SaseNo;

                if (string.IsNullOrWhiteSpace(atama.DisSoforAdSoyad))
                    atama.DisSoforAdSoyad = "Komisyon Şoförü";

                break;
        }
    }

    #endregion

    #region Araç Alım/Satım

    public async Task<List<AracAlimSatim>> GetAracAlimSatimlarAsync()
    {
        return await _context.AracAlimSatimlar
            .Include(a => a.Arac)
            .Include(a => a.KarsiTarafCari)
            .Include(a => a.Fatura)
            .OrderByDescending(a => a.IslemTarihi)
            .ToListAsync();
    }

    public async Task<List<AracAlimSatim>> GetAracAlimSatimlarAsync(int aracId)
    {
        return await _context.AracAlimSatimlar
            .Include(a => a.KarsiTarafCari)
            .Include(a => a.Fatura)
            .Where(a => a.AracId == aracId)
            .OrderByDescending(a => a.IslemTarihi)
            .ToListAsync();
    }

    public async Task<AracAlimSatim?> GetAracAlimSatimAsync(int id)
    {
        return await _context.AracAlimSatimlar
            .Include(a => a.Arac)
            .Include(a => a.KarsiTarafCari)
            .Include(a => a.Fatura)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<AracAlimSatim> CreateAracAlimSatimAsync(AracAlimSatim alimSatim)
    {
        alimSatim.CreatedAt = DateTime.UtcNow;
        _context.AracAlimSatimlar.Add(alimSatim);
        await _context.SaveChangesAsync();
        
        // Araç satıldıysa durumu güncelle
        if (alimSatim.IslemTipi == AracIslemTipiDetay.Satis && alimSatim.OdemeDurum == AracIslemOdemeDurum.TamOdendi)
        {
            var arac = await _context.Araclar.FindAsync(alimSatim.AracId);
            if (arac != null)
            {
                arac.Aktif = false;
                arac.SatisaAcik = false;
                arac.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
        
        return alimSatim;
    }

    public async Task<AracAlimSatim> UpdateAracAlimSatimAsync(AracAlimSatim alimSatim)
    {
        var existing = await _context.AracAlimSatimlar.FindAsync(alimSatim.Id);
        if (existing == null)
            throw new InvalidOperationException("Araç alım/satım kaydı bulunamadı.");

        existing.IslemTipi = alimSatim.IslemTipi;
        existing.KarsiTarafCariId = alimSatim.KarsiTarafCariId;
        existing.KarsiTarafAdSoyad = alimSatim.KarsiTarafAdSoyad;
        existing.KarsiTarafTcKimlik = alimSatim.KarsiTarafTcKimlik;
        existing.KarsiTarafTelefon = alimSatim.KarsiTarafTelefon;
        existing.IslemTarihi = alimSatim.IslemTarihi;
        existing.IslemTutari = alimSatim.IslemTutari;
        existing.KDVTutari = alimSatim.KDVTutari;
        existing.ToplamTutar = alimSatim.ToplamTutar;
        existing.NoterAdi = alimSatim.NoterAdi;
        existing.NoterTarihi = alimSatim.NoterTarihi;
        existing.NoterYevmiyeNo = alimSatim.NoterYevmiyeNo;
        existing.NoterIslemTamam = alimSatim.NoterIslemTamam;
        existing.FaturaId = alimSatim.FaturaId;
        existing.FaturaKesildi = alimSatim.FaturaKesildi;
        existing.FaturaKesimTarihi = alimSatim.FaturaKesimTarihi;
        existing.FaturaUyumu = alimSatim.FaturaUyumu;
        existing.FaturaUyumsuzlukAciklama = alimSatim.FaturaUyumsuzlukAciklama;
        existing.OdemeDurum = alimSatim.OdemeDurum;
        existing.OdemeTarihi = alimSatim.OdemeTarihi;
        existing.OdenenTutar = alimSatim.OdenenTutar;
        existing.Notlar = alimSatim.Notlar;
        existing.RuhsatTeslimAlindi = alimSatim.RuhsatTeslimAlindi;
        existing.SigortaTeslimAlindi = alimSatim.SigortaTeslimAlindi;
        existing.MuayeneBelgesiTeslimAlindi = alimSatim.MuayeneBelgesiTeslimAlindi;
        existing.AnahtarTeslimAlindi = alimSatim.AnahtarTeslimAlindi;
        existing.YedekAnahtarTeslimAlindi = alimSatim.YedekAnahtarTeslimAlindi;
        existing.ServisBakimDefteri = alimSatim.ServisBakimDefteri;
        existing.EksikBelgeler = alimSatim.EksikBelgeler;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAracAlimSatimAsync(int id)
    {
        var alimSatim = await _context.AracAlimSatimlar.FindAsync(id);
        if (alimSatim != null)
        {
            alimSatim.IsDeleted = true;
            alimSatim.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<AracAlimSatim>> GetFaturaKontrolBekleyenlerAsync()
    {
        return await _context.AracAlimSatimlar
            .Include(a => a.Arac)
            .Include(a => a.KarsiTarafCari)
            .Include(a => a.Fatura)
            .Where(a => a.NoterIslemTamam && !a.FaturaKesildi)
            .OrderBy(a => a.NoterTarihi)
            .ToListAsync();
    }

    #endregion

    #region Plaka Dönüşüm

    public async Task<List<PlakaDonusum>> GetPlakaDonusumlerAsync()
    {
        return await _context.PlakaDonusumler
            .Include(p => p.Arac)
            .Include(p => p.PlakaSatisCarisi)
            .OrderByDescending(p => p.BasvuruTarihi)
            .ToListAsync();
    }

    public async Task<PlakaDonusum?> GetPlakaDonusumAsync(int id)
    {
        return await _context.PlakaDonusumler
            .Include(p => p.Arac)
            .Include(p => p.PlakaSatisCarisi)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PlakaDonusum> CreatePlakaDonusumAsync(PlakaDonusum donusum)
    {
        donusum.CreatedAt = DateTime.UtcNow;
        _context.PlakaDonusumler.Add(donusum);
        await _context.SaveChangesAsync();
        return donusum;
    }

    public async Task<PlakaDonusum> UpdatePlakaDonusumAsync(PlakaDonusum donusum)
    {
        var existing = await _context.PlakaDonusumler.FindAsync(donusum.Id);
        if (existing == null)
            throw new InvalidOperationException("Plaka dönüşüm kaydı bulunamadı.");

        existing.YeniPlaka = donusum.YeniPlaka;
        existing.YeniPlakaTipi = donusum.YeniPlakaTipi;
        existing.Durum = donusum.Durum;
        existing.OnayTarihi = donusum.OnayTarihi;
        existing.TamamlanmaTarihi = donusum.TamamlanmaTarihi;
        existing.PlakaBedeliMasrafi = donusum.PlakaBedeliMasrafi;
        existing.EmnivetHarci = donusum.EmnivetHarci;
        existing.NoterMasrafi = donusum.NoterMasrafi;
        existing.DigerMasraflar = donusum.DigerMasraflar;
        existing.PlakaSatilacakMi = donusum.PlakaSatilacakMi;
        existing.PlakaSatisBedeli = donusum.PlakaSatisBedeli;
        existing.PlakaSatisCarisiId = donusum.PlakaSatisCarisiId;
        existing.PlakaSatildi = donusum.PlakaSatildi;
        existing.PlakaSatisTarihi = donusum.PlakaSatisTarihi;
        existing.Notlar = donusum.Notlar;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeletePlakaDonusumAsync(int id)
    {
        var donusum = await _context.PlakaDonusumler.FindAsync(id);
        if (donusum != null)
        {
            donusum.IsDeleted = true;
            donusum.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Operasyon Durum

    public async Task<List<AracOperasyonDurum>> GetAracOperasyonDurumlariAsync(int yil, int ay)
    {
        return await _context.AracOperasyonDurumlari
            .Include(a => a.Arac)
            .Where(a => a.Yil == yil && a.Ay == ay)
            .OrderBy(a => a.Arac.AktifPlaka)
            .ToListAsync();
    }

    public async Task<AracOperasyonDurum?> GetAracOperasyonDurumAsync(int aracId, int yil, int ay)
    {
        return await _context.AracOperasyonDurumlari
            .Include(a => a.Arac)
            .FirstOrDefaultAsync(a => a.AracId == aracId && a.Yil == yil && a.Ay == ay);
    }

    public async Task<AracOperasyonDurum> CreateOrUpdateOperasyonDurumAsync(AracOperasyonDurum durum)
    {
        var existing = await _context.AracOperasyonDurumlari
            .FirstOrDefaultAsync(a => a.AracId == durum.AracId && a.Yil == durum.Yil && a.Ay == durum.Ay);

        if (existing == null)
        {
            durum.CreatedAt = DateTime.UtcNow;
            _context.AracOperasyonDurumlari.Add(durum);
        }
        else
        {
            existing.OperasyonTipi = durum.OperasyonTipi;
            existing.ToplamCalismaGunu = durum.ToplamCalismaGunu;
            existing.ToplamSeferSayisi = durum.ToplamSeferSayisi;
            existing.ToplamKm = durum.ToplamKm;
            existing.BrutGelir = durum.BrutGelir;
            existing.KomisyonKesintisi = durum.KomisyonKesintisi;
            existing.YakitGideri = durum.YakitGideri;
            existing.SoforMaliyeti = durum.SoforMaliyeti;
            existing.KiraBedeli = durum.KiraBedeli;
            existing.BakimOnarimGideri = durum.BakimOnarimGideri;
            existing.SigortaGideri = durum.SigortaGideri;
            existing.VergiGideri = durum.VergiGideri;
            existing.OtoyolGideri = durum.OtoyolGideri;
            existing.DigerGiderler = durum.DigerGiderler;
            existing.Notlar = durum.Notlar;
            existing.UpdatedAt = DateTime.UtcNow;
            durum = existing;
        }

        await _context.SaveChangesAsync();
        return durum;
    }

    #endregion

    #region Raporlar

    public async Task<KomisyonculukKazancRaporu> GetKomisyonculukKazancRaporuAsync(DateTime baslangic, DateTime bitis)
    {
        var isler = await _context.KomisyonculukIsler
            .Include(k => k.MusteriCari)
            .Include(k => k.Atamalar)
            .Where(k => k.BaslangicTarihi >= baslangic && k.BaslangicTarihi <= bitis)
            .ToListAsync();

        var rapor = new KomisyonculukKazancRaporu
        {
            BaslangicTarihi = baslangic,
            BitisTarihi = bitis,
            ToplamIsSayisi = isler.Count,
            AktifIsSayisi = isler.Count(i => i.Durum == KomisyonculukIsDurum.Aktif),
            TamamlananIsSayisi = isler.Count(i => i.Durum == KomisyonculukIsDurum.Tamamlandi)
        };

        foreach (var iS in isler)
        {
            rapor.ToplamAlinanIsBedeli += iS.ToplamTutar;
            rapor.ToplamMaliyet += iS.Atamalar.Sum(a => a.ToplamMaliyet);
        }

        rapor.BrutKar = rapor.ToplamAlinanIsBedeli - rapor.ToplamMaliyet;
        rapor.KarMarji = rapor.ToplamAlinanIsBedeli > 0 
            ? (rapor.BrutKar / rapor.ToplamAlinanIsBedeli) * 100 
            : 0;

        rapor.Detaylar = isler.Select(i => new KomisyonculukIsDetay
        {
            IsKodu = i.IsKodu,
            MusteriAdi = i.MusteriCari?.Unvan ?? "-",
            BaslangicTarihi = i.BaslangicTarihi,
            BitisTarihi = i.BitisTarihi,
            AlinanIsBedeli = i.ToplamTutar,
            ToplamMaliyet = i.Atamalar.Sum(a => a.ToplamMaliyet),
            Kar = i.ToplamTutar - i.Atamalar.Sum(a => a.ToplamMaliyet),
            Durum = i.Durum
        }).ToList();

        return rapor;
    }

    public async Task<List<AracKarZararRaporu>> GetAracKarZararRaporuAsync(int yil, int ay)
    {
        var durumlar = await _context.AracOperasyonDurumlari
            .Include(a => a.Arac)
            .Where(a => a.Yil == yil && a.Ay == ay)
            .ToListAsync();

        return durumlar.Select(d => new AracKarZararRaporu
        {
            AracId = d.AracId,
            Plaka = d.Arac?.AktifPlaka ?? "-",
            Marka = d.Arac?.Marka ?? "-",
            Model = d.Arac?.Model ?? "-",
            OperasyonTipi = d.OperasyonTipi,
            ToplamCalismaGunu = d.ToplamCalismaGunu,
            ToplamSeferSayisi = d.ToplamSeferSayisi,
            BrutGelir = d.BrutGelir,
            ToplamGider = d.ToplamGider,
            NetKarZarar = d.NetKarZarar
        }).ToList();
    }

    public async Task<FiloOzetRaporu> GetFiloOzetRaporuAsync(DateTime? tarih = null)
    {
        var bugun = tarih ?? DateTime.Today;
        var yil = bugun.Year;
        var ay = bugun.Month;

        var araclar = await _context.Araclar
            .Where(a => a.Aktif)
            .ToListAsync();

        var komisyonculukIsler = await _context.KomisyonculukIsler
            .Where(k => k.Durum == KomisyonculukIsDurum.Aktif)
            .ToListAsync();

        var satisaBekleyenler = await _context.AracAlimSatimlar
            .Where(a => a.IslemTipi == AracIslemTipiDetay.Satis && !a.NoterIslemTamam)
            .CountAsync();

        var plakaDonusumleri = await _context.PlakaDonusumler
            .Where(p => p.Durum != PlakaDonusumDurum.Tamamlandi && p.Durum != PlakaDonusumDurum.IptalEdildi)
            .CountAsync();

        return new FiloOzetRaporu
        {
            Tarih = bugun,
            ToplamAracSayisi = araclar.Count,
            OzmalAracSayisi = araclar.Count(a => a.SahiplikTipi == AracSahiplikTipi.Ozmal),
            KiralikAracSayisi = araclar.Count(a => a.SahiplikTipi == AracSahiplikTipi.Kiralik),
            KomisyonAracSayisi = araclar.Count(a => a.SahiplikTipi == AracSahiplikTipi.Komisyon),
            SatisaBekleyenAracSayisi = araclar.Count(a => a.SatisaAcik),
            AktifKomisyonculukIsSayisi = komisyonculukIsler.Count,
            NoterBekleyenSatisSayisi = satisaBekleyenler,
            DevamEdenPlakaDonusumSayisi = plakaDonusumleri
        };
    }

    #endregion
}

#region Rapor Modelleri

public class KomisyonculukKazancRaporu
{
    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }
    public int ToplamIsSayisi { get; set; }
    public int AktifIsSayisi { get; set; }
    public int TamamlananIsSayisi { get; set; }
    public decimal ToplamAlinanIsBedeli { get; set; }
    public decimal ToplamMaliyet { get; set; }
    public decimal BrutKar { get; set; }
    public decimal KarMarji { get; set; }
    public List<KomisyonculukIsDetay> Detaylar { get; set; } = new();
}

public class KomisyonculukIsDetay
{
    public string IsKodu { get; set; } = string.Empty;
    public string MusteriAdi { get; set; } = string.Empty;
    public DateTime BaslangicTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }
    public decimal AlinanIsBedeli { get; set; }
    public decimal ToplamMaliyet { get; set; }
    public decimal Kar { get; set; }
    public KomisyonculukIsDurum Durum { get; set; }
}

public class AracKarZararRaporu
{
    public int AracId { get; set; }
    public string Plaka { get; set; } = string.Empty;
    public string Marka { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public AracOperasyonTipi OperasyonTipi { get; set; }
    public int ToplamCalismaGunu { get; set; }
    public int ToplamSeferSayisi { get; set; }
    public decimal BrutGelir { get; set; }
    public decimal ToplamGider { get; set; }
    public decimal NetKarZarar { get; set; }
}

public class FiloOzetRaporu
{
    public DateTime Tarih { get; set; }
    public int ToplamAracSayisi { get; set; }
    public int OzmalAracSayisi { get; set; }
    public int KiralikAracSayisi { get; set; }
    public int KomisyonAracSayisi { get; set; }
    public int SatisaBekleyenAracSayisi { get; set; }
    public int AktifKomisyonculukIsSayisi { get; set; }
    public int NoterBekleyenSatisSayisi { get; set; }
    public int DevamEdenPlakaDonusumSayisi { get; set; }
}

#endregion
