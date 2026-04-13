using KOAFiloServis.Shared.Entities;

namespace KOAFiloServis.Web.Models;

/// <summary>
/// XML/PDF fatura AI analiz sonucu - kullanıcıya öneri olarak sunulur
/// </summary>
public class FaturaAIAnalizSonuc
{
    // Fatura Temel Bilgileri (XML'den parse edilen)
    public string? FaturaNo { get; set; }
    public DateTime? FaturaTarihi { get; set; }
    public DateTime? VadeTarihi { get; set; }
    public string? EttnNo { get; set; }
    public FaturaYonu FaturaYonu { get; set; } = FaturaYonu.Gelen;
    public EFaturaTipi EFaturaTipi { get; set; } = EFaturaTipi.EFatura;

    // Satıcı/Alıcı Bilgileri (XML'den)
    public FaturaAICariBilgi SaticiBilgi { get; set; } = new();
    public FaturaAICariBilgi AliciBilgi { get; set; } = new();

    // Cari Eşleşme Sonucu
    public CariEslesmeSonuc CariEslesme { get; set; } = new();

    // Tutar Bilgileri
    public decimal AraToplam { get; set; }
    public decimal IskontoTutar { get; set; }
    public decimal KdvTutar { get; set; }
    public decimal GenelToplam { get; set; }

    // Tevkifat
    public bool TevkifatliMi { get; set; }
    public decimal TevkifatOrani { get; set; }
    public string? TevkifatKodu { get; set; }

    // AI ile tespit edilen kalemler
    public List<FaturaAIKalem> Kalemler { get; set; } = [];

    // Genel AI tahmini
    public string? AIYorum { get; set; }
    public bool AnalizBasarili { get; set; } = true;
    public string? HataMesaji { get; set; }

    // Kaynak dosya
    public string? DosyaAdi { get; set; }
    public string? DosyaTipi { get; set; } // "xml" veya "pdf"
    public string? DosyaIcerik { get; set; } // Base64
}

/// <summary>
/// Fatura cari bilgileri (satıcı veya alıcı)
/// </summary>
public class FaturaAICariBilgi
{
    public string? Unvan { get; set; }
    public string? VergiDairesi { get; set; }
    public string? VergiNo { get; set; }
    public string? TcKimlikNo { get; set; }
    public string? Adres { get; set; }
    public string? Il { get; set; }
    public string? Ilce { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
}

/// <summary>
/// Cari eşleşme sonucu - varsa mevcut cari, yoksa oluşturma önerisi
/// </summary>
public class CariEslesmeSonuc
{
    public bool CariMevcut { get; set; }
    public int? MevcutCariId { get; set; }
    public string? MevcutCariUnvan { get; set; }
    public string? MevcutCariKodu { get; set; }

    // Eşleşme metodu
    public string? EslesmeYontemi { get; set; } // "VergiNo", "Unvan", "Yok"

    // Yeni cari önerisi (mevcut değilse)
    public Cari? YeniCariOnerisi { get; set; }
    public bool YeniCariOlusturulacak { get; set; }
}

/// <summary>
/// AI ile tespit edilen fatura kalemi - kullanıcı düzeltebilir
/// </summary>
public class FaturaAIKalem
{
    public int SiraNo { get; set; }
    public string Aciklama { get; set; } = string.Empty;
    public string? UrunKodu { get; set; }
    public decimal Miktar { get; set; } = 1;
    public string Birim { get; set; } = "Adet";
    public decimal BirimFiyat { get; set; }
    public decimal IskontoOrani { get; set; }
    public decimal IskontoTutar { get; set; }
    public decimal KdvOrani { get; set; } = 20;
    public decimal KdvTutar { get; set; }
    public decimal ToplamTutar { get; set; }

    // Tevkifat (kalem bazında)
    public decimal TevkifatOrani { get; set; }
    public decimal TevkifatTutar { get; set; }

    // AI Tahmini - Kalem Tipi
    public FaturaKalemTipi AIKalemTipi { get; set; } = FaturaKalemTipi.Hizmet;
    public FaturaKalemAltTipi? AIAltTipi { get; set; }
    public int AIGuvenSkoru { get; set; } // 0-100

    // Kullanıcı tarafından düzeltilen
    public FaturaKalemTipi KullaniciKalemTipi { get; set; } = FaturaKalemTipi.Hizmet;
    public FaturaKalemAltTipi? KullaniciAltTipi { get; set; }
    public bool KullaniciDuzeltti { get; set; }

    // Güzergah eşleşme (hizmet/taşıma ise)
    public GuzergahEslesmeSonuc? GuzergahEslesme { get; set; }

    // Stok kartı eşleşme (mal ise)
    public StokEslesmeSonuc? StokEslesme { get; set; }

    // AI açıklaması
    public string? AIAciklama { get; set; }
}

/// <summary>
/// Güzergah eşleşme sonucu
/// </summary>
public class GuzergahEslesmeSonuc
{
    public bool GuzergahMevcut { get; set; }
    public int? MevcutGuzergahId { get; set; }
    public string? MevcutGuzergahAdi { get; set; }
    public string? MevcutGuzergahKodu { get; set; }
    public decimal? MevcutBirimFiyat { get; set; }

    // Önerilen güzergah bilgileri (mevcut değilse)
    public string? OnerilenGuzergahAdi { get; set; }
    public string? OnerilenBaslangic { get; set; }
    public string? OnerilenBitis { get; set; }
    public bool YeniGuzergahOlusturulacak { get; set; }

    // Benzer güzergahlar
    public List<BenzerGuzergah> BenzerGuzergahlar { get; set; } = [];
}

public class BenzerGuzergah
{
    public int GuzergahId { get; set; }
    public string GuzergahAdi { get; set; } = string.Empty;
    public string? GuzergahKodu { get; set; }
    public decimal BirimFiyat { get; set; }
    public string? CariUnvan { get; set; }
    public int BenzerlikSkoru { get; set; } // 0-100
}

/// <summary>
/// Stok kartı eşleşme sonucu
/// </summary>
public class StokEslesmeSonuc
{
    public bool StokMevcut { get; set; }
    public int? MevcutStokId { get; set; }
    public string? MevcutStokKodu { get; set; }
    public string? MevcutStokAdi { get; set; }
    public decimal? MevcutFiyat { get; set; }

    // Benzer stoklar
    public List<BenzerStok> BenzerStoklar { get; set; } = [];

    // Yeni stok önerisi
    public bool YeniStokOlusturulacak { get; set; }
}

public class BenzerStok
{
    public int StokId { get; set; }
    public string StokKodu { get; set; } = string.Empty;
    public string StokAdi { get; set; } = string.Empty;
    public decimal Fiyat { get; set; }
    public string? Birim { get; set; }
    public int BenzerlikSkoru { get; set; } // 0-100
}

/// <summary>
/// Kaydetme sonucu
/// </summary>
public class FaturaAIKaydetSonuc
{
    public bool Basarili { get; set; }
    public int? FaturaId { get; set; }
    public int? CariId { get; set; }
    public List<int> OlusturulanGuzergahIdler { get; set; } = [];
    public List<int> OlusturulanStokIdler { get; set; } = [];
    public string? Mesaj { get; set; }
    public List<string> Uyarilar { get; set; } = [];
}
