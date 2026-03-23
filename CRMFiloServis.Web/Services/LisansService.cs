using System.Security.Cryptography;
using System.Text;
using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class LisansService : ILisansService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private static Lisans? _cachedLisans;

    public LisansService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Lisans?> GetAktifLisansAsync()
    {
        if (_cachedLisans != null && _cachedLisans.Gecerli)
            return _cachedLisans;

        using var context = await _contextFactory.CreateDbContextAsync();
        var lisans = await context.Lisanslar
            .OrderByDescending(l => l.CreatedAt)
            .FirstOrDefaultAsync();

        if (lisans == null)
        {
            lisans = await OlusturTrialLisansAsync();
        }

        _cachedLisans = lisans;
        return lisans;
    }

    public async Task<bool> LisansGecerliMiAsync()
    {
        var lisans = await GetAktifLisansAsync();
        return lisans?.Gecerli ?? false;
    }

    public async Task<Lisans> AktiveLisansAsync(string lisansAnahtari)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // Mevcut lisansi pasif yap
        var mevcutLisans = await context.Lisanslar.FirstOrDefaultAsync(l => !l.IsDeleted);
        if (mevcutLisans != null)
        {
            mevcutLisans.IsDeleted = true;
        }

        // Yeni lisans olustur
        var lisansBilgi = ParseLisansAnahtari(lisansAnahtari);
        var yeniLisans = new Lisans
        {
            LisansAnahtari = lisansAnahtari,
            Tur = lisansBilgi.Tur,
            BaslangicTarihi = DateTime.Now,
            BitisTarihi = DateTime.Now.AddDays(lisansBilgi.Gun),
            MaxKullaniciSayisi = lisansBilgi.KullaniciSayisi,
            MakineKodu = await GetMakineKoduAsync(),
            ExcelExportIzni = true,
            PdfExportIzni = true,
            RaporlamaIzni = true,
            YedeklemeIzni = lisansBilgi.Tur >= LisansTuru.Basic,
            MuhasebeIzni = lisansBilgi.Tur >= LisansTuru.Professional,
            SatisModuluIzni = lisansBilgi.Tur >= LisansTuru.Professional,
            CreatedAt = DateTime.UtcNow
        };

        context.Lisanslar.Add(yeniLisans);
        await context.SaveChangesAsync();

        _cachedLisans = yeniLisans;
        return yeniLisans;
    }

    public async Task<int> KalanGunAsync()
    {
        var lisans = await GetAktifLisansAsync();
        return lisans?.KalanGun ?? 0;
    }

    public Task<string> GetMakineKoduAsync()
    {
        try
        {
            // Basit makine kodu olusturma - cross-platform uyumlu
            var machineName = Environment.MachineName;
            var userName = Environment.UserName;
            var osVersion = Environment.OSVersion.ToString();
            
            var combined = $"{machineName}-{userName}-{osVersion}";
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(combined));
            return Task.FromResult(Convert.ToBase64String(hash).Substring(0, 16));
        }
        catch
        {
            return Task.FromResult("LOCAL-DEV-MODE");
        }
    }

    public async Task<Lisans> OlusturTrialLisansAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var trialLisans = new Lisans
        {
            LisansAnahtari = GenerateTrialKey(),
            Tur = LisansTuru.Trial,
            BaslangicTarihi = DateTime.Now,
            BitisTarihi = DateTime.Now.AddDays(30), // 30 gun trial
            MaxKullaniciSayisi = 5, // 5 kullanici
            MakineKodu = await GetMakineKoduAsync(),
            ExcelExportIzni = true,
            PdfExportIzni = true,
            RaporlamaIzni = true,
            YedeklemeIzni = true,
            MuhasebeIzni = true,
            SatisModuluIzni = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Lisanslar.Add(trialLisans);
        await context.SaveChangesAsync();

        _cachedLisans = trialLisans;
        return trialLisans;
    }

    public async Task<bool> KullanicLimitiKontrolAsync()
    {
        var lisans = await GetAktifLisansAsync();
        if (lisans == null) return false;

        using var context = await _contextFactory.CreateDbContextAsync();
        var kullaniciSayisi = await context.Kullanicilar.CountAsync(k => k.Aktif);

        return kullaniciSayisi < lisans.MaxKullaniciSayisi;
    }

    public async Task<bool> ModulIzniVarMiAsync(string modulAdi)
    {
        var lisans = await GetAktifLisansAsync();
        if (lisans == null || !lisans.Gecerli) return false;

        return modulAdi.ToLower() switch
        {
            "excel" => lisans.ExcelExportIzni,
            "pdf" => lisans.PdfExportIzni,
            "rapor" => lisans.RaporlamaIzni,
            "yedekleme" => lisans.YedeklemeIzni,
            "muhasebe" => lisans.MuhasebeIzni,
            "satis" => lisans.SatisModuluIzni,
            _ => true
        };
    }

    private string GenerateTrialKey()
    {
        var guid = Guid.NewGuid().ToString("N").ToUpper();
        return $"TRIAL-{guid.Substring(0, 4)}-{guid.Substring(4, 4)}-{guid.Substring(8, 4)}";
    }

    private (LisansTuru Tur, int Gun, int KullaniciSayisi) ParseLisansAnahtari(string anahtar)
    {
        // Basit bir lisans parse islemi
        if (anahtar.StartsWith("TRIAL"))
            return (LisansTuru.Trial, 30, 5);
        if (anahtar.StartsWith("BASIC"))
            return (LisansTuru.Basic, 365, 5);
        if (anahtar.StartsWith("PRO"))
            return (LisansTuru.Professional, 365, 10);
        if (anahtar.StartsWith("ENT"))
            return (LisansTuru.Enterprise, 365, 999);

        return (LisansTuru.Trial, 30, 5);
    }
}
