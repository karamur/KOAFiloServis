namespace CRMFiloServis.Shared.Entities;

/// <summary>
/// Fatura bilgileri
/// </summary>
public class Fatura : BaseEntity
{
    public string FaturaNo { get; set; } = string.Empty;
    public DateTime FaturaTarihi { get; set; }
    public DateTime? VadeTarihi { get; set; }
    public FaturaTipi FaturaTipi { get; set; }
    public FaturaDurum Durum { get; set; } = FaturaDurum.Beklemede;

    // Tutarlar
    public decimal AraToplam { get; set; }
    public decimal KdvOrani { get; set; } = 20;
    public decimal KdvTutar { get; set; }
    public decimal GenelToplam { get; set; }
    public decimal OdenenTutar { get; set; } = 0;
    public decimal KalanTutar => GenelToplam - OdenenTutar;

    public string? Aciklama { get; set; }
    public string? Notlar { get; set; }

    // Foreign Key
    public int CariId { get; set; }

    // Navigation Properties
    public virtual Cari Cari { get; set; } = null!;
    public virtual ICollection<FaturaKalem> FaturaKalemleri { get; set; } = new List<FaturaKalem>();
    public virtual ICollection<OdemeEslestirme> OdemeEslestirmeleri { get; set; } = new List<OdemeEslestirme>();
}

public enum FaturaTipi
{
    SatisFaturasi = 1,      // Müþteriye kesilen
    AlisFaturasi = 2,       // Tedarikçiden alýnan
    SatisIadeFaturasi = 3,
    AlisIadeFaturasi = 4
}

public enum FaturaDurum
{
    Beklemede = 1,
    KismiOdendi = 2,
    Odendi = 3,
    IptalEdildi = 4
}
