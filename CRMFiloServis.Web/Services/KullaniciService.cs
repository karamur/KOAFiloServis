using System.Security.Cryptography;
using System.Text;
using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class KullaniciService : IKullaniciService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly AppAuthenticationStateProvider _authProvider;
    private readonly ILogger<KullaniciService> _logger;

    public KullaniciService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        AppAuthenticationStateProvider authProvider,
        ILogger<KullaniciService> logger)
    {
        _contextFactory = contextFactory;
        _authProvider = authProvider;
        _logger = logger;
    }

    #region CRUD

    public async Task<List<Kullanici>> GetAllAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Kullanicilar
            .Include(k => k.Rol)
            .OrderBy(k => k.AdSoyad)
            .ToListAsync();
    }

    public async Task<Kullanici?> GetByIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Kullanicilar
            .Include(k => k.Rol)
            .FirstOrDefaultAsync(k => k.Id == id);
    }

    public async Task<Kullanici?> GetByKullaniciAdiAsync(string kullaniciAdi)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Kullanicilar
            .Include(k => k.Rol)
            .FirstOrDefaultAsync(k => k.KullaniciAdi == kullaniciAdi);
    }

    public async Task<Kullanici> CreateAsync(Kullanici kullanici, string sifre)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // Kullanici adi kontrolu
        if (await context.Kullanicilar.AnyAsync(k => k.KullaniciAdi == kullanici.KullaniciAdi))
            throw new Exception("Bu kullanici adi zaten kayitli!");

        kullanici.SifreHash = HashPassword(sifre);
        kullanici.CreatedAt = DateTime.UtcNow;

        context.Kullanicilar.Add(kullanici);
        await context.SaveChangesAsync();
        return kullanici;
    }

    public async Task<Kullanici> UpdateAsync(Kullanici kullanici)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.Kullanicilar.FindAsync(kullanici.Id);
        if (existing == null) throw new Exception("Kullanici bulunamadi");

        existing.AdSoyad = kullanici.AdSoyad;
        existing.Email = kullanici.Email;
        existing.Telefon = kullanici.Telefon;
        existing.RolId = kullanici.RolId;
        existing.SoforId = kullanici.SoforId;
        existing.Aktif = kullanici.Aktif;
        existing.Tema = kullanici.Tema;
        existing.KompaktMod = kullanici.KompaktMod;
        existing.UpdatedAt = DateTime.UtcNow;

        context.Kullanicilar.Update(existing);
        await context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var kullanici = await context.Kullanicilar.FindAsync(id);
        if (kullanici == null) return;

        kullanici.IsDeleted = true;
        kullanici.UpdatedAt = DateTime.UtcNow;
        context.Kullanicilar.Update(kullanici);
        await context.SaveChangesAsync();
    }

    #endregion

    #region Giris/Cikis

    public async Task<KullaniciGirisSonuc> GirisYapAsync(string kullaniciAdi, string sifre)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var kullanici = await context.Kullanicilar
            .Include(k => k.Rol)
            .FirstOrDefaultAsync(k => k.KullaniciAdi == kullaniciAdi);

        if (kullanici == null)
        {
            _logger.LogWarning("Giris basarisiz - kullanici bulunamadi: {KullaniciAdi}", kullaniciAdi);
            return new KullaniciGirisSonuc { Basarili = false, Mesaj = "Kullanici bulunamadi" };
        }

        if (!kullanici.Aktif)
        {
            _logger.LogWarning("Giris basarisiz - kullanici aktif degil: {KullaniciAdi}", kullaniciAdi);
            return new KullaniciGirisSonuc { Basarili = false, Mesaj = "Kullanici aktif degil" };
        }

        if (kullanici.Kilitli)
        {
            _logger.LogWarning("Giris basarisiz - kullanici kilitli: {KullaniciAdi}", kullaniciAdi);
            return new KullaniciGirisSonuc { Basarili = false, Mesaj = "Kullanici kilitli. Yoneticiye basvurun." };
        }

        if (!VerifyPassword(sifre, kullanici.SifreHash))
        {
            kullanici.BasarisizGirisSayisi++;
            if (kullanici.BasarisizGirisSayisi >= 5)
            {
                kullanici.Kilitli = true;
                _logger.LogWarning("Kullanici kilitlendi (5 basarisiz deneme): {KullaniciAdi}", kullaniciAdi);
            }
            context.Kullanicilar.Update(kullanici);
            await context.SaveChangesAsync();

            _logger.LogWarning("Giris basarisiz - sifre hatali: {KullaniciAdi}", kullaniciAdi);
            return new KullaniciGirisSonuc { Basarili = false, Mesaj = "Sifre hatali" };
        }

        // Basarili giris
        kullanici.SonGirisTarihi = DateTime.UtcNow;
        kullanici.BasarisizGirisSayisi = 0;
        context.Kullanicilar.Update(kullanici);
        await context.SaveChangesAsync();

        // Authentication state guncelle - async versiyon
        await _authProvider.GirisYapAsync(kullanici);

        _logger.LogInformation("Basarili giris: {KullaniciAdi}, Rol: {Rol}",
            kullaniciAdi, kullanici.Rol?.RolAdi);

        return new KullaniciGirisSonuc { Basarili = true, Kullanici = kullanici };
    }

    public async Task CikisYapAsync()
    {
        var aktifKullanici = _authProvider.GetAktifKullanici();
        var sessionId = _authProvider.GetSessionId();

        _logger.LogInformation("Cikis yapiliyor: {KullaniciAdi}, SessionId: {SessionId}",
            aktifKullanici?.KullaniciAdi, sessionId);

        await _authProvider.CikisYapAsync();
    }

    public Task<Kullanici?> GetAktifKullaniciAsync()
    {
        return Task.FromResult(_authProvider.GetAktifKullanici());
    }

    #endregion

    #region Sifre

    public async Task SifreDegistirAsync(int kullaniciId, string eskiSifre, string yeniSifre)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var kullanici = await context.Kullanicilar.FindAsync(kullaniciId);
        if (kullanici == null) throw new Exception("Kullanici bulunamadi");

        if (!VerifyPassword(eskiSifre, kullanici.SifreHash))
            throw new Exception("Mevcut sifre hatali");

        kullanici.SifreHash = HashPassword(yeniSifre);
        kullanici.UpdatedAt = DateTime.UtcNow;
        context.Kullanicilar.Update(kullanici);
        await context.SaveChangesAsync();
    }

    public async Task SifreSifirlaAsync(int kullaniciId, string yeniSifre)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var kullanici = await context.Kullanicilar.FindAsync(kullaniciId);
        if (kullanici == null) throw new Exception("Kullanici bulunamadi");

        kullanici.SifreHash = HashPassword(yeniSifre);
        kullanici.Kilitli = false;
        kullanici.BasarisizGirisSayisi = 0;
        kullanici.UpdatedAt = DateTime.UtcNow;
        context.Kullanicilar.Update(kullanici);
        await context.SaveChangesAsync();
    }

    #endregion

    #region Yetki

    public async Task<bool> YetkiVarMiAsync(int kullaniciId, string yetkiKodu)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var kullanici = await context.Kullanicilar
            .Include(k => k.Rol)
            .ThenInclude(r => r.Yetkiler)
            .FirstOrDefaultAsync(k => k.Id == kullaniciId);

        if (kullanici == null) return false;
        if (kullanici.Rol.RolAdi == "Admin") return true; // Admin her seye yetkili

        return kullanici.Rol.Yetkiler.Any(y => y.YetkiKodu == yetkiKodu && y.Izin);
    }

    public async Task<List<string>> GetKullaniciYetkileriAsync(int kullaniciId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var kullanici = await context.Kullanicilar
            .Include(k => k.Rol)
            .ThenInclude(r => r.Yetkiler)
            .FirstOrDefaultAsync(k => k.Id == kullaniciId);

        if (kullanici == null) return new List<string>();
        if (kullanici.Rol.RolAdi == "Admin") return GetTumYetkiler();

        return kullanici.Rol.Yetkiler.Where(y => y.Izin).Select(y => y.YetkiKodu).ToList();
    }

    private List<string> GetTumYetkiler()
    {
        return Yetkiler.GetAll();
    }

    #endregion

    #region Roller

    public async Task<List<Rol>> GetRollerAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Roller
            .Include(r => r.Yetkiler)
            .OrderBy(r => r.RolAdi)
            .ToListAsync();
    }

    public async Task<Rol> CreateRolAsync(Rol rol)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        rol.CreatedAt = DateTime.UtcNow;
        context.Roller.Add(rol);
        await context.SaveChangesAsync();
        return rol;
    }

    public async Task<Rol> UpdateRolAsync(Rol rol)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.Roller.FindAsync(rol.Id);
        if (existing == null) throw new Exception("Rol bulunamadi");
        if (existing.SistemRolu) throw new Exception("Sistem rolu duzenlenemez");

        existing.RolAdi = rol.RolAdi;
        existing.Aciklama = rol.Aciklama;
        existing.Renk = rol.Renk;
        existing.UpdatedAt = DateTime.UtcNow;

        context.Roller.Update(existing);
        await context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteRolAsync(int rolId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var rol = await context.Roller.FindAsync(rolId);
        if (rol == null) return;
        if (rol.SistemRolu) throw new Exception("Sistem rolu silinemez");

        // Rolu kullanan kullanici var mi?
        if (await context.Kullanicilar.AnyAsync(k => k.RolId == rolId))
            throw new Exception("Bu role atanmis kullanicilar var");

        rol.IsDeleted = true;
        rol.UpdatedAt = DateTime.UtcNow;
        context.Roller.Update(rol);
        await context.SaveChangesAsync();
    }

    public async Task<Rol> UpdateRolYetkileriAsync(int rolId, List<RolYetki> yetkiler)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var rol = await context.Roller
            .Include(r => r.Yetkiler)
            .FirstOrDefaultAsync(r => r.Id == rolId);

        if (rol == null) throw new Exception("Rol bulunamadi");

        // Mevcut yetkileri sil
        context.RolYetkileri.RemoveRange(rol.Yetkiler);

        // Yeni yetkileri ekle
        foreach (var yetki in yetkiler)
        {
            yetki.RolId = rolId;
            yetki.CreatedAt = DateTime.UtcNow;
            context.RolYetkileri.Add(yetki);
        }

        await context.SaveChangesAsync();
        return rol;
    }

    public async Task SetRolYetkileriAsync(int rolId, List<string> yetkiKodlari)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var rol = await context.Roller
            .Include(r => r.Yetkiler)
            .FirstOrDefaultAsync(r => r.Id == rolId);

        if (rol == null) throw new Exception("Rol bulunamadi");

        // Mevcut yetkileri sil
        context.RolYetkileri.RemoveRange(rol.Yetkiler);

        // Yeni yetkileri ekle
        foreach (var yetkiKodu in yetkiKodlari)
        {
            context.RolYetkileri.Add(new RolYetki
            {
                RolId = rolId,
                YetkiKodu = yetkiKodu,
                Izin = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
    }

    #endregion

    #region Seed

    public async Task SeedAdminAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // Tum sistem rollerini olustur
        foreach (var rolTanim in SistemRolleri.GetAllRoles())
        {
            var mevcutRol = await context.Roller.FirstOrDefaultAsync(r => r.RolAdi == rolTanim.Name);
            if (mevcutRol == null)
            {
                var yeniRol = new Rol
                {
                    RolAdi = rolTanim.Name,
                    Aciklama = rolTanim.Description,
                    Renk = rolTanim.Color,
                    SistemRolu = rolTanim.Name == SistemRolleri.Admin || rolTanim.Name == SistemRolleri.Kullanici,
                    CreatedAt = DateTime.UtcNow
                };
                context.Roller.Add(yeniRol);
                await context.SaveChangesAsync();

                // Varsayilan yetkileri ata
                var varsayilanYetkiler = SistemRolleri.GetDefaultPermissions(rolTanim.Name);
                foreach (var yetkiKodu in varsayilanYetkiler)
                {
                    context.RolYetkileri.Add(new RolYetki
                    {
                        RolId = yeniRol.Id,
                        YetkiKodu = yetkiKodu,
                        Izin = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                await context.SaveChangesAsync();
            }
            else if (string.IsNullOrEmpty(mevcutRol.Renk))
            {
                // Mevcut role renk ekle
                mevcutRol.Renk = rolTanim.Color;
                mevcutRol.Aciklama = rolTanim.Description;
                await context.SaveChangesAsync();
            }
        }

        // Admin kullanici olustur veya sifresini dogrula
        var adminRol = await context.Roller.FirstOrDefaultAsync(r => r.RolAdi == SistemRolleri.Admin);
        if (adminRol != null)
        {
            var adminUser = await context.Kullanicilar.FirstOrDefaultAsync(k => k.KullaniciAdi == "admin");
            if (adminUser == null)
            {
                adminUser = new Kullanici
                {
                    KullaniciAdi = "admin",
                    SifreHash = HashPassword("admin123"),
                    AdSoyad = "Sistem Yoneticisi",
                    RolId = adminRol.Id,
                    Aktif = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Kullanicilar.Add(adminUser);
                await context.SaveChangesAsync();
            }
            else
            {
                // Admin varsa sifresinin dogru oldugundan emin ol
                // ve kilitli/pasif ise duzelt
                var dogruHash = HashPassword("admin123");
                if (adminUser.SifreHash != dogruHash || !adminUser.Aktif || adminUser.Kilitli)
                {
                    adminUser.SifreHash = dogruHash;
                    adminUser.Aktif = true;
                    adminUser.Kilitli = false;
                    adminUser.BasarisizGirisSayisi = 0;
                    adminUser.RolId = adminRol.Id;
                    adminUser.UpdatedAt = DateTime.UtcNow;
                    context.Kullanicilar.Update(adminUser);
                    await context.SaveChangesAsync();
                }
            }
        }

        // TEST kullanici olustur - hizli giris icin
        if (adminRol != null)
        {
            var testUser = await context.Kullanicilar.FirstOrDefaultAsync(k => k.KullaniciAdi == "test");
            if (testUser == null)
            {
                testUser = new Kullanici
                {
                    KullaniciAdi = "test",
                    SifreHash = HashPassword("test123"),
                    AdSoyad = "Test Kullanici",
                    RolId = adminRol.Id,
                    Aktif = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Kullanicilar.Add(testUser);
                await context.SaveChangesAsync();
            }
            else
            {
                // Test kullanici sifresi/durumu duzelt
                var dogruHash = HashPassword("test123");
                if (testUser.SifreHash != dogruHash || !testUser.Aktif || testUser.Kilitli)
                {
                    testUser.SifreHash = dogruHash;
                    testUser.Aktif = true;
                    testUser.Kilitli = false;
                    testUser.BasarisizGirisSayisi = 0;
                    testUser.UpdatedAt = DateTime.UtcNow;
                    context.Kullanicilar.Update(testUser);
                    await context.SaveChangesAsync();
                }
            }
        }
    }

    #endregion

    #region Helpers

    private string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password + "CRMFiloServisSalt"));
        return Convert.ToBase64String(bytes);
    }

    private bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }

    #endregion
}
