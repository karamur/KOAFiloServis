using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class CariService : ICariService
{
    private readonly ApplicationDbContext _context;

    public CariService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Cari>> GetAllAsync()
    {
        return await _context.Cariler
            .OrderBy(c => c.Unvan)
            .ToListAsync();
    }

    public async Task<Cari?> GetByIdAsync(int id)
    {
        return await _context.Cariler
            .Include(c => c.Guzergahlar)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Cari?> GetByKodAsync(string cariKodu)
    {
        return await _context.Cariler
            .FirstOrDefaultAsync(c => c.CariKodu == cariKodu);
    }

    public async Task<List<Cari>> GetByTipAsync(CariTipi tip)
    {
        return await _context.Cariler
            .Where(c => c.CariTipi == tip || c.CariTipi == CariTipi.MusteriTedarikci)
            .OrderBy(c => c.Unvan)
            .ToListAsync();
    }

    public async Task<Cari> CreateAsync(Cari cari)
    {
        _context.Cariler.Add(cari);
        await _context.SaveChangesAsync();
        return cari;
    }

    public async Task<Cari> UpdateAsync(Cari cari)
    {
        _context.Cariler.Update(cari);
        await _context.SaveChangesAsync();
        return cari;
    }

    public async Task DeleteAsync(int id)
    {
        var cari = await _context.Cariler.FindAsync(id);
        if (cari != null)
        {
            cari.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateNextKodAsync()
    {
        var lastCari = await _context.Cariler
            .IgnoreQueryFilters()
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync();

        var nextNumber = (lastCari?.Id ?? 0) + 1;
        return $"CRI-{nextNumber:D5}";
    }
}
