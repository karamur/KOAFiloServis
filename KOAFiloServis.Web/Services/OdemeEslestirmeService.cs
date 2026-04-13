using KOAFiloServis.Shared.Entities;
using KOAFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace KOAFiloServis.Web.Services;

public class OdemeEslestirmeService : IOdemeEslestirmeService
{
    private readonly ApplicationDbContext _context;
    private readonly IFaturaService _faturaService;

    public OdemeEslestirmeService(ApplicationDbContext context, IFaturaService faturaService)
    {
        _context = context;
        _faturaService = faturaService;
    }

    public async Task<List<OdemeEslestirme>> GetAllAsync()
    {
        return await _context.OdemeEslestirmeleri
            .Include(e => e.Fatura)
                .ThenInclude(f => f.Cari)
            .Include(e => e.BankaKasaHareket)
                .ThenInclude(h => h.BankaHesap)
            .OrderByDescending(e => e.EslestirmeTarihi)
            .ToListAsync();
    }

    public async Task<List<OdemeEslestirme>> GetByFaturaIdAsync(int faturaId)
    {
        return await _context.OdemeEslestirmeleri
            .Include(e => e.BankaKasaHareket)
                .ThenInclude(h => h.BankaHesap)
            .Where(e => e.FaturaId == faturaId)
            .OrderByDescending(e => e.EslestirmeTarihi)
            .ToListAsync();
    }

    public async Task<List<OdemeEslestirme>> GetByHareketIdAsync(int hareketId)
    {
        return await _context.OdemeEslestirmeleri
            .Include(e => e.Fatura)
                .ThenInclude(f => f.Cari)
            .Where(e => e.BankaKasaHareketId == hareketId)
            .OrderByDescending(e => e.EslestirmeTarihi)
            .ToListAsync();
    }

    public async Task<OdemeEslestirme?> GetByIdAsync(int id)
    {
        return await _context.OdemeEslestirmeleri
            .Include(e => e.Fatura)
            .Include(e => e.BankaKasaHareket)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<OdemeEslestirme> CreateAsync(OdemeEslestirme eslestirme)
    {
        _context.OdemeEslestirmeleri.Add(eslestirme);
        await _context.SaveChangesAsync();

        // Faturan�n �denen tutar�n� g�ncelle
        await _faturaService.UpdateOdenenTutarAsync(eslestirme.FaturaId);

        return eslestirme;
    }

    public async Task DeleteAsync(int id)
    {
        var eslestirme = await _context.OdemeEslestirmeleri.FindAsync(id);
        if (eslestirme != null)
        {
            var faturaId = eslestirme.FaturaId;
            eslestirme.IsDeleted = true;
            await _context.SaveChangesAsync();

            // Faturan�n �denen tutar�n� g�ncelle
            await _faturaService.UpdateOdenenTutarAsync(faturaId);
        }
    }

    public async Task<decimal> GetFaturaEslestirilenTutarAsync(int faturaId)
    {
        return await _context.OdemeEslestirmeleri
            .Where(e => e.FaturaId == faturaId)
            .SumAsync(e => e.EslestirilenTutar);
    }

    public async Task<decimal> GetHareketEslestirilenTutarAsync(int hareketId)
    {
        return await _context.OdemeEslestirmeleri
            .Where(e => e.BankaKasaHareketId == hareketId)
            .SumAsync(e => e.EslestirilenTutar);
    }
}
