using CRMFiloServis.Web.Components;
using CRMFiloServis.Web.Data;
using CRMFiloServis.Web.Services;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

// EPPlus lisans ayari (NonCommercial kullanim icin)
ExcelPackage.License.SetNonCommercialPersonal("CRMFiloServis");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// PostgreSQL Database - Pooled DbContextFactory kullan (thread-safe)
builder.Services.AddPooledDbContextFactory<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
        npgsqlOptions.CommandTimeout(30);
    });
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
});

// DbContext - Factory'den olustur
builder.Services.AddScoped<ApplicationDbContext>(sp =>
    sp.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());

// Application Services
builder.Services.AddSingleton<IFirmaService, FirmaService>(); // Singleton - aktif firma state tutmak icin
builder.Services.AddSingleton<ILisansService, LisansService>(); // Singleton - lisans cache
builder.Services.AddSingleton<IKullaniciService, KullaniciService>(); // Singleton - aktif kullanici
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
builder.Services.AddScoped<IBackupService, BackupService>();
builder.Services.AddScoped<IAktiviteLogService, AktiviteLogService>();
builder.Services.AddScoped<IDatabaseSettingsService, DatabaseSettingsService>();
builder.Services.AddScoped<IMuhasebeService, MuhasebeService>();
builder.Services.AddScoped<ISatisService, SatisService>();
builder.Services.AddHostedService<AutoBackupService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DbInitializer.InitializeAsync(context);
    
    // Kullanici ve Lisans seed
    var kullaniciService = scope.ServiceProvider.GetRequiredService<IKullaniciService>();
    await kullaniciService.SeedAdminAsync();
    
    var lisansService = scope.ServiceProvider.GetRequiredService<ILisansService>();
    await lisansService.GetAktifLisansAsync(); // Trial lisans olusturur
    
    var satisService = scope.ServiceProvider.GetRequiredService<ISatisService>();
    await satisService.SeedMarkaModelAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
