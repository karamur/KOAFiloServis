namespace KOAFiloServis.Shared.Entities;

/// <summary>
/// Aylık bordro kayıtları
/// </summary>
public class Bordro : BaseEntity
{
    public int Yil { get; set; }
    public int Ay { get; set; }
    public int? FirmaId { get; set; }
    public BordroTipi BordroTipi { get; set; } = BordroTipi.Normal;
    public DateTime? HesaplamaTarihi { get; set; }
    public DateTime? OnayTarihi { get; set; }
    public bool Onaylandi { get; set; } = false;
    public string? OnaylayanKullanici { get; set; }
    public string? Aciklama { get; set; }
    
    // Özet Bilgiler
    public int ToplamPersonelSayisi { get; set; }
    public decimal ToplamBrutMaas { get; set; }
    public decimal ToplamNetMaas { get; set; }
    public decimal ToplamSgkMatrahi { get; set; }
    public decimal ToplamEkOdeme { get; set; }
    public decimal GenelToplam => ToplamNetMaas + ToplamEkOdeme;
    
    // Navigation Properties
    public virtual Firma? Firma { get; set; }
    public virtual ICollection<BordroDetay> BordroDetaylar { get; set; } = new List<BordroDetay>();
    public virtual ICollection<MuhasebeFis> MuhasebeFisleri { get; set; } = new List<MuhasebeFis>();
    
    public string DonemeAdi => $"{Ay}/{Yil}";
}

/// <summary>
/// Bordro detay satırları (personel bazında)
/// </summary>
public class BordroDetay : BaseEntity
{
    public int BordroId { get; set; }
    public int PersonelId { get; set; }
    public int? FirmaId { get; set; }
    
    // Maaş Bilgileri
    public decimal BrutMaas { get; set; }
    public decimal NetMaas { get; set; }
    public decimal TopluMaas { get; set; } // Personelin toplu maaşı
    public decimal SgkMaasi { get; set; } // SGK'ya bildirilen
    public decimal EkOdeme { get; set; } // Toplu - SGK
    
    // Kesintiler
    public decimal SgkIssizlikKesinti { get; set; }
    public decimal GelirVergisi { get; set; }
    public decimal DamgaVergisi { get; set; }
    public decimal ToplamKesinti => SgkIssizlikKesinti + GelirVergisi + DamgaVergisi;
    
    // Ek Ödemeler
    public decimal YemekYardimi { get; set; }
    public decimal YolYardimi { get; set; }
    public decimal PrimTutar { get; set; }
    public decimal DigerEkOdeme { get; set; }
    public decimal ToplamEkOdeme => YemekYardimi + YolYardimi + PrimTutar + DigerEkOdeme;
    
    // Ödeme Durumu
    public bool BankaOdemesiYapildi { get; set; } = false;
    public DateTime? BankaOdemeTarihi { get; set; }
    public bool EkOdemeYapildi { get; set; } = false;
    public DateTime? EkOdemeTarihi { get; set; }
    
    public decimal ToplamOdenecek => NetMaas + ToplamEkOdeme + EkOdeme;

    public string? Notlar { get; set; }

    // UI Helper (NotMapped)
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool Secili { get; set; } = false;

    // Navigation Properties
    public virtual Bordro Bordro { get; set; } = null!;
    public virtual Sofor Personel { get; set; } = null!;
    public virtual Firma? Firma { get; set; }
}

/// <summary>
/// Bordro ödeme kayıtları (2 taksit sistemi için)
/// </summary>
public class BordroOdeme : BaseEntity
{
    public int BordroDetayId { get; set; }
    public OdemeTipi OdemeTipi { get; set; } // BankaOdemesi veya EkOdeme
    public DateTime OdemeTarihi { get; set; }
    public decimal OdemeTutari { get; set; }
    public OdemeSekli OdemeSekli { get; set; }
    public int? BankaHesapId { get; set; }
    public string? EvrakNo { get; set; }
    public string? Aciklama { get; set; }
    
    // Muhasebe Entegrasyonu
    public int? MuhasebeFisId { get; set; }
    
    // Navigation Properties
    public virtual BordroDetay BordroDetay { get; set; } = null!;
    public virtual BankaHesap? BankaHesap { get; set; }
    public virtual MuhasebeFis? MuhasebeFis { get; set; }
}

/// <summary>
/// Bordro ayarları
/// </summary>
public class BordroAyar : BaseEntity
{
    public int? FirmaId { get; set; }
    
    // Muhasebe Hesap Kodları
    public string PersonelMaasHesapKodu { get; set; } = "335"; // Personele Borçlar
    public string SgkPrimHesapKodu { get; set; } = "361"; // SGK Prim Borçları
    public string GelirVergisiHesapKodu { get; set; } = "360"; // Ödenecek Vergiler
    public string KasaHesapKodu { get; set; } = "100"; // Kasa
    public string BankaHesapKodu { get; set; } = "102"; // Banka
    public string PersonelAvansHesapKodu { get; set; } = "195"; // Personel Avansları (mahsup için)
    
    // Oran Ayarları (%)
    public decimal SgkIsciPayiOrani { get; set; } = 14; // SGK işçi payı %
    public decimal IssizlikIsciPayiOrani { get; set; } = 1; // İşsizlik işçi payı %
    public decimal DamgaVergisiOrani { get; set; } = 0.759M; // Damga vergisi %
    
    // ARGE Özel Ayarlar
    public bool ArgeSgkIsverenDestekVarMi { get; set; } = true;
    public decimal ArgeSgkIsverenDestekOrani { get; set; } = 100; // % tam destek
    
    // Navigation Properties
    public virtual Firma? Firma { get; set; }
}

public enum BordroTipi
{
    Normal = 1,
    Arge = 2,
    Diger = 99
}

public enum OdemeTipi
{
    BankaOdemesi = 1, // SGK maaşı (1. taksit)
    EkOdeme = 2 // Kalan tutar (2. taksit)
}

public enum OdemeSekli
{
    Nakit = 1,
    BankaTransfer = 2,
    Cek = 3,
    Senet = 4
}
