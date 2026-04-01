using CRMFiloServis.Shared.Entities;

namespace CRMFiloServis.Web.Services;

public interface IBudgetService
{
    // Odeme Islemleri
    Task<List<BudgetOdeme>> GetOdemelerAsync(int yil, int? ay = null, int? firmaId = null);
    Task<List<BudgetOdeme>> GetBekleyenOdemelerAsync(int yil, int? ay = null);
    Task<List<BudgetOdeme>> GetDevirBekleyenOdemelerAsync(DateTime donemBaslangic, int? firmaId = null);
    Task<List<BudgetOdeme>> GetOdemelerByDateRangeAsync(DateTime baslangic, DateTime bitis);
    Task<BudgetOdeme?> GetOdemeByIdAsync(int id);
    Task<BudgetOdeme> CreateOdemeAsync(BudgetOdeme odeme);
    Task<BudgetOdeme> UpdateOdemeAsync(BudgetOdeme odeme);
    Task DeleteOdemeAsync(int id); // Soft delete
    Task HardDeleteOdemeAsync(int id); // Kalici silme
    Task<BudgetOdeme> OdemeYapAsync(int odemeId, OdemeYapRequest request); // Kasa=Borc, Odeme=Alacak

    // Fatura ile kapatma
    Task<BudgetOdeme> FaturaIleKapatAsync(int odemeId, int faturaId);

    // Taksitli Odeme Islemleri
    Task<List<BudgetOdeme>> CreateTaksitliOdemeAsync(TaksitliOdemeRequest request);
    Task<List<BudgetOdeme>> GetTaksitGrubuAsync(Guid taksitGrupId);
    Task UpdateTaksitGrubuAsync(List<BudgetOdeme> taksitler);
    
    // Excel Islemleri
    Task<byte[]> GetExcelSablonAsync(List<Firma> firmalar);
    Task<int> ImportFromExcelAsync(byte[] fileContent);

    // Masraf Kalemleri
    Task<List<BudgetMasrafKalemi>> GetMasrafKalemleriAsync();
    Task<BudgetMasrafKalemi> CreateMasrafKalemiAsync(BudgetMasrafKalemi kalem);
    Task<BudgetMasrafKalemi> UpdateMasrafKalemiAsync(BudgetMasrafKalemi kalem);
    Task DeleteMasrafKalemiAsync(int id);
    Task SeedMasrafKalemleriAsync();

    // Raporlar
    Task<BudgetOzet> GetAylikOzetAsync(int yil, int ay, int? firmaId = null);
    Task<BudgetOzet> GetPeriyodOzetAsync(DateTime baslangic, DateTime bitis);
    Task<BudgetYillikOzet> GetYillikOzetAsync(int yil, int? firmaId = null);
    Task<List<BudgetGunlukOzet>> GetTakvimDataAsync(int yil, int ay, int? firmaId = null);
    Task<List<BudgetKategoriOzet>> GetKategoriOzetAsync(int yil, int? ay = null);
    Task<List<BudgetKategoriOzet>> GetKategoriOzetByDateRangeAsync(DateTime baslangic, DateTime bitis);
    Task<List<BudgetTrendData>> GetTrendDataAsync(DateTime baslangic, DateTime bitis, string periyod);
    
    // Kredi/Taksit Raporlari
    Task<List<KrediOzet>> GetAktifKredilerAsync(int? firmaId = null);
    Task<List<KrediOzet>> GetKrediOzetleriAsync(int? yil = null, int? firmaId = null);
    Task<List<KrediTaksitDetay>> GetKrediTaksitDetaylariAsync(Guid taksitGrupId);
    Task<BudgetOdeme?> GetTaksitOdemeAsync(Guid taksitGrupId, int taksitNo);
    Task<List<AylikKrediTaksitRapor>> GetAylikKrediTaksitRaporuAsync(int yil);
    Task OdemeYapAsync(int odemeId, int bankaHesapId, DateTime odemeTarihi);

    // Kredi Karti Islemleri
    Task AddKrediKartiBorcAsync(int bankaHesapId, decimal tutar, int ay, int yil, string aciklama);
    Task<List<BudgetOdeme>> GetKrediKartiHareketleriAsync(int bankaHesapId, int? yil = null);

    // Tekrarlayan Odeme Islemleri
    Task<List<TekrarlayanOdeme>> GetTekrarlayanOdemelerAsync(int? firmaId = null);
    Task<List<TekrarlayanOdeme>> GetAktifTekrarlayanOdemelerAsync(int? firmaId = null);
    Task<TekrarlayanOdeme?> GetTekrarlayanOdemeByIdAsync(int id);
    Task<TekrarlayanOdeme> CreateTekrarlayanOdemeAsync(TekrarlayanOdeme odeme);
    Task<TekrarlayanOdeme> UpdateTekrarlayanOdemeAsync(TekrarlayanOdeme odeme);
    Task DeleteTekrarlayanOdemeAsync(int id);
    Task<int> TekrarlayanOdemelerdenKayitOlusturAsync(int yil, int ay, int? firmaId = null);
}

public class TaksitliOdemeRequest
{
    public DateTime BaslangicTarihi { get; set; }
    public string MasrafKalemi { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public decimal ToplamTutar { get; set; }
    public int TaksitSayisi { get; set; }
    public string? Notlar { get; set; }
    public int? FirmaId { get; set; }
    public List<TaksitDetayRequest> TaksitPlani { get; set; } = new();
}

public class TaksitDetayRequest
{
    public int Sira { get; set; }
    public DateTime Tarih { get; set; }
    public decimal Tutar { get; set; }
}

public class OdemeYapRequest
{
    public OdemeTipi OdemeTipi { get; set; }
    public int? BankaHesapId { get; set; }
    public string? Aciklama { get; set; }
    public DateTime OdemeTarihi { get; set; } = DateTime.Today;
    public decimal? KismiOdemeTutari { get; set; }
    public Guid? KrediTaksitGrupId { get; set; } // Kredi kartı için ilişkili kredi

    // Cari Mahsup için
    public int? CariId { get; set; }
    public bool CaridenTahsilat { get; set; } = false; // true: cariden tahsilat, false: cariye ödeme

    // Kesinti bilgileri
    public decimal MasrafKesintisi { get; set; } = 0;
    public decimal CezaKesintisi { get; set; } = 0;
    public decimal DigerKesinti { get; set; } = 0;
    public string? KesintiAciklamasi { get; set; }
    public string? OdemeNotu { get; set; }

    // Muhasebe Eşleştirme
    public string? MuhasebeHesapKodu { get; set; }
    public string? KostMerkeziKodu { get; set; }
    public string? ProjeKodu { get; set; }

    // Hesaplanan
    public decimal ToplamKesinti => MasrafKesintisi + CezaKesintisi + DigerKesinti;
}

public enum OdemeTipi
{
    Kasa = 1,
    Banka = 2,
    Mahsup = 3,         // Hesaplar arası transfer
    KrediKarti = 4,
    CariMahsup = 5      // Cari hesap ile mahsup
}

public class BudgetOzet
{
    public int Yil { get; set; }
    public int Ay { get; set; }
    public DateTime? BaslangicTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }
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
    public decimal BekleyenToplamOdeme { get; set; }
    public int BekleyenOdemeSayisi { get; set; }
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

public class BudgetTrendData
{
    public string Etiket { get; set; } = string.Empty;
    public DateTime Tarih { get; set; }
    public decimal Toplam { get; set; }
    public decimal Odenen { get; set; }
    public decimal Bekleyen { get; set; }
    public int OdemeSayisi { get; set; }
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
