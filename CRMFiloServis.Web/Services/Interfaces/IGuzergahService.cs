using CRMFiloServis.Shared.Entities;

namespace CRMFiloServis.Web.Services;

public interface IGuzergahService
{
    Task<List<Guzergah>> GetAllAsync();
    Task<List<Guzergah>> GetActiveAsync();
    Task<List<Guzergah>> GetByCariIdAsync(int cariId);
    Task<Guzergah?> GetByIdAsync(int id);
    Task<Guzergah> CreateAsync(Guzergah guzergah);
    Task<Guzergah> UpdateAsync(Guzergah guzergah);
    Task DeleteAsync(int id);
    Task<string> GenerateNextKodAsync();
}
