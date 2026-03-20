using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Models;

namespace CRMFiloServis.Web.Services;

public interface IMaliAnalizService
{
    // Dashboard
    Task<MaliAnalizDashboard> GetDashboardAsync(int yil, int ay);

    // Özmal Araç Raporu
    Task<OzmalAracRaporu> GetOzmalAracRaporuAsync(int yil, int ay);

    // Kiralýk Araç Raporu
    Task<KiralikAracRaporu> GetKiralikAracRaporuAsync(int yil, int ay);

    // Komisyon Raporu
    Task<KomisyonRaporu> GetKomisyonRaporuAsync(int yil, int ay);

    // Checklist
    Task<ChecklistOzet> GetChecklistOzetAsync(int yil, int ay);

    // Trend Analizi
    Task<List<GrafikVeri>> GetYillikTrendAsync(int yil);
}
