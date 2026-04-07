using CRMFiloServis.Shared.Entities;

namespace CRMFiloServis.Web.Models;

/// <summary>
/// İhale hazırlık AI maliyet tahmin istek modeli
/// </summary>
public class IhaleMaliyetTahminIstek
{
    public string AracModel { get; set; } = string.Empty;
    public int AracModelYili { get; set; }
    public int KoltukSayisi { get; set; }
    public decimal MesafeKm { get; set; }
    public int GunlukSeferSayisi { get; set; }
    public int AylikCalismGunu { get; set; }
    public AracSahiplikKalem SahiplikDurumu { get; set; }
    public decimal YakitTuketimi { get; set; } // lt/100km
    public decimal YakitFiyati { get; set; }
    public int SozlesmeSuresiAy { get; set; }
    public decimal EnflasyonOrani { get; set; }
}

/// <summary>
/// AI maliyet tahmin sonucu
/// </summary>
public class IhaleMaliyetTahminSonuc
{
    public decimal TahminiAylikBakim { get; set; }
    public decimal TahminiAylikLastik { get; set; }
    public decimal TahminiAylikSigorta { get; set; }
    public decimal TahminiAylikKasko { get; set; }
    public decimal TahminiAylikMuayene { get; set; }
    public decimal TahminiAylikYedekParca { get; set; }
    public decimal TahminiAylikDigerMasraf { get; set; }
    public string? AIAciklama { get; set; }
    public bool Basarili { get; set; }
}

/// <summary>
/// AI şoför maaş tahmin sonucu
/// </summary>
public class IhaleSoforMaasTahmin
{
    public decimal TahminiBrutMaas { get; set; }
    public decimal TahminiNetMaas { get; set; }
    public decimal TahminiSGKIsverenPay { get; set; }
    public decimal TahminiToplamMaliyet { get; set; }
    public decimal EnflasyonluBrutMaas { get; set; } // Proje sonu tahmini
    public string? AIAciklama { get; set; }
    public bool Basarili { get; set; }
}

/// <summary>
/// İhale proje özet rapor
/// </summary>
public class IhaleProjeOzet
{
    public int ProjeId { get; set; }
    public string ProjeKodu { get; set; } = string.Empty;
    public string ProjeAdi { get; set; } = string.Empty;
    public string? MusteriFirma { get; set; }
    public int SozlesmeSuresiAy { get; set; }
    public int GuzergahSayisi { get; set; }
    public int AracSayisi { get; set; }

    // Maliyet özet
    public decimal ToplamAylikYakit { get; set; }
    public decimal ToplamAylikAracMasraf { get; set; }
    public decimal ToplamAylikSoforMaliyet { get; set; }
    public decimal ToplamAylikKiraKomisyon { get; set; }
    public decimal ToplamAylikAmortisman { get; set; }
    public decimal ToplamAylikMaliyet { get; set; }

    // Kar/Zarar
    public decimal ToplamAylikKar { get; set; }
    public decimal ToplamAylikTeklifFiyati { get; set; }
    public decimal KarMarjiOrtalama { get; set; }

    // Proje toplam
    public decimal ToplamProjeMaliyeti { get; set; }
    public decimal ToplamProjeKar { get; set; }
    public decimal ToplamProjeTeklif { get; set; }

    // Birim fiyatlar
    public decimal OrtalamaSeferBasiMaliyet { get; set; }
    public decimal OrtalamaSaatlikMaliyet { get; set; }
    public decimal OrtalamaKmBasiMaliyet { get; set; }

    // Hat detayları
    public List<IhaleKalemOzet> KalemOzetleri { get; set; } = [];

    // Enflasyonlu projeksiyon
    public List<AylikProjeksiyonOzet> AylikProjeksiyonlar { get; set; } = [];
}

/// <summary>
/// Hat bazlı özet
/// </summary>
public class IhaleKalemOzet
{
    public int KalemId { get; set; }
    public string HatAdi { get; set; } = string.Empty;
    public decimal MesafeKm { get; set; }
    public string SahiplikDurumu { get; set; } = string.Empty;
    public string AracBilgi { get; set; } = string.Empty;
    public decimal AylikMaliyet { get; set; }
    public decimal AylikTeklifFiyati { get; set; }
    public decimal KarMarji { get; set; }
    public decimal SeferBasiTeklif { get; set; }
}

/// <summary>
/// Aylık projeksiyon özet
/// </summary>
public class AylikProjeksiyonOzet
{
    public int Ay { get; set; }
    public string DonemAdi { get; set; } = string.Empty; // "Ocak 2025"
    public decimal ToplamMaliyet { get; set; }
    public decimal ToplamKar { get; set; }
    public decimal ToplamTeklif { get; set; }
    public decimal KumulatifMaliyet { get; set; }
    public decimal KumulatifKar { get; set; }
    public decimal EnflasyonEtkisi { get; set; } // Enflasyon farkı
}
