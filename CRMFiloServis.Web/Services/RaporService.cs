using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using CRMFiloServis.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class RaporService : IRaporService
{
    private readonly ApplicationDbContext _context;

    public RaporService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ServisCalismaRaporItem>> GetServisCalismaRaporuAsync(
        DateTime startDate,
        DateTime endDate,
        int? aracId = null,
        int? soforId = null,
        int? guzergahId = null,
        int? cariId = null)
    {
        var query = _context.ServisCalismalari
            .Include(s => s.Arac)
            .Include(s => s.Sofor)
            .Include(s => s.Guzergah)
                .ThenInclude(g => g.Cari)
            .Where(s => s.CalismaTarihi >= startDate && s.CalismaTarihi <= endDate)
            .Where(s => s.Durum == CalismaDurum.Tamamlandi);

        if (aracId.HasValue)
            query = query.Where(s => s.AracId == aracId.Value);

        if (soforId.HasValue)
            query = query.Where(s => s.SoforId == soforId.Value);

        if (guzergahId.HasValue)
            query = query.Where(s => s.GuzergahId == guzergahId.Value);

        if (cariId.HasValue)
            query = query.Where(s => s.Guzergah.CariId == cariId.Value);

        var data = await query.ToListAsync();

        // Gruplama ve özet hesaplama
        var grouped = data
            .GroupBy(s => new { s.AracId, s.SoforId, s.GuzergahId })
            .Select(g => new ServisCalismaRaporItem
            {
                Tarih = g.Min(x => x.CalismaTarihi),
                Plaka = g.First().Arac.Plaka,
                SoforAdi = g.First().Sofor.TamAd,
                GuzergahAdi = g.First().Guzergah.GuzergahAdi,
                FirmaAdi = g.First().Guzergah.Cari.Unvan,
                ServisTuru = string.Join(", ", g.Select(x => x.ServisTuru.ToString()).Distinct()),
                BirimFiyat = g.First().Fiyat ?? g.First().Guzergah.BirimFiyat,
                CalisilanGun = g.Count(),
                ToplamTutar = g.Sum(x => x.Fiyat ?? x.Guzergah.BirimFiyat)
            })
            .OrderBy(x => x.FirmaAdi)
            .ThenBy(x => x.GuzergahAdi)
            .ThenBy(x => x.Plaka)
            .ToList();

        return grouped;
    }

    public async Task<List<FaturaOdemeRaporItem>> GetFaturaOdemeRaporuAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? cariId = null,
        bool? sadeceBekleyenler = null)
    {
        var query = _context.Faturalar
            .Include(f => f.Cari)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(f => f.FaturaTarihi >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(f => f.FaturaTarihi <= endDate.Value);

        if (cariId.HasValue)
            query = query.Where(f => f.CariId == cariId.Value);

        if (sadeceBekleyenler == true)
            query = query.Where(f => f.Durum == FaturaDurum.Beklemede || f.Durum == FaturaDurum.KismiOdendi);

        var data = await query.OrderBy(f => f.VadeTarihi).ToListAsync();

        return data.Select(f => new FaturaOdemeRaporItem
        {
            FaturaId = f.Id,
            FaturaNo = f.FaturaNo,
            FaturaTarihi = f.FaturaTarihi,
            VadeTarihi = f.VadeTarihi,
            CariUnvan = f.Cari.Unvan,
            FaturaTipi = f.FaturaTipi.ToString(),
            Durum = f.Durum.ToString(),
            GenelToplam = f.GenelToplam,
            OdenenTutar = f.OdenenTutar,
            KalanTutar = f.KalanTutar,
            VadeGunu = f.VadeTarihi.HasValue 
                ? (f.VadeTarihi.Value - DateTime.Today).Days 
                : 0
        }).ToList();
    }

    public async Task<List<AracMasrafRaporItem>> GetAracMasrafRaporuAsync(
        DateTime startDate,
        DateTime endDate,
        int? aracId = null)
    {
        var query = _context.AracMasraflari
            .Include(m => m.Arac)
            .Include(m => m.MasrafKalemi)
            .Include(m => m.Guzergah)
            .Where(m => m.MasrafTarihi >= startDate && m.MasrafTarihi <= endDate);

        if (aracId.HasValue)
            query = query.Where(m => m.AracId == aracId.Value);

        var data = await query.OrderByDescending(m => m.MasrafTarihi).ToListAsync();

        return data.Select(m => new AracMasrafRaporItem
        {
            MasrafTarihi = m.MasrafTarihi,
            Plaka = m.Arac.Plaka,
            MasrafKalemi = m.MasrafKalemi.MasrafAdi,
            Kategori = m.MasrafKalemi.Kategori.ToString(),
            GuzergahAdi = m.Guzergah?.GuzergahAdi,
            Tutar = m.Tutar,
            BelgeNo = m.BelgeNo,
            Aciklama = m.Aciklama,
            ArizaKaynakli = m.ArizaKaynaklimi
        }).ToList();
    }

    public async Task<CariEkstre> GetCariEkstreAsync(
        int cariId,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var cari = await _context.Cariler.FindAsync(cariId);
        if (cari == null)
            throw new ArgumentException("Cari bulunamadý", nameof(cariId));

        var ekstre = new CariEkstre
        {
            CariId = cari.Id,
            CariKodu = cari.CariKodu,
            CariUnvan = cari.Unvan,
            BaslangicTarihi = startDate,
            BitisTarihi = endDate
        };

        var hareketler = new List<CariEkstreItem>();

        // Faturalarý getir
        var faturalar = await _context.Faturalar
            .Where(f => f.CariId == cariId)
            .Where(f => !startDate.HasValue || f.FaturaTarihi >= startDate.Value)
            .Where(f => !endDate.HasValue || f.FaturaTarihi <= endDate.Value)
            .OrderBy(f => f.FaturaTarihi)
            .ToListAsync();

        foreach (var fatura in faturalar)
        {
            var isBorc = fatura.FaturaTipi == FaturaTipi.SatisFaturasi || fatura.FaturaTipi == FaturaTipi.AlisIadeFaturasi;
            hareketler.Add(new CariEkstreItem
            {
                Tarih = fatura.FaturaTarihi,
                BelgeNo = fatura.FaturaNo,
                IslemTipi = fatura.FaturaTipi.ToString(),
                Aciklama = fatura.Aciklama ?? "Fatura",
                Borc = isBorc ? fatura.GenelToplam : 0,
                Alacak = isBorc ? 0 : fatura.GenelToplam
            });
        }

        // Banka/Kasa hareketlerini getir
        var bankaHareketler = await _context.BankaKasaHareketleri
            .Where(h => h.CariId == cariId)
            .Where(h => !startDate.HasValue || h.IslemTarihi >= startDate.Value)
            .Where(h => !endDate.HasValue || h.IslemTarihi <= endDate.Value)
            .OrderBy(h => h.IslemTarihi)
            .ToListAsync();

        foreach (var hareket in bankaHareketler)
        {
            hareketler.Add(new CariEkstreItem
            {
                Tarih = hareket.IslemTarihi,
                BelgeNo = hareket.IslemNo,
                IslemTipi = hareket.HareketTipi.ToString(),
                Aciklama = hareket.Aciklama ?? "Ödeme/Tahsilat",
                Borc = hareket.HareketTipi == HareketTipi.Cikis ? hareket.Tutar : 0,
                Alacak = hareket.HareketTipi == HareketTipi.Giris ? hareket.Tutar : 0
            });
        }

        // Tarihe göre sýrala ve bakiye hesapla
        hareketler = hareketler.OrderBy(h => h.Tarih).ThenBy(h => h.BelgeNo).ToList();

        decimal bakiye = 0;
        foreach (var hareket in hareketler)
        {
            bakiye += hareket.Borc - hareket.Alacak;
            hareket.Bakiye = bakiye;
        }

        ekstre.Hareketler = hareketler;
        ekstre.ToplamBorc = hareketler.Sum(h => h.Borc);
        ekstre.ToplamAlacak = hareketler.Sum(h => h.Alacak);
        ekstre.Bakiye = bakiye;

        return ekstre;
    }
}
