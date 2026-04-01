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
            .Include(g => g.Firma)
            .Where(g => g.Cari == null || !g.Cari.IsDeleted)
            .OrderBy(g => g.GuzergahAdi)
            .ToListAsync();
    }

    public async Task<List<Guzergah>> GetActiveAsync()
    {
        return await _context.Guzergahlar
            .Include(g => g.Cari)
            .Include(g => g.Firma)
            .Where(g => g.Aktif)
            .Where(g => g.Cari == null || !g.Cari.IsDeleted)
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

    public async Task<List<Guzergah>> GetByFirmaIdAsync(int firmaId)
    {
        return await _context.Guzergahlar
            .Include(g => g.VarsayilanArac)
            .Include(g => g.VarsayilanSofor)
            .Where(g => g.FirmaId == firmaId && g.Aktif)
            .OrderBy(g => g.GuzergahAdi)
            .ToListAsync();
    }

    public async Task<Guzergah?> GetByIdAsync(int id)
    {
        return await _context.Guzergahlar
            .Include(g => g.Cari)
            .Include(g => g.Firma)
            .Include(g => g.VarsayilanArac)
            .Include(g => g.VarsayilanSofor)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<Guzergah> CreateAsync(Guzergah guzergah)
    {
        _context.Guzergahlar.Add(guzergah);
        await _context.SaveChangesAsync();
        return guzergah;
    }

    public async Task<Guzergah> AddAsync(Guzergah guzergah)
    {
        return await CreateAsync(guzergah);
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

    public async Task<string> GenerateGuzergahKoduAsync(int firmaId)
    {
        var firma = await _context.Firmalar.FindAsync(firmaId);
        var firmaKisaltma = firma?.FirmaAdi?.Length >= 3 
            ? firma.FirmaAdi.Substring(0, 3).ToUpperInvariant() 
            : "GZR";

        var sonGuzergah = await _context.Guzergahlar
            .IgnoreQueryFilters()
            .Where(g => g.FirmaId == firmaId)
            .OrderByDescending(g => g.Id)
            .FirstOrDefaultAsync();

        var sayi = 1;
        if (sonGuzergah != null)
        {
            // Mevcut koddan sayıyı çıkar
            var parts = sonGuzergah.GuzergahKodu.Split('-');
            if (parts.Length >= 2 && int.TryParse(parts[^1], out var mevcutSayi))
            {
                sayi = mevcutSayi + 1;
            }
            else
            {
                sayi = await _context.Guzergahlar.CountAsync(g => g.FirmaId == firmaId) + 1;
            }
        }

        return $"{firmaKisaltma}-{sayi:D3}";
    }
}
