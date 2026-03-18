namespace CRMFiloServis.Shared.Entities;

/// <summary>
/// Banka hesapları
/// </summary>
public class BankaHesap : BaseEntity
{
    public string HesapKodu { get; set; } = string.Empty;
    public string HesapAdi { get; set; } = string.Empty;
    public HesapTipi HesapTipi { get; set; }
    public string? BankaAdi { get; set; }
    public string? SubeAdi { get; set; }
    public string? SubeKodu { get; set; }
    public string? HesapNo { get; set; }
    public string? Iban { get; set; }
    public string? ParaBirimi { get; set; } = "TRY";
    public decimal AcilisBakiye { get; set; } = 0;
    public bool Aktif { get; set; } = true;
    public string? Notlar { get; set; }

    // Navigation Properties
    public virtual ICollection<BankaKasaHareket> Hareketler { get; set; } = new List<BankaKasaHareket>();
}

public enum HesapTipi
{
    Kasa = 1,
    VadesizHesap = 2,
    VadeliHesap = 3,
    KrediHesabi = 4
}
