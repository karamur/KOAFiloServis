using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class TekrarlayanOdemeService : ITekrarlayanOdemeService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ILogger<TekrarlayanOdemeService> _logger;

    public TekrarlayanOdemeService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        ILogger<TekrarlayanOdemeService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<List<TekrarlayanOdeme>> GetAllAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TekrarlayanOdemeler
            .Include(t => t.Firma)
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.OdemeAdi)
            .ToListAsync();
    }

    public async Task<TekrarlayanOdeme?> GetByIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TekrarlayanOdemeler
            .Include(t => t.Firma)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
    }

    public async Task<TekrarlayanOdeme> CreateAsync(TekrarlayanOdeme odeme)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        odeme.CreatedAt = DateTime.UtcNow;
        context.TekrarlayanOdemeler.Add(odeme);
        await context.SaveChangesAsync();
        _logger.LogInformation("Tekrarlayan ödeme oluþturuldu: {OdemeAdi}", odeme.OdemeAdi);
        return odeme;
    }

    public async Task<TekrarlayanOdeme> UpdateAsync(TekrarlayanOdeme odeme)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.TekrarlayanOdemeler.FindAsync(odeme.Id);
        if (existing == null)
            throw new Exception("Kayýt bulunamadý");

        existing.OdemeAdi = odeme.OdemeAdi;
        existing.MasrafKalemi = odeme.MasrafKalemi;
        existing.Aciklama = odeme.Aciklama;
        existing.Tutar = odeme.Tutar;
        existing.Periyod = odeme.Periyod;
        existing.OdemeGunu = odeme.OdemeGunu;
        existing.BaslangicTarihi = odeme.BaslangicTarihi;
        existing.BitisTarihi = odeme.BitisTarihi;
        existing.HatirlatmaGunSayisi = odeme.HatirlatmaGunSayisi;
        existing.Aktif = odeme.Aktif;
        existing.Renk = odeme.Renk;
        existing.Icon = odeme.Icon;
        existing.Notlar = odeme.Notlar;
        existing.UpdatedAt = DateTime.UtcNow;

        context.TekrarlayanOdemeler.Update(existing);
        await context.SaveChangesAsync();
        _logger.LogInformation("Tekrarlayan ödeme güncellendi: {OdemeAdi}", odeme.OdemeAdi);
        return existing;
    }

    public async Task DeleteAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var odeme = await context.TekrarlayanOdemeler.FindAsync(id);
        if (odeme == null)
            throw new Exception("Kayýt bulunamadý");

        odeme.IsDeleted = true;
        odeme.UpdatedAt = DateTime.UtcNow;
        context.TekrarlayanOdemeler.Update(odeme);
        await context.SaveChangesAsync();
        _logger.LogInformation("Tekrarlayan ödeme silindi: {OdemeAdi}", odeme.OdemeAdi);
    }

    public async Task<List<TekrarlayanOdeme>> GetAktifOdemelerAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TekrarlayanOdemeler
            .Where(t => !t.IsDeleted && t.Aktif)
            .Where(t => t.BitisTarihi == null || t.BitisTarihi >= DateTime.Today)
            .OrderBy(t => t.OdemeGunu)
            .ToListAsync();
    }

    public async Task<List<TekrarlayanOdeme>> GetYaklasanOdemelerAsync(int gunSayisi = 7)
    {
        var bugun = DateTime.Today;
        var limitTarih = bugun.AddDays(gunSayisi);
        var buAyOdemeGunleri = Enumerable.Range(bugun.Day, gunSayisi).Select(g => g > 31 ? g - 31 : g).ToList();

        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TekrarlayanOdemeler
            .Where(t => !t.IsDeleted && t.Aktif)
            .Where(t => buAyOdemeGunleri.Contains(t.OdemeGunu))
            .OrderBy(t => t.OdemeGunu)
            .ToListAsync();
    }
}
