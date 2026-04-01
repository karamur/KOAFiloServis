using System.ComponentModel.DataAnnotations;

namespace CRMFiloServis.Shared.Entities;

/// <summary>
/// Bütçe Ödeme Kaydı
/// </summary>
public class BudgetOdeme : BaseEntity
{
    [Required]
    public DateTime OdemeTarihi { get; set; }

    [Required]
    public int OdemeAy { get; set; } // 1-12

    [Required]
    public int OdemeYil { get; set; }

    [Required]
    public string MasrafKalemi { get; set; } = string.Empty;

    public string? Aciklama { get; set; }

    [Required]
    public decimal Miktar { get; set; }

    // Firma bilgisi
    public int? FirmaId { get; set; }
    public virtual Firma? Firma { get; set; }

    // Taksit bilgileri
    public bool TaksitliMi { get; set; } = false;
    public int ToplamTaksitSayisi { get; set; } = 1;
    public int KacinciTaksit { get; set; } = 1;
    public Guid? TaksitGrupId { get; set; } // Aynı taksit grubundaki ödemeleri bağlar

    public DateTime? TaksitBaslangicAy { get; set; }
    public DateTime? TaksitBitisAy { get; set; }

    public OdemeDurum Durum { get; set; } = OdemeDurum.Bekliyor;

    public string? Notlar { get; set; }

    // Ödeme bilgileri - Kasa/Banka hareketi
    public DateTime? GercekOdemeTarihi { get; set; }
    public int? OdemeYapildigiHesapId { get; set; } // BankaHesap ID
    public decimal? OdenenTutar { get; set; }
    public string? OdemeNotu { get; set; }
    public int? BankaKasaHareketId { get; set; } // İlişkili hareket

    // Kesinti bilgileri (masraf, ceza, komisyon vb.)
    public decimal MasrafKesintisi { get; set; } = 0;
    public decimal CezaKesintisi { get; set; } = 0;
    public decimal DigerKesinti { get; set; } = 0;
    public string? KesintiAciklamasi { get; set; }

    // Fatura ile eşleştirme
    public int? FaturaId { get; set; }
    public bool FaturaIleKapatildi { get; set; } = false;

    // Navigation
    public virtual BankaHesap? OdemeYapildigiHesap { get; set; }
    public virtual Fatura? Fatura { get; set; }

    // Hesaplanan alanlar
    public int KalanTaksitSayisi => ToplamTaksitSayisi - KacinciTaksit;
    public decimal ToplamTaksitTutari => Miktar * ToplamTaksitSayisi;
    public bool OdenmisVeyaKapatilmis => Durum == OdemeDurum.Odendi || FaturaIleKapatildi;
    public decimal ToplamKesinti => MasrafKesintisi + CezaKesintisi + DigerKesinti;
    public decimal NetOdenenTutar => (OdenenTutar ?? Miktar) - ToplamKesinti;
}

public enum OdemeDurum
{
    Bekliyor = 1,
    Odendi = 2,
    Iptal = 3,
    Ertelendi = 4
}

/// <summary>
/// Bütçe Masraf Kalemleri
/// </summary>
public class BudgetMasrafKalemi : BaseEntity
{
    [Required]
    public string KalemAdi { get; set; } = string.Empty;

    public string? Kategori { get; set; }

    public string? Renk { get; set; } = "#007bff"; // Grafik rengi

    public string? Icon { get; set; } = "bi-cash";

    public bool Aktif { get; set; } = true;

    public int SiraNo { get; set; }
}
