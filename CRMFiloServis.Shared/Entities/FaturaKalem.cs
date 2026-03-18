namespace CRMFiloServis.Shared.Entities;

/// <summary>
/// Fatura kalemleri
/// </summary>
public class FaturaKalem : BaseEntity
{
    public int SiraNo { get; set; }
    public string Aciklama { get; set; } = string.Empty;
    public decimal Miktar { get; set; } = 1;
    public string Birim { get; set; } = "Adet";
    public decimal BirimFiyat { get; set; }
    public decimal KdvOrani { get; set; } = 20;
    public decimal KdvTutar { get; set; }
    public decimal ToplamTutar { get; set; }

    // Foreign Key
    public int FaturaId { get; set; }

    // Navigation Property
    public virtual Fatura Fatura { get; set; } = null!;
}
