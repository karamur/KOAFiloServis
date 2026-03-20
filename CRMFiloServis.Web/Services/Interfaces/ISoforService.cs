using CRMFiloServis.Shared.Entities;

namespace CRMFiloServis.Web.Services;

public interface ISoforService
{
    // Tüm Personel Ýþlemleri
    Task<List<Sofor>> GetAllAsync();
    Task<List<Sofor>> GetActiveAsync();
    Task<int> GetActiveCountAsync();
    Task<Sofor?> GetByIdAsync(int id);
    Task<Sofor> CreateAsync(Sofor sofor);
    Task<Sofor> UpdateAsync(Sofor sofor);
    Task DeleteAsync(int id);
    Task<string> GenerateNextKodAsync();
    
    // Görev bazlý filtreleme
    Task<List<Sofor>> GetByGorevAsync(PersonelGorev gorev);
    Task<List<Sofor>> GetActiveSoforlerAsync(); // Sadece aktif þoförler
    Task<List<Sofor>> GetActiveByGorevAsync(PersonelGorev gorev);
}
