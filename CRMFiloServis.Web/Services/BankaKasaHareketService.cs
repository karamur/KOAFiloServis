using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class BankaKasaHareketService : IBankaKasaHareketService
{
    private readonly ApplicationDbContext _context;

    public BankaKasaHareketService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BankaKasaHareket>> GetAllAsync()
    {
        return await _context.BankaKasaHareketleri
            .Include(h => h.BankaHesap)
            .Include(h => h.Cari)
            .OrderByDescending(h => h.IslemTarihi)
            .ToListAsync();
    }

    public async Task<List<BankaKasaHareket>> GetByHesapIdAsync(int hesapId)
    {
        return await _context.BankaKasaHareketleri
            .Include(h => h.Cari)
            .Where(h => h.BankaHesapId == hesapId)
            .OrderByDescending(h => h.IslemTarihi)
            .ToListAsync();
    }

    public async Task<List<BankaKasaHareket>> GetByCariIdAsync(int cariId)
    {
        return await _context.BankaKasaHareketleri
            .Include(h => h.BankaHesap)
            .Where(h => h.CariId == cariId)
            .OrderByDescending(h => h.IslemTarihi)
            .ToListAsync();
    }

    public async Task<List<BankaKasaHareket>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.BankaKasaHareketleri
            .Include(h => h.BankaHesap)
            .Include(h => h.Cari)
            .Where(h => h.IslemTarihi >= startDate && h.IslemTarihi <= endDate)
            .OrderByDescending(h => h.IslemTarihi)
            .ToListAsync();
    }

    public async Task<List<BankaKasaHareket>> GetByTipAsync(HareketTipi tip)
    {
        return await _context.BankaKasaHareketleri
            .Include(h => h.BankaHesap)
            .Include(h => h.Cari)
            .Where(h => h.HareketTipi == tip)
            .OrderByDescending(h => h.IslemTarihi)
            .ToListAsync();
    }

    public async Task<List<BankaKasaHareket>> GetEslestirmeyeUygunHareketlerAsync(int cariId, HareketTipi tip)
    {
        // Tamamen eþleþtirilmemiþ hareketleri getir
        var hareketler = await _context.BankaKasaHareketleri
            .Include(h => h.BankaHesap)
            .Include(h => h.OdemeEslestirmeleri)
            .Where(h => h.CariId == cariId && h.HareketTipi == tip)
            .OrderByDescending(h => h.IslemTarihi)
            .ToListAsync();

        // Henüz tam eþleþtirilmemiþ olanlarý filtrele
        return hareketler
            .Where(h => h.Tutar > h.OdemeEslestirmeleri.Sum(e => e.EslestirilenTutar))
            .ToList();
    }

    public async Task<BankaKasaHareket?> GetByIdAsync(int id)
    {
        return await _context.BankaKasaHareketleri
            .Include(h => h.BankaHesap)
            .Include(h => h.Cari)
            .Include(h => h.OdemeEslestirmeleri)
                .ThenInclude(e => e.Fatura)
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<BankaKasaHareket> CreateAsync(BankaKasaHareket hareket)
    {
        _context.BankaKasaHareketleri.Add(hareket);
        await _context.SaveChangesAsync();
        return hareket;
    }

    public async Task<BankaKasaHareket> UpdateAsync(BankaKasaHareket hareket)
    {
        _context.BankaKasaHareketleri.Update(hareket);
        await _context.SaveChangesAsync();
        return hareket;
    }

    public async Task DeleteAsync(int id)
    {
        var hareket = await _context.BankaKasaHareketleri.FindAsync(id);
        if (hareket != null)
        {
            hareket.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateNextIslemNoAsync()
    {
        var today = DateTime.Today;
        var prefix = $"HRK-{today:yyyyMMdd}";

        var lastHareket = await _context.BankaKasaHareketleri
            .IgnoreQueryFilters()
            .Where(h => h.IslemNo.StartsWith(prefix))
            .OrderByDescending(h => h.IslemNo)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (lastHareket != null)
        {
            var parts = lastHareket.IslemNo.Split('-');
            if (parts.Length >= 3 && int.TryParse(parts[2], out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}-{nextNumber:D4}";
    }

    // BankaHesap (Kasa/Banka) iþlemleri
    public async Task<List<BankaHesap>> GetHesaplarAsync()
    {
        var hesaplar = await _context.BankaHesaplari
            .Include(h => h.Hareketler)
            .OrderBy(h => h.HesapTipi)
            .ThenBy(h => h.HesapAdi)
            .ToListAsync();

        return hesaplar;
    }

    public async Task<List<BankaHesap>> GetAktifHesaplarAsync()
    {
        return await _context.BankaHesaplari
            .Include(h => h.Hareketler)
            .Where(h => h.Aktif)
            .OrderBy(h => h.HesapTipi)
            .ThenBy(h => h.HesapAdi)
            .ToListAsync();
    }

    public async Task<BankaHesap?> GetHesapByIdAsync(int id)
    {
        return await _context.BankaHesaplari
            .Include(h => h.Hareketler)
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<BankaHesap> CreateHesapAsync(BankaHesap hesap)
    {
        _context.BankaHesaplari.Add(hesap);
        await _context.SaveChangesAsync();
        return hesap;
    }

    public async Task<BankaHesap> UpdateHesapAsync(BankaHesap hesap)
    {
        _context.BankaHesaplari.Update(hesap);
        await _context.SaveChangesAsync();
        return hesap;
    }

    public async Task DeleteHesapAsync(int id)
    {
        var hesap = await _context.BankaHesaplari.FindAsync(id);
        if (hesap != null)
        {
            hesap.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }
}
