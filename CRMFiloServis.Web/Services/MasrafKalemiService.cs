using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class MasrafKalemiService : IMasrafKalemiService
{
    private readonly ApplicationDbContext _context;

    public MasrafKalemiService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MasrafKalemi>> GetAllAsync()
    {
        return await _context.MasrafKalemleri
            .OrderBy(m => m.Kategori)
            .ThenBy(m => m.MasrafAdi)
            .ToListAsync();
    }

    public async Task<List<MasrafKalemi>> GetActiveAsync()
    {
        return await _context.MasrafKalemleri
            .Where(m => m.Aktif)
            .OrderBy(m => m.Kategori)
            .ThenBy(m => m.MasrafAdi)
            .ToListAsync();
    }

    public async Task<List<MasrafKalemi>> GetByKategoriAsync(MasrafKategori kategori)
    {
        return await _context.MasrafKalemleri
            .Where(m => m.Kategori == kategori && m.Aktif)
            .OrderBy(m => m.MasrafAdi)
            .ToListAsync();
    }

    public async Task<MasrafKalemi?> GetByIdAsync(int id)
    {
        return await _context.MasrafKalemleri.FindAsync(id);
    }

    public async Task<MasrafKalemi> CreateAsync(MasrafKalemi masrafKalemi)
    {
        _context.MasrafKalemleri.Add(masrafKalemi);
        await _context.SaveChangesAsync();
        return masrafKalemi;
    }

    public async Task<MasrafKalemi> UpdateAsync(MasrafKalemi masrafKalemi)
    {
        _context.MasrafKalemleri.Update(masrafKalemi);
        await _context.SaveChangesAsync();
        return masrafKalemi;
    }

    public async Task DeleteAsync(int id)
    {
        var masrafKalemi = await _context.MasrafKalemleri.FindAsync(id);
        if (masrafKalemi != null)
        {
            masrafKalemi.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateNextKodAsync()
    {
        var last = await _context.MasrafKalemleri
            .IgnoreQueryFilters()
            .OrderByDescending(m => m.Id)
            .FirstOrDefaultAsync();

        var nextNumber = (last?.Id ?? 0) + 1;
        return $"MSR-{nextNumber:D4}";
    }
}
