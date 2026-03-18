namespace CRMFiloServis.Shared.Entities;

/// <summary>
/// Þoför bilgileri
/// </summary>
public class Sofor : BaseEntity
{
    public string SoforKodu { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;
    public string? TcKimlikNo { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? Adres { get; set; }
    public string? EhliyetNo { get; set; }
    public DateTime? EhliyetGecerlilikTarihi { get; set; }
    public DateTime? IseBaslamaTarihi { get; set; }
    public bool Aktif { get; set; } = true;
    public string? Notlar { get; set; }

    public string TamAd => $"{Ad} {Soyad}";

    // Navigation Properties
    public virtual ICollection<ServisCalisma> ServisCalismalari { get; set; } = new List<ServisCalisma>();
}
