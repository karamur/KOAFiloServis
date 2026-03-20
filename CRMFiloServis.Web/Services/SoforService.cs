using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class SoforService : ISoforService
{
    private readonly ApplicationDbContext _context;

    public SoforService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Sofor>> GetAllAsync()
    {
        return await _context.Soforler
            .OrderBy(s => s.Ad)
            .ThenBy(s => s.Soyad)
            .ToListAsync();
    }

    public async Task<List<Sofor>> GetActiveAsync()
    {
        return await _context.Soforler
            .Where(s => s.Aktif)
            .OrderBy(s => s.Ad)
            .ThenBy(s => s.Soyad)
            .ToListAsync();
    }

    public async Task<int> GetActiveCountAsync()
    {
        return await _context.Soforler
            .Where(s => s.Aktif)
            .CountAsync();
    }

    public async Task<Sofor?> GetByIdAsync(int id)
    {
        return await _context.Soforler.FindAsync(id);
    }

    public async Task<Sofor> CreateAsync(Sofor sofor)
    {
        _context.Soforler.Add(sofor);
        await _context.SaveChangesAsync();
        return sofor;
    }

    public async Task<Sofor> UpdateAsync(Sofor sofor)
    {
        _context.Soforler.Update(sofor);
        await _context.SaveChangesAsync();
        return sofor;
    }

    public async Task DeleteAsync(int id)
    {
        var sofor = await _context.Soforler.FindAsync(id);
        if (sofor != null)
        {
            sofor.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateNextKodAsync()
    {
        var lastSofor = await _context.Soforler
            .IgnoreQueryFilters()
            .OrderByDescending(s => s.Id)
            .FirstOrDefaultAsync();

        var nextNumber = (lastSofor?.Id ?? 0) + 1;
        return $"SFR-{nextNumber:D4}";
    }

    // Görev bazlý filtreleme metodlarý
    public async Task<List<Sofor>> GetByGorevAsync(PersonelGorev gorev)
    {
        return await _context.Soforler
            .Where(s => s.Gorev == gorev)
            .OrderBy(s => s.Ad)
            .ThenBy(s => s.Soyad)
            .ToListAsync();
    }

    public async Task<List<Sofor>> GetActiveSoforlerAsync()
    {
        return await _context.Soforler
            .Where(s => s.Aktif && s.Gorev == PersonelGorev.Sofor)
            .OrderBy(s => s.Ad)
            .ThenBy(s => s.Soyad)
            .ToListAsync();
    }

    public async Task<List<Sofor>> GetActiveByGorevAsync(PersonelGorev gorev)
    {
        return await _context.Soforler
            .Where(s => s.Aktif && s.Gorev == gorev)
            .OrderBy(s => s.Ad)
            .ThenBy(s => s.Soyad)
            .ToListAsync();
    }
}
