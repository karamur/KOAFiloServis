using CRMFiloServis.Shared.Entities;

namespace CRMFiloServis.Web.Services;

public interface IAracService
{
    Task<List<Arac>> GetAllAsync();
    Task<List<Arac>> GetActiveAsync();
    Task<Arac?> GetByIdAsync(int id);
    Task<Arac?> GetByPlakaAsync(string plaka);
    Task<Arac> CreateAsync(Arac arac);
    Task<Arac> UpdateAsync(Arac arac);
    Task DeleteAsync(int id);
}
