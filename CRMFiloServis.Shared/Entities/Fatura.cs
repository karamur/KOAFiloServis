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
    
    // E-Fatura / E-Arsiv
    public EFaturaTipi EFaturaTipi { get; set; } = EFaturaTipi.EArsiv;
    public FaturaYonu FaturaYonu { get; set; } = FaturaYonu.Giden;
    public string? EttnNo { get; set; } // E-Fatura ETTN numarasi
    public string? GibKodu { get; set; } // GIB onay kodu
    public DateTime? GibOnayTarihi { get; set; }
    public string? ImportKaynak { get; set; } // Excel, Luca, Manuel, XML

    // Firma bilgisi
    public int? FirmaId { get; set; }
    public virtual Firma? Firma { get; set; }

    // Tutarlar
    public decimal AraToplam { get; set; }
    public decimal IskontoTutar { get; set; } = 0;
    public decimal KdvOrani { get; set; } = 20;
    public decimal KdvTutar { get; set; }
    public decimal GenelToplam { get; set; }
    public decimal OdenenTutar { get; set; } = 0;
    public decimal KalanTutar => GenelToplam - OdenenTutar;

    // Tevkifat
    public bool TevkifatliMi { get; set; } = false;
    public decimal TevkifatOrani { get; set; } = 0; // Ör: 5/10 için 50, 9/10 için 90
    public string? TevkifatKodu { get; set; } // GİB tevkifat kodu (601, 602, vb.)
    public decimal TevkifatTutar { get; set; } = 0;
    public decimal TevkifatliKdvTutar => KdvTutar - TevkifatTutar;

    public string? Aciklama { get; set; }
    public string? Notlar { get; set; }

    // Muhasebe Fişi oluşturuldu mu?
    public bool MuhasebeFisiOlusturuldu { get; set; } = false;
    public int? MuhasebeFisId { get; set; }

    // Foreign Key
    public int CariId { get; set; }

    // Navigation Properties
    public virtual Cari Cari { get; set; } = null!;
    public virtual ICollection<FaturaKalem> FaturaKalemleri { get; set; } = new List<FaturaKalem>();
    public virtual ICollection<OdemeEslestirme> OdemeEslestirmeleri { get; set; } = new List<OdemeEslestirme>();
}

public enum FaturaTipi
{
    SatisFaturasi = 1,      // Musteriye kesilen
    AlisFaturasi = 2,       // Tedarikçiden alinan
    SatisIadeFaturasi = 3,
    AlisIadeFaturasi = 4,
    TevkifatliFatura = 5    // Tevkifatlı fatura
}

public enum FaturaDurum
{
    Beklemede = 1,
    KismiOdendi = 2,
    Odendi = 3,
    IptalEdildi = 4
}

public enum EFaturaTipi
{
    EFatura = 1,    // E-Fatura (Tescilli mukellefler arasi)
    EArsiv = 2      // E-Arsiv Fatura
}

public enum FaturaYonu
{
    Giden = 1,      // Kesilen fatura
    Gelen = 2       // Alinan fatura
}
