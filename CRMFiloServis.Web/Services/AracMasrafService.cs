using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class AracMasrafService : IAracMasrafService
{
    private readonly ApplicationDbContext _context;

    public AracMasrafService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AracMasraf>> GetAllAsync()
    {
        return await _context.AracMasraflari
            .Include(m => m.Arac)
            .Include(m => m.MasrafKalemi)
            .Include(m => m.Guzergah)
            .OrderByDescending(m => m.MasrafTarihi)
            .ToListAsync();
    }

    public async Task<List<AracMasraf>> GetByAracIdAsync(int aracId)
    {
        return await _context.AracMasraflari
            .Include(m => m.MasrafKalemi)
            .Include(m => m.Guzergah)
            .Where(m => m.AracId == aracId)
            .OrderByDescending(m => m.MasrafTarihi)
            .ToListAsync();
    }

    public async Task<List<AracMasraf>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.AracMasraflari
            .Include(m => m.Arac)
            .Include(m => m.MasrafKalemi)
            .Include(m => m.Guzergah)
            .Where(m => m.MasrafTarihi >= startDate && m.MasrafTarihi <= endDate)
            .OrderByDescending(m => m.MasrafTarihi)
            .ToListAsync();
    }

    public async Task<List<AracMasraf>> GetByAracAndDateRangeAsync(int aracId, DateTime startDate, DateTime endDate)
    {
        return await _context.AracMasraflari
            .Include(m => m.MasrafKalemi)
            .Include(m => m.Guzergah)
            .Where(m => m.AracId == aracId && m.MasrafTarihi >= startDate && m.MasrafTarihi <= endDate)
            .OrderByDescending(m => m.MasrafTarihi)
            .ToListAsync();
    }

    public async Task<List<AracMasraf>> GetArizaMasraflariAsync()
    {
        return await _context.AracMasraflari
            .Include(m => m.Arac)
            .Include(m => m.MasrafKalemi)
            .Include(m => m.Guzergah)
            .Include(m => m.ServisCalisma)
            .Where(m => m.ArizaKaynaklimi)
            .OrderByDescending(m => m.MasrafTarihi)
            .ToListAsync();
    }

    public async Task<AracMasraf?> GetByIdAsync(int id)
    {
        return await _context.AracMasraflari
            .Include(m => m.Arac)
            .Include(m => m.MasrafKalemi)
            .Include(m => m.Guzergah)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<AracMasraf> CreateAsync(AracMasraf aracMasraf)
    {
        _context.AracMasraflari.Add(aracMasraf);
        await _context.SaveChangesAsync();
        return aracMasraf;
    }

    public async Task<AracMasraf> UpdateAsync(AracMasraf aracMasraf)
    {
        _context.AracMasraflari.Update(aracMasraf);
        await _context.SaveChangesAsync();
        return aracMasraf;
    }

    public async Task DeleteAsync(int id)
    {
        var aracMasraf = await _context.AracMasraflari.FindAsync(id);
        if (aracMasraf != null)
        {
            aracMasraf.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<decimal> GetToplamMasrafByAracAsync(int aracId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.AracMasraflari.Where(m => m.AracId == aracId);

        if (startDate.HasValue)
            query = query.Where(m => m.MasrafTarihi >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(m => m.MasrafTarihi <= endDate.Value);

        return await query.SumAsync(m => m.Tutar);
    }
}
