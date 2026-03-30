using CRMFiloServis.Web.Components;
using CRMFiloServis.Web.Data;
using CRMFiloServis.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

// EPPlus lisans ayari (NonCommercial kullanim icin)
ExcelPackage.License.SetNonCommercialPersonal("CRMFiloServis");

var builder = WebApplication.CreateBuilder(args);

// Database Provider Secimi (appsettings.json'dan)
var dbProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "SQLite";

// Diger PC'lerden erisim icin tum IP'lerden dinle
// Eger HTTPS_PORT ortam degiskeni veya --urls parametresi verilmisse onu kullan
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")) &&
    !args.Any(a => a.StartsWith("--urls")))
{
    builder.WebHost.UseUrls("http://0.0.0.0:5190", "https://0.0.0.0:7113");
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Database - Pooled DbContextFactory kullan (thread-safe)
builder.Services.AddPooledDbContextFactory<ApplicationDbContext>(options =>
{
    if (dbProvider == "PostgreSQL")
    {
        // PostgreSQL timestamp ayari
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        
        options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL"), npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);
            npgsqlOptions.CommandTimeout(30);
        });
    }
    else // SQLite
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("SQLite"));
    }
    
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    // Pending migration uyarisini devre disi birak
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
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
builder.Services.AddHttpClient("OpenAI"); // OpenAI icin HttpClient
builder.Services.AddHttpClient("Scraper"); // Scraper icin HttpClient
builder.Services.AddHostedService<AutoBackupService>();
builder.Services.AddHttpContextAccessor();

// API Controller destegi - Mobil uygulama icin
builder.Services.AddControllers();

var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    await DbInitializer.InitializeAsync(context, configuration);
    
    // Seed kritik verileri
    await DbSeeder.SeedAsync(context);
    
    // Kullanici ve Lisans seed
    var kullaniciService = scope.ServiceProvider.GetRequiredService<IKullaniciService>();
    await kullaniciService.SeedAdminAsync();
    
    var lisansService = scope.ServiceProvider.GetRequiredService<ILisansService>();
    await lisansService.GetAktifLisansAsync(); // Trial lisans olusturur
    
    var satisService = scope.ServiceProvider.GetRequiredService<ISatisService>();
    await satisService.SeedMarkaModelAsync();
    
    // Muhasebe hesap plani seed
    var muhasebeService = scope.ServiceProvider.GetRequiredService<IMuhasebeService>();
    await muhasebeService.SeedVarsayilanHesapPlaniAsync();
    
    // Piyasa kaynakları seed
    var piyasaKaynakService = scope.ServiceProvider.GetRequiredService<IPiyasaKaynakService>();
    await piyasaKaynakService.SeedDefaultKaynaklarAsync();
    
    // Bütçe masraf kalemleri seed (Kredi Kartı dahil)
    var budgetService = scope.ServiceProvider.GetRequiredService<IBudgetService>();
    await budgetService.SeedMasrafKalemleriAsync();
}

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

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapControllers(); // API Controller'larini haritalandir

app.Run();
