using CRMFiloServis.Shared.Entities;

namespace CRMFiloServis.Web.Services;

public interface IBankaKasaHareketService
{
    Task<List<BankaKasaHareket>> GetAllAsync();
    Task<List<BankaKasaHareket>> GetRecentAsync(int count = 5);
    Task<List<BankaKasaHareket>> GetByHesapIdAsync(int hesapId);
    Task<List<BankaKasaHareket>> GetByCariIdAsync(int cariId);
    Task<List<BankaKasaHareket>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<List<BankaKasaHareket>> GetByTipAsync(HareketTipi tip);
    Task<List<BankaKasaHareket>> GetEslestirmeyeUygunHareketlerAsync(int cariId, HareketTipi tip);
    Task<BankaKasaHareket?> GetByIdAsync(int id);
    Task<BankaKasaHareket> CreateAsync(BankaKasaHareket hareket);
    Task<BankaKasaHareket> UpdateAsync(BankaKasaHareket hareket);
    Task DeleteAsync(int id);
    Task<string> GenerateNextIslemNoAsync();

    // BankaHesap (Kasa/Banka) işlemleri
    Task<List<BankaHesap>> GetHesaplarAsync();
    Task<List<BankaHesap>> GetAktifHesaplarAsync();
    Task<BankaHesap?> GetHesapByIdAsync(int id);
    Task<BankaHesap> CreateHesapAsync(BankaHesap hesap);
    Task<BankaHesap> UpdateHesapAsync(BankaHesap hesap);
    Task DeleteHesapAsync(int id);

    // Mahsup işlemleri
    Task<MahsupSonuc> HesaplarArasiTransferAsync(int kaynakHesapId, int hedefHesapId, decimal tutar, DateTime tarih, string aciklama);
    Task<MahsupSonuc> CariMahsupAsync(int cariId, int hesapId, decimal tutar, DateTime tarih, string aciklama, bool caridenHesaba);
    Task<List<BankaKasaHareket>> GetMahsupHareketleriAsync(DateTime? baslangic = null, DateTime? bitis = null);
    Task MahsupIptalAsync(Guid mahsupGrupId);
    Task<decimal> GetHesapBakiyeAsync(int hesapId);
    Task<Dictionary<int, decimal>> GetTumHesapBakiyeleriAsync();

    // Dashboard optimized methods
    Task<DashboardBankaStats> GetDashboardStatsAsync();
}

public class DashboardBankaStats
{
    public decimal ToplamKasa { get; set; }
    public decimal ToplamBanka { get; set; }
}

public class MahsupSonuc
{
    public bool Basarili { get; set; }
    public string? Hata { get; set; }
    public Guid? MahsupGrupId { get; set; }
    public BankaKasaHareket? KaynakHareket { get; set; }
    public BankaKasaHareket? HedefHareket { get; set; }
}
