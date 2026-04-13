using KOAFiloServis.Web.Models;

namespace KOAFiloServis.Web.Services;

public interface IFaturaAIImportService
{
    /// <summary>
    /// XML dosyasını parse edip AI analizi yapar
    /// </summary>
    Task<FaturaAIAnalizSonuc> AnalizEtXmlAsync(string xmlIcerik, string dosyaAdi);

    /// <summary>
    /// PDF dosyasından AI ile bilgi çıkarır
    /// </summary>
    Task<FaturaAIAnalizSonuc> AnalizEtPdfAsync(byte[] pdfIcerik, string dosyaAdi);

    /// <summary>
    /// Kalem açıklamalarını AI ile sınıflandırır (hizmet/mal/kiralama, güzergah/stok eşleştirme)
    /// </summary>
    Task<List<FaturaAIKalem>> KalemleriSiniflandirAsync(List<FaturaAIKalem> kalemler, int? cariId, string? cariUnvan);

    /// <summary>
    /// Cari eşleştirmesi yapar (VKN/TCKN ve Unvan bazlı)
    /// </summary>
    Task<CariEslesmeSonuc> CariEslestirAsync(FaturaAICariBilgi cariBilgi);

    /// <summary>
    /// Güzergah eşleştirmesi yapar (kalem açıklaması ve cari bazlı)
    /// </summary>
    Task<GuzergahEslesmeSonuc> GuzergahEslestirAsync(string kalemAciklama, int? cariId, decimal birimFiyat, decimal miktar);

    /// <summary>
    /// Stok kartı eşleştirmesi yapar (ürün kodu ve açıklama bazlı)
    /// </summary>
    Task<StokEslesmeSonuc> StokEslestirAsync(string kalemAciklama, string? urunKodu);

    /// <summary>
    /// AI analiz sonucunu onaylayıp fatura + ilişkili kayıtları oluşturur
    /// </summary>
    Task<FaturaAIKaydetSonuc> KaydetAsync(FaturaAIAnalizSonuc sonuc, int? firmaId);
}
