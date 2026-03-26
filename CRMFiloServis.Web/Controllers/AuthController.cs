using Microsoft.AspNetCore.Mvc;
using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Services;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace CRMFiloServis.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        ILogger<AuthController> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    /// <summary>
    /// Mobil uygulama icin login - her cihaz bagimsiz token alir
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.KullaniciAdi) || string.IsNullOrWhiteSpace(request.Sifre))
        {
            return BadRequest(new { Basarili = false, Mesaj = "Kullanici adi ve sifre gerekli!" });
        }

        using var context = await _contextFactory.CreateDbContextAsync();
        var kullanici = await context.Kullanicilar
            .Include(k => k.Rol)
            .FirstOrDefaultAsync(k => k.KullaniciAdi == request.KullaniciAdi);

        if (kullanici == null)
        {
            _logger.LogWarning("API Login basarisiz - kullanici bulunamadi: {KullaniciAdi}", request.KullaniciAdi);
            return Unauthorized(new { Basarili = false, Mesaj = "Kullanici bulunamadi" });
        }

        if (!kullanici.Aktif)
        {
            _logger.LogWarning("API Login basarisiz - kullanici aktif degil: {KullaniciAdi}", request.KullaniciAdi);
            return Unauthorized(new { Basarili = false, Mesaj = "Kullanici aktif degil" });
        }

        if (kullanici.Kilitli)
        {
            _logger.LogWarning("API Login basarisiz - kullanici kilitli: {KullaniciAdi}", request.KullaniciAdi);
            return Unauthorized(new { Basarili = false, Mesaj = "Kullanici kilitli. Yoneticiye basvurun." });
        }

        if (!VerifyPassword(request.Sifre, kullanici.SifreHash))
        {
            kullanici.BasarisizGirisSayisi++;
            if (kullanici.BasarisizGirisSayisi >= 5)
            {
                kullanici.Kilitli = true;
                _logger.LogWarning("Kullanici kilitlendi (5 basarisiz deneme): {KullaniciAdi}", request.KullaniciAdi);
            }
            context.Kullanicilar.Update(kullanici);
            await context.SaveChangesAsync();

            _logger.LogWarning("API Login basarisiz - sifre hatali: {KullaniciAdi}", request.KullaniciAdi);
            return Unauthorized(new { Basarili = false, Mesaj = "Sifre hatali" });
        }

        // Basarili giris
        kullanici.SonGirisTarihi = DateTime.UtcNow;
        kullanici.BasarisizGirisSayisi = 0;
        context.Kullanicilar.Update(kullanici);
        await context.SaveChangesAsync();

        // Her cihaz icin benzersiz token olustur
        var deviceId = request.DeviceId ?? Guid.NewGuid().ToString("N");
        var token = GenerateSecureToken(kullanici.Id, deviceId);

        _logger.LogInformation("API Login basarili: {KullaniciAdi}, Rol: {Rol}, DeviceId: {DeviceId}",
            request.KullaniciAdi, kullanici.Rol?.RolAdi, deviceId);

        return Ok(new
        {
            Basarili = true,
            Kullanici = new
            {
                kullanici.Id,
                kullanici.KullaniciAdi,
                kullanici.AdSoyad,
                kullanici.Email,
                kullanici.Telefon,
                kullanici.SonGirisTarihi,
                Rol = new
                {
                    kullanici.Rol?.Id,
                    kullanici.Rol?.RolAdi,
                    kullanici.Rol?.Aciklama
                }
            },
            Token = token,
            DeviceId = deviceId,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });
    }

    /// <summary>
    /// Mobil uygulama icin logout
    /// </summary>
    [HttpPost("logout")]
    public IActionResult Logout([FromBody] LogoutRequest? request)
    {
        // API tabanlý logout - token'ý istemci tarafýnda silmek yeterli
        // Ýleride token blacklist eklenebilir
        _logger.LogInformation("API Logout: DeviceId: {DeviceId}", request?.DeviceId);
        return Ok(new { Basarili = true });
    }

    /// <summary>
    /// Token dogrulama - mobil uygulama baslangicinda kullanilir
    /// </summary>
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return BadRequest(new { Gecerli = false, Mesaj = "Token gerekli" });
        }

        var tokenData = ParseToken(request.Token);
        if (tokenData == null || tokenData.ExpiresAt < DateTime.UtcNow)
        {
            return Ok(new { Gecerli = false, Mesaj = "Token gecersiz veya suresi dolmus" });
        }

        using var context = await _contextFactory.CreateDbContextAsync();
        var kullanici = await context.Kullanicilar
            .Include(k => k.Rol)
            .FirstOrDefaultAsync(k => k.Id == tokenData.UserId);

        if (kullanici == null || !kullanici.Aktif || kullanici.Kilitli)
        {
            return Ok(new { Gecerli = false, Mesaj = "Kullanici gecersiz veya aktif degil" });
        }

        return Ok(new
        {
            Gecerli = true,
            Kullanici = new
            {
                kullanici.Id,
                kullanici.KullaniciAdi,
                kullanici.AdSoyad,
                Rol = kullanici.Rol?.RolAdi
            }
        });
    }

    /// <summary>
    /// Guvenli token olusturur - cihaz bazli
    /// </summary>
    private string GenerateSecureToken(int userId, string deviceId)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var expiry = DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds();
        var randomBytes = new byte[16];
        RandomNumberGenerator.Fill(randomBytes);
        var random = Convert.ToBase64String(randomBytes);

        var payload = $"{userId}:{deviceId}:{timestamp}:{expiry}:{random}";
        var signature = ComputeHmac(payload);

        return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{payload}:{signature}"));
    }

    /// <summary>
    /// Token'i parse eder ve dogrular
    /// </summary>
    private TokenData? ParseToken(string token)
    {
        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var parts = decoded.Split(':', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 6) return null;

            var payload = string.Join(":", parts.Take(5));
            var signature = parts[5];

            if (ComputeHmac(payload) != signature) return null;

            return new TokenData
            {
                UserId = int.Parse(parts[0]),
                DeviceId = parts[1],
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(parts[2])).UtcDateTime,
                ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(parts[3])).UtcDateTime
            };
        }
        catch
        {
            return null;
        }
    }

    private string ComputeHmac(string data)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("CRMFiloServisSecretKey2024!"));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }

    private string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password + "CRMFiloServisSalt"));
        return Convert.ToBase64String(bytes);
    }

    private bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}

public class LoginRequest
{
    public string KullaniciAdi { get; set; } = "";
    public string Sifre { get; set; } = "";
    public string? DeviceId { get; set; }
}

public class LogoutRequest
{
    public string? DeviceId { get; set; }
    public string? Token { get; set; }
}

public class ValidateTokenRequest
{
    public string Token { get; set; } = "";
}

internal class TokenData
{
    public int UserId { get; set; }
    public string DeviceId { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
