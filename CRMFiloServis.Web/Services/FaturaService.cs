using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

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
