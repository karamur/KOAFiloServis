using CRMFiloServis.Shared.Entities;

namespace CRMFiloServis.Web.Services;

public interface IBudgetService
{
    // Ödeme Ýþlemleri
    Task<List<BudgetOdeme>> GetOdemelerAsync(int yil, int? ay = null);
    Task<BudgetOdeme?> GetOdemeByIdAsync(int id);
    Task<BudgetOdeme> CreateOdemeAsync(BudgetOdeme odeme);
    Task<BudgetOdeme> UpdateOdemeAsync(BudgetOdeme odeme);
    Task DeleteOdemeAsync(int id);

    // Taksitli Ödeme Ýþlemleri
    Task<List<BudgetOdeme>> CreateTaksitliOdemeAsync(TaksitliOdemeRequest request);
    Task<List<BudgetOdeme>> GetTaksitGrubuAsync(Guid taksitGrupId);
    Task UpdateTaksitGrubuAsync(List<BudgetOdeme> taksitler);
    
    // Toplu Ýþlemler (Excel)
    Task<List<BudgetOdeme>> CreateBulkOdemeAsync(List<BudgetOdeme> odemeler);
    byte[] GenerateExcelTemplate();
    Task<ExcelImportResult> ImportFromExcelAsync(byte[] fileContent);

    // Masraf Kalemleri
    Task<List<BudgetMasrafKalemi>> GetMasrafKalemleriAsync();
    Task<BudgetMasrafKalemi> CreateMasrafKalemiAsync(BudgetMasrafKalemi kalem);
    Task<BudgetMasrafKalemi> UpdateMasrafKalemiAsync(BudgetMasrafKalemi kalem);
    Task DeleteMasrafKalemiAsync(int id);

    // Raporlar
    Task<BudgetOzet> GetAylikOzetAsync(int yil, int ay);
    Task<BudgetYillikOzet> GetYillikOzetAsync(int yil);
    Task<List<BudgetGunlukOzet>> GetTakvimDataAsync(int yil, int ay);
    Task<List<BudgetKategoriOzet>> GetKategoriOzetAsync(int yil, int? ay = null);
    
    // Kredi/Taksit Raporlari
    Task<List<KrediOzet>> GetAktifKredilerAsync();
    Task<List<AylikKrediTaksitRapor>> GetAylikKrediTaksitRaporuAsync(int yil);
}

public class TaksitliOdemeRequest
{
    public DateTime BaslangicTarihi { get; set; }
    public string MasrafKalemi { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public decimal ToplamTutar { get; set; }
    public int TaksitSayisi { get; set; }
    public string? Notlar { get; set; }
}

public class BudgetOzet
{
    public int Yil { get; set; }
    public int Ay { get; set; }
    public decimal ToplamOdeme { get; set; }
    public decimal OdenenToplam { get; set; }
    public decimal BekleyenToplam { get; set; }
    public int ToplamKayit { get; set; }
    public int OdenenKayit { get; set; }
    public int BekleyenKayit { get; set; }
    public List<BudgetKategoriOzet> KategoriOzetleri { get; set; } = new();
}

public class BudgetYillikOzet
{
    public int Yil { get; set; }
    public decimal ToplamOdeme { get; set; }
    public List<BudgetAylikToplam> AylikToplamlar { get; set; } = new();
    public List<BudgetKategoriOzet> KategoriOzetleri { get; set; } = new();
}

public class BudgetAylikToplam
{
    public int Ay { get; set; }
    public string AyAdi { get; set; } = string.Empty;
    public decimal Toplam { get; set; }
    public decimal Odenen { get; set; }
    public decimal Bekleyen { get; set; }
}

public class BudgetGunlukOzet
{
    public DateTime Tarih { get; set; }
    public int Gun { get; set; }
    public decimal ToplamOdeme { get; set; }
    public int OdemeSayisi { get; set; }
    public List<BudgetOdeme> Odemeler { get; set; } = new();
}

public class BudgetKategoriOzet
{
    public string MasrafKalemi { get; set; } = string.Empty;
    public string? Renk { get; set; }
    public decimal Toplam { get; set; }
    public int Adet { get; set; }
    public decimal Yuzde { get; set; }
}

public class ExcelImportResult
{
    public bool Success { get; set; }
    public int ImportedCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<BudgetOdeme> ImportedItems { get; set; } = new();
}

// Kredi/Taksit Rapor Modelleri
public class KrediOzet
{
    public Guid TaksitGrupId { get; set; }
    public string MasrafKalemi { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }
    public int ToplamTaksitSayisi { get; set; }
    public int OdenenTaksitSayisi { get; set; }
    public int KalanTaksitSayisi { get; set; }
    public decimal TaksitTutari { get; set; }
    public decimal ToplamTutar { get; set; }
    public decimal OdenenTutar { get; set; }
    public decimal KalanTutar { get; set; }
    public decimal TamamlanmaYuzdesi { get; set; }
    public DateTime? SonrakiTaksitTarihi { get; set; }
}

public class AylikKrediTaksitRapor
{
    public int Ay { get; set; }
    public string AyAdi { get; set; } = string.Empty;
    public decimal ToplamTaksitTutari { get; set; }
    public decimal OdenenTutar { get; set; }
    public decimal BekleyenTutar { get; set; }
    public int TaksitSayisi { get; set; }
    public List<KrediTaksitDetay> Taksitler { get; set; } = new();
}

public class KrediTaksitDetay
{
    public string MasrafKalemi { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public int KacinciTaksit { get; set; }
    public int ToplamTaksitSayisi { get; set; }
    public decimal Tutar { get; set; }
    public OdemeDurum Durum { get; set; }
    public DateTime OdemeTarihi { get; set; }
}
