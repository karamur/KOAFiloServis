using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using KOAFiloServis.Shared.Entities;

namespace KOAFiloServis.Web.Services;

/// <summary>
/// Her kullanici/tarayici (circuit) icin bagimsiz oturum yonetimi saglayan Authentication Provider.
/// Scoped olarak kayitli - her Blazor circuit kendi instance'ini alir.
/// NOT: Bu provider static degisken KULLANMAZ - her circuit bagimsizdir.
/// </summary>
public class AppAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILogger<AppAuthenticationStateProvider> _logger;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
    private Kullanici? _aktifKullanici;
    private string? _sessionId;

    public AppAuthenticationStateProvider(
        ILogger<AppAuthenticationStateProvider> logger,
        ICurrentUserAccessor currentUserAccessor)
    {
        _logger = logger;
        _currentUserAccessor = currentUserAccessor;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(_currentUser));
    }

    /// <summary>
    /// Kullaniciyi oturum acar
    /// </summary>
    public void GirisYap(Kullanici kullanici)
    {
        _aktifKullanici = kullanici;
        _sessionId = Guid.NewGuid().ToString("N");

        // CurrentUserAccessor'a kullanıcı bilgisini set et (interceptor için)
        _currentUserAccessor.SetCurrentUser(kullanici.KullaniciAdi, kullanici.AdSoyad);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, kullanici.Id.ToString()),
            new Claim(ClaimTypes.Name, kullanici.KullaniciAdi),
            new Claim("AdSoyad", kullanici.AdSoyad),
            new Claim(ClaimTypes.Role, kullanici.Rol?.RolAdi ?? "Kullanici"),
            new Claim("SessionId", _sessionId)
        };

        if (!string.IsNullOrEmpty(kullanici.Email))
            claims.Add(new Claim(ClaimTypes.Email, kullanici.Email));

        var identity = new ClaimsIdentity(claims, "KOAFiloServisAuth");
        _currentUser = new ClaimsPrincipal(identity);

        _logger.LogInformation("Kullanici giris yapti: {KullaniciAdi}, Rol: {Rol}, SessionId: {SessionId}", 
            kullanici.KullaniciAdi, kullanici.Rol?.RolAdi, _sessionId);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
    }

    /// <summary>
    /// Async giris - uyumluluk icin
    /// </summary>
    public Task GirisYapAsync(Kullanici kullanici)
    {
        GirisYap(kullanici);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Kullaniciyi oturumdan cikarir
    /// </summary>
    public void CikisYap()
    {
        var kullaniciAdi = _aktifKullanici?.KullaniciAdi;

        _aktifKullanici = null;
        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

        // CurrentUserAccessor'dan kullanıcıyı temizle
        _currentUserAccessor.ClearCurrentUser();

        _logger.LogInformation("Kullanici cikis yapti: {KullaniciAdi}", kullaniciAdi);
        
        _sessionId = null;

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
    }

    /// <summary>
    /// Async cikis - uyumluluk icin
    /// </summary>
    public Task CikisYapAsync()
    {
        CikisYap();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Aktif kullaniciyi dondurur
    /// </summary>
    public Kullanici? GetAktifKullanici() => _aktifKullanici;

    /// <summary>
    /// Kullanici giris yapmis mi kontrol eder
    /// </summary>
    public bool IsAuthenticated => _aktifKullanici != null;

    /// <summary>
    /// Mevcut session ID'yi dondurur
    /// </summary>
    public string? GetSessionId() => _sessionId;
}
