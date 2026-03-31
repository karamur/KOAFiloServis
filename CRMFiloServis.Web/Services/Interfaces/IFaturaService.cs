using CRMFiloServis.Shared.Entities;

namespace CRMFiloServis.Web.Services;

public interface IFaturaService
{
    Task<List<Fatura>> GetAllAsync();
    Task<List<Fatura>> GetByCariIdAsync(int cariId);
    Task<List<Fatura>> GetByTipAsync(FaturaTipi tip);
    Task<List<Fatura>> GetByDurumAsync(FaturaDurum durum);
    Task<List<Fatura>> GetOdenmemisFaturalarAsync();
    Task<List<Fatura>> GetOdenmisFaturalarAsync();
    Task<List<Fatura>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Fatura?> GetByIdAsync(int id);
    Task<Fatura?> GetByIdWithKalemlerAsync(int id);
    Task<Fatura> CreateAsync(Fatura fatura);
    Task<Fatura> UpdateAsync(Fatura fatura);
    Task DeleteAsync(int id);
    Task<string> GenerateNextFaturaNoAsync(FaturaTipi tip);
    Task UpdateOdenenTutarAsync(int faturaId);
    
    // Muhasebe Fişi
    Task<MuhasebeFis> CreateMuhasebeFisiAsync(int faturaId);
    
    // E-Fatura / E-Arsiv metodlari
    Task<List<Fatura>> GetByYonAsync(FaturaYonu yon, int? firmaId = null);
    Task<List<Fatura>> GetByYonAndDateRangeAsync(FaturaYonu yon, DateTime? baslangic, DateTime? bitis, int? firmaId = null);
    Task<List<Fatura>> GetByEFaturaTipiAsync(EFaturaTipi tip);
    Task<EFaturaImportResult> ImportFromExcelAsync(byte[] fileContent, FaturaYonu yon, int? firmaId = null, EFaturaTipi? eFaturaTipi = null);
    Task<EFaturaImportResult> ImportFromXmlAsync(List<XmlFileContent> xmlFiles, FaturaYonu yon, int? firmaId = null, EFaturaTipi? eFaturaTipi = null);
    
    // Excel Sablon ve Export - Yeni format (ornek dosya ile uyumlu)
    Task<byte[]> GetExcelSablonAsync(FaturaYonu yon);
    Task<byte[]> ExportToExcelAsync(List<Fatura> faturalar);
    
    // Dashboard optimized methods
    Task<DashboardFaturaStats> GetDashboardStatsAsync();
    
    // Fatura Kalemleri - Stok Türü Eşleştirme
    Task<List<FaturaKalem>> GetFaturaKalemleriAsync(DateTime? baslangic = null, DateTime? bitis = null);
    Task<List<FaturaKalem>> GetEslesmemisKalemleriAsync(DateTime? baslangic = null, DateTime? bitis = null);
    Task<List<FaturaKalem>> GetEslesmisKalemleriAsync(DateTime? baslangic = null, DateTime? bitis = null);
    Task<StokKartiOlusturSonuc> UpdateFaturaKalemleriVeStokKartiOlusturAsync(List<FaturaKalem> kalemler, bool stokKartiOlustur = true);
    Task UpdateFaturaKalemleriAsync(List<FaturaKalem> kalemler);
}

public class StokKartiOlusturSonuc
{
    public int GuncellenenKalemSayisi { get; set; }
    public int OlusturulanStokKartiSayisi { get; set; }
    public int AtlananStokKartiSayisi { get; set; }
    public int OlusturulanStokHareketSayisi { get; set; }
    public int OlusturulanGiderKayitSayisi { get; set; }
    public List<string> Hatalar { get; set; } = new();
}

public class DashboardFaturaStats
{
    public int BekleyenFaturaSayisi { get; set; }
    public decimal BuAyGelir { get; set; }
    public decimal BuAyGider { get; set; }
    public List<Fatura> VadeGecmisFaturalar { get; set; } = [];
    public List<Fatura> VadeYaklasanFaturalar { get; set; } = [];
}

public class EFaturaImportResult
{
    public bool Success { get; set; }
    public int ImportedCount { get; set; }
    public int SkippedCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<Fatura> ImportedItems { get; set; } = new();
}

public class XmlFileContent
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
}
