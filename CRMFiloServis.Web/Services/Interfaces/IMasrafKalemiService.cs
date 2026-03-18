using CRMFiloServis.Shared.Entities;

namespace CRMFiloServis.Web.Services;

public interface IMasrafKalemiService
{
    Task<List<MasrafKalemi>> GetAllAsync();
    Task<List<MasrafKalemi>> GetActiveAsync();
    Task<List<MasrafKalemi>> GetByKategoriAsync(MasrafKategori kategori);
    Task<MasrafKalemi?> GetByIdAsync(int id);
    Task<MasrafKalemi> CreateAsync(MasrafKalemi masrafKalemi);
    Task<MasrafKalemi> UpdateAsync(MasrafKalemi masrafKalemi);
    Task DeleteAsync(int id);
    Task<string> GenerateNextKodAsync();
}
