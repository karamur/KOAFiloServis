using ClosedXML.Excel;
using CRMFiloServis.Web.Models;

namespace CRMFiloServis.Web.Services;

public class ExcelService : IExcelService
{
    public byte[] ExportToExcel<T>(List<T> data, string sheetName = "Rapor")
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);

        // Header ve data ekle
        worksheet.Cell(1, 1).InsertTable(data);

        // Stil ayarlarý
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportServisCalismaRaporu(List<ServisCalismaRaporItem> data)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Servis Çalýþma Raporu");

        // Baþlýklar
        var headers = new[] { "Firma", "Güzergah", "Plaka", "Þoför", "Servis Türü", "Birim Fiyat", "Çalýþýlan Gün", "Toplam Tutar" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
        }

        // Header stilini ayarla
        var headerRange = worksheet.Range(1, 1, 1, headers.Length);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

        // Veriler
        int row = 2;
        foreach (var item in data)
        {
            worksheet.Cell(row, 1).Value = item.FirmaAdi;
            worksheet.Cell(row, 2).Value = item.GuzergahAdi;
            worksheet.Cell(row, 3).Value = item.Plaka;
            worksheet.Cell(row, 4).Value = item.SoforAdi;
            worksheet.Cell(row, 5).Value = item.ServisTuru;
            worksheet.Cell(row, 6).Value = item.BirimFiyat;
            worksheet.Cell(row, 7).Value = item.CalisilanGun;
            worksheet.Cell(row, 8).Value = item.ToplamTutar;
            row++;
        }

        // Toplam satýrý
        worksheet.Cell(row, 6).Value = "TOPLAM:";
        worksheet.Cell(row, 6).Style.Font.Bold = true;
        worksheet.Cell(row, 7).FormulaA1 = $"SUM(G2:G{row - 1})";
        worksheet.Cell(row, 8).FormulaA1 = $"SUM(H2:H{row - 1})";
        worksheet.Cell(row, 7).Style.Font.Bold = true;
        worksheet.Cell(row, 8).Style.Font.Bold = true;

        // Para formatý
        worksheet.Column(6).Style.NumberFormat.Format = "#,##0.00 ?";
        worksheet.Column(8).Style.NumberFormat.Format = "#,##0.00 ?";

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportFaturaOdemeRaporu(List<FaturaOdemeRaporItem> data)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Fatura Ödeme Raporu");

        // Baþlýklar
        var headers = new[] { "Fatura No", "Fatura Tarihi", "Vade Tarihi", "Cari", "Fatura Tipi", "Durum", "Genel Toplam", "Ödenen", "Kalan", "Vade Günü" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
        }

        var headerRange = worksheet.Range(1, 1, 1, headers.Length);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGreen;
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

        // Veriler
        int row = 2;
        foreach (var item in data)
        {
            worksheet.Cell(row, 1).Value = item.FaturaNo;
            worksheet.Cell(row, 2).Value = item.FaturaTarihi.ToString("dd.MM.yyyy");
            worksheet.Cell(row, 3).Value = item.VadeTarihi?.ToString("dd.MM.yyyy") ?? "-";
            worksheet.Cell(row, 4).Value = item.CariUnvan;
            worksheet.Cell(row, 5).Value = item.FaturaTipi;
            worksheet.Cell(row, 6).Value = item.Durum;
            worksheet.Cell(row, 7).Value = item.GenelToplam;
            worksheet.Cell(row, 8).Value = item.OdenenTutar;
            worksheet.Cell(row, 9).Value = item.KalanTutar;
            worksheet.Cell(row, 10).Value = item.VadeGunu;

            // Gecikmiþ ödemeleri kýrmýzý yap
            if (item.VadeGunu < 0 && item.KalanTutar > 0)
            {
                worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.LightPink;
            }
            row++;
        }

        // Toplam satýrý
        worksheet.Cell(row, 6).Value = "TOPLAM:";
        worksheet.Cell(row, 6).Style.Font.Bold = true;
        worksheet.Cell(row, 7).FormulaA1 = $"SUM(G2:G{row - 1})";
        worksheet.Cell(row, 8).FormulaA1 = $"SUM(H2:H{row - 1})";
        worksheet.Cell(row, 9).FormulaA1 = $"SUM(I2:I{row - 1})";

        // Para formatý
        worksheet.Column(7).Style.NumberFormat.Format = "#,##0.00 ?";
        worksheet.Column(8).Style.NumberFormat.Format = "#,##0.00 ?";
        worksheet.Column(9).Style.NumberFormat.Format = "#,##0.00 ?";

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportAracMasrafRaporu(List<AracMasrafRaporItem> data)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Araç Masraf Raporu");

        // Baþlýklar
        var headers = new[] { "Tarih", "Plaka", "Masraf Kalemi", "Kategori", "Güzergah", "Tutar", "Belge No", "Açýklama", "Arýza Kaynaklý" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
        }

        var headerRange = worksheet.Range(1, 1, 1, headers.Length);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightCoral;
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

        // Veriler
        int row = 2;
        foreach (var item in data)
        {
            worksheet.Cell(row, 1).Value = item.MasrafTarihi.ToString("dd.MM.yyyy");
            worksheet.Cell(row, 2).Value = item.Plaka;
            worksheet.Cell(row, 3).Value = item.MasrafKalemi;
            worksheet.Cell(row, 4).Value = item.Kategori;
            worksheet.Cell(row, 5).Value = item.GuzergahAdi ?? "-";
            worksheet.Cell(row, 6).Value = item.Tutar;
            worksheet.Cell(row, 7).Value = item.BelgeNo ?? "-";
            worksheet.Cell(row, 8).Value = item.Aciklama ?? "-";
            worksheet.Cell(row, 9).Value = item.ArizaKaynakli ? "Evet" : "Hayýr";
            row++;
        }

        // Toplam satýrý
        worksheet.Cell(row, 5).Value = "TOPLAM:";
        worksheet.Cell(row, 5).Style.Font.Bold = true;
        worksheet.Cell(row, 6).FormulaA1 = $"SUM(F2:F{row - 1})";
        worksheet.Cell(row, 6).Style.Font.Bold = true;

        // Para formatý
        worksheet.Column(6).Style.NumberFormat.Format = "#,##0.00 ?";

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
