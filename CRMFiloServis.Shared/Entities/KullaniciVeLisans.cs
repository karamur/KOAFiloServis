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

    public DateTime BaslangicTarihi { get; set; } = DateTime.Now;
    public DateTime BitisTarihi { get; set; } = DateTime.Now.AddDays(30);

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
    public LisansDurumu Durum => DateTime.Now > BitisTarihi ? LisansDurumu.SuresiDolmus : LisansDurumu.Aktif;
    public int KalanGun => Math.Max(0, (BitisTarihi.Date - DateTime.Now.Date).Days);
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
    public string YetkiKodu { get; set; } = string.Empty; // Ornek: "fatura.okuma", "fatura.yazma"

    public bool Izin { get; set; } = false;
}

/// <summary>
/// Yetki Tanimlari
/// </summary>
public static class Yetkiler
{
    // Genel
    public const string Dashboard = "dashboard";

    // Cari
    public const string CariOkuma = "cari.okuma";
    public const string CariYazma = "cari.yazma";
    public const string CariSilme = "cari.silme";

    // Fatura
    public const string FaturaOkuma = "fatura.okuma";
    public const string FaturaYazma = "fatura.yazma";
    public const string FaturaSilme = "fatura.silme";

    // Banka
    public const string BankaOkuma = "banka.okuma";
    public const string BankaYazma = "banka.yazma";

    // Muhasebe
    public const string MuhasebeOkuma = "muhasebe.okuma";
    public const string MuhasebeYazma = "muhasebe.yazma";

    // Raporlar
    public const string RaporOkuma = "rapor.okuma";
    public const string RaporExport = "rapor.export";

    // Ayarlar
    public const string AyarlarOkuma = "ayarlar.okuma";
    public const string AyarlarYazma = "ayarlar.yazma";

    // Satis
    public const string SatisOkuma = "satis.okuma";
    public const string SatisYazma = "satis.yazma";

    // Yonetim
    public const string KullaniciYonetimi = "kullanici.yonetim";
    public const string YedeklemeYonetimi = "yedekleme.yonetim";
    public const string LisansYonetimi = "lisans.yonetim";
}

#endregion
