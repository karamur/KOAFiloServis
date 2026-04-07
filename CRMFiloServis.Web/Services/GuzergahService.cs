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
            .AsNoTracking()
            .Include(g => g.Cari)
            .Include(g => g.Firma)
            .Where(g => g.Cari == null || !g.Cari.IsDeleted)
            .OrderBy(g => g.GuzergahAdi)
            .ToListAsync();
    }

    public async Task<List<Guzergah>> GetActiveAsync()
    {
        return await _context.Guzergahlar
            .AsNoTracking()
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
            .AsNoTracking()
            .Where(g => g.CariId == cariId)
            .OrderBy(g => g.GuzergahAdi)
            .ToListAsync();
    }

    public async Task<List<Guzergah>> GetByFirmaIdAsync(int firmaId)
    {
        return await _context.Guzergahlar
            .AsNoTracking()
            .Include(g => g.VarsayilanArac)
            .Include(g => g.VarsayilanSofor)
            .Where(g => g.FirmaId == firmaId && g.Aktif)
            .OrderBy(g => g.GuzergahAdi)
            .ToListAsync();
    }

    public async Task<Guzergah?> GetByIdAsync(int id)
    {
        return await _context.Guzergahlar
            .AsNoTracking()
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
        var existing = await _context.Guzergahlar.FindAsync(guzergah.Id);
        if (existing == null)
            throw new InvalidOperationException($"Güzergah bulunamadı. Id: {guzergah.Id}");

        existing.GuzergahKodu = guzergah.GuzergahKodu;
        existing.GuzergahAdi = guzergah.GuzergahAdi;
        existing.BaslangicNoktasi = guzergah.BaslangicNoktasi;
        existing.BitisNoktasi = guzergah.BitisNoktasi;
        existing.Mesafe = guzergah.Mesafe;
        existing.TahminiSure = guzergah.TahminiSure;
        existing.CariId = guzergah.CariId;
        existing.FirmaId = guzergah.FirmaId;
        existing.VarsayilanAracId = guzergah.VarsayilanAracId;
        existing.VarsayilanSoforId = guzergah.VarsayilanSoforId;
        existing.Notlar = guzergah.Notlar;
        existing.Aktif = guzergah.Aktif;
        existing.IsDeleted = guzergah.IsDeleted;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(int id)
    {
        var guzergah = await _context.Guzergahlar.FindAsync(id);
        if (guzergah != null)
        {
            guzergah.IsDeleted = true;
            guzergah.UpdatedAt = DateTime.UtcNow;
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
        var firma = await _context.Firmalar
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == firmaId);
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
                sayi = await _context.Guzergahlar
                    .AsNoTracking()
                    .CountAsync(g => g.FirmaId == firmaId) + 1;
            }
        }

        return $"{firmaKisaltma}-{sayi:D3}";
    }

    #region Doğrulama Metodları

    public async Task<bool> FaturaKalemdenGuzergahVarMiAsync(int faturaKalemId)
    {
        return await _context.Guzergahlar
            .AsNoTracking()
            .AnyAsync(g => g.FaturaKalemId == faturaKalemId && !g.IsDeleted);
    }

    public async Task<Guzergah?> GetByFaturaKalemIdAsync(int faturaKalemId)
    {
        return await _context.Guzergahlar
            .AsNoTracking()
            .Include(g => g.Firma)
            .FirstOrDefaultAsync(g => g.FaturaKalemId == faturaKalemId && !g.IsDeleted);
    }

    public async Task<bool> BenzersizGuzergahMiAsync(int firmaId, string guzergahAdi, int? haricId = null)
    {
        var normalizedAdi = guzergahAdi.Trim().ToLowerInvariant();

        var query = _context.Guzergahlar
            .AsNoTracking()
            .Where(g => g.FirmaId == firmaId && !g.IsDeleted);

        if (haricId.HasValue)
            query = query.Where(g => g.Id != haricId.Value);

        var mevcutlar = await query
            .Select(g => g.GuzergahAdi.ToLower().Trim())
            .ToListAsync();

        return !mevcutlar.Contains(normalizedAdi);
    }

    #endregion
}
