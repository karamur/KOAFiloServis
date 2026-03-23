using CRMFiloServis.Shared.Entities;

namespace CRMFiloServis.Web.Services;

public interface ICariService
{
    Task<List<Cari>> GetAllAsync();
    Task<List<Cari>> GetAllWithBakiyeAsync(); // Borc/Alacak hesaplanmis
    Task<int> GetCountAsync();
    Task<Cari?> GetByIdAsync(int id);
    Task<Cari?> GetByKodAsync(string cariKodu);
    Task<List<Cari>> GetByTipAsync(CariTipi tip);
    Task<Cari> CreateAsync(Cari cari);
    Task<Cari> UpdateAsync(Cari cari);
    Task DeleteAsync(int id);
    Task<string> GenerateNextKodAsync();
}
