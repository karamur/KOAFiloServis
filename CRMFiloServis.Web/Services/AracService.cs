using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class AracService : IAracService
{
    private readonly ApplicationDbContext _context;

    public AracService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Arac>> GetAllAsync()
    {
        return await _context.Araclar
            .OrderBy(a => a.Plaka)
            .ToListAsync();
    }

    public async Task<List<Arac>> GetActiveAsync()
    {
        return await _context.Araclar
            .Where(a => a.Aktif)
            .OrderBy(a => a.Plaka)
            .ToListAsync();
    }

    public async Task<Arac?> GetByIdAsync(int id)
    {
        return await _context.Araclar.FindAsync(id);
    }

    public async Task<Arac?> GetByPlakaAsync(string plaka)
    {
        return await _context.Araclar
            .FirstOrDefaultAsync(a => a.Plaka == plaka);
    }

    public async Task<Arac> CreateAsync(Arac arac)
    {
        _context.Araclar.Add(arac);
        await _context.SaveChangesAsync();
        return arac;
    }

    public async Task<Arac> UpdateAsync(Arac arac)
    {
        _context.Araclar.Update(arac);
        await _context.SaveChangesAsync();
        return arac;
    }

    public async Task DeleteAsync(int id)
    {
        var arac = await _context.Araclar.FindAsync(id);
        if (arac != null)
        {
            arac.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }
}
