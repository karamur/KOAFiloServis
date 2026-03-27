using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class DashboardGrafikService : IDashboardGrafikService
{
    private readonly ApplicationDbContext _context;

    public DashboardGrafikService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AylikGrafikData> GetAylikGelirGiderAsync(int yil)
    {
        var data = new AylikGrafikData
        {
            Veri1Label = "Gelir",
            Veri2Label = "Gider"
        };

        var aylar = new[] { "Oca", "Þub", "Mar", "Nis", "May", "Haz", "Tem", "Aðu", "Eyl", "Eki", "Kas", "Ara" };
        data.Aylar = aylar.ToList();

        // Aylýk gelirler (servis çalýþmalarýndan)
        var calismalar = await _context.ServisCalismalari
            .Where(c => c.CalismaTarihi.Year == yil)
            .GroupBy(c => c.CalismaTarihi.Month)
            .Select(g => new { Ay = g.Key, Toplam = g.Sum(c => c.Fiyat ?? 0) })
            .ToListAsync();

        // Aylýk giderler (araç masraflarýndan)
        var masraflar = await _context.AracMasraflari
            .Where(m => m.MasrafTarihi.Year == yil)
            .GroupBy(m => m.MasrafTarihi.Month)
            .Select(g => new { Ay = g.Key, Toplam = g.Sum(m => m.Tutar) })
            .ToListAsync();

        for (int ay = 1; ay <= 12; ay++)
        {
            var gelir = calismalar.FirstOrDefault(c => c.Ay == ay)?.Toplam ?? 0;
            var gider = masraflar.FirstOrDefault(m => m.Ay == ay)?.Toplam ?? 0;
            data.Veri1.Add(gelir);
            data.Veri2.Add(gider);
        }

        return data;
    }

    public async Task<AylikGrafikData> GetAylikSeferSayisiAsync(int yil)
    {
        var data = new AylikGrafikData
        {
            Veri1Label = "Sefer Sayýsý"
        };

        var aylar = new[] { "Oca", "Þub", "Mar", "Nis", "May", "Haz", "Tem", "Aðu", "Eyl", "Eki", "Kas", "Ara" };
        data.Aylar = aylar.ToList();

        var calismalar = await _context.ServisCalismalari
            .Where(c => c.CalismaTarihi.Year == yil)
            .GroupBy(c => c.CalismaTarihi.Month)
            .Select(g => new { Ay = g.Key, Sayi = g.Count() })
            .ToListAsync();

        for (int ay = 1; ay <= 12; ay++)
        {
            var sayi = calismalar.FirstOrDefault(c => c.Ay == ay)?.Sayi ?? 0;
            data.Veri1.Add(sayi);
        }

        return data;
    }

    public async Task<List<AracPerformansData>> GetAracPerformansAsync(int yil, int ay)
    {
        var calismalar = await _context.ServisCalismalari
            .Include(c => c.Arac)
            .Where(c => c.CalismaTarihi.Year == yil && c.CalismaTarihi.Month == ay)
            .GroupBy(c => new { c.AracId, Plaka = c.Arac!.AktifPlaka ?? "" })
            .Select(g => new
            {
                Plaka = g.Key.Plaka,
                AracId = g.Key.AracId,
                SeferSayisi = g.Count(),
                ToplamCiro = g.Sum(c => c.Fiyat ?? 0)
            })
            .ToListAsync();

        var masraflar = await _context.AracMasraflari
            .Where(m => m.MasrafTarihi.Year == yil && m.MasrafTarihi.Month == ay)
            .GroupBy(m => m.AracId)
            .Select(g => new { AracId = g.Key, ToplamMasraf = g.Sum(m => m.Tutar) })
            .ToListAsync();

        return calismalar.Select(c => new AracPerformansData
        {
            Plaka = c.Plaka,
            SeferSayisi = c.SeferSayisi,
            ToplamCiro = c.ToplamCiro,
            ToplamMasraf = masraflar.FirstOrDefault(m => m.AracId == c.AracId)?.ToplamMasraf ?? 0
        })
        .OrderByDescending(a => a.ToplamCiro)
        .Take(10)
        .ToList();
    }

    public async Task<List<CariPerformansData>> GetCariPerformansAsync(int yil, int ay)
    {
        var faturalar = await _context.Faturalar
            .Include(f => f.Cari)
            .Where(f => f.FaturaTarihi.Year == yil && f.FaturaTarihi.Month == ay && f.FaturaTipi == FaturaTipi.SatisFaturasi)
            .GroupBy(f => new { f.CariId, f.Cari!.Unvan })
            .Select(g => new CariPerformansData
            {
                CariUnvan = g.Key.Unvan,
                SeferSayisi = g.Count(),
                ToplamCiro = g.Sum(f => f.GenelToplam),
                OdenenTutar = g.Sum(f => f.OdenenTutar),
                KalanBakiye = g.Sum(f => f.GenelToplam - f.OdenenTutar)
            })
            .OrderByDescending(c => c.ToplamCiro)
            .Take(10)
            .ToListAsync();

        return faturalar;
    }
}
