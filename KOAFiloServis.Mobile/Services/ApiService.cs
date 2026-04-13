using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.Extensions.Logging;

namespace KOAFiloServis.Mobile.Services;

/// <summary>
/// REST API iletişim servisi
/// </summary>
public class ApiService : IApiService
{
    private const string VarsayilanSunucuAdresi = "http://192.168.1.112:5190/";
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<ApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    
    private string? _token;
    private DateTime? _tokenSonlanma;
    
    private const string TokenKey = "jwt_token";
    private const string TokenSonlanmaKey = "jwt_token_expiry";
    private const string KullaniciBilgisiKey = "kullanici_bilgisi";
    private const string SunucuAdresiKey = "sunucu_adresi";
    
    public bool TokenGecerliMi => !string.IsNullOrEmpty(_token) && 
                                   _tokenSonlanma.HasValue && 
                                   _tokenSonlanma.Value > DateTime.UtcNow;

    public ApiService(
        HttpClient httpClient, 
        ILocalStorageService localStorage,
        ILogger<ApiService> logger)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Uygulama başladığında token'ı yükle
    /// </summary>
    public async Task TokenYukleAsync()
    {
        try
        {
            await SunucuAdresiYukleAsync();
            _token = await _localStorage.GetItemAsync<string>(TokenKey);
            _tokenSonlanma = await _localStorage.GetItemAsync<DateTime?>(TokenSonlanmaKey);
            
            if (TokenGecerliMi)
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", _token);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token yüklenirken hata oluştu");
        }
    }

    public async Task<GirisYanit?> GirisYapAsync(string kullaniciAdi, string sifre)
    {
        try
        {
            await SunucuAdresiYukleAsync();
            var request = new { KullaniciAdi = kullaniciAdi, Sifre = sifre };
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request, _jsonOptions);
            
            if (response.IsSuccessStatusCode)
            {
                var yanit = await response.Content.ReadFromJsonAsync<GirisYanit>(_jsonOptions);
                
                if (yanit?.Basarili == true && !string.IsNullOrEmpty(yanit.Token))
                {
                    _token = yanit.Token;
                    _tokenSonlanma = yanit.TokenSonlanma ?? DateTime.UtcNow.AddHours(24);
                    
                    // Token'ı kaydet
                    await _localStorage.SetItemAsync(TokenKey, _token);
                    await _localStorage.SetItemAsync(TokenSonlanmaKey, _tokenSonlanma);
                    
                    if (yanit.Kullanici != null)
                    {
                        await _localStorage.SetItemAsync(KullaniciBilgisiKey, yanit.Kullanici);
                    }
                    
                    // HTTP client'a header ekle
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new AuthenticationHeaderValue("Bearer", _token);
                }
                
                return yanit;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Giriş başarısız: {StatusCode} - {Error}", response.StatusCode, errorContent);
            
            return new GirisYanit 
            { 
                Basarili = false, 
                Mesaj = $"Giriş başarısız: {response.StatusCode}" 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Giriş yapılırken hata oluştu");
            return new GirisYanit 
            { 
                Basarili = false, 
                Mesaj = "Bağlantı hatası. Lütfen internet bağlantınızı kontrol edin." 
            };
        }
    }

    public async Task CikisYapAsync()
    {
        try
        {
            _token = null;
            _tokenSonlanma = null;
            
            await _localStorage.RemoveItemAsync(TokenKey);
            await _localStorage.RemoveItemAsync(TokenSonlanmaKey);
            await _localStorage.RemoveItemAsync(KullaniciBilgisiKey);
            
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Çıkış yapılırken hata oluştu");
        }
    }

    public async Task<KullaniciBilgisi?> KullaniciBilgisiGetirAsync()
    {
        try
        {
            // Önce cache'den dene
            var cached = await _localStorage.GetItemAsync<KullaniciBilgisi>(KullaniciBilgisiKey);
            if (cached != null) return cached;
            
            // API'den al
            if (!TokenGecerliMi) return null;
            
            var response = await _httpClient.GetAsync("api/auth/me");
            if (response.IsSuccessStatusCode)
            {
                var kullanici = await response.Content.ReadFromJsonAsync<KullaniciBilgisi>(_jsonOptions);
                if (kullanici != null)
                {
                    await _localStorage.SetItemAsync(KullaniciBilgisiKey, kullanici);
                }
                return kullanici;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kullanıcı bilgisi alınırken hata oluştu");
        }
        return null;
    }

    public async Task<List<AracOzet>> SoforAraclariniGetirAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/mobile/araclar");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<AracOzet>>(_jsonOptions) ?? new();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şoför araçları alınırken hata oluştu");
        }
        return new();
    }

    public async Task<List<SeferOzet>> AktifSeferleriGetirAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/mobile/seferler/aktif");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<SeferOzet>>(_jsonOptions) ?? new();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Aktif seferler alınırken hata oluştu");
        }
        return new();
    }

    public async Task<SeferDetay?> SeferBaslatAsync(SeferBaslatRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/mobile/seferler/baslat", request, _jsonOptions);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SeferDetay>(_jsonOptions);
            }
            
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Sefer başlatılamadı: {Error}", error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sefer başlatılırken hata oluştu");
        }
        return null;
    }

    public async Task<SeferDetay?> SeferBitirAsync(int seferId, SeferBitirRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/mobile/seferler/{seferId}/bitir", request, _jsonOptions);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SeferDetay>(_jsonOptions);
            }
            
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Sefer bitirilemedi: {Error}", error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sefer bitirilirken hata oluştu");
        }
        return null;
    }

    public async Task<bool> KonumGonderAsync(KonumGonderRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/mobile/konum", request, _jsonOptions);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Konum gönderilirken hata oluştu");
            return false;
        }
    }

    public async Task<bool> ArizaBildirAsync(ArizaBildirimRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/mobile/ariza", request, _jsonOptions);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Arıza bildirimi gönderilirken hata oluştu");
            return false;
        }
    }

    public async Task<bool> MasrafKaydetAsync(MasrafKayitRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/mobile/masraf", request, _jsonOptions);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Masraf kaydedilirken hata oluştu");
            return false;
        }
    }

    public async Task<List<MasrafKalemiOzet>> MasrafKalemleriGetirAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/mobile/masraf-kalemleri");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<MasrafKalemiOzet>>(_jsonOptions) ?? new();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Masraf kalemleri alınırken hata oluştu");
        }
        return new();
    }

    public async Task<List<GuzergahOzet>> GuzergahlariGetirAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/mobile/guzergahlar");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<GuzergahOzet>>(_jsonOptions) ?? new();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Güzergahlar alınırken hata oluştu");
        }
        return new();
    }

    public async Task<List<SeferOzet>> SeferGecmisiniGetirAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/mobile/seferler");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<SeferOzet>>(_jsonOptions) ?? new();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sefer geçmişi alınırken hata oluştu");
        }
        return new();
    }

    public async Task<SeferOzet?> SeferGetirAsync(int seferId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/mobile/seferler/{seferId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SeferOzet>(_jsonOptions);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sefer bilgisi alınırken hata oluştu: {SeferId}", seferId);
        }
        return null;
    }

    public async Task<bool> SeferBitirAsync(object model)
    {
        try
        {
            // Dynamic model'den değerleri al
            var json = JsonSerializer.Serialize(model, _jsonOptions);
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

            if (dict == null || !dict.ContainsKey("seferId")) return false;

            var seferId = dict["seferId"].GetInt32();
            var request = new SeferBitirRequest
            {
                BitisKm = dict.ContainsKey("bitisKm") ? (int)dict["bitisKm"].GetDecimal() : 0,
                BitisEnlem = dict.ContainsKey("bitisEnlem") ? dict["bitisEnlem"].GetDouble() : null,
                BitisBoylam = dict.ContainsKey("bitisBoylam") ? dict["bitisBoylam"].GetDouble() : null,
                Notlar = dict.ContainsKey("notlar") ? dict["notlar"].GetString() : null
            };

            var result = await SeferBitirAsync(seferId, request);
            return result != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sefer bitirme hatası");
            return false;
        }
    }

    public async Task<bool> BaglantiyiTestEtAsync()
    {
        try
        {
            await SunucuAdresiYukleAsync();
            var response = await _httpClient.GetAsync("api/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetSunucuAdresiAsync()
    {
        return await SunucuAdresiYukleAsync();
    }

    public async Task SetSunucuAdresiAsync(string sunucuAdresi)
    {
        var normalized = NormalizeSunucuAdresi(sunucuAdresi);
        _httpClient.BaseAddress = new Uri(normalized);
        await _localStorage.SetItemAsync(SunucuAdresiKey, normalized);
    }

    private async Task<string> SunucuAdresiYukleAsync()
    {
        var kayitliAdres = await _localStorage.GetItemAsync<string>(SunucuAdresiKey);
        var normalized = NormalizeSunucuAdresi(kayitliAdres);

        if (_httpClient.BaseAddress is null || !string.Equals(_httpClient.BaseAddress.AbsoluteUri, normalized, StringComparison.OrdinalIgnoreCase))
        {
            _httpClient.BaseAddress = new Uri(normalized);
        }

        if (string.IsNullOrWhiteSpace(kayitliAdres))
        {
            await _localStorage.SetItemAsync(SunucuAdresiKey, normalized);
        }

        return normalized;
    }

    private static string NormalizeSunucuAdresi(string? sunucuAdresi)
    {
        var value = string.IsNullOrWhiteSpace(sunucuAdresi) ? VarsayilanSunucuAdresi : sunucuAdresi.Trim();

        if (!value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            value = $"http://{value}";
        }

        if (!value.EndsWith('/'))
        {
            value += "/";
        }

        return value;
    }
}
