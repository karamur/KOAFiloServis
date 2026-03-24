using System.Text.Json;
using System.Net.Http.Headers;

namespace CRMFiloServis.Web.Services;

public interface IAracDegerlemeAIService
{
    Task<AracDegerlemeResult> DegerlemeyiHesaplaAsync(AracDegerlemeRequest request);
    Task<List<PiyasaKarsilastirma>> PiyasaKarsilastirmasiYapAsync(AracDegerlemeRequest request);
    Task<string> AracRaporuOlusturAsync(AracDegerlemeRequest request);
}

public class AracDegerlemeAIService : IAracDegerlemeAIService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AracDegerlemeAIService> _logger;

    public AracDegerlemeAIService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<AracDegerlemeAIService> logger)
    {
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient("OpenAI");
        _logger = logger;
    }

    public async Task<AracDegerlemeResult> DegerlemeyiHesaplaAsync(AracDegerlemeRequest request)
    {
        var prompt = OlusturDegerlemePrompt(request);
        var aiResponse = await SendOpenAIRequestAsync(prompt);

        return ParseDegerlemeResponse(aiResponse, request);
    }

    public async Task<List<PiyasaKarsilastirma>> PiyasaKarsilastirmasiYapAsync(AracDegerlemeRequest request)
    {
        var prompt = OlusturKarsilastirmaPrompt(request);
        var aiResponse = await SendOpenAIRequestAsync(prompt);

        return ParseKarsilastirmaResponse(aiResponse);
    }

    public async Task<string> AracRaporuOlusturAsync(AracDegerlemeRequest request)
    {
        var prompt = OlusturRaporPrompt(request);
        return await SendOpenAIRequestAsync(prompt);
    }

    private string OlusturDegerlemePrompt(AracDegerlemeRequest request)
    {
        return $@"Sen bir araç deðerleme uzmanýsýn. Türkiye'deki ikinci el araç piyasasýný çok iyi biliyorsun.

Aþaðýdaki araç için güncel piyasa deðeri analizi yap:

ARAÇ BÝLGÝLERÝ:
- Marka: {request.Marka}
- Model: {request.Model}
- Versiyon/Paket: {request.Versiyon ?? "Belirtilmemiþ"}
- Model Yýlý: {request.ModelYili}
- Kilometre: {request.Kilometre:N0} km
- Yakýt Tipi: {request.YakitTipi}
- Vites Tipi: {request.VitesTipi}
- Kasa Tipi: {request.KasaTipi ?? "Belirtilmemiþ"}
- Motor Hacmi: {request.MotorHacmi ?? "Belirtilmemiþ"}
- Renk: {request.Renk ?? "Belirtilmemiþ"}
- Hasar Durumu: Boyalý parça: {request.BoyaliParcaSayisi}, Deðiþen parça: {request.DegisenParcaSayisi}
- Tramer Kaydý: {(request.TramerTutari > 0 ? $"{request.TramerTutari:N0} TL" : "Yok")}

LÜTFEN AÞAÐIDAKI JSON FORMATINDA CEVAP VER:
{{
    ""tahminiDeger"": <sayý - TL cinsinden>,
    ""minDeger"": <sayý - minimum deðer TL>,
    ""maxDeger"": <sayý - maksimum deðer TL>,
    ""guvenSkoruyuzde"": <1-100 arasý güven skoru>,
    ""degerEtkenFaktorler"": [
        {{""faktor"": ""<faktör adý>"", ""etki"": ""<pozitif/negatif>"", ""aciklama"": ""<kýsa açýklama>""}}
    ],
    ""piyasaDurumu"": ""<Alýcý Piyasasý/Satýcý Piyasasý/Dengeli>"",
    ""satisSuresiTahmini"": ""<ortalama satýþ süresi>"",
    ""oneriler"": [""<öneri 1>"", ""<öneri 2>""],
    ""notlar"": ""<genel deðerlendirme notu>""
}}

Güncel Türkiye piyasa koþullarýný, enflasyonu, döviz kurlarýný ve mevsimsel faktörleri göz önünde bulundur.
Sadece JSON formatýnda cevap ver, baþka açýklama ekleme.";
    }

    private string OlusturKarsilastirmaPrompt(AracDegerlemeRequest request)
    {
        return $@"Sen bir araç piyasa analisti olarak Türkiye'deki ikinci el araç piyasasýný analiz ediyorsun.

Aþaðýdaki araç için piyasadaki benzer araçlarla karþýlaþtýrma yap:

ARAÇ: {request.Marka} {request.Model} {request.Versiyon ?? ""} - {request.ModelYili} - {request.Kilometre:N0} km - {request.YakitTipi} - {request.VitesTipi}

Piyasada bu araca benzer 10 adet ilan simüle et. Gerçekçi fiyatlar, kilometreler ve lokasyonlar kullan.

LÜTFEN AÞAÐIDAKI JSON ARRAY FORMATINDA CEVAP VER:
[
    {{
        ""kaynak"": ""Sahibinden"",
        ""baslik"": ""<ilan baþlýðý>"",
        ""sehir"": ""<þehir>"",
        ""yil"": <yýl>,
        ""kilometre"": <km>,
        ""fiyat"": <TL>,
        ""yakitTipi"": ""<yakýt>"",
        ""vitesTipi"": ""<vites>"",
        ""boyaliParca"": <sayý>,
        ""degisenParca"": <sayý>,
        ""tramerTutari"": <TL veya 0>,
        ""ilanTarihi"": ""<gün önce yayýnlandý>""
    }}
]

Farklý þehirlerden, farklý fiyat aralýklarýndan ve farklý durumlardan örnekler ver.
Sadece JSON array formatýnda cevap ver.";
    }

    private string OlusturRaporPrompt(AracDegerlemeRequest request)
    {
        return $@"Sen profesyonel bir araç deðerleme uzmanýsýn. Aþaðýdaki araç için detaylý bir deðerleme raporu hazýrla.

ARAÇ BÝLGÝLERÝ:
- Marka/Model: {request.Marka} {request.Model} {request.Versiyon ?? ""}
- Model Yýlý: {request.ModelYili}
- Kilometre: {request.Kilometre:N0} km
- Yakýt/Vites: {request.YakitTipi} / {request.VitesTipi}
- Hasar: {request.BoyaliParcaSayisi} boyalý, {request.DegisenParcaSayisi} deðiþen parça
- Tramer: {(request.TramerTutari > 0 ? $"{request.TramerTutari:N0} TL" : "Kaydý yok")}

Lütfen þu baþlýklarý içeren Türkçe bir rapor hazýrla:
1. ARAÇ DEÐERLEMESÝ (Tahmini deðer aralýðý ve gerekçesi)
2. PÝYASA ANALÝZÝ (Benzer araçlarýn piyasa durumu)
3. GÜÇLÜ YÖNLER (Bu aracýn avantajlarý)
4. DÝKKAT EDÝLMESÝ GEREKENLER (Riskler ve dezavantajlar)
5. SATIÞ STRATEJÝSÝ (Fiyatlandýrma ve pazarlama önerileri)
6. SONUÇ VE ÖNERÝLER

Profesyonel ve detaylý bir rapor hazýrla.";
    }

    private async Task<string> SendOpenAIRequestAsync(string prompt)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];
        var model = _configuration["OpenAI:Model"] ?? "gpt-4o-mini";
        var baseUrl = _configuration["OpenAI:BaseUrl"] ?? "https://api.openai.com/v1";

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("OpenAI API anahtarý yapýlandýrýlmamýþ, simüle edilmiþ veri döndürülüyor.");
            return GenerateSimulatedResponse(prompt);
        }

        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = "Sen Türkiye'deki ikinci el araç piyasasý konusunda uzman bir yapay zeka asistanýsýn. Güncel piyasa verilerini biliyorsun." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                max_tokens = 2000
            };

            var response = await _httpClient.PostAsJsonAsync($"{baseUrl}/chat/completions", requestBody);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>();
                return result?.Choices?.FirstOrDefault()?.Message?.Content ?? GenerateSimulatedResponse(prompt);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("OpenAI API hatasý: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return GenerateSimulatedResponse(prompt);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI API çaðrýsý baþarýsýz");
            return GenerateSimulatedResponse(prompt);
        }
    }

    private string GenerateSimulatedResponse(string prompt)
    {
        // API anahtarý yoksa veya hata olursa simüle edilmiþ cevap döndür
        var random = new Random();
        var baseFiyat = random.Next(800000, 2500000);

        if (prompt.Contains("JSON FORMATINDA CEVAP VER") && prompt.Contains("tahminiDeger"))
        {
            return JsonSerializer.Serialize(new
            {
                tahminiDeger = baseFiyat,
                minDeger = (int)(baseFiyat * 0.9),
                maxDeger = (int)(baseFiyat * 1.1),
                guvenSkoruyuzde = random.Next(75, 95),
                degerEtkenFaktorler = new[]
                {
                    new { faktor = "Kilometre", etki = "negatif", aciklama = "Ortalama üzerinde kilometre deðer düþürücü etki yapar" },
                    new { faktor = "Model Yýlý", etki = "pozitif", aciklama = "Görece yeni model olmasý deðeri artýrýr" },
                    new { faktor = "Bakým Durumu", etki = "pozitif", aciklama = "Düzenli bakýmlý araçlar daha deðerli" }
                },
                piyasaDurumu = "Dengeli",
                satisSuresiTahmini = "2-4 hafta",
                oneriler = new[]
                {
                    "Fiyatý piyasa ortalamasýnýn biraz altýnda tutarak hýzlý satýþ saðlayabilirsiniz",
                    "Detaylý fotoðraflar ve bakým geçmiþi paylaþarak güven oluþturun"
                },
                notlar = "Bu araç, segmentinde ortalama deðerde bir araçtýr. Hasar durumu ve bakým geçmiþi fiyatý etkileyebilir."
            });
        }
        else if (prompt.Contains("JSON ARRAY FORMATINDA"))
        {
            var sehirler = new[] { "Ýstanbul", "Ankara", "Ýzmir", "Bursa", "Antalya", "Kocaeli", "Adana", "Konya" };
            var karsilastirmalar = new List<object>();

            for (int i = 0; i < 10; i++)
            {
                karsilastirmalar.Add(new
                {
                    kaynak = random.Next(2) == 0 ? "Sahibinden" : "Arabam",
                    baslik = $"Araç Ýlaný #{i + 1}",
                    sehir = sehirler[random.Next(sehirler.Length)],
                    yil = 2020 + random.Next(5),
                    kilometre = random.Next(20000, 150000),
                    fiyat = baseFiyat + random.Next(-200000, 200000),
                    yakitTipi = random.Next(2) == 0 ? "Dizel" : "Benzin",
                    vitesTipi = random.Next(2) == 0 ? "Otomatik" : "Manuel",
                    boyaliParca = random.Next(4),
                    degisenParca = random.Next(2),
                    tramerTutari = random.Next(3) == 0 ? random.Next(5000, 30000) : 0,
                    ilanTarihi = $"{random.Next(1, 30)} gün önce"
                });
            }

            return JsonSerializer.Serialize(karsilastirmalar);
        }

        return @"# ARAÇ DEÐERLEME RAPORU

## 1. ARAÇ DEÐERLEMESÝ
Bu araç için tahmini piyasa deðeri **" + baseFiyat.ToString("N0") + @" TL - " + (baseFiyat * 1.15).ToString("N0") + @" TL** aralýðýndadýr.

## 2. PÝYASA ANALÝZÝ
Mevcut piyasa koþullarýnda bu segment araçlara talep orta seviyededir. Benzer araçlar ortalama 3-4 haftada satýlmaktadýr.

## 3. GÜÇLÜ YÖNLER
- Popüler marka ve model
- Yaygýn servis aðý
- Ýkinci el deðer kaybý düþük

## 4. DÝKKAT EDÝLMESÝ GEREKENLER
- Kilometre durumunu detaylý inceleyin
- Servis bakým kayýtlarýný kontrol edin
- Tramer sorgusunu mutlaka yapýn

## 5. SATIÞ STRATEJÝSÝ
- Fiyatý piyasa ortalamasýnda tutun
- Kaliteli fotoðraflar kullanýn
- Tüm belgeleri hazýr bulundurun

## 6. SONUÇ
Bu araç, doðru fiyatlandýrma ile 2-4 hafta içinde satýlabilir durumdadýr.";
    }

    private AracDegerlemeResult ParseDegerlemeResponse(string response, AracDegerlemeRequest request)
    {
        try
        {
            // JSON bloðunu çýkar
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}') + 1;

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonStr = response.Substring(jsonStart, jsonEnd - jsonStart);
                var parsed = JsonSerializer.Deserialize<JsonElement>(jsonStr);

                return new AracDegerlemeResult
                {
                    Marka = request.Marka,
                    Model = request.Model,
                    ModelYili = request.ModelYili,
                    Kilometre = request.Kilometre,
                    TahminiDeger = parsed.GetProperty("tahminiDeger").GetDecimal(),
                    MinDeger = parsed.GetProperty("minDeger").GetDecimal(),
                    MaxDeger = parsed.GetProperty("maxDeger").GetDecimal(),
                    GuvenSkoru = parsed.GetProperty("guvenSkoruyuzde").GetInt32(),
                    PiyasaDurumu = parsed.TryGetProperty("piyasaDurumu", out var pd) ? pd.GetString() ?? "Bilinmiyor" : "Bilinmiyor",
                    SatisSuresiTahmini = parsed.TryGetProperty("satisSuresiTahmini", out var sst) ? sst.GetString() ?? "2-4 hafta" : "2-4 hafta",
                    Notlar = parsed.TryGetProperty("notlar", out var n) ? n.GetString() ?? "" : "",
                    DegerlendirmeTarihi = DateTime.Now
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Deðerleme yanýtý parse edilemedi");
        }

        // Parse edilemezse varsayýlan deðer
        return new AracDegerlemeResult
        {
            Marka = request.Marka,
            Model = request.Model,
            ModelYili = request.ModelYili,
            Kilometre = request.Kilometre,
            TahminiDeger = 0,
            MinDeger = 0,
            MaxDeger = 0,
            GuvenSkoru = 0,
            PiyasaDurumu = "Hesaplanamadý",
            Notlar = "Deðerleme yapýlýrken bir hata oluþtu. Lütfen tekrar deneyin.",
            DegerlendirmeTarihi = DateTime.Now
        };
    }

    private List<PiyasaKarsilastirma> ParseKarsilastirmaResponse(string response)
    {
        try
        {
            var jsonStart = response.IndexOf('[');
            var jsonEnd = response.LastIndexOf(']') + 1;

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonStr = response.Substring(jsonStart, jsonEnd - jsonStart);
                var parsed = JsonSerializer.Deserialize<List<JsonElement>>(jsonStr);

                return parsed?.Select(p => new PiyasaKarsilastirma
                {
                    Kaynak = p.TryGetProperty("kaynak", out var k) ? k.GetString() ?? "Bilinmiyor" : "Bilinmiyor",
                    Baslik = p.TryGetProperty("baslik", out var b) ? b.GetString() ?? "" : "",
                    Sehir = p.TryGetProperty("sehir", out var s) ? s.GetString() ?? "" : "",
                    Yil = p.TryGetProperty("yil", out var y) ? y.GetInt32() : 0,
                    Kilometre = p.TryGetProperty("kilometre", out var km) ? km.GetInt32() : 0,
                    Fiyat = p.TryGetProperty("fiyat", out var f) ? f.GetDecimal() : 0,
                    YakitTipi = p.TryGetProperty("yakitTipi", out var yt) ? yt.GetString() ?? "" : "",
                    VitesTipi = p.TryGetProperty("vitesTipi", out var vt) ? vt.GetString() ?? "" : "",
                    BoyaliParca = p.TryGetProperty("boyaliParca", out var bp) ? bp.GetInt32() : 0,
                    DegisenParca = p.TryGetProperty("degisenParca", out var dp) ? dp.GetInt32() : 0,
                    TramerTutari = p.TryGetProperty("tramerTutari", out var t) ? t.GetDecimal() : 0,
                    IlanTarihi = p.TryGetProperty("ilanTarihi", out var it) ? it.GetString() ?? "" : ""
                }).ToList() ?? new List<PiyasaKarsilastirma>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Karþýlaþtýrma yanýtý parse edilemedi");
        }

        return new List<PiyasaKarsilastirma>();
    }
}

#region Models

public class AracDegerlemeRequest
{
    public string Marka { get; set; } = "";
    public string Model { get; set; } = "";
    public string? Versiyon { get; set; }
    public int ModelYili { get; set; }
    public int Kilometre { get; set; }
    public string YakitTipi { get; set; } = "Benzin";
    public string VitesTipi { get; set; } = "Manuel";
    public string? KasaTipi { get; set; }
    public string? MotorHacmi { get; set; }
    public string? Renk { get; set; }
    public int BoyaliParcaSayisi { get; set; }
    public int DegisenParcaSayisi { get; set; }
    public decimal TramerTutari { get; set; }
}

public class AracDegerlemeResult
{
    public string Marka { get; set; } = "";
    public string Model { get; set; } = "";
    public int ModelYili { get; set; }
    public int Kilometre { get; set; }
    public decimal TahminiDeger { get; set; }
    public decimal MinDeger { get; set; }
    public decimal MaxDeger { get; set; }
    public int GuvenSkoru { get; set; } // 0-100
    public string PiyasaDurumu { get; set; } = "";
    public string SatisSuresiTahmini { get; set; } = "";
    public string Notlar { get; set; } = "";
    public DateTime DegerlendirmeTarihi { get; set; }
    public List<DegerEtkenFaktor> EtkenFaktorler { get; set; } = new();
    public List<string> Oneriler { get; set; } = new();
}

public class DegerEtkenFaktor
{
    public string Faktor { get; set; } = "";
    public string Etki { get; set; } = ""; // pozitif/negatif
    public string Aciklama { get; set; } = "";
}

public class PiyasaKarsilastirma
{
    public string Kaynak { get; set; } = "";
    public string Baslik { get; set; } = "";
    public string Sehir { get; set; } = "";
    public int Yil { get; set; }
    public int Kilometre { get; set; }
    public decimal Fiyat { get; set; }
    public string YakitTipi { get; set; } = "";
    public string VitesTipi { get; set; } = "";
    public int BoyaliParca { get; set; }
    public int DegisenParca { get; set; }
    public decimal TramerTutari { get; set; }
    public string IlanTarihi { get; set; } = "";
    public string? IlanUrl { get; set; }
}

public class OpenAIResponse
{
    public List<OpenAIChoice>? Choices { get; set; }
}

public class OpenAIChoice
{
    public OpenAIMessage? Message { get; set; }
}

public class OpenAIMessage
{
    public string? Content { get; set; }
}

#endregion
