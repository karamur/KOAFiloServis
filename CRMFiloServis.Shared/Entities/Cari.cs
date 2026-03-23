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
    public bool Aktif { get; set; } = true;

    // Borc/Alacak (hesaplanan degerler - veritabaninda saklanmaz)
    public decimal Borc { get; set; }
    public decimal Alacak { get; set; }

    // Muhasebe Hesap Eslestirme
    public int? MuhasebeHesapId { get; set; } // 120.xxx veya 320.xxx
    public virtual MuhasebeHesap? MuhasebeHesap { get; set; }

    // Firma iliskisi (coklu firma destegi)
    public int? FirmaId { get; set; }
    public virtual Firma? Firma { get; set; }

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
