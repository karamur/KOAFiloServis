using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace CRMFiloServis.Web.Services;

public class FaturaService : IFaturaService
{
    private readonly ApplicationDbContext _context;

    public FaturaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Fatura>> GetAllAsync()
    {
        return await _context.Faturalar
            .Include(f => f.Cari)
            .OrderByDescending(f => f.FaturaTarihi)
            .ToListAsync();
    }

    public async Task<List<Fatura>> GetByCariIdAsync(int cariId)
    {
        return await _context.Faturalar
            .Include(f => f.Cari)
            .Where(f => f.CariId == cariId)
            .OrderByDescending(f => f.FaturaTarihi)
            .ToListAsync();
    }

    public async Task<List<Fatura>> GetByTipAsync(FaturaTipi tip)
    {
        return await _context.Faturalar
            .Include(f => f.Cari)
            .Where(f => f.FaturaTipi == tip)
            .OrderByDescending(f => f.FaturaTarihi)
            .ToListAsync();
    }

    public async Task<List<Fatura>> GetByDurumAsync(FaturaDurum durum)
    {
        return await _context.Faturalar
            .Include(f => f.Cari)
            .Where(f => f.Durum == durum)
            .OrderByDescending(f => f.FaturaTarihi)
            .ToListAsync();
    }

    public async Task<List<Fatura>> GetOdenmemisFaturalarAsync()
    {
        return await _context.Faturalar
            .Include(f => f.Cari)
            .Where(f => f.Durum == FaturaDurum.Beklemede || f.Durum == FaturaDurum.KismiOdendi)
            .OrderBy(f => f.VadeTarihi)
            .ToListAsync();
    }

    public async Task<List<Fatura>> GetOdenmisFaturalarAsync()
    {
        return await _context.Faturalar
            .Include(f => f.Cari)
            .Where(f => f.Durum == FaturaDurum.Odendi)
            .OrderByDescending(f => f.FaturaTarihi)
            .ToListAsync();
    }

    public async Task<List<Fatura>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Faturalar
            .Include(f => f.Cari)
            .Where(f => f.FaturaTarihi >= startDate && f.FaturaTarihi <= endDate)
            .OrderByDescending(f => f.FaturaTarihi)
            .ToListAsync();
    }

    public async Task<Fatura?> GetByIdAsync(int id)
    {
        return await _context.Faturalar
            .Include(f => f.Cari)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<Fatura?> GetByIdWithKalemlerAsync(int id)
    {
        return await _context.Faturalar
            .Include(f => f.Cari)
            .Include(f => f.FaturaKalemleri)
            .Include(f => f.OdemeEslestirmeleri)
                .ThenInclude(o => o.BankaKasaHareket)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<Fatura> CreateAsync(Fatura fatura)
    {
        // Tutarlarý hesapla
        CalculateTotals(fatura);

        _context.Faturalar.Add(fatura);
        await _context.SaveChangesAsync();
        return fatura;
    }

    public async Task<Fatura> UpdateAsync(Fatura fatura)
    {
        // Tutarlarý yeniden hesapla
        CalculateTotals(fatura);

        _context.Faturalar.Update(fatura);
        await _context.SaveChangesAsync();
        return fatura;
    }

    public async Task DeleteAsync(int id)
    {
        var fatura = await _context.Faturalar.FindAsync(id);
        if (fatura != null)
        {
            fatura.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateNextFaturaNoAsync(FaturaTipi tip)
    {
        var prefix = tip switch
        {
            FaturaTipi.SatisFaturasi => "SF",
            FaturaTipi.AlisFaturasi => "AF",
            FaturaTipi.SatisIadeFaturasi => "SIF",
            FaturaTipi.AlisIadeFaturasi => "AIF",
            _ => "FTR"
        };

        var year = DateTime.Now.Year;
        var lastFatura = await _context.Faturalar
            .IgnoreQueryFilters()
            .Where(f => f.FaturaNo.StartsWith($"{prefix}-{year}"))
            .OrderByDescending(f => f.FaturaNo)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (lastFatura != null)
        {
            var parts = lastFatura.FaturaNo.Split('-');
            if (parts.Length >= 3 && int.TryParse(parts[2], out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}-{year}-{nextNumber:D6}";
    }

    public async Task UpdateOdenenTutarAsync(int faturaId)
    {
        var fatura = await _context.Faturalar
            .Include(f => f.OdemeEslestirmeleri)
            .FirstOrDefaultAsync(f => f.Id == faturaId);

        if (fatura != null)
        {
            fatura.OdenenTutar = fatura.OdemeEslestirmeleri.Sum(o => o.EslestirilenTutar);

            // Durumu güncelle
            if (fatura.OdenenTutar >= fatura.GenelToplam)
            {
                fatura.Durum = FaturaDurum.Odendi;
            }
            else if (fatura.OdenenTutar > 0)
            {
                fatura.Durum = FaturaDurum.KismiOdendi;
            }
            else
            {
                fatura.Durum = FaturaDurum.Beklemede;
            }

            await _context.SaveChangesAsync();
        }
    }

    public async Task<DashboardFaturaStats> GetDashboardStatsAsync()
    {
        var stats = new DashboardFaturaStats();
        var today = DateTime.Today;
        var buAyBaslangic = new DateTime(today.Year, today.Month, 1);

        // Single optimized query for invoices needed for dashboard
        var relevantFaturalar = await _context.Faturalar
            .Include(f => f.Cari)
            .Where(f => f.Durum != FaturaDurum.IptalEdildi)
            .Select(f => new
            {
                f.Id,
                f.FaturaNo,
                f.FaturaTarihi,
                f.FaturaTipi,
                f.VadeTarihi,
                f.GenelToplam,
                f.OdenenTutar,
                f.Durum,
                KalanTutar = f.GenelToplam - f.OdenenTutar,
                CariUnvan = f.Cari.Unvan
            })
            .ToListAsync();

        // Count pending invoices
        stats.BekleyenFaturaSayisi = relevantFaturalar
            .Count(f => f.KalanTutar > 0);

        // Calculate this month's income/expense
        var buAyFaturalar = relevantFaturalar
            .Where(f => f.FaturaTarihi >= buAyBaslangic);
        
        stats.BuAyGelir = buAyFaturalar
            .Where(f => f.FaturaTipi == FaturaTipi.SatisFaturasi)
            .Sum(f => f.GenelToplam);
        
        stats.BuAyGider = buAyFaturalar
            .Where(f => f.FaturaTipi == FaturaTipi.AlisFaturasi)
            .Sum(f => f.GenelToplam);

        // Overdue invoices - need full entity for display
        var vadeGecmisIds = relevantFaturalar
            .Where(f => f.KalanTutar > 0 && f.VadeTarihi.HasValue && f.VadeTarihi.Value < today)
            .OrderBy(f => f.VadeTarihi)
            .Take(10)
            .Select(f => f.Id)
            .ToList();

        if (vadeGecmisIds.Count > 0)
        {
            stats.VadeGecmisFaturalar = await _context.Faturalar
                .Include(f => f.Cari)
                .Where(f => vadeGecmisIds.Contains(f.Id))
                .OrderBy(f => f.VadeTarihi)
                .ToListAsync();
        }

        // Upcoming due invoices
        var vadeYaklasanIds = relevantFaturalar
            .Where(f => f.KalanTutar > 0 && f.VadeTarihi.HasValue && 
                   f.VadeTarihi.Value >= today && f.VadeTarihi.Value <= today.AddDays(7))
            .OrderBy(f => f.VadeTarihi)
            .Take(10)
            .Select(f => f.Id)
            .ToList();

        if (vadeYaklasanIds.Count > 0)
        {
            stats.VadeYaklasanFaturalar = await _context.Faturalar
                .Include(f => f.Cari)
                .Where(f => vadeYaklasanIds.Contains(f.Id))
                .OrderBy(f => f.VadeTarihi)
                .ToListAsync();
        }

        return stats;
    }

    #region E-Fatura / E-Arsiv Metodlari

    public async Task<List<Fatura>> GetByYonAsync(FaturaYonu yon, int? firmaId = null)
    {
        var query = _context.Faturalar
            .Include(f => f.Cari)
            .Where(f => f.FaturaYonu == yon);

        if (firmaId.HasValue)
            query = query.Where(f => f.FirmaId == firmaId.Value);

        return await query.OrderByDescending(f => f.FaturaTarihi).ToListAsync();
    }

    public async Task<List<Fatura>> GetByYonAndDateRangeAsync(FaturaYonu yon, DateTime? baslangic, DateTime? bitis, int? firmaId = null)
    {
        var query = _context.Faturalar
            .Include(f => f.Cari)
            .Where(f => f.FaturaYonu == yon);

        if (firmaId.HasValue)
            query = query.Where(f => f.FirmaId == firmaId.Value);

        if (baslangic.HasValue)
            query = query.Where(f => f.FaturaTarihi >= baslangic.Value);
        
        if (bitis.HasValue)
            query = query.Where(f => f.FaturaTarihi <= bitis.Value);

        return await query.OrderByDescending(f => f.FaturaTarihi).ToListAsync();
    }

    public async Task<List<Fatura>> GetByEFaturaTipiAsync(EFaturaTipi tip)
    {
        return await _context.Faturalar
            .Include(f => f.Cari)
            .Where(f => f.EFaturaTipi == tip)
            .OrderByDescending(f => f.FaturaTarihi)
            .ToListAsync();
    }

    /// <summary>
    /// Excel Import - Ornek dosya formatina gore:
    /// A: Unvani/Adi Soyadi, B: Vkn/Tckn, C: Fatura Tipi, D: Fatura Tarihi, E: Fatura No,
    /// F: Iskonto, G: Kdv Matrahi %0, H: Kdv Matrahi %1, I: Kdv Matrahi %10, J: Kdv Matrahi %20,
    /// K: Kdv%1, L: Kdv%10, M: Kdv%20, N: Odenecek Tutar Turk Lirasi
    /// </summary>
    public async Task<EFaturaImportResult> ImportFromExcelAsync(byte[] fileContent, FaturaYonu yon, int? firmaId = null)
    {
        var result = new EFaturaImportResult();
        
        try
        {
            using var stream = new MemoryStream(fileContent);
            using var package = new OfficeOpenXml.ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            
            if (worksheet == null)
            {
                result.Errors.Add("Excel dosyasinda sayfa bulunamadi.");
                return result;
            }

            var rowCount = worksheet.Dimension?.Rows ?? 0;
            
            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    // Ornek dosya formati:
                    // A: Unvani/Adi Soyadi, B: Vkn/Tckn, C: Fatura Tipi, D: Fatura Tarihi, E: Fatura No
                    // F: Iskonto, G: Kdv Matrahi %0, H: Kdv Matrahi %1, I: Kdv Matrahi %10, J: Kdv Matrahi %20
                    // K: Kdv%1, L: Kdv%10, M: Kdv%20, N: Odenecek Tutar
                    
                    var cariUnvan = worksheet.Cells[row, 1].Text?.Trim();
                    var cariVkn = worksheet.Cells[row, 2].Text?.Trim();
                    var faturaTipiStr = worksheet.Cells[row, 3].Text?.Trim();
                    var tarihStr = worksheet.Cells[row, 4].Text?.Trim();
                    var faturaNo = worksheet.Cells[row, 5].Text?.Trim();
                    var iskonto = ParseDecimal(worksheet.Cells[row, 6].Text);
                    var kdvMatrah0 = ParseDecimal(worksheet.Cells[row, 7].Text);
                    var kdvMatrah1 = ParseDecimal(worksheet.Cells[row, 8].Text);
                    var kdvMatrah10 = ParseDecimal(worksheet.Cells[row, 9].Text);
                    var kdvMatrah20 = ParseDecimal(worksheet.Cells[row, 10].Text);
                    var kdv1 = ParseDecimal(worksheet.Cells[row, 11].Text);
                    var kdv10 = ParseDecimal(worksheet.Cells[row, 12].Text);
                    var kdv20 = ParseDecimal(worksheet.Cells[row, 13].Text);
                    var odenecekTutar = ParseDecimal(worksheet.Cells[row, 14].Text);

                    if (string.IsNullOrEmpty(faturaNo) || string.IsNullOrEmpty(cariUnvan)) continue;

                    // Tarihi parse et
                    DateTime faturaTarihi = DateTime.UtcNow;
                    if (!string.IsNullOrEmpty(tarihStr))
                    {
                        if (DateTime.TryParseExact(tarihStr, new[] { "dd.MM.yyyy", "d.MM.yyyy", "dd.M.yyyy", "d.M.yyyy" }, 
                            System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedTarih))
                        {
                            faturaTarihi = DateTime.SpecifyKind(parsedTarih.Date, DateTimeKind.Utc);
                        }
                        else if (DateTime.TryParse(tarihStr, out parsedTarih))
                        {
                            faturaTarihi = DateTime.SpecifyKind(parsedTarih.Date, DateTimeKind.Utc);
                        }
                    }

                    // KONTROL: Fatura no VE tarih ayni olan kayit var mi?
                    var existingFatura = await _context.Faturalar.FirstOrDefaultAsync(f => 
                        f.FaturaNo == faturaNo && f.FaturaTarihi.Date == faturaTarihi.Date);
                    
                    if (existingFatura != null)
                    {
                        result.SkippedCount++;
                        result.Errors.Add($"Satir {row}: '{faturaNo}' zaten mevcut.");
                        continue;
                    }

                    // CARÝ KONTROL: VKN ile bul, yoksa unvan ile bul, yoksa olustur
                    Cari? cari = null;
                    
                    if (!string.IsNullOrEmpty(cariVkn))
                    {
                        cari = await _context.Cariler.FirstOrDefaultAsync(c => c.VergiNo == cariVkn);
                    }
                    
                    if (cari == null && !string.IsNullOrEmpty(cariUnvan))
                    {
                        cari = await _context.Cariler.FirstOrDefaultAsync(c => 
                            c.Unvan.ToLower() == cariUnvan.ToLower());
                    }
                    
                    if (cari == null)
                    {
                        cari = new Cari
                        {
                            Unvan = cariUnvan,
                            VergiNo = cariVkn ?? "",
                            CariTipi = yon == FaturaYonu.Giden ? CariTipi.Musteri : CariTipi.Tedarikci,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Cariler.Add(cari);
                        await _context.SaveChangesAsync();
                    }

                    // Toplam matrah ve KDV hesapla
                    var toplamMatrah = kdvMatrah0 + kdvMatrah1 + kdvMatrah10 + kdvMatrah20;
                    var toplamKdv = kdv1 + kdv10 + kdv20;
                    var genelToplam = odenecekTutar > 0 ? odenecekTutar : (toplamMatrah + toplamKdv - iskonto);

                    var fatura = new Fatura
                    {
                        FaturaNo = faturaNo,
                        FaturaTarihi = faturaTarihi,
                        CariId = cari.Id,
                        FirmaId = firmaId,
                        FaturaYonu = yon,
                        FaturaTipi = yon == FaturaYonu.Giden ? FaturaTipi.SatisFaturasi : FaturaTipi.AlisFaturasi,
                        EFaturaTipi = faturaTipiStr?.ToUpper() == "SATIS" ? EFaturaTipi.EFatura : EFaturaTipi.EArsiv,
                        AraToplam = toplamMatrah,
                        IskontoTutar = iskonto,
                        KdvTutar = toplamKdv,
                        GenelToplam = genelToplam,
                        ImportKaynak = "Excel",
                        Durum = FaturaDurum.Beklemede,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Faturalar.Add(fatura);
                    result.ImportedItems.Add(fatura);
                    result.ImportedCount++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Satir {row}: {ex.Message}");
                    result.ErrorCount++;
                }
            }

            await _context.SaveChangesAsync();
            result.Success = result.ImportedCount > 0;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Excel okuma hatasi: {ex.Message}");
        }

        return result;
    }

    private static decimal ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0;
        value = value.Replace(".", "").Replace(",", ".").Trim();
        return decimal.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : 0;
    }

    /// <summary>
    /// Excel Sablon - Ornek dosya formatina gore:
    /// A: Unvani/Adi Soyadi, B: Vkn/Tckn, C: Fatura Tipi, D: Fatura Tarihi, E: Fatura No,
    /// F: Iskonto, G: Kdv Matrahi %0, H: Kdv Matrahi %1, I: Kdv Matrahi %10, J: Kdv Matrahi %20,
    /// K: Kdv%1, L: Kdv%10, M: Kdv%20, N: Odenecek Tutar Turk Lirasi
    /// </summary>
    public async Task<byte[]> GetExcelSablonAsync(FaturaYonu yon)
    {
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Fatura Import");

        // Basliklar - Ornek dosya formatinda
        var headers = new[] { 
            "Ünvaný/Adý Soyadý", "Vkn/Tckn", "Fatura Tipi", "Fatura Tarihi", "Fatura No.",
            "Ýskonto", "Kdv Matrahý %0", "Kdv Matrahý %1", "Kdv Matrahý %10", "Kdv Matrahý %20",
            "Kdv%1", "Kdv%10", "Kdv%20", "Ödenecek Tutar Türk Lirasý"
        };
        
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cells[1, i + 1].Value = headers[i];
            ws.Cells[1, i + 1].Style.Font.Bold = true;
            ws.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            ws.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
        }

        // Ornek satir
        ws.Cells[2, 1].Value = "ORNEK FIRMA A.S.";
        ws.Cells[2, 2].Value = "1234567890";
        ws.Cells[2, 3].Value = "SATIS";
        ws.Cells[2, 4].Value = DateTime.Today.ToString("dd.MM.yyyy");
        ws.Cells[2, 5].Value = $"FTR{DateTime.Now:yyyyMM}000001";
        ws.Cells[2, 6].Value = "0,00";
        ws.Cells[2, 7].Value = ""; // %0 matrah
        ws.Cells[2, 8].Value = ""; // %1 matrah
        ws.Cells[2, 9].Value = ""; // %10 matrah
        ws.Cells[2, 10].Value = "1000,00"; // %20 matrah
        ws.Cells[2, 11].Value = ""; // %1 kdv
        ws.Cells[2, 12].Value = ""; // %10 kdv
        ws.Cells[2, 13].Value = "200,00"; // %20 kdv
        ws.Cells[2, 14].Value = "1200,00"; // Toplam

        // Aciklamalar
        ws.Cells[5, 1].Value = "ACIKLAMALAR:";
        ws.Cells[5, 1].Style.Font.Bold = true;
        ws.Cells[6, 1].Value = "* Ünvaný: Cari unvani (zorunlu)";
        ws.Cells[7, 1].Value = "* Vkn/Tckn: Vergi veya TC kimlik no (varsa mevcut cari bulunur, yoksa yeni olusturulur)";
        ws.Cells[8, 1].Value = "* Fatura Tipi: SATIS veya ALIS";
        ws.Cells[9, 1].Value = "* Fatura Tarihi: GG.AA.YYYY formatinda";
        ws.Cells[10, 1].Value = "* Fatura No: Benzersiz fatura numarasi (zorunlu)";
        ws.Cells[11, 1].Value = "* KDV Matrahlari: Ýlgili KDV oranina gore matrah tutarlari";
        ws.Cells[12, 1].Value = "* KDV Tutarlari: Her oran icin KDV tutarlari";
        ws.Cells[13, 1].Value = "* Odenecek Tutar: Toplam fatura tutari";

        ws.Cells.AutoFitColumns();
        return await Task.FromResult(package.GetAsByteArray());
    }

    public async Task<byte[]> ExportToExcelAsync(List<Fatura> faturalar)
    {
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Faturalar");

        // Basliklar
        var headers = new[] { "Fatura No", "Tarih", "Vade", "Cari", "VKN", "Matrah", "KDV", "Toplam", "Odenen", "Kalan", "Durum", "Tip", "ETTN" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cells[1, i + 1].Value = headers[i];
            ws.Cells[1, i + 1].Style.Font.Bold = true;
            ws.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            ws.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
        }

        // Veriler
        int row = 2;
        foreach (var f in faturalar)
        {
            ws.Cells[row, 1].Value = f.FaturaNo;
            ws.Cells[row, 2].Value = f.FaturaTarihi.ToString("dd.MM.yyyy");
            ws.Cells[row, 3].Value = f.VadeTarihi?.ToString("dd.MM.yyyy");
            ws.Cells[row, 4].Value = f.Cari?.Unvan;
            ws.Cells[row, 5].Value = f.Cari?.VergiNo;
            ws.Cells[row, 6].Value = f.AraToplam;
            ws.Cells[row, 7].Value = f.KdvTutar;
            ws.Cells[row, 8].Value = f.GenelToplam;
            ws.Cells[row, 9].Value = f.OdenenTutar;
            ws.Cells[row, 10].Value = f.KalanTutar;
            ws.Cells[row, 11].Value = f.Durum.ToString();
            ws.Cells[row, 12].Value = f.EFaturaTipi == EFaturaTipi.EFatura ? "E-Fatura" : "E-Arsiv";
            ws.Cells[row, 13].Value = f.EttnNo;
            row++;
        }

        // Ozet satiri
        row++;
        ws.Cells[row, 5].Value = "TOPLAM:";
        ws.Cells[row, 5].Style.Font.Bold = true;
        ws.Cells[row, 6].Value = faturalar.Sum(f => f.AraToplam);
        ws.Cells[row, 7].Value = faturalar.Sum(f => f.KdvTutar);
        ws.Cells[row, 8].Value = faturalar.Sum(f => f.GenelToplam);
        ws.Cells[row, 9].Value = faturalar.Sum(f => f.OdenenTutar);
        ws.Cells[row, 10].Value = faturalar.Sum(f => f.KalanTutar);

        // Format
        ws.Cells[2, 6, row, 10].Style.Numberformat.Format = "#,##0.00";
        ws.Cells.AutoFitColumns();

        return await Task.FromResult(package.GetAsByteArray());
    }

    #endregion

    private static void CalculateTotals(Fatura fatura)
    {
        if (fatura.FaturaKalemleri != null && fatura.FaturaKalemleri.Count != 0)
        {
            foreach (var kalem in fatura.FaturaKalemleri)
            {
                var netTutar = kalem.Miktar * kalem.BirimFiyat;
                kalem.KdvTutar = netTutar * kalem.KdvOrani / 100;
                kalem.ToplamTutar = netTutar + kalem.KdvTutar;
            }

            fatura.AraToplam = fatura.FaturaKalemleri.Sum(k => k.Miktar * k.BirimFiyat);
            fatura.KdvTutar = fatura.FaturaKalemleri.Sum(k => k.KdvTutar);
            fatura.GenelToplam = fatura.FaturaKalemleri.Sum(k => k.ToplamTutar);
        }
    }
}
