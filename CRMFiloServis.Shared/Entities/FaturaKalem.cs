namespace CRMFiloServis.Shared.Entities;

/// <summary>
/// Fatura kalemleri
/// </summary>
public class FaturaKalem : BaseEntity
{
    public int SiraNo { get; set; }
    public string? UrunKodu { get; set; } // Ürün/Hizmet kodu
    public string Aciklama { get; set; } = string.Empty;
    public decimal Miktar { get; set; } = 1;
    public string Birim { get; set; } = "Adet";
    public decimal BirimFiyat { get; set; }
    public decimal IskontoOrani { get; set; } = 0;
    public decimal IskontoTutar { get; set; } = 0;
    public decimal KdvOrani { get; set; } = 20;
    public decimal KdvTutar { get; set; }
    public decimal ToplamTutar { get; set; }

    // Tevkifat (kalem bazında)
    public decimal TevkifatOrani { get; set; } = 0;
    public decimal TevkifatTutar { get; set; } = 0;

    // Muhasebe Hesap Eşleştirme (Gelir/Gider hesabı)
    public int? MuhasebeHesapId { get; set; }
    public virtual MuhasebeHesap? MuhasebeHesap { get; set; }

    // Foreign Key
    public int FaturaId { get; set; }

    // Navigation Property
    public virtual Fatura Fatura { get; set; } = null!;

    // Hesaplanan değerler
    public decimal NetTutar => (Miktar * BirimFiyat) - IskontoTutar;
    public decimal TevkifatliKdvTutar => KdvTutar - TevkifatTutar;
}
