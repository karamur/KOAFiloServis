namespace CRMFiloServis.Web.Services;

public interface IExcelService
{
    byte[] ExportToExcel<T>(List<T> data, string sheetName = "Rapor");
    byte[] ExportServisCalismaRaporu(List<Models.ServisCalismaRaporItem> data);
    byte[] ExportFaturaOdemeRaporu(List<Models.FaturaOdemeRaporItem> data);
    byte[] ExportAracMasrafRaporu(List<Models.AracMasrafRaporItem> data);
}
