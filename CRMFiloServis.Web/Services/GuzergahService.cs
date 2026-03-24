using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class GuzergahService : IGuzergahService
{
    private readonly ApplicationDbContext _context;

    public GuzergahService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Guzergah>> GetAllAsync()
    {
        return await _context.Guzergahlar
            .Include(g => g.Cari)
            .Where(g => g.Cari == null || !g.Cari.IsDeleted) // Silinmis cari kontrolu
            .OrderBy(g => g.GuzergahAdi)
            .ToListAsync();
    }

    public async Task<List<Guzergah>> GetActiveAsync()
    {
        return await _context.Guzergahlar
            .Include(g => g.Cari)
            .Where(g => g.Aktif)
            .Where(g => g.Cari == null || !g.Cari.IsDeleted) // Silinmis cari kontrolu
            .OrderBy(g => g.GuzergahAdi)
            .ToListAsync();
    }

    public async Task<List<Guzergah>> GetByCariIdAsync(int cariId)
    {
        return await _context.Guzergahlar
            .Where(g => g.CariId == cariId)
            .OrderBy(g => g.GuzergahAdi)
            .ToListAsync();
    }

    public async Task<Guzergah?> GetByIdAsync(int id)
    {
        return await _context.Guzergahlar
            .Include(g => g.Cari)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<Guzergah> CreateAsync(Guzergah guzergah)
    {
        _context.Guzergahlar.Add(guzergah);
        await _context.SaveChangesAsync();
        return guzergah;
    }

    public async Task<Guzergah> UpdateAsync(Guzergah guzergah)
    {
        _context.Guzergahlar.Update(guzergah);
        await _context.SaveChangesAsync();
        return guzergah;
    }

    public async Task DeleteAsync(int id)
    {
        var guzergah = await _context.Guzergahlar.FindAsync(id);
        if (guzergah != null)
        {
            guzergah.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateNextKodAsync()
    {
        var lastGuzergah = await _context.Guzergahlar
            .IgnoreQueryFilters()
            .OrderByDescending(g => g.Id)
            .FirstOrDefaultAsync();

        var nextNumber = (lastGuzergah?.Id ?? 0) + 1;
        return $"GZR-{nextNumber:D4}";
    }
}
