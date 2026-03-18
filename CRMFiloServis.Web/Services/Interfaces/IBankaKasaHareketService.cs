using CRMFiloServis.Shared.Entities;

namespace CRMFiloServis.Web.Services;

public interface IBankaKasaHareketService
{
    Task<List<BankaKasaHareket>> GetAllAsync();
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
    
    // BankaHesap (Kasa/Banka) iþlemleri
    Task<List<BankaHesap>> GetHesaplarAsync();
    Task<List<BankaHesap>> GetAktifHesaplarAsync();
    Task<BankaHesap?> GetHesapByIdAsync(int id);
    Task<BankaHesap> CreateHesapAsync(BankaHesap hesap);
    Task<BankaHesap> UpdateHesapAsync(BankaHesap hesap);
    Task DeleteHesapAsync(int id);
}
