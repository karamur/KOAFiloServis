namespace CRMFiloServis.Shared.Entities;

/// <summary>
/// Güzergah bilgileri (Firma bazlý)
/// </summary>
public class Guzergah : BaseEntity
{
    public string GuzergahKodu { get; set; } = string.Empty;
    public string GuzergahAdi { get; set; } = string.Empty;
    public string? BaslangicNoktasi { get; set; }
    public string? BitisNoktasi { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal? Mesafe { get; set; } // km
    public int? TahminiSure { get; set; } // dakika
    public bool Aktif { get; set; } = true;
    public string? Notlar { get; set; }

    // Foreign Key
    public int CariId { get; set; }

    // Navigation Properties
    public virtual Cari Cari { get; set; } = null!;
    public virtual ICollection<ServisCalisma> ServisCalismalari { get; set; } = new List<ServisCalisma>();
    public virtual ICollection<AracMasraf> AracMasraflari { get; set; } = new List<AracMasraf>();
}
