namespace CRMFiloServis.Shared.Entities;

/// <summary>
/// Araç masraf giriþleri
/// </summary>
public class AracMasraf : BaseEntity
{
    public DateTime MasrafTarihi { get; set; }
    public decimal Tutar { get; set; }
    public string? Aciklama { get; set; }
    public string? BelgeNo { get; set; } // Fatura/Fiþ numarasý
    public bool ArizaKaynaklimi { get; set; } = false; // Arýza nedeniyle mi?

    // Foreign Keys
    public int AracId { get; set; }
    public int MasrafKalemiId { get; set; }
    public int? GuzergahId { get; set; } // Arýza kaynaklý personel ulaþým masraflarý için
    public int? ServisCalismaId { get; set; } // Ýlgili servis çalýþmasý

    // Navigation Properties
    public virtual Arac Arac { get; set; } = null!;
    public virtual MasrafKalemi MasrafKalemi { get; set; } = null!;
    public virtual Guzergah? Guzergah { get; set; }
    public virtual ServisCalisma? ServisCalisma { get; set; }
}
