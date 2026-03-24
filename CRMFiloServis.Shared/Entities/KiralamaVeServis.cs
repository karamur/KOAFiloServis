using System.ComponentModel.DataAnnotations;

namespace CRMFiloServis.Shared.Entities;

/// <summary>
/// Kiralanmýþ araç kayýtlarý (dýþarýdan kiralanan araçlar)
/// </summary>
public class KiralamaArac : BaseEntity
{
    [Required]
    public int FirmaId { get; set; }

    /// <summary>
    /// Kiralayan cari (araç sahibi)
    /// </summary>
    [Required]
    public int KiralayýcýCariId { get; set; }

    [Required]
    [StringLength(15)]
    public string Plaka { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Marka { get; set; }

    [StringLength(50)]
    public string? Model { get; set; }

    public int? ModelYili { get; set; }

    public AracTipi AracTipi { get; set; }

    public int? KoltukSayisi { get; set; }

    /// <summary>
    /// Kiralama baþlangýç tarihi
    /// </summary>
    [Required]
    public DateTime KiralamaBaslangic { get; set; }

    /// <summary>
    /// Kiralama bitiþ tarihi (null ise süresiz)
    /// </summary>
    public DateTime? KiralamaBitis { get; set; }

    /// <summary>
    /// Günlük kira bedeli
    /// </summary>
    public decimal? GunlukKiraBedeli { get; set; }

    /// <summary>
    /// Sefer baþýna kira bedeli
    /// </summary>
    public decimal? SeferBasinaKiraBedeli { get; set; }

    /// <summary>
    /// Aylýk sabit kira bedeli
    /// </summary>
    public decimal? AylikKiraBedeli { get; set; }

    /// <summary>
    /// Komisyon oraný (%)
    /// </summary>
    public decimal? KomisyonOrani { get; set; }

    /// <summary>
    /// Sabit komisyon tutarý
    /// </summary>
    public decimal? SabitKomisyonTutari { get; set; }

    public string? SozlesmeNo { get; set; }

    public string? Notlar { get; set; }

    public bool Aktif { get; set; } = true;

    // Navigation
    public virtual Firma? Firma { get; set; }
    public virtual Cari? KiralayýcýCari { get; set; }
    public virtual ICollection<ServisCalismaKiralama> ServisCalismalari { get; set; } = new List<ServisCalismaKiralama>();
}

/// <summary>
/// Kiralanmýþ araçlarýn servis çalýþmalarý
/// (Hem kendi araçlarý hem kiralýk araçlar için ortak kayýt)
/// </summary>
public class ServisCalismaKiralama : BaseEntity
{
    [Required]
    public int FirmaId { get; set; }

    [Required]
    public DateTime CalismaTarihi { get; set; }

    [Required]
    public ServisTuru ServisTuru { get; set; }

    /// <summary>
    /// Araç türü (Kendi/Kiralýk)
    /// </summary>
    [Required]
    public AracSahiplikTuru AracSahiplikTuru { get; set; }

    /// <summary>
    /// Kendi aracýmýz ise
    /// </summary>
    public int? AracId { get; set; }

    /// <summary>
    /// Kiralýk araç ise
    /// </summary>
    public int? KiralamaAracId { get; set; }

    [Required]
    public int SoforId { get; set; }

    [Required]
    public int GuzergahId { get; set; }

    /// <summary>
    /// Müþteri firma (Baþkasýnýn güzergahýnda çalýþýyorsak)
    /// </summary>
    public int? MusteriFirmaId { get; set; }

    /// <summary>
    /// Çalýþma fiyatý
    /// </summary>
    public decimal? CalismaBedeli { get; set; }

    /// <summary>
    /// Araç kira bedeli (kiralýk araç ise)
    /// </summary>
    public decimal? AracKiraBedeli { get; set; }

    /// <summary>
    /// Komisyon tutarý (varsa)
    /// </summary>
    public decimal? KomisyonTutari { get; set; }

    /// <summary>
    /// Net kazanç (Çalýþma bedeli - Kira - Komisyon)
    /// </summary>
    public decimal? NetKazanc { get; set; }

    public int? KmBaslangic { get; set; }
    public int? KmBitis { get; set; }
    public int? ToplamKm { get; set; }

    public TimeSpan? BaslangicSaati { get; set; }
    public TimeSpan? BitisSaati { get; set; }

    public bool ArizaOlduMu { get; set; }
    public string? ArizaAciklamasi { get; set; }

    public CalismaDurum Durum { get; set; } = CalismaDurum.Tamamlandi;

    public string? Notlar { get; set; }

    // Navigation
    public virtual Firma? Firma { get; set; }
    public virtual Arac? Arac { get; set; }
    public virtual KiralamaArac? KiralamaArac { get; set; }
    public virtual Sofor? Sofor { get; set; }
    public virtual Guzergah? Guzergah { get; set; }
    public virtual Firma? MusteriFirma { get; set; }
}

/// <summary>
/// Araç sahiplik türü
/// </summary>
public enum AracSahiplikTuru
{
    /// <summary>
    /// Kendi aracýmýz
    /// </summary>
    KendiArac = 1,

    /// <summary>
    /// Kiralýk araç
    /// </summary>
    KiralýkArac = 2
}

/// <summary>
/// Servis çalýþma puantaj raporu (Excel için)
/// </summary>
public class ServisCalismaRapor
{
    public DateTime Tarih { get; set; }
    public string? Plaka { get; set; }
    public string? AracSahiplik { get; set; } // "Kendi" veya "Kiralýk"
    public string? SoforAdi { get; set; }
    public string? GuzergahAdi { get; set; }
    public string? MusteriFirma { get; set; } // Baþka firma için çalýþýyorsak
    public string? ServisTuru { get; set; }
    public decimal? CalismaBedeli { get; set; }
    public decimal? AracKiraBedeli { get; set; }
    public decimal? KomisyonTutari { get; set; }
    public decimal? NetKazanc { get; set; }
    public int? ToplamKm { get; set; }
    public string? BaslangicSaati { get; set; }
    public string? BitisSaati { get; set; }
    public string? Durum { get; set; }
    public string? Notlar { get; set; }
}
