using System.ComponentModel.DataAnnotations;

namespace CRMFiloServis.Shared.Entities;

#region Lisans

/// <summary>
/// Lisans Bilgileri
/// </summary>
public class Lisans : BaseEntity
{
    [Required]
    public string LisansAnahtari { get; set; } = string.Empty;

    public LisansTuru Tur { get; set; } = LisansTuru.Trial;

    public DateTime BaslangicTarihi { get; set; } = DateTime.UtcNow;
    public DateTime BitisTarihi { get; set; } = DateTime.UtcNow.AddDays(30);

    public string? FirmaAdi { get; set; }
    public string? YetkiliKisi { get; set; }
    public string? Email { get; set; }
    public string? Telefon { get; set; }

    public string MakineKodu { get; set; } = string.Empty;
    public int MaxKullaniciSayisi { get; set; } = 5;

    // Izinler
    public bool ExcelExportIzni { get; set; } = true;
    public bool PdfExportIzni { get; set; } = true;
    public bool RaporlamaIzni { get; set; } = true;
    public bool YedeklemeIzni { get; set; } = true;
    public bool MuhasebeIzni { get; set; } = true;
    public bool SatisModuluIzni { get; set; } = true;

    public string? Imza { get; set; }

    // Hesaplanan ozellikler
    public LisansDurumu Durum => DateTime.UtcNow > BitisTarihi ? LisansDurumu.SuresiDolmus : LisansDurumu.Aktif;
    public int KalanGun => Math.Max(0, (BitisTarihi.Date - DateTime.UtcNow.Date).Days);
    public bool Gecerli => Durum == LisansDurumu.Aktif && !string.IsNullOrEmpty(LisansAnahtari);
}

public enum LisansTuru
{
    Trial = 0,          // 30 gunluk deneme
    Basic = 1,          // Temel - 5 kullanici
    Professional = 2,   // Profesyonel - 10 kullanici
    Enterprise = 3      // Kurumsal - Sinirsiz
}

public enum LisansDurumu
{
    Aktif = 0,
    SuresiDolmus = 1,
    IptalEdilmis = 2,
    Gecersiz = 3
}

#endregion

#region Kullanici ve Rol

/// <summary>
/// Uygulama Kullanicisi
/// </summary>
public class Kullanici : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string KullaniciAdi { get; set; } = string.Empty;

    [Required]
    public string SifreHash { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string AdSoyad { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? Telefon { get; set; }

    public int? SoforId { get; set; } // Personel ile iliskilendirme
    public virtual Sofor? Sofor { get; set; }

    public int RolId { get; set; }
    public virtual Rol Rol { get; set; } = null!;

    public bool Aktif { get; set; } = true;
    public DateTime? SonGirisTarihi { get; set; }
    public int BasarisizGirisSayisi { get; set; } = 0;
    public bool Kilitli { get; set; } = false;

    // Tercihler
    public string Tema { get; set; } = "Default";
    public bool KompaktMod { get; set; } = false;
}

/// <summary>
/// Kullanici Rolleri
/// </summary>
public class Rol : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string RolAdi { get; set; } = string.Empty;

    public string? Aciklama { get; set; }

    public string? Renk { get; set; }

    public bool SistemRolu { get; set; } = false; // Admin gibi silinemeyen roller

    // Navigation
    public virtual ICollection<Kullanici> Kullanicilar { get; set; } = new List<Kullanici>();
    public virtual ICollection<RolYetki> Yetkiler { get; set; } = new List<RolYetki>();
}

/// <summary>
/// Rol Yetkileri
/// </summary>
public class RolYetki : BaseEntity
{
    public int RolId { get; set; }
    public virtual Rol Rol { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string YetkiKodu { get; set; } = string.Empty;

    public bool Izin { get; set; } = false;
}

#endregion

#region Sistem Rolleri

/// <summary>
/// Sistem rol tanimlari - crmdestek projesinden uyarlandi
/// </summary>
public static class SistemRolleri
{
    public const string Admin = "Admin";
    public const string Muhasebeci = "Muhasebeci";
    public const string Operasyon = "Operasyon";
    public const string SatisTemsilcisi = "SatisTemsilcisi";
    public const string Sofor = "SoforRol";
    public const string Kullanici = "Kullanici";

    public static List<RolTanim> GetAllRoles()
    {
        return new List<RolTanim>
        {
            new(Admin, "Sistem Yoneticisi", "Tum sistem yetkilerine sahip tam yetkili yonetici", "#dc3545", "bi-shield-lock"),
            new(Muhasebeci, "Muhasebeci", "Butce, fatura, banka ve muhasebe islemleri", "#6f42c1", "bi-calculator"),
            new(Operasyon, "Operasyon Sorumlusu", "Arac, sofor, guzergah ve servis islemleri", "#0d6efd", "bi-truck"),
            new(SatisTemsilcisi, "Satis Temsilcisi", "Satis modulu ve piyasa arastirma", "#198754", "bi-graph-up-arrow"),
            new(Sofor, "Sofor", "Kendine atanan arac ve guzergah bilgileri", "#fd7e14", "bi-person-badge"),
            new(Kullanici, "Genel Kullanici", "Temel goruntuleme yetkilerine sahip kullanici", "#6c757d", "bi-person"),
        };
    }

    /// <summary>
    /// Role gore varsayilan yetkileri dondurur
    /// </summary>
    public static List<string> GetDefaultPermissions(string roleName)
    {
        return roleName switch
        {
            Admin => Yetkiler.GetAll(),

            Muhasebeci => new List<string>
            {
                Yetkiler.Dashboard,
                // Cari
                Yetkiler.CariGoruntule, Yetkiler.CariEkle, Yetkiler.CariDuzenle,
                // Fatura
                Yetkiler.FaturaGoruntule, Yetkiler.FaturaEkle, Yetkiler.FaturaDuzenle, Yetkiler.FaturaSil,
                // Banka
                Yetkiler.BankaGoruntule, Yetkiler.BankaEkle, Yetkiler.BankaDuzenle,
                // Butce
                Yetkiler.ButceGoruntule, Yetkiler.ButceEkle, Yetkiler.ButceDuzenle, Yetkiler.ButceSil,
                // Muhasebe
                Yetkiler.MuhasebeGoruntule, Yetkiler.MuhasebeEkle, Yetkiler.MuhasebeDuzenle,
                // Rapor
                Yetkiler.RaporGoruntule, Yetkiler.RaporExport,
                // Yedek
                Yetkiler.YedeklemeGoruntule, Yetkiler.YedeklemeOlustur,
            },

            Operasyon => new List<string>
            {
                Yetkiler.Dashboard,
                // Arac
                Yetkiler.AracGoruntule, Yetkiler.AracEkle, Yetkiler.AracDuzenle,
                // Sofor
                Yetkiler.SoforGoruntule, Yetkiler.SoforEkle, Yetkiler.SoforDuzenle,
                // Guzergah
                Yetkiler.GuzergahGoruntule, Yetkiler.GuzergahEkle, Yetkiler.GuzergahDuzenle,
                // Servis
                Yetkiler.ServisGoruntule, Yetkiler.ServisEkle, Yetkiler.ServisDuzenle,
                // Masraf
                Yetkiler.MasrafGoruntule, Yetkiler.MasrafEkle,
                // Rapor
                Yetkiler.RaporGoruntule,
            },

            SatisTemsilcisi => new List<string>
            {
                Yetkiler.Dashboard,
                Yetkiler.SatisGoruntule, Yetkiler.SatisEkle, Yetkiler.SatisDuzenle,
                Yetkiler.CariGoruntule,
                Yetkiler.RaporGoruntule,
            },

            Sofor => new List<string>
            {
                Yetkiler.Dashboard,
                Yetkiler.AracGoruntule,
                Yetkiler.GuzergahGoruntule,
                Yetkiler.ServisGoruntule,
            },

            Kullanici => new List<string>
            {
                Yetkiler.Dashboard,
                Yetkiler.CariGoruntule,
                Yetkiler.RaporGoruntule,
            },

            _ => new List<string> { Yetkiler.Dashboard }
        };
    }
}

public class RolTanim
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string Color { get; set; }
    public string Icon { get; set; }

    public RolTanim(string name, string displayName, string description, string color, string icon)
    {
        Name = name;
        DisplayName = displayName;
        Description = description;
        Color = color;
        Icon = icon;
    }
}

#endregion

#region Yetki Tanimlari

/// <summary>
/// Yetki Tanimlari - modullere ve menulere gore gruplanmis
/// Ana menu ve alt menu bazinda yetkilendirme destegi
/// </summary>
public static class Yetkiler
{
    // Genel
    public const string Dashboard = "dashboard";

    // === MENU YETKILERI ===
    
    // Ana Sayfa
    public const string MenuAnaSayfa = "menu.anasayfa";
    public const string MenuBelgeUyarilari = "menu.belgeuyarilari";
    
    // Cari Modulu
    public const string MenuCariModulu = "menu.cari";
    public const string MenuCariler = "menu.cari.cariler";
    public const string MenuKesilenFaturalar = "menu.cari.kesilenFaturalar";
    public const string MenuGelenFaturalar = "menu.cari.gelenFaturalar";
    
    // Filo Servis
    public const string MenuFiloServis = "menu.filoservis";
    public const string MenuAraclar = "menu.filoservis.araclar";
    public const string MenuGuzergahlar = "menu.filoservis.guzergahlar";
    public const string MenuServisCalismalari = "menu.filoservis.serviscalismalari";
    public const string MenuTopluCalisma = "menu.filoservis.toplucalisma";
    public const string MenuMasrafKalemleri = "menu.filoservis.masrafkalemleri";
    public const string MenuAracMasraflari = "menu.filoservis.aracmasraflari";
    
    // Muhasebe
    public const string MenuMuhasebe = "menu.muhasebe";
    public const string MenuMuhasebeDashboard = "menu.muhasebe.dashboard";
    public const string MenuHesapPlani = "menu.muhasebe.hesapplani";
    public const string MenuMuhasebeFisleri = "menu.muhasebe.fisler";
    public const string MenuMuhasebeRaporlari = "menu.muhasebe.raporlar";
    public const string MenuMaliAnaliz = "menu.muhasebe.malianaliz";
    
    // Personel
    public const string MenuPersonel = "menu.personel";
    public const string MenuPersonelListesi = "menu.personel.liste";
    public const string MenuMaasYonetimi = "menu.personel.maas";
    public const string MenuIzinYonetimi = "menu.personel.izin";
    
    // Fatura Modulu
    public const string MenuFaturaModulu = "menu.fatura";
    public const string MenuFaturalar = "menu.fatura.faturalar";
    public const string MenuFaturaHazirlik = "menu.fatura.hazirlik";
    
    // Banka/Kasa
    public const string MenuBankaKasa = "menu.bankakasa";
    public const string MenuBankaHesaplari = "menu.bankakasa.hesaplar";
    public const string MenuBankaHareketleri = "menu.bankakasa.hareketler";
    public const string MenuOdemeEslestirme = "menu.bankakasa.eslestirme";
    
    // Raporlar
    public const string MenuRaporlar = "menu.raporlar";
    public const string MenuButceAnaliz = "menu.raporlar.butce";
    public const string MenuOdemeYonetimi = "menu.raporlar.odemeyonetimi";
    public const string MenuMaliAnalizRapor = "menu.raporlar.malianaliz";
    public const string MenuAylikChecklist = "menu.raporlar.checklist";
    public const string MenuOzmalAracRaporu = "menu.raporlar.ozmalarac";
    public const string MenuKiralikAracRaporu = "menu.raporlar.kiraliakarac";
    public const string MenuKomisyonRaporu = "menu.raporlar.komisyon";
    public const string MenuServisRaporu = "menu.raporlar.servis";
    public const string MenuFaturaOdemeRaporu = "menu.raporlar.faturaodeme";
    public const string MenuAracMasrafRaporu = "menu.raporlar.aracmasraf";
    public const string MenuCariEkstre = "menu.raporlar.cariekstre";
    
    // Satis Modulu
    public const string MenuSatisModulu = "menu.satis";
    public const string MenuSatisDashboard = "menu.satis.dashboard";
    public const string MenuPiyasaArastirma = "menu.satis.arastirma";
    public const string MenuYeniIlan = "menu.satis.yeniilan";
    public const string MenuSatisPersoneli = "menu.satis.personel";
    
    // Ayarlar
    public const string MenuAyarlar = "menu.ayarlar";
    public const string MenuFirmaYonetimi = "menu.ayarlar.firma";
    public const string MenuVeritabaniAyarlari = "menu.ayarlar.veritabani";
    public const string MenuServisRaporlari = "menu.ayarlar.servisraporlari";
    public const string MenuLisansBilgileri = "menu.ayarlar.lisans";
    public const string MenuKullaniciYonetimi = "menu.ayarlar.kullanicilar";
    public const string MenuRolYonetimi = "menu.ayarlar.roller";
    public const string MenuPiyasaKaynaklari = "menu.ayarlar.piyasakaynaklari";
    public const string MenuSistemDurumu = "menu.ayarlar.sistemdurumu";
    public const string MenuAktiviteLog = "menu.ayarlar.aktivitelog";
    public const string MenuYedekleme = "menu.ayarlar.yedekleme";
    public const string MenuGuncelleme = "menu.ayarlar.guncelleme";

    // === ISLEM YETKILERI ===
    
    // Cari
    public const string CariGoruntule = "cari.goruntule";
    public const string CariEkle = "cari.ekle";
    public const string CariDuzenle = "cari.duzenle";
    public const string CariSil = "cari.sil";

    // Fatura
    public const string FaturaGoruntule = "fatura.goruntule";
    public const string FaturaEkle = "fatura.ekle";
    public const string FaturaDuzenle = "fatura.duzenle";
    public const string FaturaSil = "fatura.sil";

    // Banka/Kasa
    public const string BankaGoruntule = "banka.goruntule";
    public const string BankaEkle = "banka.ekle";
    public const string BankaDuzenle = "banka.duzenle";
    public const string BankaSil = "banka.sil";

    // Butce
    public const string ButceGoruntule = "butce.goruntule";
    public const string ButceEkle = "butce.ekle";
    public const string ButceDuzenle = "butce.duzenle";
    public const string ButceSil = "butce.sil";

    // Muhasebe
    public const string MuhasebeGoruntule = "muhasebe.goruntule";
    public const string MuhasebeEkle = "muhasebe.ekle";
    public const string MuhasebeDuzenle = "muhasebe.duzenle";

    // Arac
    public const string AracGoruntule = "arac.goruntule";
    public const string AracEkle = "arac.ekle";
    public const string AracDuzenle = "arac.duzenle";
    public const string AracSil = "arac.sil";

    // Sofor
    public const string SoforGoruntule = "sofor.goruntule";
    public const string SoforEkle = "sofor.ekle";
    public const string SoforDuzenle = "sofor.duzenle";
    public const string SoforSil = "sofor.sil";

    // Guzergah
    public const string GuzergahGoruntule = "guzergah.goruntule";
    public const string GuzergahEkle = "guzergah.ekle";
    public const string GuzergahDuzenle = "guzergah.duzenle";

    // Servis Calisma
    public const string ServisGoruntule = "servis.goruntule";
    public const string ServisEkle = "servis.ekle";
    public const string ServisDuzenle = "servis.duzenle";

    // Masraf
    public const string MasrafGoruntule = "masraf.goruntule";
    public const string MasrafEkle = "masraf.ekle";
    public const string MasrafDuzenle = "masraf.duzenle";

    // Satis
    public const string SatisGoruntule = "satis.goruntule";
    public const string SatisEkle = "satis.ekle";
    public const string SatisDuzenle = "satis.duzenle";
    public const string SatisSil = "satis.sil";

    // Raporlar
    public const string RaporGoruntule = "rapor.goruntule";
    public const string RaporExport = "rapor.export";

    // Ayarlar
    public const string AyarlarGoruntule = "ayarlar.goruntule";
    public const string AyarlarDuzenle = "ayarlar.duzenle";

    // Yonetim
    public const string KullaniciYonetimi = "kullanici.yonetim";
    public const string RolYonetimi = "rol.yonetim";
    public const string YedeklemeGoruntule = "yedekleme.goruntule";
    public const string YedeklemeOlustur = "yedekleme.olustur";
    public const string LisansYonetimi = "lisans.yonetim";
    public const string GuncellemeYonetimi = "guncelleme.yonetim";

    /// <summary>
    /// Tum yetki kodlarini dondurur
    /// </summary>
    public static List<string> GetAll()
    {
        var yetkiler = new List<string>
        {
            Dashboard,
            // Menu yetkileri
            MenuAnaSayfa, MenuBelgeUyarilari,
            MenuCariModulu, MenuCariler, MenuKesilenFaturalar, MenuGelenFaturalar,
            MenuFiloServis, MenuAraclar, MenuGuzergahlar, MenuServisCalismalari, MenuTopluCalisma, MenuMasrafKalemleri, MenuAracMasraflari,
            MenuMuhasebe, MenuMuhasebeDashboard, MenuHesapPlani, MenuMuhasebeFisleri, MenuMuhasebeRaporlari, MenuMaliAnaliz,
            MenuPersonel, MenuPersonelListesi, MenuMaasYonetimi, MenuIzinYonetimi,
            MenuFaturaModulu, MenuFaturalar, MenuFaturaHazirlik,
            MenuBankaKasa, MenuBankaHesaplari, MenuBankaHareketleri, MenuOdemeEslestirme,
            MenuRaporlar, MenuButceAnaliz, MenuOdemeYonetimi, MenuMaliAnalizRapor, MenuAylikChecklist, 
            MenuOzmalAracRaporu, MenuKiralikAracRaporu, MenuKomisyonRaporu, MenuServisRaporu, MenuFaturaOdemeRaporu, MenuAracMasrafRaporu, MenuCariEkstre,
            MenuSatisModulu, MenuSatisDashboard, MenuPiyasaArastirma, MenuYeniIlan, MenuSatisPersoneli,
            MenuAyarlar, MenuFirmaYonetimi, MenuVeritabaniAyarlari, MenuServisRaporlari, MenuLisansBilgileri, 
            MenuKullaniciYonetimi, MenuRolYonetimi, MenuPiyasaKaynaklari, MenuSistemDurumu, MenuAktiviteLog, MenuYedekleme, MenuGuncelleme,
            // Islem yetkileri
            CariGoruntule, CariEkle, CariDuzenle, CariSil,
            FaturaGoruntule, FaturaEkle, FaturaDuzenle, FaturaSil,
            BankaGoruntule, BankaEkle, BankaDuzenle, BankaSil,
            ButceGoruntule, ButceEkle, ButceDuzenle, ButceSil,
            MuhasebeGoruntule, MuhasebeEkle, MuhasebeDuzenle,
            AracGoruntule, AracEkle, AracDuzenle, AracSil,
            SoforGoruntule, SoforEkle, SoforDuzenle, SoforSil,
            GuzergahGoruntule, GuzergahEkle, GuzergahDuzenle,
            ServisGoruntule, ServisEkle, ServisDuzenle,
            MasrafGoruntule, MasrafEkle, MasrafDuzenle,
            SatisGoruntule, SatisEkle, SatisDuzenle, SatisSil,
            RaporGoruntule, RaporExport,
            AyarlarGoruntule, AyarlarDuzenle,
            KullaniciYonetimi, RolYonetimi,
            YedeklemeGoruntule, YedeklemeOlustur,
            LisansYonetimi, GuncellemeYonetimi
        };
        return yetkiler;
    }

    /// <summary>
    /// Menu bazli yetkileri gruplar - Rol yonetimi icin
    /// </summary>
    public static List<MenuYetkiGrup> GetMenuYetkileri()
    {
        return new List<MenuYetkiGrup>
        {
            new("Ana Sayfa", "bi-house-door", new List<YetkiTanim>
            {
                new(MenuAnaSayfa, "Ana Sayfa", "bi-house-door-fill"),
                new(MenuBelgeUyarilari, "Belge Uyarilari", "bi-exclamation-triangle"),
            }),
            
            new("Cari Modulu", "bi-people", new List<YetkiTanim>
            {
                new(MenuCariModulu, "Cari Modulu (Ana Menu)", "bi-people-fill"),
                new(MenuCariler, "Cariler", "bi-person-lines-fill"),
                new(MenuKesilenFaturalar, "Kesilen Faturalar", "bi-file-earmark-arrow-up"),
                new(MenuGelenFaturalar, "Gelen Faturalar", "bi-file-earmark-arrow-down"),
            }),
            
            new("Filo Servis", "bi-truck", new List<YetkiTanim>
            {
                new(MenuFiloServis, "Filo Servis (Ana Menu)", "bi-truck"),
                new(MenuAraclar, "Araclar", "bi-car-front-fill"),
                new(MenuGuzergahlar, "Guzergahlar", "bi-signpost-split-fill"),
                new(MenuServisCalismalari, "Servis Calismalari", "bi-calendar-check-fill"),
                new(MenuTopluCalisma, "Toplu Calisma Girisi", "bi-list-check"),
                new(MenuMasrafKalemleri, "Masraf Kalemleri", "bi-list-task"),
                new(MenuAracMasraflari, "Arac Masraflari", "bi-receipt"),
            }),
            
            new("Muhasebe", "bi-journal-text", new List<YetkiTanim>
            {
                new(MenuMuhasebe, "Muhasebe (Ana Menu)", "bi-journal-text"),
                new(MenuMuhasebeDashboard, "Muhasebe Dashboard", "bi-speedometer2"),
                new(MenuHesapPlani, "Hesap Plani", "bi-list-nested"),
                new(MenuMuhasebeFisleri, "Muhasebe Fisleri", "bi-receipt"),
                new(MenuMuhasebeRaporlari, "Muhasebe Raporlari", "bi-file-earmark-bar-graph"),
                new(MenuMaliAnaliz, "Mali Analiz", "bi-graph-up"),
            }),
            
            new("Personel", "bi-people", new List<YetkiTanim>
            {
                new(MenuPersonel, "Personel (Ana Menu)", "bi-people-fill"),
                new(MenuPersonelListesi, "Personel Listesi", "bi-people"),
                new(MenuMaasYonetimi, "Maas Yonetimi", "bi-cash-stack"),
                new(MenuIzinYonetimi, "Izin Yonetimi", "bi-calendar-check"),
            }),
            
            new("Fatura Modulu", "bi-receipt", new List<YetkiTanim>
            {
                new(MenuFaturaModulu, "Fatura Modulu (Ana Menu)", "bi-receipt"),
                new(MenuFaturalar, "Faturalar", "bi-file-earmark-text-fill"),
                new(MenuFaturaHazirlik, "Fatura Hazirlik", "bi-clipboard-check"),
            }),
            
            new("Banka / Kasa", "bi-bank", new List<YetkiTanim>
            {
                new(MenuBankaKasa, "Banka/Kasa (Ana Menu)", "bi-bank"),
                new(MenuBankaHesaplari, "Banka Hesaplari", "bi-bank2"),
                new(MenuBankaHareketleri, "Hareketler", "bi-arrow-left-right"),
                new(MenuOdemeEslestirme, "Odeme Eslestirme", "bi-link-45deg"),
            }),
            
            new("Raporlar", "bi-bar-chart", new List<YetkiTanim>
            {
                new(MenuRaporlar, "Raporlar (Ana Menu)", "bi-bar-chart-fill"),
                new(MenuButceAnaliz, "Butce Analiz", "bi-wallet2"),
                new(MenuOdemeYonetimi, "Odeme Yonetimi", "bi-credit-card"),
                new(MenuMaliAnalizRapor, "Mali Analiz", "bi-graph-up-arrow"),
                new(MenuAylikChecklist, "Aylik Checklist", "bi-clipboard-check"),
                new(MenuOzmalAracRaporu, "Ozmal Arac Raporu", "bi-truck"),
                new(MenuKiralikAracRaporu, "Kiralik Arac Raporu", "bi-building"),
                new(MenuKomisyonRaporu, "Komisyon Raporu", "bi-percent"),
                new(MenuServisRaporu, "Servis Raporu", "bi-file-earmark-bar-graph"),
                new(MenuFaturaOdemeRaporu, "Fatura Odeme", "bi-file-earmark-text"),
                new(MenuAracMasrafRaporu, "Arac Masraf", "bi-file-earmark-spreadsheet"),
                new(MenuCariEkstre, "Cari Ekstre", "bi-file-earmark-ruled"),
            }),
            
            new("Satis Modulu", "bi-car-front", new List<YetkiTanim>
            {
                new(MenuSatisModulu, "Satis Modulu (Ana Menu)", "bi-car-front-fill"),
                new(MenuSatisDashboard, "Satis Dashboard", "bi-speedometer2"),
                new(MenuPiyasaArastirma, "Piyasa Arastirma", "bi-search"),
                new(MenuYeniIlan, "Yeni Ilan", "bi-plus-circle"),
                new(MenuSatisPersoneli, "Satis Personeli", "bi-people"),
            }),
            
            new("Ayarlar", "bi-gear", new List<YetkiTanim>
            {
                new(MenuAyarlar, "Ayarlar (Ana Menu)", "bi-gear-fill"),
                new(MenuFirmaYonetimi, "Firma Yonetimi", "bi-building"),
                new(MenuVeritabaniAyarlari, "Veritabani Ayarlari", "bi-database-gear"),
                new(MenuServisRaporlari, "Servis Raporlari", "bi-file-earmark-bar-graph"),
                new(MenuLisansBilgileri, "Lisans Bilgileri", "bi-key"),
                new(MenuKullaniciYonetimi, "Kullanici Yonetimi", "bi-person-badge"),
                new(MenuRolYonetimi, "Rol Yonetimi", "bi-shield-check"),
                new(MenuPiyasaKaynaklari, "Piyasa Kaynaklari", "bi-globe"),
                new(MenuSistemDurumu, "Sistem Durumu", "bi-heart-pulse"),
                new(MenuAktiviteLog, "Aktivite Logu", "bi-clock-history"),
                new(MenuYedekleme, "Yedekleme", "bi-database-fill-gear"),
                new(MenuGuncelleme, "Uygulama Guncelleme", "bi-cloud-arrow-down"),
            }),
        };
    }

    /// <summary>
    /// Islem bazli yetkileri gruplar
    /// </summary>
    public static Dictionary<string, List<YetkiTanim>> GetIslemYetkileri()
    {
        return new Dictionary<string, List<YetkiTanim>>
        {
            ["Cari Islemleri"] = new()
            {
                new(CariGoruntule, "Goruntule", "bi-eye"),
                new(CariEkle, "Ekle", "bi-plus"),
                new(CariDuzenle, "Duzenle", "bi-pencil"),
                new(CariSil, "Sil", "bi-trash"),
            },
            ["Fatura Islemleri"] = new()
            {
                new(FaturaGoruntule, "Goruntule", "bi-eye"),
                new(FaturaEkle, "Ekle", "bi-plus"),
                new(FaturaDuzenle, "Duzenle", "bi-pencil"),
                new(FaturaSil, "Sil", "bi-trash"),
            },
            ["Banka/Kasa Islemleri"] = new()
            {
                new(BankaGoruntule, "Goruntule", "bi-eye"),
                new(BankaEkle, "Ekle", "bi-plus"),
                new(BankaDuzenle, "Duzenle", "bi-pencil"),
                new(BankaSil, "Sil", "bi-trash"),
            },
            ["Butce Islemleri"] = new()
            {
                new(ButceGoruntule, "Goruntule", "bi-eye"),
                new(ButceEkle, "Ekle", "bi-plus"),
                new(ButceDuzenle, "Duzenle", "bi-pencil"),
                new(ButceSil, "Sil", "bi-trash"),
            },
            ["Muhasebe Islemleri"] = new()
            {
                new(MuhasebeGoruntule, "Goruntule", "bi-eye"),
                new(MuhasebeEkle, "Ekle", "bi-plus"),
                new(MuhasebeDuzenle, "Duzenle", "bi-pencil"),
            },
            ["Arac Islemleri"] = new()
            {
                new(AracGoruntule, "Goruntule", "bi-eye"),
                new(AracEkle, "Ekle", "bi-plus"),
                new(AracDuzenle, "Duzenle", "bi-pencil"),
                new(AracSil, "Sil", "bi-trash"),
            },
            ["Sofor Islemleri"] = new()
            {
                new(SoforGoruntule, "Goruntule", "bi-eye"),
                new(SoforEkle, "Ekle", "bi-plus"),
                new(SoforDuzenle, "Duzenle", "bi-pencil"),
                new(SoforSil, "Sil", "bi-trash"),
            },
            ["Guzergah Islemleri"] = new()
            {
                new(GuzergahGoruntule, "Goruntule", "bi-eye"),
                new(GuzergahEkle, "Ekle", "bi-plus"),
                new(GuzergahDuzenle, "Duzenle", "bi-pencil"),
            },
            ["Servis Islemleri"] = new()
            {
                new(ServisGoruntule, "Goruntule", "bi-eye"),
                new(ServisEkle, "Ekle", "bi-plus"),
                new(ServisDuzenle, "Duzenle", "bi-pencil"),
            },
            ["Masraf Islemleri"] = new()
            {
                new(MasrafGoruntule, "Goruntule", "bi-eye"),
                new(MasrafEkle, "Ekle", "bi-plus"),
                new(MasrafDuzenle, "Duzenle", "bi-pencil"),
            },
            ["Satis Islemleri"] = new()
            {
                new(SatisGoruntule, "Goruntule", "bi-eye"),
                new(SatisEkle, "Ekle", "bi-plus"),
                new(SatisDuzenle, "Duzenle", "bi-pencil"),
                new(SatisSil, "Sil", "bi-trash"),
            },
            ["Rapor Islemleri"] = new()
            {
                new(RaporGoruntule, "Goruntule", "bi-eye"),
                new(RaporExport, "Export", "bi-download"),
            },
            ["Sistem Yonetimi"] = new()
            {
                new(AyarlarGoruntule, "Ayarlar Goruntule", "bi-gear"),
                new(AyarlarDuzenle, "Ayarlar Duzenle", "bi-gear-fill"),
                new(KullaniciYonetimi, "Kullanici Yonetimi", "bi-people"),
                new(RolYonetimi, "Rol Yonetimi", "bi-shield-check"),
                new(YedeklemeGoruntule, "Yedekleme Goruntule", "bi-database"),
                new(YedeklemeOlustur, "Yedekleme Olustur", "bi-database-add"),
                new(LisansYonetimi, "Lisans Yonetimi", "bi-key"),
                new(GuncellemeYonetimi, "Guncelleme Yonetimi", "bi-cloud-arrow-down"),
            },
        };
    }

    /// <summary>
    /// Eski GetGrouped metodu - geriye uyumluluk icin
    /// </summary>
    public static Dictionary<string, List<YetkiTanim>> GetGrouped()
    {
        return GetIslemYetkileri();
    }
}

/// <summary>
/// Menu bazli yetki grubu
/// </summary>
public class MenuYetkiGrup
{
    public string GrupAdi { get; set; }
    public string Icon { get; set; }
    public List<YetkiTanim> Yetkiler { get; set; }

    public MenuYetkiGrup(string grupAdi, string icon, List<YetkiTanim> yetkiler)
    {
        GrupAdi = grupAdi;
        Icon = icon;
        Yetkiler = yetkiler;
    }
}

public class YetkiTanim
{
    public string Kod { get; set; }
    public string Adi { get; set; }
    public string Icon { get; set; }

    public YetkiTanim(string kod, string adi, string icon)
    {
        Kod = kod;
        Adi = adi;
        Icon = icon;
    }
}

#endregion
