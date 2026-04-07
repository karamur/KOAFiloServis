using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Models;

namespace CRMFiloServis.Web.Services;

public interface ICariService
{
    Task<List<Cari>> GetAllAsync();
    Task<List<Cari>> GetAllWithBakiyeAsync(); // Borc/Alacak hesaplanmis
    Task<PagedResult<Cari>> GetPagedAsync(CariFilterParams filter); // Sayfalı ve filtrelenmiş
    Task<int> GetCountAsync();
    Task<Cari?> GetByIdAsync(int id);
    Task<Cari?> GetByKodAsync(string cariKodu);
    Task<List<Cari>> GetByTipAsync(CariTipi tip);
    Task<Cari> CreateAsync(Cari cari);
    Task<Cari> UpdateAsync(Cari cari);
    Task<Cari> MatchMuhasebeHesapByKodAsync(int cariId, string hesapKodu);
    Task<Cari> EnsureMuhasebeHesapAsync(int cariId);
    Task<bool> DeleteAsync(int id);
    Task<string> GenerateNextKodAsync();
}

/// <summary>
/// Cari listesi için filtre parametreleri
/// </summary>
public class CariFilterParams : PagingParameters
{
    public string? SearchTerm { get; set; }
    public CariTipi? CariTipi { get; set; }
    public string? DurumFiltre { get; set; } // borclu, alacakli, sifir, islemsiz
    public bool? Aktif { get; set; } = true;
}
