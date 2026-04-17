using KOAFiloServis.Shared.Entities;
using KOAFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace KOAFiloServis.Web.Services;

public class MasrafKalemiService : IMasrafKalemiService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public MasrafKalemiService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<MasrafKalemi>> GetAllAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.MasrafKalemleri
            .AsNoTracking()
            .OrderBy(m => m.Kategori)
            .ThenBy(m => m.MasrafAdi)
            .ToListAsync();
    }

    public async Task<List<MasrafKalemi>> GetActiveAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.MasrafKalemleri
            .AsNoTracking()
            .Where(m => m.Aktif)
            .OrderBy(m => m.Kategori)
            .ThenBy(m => m.MasrafAdi)
            .ToListAsync();
    }

    public async Task<List<MasrafKalemi>> GetByKategoriAsync(MasrafKategori kategori)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.MasrafKalemleri
            .AsNoTracking()
            .Where(m => m.Kategori == kategori && m.Aktif)
            .OrderBy(m => m.MasrafAdi)
            .ToListAsync();
    }

    public async Task<MasrafKalemi?> GetByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.MasrafKalemleri
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<MasrafKalemi> CreateAsync(MasrafKalemi masrafKalemi)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.MasrafKalemleri.Add(masrafKalemi);
        await context.SaveChangesAsync();
        return masrafKalemi;
    }

    public async Task<MasrafKalemi> UpdateAsync(MasrafKalemi masrafKalemi)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.MasrafKalemleri.FindAsync(masrafKalemi.Id);
        if (existing == null)
            throw new InvalidOperationException($"Masraf kalemi bulunamadı. Id: {masrafKalemi.Id}");

        existing.MasrafKodu = masrafKalemi.MasrafKodu;
        existing.MasrafAdi = masrafKalemi.MasrafAdi;
        existing.Kategori = masrafKalemi.Kategori;
        existing.Notlar = masrafKalemi.Notlar;
        existing.Aktif = masrafKalemi.Aktif;
        existing.IsDeleted = masrafKalemi.IsDeleted;
        existing.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var masrafKalemi = await context.MasrafKalemleri.FindAsync(id);
        if (masrafKalemi != null)
        {
            masrafKalemi.IsDeleted = true;
            masrafKalemi.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateNextKodAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var last = await context.MasrafKalemleri
            .IgnoreQueryFilters()
            .OrderByDescending(m => m.Id)
            .FirstOrDefaultAsync();

        var nextNumber = (last?.Id ?? 0) + 1;
        return $"MSR-{nextNumber:D4}";
    }
}
