namespace CRMFiloServis.Shared.Entities;

/// <summary>
/// Arac Evrak/Belge Yonetimi - Ruhsat, Sigorta, Muayene vb.
/// </summary>
public class AracEvrak : BaseEntity
{
    public int AracId { get; set; }
    public string EvrakKategorisi { get; set; } = string.Empty; // Ruhsat, Sigorta, Muayene, Kasko, SRC Belgesi vb.
    public string? EvrakAdi { get; set; }
    public string? Aciklama { get; set; }

    // Tarih bilgileri
    public DateTime? BaslangicTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }
    public DateTime? HatirlatmaTarihi { get; set; }

    // Tutar bilgileri
    public decimal? Tutar { get; set; }
    public string? SigortaSirketi { get; set; }
    public string? PoliceNo { get; set; }

    // Durum
    public EvrakDurum Durum { get; set; } = EvrakDurum.Aktif;
    public bool HatirlatmaAktif { get; set; } = true;
    public int HatirlatmaGunOnce { get; set; } = 15; // Kac gun once hatirlatilsin

    // Navigation
    public virtual Arac? Arac { get; set; }
    public virtual ICollection<AracEvrakDosya> Dosyalar { get; set; } = new List<AracEvrakDosya>();
}

/// <summary>
/// Arac Evrak Dosyalari - Birden fazla dosya eklenebilir
/// </summary>
public class AracEvrakDosya : BaseEntity
{
    public int AracEvrakId { get; set; }
    public string DosyaAdi { get; set; } = string.Empty;
    public string DosyaYolu { get; set; } = string.Empty;
    public string? DosyaTipi { get; set; } // pdf, jpg, png vb.
    public long DosyaBoyutu { get; set; }
    public string? Aciklama { get; set; }

    /// <summary>
    /// Mevcut versiyon numarası (1'den başlar)
    /// </summary>
    public int VersiyonNo { get; set; } = 1;

    /// <summary>
    /// Son değişiklik notu
    /// </summary>
    public string? SonDegisiklikNotu { get; set; }

    // Navigation
    public virtual AracEvrak? AracEvrak { get; set; }

    /// <summary>
    /// Versiyon geçmişi - önceki versiyonlar
    /// </summary>
    public virtual ICollection<AracEvrakDosyaVersiyon> Versiyonlar { get; set; } = new List<AracEvrakDosyaVersiyon>();
}

public enum EvrakDurum
{
    Aktif = 1,
    Pasif = 2,
    SuresiDolmus = 3
}

/// <summary>
/// Evrak Kategorileri - Varsayilan kategoriler
/// </summary>
public static class EvrakKategorileri
{
    public const string Ruhsat = "Ruhsat";
    public const string TrafikSigortasi = "Trafik Sigortasi";
    public const string Kasko = "Kasko";
    public const string Muayene = "Muayene";
    public const string SRCBelgesi = "SRC Belgesi";
    public const string YetkiBelgesi = "Yetki Belgesi";
    public const string TasimaIzinBelgesi = "Tasima Izin Belgesi";
    public const string EmisyonBelgesi = "Emisyon Belgesi";
    public const string Diger = "Diger";

    public static readonly string[] TumKategoriler = new[]
    {
        Ruhsat, TrafikSigortasi, Kasko, Muayene, SRCBelgesi, 
        YetkiBelgesi, TasimaIzinBelgesi, EmisyonBelgesi, Diger
    };
}
