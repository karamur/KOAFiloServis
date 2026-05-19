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
    private readonly IServiceProvider _rootServiceProvider;

    public TenantAwareDbContextFactory(
        PooledDbContextFactoryHolder inner,
        IHttpContextAccessor httpContextAccessor,
        IServiceProvider rootServiceProvider)
    {
        _inner = inner.Inner;
        _httpContextAccessor = httpContextAccessor;
        _rootServiceProvider = rootServiceProvider;
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
    /// Mevcut HTTP/circuit request scope'unu döndürür; yoksa (startup, background job,
    /// singleton tüketiciler) root provider'ı döndürür. Root provider verildiğinde
    /// IAktifFirmaProvider scoped çözümlenemediği için ApplicationDbContext tenant
    /// filtresini otomatik devre dışı bırakır — bu, lisans/global tablo erişimi için doğrudur.
    /// </summary>
    private IServiceProvider ResolveScope()
        => _httpContextAccessor.HttpContext?.RequestServices ?? _rootServiceProvider;
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
