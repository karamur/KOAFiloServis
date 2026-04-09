using System.Collections;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.AI;
using OllamaSharp;

namespace CRMFiloServis.Web.Services;

/// <summary>
/// Ollama ile interaktif chat servisi - Microsoft.Extensions.AI ve OllamaSharp kullanarak
/// </summary>
public interface IOllamaAIChatService
{
    Task<string> ChatAsync(string message, CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> ChatStreamAsync(string message, CancellationToken cancellationToken = default);
    Task<string> ChatWithHistoryAsync(List<ChatMessage> history, string message, CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> ChatWithHistoryStreamAsync(List<ChatMessage> history, string message, CancellationToken cancellationToken = default);
    Task<bool> IsAvailableAsync();
    Task<List<string>> GetAvailableModelsAsync();
    void ClearHistory();
    string CurrentModel { get; }
    void SetModel(string modelName);
}

public class OllamaAIChatService : IOllamaAIChatService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OllamaAIChatService> _logger;
    private readonly List<ChatMessage> _chatHistory = new();
    private string _currentModel;
    private readonly string _baseUrl;

    public OllamaAIChatService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<OllamaAIChatService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("Ollama");
        _configuration = configuration;
        _logger = logger;
        _baseUrl = (_configuration["Ollama:BaseUrl"] ?? "http://localhost:11434").TrimEnd('/');
        _currentModel = _configuration["Ollama:Model"] ?? "llama3.2";
        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.Timeout = TimeSpan.FromMinutes(5);
    }

    public string CurrentModel => _currentModel;

    public void SetModel(string modelName)
    {
        _currentModel = modelName;
        _logger.LogInformation("Ollama modeli değiştirildi: {Model}", modelName);
    }

    public void ClearHistory()
    {
        _chatHistory.Clear();
        _logger.LogInformation("Chat geçmişi temizlendi");
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var response = await _httpClient.GetAsync("/api/tags", cts.Token);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ollama bağlantı kontrolü başarısız");
            return false;
        }
    }

    public async Task<List<string>> GetAvailableModelsAsync()
    {
        try
        {
            var ollamaClient = new OllamaApiClient(new Uri(_baseUrl));
            var models = await ollamaClient.ListLocalModelsAsync();
            return models.Select(m => m.Name).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ollama model listesi alınamadı");
            return new List<string>();
        }
    }

    public async Task<string> ChatAsync(string message, CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureValidModelAsync(cancellationToken);

            _chatHistory.Add(new ChatMessage(ChatRole.User, message));
            var assistantMessage = await GenerateAsync(BuildPrompt(_chatHistory), cancellationToken);

            _chatHistory.Add(new ChatMessage(ChatRole.Assistant, assistantMessage));

            return assistantMessage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ollama chat hatası");
            throw new InvalidOperationException($"Ollama ile iletişim kurulamadı: {ex.Message}", ex);
        }
    }

    public async IAsyncEnumerable<string> ChatStreamAsync(
        string message,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await EnsureValidModelAsync(cancellationToken);

        _chatHistory.Add(new ChatMessage(ChatRole.User, message));
        var fullResponse = new StringBuilder();

        await foreach (var chunk in GenerateStreamAsync(BuildPrompt(_chatHistory), cancellationToken))
        {
            if (!string.IsNullOrEmpty(chunk))
            {
                fullResponse.Append(chunk);
                yield return chunk;
            }
        }

        _chatHistory.Add(new ChatMessage(ChatRole.Assistant, fullResponse.ToString()));
    }

    public async Task<string> ChatWithHistoryAsync(
        List<ChatMessage> history,
        string message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureValidModelAsync(cancellationToken);
            var messages = new List<ChatMessage>(history)
            {
                new ChatMessage(ChatRole.User, message)
            };

            return await GenerateAsync(BuildPrompt(messages), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ollama chat with history hatası");
            throw new InvalidOperationException($"Ollama ile iletişim kurulamadı: {ex.Message}", ex);
        }
    }

    public async IAsyncEnumerable<string> ChatWithHistoryStreamAsync(
        List<ChatMessage> history,
        string message,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await EnsureValidModelAsync(cancellationToken);

        var messages = new List<ChatMessage>(history)
        {
            new ChatMessage(ChatRole.User, message)
        };

        await foreach (var chunk in GenerateStreamAsync(BuildPrompt(messages), cancellationToken))
        {
            if (!string.IsNullOrEmpty(chunk))
            {
                yield return chunk;
            }
        }
    }

    private async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken)
    {
        using var response = await SendGenerateRequestAsync(prompt, false, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(CreateOllamaErrorMessage(response.StatusCode, responseBody));
        }

        var result = JsonSerializer.Deserialize<OllamaGenerateResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return result?.Response?.Trim() ?? string.Empty;
    }

    private async IAsyncEnumerable<string> GenerateStreamAsync(
        string prompt,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var response = await SendGenerateRequestAsync(prompt, true, cancellationToken, HttpCompletionOption.ResponseHeadersRead);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(CreateOllamaErrorMessage(response.StatusCode, responseBody));
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            using var document = JsonDocument.Parse(line);

            if (document.RootElement.TryGetProperty("error", out var errorElement))
            {
                throw new InvalidOperationException(errorElement.GetString() ?? "Ollama akış hatası.");
            }

            if (document.RootElement.TryGetProperty("response", out var responseElement))
            {
                var chunk = responseElement.GetString();
                if (!string.IsNullOrEmpty(chunk))
                {
                    yield return chunk;
                }
            }

            if (document.RootElement.TryGetProperty("done", out var doneElement) && doneElement.GetBoolean())
            {
                yield break;
            }
        }
    }

    private async Task<HttpResponseMessage> SendGenerateRequestAsync(
        string prompt,
        bool stream,
        CancellationToken cancellationToken,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/generate")
        {
            Content = new StringContent(JsonSerializer.Serialize(new OllamaGenerateRequest
            {
                Model = _currentModel,
                Prompt = prompt,
                Stream = stream,
                Options = new OllamaOptions
                {
                    Temperature = 0.3,
                    TopP = 0.9,
                    NumPredict = 2048
                }
            }), Encoding.UTF8, "application/json")
        };

        return await _httpClient.SendAsync(request, completionOption, cancellationToken);
    }

    private string CreateOllamaErrorMessage(HttpStatusCode statusCode, string responseBody)
    {
        if (statusCode == HttpStatusCode.NotFound)
        {
            return $"Ollama 404 hatası verdi. Model bulunamadı veya eski chat endpoint uyumsuzluğu oluştu. Aktif model: {_currentModel}. Detay: {responseBody}";
        }

        return $"Ollama hatası ({(int)statusCode}): {responseBody}";
    }

    private static string BuildPrompt(IEnumerable<ChatMessage> messages)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Sen Koa Filo Servis uygulaması için çalışan Türkçe bir AI asistansın.");
        builder.AppendLine("Kısa, net ve yardımcı cevap ver.");
        builder.AppendLine();

        foreach (var message in messages)
        {
            var content = GetMessageText(message);
            if (string.IsNullOrWhiteSpace(content))
            {
                continue;
            }

            builder.AppendLine($"{GetRoleText(message)}: {content}");
        }

        builder.Append("assistant: ");
        return builder.ToString();
    }

    private static string GetRoleText(ChatMessage message)
    {
        return typeof(ChatMessage).GetProperty("Role")?.GetValue(message)?.ToString()?.ToLowerInvariant() ?? "user";
    }

    private static string GetMessageText(ChatMessage message)
    {
        var textProperty = typeof(ChatMessage).GetProperty("Text");
        if (textProperty?.GetValue(message) is string text && !string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        var contentsProperty = typeof(ChatMessage).GetProperty("Contents");
        if (contentsProperty?.GetValue(message) is IEnumerable contents)
        {
            var parts = new List<string>();

            foreach (var item in contents)
            {
                if (item is null)
                {
                    continue;
                }

                var itemText = item.GetType().GetProperty("Text")?.GetValue(item) as string;
                if (!string.IsNullOrWhiteSpace(itemText))
                {
                    parts.Add(itemText);
                    continue;
                }

                var raw = item.ToString();
                if (!string.IsNullOrWhiteSpace(raw))
                {
                    parts.Add(raw);
                }
            }

            if (parts.Count > 0)
            {
                return string.Join(Environment.NewLine, parts);
            }
        }

        return message.ToString() ?? string.Empty;
    }

    private async Task EnsureValidModelAsync(CancellationToken cancellationToken = default)
    {
        var models = await GetAvailableModelsAsync();

        if (!models.Any())
        {
            throw new InvalidOperationException("Ollama çalışıyor ancak yüklü model bulunamadı. Örn: `ollama pull llama3.2` komutunu çalıştırın.");
        }

        if (models.Contains(_currentModel, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        var eskiModel = _currentModel;
        _currentModel = models.First();
        _logger.LogWarning("Seçili Ollama modeli bulunamadı. Model otomatik değiştirildi. Eski: {EskiModel}, Yeni: {YeniModel}", eskiModel, _currentModel);
    }
}

// Ollama yapılandırma sınıfı
public class OllamaSettings
{
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string Model { get; set; } = "llama3.2";
}
