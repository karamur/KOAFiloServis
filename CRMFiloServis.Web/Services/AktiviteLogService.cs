using System.Text.Json;
using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class AktiviteLogService : IAktiviteLogService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AktiviteLogService> _logger;

    public AktiviteLogService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AktiviteLogService> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task LogAsync(string islemTipi, string modul, string? aciklama = null,
        string? entityTipi = null, int? entityId = null, string? entityAdi = null,
        string? eskiDeger = null, string? yeniDeger = null,
        AktiviteSeviye seviye = AktiviteSeviye.Bilgi)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;

            var log = new AktiviteLog
            {
                IslemZamani = DateTime.Now,
                IslemTipi = islemTipi,
                Modul = modul,
                EntityTipi = entityTipi,
                EntityId = entityId,
                EntityAdi = entityAdi,
                Aciklama = aciklama,
                EskiDeger = eskiDeger,
                YeniDeger = yeniDeger,
                Seviye = seviye,
                KullaniciAdi = httpContext?.User?.Identity?.Name ?? "Sistem",
                IpAdresi = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                Tarayici = httpContext?.Request?.Headers["User-Agent"].ToString()
            };

            _context.AktiviteLoglar.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Aktivite log kaydý hatasý");
        }
    }

    public async Task LogEklemeAsync(string modul, string entityTipi, int entityId, string entityAdi)
    {
        await LogAsync("Ekleme", modul, 
            aciklama: $"{entityTipi} eklendi: {entityAdi}",
            entityTipi: entityTipi, entityId: entityId, entityAdi: entityAdi);
    }

    public async Task LogGuncellemeAsync(string modul, string entityTipi, int entityId, string entityAdi, 
        object? eskiDeger = null, object? yeniDeger = null)
    {
        string? eskiJson = eskiDeger != null ? JsonSerializer.Serialize(eskiDeger) : null;
        string? yeniJson = yeniDeger != null ? JsonSerializer.Serialize(yeniDeger) : null;

        await LogAsync("Güncelleme", modul,
            aciklama: $"{entityTipi} güncellendi: {entityAdi}",
            entityTipi: entityTipi, entityId: entityId, entityAdi: entityAdi,
            eskiDeger: eskiJson, yeniDeger: yeniJson);
    }

    public async Task LogSilmeAsync(string modul, string entityTipi, int entityId, string entityAdi)
    {
        await LogAsync("Silme", modul,
            aciklama: $"{entityTipi} silindi: {entityAdi}",
            entityTipi: entityTipi, entityId: entityId, entityAdi: entityAdi,
            seviye: AktiviteSeviye.Uyari);
    }

    public async Task LogHataAsync(string modul, string aciklama, Exception? ex = null)
    {
        var detay = ex != null ? $"{aciklama} - Hata: {ex.Message}" : aciklama;
        await LogAsync("Hata", modul, aciklama: detay, seviye: AktiviteSeviye.Hata);
    }

    public async Task<List<AktiviteLogItem>> GetLogsAsync(AktiviteLogFilter? filter = null)
    {
        var query = _context.AktiviteLoglar.AsQueryable();

        if (filter != null)
        {
            if (filter.BaslangicTarihi.HasValue)
                query = query.Where(l => l.IslemZamani >= filter.BaslangicTarihi.Value);

            if (filter.BitisTarihi.HasValue)
                query = query.Where(l => l.IslemZamani <= filter.BitisTarihi.Value.AddDays(1));

            if (!string.IsNullOrEmpty(filter.Modul))
                query = query.Where(l => l.Modul == filter.Modul);

            if (!string.IsNullOrEmpty(filter.IslemTipi))
                query = query.Where(l => l.IslemTipi == filter.IslemTipi);

            if (filter.Seviye.HasValue)
                query = query.Where(l => l.Seviye == filter.Seviye.Value);

            if (!string.IsNullOrEmpty(filter.AramaMetni))
                query = query.Where(l => 
                    (l.Aciklama != null && l.Aciklama.Contains(filter.AramaMetni)) ||
                    (l.EntityAdi != null && l.EntityAdi.Contains(filter.AramaMetni)));
        }

        var skip = ((filter?.Sayfa ?? 1) - 1) * (filter?.SayfaBoyutu ?? 50);

        return await query
            .OrderByDescending(l => l.IslemZamani)
            .Skip(skip)
            .Take(filter?.SayfaBoyutu ?? 50)
            .Select(l => new AktiviteLogItem
            {
                Id = l.Id,
                IslemZamani = l.IslemZamani,
                IslemTipi = l.IslemTipi,
                Modul = l.Modul,
                EntityTipi = l.EntityTipi,
                EntityId = l.EntityId,
                EntityAdi = l.EntityAdi,
                Aciklama = l.Aciklama,
                Seviye = l.Seviye,
                KullaniciAdi = l.KullaniciAdi
            })
            .ToListAsync();
    }

    public async Task<AktiviteLogOzet> GetOzetAsync(int gunSayisi = 7)
    {
        var baslangic = DateTime.Today.AddDays(-gunSayisi);
        var bugun = DateTime.Today;

        var logs = await _context.AktiviteLoglar
            .Where(l => l.IslemZamani >= baslangic)
            .ToListAsync();

        var ozet = new AktiviteLogOzet
        {
            ToplamLog = logs.Count,
            BugunLog = logs.Count(l => l.IslemZamani.Date == bugun),
            EklemeAdet = logs.Count(l => l.IslemTipi == "Ekleme"),
            GuncellemeAdet = logs.Count(l => l.IslemTipi == "Güncelleme"),
            SilmeAdet = logs.Count(l => l.IslemTipi == "Silme"),
            HataAdet = logs.Count(l => l.IslemTipi == "Hata" || l.Seviye == AktiviteSeviye.Hata)
        };

        // Modül aktiviteleri
        ozet.ModulAktiviteleri = logs
            .GroupBy(l => l.Modul)
            .Select(g => new ModulAktivite { Modul = g.Key, Adet = g.Count() })
            .OrderByDescending(m => m.Adet)
            .Take(10)
            .ToList();

        // Günlük aktiviteler
        for (int i = gunSayisi; i >= 0; i--)
        {
            var tarih = DateTime.Today.AddDays(-i);
            ozet.GunlukAktiviteler.Add(new GunlukAktivite
            {
                Tarih = tarih,
                Adet = logs.Count(l => l.IslemZamani.Date == tarih)
            });
        }

        return ozet;
    }

    public async Task<int> GetLogCountAsync(DateTime? baslangic = null, DateTime? bitis = null)
    {
        var query = _context.AktiviteLoglar.AsQueryable();

        if (baslangic.HasValue)
            query = query.Where(l => l.IslemZamani >= baslangic.Value);

        if (bitis.HasValue)
            query = query.Where(l => l.IslemZamani <= bitis.Value);

        return await query.CountAsync();
    }

    public async Task CleanupOldLogsAsync(int gunSakla = 90)
    {
        var silinecekTarih = DateTime.Today.AddDays(-gunSakla);

        var eskiLoglar = await _context.AktiviteLoglar
            .Where(l => l.IslemZamani < silinecekTarih)
            .ToListAsync();

        if (eskiLoglar.Any())
        {
            _context.AktiviteLoglar.RemoveRange(eskiLoglar);
            await _context.SaveChangesAsync();

            _logger.LogInformation("{Count} eski log kaydý silindi", eskiLoglar.Count);
        }
    }
}
