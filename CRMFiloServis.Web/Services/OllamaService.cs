using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CRMFiloServis.Web.Services;

public interface IOllamaService
{
    Task<string> RaporYorumlaAsync(string prompt);
    Task<string> AnalizYapAsync(string prompt, string sistemPrompt);
    Task<bool> BaglantiKontrolAsync();
    Task<float[]?> EmbeddingOlusturAsync(string metin);
    Task<List<float[]>?> TopluEmbeddingOlusturAsync(IEnumerable<string> metinler);
    string ModelAdi { get; }
    string EmbeddingModelAdi { get; }
}

public class OllamaService : IOllamaService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OllamaService> _logger;
    private readonly string _model;
    private readonly string _embeddingModel;
    private readonly string _baseUrl;

    public string ModelAdi => _model;
    public string EmbeddingModelAdi => _embeddingModel;

    public OllamaService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<OllamaService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("Ollama");
        _configuration = configuration;
        _logger = logger;
        _baseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
        _model = configuration["Ollama:Model"] ?? "llama3.2";
        _embeddingModel = configuration["Ollama:EmbeddingModel"] ?? "nomic-embed-text";
        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.Timeout = TimeSpan.FromMinutes(3);
    }

    public async Task<bool> BaglantiKontrolAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/tags");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogInformation("Ollama bağlantı kontrolü başarısız: {BaseUrl} - {Mesaj}", _baseUrl, ex.Message);
            return false;
        }
    }

    public async Task<string> RaporYorumlaAsync(string prompt)
    {
        try
        {
            var sistemPrompt = @"Sen bir Türk mali müşavir ve bütçe analistisin. 
Kullanıcının bütçe/harcama verilerini analiz edip Türkçe rapor yazıyorsun.
Kısa, öz ve aksiyona yönelik yorumlar yap. Madde işaretleri kullan.
Emoji kullanma. Tutarları TL olarak göster. 
Önerilerin somut ve uygulanabilir olsun.";

            var request = new OllamaGenerateRequest
            {
                Model = _model,
                Prompt = prompt,
                System = sistemPrompt,
                Stream = false,
                Options = new OllamaOptions
                {
                    Temperature = 0.3,
                    TopP = 0.9,
                    NumPredict = 1024
                }
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/generate", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Ollama API hatası: {Status} - {Body}", response.StatusCode, errorBody);
                return $"AI analiz yapılamadı. Ollama yanıt kodu: {response.StatusCode}";
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<OllamaGenerateResponse>(responseBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

                return result?.Response?.Trim() ?? "AI yanıt alınamadı.";
                }
                catch (TaskCanceledException)
                {
                    return "AI analiz zaman aşımına uğradı. Ollama sunucusunun çalıştığından emin olun.";
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Ollama bağlantı hatası");
                    return $"Ollama sunucusuna bağlanılamadı ({_baseUrl}). Lütfen Ollama'nın çalıştığından emin olun.";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ollama rapor yorumlama hatası");
                    return $"AI analiz hatası: {ex.Message}";
                }
            }

            public async Task<string> AnalizYapAsync(string prompt, string sistemPrompt)
            {
                try
                {
                    var request = new OllamaGenerateRequest
                    {
                        Model = _model,
                        Prompt = prompt,
                        System = sistemPrompt,
                        Stream = false,
                        Options = new OllamaOptions
                        {
                            Temperature = 0.3,
                            TopP = 0.9,
                            NumPredict = 2048
                        }
                    };

                    var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    });

                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync("/api/generate", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorBody = await response.Content.ReadAsStringAsync();
                        _logger.LogError("Ollama API hatası: {Status} - {Body}", response.StatusCode, errorBody);
                        return $"AI analiz yapılamadı. Ollama yanıt kodu: {response.StatusCode}";
                    }

                    var responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<OllamaGenerateResponse>(responseBody, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    return result?.Response?.Trim() ?? "AI yanıt alınamadı.";
                }
                catch (TaskCanceledException)
                {
                    return "AI analiz zaman aşımına uğradı. Ollama sunucusunun çalıştığından emin olun.";
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Ollama bağlantı hatası");
                    return $"Ollama sunucusuna bağlanılamadı ({_baseUrl}). Lütfen Ollama'nın çalıştığından emin olun.";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ollama analiz hatası");
                    return $"AI analiz hatası: {ex.Message}";
                }
            }

    /// <summary>
    /// Metin için embedding vektörü oluşturur (Semantic Search için)
    /// </summary>
    public async Task<float[]?> EmbeddingOlusturAsync(string metin)
    {
        if (string.IsNullOrWhiteSpace(metin))
            return null;

        try
        {
            var request = new OllamaEmbeddingRequest
            {
                Model = _embeddingModel,
                Prompt = metin.Trim()
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/embeddings", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Ollama Embedding API hatası: {Status} - {Body}", response.StatusCode, errorBody);
                return null;
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<OllamaEmbeddingResponse>(responseBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return result?.Embedding;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Embedding oluşturma hatası: {Metin}", metin.Length > 100 ? metin[..100] + "..." : metin);
            return null;
        }
    }

    /// <summary>
    /// Birden fazla metin için toplu embedding oluşturur
    /// </summary>
    public async Task<List<float[]>?> TopluEmbeddingOlusturAsync(IEnumerable<string> metinler)
    {
        var sonuclar = new List<float[]>();

        foreach (var metin in metinler)
        {
            var embedding = await EmbeddingOlusturAsync(metin);
            if (embedding != null)
            {
                sonuclar.Add(embedding);
            }
        }

        return sonuclar.Count > 0 ? sonuclar : null;
    }
}

// Ollama API Request/Response modelleri
public class OllamaGenerateRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "";

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = "";

    [JsonPropertyName("system")]
    public string? System { get; set; }

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }

    [JsonPropertyName("options")]
    public OllamaOptions? Options { get; set; }
}

public class OllamaOptions
{
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 0.3;

    [JsonPropertyName("top_p")]
    public double TopP { get; set; } = 0.9;

    [JsonPropertyName("num_predict")]
    public int NumPredict { get; set; } = 1024;
}

public class OllamaGenerateResponse
{
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("response")]
    public string? Response { get; set; }

    [JsonPropertyName("done")]
    public bool Done { get; set; }

    [JsonPropertyName("total_duration")]
    public long? TotalDuration { get; set; }
}

// Ollama Embedding API modelleri
public class OllamaEmbeddingRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "";

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = "";
}

public class OllamaEmbeddingResponse
{
    [JsonPropertyName("embedding")]
    public float[]? Embedding { get; set; }
}
