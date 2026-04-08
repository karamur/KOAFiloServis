using CRMFiloServis.Web.Components;
using CRMFiloServis.Web.Data;
using CRMFiloServis.Web.Helpers;
using CRMFiloServis.Web.Services;
using CRMFiloServis.Web.Services.Interfaces;
using CRMFiloServis.Shared.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Text.Json;

// EPPlus lisans ayari (NonCommercial kullanim icin)
ExcelPackage.License.SetNonCommercialPersonal("CRMFiloServis");

var builder = WebApplication.CreateBuilder(args);

// Database Provider Secimi (dbsettings.json varsa onu oncele)
var dbProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "SQLite";
var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
var dbSettingsPath = Path.Combine(builder.Environment.ContentRootPath, "dbsettings.json");

if (File.Exists(dbSettingsPath))
{
    try
    {
        var dbSettingsJson = await File.ReadAllTextAsync(dbSettingsPath);
        var dbSettings = JsonSerializer.Deserialize<DatabaseSettings>(dbSettingsJson);
        if (dbSettings != null)
        {
            dbProvider = dbSettings.Provider switch
            {
                DatabaseProvider.PostgreSQL => "PostgreSQL",
                DatabaseProvider.MySQL => "MySQL",
                DatabaseProvider.SQLServer => "SQLServer",
                _ => "SQLite"
            };
            defaultConnectionString = dbSettings.GetConnectionString();
        }
    }
    catch
    {
        // dbsettings.json okunamazsa appsettings ile devam et
    }
}

// Diger PC'lerden erisim icin tum IP'lerden dinle
// Kurulum ortami icin varsayilan olarak sadece HTTP acilir.
// HTTPS kullanilacaksa kullanici sertifika/URL ayarini disaridan vermelidir.
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")) &&
    !args.Any(a => a.StartsWith("--urls")))
{
    builder.WebHost.UseUrls("http://0.0.0.0:5190");
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<AktiviteLogInterceptor>();
builder.Services.AddSingleton<ICurrentUserAccessor, CurrentUserAccessor>();

// Database - Pooled DbContextFactory kullan (thread-safe)
builder.Services.AddPooledDbContextFactory<ApplicationDbContext>((sp, options) =>
{
    var enableSensitiveDataLogging = builder.Environment.IsDevelopment() &&
        builder.Configuration.GetValue<bool>("EntityFramework:EnableSensitiveDataLogging");

    if (dbProvider == "PostgreSQL")
    {
        // PostgreSQL timestamp ayari
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        options.UseNpgsql(defaultConnectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);
            npgsqlOptions.CommandTimeout(30);
            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "public");
        });
    }
    else // SQLite
    {
        options.UseSqlite(defaultConnectionString);
    }
    
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    if (enableSensitiveDataLogging)
    {
        options.EnableSensitiveDataLogging();
    }

    // Pending migration uyarisini devre disi birak
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    options.AddInterceptors(sp.GetRequiredService<AktiviteLogInterceptor>());
});

// DbContext - Factory'den olustur
builder.Services.AddScoped<ApplicationDbContext>(sp =>
    sp.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());

// Authentication - Her circuit (tarayici baglantisi) icin bagimsiz oturum yonetimi
// Scoped: Her Blazor circuit kendi oturumunu yonetir - farkli PC/tarayicilar birbirini etkilemez
builder.Services.AddScoped<AppAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => 
    sp.GetRequiredService<AppAuthenticationStateProvider>());
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
var dataProtectionKeysRoot = new DirectoryInfo(AppStoragePaths.GetDataProtectionKeysRoot(builder.Environment.ContentRootPath));
dataProtectionKeysRoot.Create();
builder.Services.AddDataProtection()
    .SetApplicationName("CRMFiloServis");
builder.Services.AddSingleton<IConfigureOptions<KeyManagementOptions>>(sp =>
    new ConfigureOptions<KeyManagementOptions>(options =>
    {
        options.XmlRepository = new FileSystemXmlRepository(dataProtectionKeysRoot, sp.GetRequiredService<ILoggerFactory>());
    }));
builder.Services.AddSingleton<IPortalProjectCatalogService, PortalProjectCatalogService>();
builder.Services.AddSingleton<ISecureFileService, SecureFileService>();

// Application Services
builder.Services.AddSingleton<IFirmaService, FirmaService>(); // Singleton - aktif firma state tutmak icin
builder.Services.AddSingleton<ILisansService, LisansService>(); // Singleton - lisans cache
builder.Services.AddScoped<IKullaniciService, KullaniciService>(); // Scoped - her circuit kendi oturumunu yonetir
builder.Services.AddScoped<ICariService, CariService>();
builder.Services.AddScoped<ISoforService, SoforService>();
builder.Services.AddScoped<IAracService, AracService>();
builder.Services.AddScoped<IGuzergahService, GuzergahService>();
builder.Services.AddScoped<IMasrafKalemiService, MasrafKalemiService>();
builder.Services.AddScoped<IAracMasrafService, AracMasrafService>();
builder.Services.AddScoped<IServisCalismaService, ServisCalismaService>();
builder.Services.AddScoped<IFaturaService, FaturaService>();
builder.Services.AddScoped<IBankaHesapService, BankaHesapService>();
builder.Services.AddScoped<IBankaKasaHareketService, BankaKasaHareketService>();
builder.Services.AddScoped<IOdemeEslestirmeService, OdemeEslestirmeService>();
builder.Services.AddScoped<IRaporService, RaporService>();
builder.Services.AddScoped<IExcelService, ExcelService>();
builder.Services.AddScoped<IFaturaHazirlikService, FaturaHazirlikService>();
builder.Services.AddScoped<IMaliAnalizService, MaliAnalizService>();
builder.Services.AddScoped<IPersonelMaasIzinService, PersonelMaasIzinService>();
builder.Services.AddScoped<IBelgeUyariService, BelgeUyariService>();
builder.Services.AddScoped<IDashboardGrafikService, DashboardGrafikService>();
builder.Services.AddScoped<IGlobalSearchService, GlobalSearchService>();
builder.Services.AddScoped<IToastService, ToastService>();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<ITekrarlayanOdemeService, TekrarlayanOdemeService>(); // Kredi/Taksit Ynetimi
builder.Services.AddScoped<IBackupService, BackupService>();
builder.Services.AddScoped<IAktiviteLogService, AktiviteLogService>();
builder.Services.AddScoped<IDatabaseSettingsService, DatabaseSettingsService>();
builder.Services.AddScoped<IMuhasebeService, MuhasebeService>();
builder.Services.AddScoped<ISatisService, SatisService>();
builder.Services.AddScoped<IPuantajService, PuantajService>();
builder.Services.AddScoped(typeof(CRMFiloServis.Web.Services.Interfaces.IFiloKomisyonService), typeof(FiloKomisyonService));
builder.Services.AddScoped<IAracDegerlemeAIService, AracDegerlemeAIService>(); // AI Arac Degerleme
builder.Services.AddScoped<IPiyasaKaynakService, PiyasaKaynakService>(); // Piyasa Kaynak Yonetimi (once kaydet)
builder.Services.AddScoped<IHttpScraperService, HttpScraperService>(); // HTTP Scraper (en hizli)
builder.Services.AddScoped<IPlaywrightScraperService, PlaywrightScraperService>(); // Playwright Web Scraper (yedek)
builder.Services.AddScoped<IAracPiyasaArastirmaService, AracPiyasaArastirmaService>(); // AI Piyasa Arastirma
builder.Services.AddScoped<IMusteriKiralamaService, MusteriKiralamaService>(); // Musteri Kiralama Servisi
builder.Services.AddScoped<ICRMService, CRMService>(); // CRM Servisi - Bildirim, Mesaj, Hatırlatıcı
builder.Services.AddScoped<CRMFiloServis.Web.Services.Interfaces.IWhatsAppService, WhatsAppService>(); // WhatsApp Servisi
builder.Services.AddScoped<IStokService, StokService>(); // Stok/Envanter Servisi
builder.Services.AddScoped<IPersonelOzlukService, PersonelOzlukService>(); // Personel Özlük Evrak Servisi
builder.Services.AddScoped<IPersonelFinansService, PersonelFinansService>(); // Personel Finans (Avans/Borç) Servisi
builder.Services.AddScoped<IBordroService, BordroService>(); // Bordro Servisi
builder.Services.AddScoped<IFiloOperasyonService, FiloOperasyonService>(); // Filo Operasyon (Komisyonculuk, Alım/Satım, Plaka Dönüşüm)
builder.Services.AddScoped<IIlanYayinService, IlanYayinService>(); // Araç İlan Yayın ve Kullanıcı Tercihleri
builder.Services.AddScoped<IHakedisService, HakedisService>(); // Hakedis/Puantaj Excel Import ve Takip
builder.Services.AddScoped<IProformaFaturaService, ProformaFaturaService>(); // Proforma Fatura Servisi
builder.Services.AddScoped<ICariHareketTakipService, CariHareketTakipService>(); // Cari Borç/Alacak Takip Servisi
builder.Services.AddScoped<UpdateService>(); // Güncelleme Yönetimi Servisi
builder.Services.AddScoped<IEmailService, EmailService>(); // E-posta Bildirim Servisi
builder.Services.AddScoped<ISystemHealthService, SystemHealthService>(); // Sistem Sağlık Kontrolü
builder.Services.AddHostedService<DatabaseBackupService>(); // Otomatik Veritabanı Yedekleme
builder.Services.AddHostedService<BelgeUyariBackgroundService>(); // Belge Süresi Email Uyarı
builder.Services.AddHttpClient("OpenAI"); // OpenAI icin HttpClient
builder.Services.AddHttpClient("Scraper"); // Scraper icin HttpClient
builder.Services.AddHttpClient("Ollama"); // Ollama Local LLM icin HttpClient
builder.Services.AddScoped<IOllamaService, OllamaService>(); // Ollama AI Rapor Yorumlama
builder.Services.AddScoped<IFaturaAIImportService, FaturaAIImportService>(); // AI Fatura Import Servisi
builder.Services.AddScoped<IIhaleHazirlikService, IhaleHazirlikService>(); // İhale Hazırlık Servisi
builder.Services.AddScoped<ICariRiskService, CariRiskService>(); // Cari Risk Analizi Servisi
builder.Services.AddScoped<IKolayMuhasebeService, KolayMuhasebeService>(); // Kolay Muhasebe Girişi Servisi
builder.Services.AddScoped<ITopluFaturaService, TopluFaturaService>(); // Toplu Fatura Oluşturma Servisi
builder.Services.AddHostedService<AutoBackupService>();
builder.Services.AddHttpContextAccessor();

// API Controller destegi - Mobil uygulama icin
builder.Services.AddControllers();

var app = builder.Build();

static async Task RunScopedAsync(WebApplication app, Func<IServiceProvider, Task> action)
{
    using var scope = app.Services.CreateScope();
    await action(scope.ServiceProvider);
}

// Seed Database
await RunScopedAsync(app, async services =>
{
    var context = services.GetRequiredService<ApplicationDbContext>();
    var configuration = services.GetRequiredService<IConfiguration>();
    await DbInitializer.InitializeAsync(context, configuration);
});

await RunScopedAsync(app, async services =>
{
    var context = services.GetRequiredService<ApplicationDbContext>();
    await CRMFiloServis.Web.Data.Migrations.PersonelTableMigrationHelper.ApplyPersonelTableMigrationAsync(context);
});

await RunScopedAsync(app, async services =>
{
    var context = services.GetRequiredService<ApplicationDbContext>();
    await CRMFiloServis.Web.Data.Migrations.PersonelMaasHesaplamaMigrationHelper.ApplyPersonelMaasHesaplamaAsync(context);
});

await RunScopedAsync(app, async services =>
{
    var context = services.GetRequiredService<ApplicationDbContext>();
    await CRMFiloServis.Web.Data.Migrations.SoforMaasMigrationHelper.ApplySoforMaasAlanlariAsync(context);
});

await RunScopedAsync(app, async services =>
{
    var context = services.GetRequiredService<ApplicationDbContext>();
    await DbSeeder.SeedAsync(context);
});

await RunScopedAsync(app, async services =>
{
    var kullaniciService = services.GetRequiredService<IKullaniciService>();
    await kullaniciService.SeedAdminAsync();
});

await RunScopedAsync(app, async services =>
{
    var lisansService = services.GetRequiredService<ILisansService>();
    await lisansService.GetAktifLisansAsync(); // Trial lisans olusturur
});

await RunScopedAsync(app, async services =>
{
    var satisService = services.GetRequiredService<ISatisService>();
    await satisService.SeedMarkaModelAsync();
});

await RunScopedAsync(app, async services =>
{
    var muhasebeService = services.GetRequiredService<IMuhasebeService>();
    await muhasebeService.SeedVarsayilanHesapPlaniAsync();
});

await RunScopedAsync(app, async services =>
{
    var piyasaKaynakService = services.GetRequiredService<IPiyasaKaynakService>();
    await piyasaKaynakService.SeedDefaultKaynaklarAsync();
});

await RunScopedAsync(app, async services =>
{
    var budgetService = services.GetRequiredService<IBudgetService>();
    await budgetService.SeedMasrafKalemleriAsync();
});

await RunScopedAsync(app, async services =>
{
    var context = services.GetRequiredService<ApplicationDbContext>();
    await CRMFiloServis.Web.Data.Migrations.CariMigrationHelper.ApplyCariAlanGenisletmeAsync(context);
});

await RunScopedAsync(app, async services =>
{
    var context = services.GetRequiredService<ApplicationDbContext>();
    await CRMFiloServis.Web.Data.Migrations.BordroMigrationHelper.ApplyBordroTablolariAsync(context);
});

await RunScopedAsync(app, async services =>
{
    var context = services.GetRequiredService<ApplicationDbContext>();
    await CRMFiloServis.Web.Data.Migrations.AracMasrafMuhasebeMigrationHelper.ApplyAracMasrafMuhasebeAlanlariAsync(context);
});

await RunScopedAsync(app, async services =>
{
    var context = services.GetRequiredService<ApplicationDbContext>();
    await CRMFiloServis.Web.Data.Migrations.OzlukEvrakMigrationHelper.ApplyOzlukEvrakMigrationAsync(context);
});

await RunScopedAsync(app, async services =>
{
    var context = services.GetRequiredService<ApplicationDbContext>();
    await CRMFiloServis.Web.Data.Migrations.MuhasebeAyarMigrationHelper.ApplyStokMasrafAyarlariAsync(context);
});

await RunScopedAsync(app, async services =>
{
    var ozlukService = services.GetRequiredService<IPersonelOzlukService>();
    await ozlukService.SeedDefaultEvrakTanimlariAsync();
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

// HTTPS yonlendirme - sadece HTTPS portu aktifse calistir
// Ag uzerinden HTTP ile erisme sorun cikarmasin diye kontrol eklendi
var httpsPort = app.Configuration.GetValue<int?>("HTTPS_PORT") ?? 
                (app.Urls.Any(u => u.StartsWith("https")) ? 7113 : (int?)null);
if (httpsPort.HasValue)
{
    app.UseHttpsRedirection();
}

app.UseAntiforgery();

var externalUploadsPath = AppStoragePaths.GetUploadsRoot(app.Environment.ContentRootPath);
Directory.CreateDirectory(externalUploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(externalUploadsPath),
    RequestPath = "/uploads"
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapControllers(); // API Controller'larini haritalandir

app.Run();
