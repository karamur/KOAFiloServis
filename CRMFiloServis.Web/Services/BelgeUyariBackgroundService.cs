using Microsoft.EntityFrameworkCore;
using CRMFiloServis.Web.Data;
using CRMFiloServis.Shared.Entities;

namespace CRMFiloServis.Web.Services;

/// <summary>
/// Belge uyarıları için otomatik email gönderim servisi
/// Her gün süresi yaklaşan belgeleri kontrol eder ve email gönderir
/// </summary>
public class BelgeUyariBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BelgeUyariBackgroundService> _logger;
    private readonly IConfiguration _configuration;
    private readonly TimeSpan _checkInterval;
    private readonly bool _enabled;

    public BelgeUyariBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<BelgeUyariBackgroundService> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _configuration = configuration;
        
        _enabled = configuration.GetValue("BelgeUyari:EmailEnabled", false);
        _checkInterval = TimeSpan.FromHours(configuration.GetValue("BelgeUyari:CheckIntervalHours", 24));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_enabled)
        {
            _logger.LogInformation("Belge uyarı email servisi devre dışı");
            return;
        }

        _logger.LogInformation("Belge uyarı email servisi başlatıldı");

        // İlk çalışmayı bir dakika sonra yap (uygulama başlangıcını bekle)
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await KontrolVeGonderAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Belge uyarı kontrolü hatası");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    public Task RunOnceAsync(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        return KontrolVeGonderAsync();
    }

    private async Task KontrolVeGonderAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var emailService = scope.ServiceProvider.GetService<IEmailService>();
        var bildirimService = scope.ServiceProvider.GetService<Interfaces.IBildirimService>();

        // Önce bildirim servisini kullanarak kullanıcı bazlı e-posta gönder
        if (bildirimService != null)
        {
            try
            {
                // Uygulama içi bildirimleri oluştur
                await bildirimService.TaraVeBildirimOlusturAsync();

                // Kullanıcı bazlı e-posta gönder (ayarlara göre)
                var gonderimSayisi = await bildirimService.EpostaBildirimGonderAsync();
                _logger.LogInformation("Kullanıcı bazlı {Sayi} e-posta bildirimi gönderildi", gonderimSayisi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı bazlı bildirim gönderimi hatası");
            }
        }

        if (emailService == null)
        {
            _logger.LogWarning("Email servisi bulunamadı, belge uyarı emaili gönderilemedi");
            return;
        }

        var bugun = DateTime.Today;
        var uyariGunleri = _configuration.GetSection("BelgeUyari:UyariGunleri").Get<int[]>() ?? [30, 15, 7, 3, 1];

        // Admin kullanıcılarını al
        var adminler = await context.Kullanicilar
            .Where(k => k.Aktif && k.Email != null && k.Email != "" &&
                       (k.Rol.RolAdi == "Admin" || k.Rol.RolAdi == "Yonetici"))
            .ToListAsync();

        if (!adminler.Any())
        {
            _logger.LogWarning("Email gönderilecek admin kullanıcı bulunamadı");
            return;
        }

        // Araç belgelerini kontrol et
        var aracBelgeleri = await GetAracBelgeUyarilariAsync(context, bugun, uyariGunleri);
        
        // Personel belgelerini kontrol et  
        var personelBelgeleri = await GetPersonelBelgeUyarilariAsync(context, bugun, uyariGunleri);
        
        // Firma belgelerini kontrol et
        var firmaBelgeleri = await GetFirmaBelgeUyarilariAsync(context, bugun, uyariGunleri);

        var toplamUyari = aracBelgeleri.Count + personelBelgeleri.Count + firmaBelgeleri.Count;

        if (toplamUyari == 0)
        {
            _logger.LogInformation("Uyarılacak belge bulunamadı");
            return;
        }

        _logger.LogInformation("Toplam {Count} belge uyarısı tespit edildi", toplamUyari);

        // Her admin için email gönder
        foreach (var admin in adminler)
        {
            try
            {
                await GonderBelgeUyariEmailAsync(emailService, admin, aracBelgeleri, personelBelgeleri, firmaBelgeleri);
                _logger.LogInformation("Belge uyarı emaili gönderildi: {Email}", admin.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email gönderim hatası: {Email}", admin.Email);
            }
        }
    }

    private async Task<List<BelgeUyariItem>> GetAracBelgeUyarilariAsync(ApplicationDbContext context, DateTime bugun, int[] uyariGunleri)
    {
        var sonrakiAy = bugun.AddDays(uyariGunleri.Max());
        var uyarilar = new List<BelgeUyariItem>();

        // Muayene
        var muayeneler = await context.Araclar
            .Where(a => a.MuayeneBitisTarihi.HasValue && 
                       a.MuayeneBitisTarihi.Value <= sonrakiAy &&
                       a.MuayeneBitisTarihi.Value >= bugun)
            .Select(a => new { Plaka = a.AktifPlaka ?? a.SaseNo, Tarih = a.MuayeneBitisTarihi!.Value })
            .ToListAsync();

        foreach (var m in muayeneler)
        {
            var kalanGun = (m.Tarih - bugun).Days;
            if (uyariGunleri.Contains(kalanGun) || kalanGun <= 0)
            {
                uyarilar.Add(new BelgeUyariItem
                {
                    EntityTipi = "Araç",
                    EntityAdi = m.Plaka,
                    BelgeTipi = "Muayene",
                    BitisTarihi = m.Tarih,
                    KalanGun = kalanGun
                });
            }
        }

        // Kasko
        var kaskolar = await context.Araclar
            .Where(a => a.KaskoBitisTarihi.HasValue && 
                       a.KaskoBitisTarihi.Value <= sonrakiAy &&
                       a.KaskoBitisTarihi.Value >= bugun)
            .Select(a => new { Plaka = a.AktifPlaka ?? a.SaseNo, Tarih = a.KaskoBitisTarihi!.Value })
            .ToListAsync();

        foreach (var k in kaskolar)
        {
            var kalanGun = (k.Tarih - bugun).Days;
            if (uyariGunleri.Contains(kalanGun) || kalanGun <= 0)
            {
                uyarilar.Add(new BelgeUyariItem
                {
                    EntityTipi = "Araç",
                    EntityAdi = k.Plaka,
                    BelgeTipi = "Kasko",
                    BitisTarihi = k.Tarih,
                    KalanGun = kalanGun
                });
            }
        }

        // Trafik Sigortası
        var sigortalar = await context.Araclar
            .Where(a => a.TrafikSigortaBitisTarihi.HasValue && 
                       a.TrafikSigortaBitisTarihi.Value <= sonrakiAy &&
                       a.TrafikSigortaBitisTarihi.Value >= bugun)
            .Select(a => new { Plaka = a.AktifPlaka ?? a.SaseNo, Tarih = a.TrafikSigortaBitisTarihi!.Value })
            .ToListAsync();

        foreach (var s in sigortalar)
        {
            var kalanGun = (s.Tarih - bugun).Days;
            if (uyariGunleri.Contains(kalanGun) || kalanGun <= 0)
            {
                uyarilar.Add(new BelgeUyariItem
                {
                    EntityTipi = "Araç",
                    EntityAdi = s.Plaka,
                    BelgeTipi = "Trafik Sigortası",
                    BitisTarihi = s.Tarih,
                    KalanGun = kalanGun
                });
            }
        }

        return uyarilar;
    }

    private async Task<List<BelgeUyariItem>> GetPersonelBelgeUyarilariAsync(ApplicationDbContext context, DateTime bugun, int[] uyariGunleri)
    {
        var sonrakiAy = bugun.AddDays(uyariGunleri.Max());
        var uyarilar = new List<BelgeUyariItem>();

        // Ehliyet
        var ehliyetler = await context.Soforler
            .Where(p => p.Aktif && p.EhliyetGecerlilikTarihi.HasValue && 
                       p.EhliyetGecerlilikTarihi.Value <= sonrakiAy &&
                       p.EhliyetGecerlilikTarihi.Value >= bugun)
            .Select(p => new { AdSoyad = p.TamAd, Tarih = p.EhliyetGecerlilikTarihi!.Value })
            .ToListAsync();

        foreach (var e in ehliyetler)
        {
            var kalanGun = (e.Tarih - bugun).Days;
            if (uyariGunleri.Contains(kalanGun) || kalanGun <= 0)
            {
                uyarilar.Add(new BelgeUyariItem
                {
                    EntityTipi = "Personel",
                    EntityAdi = e.AdSoyad,
                    BelgeTipi = "Ehliyet",
                    BitisTarihi = e.Tarih,
                    KalanGun = kalanGun
                });
            }
        }

        // SRC
        var srcler = await context.Soforler
            .Where(p => p.Aktif && p.SrcBelgesiGecerlilikTarihi.HasValue && 
                       p.SrcBelgesiGecerlilikTarihi.Value <= sonrakiAy &&
                       p.SrcBelgesiGecerlilikTarihi.Value >= bugun)
            .Select(p => new { AdSoyad = p.TamAd, Tarih = p.SrcBelgesiGecerlilikTarihi!.Value })
            .ToListAsync();

        foreach (var s in srcler)
        {
            var kalanGun = (s.Tarih - bugun).Days;
            if (uyariGunleri.Contains(kalanGun) || kalanGun <= 0)
            {
                uyarilar.Add(new BelgeUyariItem
                {
                    EntityTipi = "Personel",
                    EntityAdi = s.AdSoyad,
                    BelgeTipi = "SRC Belgesi",
                    BitisTarihi = s.Tarih,
                    KalanGun = kalanGun
                });
            }
        }

        // Psikoteknik
        var psikolar = await context.Soforler
            .Where(p => p.Aktif && p.PsikoteknikGecerlilikTarihi.HasValue && 
                       p.PsikoteknikGecerlilikTarihi.Value <= sonrakiAy &&
                       p.PsikoteknikGecerlilikTarihi.Value >= bugun)
            .Select(p => new { AdSoyad = p.TamAd, Tarih = p.PsikoteknikGecerlilikTarihi!.Value })
            .ToListAsync();

        foreach (var p in psikolar)
        {
            var kalanGun = (p.Tarih - bugun).Days;
            if (uyariGunleri.Contains(kalanGun) || kalanGun <= 0)
            {
                uyarilar.Add(new BelgeUyariItem
                {
                    EntityTipi = "Personel",
                    EntityAdi = p.AdSoyad,
                    BelgeTipi = "Psikoteknik",
                    BitisTarihi = p.Tarih,
                    KalanGun = kalanGun
                });
            }
        }

        return uyarilar;
    }

    private async Task<List<BelgeUyariItem>> GetFirmaBelgeUyarilariAsync(ApplicationDbContext context, DateTime bugun, int[] uyariGunleri)
    {
        // Firma belgelerini kontrol et (varsa)
        // Şu an için boş döndür
        return await Task.FromResult(new List<BelgeUyariItem>());
    }

    private async Task GonderBelgeUyariEmailAsync(
        IEmailService emailService, 
        Kullanici admin,
        List<BelgeUyariItem> aracBelgeleri,
        List<BelgeUyariItem> personelBelgeleri,
        List<BelgeUyariItem> firmaBelgeleri)
    {
        var konu = $"[CRM Filo Servis] Belge Süresi Uyarısı - {DateTime.Today:dd.MM.yyyy}";

        var icerik = $@"
<h2>Belge Süresi Uyarı Raporu</h2>
<p>Tarih: {DateTime.Now:dd.MM.yyyy HH:mm}</p>

<h3 style='color: #0d6efd;'>🚗 Araç Belgeleri ({aracBelgeleri.Count} adet)</h3>
{(aracBelgeleri.Any() ? OlusturBelgeTablosu(aracBelgeleri) : "<p>Uyarılacak araç belgesi yok.</p>")}

<h3 style='color: #198754;'>👤 Personel Belgeleri ({personelBelgeleri.Count} adet)</h3>
{(personelBelgeleri.Any() ? OlusturBelgeTablosu(personelBelgeleri) : "<p>Uyarılacak personel belgesi yok.</p>")}

{(firmaBelgeleri.Any() ? $@"
<h3 style='color: #ffc107;'>🏢 Firma Belgeleri ({firmaBelgeleri.Count} adet)</h3>
{OlusturBelgeTablosu(firmaBelgeleri)}" : "")}

<hr>
<p style='color: #6c757d; font-size: 12px;'>
Bu email CRM Filo Servis sistemi tarafından otomatik olarak gönderilmiştir.
</p>";

        await emailService.SendEmailAsync(admin.Email!, konu, icerik, true);
    }

    private string OlusturBelgeTablosu(List<BelgeUyariItem> belgeler)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<table style='border-collapse: collapse; width: 100%;'>");
        sb.AppendLine("<thead>");
        sb.AppendLine("<tr style='background-color: #f8f9fa;'>");
        sb.AppendLine("<th style='border: 1px solid #dee2e6; padding: 8px; text-align: left;'>Kayıt</th>");
        sb.AppendLine("<th style='border: 1px solid #dee2e6; padding: 8px; text-align: left;'>Belge Tipi</th>");
        sb.AppendLine("<th style='border: 1px solid #dee2e6; padding: 8px; text-align: center;'>Bitiş Tarihi</th>");
        sb.AppendLine("<th style='border: 1px solid #dee2e6; padding: 8px; text-align: center;'>Kalan Gün</th>");
        sb.AppendLine("</tr>");
        sb.AppendLine("</thead>");
        sb.AppendLine("<tbody>");

        foreach (var belge in belgeler.OrderBy(b => b.KalanGun))
        {
            var satirRenk = belge.KalanGun switch
            {
                <= 0 => "background-color: #f8d7da;", // Süresi geçmiş
                <= 3 => "background-color: #fff3cd;", // Kritik
                <= 7 => "background-color: #d1e7dd;", // Yakın
                _ => ""
            };

            var gunRenk = belge.KalanGun switch
            {
                <= 0 => "color: #dc3545; font-weight: bold;",
                <= 3 => "color: #fd7e14; font-weight: bold;",
                <= 7 => "color: #ffc107;",
                _ => "color: #198754;"
            };

            sb.AppendLine($"<tr style='{satirRenk}'>");
            sb.AppendLine($"<td style='border: 1px solid #dee2e6; padding: 8px;'>{belge.EntityAdi}</td>");
            sb.AppendLine($"<td style='border: 1px solid #dee2e6; padding: 8px;'>{belge.BelgeTipi}</td>");
            sb.AppendLine($"<td style='border: 1px solid #dee2e6; padding: 8px; text-align: center;'>{belge.BitisTarihi:dd.MM.yyyy}</td>");
            sb.AppendLine($"<td style='border: 1px solid #dee2e6; padding: 8px; text-align: center; {gunRenk}'>{(belge.KalanGun <= 0 ? "SÜRESİ GEÇTİ!" : $"{belge.KalanGun} gün")}</td>");
            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</tbody>");
        sb.AppendLine("</table>");

        return sb.ToString();
    }
}

public class BelgeUyariItem
{
    public string EntityTipi { get; set; } = "";
    public string EntityAdi { get; set; } = "";
    public string BelgeTipi { get; set; } = "";
    public DateTime BitisTarihi { get; set; }
    public int KalanGun { get; set; }
}
