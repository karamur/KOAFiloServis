using CRMFiloServis.Shared.Entities;

namespace CRMFiloServis.Web.Services;

public interface ILisansService
{
    // Lisans Yonetimi
    Task<Lisans?> GetAktifLisansAsync();
    Task<bool> LisansGecerliMiAsync();
    Task<Lisans> AktiveLisansAsync(string lisansAnahtari);
    Task<int> KalanGunAsync();
    Task<string> GetMakineKoduAsync();

    // Trial
    Task<Lisans> OlusturTrialLisansAsync();

    // Kontroller
    Task<bool> KullanicLimitiKontrolAsync();
    Task<bool> ModulIzniVarMiAsync(string modulAdi);
}

public interface IKullaniciService
{
    // Kullanici CRUD
    Task<List<Kullanici>> GetAllAsync();
    Task<Kullanici?> GetByIdAsync(int id);
    Task<Kullanici?> GetByKullaniciAdiAsync(string kullaniciAdi);
    Task<Kullanici> CreateAsync(Kullanici kullanici, string sifre);
    Task<Kullanici> UpdateAsync(Kullanici kullanici);
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

    // Roller
    Task<List<Rol>> GetRollerAsync();
    Task<Rol> CreateRolAsync(Rol rol);
    Task<Rol> UpdateRolAsync(Rol rol);
    Task DeleteRolAsync(int rolId);
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
