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
    Task<Rol> UpdateRolYetkileriAsync(int rolId, List<RolYetki> yetkiler);
    
    // Seed
    Task SeedAdminAsync();
}

public class KullaniciGirisSonuc
{
    public bool Basarili { get; set; }
    public string? Mesaj { get; set; }
    public Kullanici? Kullanici { get; set; }
}

// Yetki kodlari
public static class Yetkiler
{
    public const string Dashboard = "dashboard";
    public const string CariOkuma = "cari.okuma";
    public const string CariYazma = "cari.yazma";
    public const string CariSilme = "cari.silme";
    public const string FaturaOkuma = "fatura.okuma";
    public const string FaturaYazma = "fatura.yazma";
    public const string FaturaSilme = "fatura.silme";
    public const string BankaOkuma = "banka.okuma";
    public const string BankaYazma = "banka.yazma";
    public const string MuhasebeOkuma = "muhasebe.okuma";
    public const string MuhasebeYazma = "muhasebe.yazma";
    public const string RaporOkuma = "rapor.okuma";
    public const string RaporExport = "rapor.export";
    public const string AyarlarOkuma = "ayarlar.okuma";
    public const string AyarlarYazma = "ayarlar.yazma";
    public const string SatisOkuma = "satis.okuma";
    public const string SatisYazma = "satis.yazma";
    public const string KullaniciYonetimi = "kullanici.yonetim";
    public const string YedeklemeYonetimi = "yedekleme.yonetim";
    public const string LisansYonetimi = "lisans.yonetim";
}
