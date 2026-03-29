using System.Collections.Concurrent;
using System.Text.Json;
using CRMFiloServis.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CRMFiloServis.Web.Data;

public sealed class AktiviteLogInterceptor : SaveChangesInterceptor
{
    private static readonly ConcurrentDictionary<Guid, List<PendingAktiviteLog>> PendingLogs = new();
    private static readonly AsyncLocal<bool> IsWritingLog = new();

    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AktiviteLogInterceptor> _logger;

    public AktiviteLogInterceptor(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AktiviteLogInterceptor> logger)
    {
        _contextFactory = contextFactory;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        CaptureEntries(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null)
        {
            _ = PersistLogsAsync(eventData.Context.ContextId.InstanceId, cancellationToken);
        }

        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null)
        {
            PendingLogs.TryRemove(eventData.Context.ContextId.InstanceId, out _);
        }

        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    private void CaptureEntries(DbContext? context)
    {
        if (context == null || IsWritingLog.Value)
        {
            return;
        }

        var entries = context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity is not AktiviteLog && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        if (!entries.Any())
        {
            return;
        }

        PendingLogs[context.ContextId.InstanceId] = entries
            .Select(CreatePendingLog)
            .Where(x => x != null)
            .Cast<PendingAktiviteLog>()
            .ToList();
    }

    private PendingAktiviteLog? CreatePendingLog(EntityEntry<BaseEntity> entry)
    {
        var entityType = entry.Entity.GetType();
        var entityAdi = ResolveEntityAdi(entry);
        var islemTipi = entry.State switch
        {
            EntityState.Added => "Ekleme",
            EntityState.Modified => "Güncelleme",
            EntityState.Deleted => "Silme",
            _ => null
        };

        if (islemTipi == null)
        {
            return null;
        }

        return new PendingAktiviteLog
        {
            Entity = entry.Entity,
            EntityTipi = entityType.Name,
            Modul = ResolveModul(entityType.Name),
            IslemTipi = islemTipi,
            EntityAdi = entityAdi,
            EskiDeger = entry.State is EntityState.Modified or EntityState.Deleted ? SerializeValues(entry.Properties, original: true) : null,
            YeniDeger = entry.State is EntityState.Added or EntityState.Modified ? SerializeValues(entry.Properties, original: false) : null,
            Aciklama = $"{ResolveModul(entityType.Name)} modülünde {entityType.Name} için {islemTipi.ToLowerInvariant()} işlemi yapıldı."
        };
    }

    private static string? ResolveEntityAdi(EntityEntry<BaseEntity> entry)
    {
        var preferredProperties = new[] { "Ad", "Adi", "AdSoyad", "Baslik", "Baslik", "Unvan", "FirmaAdi", "Plaka", "FaturaNo", "IslemNo", "Marka", "Model", "RolAdi" };

        foreach (var propertyName in preferredProperties)
        {
            var property = entry.Properties.FirstOrDefault(p => p.Metadata.Name == propertyName);
            var value = property?.CurrentValue?.ToString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return entry.Entity.GetType().Name;
    }

    private static string SerializeValues(IEnumerable<PropertyEntry> properties, bool original)
    {
        var values = properties
            .Where(p => p.Metadata.Name is not nameof(BaseEntity.CreatedAt) and not nameof(BaseEntity.UpdatedAt))
            .ToDictionary(
                p => p.Metadata.Name,
                p => original ? p.OriginalValue : p.CurrentValue);

        return JsonSerializer.Serialize(values, new JsonSerializerOptions { WriteIndented = true });
    }

    private async Task PersistLogsAsync(Guid contextId, CancellationToken cancellationToken)
    {
        if (IsWritingLog.Value)
        {
            return;
        }

        if (!PendingLogs.TryRemove(contextId, out var logs) || logs.Count == 0)
        {
            return;
        }

        try
        {
            IsWritingLog.Value = true;
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var httpContext = _httpContextAccessor.HttpContext;
            var now = DateTime.Now;

            foreach (var item in logs)
            {
                context.AktiviteLoglar.Add(new AktiviteLog
                {
                    IslemZamani = now,
                    IslemTipi = item.IslemTipi,
                    Modul = item.Modul,
                    EntityTipi = item.EntityTipi,
                    EntityId = item.Entity.Id > 0 ? item.Entity.Id : null,
                    EntityAdi = item.EntityAdi,
                    Aciklama = item.Aciklama,
                    EskiDeger = item.EskiDeger,
                    YeniDeger = item.YeniDeger,
                    KullaniciAdi = httpContext?.User?.Identity?.Name ?? "Sistem",
                    IpAdresi = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                    Tarayici = httpContext?.Request?.Headers["User-Agent"].ToString(),
                    Seviye = item.IslemTipi == "Silme" ? AktiviteSeviye.Uyari : AktiviteSeviye.Bilgi
                });
            }

            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Aktivite log interceptor kayıt hatası");
        }
        finally
        {
            IsWritingLog.Value = false;
        }
    }

    private static string ResolveModul(string entityType) => entityType switch
    {
        "Cari" or "KullaniciCari" => "Cari",
        "Arac" or "AracPlaka" or "AracMasraf" or "AracEvrak" or "AracEvrakDosya" => "Araç",
        "Sofor" or "PersonelMaas" or "PersonelIzin" or "PersonelIzinHakki" => "Personel",
        "Fatura" or "FaturaKalem" or "OdemeEslestirme" => "Fatura",
        "BankaHesap" or "BankaKasaHareket" => "Finans",
        "ServisCalisma" or "Guzergah" or "MasrafKalemi" => "Servis",
        "Hatirlatici" or "Bildirim" or "Mesaj" or "WhatsAppAyar" or "EmailAyar" => "CRM",
        "Kullanici" or "Rol" or "RolYetki" or "Lisans" or "Firma" or "AktiviteLog" => "Sistem",
        "AracPiyasaArastirma" or "PiyasaArastirmaIlan" or "PiyasaKaynak" => "Piyasa Araştırma",
        "AracIlan" or "AracSatis" or "SatisPersoneli" => "Satış",
        "BudgetOdeme" or "BudgetMasrafKalemi" or "TekrarlayanOdeme" => "Bütçe",
        _ => entityType
    };

    private sealed class PendingAktiviteLog
    {
        public BaseEntity Entity { get; set; } = null!;
        public string IslemTipi { get; set; } = string.Empty;
        public string Modul { get; set; } = string.Empty;
        public string EntityTipi { get; set; } = string.Empty;
        public string? EntityAdi { get; set; }
        public string? Aciklama { get; set; }
        public string? EskiDeger { get; set; }
        public string? YeniDeger { get; set; }
    }
}
