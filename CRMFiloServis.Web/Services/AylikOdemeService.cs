using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace CRMFiloServis.Web.Services;

public class AylikOdemeService : IAylikOdemeService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IFirmaService _firmaService;

    public AylikOdemeService(IDbContextFactory<ApplicationDbContext> contextFactory, IFirmaService firmaService)
    {
        _contextFactory = contextFactory;
        _firmaService = firmaService;
    }

    public async Task<List<AylikOdemePlani>> GetTumPlanlariAsync(int firmaId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.AylikOdemePlanlari
            .Include(p => p.Cari)
            .Include(p => p.BankaHesap)
            .Include(p => p.MasrafKalemi)
            .Where(p => p.FirmaId == firmaId && !p.IsDeleted)
            .OrderBy(p => p.OdemeGunu)
            .ToListAsync();
    }

    public async Task<List<AylikOdemePlani>> GetAktifPlanlariAsync(int firmaId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var bugun = DateTime.Today;

        return await context.AylikOdemePlanlari
            .Include(p => p.Cari)
            .Include(p => p.BankaHesap)
            .Include(p => p.MasrafKalemi)
            .Where(p => p.FirmaId == firmaId && 
                       p.Aktif && 
                       !p.IsDeleted &&
                       p.BaslangicTarihi <= bugun &&
                       (!p.BitisTarihi.HasValue || p.BitisTarihi.Value >= bugun))
            .OrderBy(p => p.OdemeGunu)
            .ToListAsync();
    }

    public async Task<AylikOdemePlani?> GetPlanByIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.AylikOdemePlanlari
            .Include(p => p.Cari)
            .Include(p => p.BankaHesap)
            .Include(p => p.MasrafKalemi)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<AylikOdemePlani> CreatePlanAsync(AylikOdemePlani plan)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        plan.CreatedAt = DateTime.UtcNow;
        context.AylikOdemePlanlari.Add(plan);
        await context.SaveChangesAsync();
        return plan;
    }

    public async Task<AylikOdemePlani> UpdatePlanAsync(AylikOdemePlani plan)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        plan.UpdatedAt = DateTime.UtcNow;
        context.AylikOdemePlanlari.Update(plan);
        await context.SaveChangesAsync();
        return plan;
    }

    public async Task DeletePlanAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var plan = await context.AylikOdemePlanlari.FindAsync(id);
        if (plan != null)
        {
            plan.IsDeleted = true;
            plan.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<AylikOdemeGerceklesen>> GetAylikOdemeleriAsync(int firmaId, int yil, int ay)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.AylikOdemeGerceklesenler
            .Include(o => o.Plan)
            .Include(o => o.BankaKasaHareket)
            .Where(o => o.FirmaId == firmaId && o.Yil == yil && o.Ay == ay && !o.IsDeleted)
            .OrderBy(o => o.Plan!.OdemeGunu)
            .ToListAsync();
    }

    public async Task<List<AylikOdemeGerceklesen>> GetBekleyenOdemeleriAsync(int firmaId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.AylikOdemeGerceklesenler
            .Include(o => o.Plan)
            .Where(o => o.FirmaId == firmaId && 
                       o.Durum == OdemeDurumu.Bekleniyor && 
                       !o.IsDeleted)
            .OrderBy(o => o.Yil).ThenBy(o => o.Ay).ThenBy(o => o.Plan!.OdemeGunu)
            .ToListAsync();
    }

    public async Task<List<AylikOdemeGerceklesen>> GetGecikmiOdemeleriAsync(int firmaId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var bugun = DateTime.Today;

        return await context.AylikOdemeGerceklesenler
            .Include(o => o.Plan)
            .Where(o => o.FirmaId == firmaId && 
                       o.Durum == OdemeDurumu.Bekleniyor &&
                       !o.IsDeleted)
            .AsEnumerable()
            .Where(o => new DateTime(o.Yil, o.Ay, o.Plan?.OdemeGunu ?? 1) < bugun)
            .OrderBy(o => o.Yil).ThenBy(o => o.Ay)
            .ToList();
    }

    public async Task<AylikOdemeGerceklesen?> GetGerceklesenByIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.AylikOdemeGerceklesenler
            .Include(o => o.Plan)
            .Include(o => o.BankaKasaHareket)
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);
    }

    public async Task<AylikOdemeGerceklesen> OdemeKaydetAsync(int planId, int yil, int ay, decimal tutar, DateTime? odemeTarihi)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var plan = await context.AylikOdemePlanlari.FindAsync(planId);
        if (plan == null) throw new Exception("Plan bulunamadý");

        var gerceklesen = await context.AylikOdemeGerceklesenler
            .FirstOrDefaultAsync(o => o.AylikOdemePlaniId == planId && o.Yil == yil && o.Ay == ay);

        if (gerceklesen == null)
        {
            gerceklesen = new AylikOdemeGerceklesen
            {
                AylikOdemePlaniId = planId,
                FirmaId = plan.FirmaId,
                Yil = yil,
                Ay = ay,
                PlanlananTutar = plan.AylikTutar,
                OdenenTutar = 0,
                Durum = OdemeDurumu.Bekleniyor,
                CreatedAt = DateTime.UtcNow
            };
            context.AylikOdemeGerceklesenler.Add(gerceklesen);
        }

        gerceklesen.OdenenTutar += tutar;
        gerceklesen.OdemeTarihi = odemeTarihi ?? DateTime.Now;
        gerceklesen.Durum = gerceklesen.OdenenTutar >= gerceklesen.PlanlananTutar 
            ? OdemeDurumu.Odendi 
            : OdemeDurumu.KismiOdendi;
        gerceklesen.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return gerceklesen;
    }

    public async Task<AylikOdemeGerceklesen> OdemeDurumGuncelleAsync(int id, OdemeDurumu durum)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var gerceklesen = await context.AylikOdemeGerceklesenler.FindAsync(id);

        if (gerceklesen == null) throw new Exception("Ödeme bulunamadý");

        gerceklesen.Durum = durum;
        gerceklesen.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return gerceklesen;
    }

    public async Task<List<AylikOdemeGerceklesen>> GetTakvimOdemeleriAsync(int firmaId, int yil, int ay)
    {
        // Önce otomatik kayýtlarý oluþtur
        await OtomatikOdemeKayitlariOlusturAsync(firmaId, yil, ay);

        // Sonra listeyi getir
        return await GetAylikOdemeleriAsync(firmaId, yil, ay);
    }

    public async Task OtomatikOdemeKayitlariOlusturAsync(int firmaId, int yil, int ay)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var aktifPlanlar = await GetAktifPlanlariAsync(firmaId);
        var tarih = new DateTime(yil, ay, 1);

        foreach (var plan in aktifPlanlar.Where(p => p.OtomatikKayitOlustur))
        {
            // Bu ay için kayýt var mý kontrol et
            var mevcutKayit = await context.AylikOdemeGerceklesenler
                .AnyAsync(o => o.AylikOdemePlaniId == plan.Id && o.Yil == yil && o.Ay == ay);

            if (!mevcutKayit)
            {
                var gerceklesen = new AylikOdemeGerceklesen
                {
                    AylikOdemePlaniId = plan.Id,
                    FirmaId = firmaId,
                    Yil = yil,
                    Ay = ay,
                    PlanlananTutar = plan.AylikTutar,
                    OdenenTutar = 0,
                    Durum = OdemeDurumu.Bekleniyor,
                    CreatedAt = DateTime.UtcNow
                };
                context.AylikOdemeGerceklesenler.Add(gerceklesen);
            }
        }

        await context.SaveChangesAsync();
    }

    public async Task<decimal> GetAylikToplamTutarAsync(int firmaId, int yil, int ay)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.AylikOdemeGerceklesenler
            .Where(o => o.FirmaId == firmaId && o.Yil == yil && o.Ay == ay && !o.IsDeleted)
            .SumAsync(o => o.PlanlananTutar);
    }

    public async Task<Dictionary<int, decimal>> GetYillikOdemeDagilimiAsync(int firmaId, int yil)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var odemeler = await context.AylikOdemeGerceklesenler
            .Where(o => o.FirmaId == firmaId && o.Yil == yil && !o.IsDeleted)
            .GroupBy(o => o.Ay)
            .Select(g => new { Ay = g.Key, Toplam = g.Sum(o => o.PlanlananTutar) })
            .ToListAsync();

        var sonuc = new Dictionary<int, decimal>();
        for (int ay = 1; ay <= 12; ay++)
        {
            sonuc[ay] = odemeler.FirstOrDefault(o => o.Ay == ay)?.Toplam ?? 0;
        }

        return sonuc;
    }

    public async Task<decimal> GetToplamAylikOdemeAsync(int firmaId)
    {
        var planlar = await GetAktifPlanlariAsync(firmaId);
        return planlar.Sum(p => p.AylikTutar);
    }

    public async Task<int> GetBekleyenOdemeSayisiAsync(int firmaId)
    {
        var bekleyenler = await GetBekleyenOdemeleriAsync(firmaId);
        return bekleyenler.Count;
    }

    public async Task<decimal> GetBuAyOdenecekTutarAsync(int firmaId)
    {
        var bugun = DateTime.Today;
        return await GetAylikToplamTutarAsync(firmaId, bugun.Year, bugun.Month);
    }
}
