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
            BaslangicTarihi = DateTime.UtcNow,
            BitisTarihi = DateTime.UtcNow.AddDays(lisansBilgi.Gun),
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
            // Windows için System.Management ile donaným bilgilerini al
            if (OperatingSystem.IsWindows())
            {
                return Task.FromResult(CRMFiloServis.Shared.LisansHelper.GetMachineCode());
            }
            
            // Diðer platformlar için basit kod
            var machineName = Environment.MachineName;
            var userName = Environment.UserName;
            var osVersion = Environment.OSVersion.ToString();
            
            var combined = $"{machineName}-{userName}-{osVersion}";
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(combined));
            return Task.FromResult(Convert.ToBase64String(hash).Substring(0, 32).Replace("/", "").Replace("+", ""));
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
            BaslangicTarihi = DateTime.UtcNow,
            BitisTarihi = DateTime.UtcNow.AddDays(30), // 30 gun trial
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
        try
        {
            // Þifrelenmiþ lisans anahtarýný çöz
            var lisansJson = DecryptString(anahtar);
            
            // JSON'dan LisansBilgi deserialize et
            var lisansBilgi = System.Text.Json.JsonSerializer.Deserialize<DesktopLisansBilgi>(lisansJson);
            
            if (lisansBilgi == null)
                throw new Exception("Geçersiz lisans formatý");
            
            // Makine kodu kontrolü
            var currentMakineKodu = GetMakineKoduAsync().Result;
            if (lisansBilgi.MakineKodu != currentMakineKodu)
            {
                throw new Exception($"Bu lisans baþka bir bilgisayar için oluþturulmuþ!\n\nLisans Makine Kodu: {lisansBilgi.MakineKodu}\nBu PC Makine Kodu: {currentMakineKodu}");
            }
            
            // Tarih kontrolü
            if (DateTime.UtcNow > lisansBilgi.BitisTarihi)
            {
                throw new Exception($"Lisans süresi dolmuþ! Bitiþ Tarihi: {lisansBilgi.BitisTarihi:dd.MM.yyyy}");
            }
            
            // Lisans tipine göre enum deðeri
            var tur = lisansBilgi.LisansTipi.ToLower() switch
            {
                "trial" => LisansTuru.Trial,
                "standard" => LisansTuru.Basic,
                "professional" => LisansTuru.Professional,
                "enterprise" => LisansTuru.Enterprise,
                _ => LisansTuru.Trial
            };
            
            // Kalan gün hesapla
            var kalanGun = (int)(lisansBilgi.BitisTarihi - DateTime.UtcNow).TotalDays;
            
            return (tur, kalanGun, lisansBilgi.MaxKullaniciSayisi);
        }
        catch (Exception ex)
        {
            // Hata durumunda Exception fýrlat
            throw new Exception($"Lisans aktive edilemedi: {ex.Message}");
        }
    }

    private const string LisansAnahtar = "CRMFiloServis2026SecretKey!@#";

    private string DecryptString(string cipherText)
    {
        var fullCipher = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        var key = SHA256.HashData(Encoding.UTF8.GetBytes(LisansAnahtar));
        aes.Key = key;

        var iv = new byte[aes.IV.Length];
        var cipher = new byte[fullCipher.Length - iv.Length];

        Array.Copy(fullCipher, iv, iv.Length);
        Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var msDecrypt = new MemoryStream(cipher);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);

        return srDecrypt.ReadToEnd();
    }

    // Desktop lisans bilgisi için model
    private class DesktopLisansBilgi
    {
        public string LisansKodu { get; set; } = "";
        public string FirmaAdi { get; set; } = "";
        public string YetkiliKisi { get; set; } = "";
        public string Email { get; set; } = "";
        public string Telefon { get; set; } = "";
        public string LisansTipi { get; set; } = "";
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }
        public int MaxKullaniciSayisi { get; set; }
        public int MaxAracSayisi { get; set; }
        public string MakineKodu { get; set; } = "";
        public bool Aktif { get; set; }
    }
}
