using CRMFiloServis.Shared.Entities;

namespace CRMFiloServis.Web.Services;

public interface IKullaniciService
{
    // CRUD
    Task<List<Kullanici>> GetAllAsync();
    Task<Kullanici?> GetByIdAsync(int id);
    Task<Kullanici?> GetByKullaniciAdiAsync(string kullaniciAdi);
    Task<Kullanici> CreateAsync(Kullanici kullanici, string sifre);
    Task<Kullanici> UpdateAsync(Kullanici kullanici);
    Task<Kullanici> ToggleAktifAsync(int id);
    Task DeleteAsync(int id);
    
    // Giris/Cikis
    Task<KullaniciGirisSonuc> GirisYapAsync(string kullaniciAdi, string sifre);
    Task CikisYapAsync();
    Task<Kullanici?> GetAktifKullaniciAsync();
    
    // Sifre
    Task SifreDegistirAsync(int kullaniciId, string eskiSifre, string yeniSifre);
    Task SifreSifirlaAsync(int kullaniciId, string yeniSifre);
    
    // Yetki
    Task<bool> YetkiVarMiAsync(int kullaniciId, string yetkiKodu);
    Task<List<string>> GetKullaniciYetkileriAsync(int kullaniciId);
    Task<HashSet<string>> GetCurrentUserYetkilerAsync();
    
    // Roller
    Task<List<Rol>> GetRollerAsync();
    Task<Rol> CreateRolAsync(Rol rol);
    Task<Rol> UpdateRolAsync(Rol rol);
    Task DeleteRolAsync(int rolId);
    Task<Rol> UpdateRolYetkileriAsync(int rolId, List<RolYetki> yetkiler);
    Task SetRolYetkileriAsync(int rolId, List<string> yetkiKodlari);
    
    // Seed
    Task SeedAdminAsync();
}

public class KullaniciGirisSonuc
{
    public bool Basarili { get; set; }
    public string? Mesaj { get; set; }
    public Kullanici? Kullanici { get; set; }
}
