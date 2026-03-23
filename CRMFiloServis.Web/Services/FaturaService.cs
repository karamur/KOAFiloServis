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

    public async Task<List<Fatura>> GetByYonAsync(FaturaYonu yon)
    {
        return await _context.Faturalar
            .Include(f => f.Cari)
            .Where(f => f.FaturaYonu == yon)
            .OrderByDescending(f => f.FaturaTarihi)
            .ToListAsync();
    }

    public async Task<List<Fatura>> GetByYonAndDateRangeAsync(FaturaYonu yon, DateTime? baslangic, DateTime? bitis)
    {
        var query = _context.Faturalar
            .Include(f => f.Cari)
            .Where(f => f.FaturaYonu == yon);

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

    public async Task<EFaturaImportResult> ImportFromExcelAsync(byte[] fileContent, FaturaYonu yon)
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
                    var faturaNo = worksheet.Cells[row, 1].Text?.Trim();
                    var tarihStr = worksheet.Cells[row, 2].Text?.Trim();
                    var cariVkn = worksheet.Cells[row, 3].Text?.Trim();
                    var cariUnvan = worksheet.Cells[row, 4].Text?.Trim();
                    var araToplam = ParseDecimal(worksheet.Cells[row, 5].Text);
                    var kdvTutar = ParseDecimal(worksheet.Cells[row, 6].Text);
                    var genelToplam = ParseDecimal(worksheet.Cells[row, 7].Text);
                    var ettnNo = worksheet.Cells[row, 8].Text?.Trim();
                    var eFaturaTipiStr = worksheet.Cells[row, 9].Text?.Trim();

                    if (string.IsNullOrEmpty(faturaNo)) continue;

                    // Tarihi parse et
                    DateTime faturaTarihi = DateTime.UtcNow;
                    if (DateTime.TryParse(tarihStr, out var parsedTarih))
                    {
                        faturaTarihi = DateTime.SpecifyKind(parsedTarih.Date, DateTimeKind.Utc);
                    }

                    // KONTROL: Fatura no VE tarih ayni olan kayit var mi?
                    var existingFatura = await _context.Faturalar.FirstOrDefaultAsync(f => 
                        f.FaturaNo == faturaNo && f.FaturaTarihi.Date == faturaTarihi.Date);
                    
                    if (existingFatura != null)
                    {
                        result.SkippedCount++;
                        result.Errors.Add($"Satir {row}: '{faturaNo}' numarali fatura {faturaTarihi:dd.MM.yyyy} tarihinde zaten mevcut.");
                        continue;
                    }

                    // CARÝ KONTROL: VKN ile bul, yoksa unvan ile bul, yoksa olustur
                    Cari? cari = null;
                    
                    // Oncelikle VKN ile ara
                    if (!string.IsNullOrEmpty(cariVkn))
                    {
                        cari = await _context.Cariler.FirstOrDefaultAsync(c => c.VergiNo == cariVkn);
                    }
                    
                    // VKN ile bulunamadiysa unvan ile ara
                    if (cari == null && !string.IsNullOrEmpty(cariUnvan))
                    {
                        cari = await _context.Cariler.FirstOrDefaultAsync(c => 
                            c.Unvan.ToLower() == cariUnvan.ToLower());
                    }
                    
                    // Hala bulunamadiysa yeni cari olustur
                    if (cari == null && !string.IsNullOrEmpty(cariUnvan))
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

                    if (cari == null)
                    {
                        result.Errors.Add($"Satir {row}: Cari bulunamadi veya olusturulamadi.");
                        result.ErrorCount++;
                        continue;
                    }

                    var fatura = new Fatura
                    {
                        FaturaNo = faturaNo,
                        FaturaTarihi = faturaTarihi,
                        CariId = cari.Id,
                        FaturaYonu = yon,
                        FaturaTipi = yon == FaturaYonu.Giden ? FaturaTipi.SatisFaturasi : FaturaTipi.AlisFaturasi,
                        EFaturaTipi = eFaturaTipiStr?.ToLower() == "e-fatura" ? EFaturaTipi.EFatura : EFaturaTipi.EArsiv,
                        AraToplam = araToplam,
                        KdvTutar = kdvTutar,
                        GenelToplam = genelToplam > 0 ? genelToplam : araToplam + kdvTutar,
                        EttnNo = ettnNo,
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

    public async Task<EFaturaImportResult> ImportFromLucaAsync(byte[] fileContent, FaturaYonu yon)
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
            
            // Luca formatinda kolonlar:
            // A: Fatura No, B: Tarih, C: VKN/TCKN, D: Unvan, E: Matrah, F: KDV, G: Toplam, H: ETTN, I: Fatura Tipi
            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var faturaNo = worksheet.Cells[row, 1].Text?.Trim();
                    if (string.IsNullOrEmpty(faturaNo)) continue;

                    var tarihStr = worksheet.Cells[row, 2].Text?.Trim();
                    
                    // Tarihi parse et
                    DateTime faturaTarihi = DateTime.UtcNow;
                    if (DateTime.TryParse(tarihStr, out var parsedTarih))
                    {
                        faturaTarihi = DateTime.SpecifyKind(parsedTarih.Date, DateTimeKind.Utc);
                    }

                    // KONTROL: Fatura no VE tarih ayni olan kayit var mi?
                    var existingFatura = await _context.Faturalar.FirstOrDefaultAsync(f => 
                        f.FaturaNo == faturaNo && f.FaturaTarihi.Date == faturaTarihi.Date);
                    
                    if (existingFatura != null)
                    {
                        result.SkippedCount++;
                        result.Errors.Add($"Satir {row}: '{faturaNo}' numarali fatura {faturaTarihi:dd.MM.yyyy} tarihinde zaten mevcut.");
                        continue;
                    }

                    var cariVkn = worksheet.Cells[row, 3].Text?.Trim();
                    var cariUnvan = worksheet.Cells[row, 4].Text?.Trim();
                    var matrah = ParseDecimal(worksheet.Cells[row, 5].Text);
                    var kdvTutar = ParseDecimal(worksheet.Cells[row, 6].Text);
                    var genelToplam = ParseDecimal(worksheet.Cells[row, 7].Text);
                    var ettnNo = worksheet.Cells[row, 8].Text?.Trim();
                    var faturaTipiStr = worksheet.Cells[row, 9].Text?.Trim();

                    // CARÝ KONTROL: VKN ile bul, yoksa unvan ile bul, yoksa olustur
                    Cari? cari = null;
                    
                    // Oncelikle VKN ile ara
                    if (!string.IsNullOrEmpty(cariVkn))
                    {
                        cari = await _context.Cariler.FirstOrDefaultAsync(c => c.VergiNo == cariVkn);
                    }
                    
                    // VKN ile bulunamadiysa unvan ile ara
                    if (cari == null && !string.IsNullOrEmpty(cariUnvan))
                    {
                        cari = await _context.Cariler.FirstOrDefaultAsync(c => 
                            c.Unvan.ToLower() == cariUnvan.ToLower());
                    }
                    
                    // Hala bulunamadiysa yeni cari olustur
                    if (cari == null && !string.IsNullOrEmpty(cariUnvan))
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

                    if (cari == null)
                    {
                        result.Errors.Add($"Satir {row}: Cari bulunamadi veya olusturulamadi.");
                        result.ErrorCount++;
                        continue;
                    }

                    var eFaturaTipi = faturaTipiStr?.ToLower() switch
                    {
                        "e-fatura" or "efatura" => EFaturaTipi.EFatura,
                        _ => EFaturaTipi.EArsiv
                    };

                    var fatura = new Fatura
                    {
                        FaturaNo = faturaNo,
                        FaturaTarihi = faturaTarihi,
                        CariId = cari.Id,
                        FaturaYonu = yon,
                        FaturaTipi = yon == FaturaYonu.Giden ? FaturaTipi.SatisFaturasi : FaturaTipi.AlisFaturasi,
                        EFaturaTipi = eFaturaTipi,
                        AraToplam = matrah,
                        KdvTutar = kdvTutar,
                        GenelToplam = genelToplam > 0 ? genelToplam : matrah + kdvTutar,
                        EttnNo = ettnNo,
                        ImportKaynak = "Luca",
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
            result.Errors.Add($"Luca dosyasi okuma hatasi: {ex.Message}");
        }

        return result;
    }

    private static decimal ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0;
        value = value.Replace(".", "").Replace(",", ".").Trim();
        return decimal.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : 0;
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
