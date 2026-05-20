using Microsoft.EntityFrameworkCore;

namespace KOAFiloServis.Web.Data;

/// <summary>
/// Pooled <see cref="IDbContextFactory{ApplicationDbContext}"/>'i sarmalayan scoped factory.
/// Her oluşturulan <see cref="ApplicationDbContext"/>'e mevcut scope'un
/// <see cref="IServiceProvider"/>'ını enjekte eder; bu sayede <c>IAktifFirmaProvider</c>
/// üzerinden tenant (firma) global query filter'ları doğru biçimde devreye girer.
/// </summary>
/// <remarks>
/// <para>
/// Bug: v1.0.21 öncesi, servisler doğrudan pooled <c>IDbContextFactory&lt;ApplicationDbContext&gt;</c>
/// kullanıyordu; bu durumda <see cref="ApplicationDbContext.SetServiceProvider"/> hiç çağrılmıyor,
/// dolayısıyla <c>IAktifFirmaProvider</c> çözümlenemiyor ve <c>FirmaTenantDisabled</c> true oluyordu —
/// sonuç: tüm firmaların kayıtları (örn. "Araç Düzenle"de tüm araçlar) listeleniyordu.
/// </para>
/// </remarks>
public sealed class TenantAwareDbContextFactory : IDbContextFactory<ApplicationDbContext>
{
    private readonly IDbContextFactory<ApplicationDbContext> _inner;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _scopedServiceProvider;

    public TenantAwareDbContextFactory(
        PooledDbContextFactoryHolder inner,
        IHttpContextAccessor httpContextAccessor,
        IServiceProvider scopedServiceProvider)
    {
        _inner = inner.Inner;
        _httpContextAccessor = httpContextAccessor;
        _scopedServiceProvider = scopedServiceProvider;
    }

    public ApplicationDbContext CreateDbContext()
    {
        var ctx = _inner.CreateDbContext();
        ctx.SetServiceProvider(ResolveScope());
        return ctx;
    }

    public async Task<ApplicationDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        var ctx = await _inner.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        ctx.SetServiceProvider(ResolveScope());
        return ctx;
    }

    /// <summary>
    /// Aktif firma (tenant) bilgisi için doğru <see cref="IServiceProvider"/> kapsamını döndürür.
    /// <para>
    /// Bu factory <b>Scoped</b> olarak kayıtlıdır, bu yüzden constructor'a inject edilen
    /// <see cref="IServiceProvider"/> her zaman geçerli scope'un (HTTP isteği veya Blazor Server SignalR circuit)
    /// provider'ıdır. Tenant aware <see cref="IAktifFirmaProvider"/> (Scoped) bu provider üzerinden
    /// doğrudan çözümlenir.
    /// </para>
    /// <para>
    /// Bug: Önceden önce <see cref="IHttpContextAccessor.HttpContext"/> kontrol ediliyordu. Blazor Server'da
    /// kullanıcı butona tıkladığında istek SignalR circuit üzerinden geldiği için <c>HttpContext</c> <b>null</b>
    /// olur ve fallback yolu farklı bir scope'a düşebiliyor, bu da circuit'in <c>AktifFirmaProvider</c>
    /// instance'ına ulaşamamaya ve dolayısıyla bütçe ödemesinde
    /// "Aktif firma seçili olmadan 'BankaKasaHareket' kaydı eklenemez" hatasına neden oluyordu.
    /// Çözüm: doğrudan scoped provider'ı kullan; bu hem HTTP istekleri hem de Blazor circuit'i için doğru scope'tur.
    /// </para>
    /// </summary>
    private IServiceProvider ResolveScope() => _scopedServiceProvider;
}

/// <summary>
/// Pooled <see cref="IDbContextFactory{ApplicationDbContext}"/>'in singleton tutucusu.
/// <see cref="TenantAwareDbContextFactory"/>'nin altında delege ettiği "asıl" factory'yi sağlar.
/// </summary>
public sealed class PooledDbContextFactoryHolder
{
    public IDbContextFactory<ApplicationDbContext> Inner { get; }

    public PooledDbContextFactoryHolder(IDbContextFactory<ApplicationDbContext> inner)
    {
        Inner = inner;
    }
}
