using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMFiloServis.Shared.Entities;

/// <summary>
/// Müþteriye araç kiralama kaydý
/// Þirketin kendi araçlarýný müþterilere kiralamasý
/// </summary>
public class MusteriKiralama : BaseEntity
{
    [Required]
    public int FirmaId { get; set; }

    /// <summary>
    /// Kiralayan müþteri
    /// </summary>
    [Required]
    public int MusteriId { get; set; }

    /// <summary>
    /// Kiralanan araç
    /// </summary>
    [Required]
    public int AracId { get; set; }

    /// <summary>
    /// Kiralama baþlangýç tarihi ve saati
    /// </summary>
    [Required]
    public DateTime BaslangicTarihi { get; set; }

    /// <summary>
    /// Planlanan bitiþ tarihi
    /// </summary>
    [Required]
    public DateTime PlanlananBitisTarihi { get; set; }

    /// <summary>
    /// Gerçek teslim tarihi
    /// </summary>
    public DateTime? GercekBitisTarihi { get; set; }

    /// <summary>
    /// Baþlangýç kilometresi
    /// </summary>
    public int? BaslangicKm { get; set; }

    /// <summary>
    /// Bitiþ kilometresi
    /// </summary>
    public int? BitisKm { get; set; }

    /// <summary>
    /// Günlük kira bedeli
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal GunlukFiyat { get; set; }

    /// <summary>
    /// Toplam tutar
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ToplamTutar { get; set; }

    /// <summary>
    /// Alýnan depozito
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? Depozito { get; set; }

    /// <summary>
    /// Kiralama durumu
    /// </summary>
    public KiralamaDurumu Durum { get; set; } = KiralamaDurumu.Rezervasyon;

    /// <summary>
    /// Ödeme durumu
    /// </summary>
    public KiralamaOdemeDurumu OdemeDurumu { get; set; } = KiralamaOdemeDurumu.Beklemede;

    /// <summary>
    /// Teslim alan personel
    /// </summary>
    public int? TeslimAlanPersonelId { get; set; }

    /// <summary>
    /// Teslim eden personel
    /// </summary>
    public int? TeslimEdenPersonelId { get; set; }

    /// <summary>
    /// Notlar
    /// </summary>
    [StringLength(500)]
    public string? Notlar { get; set; }

    /// <summary>
    /// Sözleþme numarasý
    /// </summary>
    [StringLength(50)]
    public string? SozlesmeNo { get; set; }
}

public enum KiralamaDurumu
{
    Rezervasyon = 0,
    Aktif = 1,
    Tamamlandi = 2,
    IptalEdildi = 3
}

public enum KiralamaOdemeDurumu
{
    Beklemede = 0,
    KismiOdendi = 1,
    Odendi = 2,
    IadeEdildi = 3
}
