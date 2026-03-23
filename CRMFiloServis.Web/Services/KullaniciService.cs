using System.Security.Cryptography;
using System.Text;
using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class KullaniciService : IKullaniciService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private static Kullanici? _aktifKullanici;

    public KullaniciService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
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
            return new KullaniciGirisSonuc { Basarili = false, Mesaj = "Kullanici bulunamadi" };

        if (!kullanici.Aktif)
            return new KullaniciGirisSonuc { Basarili = false, Mesaj = "Kullanici aktif degil" };

        if (kullanici.Kilitli)
            return new KullaniciGirisSonuc { Basarili = false, Mesaj = "Kullanici kilitli. Yoneticiye basvurun." };

        if (!VerifyPassword(sifre, kullanici.SifreHash))
        {
            kullanici.BasarisizGirisSayisi++;
            if (kullanici.BasarisizGirisSayisi >= 5)
                kullanici.Kilitli = true;
            await context.SaveChangesAsync();

            return new KullaniciGirisSonuc { Basarili = false, Mesaj = "Sifre hatali" };
        }

        // Basarili giris
        kullanici.SonGirisTarihi = DateTime.UtcNow;
        kullanici.BasarisizGirisSayisi = 0;
        await context.SaveChangesAsync();

        _aktifKullanici = kullanici;
        return new KullaniciGirisSonuc { Basarili = true, Kullanici = kullanici };
    }

    public Task CikisYapAsync()
    {
        _aktifKullanici = null;
        return Task.CompletedTask;
    }

    public Task<Kullanici?> GetAktifKullaniciAsync()
    {
        return Task.FromResult(_aktifKullanici);
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
        return new List<string>
        {
            Yetkiler.Dashboard,
            Yetkiler.CariOkuma, Yetkiler.CariYazma, Yetkiler.CariSilme,
            Yetkiler.FaturaOkuma, Yetkiler.FaturaYazma, Yetkiler.FaturaSilme,
            Yetkiler.BankaOkuma, Yetkiler.BankaYazma,
            Yetkiler.MuhasebeOkuma, Yetkiler.MuhasebeYazma,
            Yetkiler.RaporOkuma, Yetkiler.RaporExport,
            Yetkiler.AyarlarOkuma, Yetkiler.AyarlarYazma,
            Yetkiler.SatisOkuma, Yetkiler.SatisYazma,
            Yetkiler.KullaniciYonetimi, Yetkiler.YedeklemeYonetimi, Yetkiler.LisansYonetimi
        };
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
        existing.UpdatedAt = DateTime.UtcNow;

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

        // Admin rolu olustur
        var adminRol = await context.Roller.FirstOrDefaultAsync(r => r.RolAdi == "Admin");
        if (adminRol == null)
        {
            adminRol = new Rol
            {
                RolAdi = "Admin",
                Aciklama = "Sistem Yoneticisi",
                SistemRolu = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Roller.Add(adminRol);
            await context.SaveChangesAsync();
        }

        // Kullanici rolu olustur
        var kullaniciRol = await context.Roller.FirstOrDefaultAsync(r => r.RolAdi == "Kullanici");
        if (kullaniciRol == null)
        {
            kullaniciRol = new Rol
            {
                RolAdi = "Kullanici",
                Aciklama = "Standart Kullanici",
                SistemRolu = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Roller.Add(kullaniciRol);
            await context.SaveChangesAsync();
        }

        // Admin kullanici olustur
        if (!await context.Kullanicilar.AnyAsync(k => k.KullaniciAdi == "admin"))
        {
            var admin = new Kullanici
            {
                KullaniciAdi = "admin",
                SifreHash = HashPassword("admin123"),
                AdSoyad = "Sistem Yoneticisi",
                RolId = adminRol.Id,
                Aktif = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Kullanicilar.Add(admin);
            await context.SaveChangesAsync();
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
