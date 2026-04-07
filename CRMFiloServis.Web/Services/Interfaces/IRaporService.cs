using CRMFiloServis.Web.Models;

namespace CRMFiloServis.Web.Services;

public interface IRaporService
{
    Task<List<ServisCalismaRaporItem>> GetServisCalismaRaporuAsync(
        DateTime startDate,
        DateTime endDate,
        int? aracId = null,
        int? soforId = null,
        int? guzergahId = null,
        int? cariId = null);

    Task<List<FaturaOdemeRaporItem>> GetFaturaOdemeRaporuAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? cariId = null,
        bool? sadeceBekleyenler = null);

    Task<List<AracMasrafRaporItem>> GetAracMasrafRaporuAsync(
        DateTime startDate,
        DateTime endDate,
        int? aracId = null);

    Task<CariEkstre> GetCariEkstreAsync(
        int cariId,
        DateTime? startDate = null,
        DateTime? endDate = null);

    // Şoför Performans Raporu
    Task<SoforPerformansOzet> GetSoforPerformansAsync(
        int soforId,
        DateTime startDate,
        DateTime endDate);

    Task<List<SoforKarsilastirmaOzeti>> GetSoforKarsilastirmaAsync(
        DateTime startDate,
        DateTime endDate);
}
