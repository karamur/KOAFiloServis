using KOAFiloServis.Shared.Entities;

namespace KOAFiloServis.Web.Services;

public interface IBankaHesapService
{
    Task<List<BankaHesap>> GetAllAsync();
    Task<List<BankaHesap>> GetActiveAsync();
    Task<List<BankaHesap>> GetByTipAsync(HesapTipi tip);
    Task<BankaHesap?> GetByIdAsync(int id);
    Task<BankaHesap> CreateAsync(BankaHesap bankaHesap);
    Task<BankaHesap> UpdateAsync(BankaHesap bankaHesap);
    Task DeleteAsync(int id);
    Task<string> GenerateNextKodAsync();
    Task<decimal> GetBakiyeAsync(int hesapId);
    Task<Dictionary<int, decimal>> GetTumHesapBakiyeleriAsync();
}
