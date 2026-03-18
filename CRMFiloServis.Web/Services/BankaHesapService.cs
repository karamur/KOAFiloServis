using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class BankaHesapService : IBankaHesapService
{
    private readonly ApplicationDbContext _context;

    public BankaHesapService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BankaHesap>> GetAllAsync()
    {
        return await _context.BankaHesaplari
            .OrderBy(b => b.HesapAdi)
            .ToListAsync();
    }

    public async Task<List<BankaHesap>> GetActiveAsync()
    {
        return await _context.BankaHesaplari
            .Where(b => b.Aktif)
            .OrderBy(b => b.HesapAdi)
            .ToListAsync();
    }

    public async Task<List<BankaHesap>> GetByTipAsync(HesapTipi tip)
    {
        return await _context.BankaHesaplari
            .Where(b => b.HesapTipi == tip && b.Aktif)
            .OrderBy(b => b.HesapAdi)
            .ToListAsync();
    }

    public async Task<BankaHesap?> GetByIdAsync(int id)
    {
        return await _context.BankaHesaplari.FindAsync(id);
    }

    public async Task<BankaHesap> CreateAsync(BankaHesap bankaHesap)
    {
        _context.BankaHesaplari.Add(bankaHesap);
        await _context.SaveChangesAsync();
        return bankaHesap;
    }

    public async Task<BankaHesap> UpdateAsync(BankaHesap bankaHesap)
    {
        _context.BankaHesaplari.Update(bankaHesap);
        await _context.SaveChangesAsync();
        return bankaHesap;
    }

    public async Task DeleteAsync(int id)
    {
        var bankaHesap = await _context.BankaHesaplari.FindAsync(id);
        if (bankaHesap != null)
        {
            bankaHesap.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateNextKodAsync()
    {
        var last = await _context.BankaHesaplari
            .IgnoreQueryFilters()
            .OrderByDescending(b => b.Id)
            .FirstOrDefaultAsync();

        var nextNumber = (last?.Id ?? 0) + 1;
        return $"HSP-{nextNumber:D4}";
    }

    public async Task<decimal> GetBakiyeAsync(int hesapId)
    {
        var hesap = await _context.BankaHesaplari.FindAsync(hesapId);
        if (hesap == null) return 0;

        var girisler = await _context.BankaKasaHareketleri
            .Where(h => h.BankaHesapId == hesapId && h.HareketTipi == HareketTipi.Giris)
            .SumAsync(h => h.Tutar);

        var cikislar = await _context.BankaKasaHareketleri
            .Where(h => h.BankaHesapId == hesapId && h.HareketTipi == HareketTipi.Cikis)
            .SumAsync(h => h.Tutar);

        return hesap.AcilisBakiye + girisler - cikislar;
    }
}
