using System.ComponentModel.DataAnnotations;

namespace CRMFiloServis.Shared.Entities;

/// <summary>
/// Muhasebe Hesap Plani - Tek Duzen Hesap Plani
/// </summary>
public class MuhasebeHesap : BaseEntity
{
    [Required]
    [StringLength(10)]
    public string HesapKodu { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string HesapAdi { get; set; } = string.Empty;

    public HesapTuru HesapTuru { get; set; }
    public HesapGrubu HesapGrubu { get; set; }

    public int? UstHesapId { get; set; }
    public virtual MuhasebeHesap? UstHesap { get; set; }

    public bool AltHesapVar { get; set; } = false;
    public bool Aktif { get; set; } = true;
    public bool SistemHesabi { get; set; } = false; // Silinemeyen sistem hesaplari

    public string? Aciklama { get; set; }

    // Navigation
    public virtual ICollection<MuhasebeFisKalem> FisKalemleri { get; set; } = new List<MuhasebeFisKalem>();
}

public enum HesapTuru
{
    Aktif = 1,      // Varlik hesaplari (1-2)
    Pasif = 2,      // Kaynak hesaplari (3-4-5)
    Gelir = 3,      // Gelir hesaplari (6)
    Gider = 4,      // Gider hesaplari (7)
    Maliyet = 5,    // Maliyet hesaplari (7)
    Nazim = 6       // Nazim hesaplar (9)
}

public enum HesapGrubu
{
    // 1 - Donen Varliklar
    DonenVarliklar = 1,
    // 2 - Duran Varliklar
    DuranVarliklar = 2,
    // 3 - Kisa Vadeli Yabanci Kaynaklar
    KisaVadeliYabanciKaynaklar = 3,
    // 4 - Uzun Vadeli Yabanci Kaynaklar
    UzunVadeliYabanciKaynaklar = 4,
    // 5 - Ozkaynaklar
    Ozkaynaklar = 5,
    // 6 - Gelir Tablosu Hesaplari
    GelirTablosu = 6,
    // 7 - Maliyet Hesaplari
    MaliyetHesaplari = 7,
    // 9 - Nazim Hesaplar
    NazimHesaplar = 9
}

/// <summary>
/// Muhasebe Fisi
/// </summary>
public class MuhasebeFis : BaseEntity
{
    [Required]
    public string FisNo { get; set; } = string.Empty;

    [Required]
    public DateTime FisTarihi { get; set; }

    public FisTipi FisTipi { get; set; }

    public string? Aciklama { get; set; }

    public decimal ToplamBorc { get; set; }
    public decimal ToplamAlacak { get; set; }

    public FisDurum Durum { get; set; } = FisDurum.Taslak;

    // Kaynak bilgisi
    public FisKaynak Kaynak { get; set; } = FisKaynak.Manuel;
    public int? KaynakId { get; set; } // Fatura, Hareket vb. ID
    public string? KaynakTip { get; set; } // "Fatura", "BankaHareket" vb.

    // Navigation
    public virtual ICollection<MuhasebeFisKalem> Kalemler { get; set; } = new List<MuhasebeFisKalem>();
}

public enum FisTipi
{
    Mahsup = 1,
    Tahsilat = 2,
    Tediye = 3,
    Acilis = 4,
    Kapanis = 5,
    Devir = 6
}

public enum FisDurum
{
    Taslak = 1,
    Onaylandi = 2,
    IptalEdildi = 3
}

public enum FisKaynak
{
    Manuel = 1,
    Fatura = 2,
    BankaHareket = 3,
    Butce = 4,
    Otomatik = 5
}

/// <summary>
/// Muhasebe Fis Kalemi
/// </summary>
public class MuhasebeFisKalem : BaseEntity
{
    public int FisId { get; set; }
    public virtual MuhasebeFis Fis { get; set; } = null!;

    public int HesapId { get; set; }
    public virtual MuhasebeHesap Hesap { get; set; } = null!;

    public int SiraNo { get; set; }

    public decimal Borc { get; set; }
    public decimal Alacak { get; set; }

    public DateTime? Tarih { get; set; } = DateTime.Today;

    public string? Aciklama { get; set; }

    // Cari/Detay bilgisi
    public int? CariId { get; set; }
    public virtual Cari? Cari { get; set; }
}

/// <summary>
/// Muhasebe Donem
/// </summary>
public class MuhasebeDonem : BaseEntity
{
    public int Yil { get; set; }
    public int Ay { get; set; }

    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }

    public DonemDurum Durum { get; set; } = DonemDurum.Acik;

    public DateTime? KapanisTarihi { get; set; }
}

public enum DonemDurum
{
    Acik = 1,
    Kapali = 2,
    Gecici = 3
}
