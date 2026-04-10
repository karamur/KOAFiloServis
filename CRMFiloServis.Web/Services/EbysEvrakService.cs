using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

/// <summary>
/// EBYS Gelen/Giden Evrak Yönetimi Servis Implementasyonu
/// </summary>
public class EbysEvrakService : IEbysEvrakService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<EbysEvrakService> _logger;

    public EbysEvrakService(
        ApplicationDbContext context,
        IWebHostEnvironment environment,
        ILogger<EbysEvrakService> logger)
    {
        _context = context;
        _environment = environment;
        _logger = logger;
    }

    #region Evrak CRUD

    public async Task<List<EbysEvrak>> GetEvraklarAsync(EbysEvrakFiltre? filtre = null)
    {
        var query = _context.EbysEvraklar
            .Include(e => e.Kategori)
            .Include(e => e.AtananKullanici)
            .Include(e => e.Dosyalar)
            .AsQueryable();

        if (filtre != null)
        {
            if (!string.IsNullOrWhiteSpace(filtre.AramaMetni))
            {
                var arama = filtre.AramaMetni.ToLower();
                query = query.Where(e =>
                    e.EvrakNo.ToLower().Contains(arama) ||
                    e.Konu.ToLower().Contains(arama) ||
                    (e.GonderenKurum != null && e.GonderenKurum.ToLower().Contains(arama)) ||
                    (e.AliciKurum != null && e.AliciKurum.ToLower().Contains(arama)));
            }

            if (filtre.Yon.HasValue)
                query = query.Where(e => e.Yon == filtre.Yon.Value);

            if (filtre.KategoriId.HasValue)
                query = query.Where(e => e.KategoriId == filtre.KategoriId.Value);

            if (filtre.Durum.HasValue)
                query = query.Where(e => e.Durum == filtre.Durum.Value);

            if (filtre.Oncelik.HasValue)
                query = query.Where(e => e.Oncelik == filtre.Oncelik.Value);

            if (filtre.BaslangicTarihi.HasValue)
                query = query.Where(e => e.EvrakTarihi >= filtre.BaslangicTarihi.Value);

            if (filtre.BitisTarihi.HasValue)
                query = query.Where(e => e.EvrakTarihi <= filtre.BitisTarihi.Value);

            if (filtre.AtananKullaniciId.HasValue)
                query = query.Where(e => e.AtananKullaniciId == filtre.AtananKullaniciId.Value);

            if (filtre.SadeceCevapBekleyenler)
                query = query.Where(e => e.CevapGerekli && e.Durum != EbysEvrakDurum.Cevaplandi && e.Durum != EbysEvrakDurum.Tamamlandi);
        }

        return await query
            .OrderByDescending(e => e.EvrakTarihi)
            .ThenByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<EbysEvrak?> GetEvrakByIdAsync(int id)
    {
        return await _context.EbysEvraklar
            .Include(e => e.Kategori)
            .Include(e => e.AtananKullanici)
            .Include(e => e.UstEvrak)
            .Include(e => e.AltEvraklar)
            .Include(e => e.Dosyalar)
            .Include(e => e.Atamalar)
                .ThenInclude(a => a.AtananKullanici)
            .Include(e => e.Hareketler)
                .ThenInclude(h => h.Kullanici)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<EbysEvrak> CreateEvrakAsync(EbysEvrakOlusturModel model)
    {
        var evrakNo = await YeniEvrakNoOlusturAsync(model.Yon);

        var evrak = new EbysEvrak
        {
            EvrakNo = evrakNo,
            Yon = model.Yon,
            EvrakTarihi = model.EvrakTarihi,
            KayitTarihi = DateTime.Now,
            Konu = model.Konu,
            Ozet = model.Ozet,
            GonderenKurum = model.GonderenKurum,
            AliciKurum = model.AliciKurum,
            GelisNo = model.GelisNo,
            GelisTarihi = model.GelisTarihi,
            GidisNo = model.GidisNo,
            GonderimTarihi = model.GonderimTarihi,
            GonderimYontemi = model.GonderimYontemi,
            KategoriId = model.KategoriId,
            Oncelik = model.Oncelik,
            Gizlilik = model.Gizlilik,
            CevapGerekli = model.CevapGerekli,
            CevapSuresi = model.CevapSuresi,
            UstEvrakId = model.UstEvrakId,
            Aciklama = model.Aciklama,
            Durum = EbysEvrakDurum.Beklemede
        };

        _context.EbysEvraklar.Add(evrak);
        await _context.SaveChangesAsync();

        // Hareket kaydı
        await HareketEkleAsync(evrak.Id, 1, EbysHareketTipi.Olusturuldu, $"Evrak oluşturuldu: {evrakNo}");

        _logger.LogInformation("EBYS Evrak oluşturuldu: {EvrakNo}", evrakNo);
        return evrak;
    }

    public async Task<EbysEvrak> UpdateEvrakAsync(EbysEvrakGuncelleModel model)
    {
        var evrak = await _context.EbysEvraklar.FindAsync(model.Id)
            ?? throw new InvalidOperationException("Evrak bulunamadı");

        evrak.Konu = model.Konu;
        evrak.Ozet = model.Ozet;
        evrak.GonderenKurum = model.GonderenKurum;
        evrak.AliciKurum = model.AliciKurum;
        evrak.GelisNo = model.GelisNo;
        evrak.GelisTarihi = model.GelisTarihi;
        evrak.GidisNo = model.GidisNo;
        evrak.GonderimTarihi = model.GonderimTarihi;
        evrak.GonderimYontemi = model.GonderimYontemi;
        evrak.KategoriId = model.KategoriId;
        evrak.Oncelik = model.Oncelik;
        evrak.Gizlilik = model.Gizlilik;
        evrak.CevapGerekli = model.CevapGerekli;
        evrak.CevapSuresi = model.CevapSuresi;
        evrak.Aciklama = model.Aciklama;
        evrak.Notlar = model.Notlar;
        evrak.UpdatedAt = DateTime.UtcNow;
        evrak.SonIslemTarihi = DateTime.Now;

        await _context.SaveChangesAsync();

        await HareketEkleAsync(evrak.Id, 1, EbysHareketTipi.Guncellendi, "Evrak güncellendi");

        return evrak;
    }

    public async Task DeleteEvrakAsync(int id)
    {
        var evrak = await _context.EbysEvraklar.FindAsync(id)
            ?? throw new InvalidOperationException("Evrak bulunamadı");

        evrak.IsDeleted = true;
        evrak.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("EBYS Evrak silindi: {EvrakNo}", evrak.EvrakNo);
    }

    #endregion

    #region Kategori İşlemleri

    public async Task<List<EbysEvrakKategori>> GetKategorilerAsync()
    {
        return await _context.EbysEvrakKategoriler
            .Where(k => k.Aktif)
            .OrderBy(k => k.SiraNo)
            .ThenBy(k => k.KategoriAdi)
            .ToListAsync();
    }

    public async Task<EbysEvrakKategori> CreateKategoriAsync(EbysEvrakKategori kategori)
    {
        _context.EbysEvrakKategoriler.Add(kategori);
        await _context.SaveChangesAsync();
        return kategori;
    }

    public async Task UpdateKategoriAsync(EbysEvrakKategori kategori)
    {
        _context.EbysEvrakKategoriler.Update(kategori);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteKategoriAsync(int id)
    {
        var kategori = await _context.EbysEvrakKategoriler.FindAsync(id);
        if (kategori != null)
        {
            kategori.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Dosya İşlemleri

    public async Task<EbysEvrakDosya> DosyaYukleAsync(int evrakId, IBrowserFile file, bool asilNusha = false)
    {
        var evrak = await _context.EbysEvraklar.FindAsync(evrakId)
            ?? throw new InvalidOperationException("Evrak bulunamadı");

        // Dosya kaydetme dizini
        var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "ebys", evrakId.ToString());
        Directory.CreateDirectory(uploadFolder);

        var dosyaAdi = $"{Guid.NewGuid()}_{file.Name}";
        var dosyaYolu = Path.Combine(uploadFolder, dosyaAdi);

        // Dosya yükleme (max 50MB)
        await using (var stream = new FileStream(dosyaYolu, FileMode.Create))
        {
            await file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024).CopyToAsync(stream);
        }

        var evrakDosya = new EbysEvrakDosya
        {
            EvrakId = evrakId,
            DosyaAdi = file.Name,
            DosyaYolu = $"/uploads/ebys/{evrakId}/{dosyaAdi}",
            DosyaTipi = Path.GetExtension(file.Name).TrimStart('.'),
            DosyaBoyutu = file.Size,
            AsilNusha = asilNusha
        };

        _context.EbysEvrakDosyalar.Add(evrakDosya);
        await _context.SaveChangesAsync();

        await HareketEkleAsync(evrakId, 1, EbysHareketTipi.DosyaEklendi, $"Dosya eklendi: {file.Name}");

        return evrakDosya;
    }

    public async Task<EbysEvrakDosya?> GetDosyaAsync(int dosyaId)
    {
        return await _context.EbysEvrakDosyalar.FindAsync(dosyaId);
    }

    public async Task DosyaSilAsync(int dosyaId)
    {
        var dosya = await _context.EbysEvrakDosyalar.FindAsync(dosyaId);
        if (dosya != null)
        {
            // Fiziksel dosyayı sil
            var fizikselYol = Path.Combine(_environment.WebRootPath, dosya.DosyaYolu.TrimStart('/'));
            if (File.Exists(fizikselYol))
            {
                File.Delete(fizikselYol);
            }

            dosya.IsDeleted = true;
            await _context.SaveChangesAsync();

            await HareketEkleAsync(dosya.EvrakId, 1, EbysHareketTipi.DosyaSilindi, $"Dosya silindi: {dosya.DosyaAdi}");
        }
    }

    public async Task<EbysEvrakDosya> DosyaGuncelleAsync(int dosyaId, IBrowserFile file, string? degisiklikNotu = null)
    {
        var dosya = await _context.EbysEvrakDosyalar.FindAsync(dosyaId)
            ?? throw new InvalidOperationException("Dosya bulunamadı");

        // Eski fiziksel dosyayı sil
        var eskiFizikselYol = Path.Combine(_environment.WebRootPath, dosya.DosyaYolu.TrimStart('/'));
        if (File.Exists(eskiFizikselYol))
        {
            File.Delete(eskiFizikselYol);
        }

        // Yeni dosyayı kaydet
        var klasor = Path.Combine(_environment.WebRootPath, "uploads", "ebys", dosya.EvrakId.ToString());
        if (!Directory.Exists(klasor))
            Directory.CreateDirectory(klasor);

        var benzersizAd = $"{Guid.NewGuid()}_{file.Name}";
        var yeniYol = Path.Combine(klasor, benzersizAd);

        await using var stream = file.OpenReadStream(50 * 1024 * 1024); // Max 50MB
        await using var fileStream = new FileStream(yeniYol, FileMode.Create);
        await stream.CopyToAsync(fileStream);

        // Dosya bilgilerini güncelle
        dosya.DosyaAdi = file.Name;
        dosya.DosyaYolu = $"/uploads/ebys/{dosya.EvrakId}/{benzersizAd}";
        dosya.DosyaTipi = Path.GetExtension(file.Name).TrimStart('.');
        dosya.DosyaBoyutu = file.Size;
        dosya.SonDegisiklikNotu = degisiklikNotu;
        dosya.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await HareketEkleAsync(dosya.EvrakId, 1, EbysHareketTipi.Guncellendi, 
            $"Dosya güncellendi (v{dosya.VersiyonNo}): {file.Name}" + 
            (!string.IsNullOrEmpty(degisiklikNotu) ? $" - {degisiklikNotu}" : ""));

        return dosya;
    }

    #endregion

    #region Atama İşlemleri

    public async Task<EbysEvrakAtama> AtamaYapAsync(EbysEvrakAtamaModel model)
    {
        var evrak = await _context.EbysEvraklar.FindAsync(model.EvrakId)
            ?? throw new InvalidOperationException("Evrak bulunamadı");

        var atama = new EbysEvrakAtama
        {
            EvrakId = model.EvrakId,
            AtananKullaniciId = model.AtananKullaniciId,
            AtananDepartmanId = model.AtananDepartmanId,
            AtayanKullaniciId = 1, // TODO: Gerçek kullanıcıdan al
            AtamaTarihi = DateTime.Now,
            Talimat = model.Talimat,
            TeslimTarihi = model.TeslimTarihi,
            Durum = AtamaDurum.Beklemede
        };

        _context.EbysEvrakAtamalar.Add(atama);

        // Evrak durumunu güncelle
        evrak.AtananKullaniciId = model.AtananKullaniciId;
        evrak.AtananDepartmanId = model.AtananDepartmanId;
        evrak.Durum = EbysEvrakDurum.AtamaBekliyor;
        evrak.SonIslemTarihi = DateTime.Now;

        await _context.SaveChangesAsync();

        await HareketEkleAsync(model.EvrakId, 1, EbysHareketTipi.AtamaYapildi,
            $"Evrak atandı: Kullanıcı #{model.AtananKullaniciId}");

        return atama;
    }

    public async Task AtamaTamamlaAsync(int atamaId, string sonuc)
    {
        var atama = await _context.EbysEvrakAtamalar
            .Include(a => a.Evrak)
            .FirstOrDefaultAsync(a => a.Id == atamaId)
            ?? throw new InvalidOperationException("Atama bulunamadı");

        atama.Durum = AtamaDurum.Tamamlandi;
        atama.Sonuc = sonuc;
        atama.UpdatedAt = DateTime.UtcNow;

        if (atama.Evrak != null)
        {
            atama.Evrak.Durum = EbysEvrakDurum.Tamamlandi;
            atama.Evrak.SonIslemTarihi = DateTime.Now;
        }

        await _context.SaveChangesAsync();

        await HareketEkleAsync(atama.EvrakId, 1, EbysHareketTipi.DurumDegisti,
            $"Atama tamamlandı: {sonuc}");
    }

    public async Task AtamaReddetAsync(int atamaId, string sebep)
    {
        var atama = await _context.EbysEvrakAtamalar
            .Include(a => a.Evrak)
            .FirstOrDefaultAsync(a => a.Id == atamaId)
            ?? throw new InvalidOperationException("Atama bulunamadı");

        atama.Durum = AtamaDurum.Reddedildi;
        atama.Sonuc = sebep;
        atama.UpdatedAt = DateTime.UtcNow;

        if (atama.Evrak != null)
        {
            atama.Evrak.Durum = EbysEvrakDurum.Beklemede;
            atama.Evrak.SonIslemTarihi = DateTime.Now;
        }

        await _context.SaveChangesAsync();

        await HareketEkleAsync(atama.EvrakId, 1, EbysHareketTipi.DurumDegisti,
            $"Atama reddedildi: {sebep}");
    }

    public async Task<List<EbysEvrakAtama>> GetEvrakAtalamariAsync(int evrakId)
    {
        return await _context.EbysEvrakAtamalar
            .Include(a => a.AtananKullanici)
            .Include(a => a.AtayanKullanici)
            .Where(a => a.EvrakId == evrakId)
            .OrderByDescending(a => a.AtamaTarihi)
            .ToListAsync();
    }

    public async Task<List<EbysEvrakAtama>> GetKullaniciAtamalariAsync(int kullaniciId)
    {
        return await _context.EbysEvrakAtamalar
            .Include(a => a.Evrak)
            .Include(a => a.AtayanKullanici)
            .Where(a => a.AtananKullaniciId == kullaniciId && a.Durum == AtamaDurum.Beklemede)
            .OrderByDescending(a => a.AtamaTarihi)
            .ToListAsync();
    }

    #endregion

    #region Durum Değişikliği

    public async Task DurumDegistirAsync(int evrakId, EbysEvrakDurum yeniDurum, string? aciklama = null)
    {
        var evrak = await _context.EbysEvraklar.FindAsync(evrakId)
            ?? throw new InvalidOperationException("Evrak bulunamadı");

        var eskiDurum = evrak.Durum;
        evrak.Durum = yeniDurum;
        evrak.SonIslemTarihi = DateTime.Now;
        evrak.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await HareketEkleAsync(evrakId, 1, EbysHareketTipi.DurumDegisti,
            aciklama ?? $"Durum değişti: {eskiDurum} → {yeniDurum}",
            eskiDurum.ToString(), yeniDurum.ToString());
    }

    #endregion

    #region Hareket Geçmişi

    public async Task<List<EbysEvrakHareket>> GetEvrakHareketleriAsync(int evrakId)
    {
        return await _context.EbysEvrakHareketler
            .Include(h => h.Kullanici)
            .Where(h => h.EvrakId == evrakId)
            .OrderByDescending(h => h.IslemTarihi)
            .ToListAsync();
    }

    private async Task HareketEkleAsync(int evrakId, int kullaniciId, EbysHareketTipi hareketTipi,
        string aciklama, string? eskiDeger = null, string? yeniDeger = null)
    {
        var hareket = new EbysEvrakHareket
        {
            EvrakId = evrakId,
            KullaniciId = kullaniciId,
            HareketTipi = hareketTipi,
            Aciklama = aciklama,
            IslemTarihi = DateTime.Now,
            EskiDeger = eskiDeger,
            YeniDeger = yeniDeger
        };

        _context.EbysEvrakHareketler.Add(hareket);
        await _context.SaveChangesAsync();
    }

    #endregion

    #region İstatistikler

    public async Task<EbysEvrakIstatistik> GetIstatistiklerAsync()
    {
        var bugun = DateTime.Today;

        var evraklar = await _context.EbysEvraklar.ToListAsync();

        var istatistik = new EbysEvrakIstatistik
        {
            ToplamGelen = evraklar.Count(e => e.Yon == EvrakYonu.Gelen),
            ToplamGiden = evraklar.Count(e => e.Yon == EvrakYonu.Giden),
            BekleyenGelen = evraklar.Count(e => e.Yon == EvrakYonu.Gelen &&
                (e.Durum == EbysEvrakDurum.Beklemede || e.Durum == EbysEvrakDurum.Isleniyor)),
            BekleyenGiden = evraklar.Count(e => e.Yon == EvrakYonu.Giden &&
                (e.Durum == EbysEvrakDurum.Beklemede || e.Durum == EbysEvrakDurum.Isleniyor)),
            CevapBekleyen = evraklar.Count(e => e.CevapGerekli &&
                e.Durum != EbysEvrakDurum.Cevaplandi && e.Durum != EbysEvrakDurum.Tamamlandi),
            BugunGelenSayisi = evraklar.Count(e => e.Yon == EvrakYonu.Gelen && e.EvrakTarihi == bugun),
            BugunGidenSayisi = evraklar.Count(e => e.Yon == EvrakYonu.Giden && e.EvrakTarihi == bugun),
            GecikmisCevap = evraklar.Count(e => e.CevapGerekli &&
                e.CevapSuresi.HasValue && e.CevapSuresi.Value < DateTime.Now &&
                e.Durum != EbysEvrakDurum.Cevaplandi && e.Durum != EbysEvrakDurum.Tamamlandi)
        };

        // Kategori bazında dağılım
        var kategoriler = await _context.EbysEvrakKategoriler.ToListAsync();
        foreach (var kategori in kategoriler)
        {
            var sayi = evraklar.Count(e => e.KategoriId == kategori.Id);
            if (sayi > 0)
                istatistik.KategoriBazindaDagilim[kategori.KategoriAdi] = sayi;
        }

        // Durum bazında dağılım
        foreach (EbysEvrakDurum durum in Enum.GetValues<EbysEvrakDurum>())
        {
            var sayi = evraklar.Count(e => e.Durum == durum);
            if (sayi > 0)
                istatistik.DurumBazindaDagilim[DurumAdiGetir(durum)] = sayi;
        }

        return istatistik;
    }

    private static string DurumAdiGetir(EbysEvrakDurum durum) => durum switch
    {
        EbysEvrakDurum.Taslak => "Taslak",
        EbysEvrakDurum.Beklemede => "Beklemede",
        EbysEvrakDurum.Isleniyor => "İşleniyor",
        EbysEvrakDurum.AtamaBekliyor => "Atama Bekliyor",
        EbysEvrakDurum.CevapBekliyor => "Cevap Bekliyor",
        EbysEvrakDurum.Cevaplandi => "Cevaplandı",
        EbysEvrakDurum.Tamamlandi => "Tamamlandı",
        EbysEvrakDurum.Arsivlendi => "Arşivlendi",
        EbysEvrakDurum.IptalEdildi => "İptal Edildi",
        _ => durum.ToString()
    };

    #endregion

    #region Evrak Numarası Oluşturma

    public async Task<string> YeniEvrakNoOlusturAsync(EvrakYonu yon)
    {
        var yil = DateTime.Now.Year;
        var prefix = yon == EvrakYonu.Gelen ? "GE" : "GI";

        var sonEvrak = await _context.EbysEvraklar
            .Where(e => e.Yon == yon && e.EvrakNo.StartsWith($"{prefix}-{yil}"))
            .OrderByDescending(e => e.Id)
            .FirstOrDefaultAsync();

        var siraNo = 1;
        if (sonEvrak != null)
        {
            var parts = sonEvrak.EvrakNo.Split('-');
            if (parts.Length >= 3 && int.TryParse(parts[2], out var sonSira))
            {
                siraNo = sonSira + 1;
            }
        }

        return $"{prefix}-{yil}-{siraNo:D5}";
    }

    #endregion
}
