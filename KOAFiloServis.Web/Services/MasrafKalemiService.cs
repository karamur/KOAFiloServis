using KOAFiloServis.Shared.Entities;
using KOAFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace KOAFiloServis.Web.Services;

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
            .AsNoTracking()
            .OrderBy(m => m.Kategori)
            .ThenBy(m => m.MasrafAdi)
            .ToListAsync();
    }

    public async Task<List<MasrafKalemi>> GetActiveAsync()
    {
        return await _context.MasrafKalemleri
            .AsNoTracking()
            .Where(m => m.Aktif)
            .OrderBy(m => m.Kategori)
            .ThenBy(m => m.MasrafAdi)
            .ToListAsync();
    }

    public async Task<List<MasrafKalemi>> GetByKategoriAsync(MasrafKategori kategori)
    {
        return await _context.MasrafKalemleri
            .AsNoTracking()
            .Where(m => m.Kategori == kategori && m.Aktif)
            .OrderBy(m => m.MasrafAdi)
            .ToListAsync();
    }

    public async Task<MasrafKalemi?> GetByIdAsync(int id)
    {
        return await _context.MasrafKalemleri
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<MasrafKalemi> CreateAsync(MasrafKalemi masrafKalemi)
    {
        _context.MasrafKalemleri.Add(masrafKalemi);
        await _context.SaveChangesAsync();
        return masrafKalemi;
    }

    public async Task<MasrafKalemi> UpdateAsync(MasrafKalemi masrafKalemi)
    {
        var existing = await _context.MasrafKalemleri.FindAsync(masrafKalemi.Id);
        if (existing == null)
            throw new InvalidOperationException($"Masraf kalemi bulunamadı. Id: {masrafKalemi.Id}");

        existing.MasrafKodu = masrafKalemi.MasrafKodu;
        existing.MasrafAdi = masrafKalemi.MasrafAdi;
        existing.Kategori = masrafKalemi.Kategori;
        existing.Notlar = masrafKalemi.Notlar;
        existing.Aktif = masrafKalemi.Aktif;
        existing.IsDeleted = masrafKalemi.IsDeleted;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(int id)
    {
        var masrafKalemi = await _context.MasrafKalemleri.FindAsync(id);
        if (masrafKalemi != null)
        {
            masrafKalemi.IsDeleted = true;
            masrafKalemi.UpdatedAt = DateTime.UtcNow;
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
