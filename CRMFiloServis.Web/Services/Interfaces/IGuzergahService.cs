using CRMFiloServis.Shared.Entities;

namespace CRMFiloServis.Web.Services;

public interface IGuzergahService
{
    Task<List<Guzergah>> GetAllAsync();
    Task<List<Guzergah>> GetActiveAsync();
    Task<List<Guzergah>> GetByCariIdAsync(int cariId);
    Task<List<Guzergah>> GetByFirmaIdAsync(int firmaId);
    Task<Guzergah?> GetByIdAsync(int id);
    Task<Guzergah> CreateAsync(Guzergah guzergah);
    Task<Guzergah> AddAsync(Guzergah guzergah);
    Task<Guzergah> UpdateAsync(Guzergah guzergah);
    Task DeleteAsync(int id);
    Task<string> GenerateNextKodAsync();
    Task<string> GenerateGuzergahKoduAsync(int firmaId);
}
