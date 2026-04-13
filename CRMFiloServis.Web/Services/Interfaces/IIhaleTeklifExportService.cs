namespace CRMFiloServis.Web.Services;

public interface IIhaleTeklifExportService
{
    Task<byte[]> ExportPdfAsync(int versiyonId);
    Task<byte[]> ExportExcelAsync(int versiyonId);
}
