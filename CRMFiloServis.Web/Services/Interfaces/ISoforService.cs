using CRMFiloServis.Shared.Entities;

namespace CRMFiloServis.Web.Services;

public interface ISoforService
{
    Task<List<Sofor>> GetAllAsync();
    Task<List<Sofor>> GetActiveAsync();
    Task<Sofor?> GetByIdAsync(int id);
    Task<Sofor> CreateAsync(Sofor sofor);
    Task<Sofor> UpdateAsync(Sofor sofor);
    Task DeleteAsync(int id);
    Task<string> GenerateNextKodAsync();
}
