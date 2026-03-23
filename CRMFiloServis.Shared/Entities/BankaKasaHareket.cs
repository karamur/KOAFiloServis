namespace CRMFiloServis.Shared.Entities;

/// <summary>
/// Banka/Kasa hareketleri
/// </summary>
public class BankaKasaHareket : BaseEntity
{
    public string IslemNo { get; set; } = string.Empty;
    public DateTime IslemTarihi { get; set; }
    public HareketTipi HareketTipi { get; set; }
    public decimal Tutar { get; set; }
    public string? Aciklama { get; set; }
    public string? BelgeNo { get; set; } // Dekont, makbuz no vb.
    public IslemKaynak IslemKaynak { get; set; } = IslemKaynak.Manuel;

    // Foreign Keys
    public int BankaHesapId { get; set; }
    public int? CariId { get; set; } // Ýliþkili cari varsa

    // Navigation Properties
    public virtual BankaHesap BankaHesap { get; set; } = null!;
    public virtual Cari? Cari { get; set; }
    public virtual ICollection<OdemeEslestirme> OdemeEslestirmeleri { get; set; } = new List<OdemeEslestirme>();
}

public enum HareketTipi
{
    Giris = 1,      // Tahsilat
    Cikis = 2       // Ödeme
}

public enum IslemKaynak
{
    Manuel = 1,
    FaturaOdeme = 2,
    FaturaTahsilat = 3,
    Havale = 4,
    Eft = 5,
    Nakit = 6,
    Butce = 7,
    EFatura = 8
}
