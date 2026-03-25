using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

// Müþteri kiralama iþlemleri için servis interface'i
// CRUD operasyonlarý + özel iþ mantýðý metodlarý
public interface IMusteriKiralamaService
{
    // Tüm kiralamalarý getir
    Task<List<MusteriKiralama>> GetAllAsync();

    // ID'ye göre kiralama getir
    Task<MusteriKiralama?> GetByIdAsync(int id);

    // Aktif kiralamalarý getir
    Task<List<MusteriKiralama>> GetAktifKiralamalarAsync();

    // Müþteriye göre kiralamalarý getir
    Task<List<MusteriKiralama>> GetByMusteriIdAsync(int musteriId);

    // Araca göre kiralamalarý getir
    Task<List<MusteriKiralama>> GetByAracIdAsync(int aracId);

    // Yeni kiralama oluþtur (tarih çakýþmasý kontrolü ile)
    Task<MusteriKiralama> CreateAsync(MusteriKiralama kiralama);

    // Kiralama güncelle
    Task<MusteriKiralama> UpdateAsync(MusteriKiralama kiralama);

    // Kiralama iptal et
    Task<bool> IptalEtAsync(int id, string? iptalNedeni = null);

    // Araç teslim al (kiralama baþlat)
    Task<MusteriKiralama> TeslimAlAsync(int kiralamaId, int baslangicKm, int personelId);

    // Araç teslim et (kiralama bitir)
    Task<MusteriKiralama> TeslimEtAsync(int kiralamaId, int bitisKm, int personelId);

    // Belirli tarih aralýðýnda araç müsait mi kontrol et
    Task<bool> AracMusaitMiAsync(int aracId, DateTime baslangic, DateTime bitis, int? haricKiralamaId = null);

    // Toplam tutarý hesapla
    decimal ToplamTutarHesapla(DateTime baslangic, DateTime bitis, decimal gunlukFiyat);
}

// Müþteri kiralama servisi implementasyonu
public class MusteriKiralamaService : IMusteriKiralamaService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MusteriKiralamaService> _logger;

    public MusteriKiralamaService(ApplicationDbContext context, ILogger<MusteriKiralamaService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Tüm kiralamalarý getir, silinmemiþ olanlar, tarihe göre sýralý
    public async Task<List<MusteriKiralama>> GetAllAsync()
    {
        return await _context.MusteriKiralamalar
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.BaslangicTarihi)
            .ToListAsync();
    }

    // ID'ye göre kiralama getir
    public async Task<MusteriKiralama?> GetByIdAsync(int id)
    {
        return await _context.MusteriKiralamalar
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
    }

    // Sadece aktif durumda olan kiralamalarý getir
    public async Task<List<MusteriKiralama>> GetAktifKiralamalarAsync()
    {
        return await _context.MusteriKiralamalar
            .Where(x => !x.IsDeleted && x.Durum == KiralamaDurumu.Aktif)
            .OrderBy(x => x.PlanlananBitisTarihi)
            .ToListAsync();
    }

    // Müþteriye ait tüm kiralamalarý getir
    public async Task<List<MusteriKiralama>> GetByMusteriIdAsync(int musteriId)
    {
        return await _context.MusteriKiralamalar
            .Where(x => !x.IsDeleted && x.MusteriId == musteriId)
            .OrderByDescending(x => x.BaslangicTarihi)
            .ToListAsync();
    }

    // Araca ait tüm kiralamalarý getir
    public async Task<List<MusteriKiralama>> GetByAracIdAsync(int aracId)
    {
        return await _context.MusteriKiralamalar
            .Where(x => !x.IsDeleted && x.AracId == aracId)
            .OrderByDescending(x => x.BaslangicTarihi)
            .ToListAsync();
    }

    // Yeni kiralama oluþtur, önce araç müsaitliðini kontrol et
    public async Task<MusteriKiralama> CreateAsync(MusteriKiralama kiralama)
    {
        // Araç müsait mi kontrol et
        var musaitMi = await AracMusaitMiAsync(kiralama.AracId, kiralama.BaslangicTarihi, kiralama.PlanlananBitisTarihi);
        if (!musaitMi)
        {
            throw new InvalidOperationException("Araç seçilen tarihler arasýnda müsait deðil!");
        }

        // Toplam tutarý hesapla
        kiralama.ToplamTutar = ToplamTutarHesapla(kiralama.BaslangicTarihi, kiralama.PlanlananBitisTarihi, kiralama.GunlukFiyat);

        // Sözleþme numarasý oluþtur
        kiralama.SozlesmeNo = $"KR-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";

        kiralama.CreatedAt = DateTime.Now;
        _context.MusteriKiralamalar.Add(kiralama);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Yeni kiralama oluþturuldu: {SozlesmeNo}", kiralama.SozlesmeNo);
        return kiralama;
    }

    // Kiralama güncelle
    public async Task<MusteriKiralama> UpdateAsync(MusteriKiralama kiralama)
    {
        var existing = await GetByIdAsync(kiralama.Id);
        if (existing == null)
        {
            throw new InvalidOperationException("Kiralama bulunamadý!");
        }

        // Tarih deðiþtiyse müsaitlik kontrolü yap
        if (existing.BaslangicTarihi != kiralama.BaslangicTarihi || 
            existing.PlanlananBitisTarihi != kiralama.PlanlananBitisTarihi ||
            existing.AracId != kiralama.AracId)
        {
            var musaitMi = await AracMusaitMiAsync(kiralama.AracId, kiralama.BaslangicTarihi, kiralama.PlanlananBitisTarihi, kiralama.Id);
            if (!musaitMi)
            {
                throw new InvalidOperationException("Araç seçilen tarihler arasýnda müsait deðil!");
            }
        }

        // Toplam tutarý yeniden hesapla
        kiralama.ToplamTutar = ToplamTutarHesapla(kiralama.BaslangicTarihi, kiralama.PlanlananBitisTarihi, kiralama.GunlukFiyat);

        kiralama.UpdatedAt = DateTime.Now;
        _context.MusteriKiralamalar.Update(kiralama);
        await _context.SaveChangesAsync();

        return kiralama;
    }

    // Kiralama iptal et
    public async Task<bool> IptalEtAsync(int id, string? iptalNedeni = null)
    {
        var kiralama = await GetByIdAsync(id);
        if (kiralama == null) return false;

        if (kiralama.Durum == KiralamaDurumu.Tamamlandi)
        {
            throw new InvalidOperationException("Tamamlanmýþ kiralama iptal edilemez!");
        }

        kiralama.Durum = KiralamaDurumu.IptalEdildi;
        kiralama.Notlar = string.IsNullOrEmpty(kiralama.Notlar) 
            ? $"Ýptal nedeni: {iptalNedeni}" 
            : $"{kiralama.Notlar}\nÝptal nedeni: {iptalNedeni}";
        kiralama.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Kiralama iptal edildi: {Id}", id);
        return true;
    }

    // Araç teslim al - kiralama baþlat
    public async Task<MusteriKiralama> TeslimAlAsync(int kiralamaId, int baslangicKm, int personelId)
    {
        var kiralama = await GetByIdAsync(kiralamaId);
        if (kiralama == null)
        {
            throw new InvalidOperationException("Kiralama bulunamadý!");
        }

        if (kiralama.Durum != KiralamaDurumu.Rezervasyon)
        {
            throw new InvalidOperationException("Sadece rezervasyon durumundaki kiralama teslim alýnabilir!");
        }

        kiralama.Durum = KiralamaDurumu.Aktif;
        kiralama.BaslangicKm = baslangicKm;
        kiralama.TeslimEdenPersonelId = personelId;
        kiralama.BaslangicTarihi = DateTime.Now; // Gerçek baþlangýç zamaný
        kiralama.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Araç teslim alýndý: Kiralama {Id}, KM: {Km}", kiralamaId, baslangicKm);
        return kiralama;
    }

    // Araç teslim et - kiralama bitir
    public async Task<MusteriKiralama> TeslimEtAsync(int kiralamaId, int bitisKm, int personelId)
    {
        var kiralama = await GetByIdAsync(kiralamaId);
        if (kiralama == null)
        {
            throw new InvalidOperationException("Kiralama bulunamadý!");
        }

        if (kiralama.Durum != KiralamaDurumu.Aktif)
        {
            throw new InvalidOperationException("Sadece aktif kiralama teslim edilebilir!");
        }

        if (bitisKm < (kiralama.BaslangicKm ?? 0))
        {
            throw new InvalidOperationException("Bitiþ kilometresi baþlangýç kilometresinden küçük olamaz!");
        }

        kiralama.Durum = KiralamaDurumu.Tamamlandi;
        kiralama.GercekBitisTarihi = DateTime.Now;
        kiralama.BitisKm = bitisKm;
        kiralama.TeslimAlanPersonelId = personelId;
        kiralama.UpdatedAt = DateTime.Now;

        // Gerçek süreye göre tutarý yeniden hesapla
        kiralama.ToplamTutar = ToplamTutarHesapla(kiralama.BaslangicTarihi, kiralama.GercekBitisTarihi.Value, kiralama.GunlukFiyat);

        await _context.SaveChangesAsync();
        _logger.LogInformation("Araç teslim edildi: Kiralama {Id}, KM: {Km}, Tutar: {Tutar}", kiralamaId, bitisKm, kiralama.ToplamTutar);
        return kiralama;
    }

    // Araç belirli tarihler arasýnda müsait mi kontrol et
    public async Task<bool> AracMusaitMiAsync(int aracId, DateTime baslangic, DateTime bitis, int? haricKiralamaId = null)
    {
        var query = _context.MusteriKiralamalar
            .Where(x => !x.IsDeleted 
                && x.AracId == aracId 
                && x.Durum != KiralamaDurumu.IptalEdildi
                && x.Durum != KiralamaDurumu.Tamamlandi);

        // Güncelleme durumunda mevcut kaydý hariç tut
        if (haricKiralamaId.HasValue)
        {
            query = query.Where(x => x.Id != haricKiralamaId.Value);
        }

        // Tarih çakýþmasý kontrolü
        var cakisan = await query.AnyAsync(x =>
            (baslangic >= x.BaslangicTarihi && baslangic <= (x.GercekBitisTarihi ?? x.PlanlananBitisTarihi)) ||
            (bitis >= x.BaslangicTarihi && bitis <= (x.GercekBitisTarihi ?? x.PlanlananBitisTarihi)) ||
            (baslangic <= x.BaslangicTarihi && bitis >= (x.GercekBitisTarihi ?? x.PlanlananBitisTarihi)));

        return !cakisan;
    }

    // Toplam tutarý hesapla (gün sayýsý * günlük fiyat)
    public decimal ToplamTutarHesapla(DateTime baslangic, DateTime bitis, decimal gunlukFiyat)
    {
        var gunSayisi = (int)Math.Ceiling((bitis - baslangic).TotalDays);
        if (gunSayisi < 1) gunSayisi = 1; // Minimum 1 gün
        return gunSayisi * gunlukFiyat;
    }
}
