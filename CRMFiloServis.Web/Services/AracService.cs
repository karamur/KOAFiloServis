using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class AracService : IAracService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public AracService(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    #region Araç CRUD Ýþlemleri

    public async Task<List<Arac>> GetAllAsync()
    {
        var araclar = await _context.Araclar
            .Include(a => a.PlakaGecmisi.Where(p => !p.IsDeleted))
            .Where(a => !a.IsDeleted)
            .ToListAsync();
            
        // Aktif plakalarý güncelle (CikisTarihi null veya gelecek tarihli olanlar)
        foreach (var arac in araclar)
        {
            var aktifPlaka = arac.PlakaGecmisi
                .Where(p => p.CikisTarihi == null || p.CikisTarihi > DateTime.Today)
                .OrderByDescending(p => p.GirisTarihi)
                .FirstOrDefault();
            
            if (aktifPlaka != null && arac.AktifPlaka != aktifPlaka.Plaka)
            {
                arac.AktifPlaka = aktifPlaka.Plaka;
            }
        }
        
        return araclar.OrderBy(a => a.AktifPlaka ?? a.SaseNo).ToList();
    }

    public async Task<List<Arac>> GetActiveAsync()
    {
        var araclar = await _context.Araclar
            .Include(a => a.PlakaGecmisi.Where(p => !p.IsDeleted))
            .Where(a => a.Aktif && !a.IsDeleted)
            .ToListAsync();
            
        // Aktif plakalarý güncelle
        foreach (var arac in araclar)
        {
            var aktifPlaka = arac.PlakaGecmisi
                .Where(p => p.CikisTarihi == null || p.CikisTarihi > DateTime.Today)
                .OrderByDescending(p => p.GirisTarihi)
                .FirstOrDefault();
            
            if (aktifPlaka != null && arac.AktifPlaka != aktifPlaka.Plaka)
            {
                arac.AktifPlaka = aktifPlaka.Plaka;
            }
        }
        
        return araclar.OrderBy(a => a.AktifPlaka ?? a.SaseNo).ToList();
    }

    public async Task<int> GetActiveCountAsync()
    {
        return await _context.Araclar
            .Where(a => a.Aktif && !a.IsDeleted)
            .CountAsync();
    }

    public async Task<Arac?> GetByIdAsync(int id)
    {
        var arac = await _context.Araclar
            .Include(a => a.PlakaGecmisi.Where(p => !p.IsDeleted).OrderByDescending(p => p.GirisTarihi))
            .Include(a => a.KiralikCari)
            .Include(a => a.KomisyoncuCari)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
            
        if (arac != null)
        {
            // Aktif plakayý güncelle
            var aktifPlaka = arac.PlakaGecmisi
                .Where(p => p.CikisTarihi == null || p.CikisTarihi > DateTime.Today)
                .OrderByDescending(p => p.GirisTarihi)
                .FirstOrDefault();
            
            if (aktifPlaka != null)
            {
                arac.AktifPlaka = aktifPlaka.Plaka;
            }
        }
        
        return arac;
    }

    public async Task<Arac?> GetByPlakaAsync(string plaka)
    {
        // Aktif plakaya göre bul (CikisTarihi null veya gelecek tarihli)
        var aracPlaka = await _context.AracPlakalar
            .Include(ap => ap.Arac)
            .FirstOrDefaultAsync(ap => ap.Plaka == plaka && 
                                       !ap.IsDeleted &&
                                       (ap.CikisTarihi == null || ap.CikisTarihi > DateTime.Today));
            
        return aracPlaka?.Arac;
    }
    
    public async Task<Arac?> GetBySaseNoAsync(string saseNo)
    {
        return await _context.Araclar
            .Include(a => a.PlakaGecmisi.Where(p => !p.IsDeleted))
            .FirstOrDefaultAsync(a => a.SaseNo == saseNo && !a.IsDeleted);
    }
    
    public async Task<bool> SaseNoMevcutMu(string saseNo, int? haricAracId = null)
    {
        return await _context.Araclar
            .AnyAsync(a => a.SaseNo == saseNo && 
                          !a.IsDeleted &&
                          (!haricAracId.HasValue || a.Id != haricAracId.Value));
    }
    
    public async Task<bool> PlakaMevcutMu(string plaka, int? haricAracPlakaId = null)
    {
        // Aktif plaka kontrolü (CikisTarihi null veya gelecek tarihli)
        return await _context.AracPlakalar
            .AnyAsync(ap => ap.Plaka == plaka && 
                           !ap.IsDeleted &&
                           (ap.CikisTarihi == null || ap.CikisTarihi > DateTime.Today) && 
                           (!haricAracPlakaId.HasValue || ap.Id != haricAracPlakaId.Value));
    }

    public async Task<Arac> CreateAsync(Arac arac, string plaka, PlakaIslemTipi islemTipi = PlakaIslemTipi.Alis, 
        decimal? islemTutari = null, int? cariId = null, string? aciklama = null)
    {
        // Þase no kontrolü
        if (await SaseNoMevcutMu(arac.SaseNo))
            throw new InvalidOperationException($"Bu þase numarasý ({arac.SaseNo}) sistemde zaten kayýtlý.");
            
        // Plaka kontrolü
        if (await PlakaMevcutMu(plaka))
            throw new InvalidOperationException($"Bu plaka ({plaka}) baþka bir araçta aktif olarak kullanýlýyor.");
        
        // Araç oluþtur
        arac.AktifPlaka = plaka;
        arac.CreatedAt = DateTime.UtcNow;
        _context.Araclar.Add(arac);
        await _context.SaveChangesAsync();
        
        // Ýlk plaka kaydýný oluþtur
        var aracPlaka = new AracPlaka
        {
            AracId = arac.Id,
            Plaka = plaka,
            GirisTarihi = DateTime.UtcNow,
            IslemTipi = islemTipi,
            IslemTutari = islemTutari,
            CariId = cariId,
            Aciklama = aciklama ?? $"Araç ilk kayýt - {islemTipi}",
            CreatedAt = DateTime.UtcNow
        };
        _context.AracPlakalar.Add(aracPlaka);
        await _context.SaveChangesAsync();
        
        return arac;
    }

    public async Task<Arac> UpdateAsync(Arac arac)
    {
        // Þase no kontrolü (kendi hariç)
        if (await SaseNoMevcutMu(arac.SaseNo, arac.Id))
            throw new InvalidOperationException($"Bu þase numarasý ({arac.SaseNo}) sistemde zaten kayýtlý.");
            
        arac.UpdatedAt = DateTime.UtcNow;
        _context.Araclar.Update(arac);
        await _context.SaveChangesAsync();
        
        // Aktif plakayý güncelle
        await GuncelleAktifPlaka(arac.Id);
        
        return arac;
    }

    public async Task DeleteAsync(int id)
    {
        var arac = await _context.Araclar.FindAsync(id);
        if (arac != null)
        {
            arac.IsDeleted = true;
            arac.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
    
    #endregion
    
    #region Plaka Ýþlemleri
    
    public async Task<List<AracPlaka>> GetPlakaGecmisiAsync(int aracId)
    {
        return await _context.AracPlakalar
            .Include(ap => ap.Cari)
            .Where(ap => ap.AracId == aracId)
            .OrderByDescending(ap => ap.GirisTarihi)
            .ToListAsync();
    }
    
    public async Task<AracPlaka> PlakaEkle(int aracId, string yeniPlaka, PlakaIslemTipi islemTipi, 
        decimal? islemTutari = null, int? cariId = null, string? aciklama = null)
    {
        // Plaka kontrolü
        if (await PlakaMevcutMu(yeniPlaka))
            throw new InvalidOperationException($"Bu plaka ({yeniPlaka}) baþka bir araçta aktif olarak kullanýlýyor.");
        
        // Mevcut aktif plakayý kapat
        var mevcutAktif = await _context.AracPlakalar
            .FirstOrDefaultAsync(ap => ap.AracId == aracId && ap.CikisTarihi == null);
            
        if (mevcutAktif != null)
        {
            mevcutAktif.CikisTarihi = DateTime.UtcNow;
            mevcutAktif.UpdatedAt = DateTime.UtcNow;
        }
        
        // Yeni plaka ekle
        var yeniPlakaKaydi = new AracPlaka
        {
            AracId = aracId,
            Plaka = yeniPlaka,
            GirisTarihi = DateTime.UtcNow,
            IslemTipi = islemTipi,
            IslemTutari = islemTutari,
            CariId = cariId,
            Aciklama = aciklama,
            CreatedAt = DateTime.UtcNow
        };
        _context.AracPlakalar.Add(yeniPlakaKaydi);
        
        // Araçtaki aktif plakayý güncelle
        var arac = await _context.Araclar.FindAsync(aracId);
        if (arac != null)
        {
            arac.AktifPlaka = yeniPlaka;
            arac.UpdatedAt = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync();
        return yeniPlakaKaydi;
    }
    
    public async Task PlakaCikis(int aracPlakaId, PlakaIslemTipi cikisIslemTipi, 
        decimal? islemTutari = null, int? cariId = null, string? aciklama = null)
    {
        var plakaKaydi = await _context.AracPlakalar
            .Include(ap => ap.Arac)
            .FirstOrDefaultAsync(ap => ap.Id == aracPlakaId);
            
        if (plakaKaydi == null)
            throw new InvalidOperationException("Plaka kaydý bulunamadý.");
            
        if (plakaKaydi.CikisTarihi.HasValue)
            throw new InvalidOperationException("Bu plaka zaten kapatýlmýþ.");
        
        plakaKaydi.CikisTarihi = DateTime.UtcNow;
        plakaKaydi.IslemTipi = cikisIslemTipi;
        if (islemTutari.HasValue) plakaKaydi.IslemTutari = islemTutari;
        if (cariId.HasValue) plakaKaydi.CariId = cariId;
        if (!string.IsNullOrEmpty(aciklama)) plakaKaydi.Aciklama = aciklama;
        plakaKaydi.UpdatedAt = DateTime.UtcNow;
        
        // Araçtaki aktif plakayý temizle
        if (plakaKaydi.Arac != null)
        {
            plakaKaydi.Arac.AktifPlaka = null;
            plakaKaydi.Arac.UpdatedAt = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync();
    }
    
    private async Task GuncelleAktifPlaka(int aracId)
    {
        var arac = await _context.Araclar.FindAsync(aracId);
        if (arac == null) return;
        
        // CikisTarihi null olan veya CikisTarihi bugünden sonra olan plakalardan en son eklenen
        var aktifPlaka = await _context.AracPlakalar
            .Where(ap => ap.AracId == aracId && 
                        !ap.IsDeleted &&
                        (ap.CikisTarihi == null || ap.CikisTarihi > DateTime.Today))
            .OrderByDescending(ap => ap.GirisTarihi)
            .FirstOrDefaultAsync();
            
        arac.AktifPlaka = aktifPlaka?.Plaka;
        await _context.SaveChangesAsync();
    }
    
    #endregion
    
    #region Satýþa Açýk Araçlar
    
    public async Task<List<Arac>> GetSatisaAcikAraclarAsync()
    {
        return await _context.Araclar
            .Include(a => a.PlakaGecmisi.Where(p => !p.IsDeleted))
            .Where(a => a.SatisaAcik && a.Aktif)
            .OrderBy(a => a.SatisaAcilmaTarihi)
            .ToListAsync();
    }
    
    public async Task AracSatisaAc(int aracId, decimal satisFiyati, string? aciklama = null)
    {
        var arac = await _context.Araclar.FindAsync(aracId);
        if (arac == null)
            throw new InvalidOperationException("Araç bulunamadý.");
            
        arac.SatisaAcik = true;
        arac.SatisFiyati = satisFiyati;
        arac.SatisaAcilmaTarihi = DateTime.UtcNow;
        arac.SatisAciklamasi = aciklama;
        arac.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
    }
    
    public async Task AracSatisKapat(int aracId)
    {
        var arac = await _context.Araclar.FindAsync(aracId);
        if (arac == null)
            throw new InvalidOperationException("Araç bulunamadý.");
            
        arac.SatisaAcik = false;
        arac.SatisFiyati = null;
        arac.SatisaAcilmaTarihi = null;
        arac.SatisAciklamasi = null;
        arac.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
    }
    
    #endregion

    #region Arac Evrak Islemleri

    public async Task<List<AracEvrak>> GetAracEvraklariAsync(int aracId)
    {
        return await _context.AracEvraklari
            .Include(e => e.Dosyalar)
            .Where(e => e.AracId == aracId)
            .OrderBy(e => e.EvrakKategorisi)
            .ThenByDescending(e => e.BitisTarihi)
            .ToListAsync();
    }

    public async Task<AracEvrak?> GetAracEvrakByIdAsync(int evrakId)
    {
        return await _context.AracEvraklari
            .Include(e => e.Dosyalar)
            .FirstOrDefaultAsync(e => e.Id == evrakId);
    }

    public async Task<AracEvrak> CreateAracEvrakAsync(AracEvrak evrak)
    {
        if (evrak.BaslangicTarihi.HasValue)
            evrak.BaslangicTarihi = DateTime.SpecifyKind(evrak.BaslangicTarihi.Value, DateTimeKind.Utc);
        if (evrak.BitisTarihi.HasValue)
            evrak.BitisTarihi = DateTime.SpecifyKind(evrak.BitisTarihi.Value, DateTimeKind.Utc);
        
        evrak.CreatedAt = DateTime.UtcNow;
        _context.AracEvraklari.Add(evrak);
        await _context.SaveChangesAsync();
        return evrak;
    }

    public async Task<AracEvrak> UpdateAracEvrakAsync(AracEvrak evrak)
    {
        if (evrak.BaslangicTarihi.HasValue)
            evrak.BaslangicTarihi = DateTime.SpecifyKind(evrak.BaslangicTarihi.Value, DateTimeKind.Utc);
        if (evrak.BitisTarihi.HasValue)
            evrak.BitisTarihi = DateTime.SpecifyKind(evrak.BitisTarihi.Value, DateTimeKind.Utc);
        
        evrak.UpdatedAt = DateTime.UtcNow;
        _context.AracEvraklari.Update(evrak);
        await _context.SaveChangesAsync();
        return evrak;
    }

    public async Task DeleteAracEvrakAsync(int evrakId)
    {
        var evrak = await _context.AracEvraklari
            .Include(e => e.Dosyalar)
            .FirstOrDefaultAsync(e => e.Id == evrakId);
            
        if (evrak != null)
        {
            // Dosyalarý sil
            foreach (var dosya in evrak.Dosyalar)
            {
                var dosyaYolu = Path.Combine(_env.ContentRootPath, "wwwroot", dosya.DosyaYolu);
                if (File.Exists(dosyaYolu))
                    File.Delete(dosyaYolu);
            }
            
            evrak.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<AracEvrakDosya> UploadEvrakDosyaAsync(int evrakId, IBrowserFile file)
    {
        var evrak = await _context.AracEvraklari.FindAsync(evrakId);
        if (evrak == null)
            throw new Exception("Evrak bulunamadi");

        var klasorYolu = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads", "evraklar", evrakId.ToString());
        if (!Directory.Exists(klasorYolu))
            Directory.CreateDirectory(klasorYolu);

        var dosyaAdi = $"{Guid.NewGuid()}{Path.GetExtension(file.Name)}";
        var dosyaYolu = Path.Combine(klasorYolu, dosyaAdi);

        await using var stream = new FileStream(dosyaYolu, FileMode.Create);
        await file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024).CopyToAsync(stream);

        var evrakDosya = new AracEvrakDosya
        {
            AracEvrakId = evrakId,
            DosyaAdi = file.Name,
            DosyaYolu = $"uploads/evraklar/{evrakId}/{dosyaAdi}",
            DosyaTipi = Path.GetExtension(file.Name).TrimStart('.').ToLower(),
            DosyaBoyutu = file.Size,
            CreatedAt = DateTime.UtcNow
        };

        _context.AracEvrakDosyalari.Add(evrakDosya);
        await _context.SaveChangesAsync();
        return evrakDosya;
    }

    public async Task<byte[]> GetEvrakDosyaAsync(int dosyaId)
    {
        var dosya = await _context.AracEvrakDosyalari.FindAsync(dosyaId);
        if (dosya == null)
            throw new Exception("Dosya bulunamadi");

        var dosyaYolu = Path.Combine(_env.ContentRootPath, "wwwroot", dosya.DosyaYolu);
        if (!File.Exists(dosyaYolu))
            throw new Exception("Dosya diskte bulunamadi");

        return await File.ReadAllBytesAsync(dosyaYolu);
    }

    public async Task DeleteEvrakDosyaAsync(int dosyaId)
    {
        var dosya = await _context.AracEvrakDosyalari.FindAsync(dosyaId);
        if (dosya != null)
        {
            var dosyaYolu = Path.Combine(_env.ContentRootPath, "wwwroot", dosya.DosyaYolu);
            if (File.Exists(dosyaYolu))
                File.Delete(dosyaYolu);

            _context.AracEvrakDosyalari.Remove(dosya);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Evrak Uyarilari

    public async Task<List<AracEvrak>> GetSuresiDolacakEvraklarAsync(int gunSayisi = 30)
    {
        var bugun = DateTime.UtcNow.Date;
        var bitisTarihi = bugun.AddDays(gunSayisi);

        return await _context.AracEvraklari
            .Include(e => e.Arac)
            .Where(e => e.Durum == EvrakDurum.Aktif && 
                        e.BitisTarihi.HasValue && 
                        e.BitisTarihi.Value <= bitisTarihi)
            .OrderBy(e => e.BitisTarihi)
            .ToListAsync();
    }

    #endregion
}
