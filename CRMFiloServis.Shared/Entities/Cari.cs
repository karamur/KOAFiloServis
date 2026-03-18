namespace CRMFiloServis.Shared.Entities;

/// <summary>
/// Cari hesap (Müþteri/Tedarikçi/Firma)
/// </summary>
public class Cari : BaseEntity
{
    public string CariKodu { get; set; } = string.Empty;
    public string Unvan { get; set; } = string.Empty;
    public CariTipi CariTipi { get; set; }
    public string? VergiDairesi { get; set; }
    public string? VergiNo { get; set; }
    public string? Adres { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? YetkiliKisi { get; set; }
    public string? Notlar { get; set; }

    // Navigation Properties
    public virtual ICollection<Fatura> Faturalar { get; set; } = new List<Fatura>();
    public virtual ICollection<Guzergah> Guzergahlar { get; set; } = new List<Guzergah>();
    public virtual ICollection<BankaKasaHareket> BankaKasaHareketler { get; set; } = new List<BankaKasaHareket>();
}

public enum CariTipi
{
    Musteri = 1,
    Tedarikci = 2,
    MusteriTedarikci = 3
}
