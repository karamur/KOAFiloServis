namespace KOAFiloServis.Web.Services.Interfaces;

/// <summary>
/// EBYS AI Servisi Interface - OCR, belge sınıflandırma ve akıllı arama
/// </summary>
public interface IEbysAIService
{
    /// <summary>
    /// Belge içeriğinden metin çıkarır (OCR)
    /// </summary>
    Task<OcrSonuc> MetinCikarAsync(Stream dosyaStream, string dosyaAdi);

    /// <summary>
    /// Belge içeriğinden metin çıkarır (dosya yolu)
    /// </summary>
    Task<OcrSonuc> MetinCikarAsync(string dosyaYolu);

    /// <summary>
    /// Belgeyi otomatik sınıflandırır (AI ile kategori tahmini)
    /// </summary>
    Task<BelgeSiniflandirmaSonuc> BelgeSiniflandirAsync(string metin, BelgeTipi belgeGrubu);

    /// <summary>
    /// Belge özeti oluşturur
    /// </summary>
    Task<string> BelgeOzetiOlusturAsync(string metin, int maxKarakter = 500);

    /// <summary>
    /// Belgeden anahtar kelimeler çıkarır
    /// </summary>
    Task<List<string>> AnahtarKelimelerCikarAsync(string metin, int maxKelime = 10);

    /// <summary>
    /// İki belge arasındaki benzerliği hesaplar
    /// </summary>
    Task<double> BelgeBenzerligiHesaplaAsync(string metin1, string metin2);

    /// <summary>
    /// Belge içeriğine göre öneriler sunar
    /// </summary>
    Task<BelgeOneriSonuc> OneriGetirAsync(string metin, BelgeTipi belgeGrubu);

    /// <summary>
    /// AI servisinin durumunu kontrol eder
    /// </summary>
    Task<AIDurumBilgi> DurumKontrolAsync();
}

/// <summary>
/// OCR işlem sonucu
/// </summary>
public class OcrSonuc
{
    public bool Basarili { get; set; }
    public string Metin { get; set; } = string.Empty;
    public string? HataMesaji { get; set; }
    public double GuvenSkor { get; set; } // 0-100 arası
    public int SayfaSayisi { get; set; }
    public string Dil { get; set; } = "tur"; // Türkçe
    public TimeSpan IslemSuresi { get; set; }
    public OcrDetayBilgi? Detay { get; set; }
}

/// <summary>
/// OCR detay bilgileri
/// </summary>
public class OcrDetayBilgi
{
    public int KarakterSayisi { get; set; }
    public int KelimeSayisi { get; set; }
    public int SatirSayisi { get; set; }
    public List<OcrSayfaBilgi> Sayfalar { get; set; } = new();
}

/// <summary>
/// OCR sayfa bazlı bilgi
/// </summary>
public class OcrSayfaBilgi
{
    public int SayfaNo { get; set; }
    public string Metin { get; set; } = string.Empty;
    public double GuvenSkor { get; set; }
}

/// <summary>
/// Belge sınıflandırma sonucu
/// </summary>
public class BelgeSiniflandirmaSonuc
{
    public bool Basarili { get; set; }
    public string? HataMesaji { get; set; }
    public string TahminEdilenKategori { get; set; } = string.Empty;
    public int? TahminEdilenKategoriId { get; set; }
    public double GuvenSkor { get; set; } // 0-100 arası
    public List<KategoriTahmin> AlternatifKategoriler { get; set; } = new();
    public string? AIAciklama { get; set; }
}

/// <summary>
/// Kategori tahmin detayı
/// </summary>
public class KategoriTahmin
{
    public string KategoriAdi { get; set; } = string.Empty;
    public int? KategoriId { get; set; }
    public double GuvenSkor { get; set; }
}

/// <summary>
/// Belge tipi (sınıflandırma grubu)
/// </summary>
public enum BelgeTipi
{
    /// <summary>EBYS Gelen/Giden Evrak</summary>
    EbysEvrak = 1,
    /// <summary>Personel Özlük Belgesi</summary>
    PersonelOzluk = 2,
    /// <summary>Araç Evrakı</summary>
    AracEvrak = 3,
    /// <summary>Genel Belge</summary>
    Genel = 99
}

/// <summary>
/// Belge öneri sonucu
/// </summary>
public class BelgeOneriSonuc
{
    public bool Basarili { get; set; }
    public string? HataMesaji { get; set; }
    public List<string> Oneriler { get; set; } = new();
    public string? TahminEdilenKonu { get; set; }
    public DateTime? TahminEdilenVadeTarihi { get; set; }
    public string? TahminEdilenOncelik { get; set; }
}

/// <summary>
/// AI durum bilgisi
/// </summary>
public class AIDurumBilgi
{
    public bool OllamaAktif { get; set; }
    public bool OcrAktif { get; set; }
    public string OllamaModel { get; set; } = string.Empty;
    public string OllamaVersiyon { get; set; } = string.Empty;
    public string OcrMotor { get; set; } = "Tesseract";
    public string OcrVersiyon { get; set; } = string.Empty;
    public List<string> DesteklenenDiller { get; set; } = new() { "tur", "eng" };
    public string? HataMesaji { get; set; }
}
