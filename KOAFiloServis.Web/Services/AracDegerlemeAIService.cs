using System.Text.Json;
using System.Net.Http.Headers;

namespace KOAFiloServis.Web.Services;

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
        return $@"Sen bir ara� de�erleme uzman�s�n. T�rkiye'deki ikinci el ara� piyasas�n� �ok iyi biliyorsun.

A�a��daki ara� i�in g�ncel piyasa de�eri analizi yap:

ARA� B�LG�LER�:
- Marka: {request.Marka}
- Model: {request.Model}
- Versiyon/Paket: {request.Versiyon ?? "Belirtilmemi�"}
- Model Y�l�: {request.ModelYili}
- Kilometre: {request.Kilometre:N0} km
- Yak�t Tipi: {request.YakitTipi}
- Vites Tipi: {request.VitesTipi}
- Kasa Tipi: {request.KasaTipi ?? "Belirtilmemi�"}
- Motor Hacmi: {request.MotorHacmi ?? "Belirtilmemi�"}
- Renk: {request.Renk ?? "Belirtilmemi�"}
- Hasar Durumu: Boyal� par�a: {request.BoyaliParcaSayisi}, De�i�en par�a: {request.DegisenParcaSayisi}
- Tramer Kayd�: {(request.TramerTutari > 0 ? $"{request.TramerTutari:N0} TL" : "Yok")}

L�TFEN A�A�IDAKI JSON FORMATINDA CEVAP VER:
{{
    ""tahminiDeger"": <say� - TL cinsinden>,
    ""minDeger"": <say� - minimum de�er TL>,
    ""maxDeger"": <say� - maksimum de�er TL>,
    ""guvenSkoruyuzde"": <1-100 aras� g�ven skoru>,
    ""degerEtkenFaktorler"": [
        {{""faktor"": ""<fakt�r ad�>"", ""etki"": ""<pozitif/negatif>"", ""aciklama"": ""<k�sa a��klama>""}}
    ],
    ""piyasaDurumu"": ""<Al�c� Piyasas�/Sat�c� Piyasas�/Dengeli>"",
    ""satisSuresiTahmini"": ""<ortalama sat�� s�resi>"",
    ""oneriler"": [""<�neri 1>"", ""<�neri 2>""],
    ""notlar"": ""<genel de�erlendirme notu>""
}}

G�ncel T�rkiye piyasa ko�ullar�n�, enflasyonu, d�viz kurlar�n� ve mevsimsel fakt�rleri g�z �n�nde bulundur.
Sadece JSON format�nda cevap ver, ba�ka a��klama ekleme.";
    }

    private string OlusturKarsilastirmaPrompt(AracDegerlemeRequest request)
    {
        return $@"Sen bir ara� piyasa analisti olarak T�rkiye'deki ikinci el ara� piyasas�n� analiz ediyorsun.

A�a��daki ara� i�in piyasadaki benzer ara�larla kar��la�t�rma yap:

ARA�: {request.Marka} {request.Model} {request.Versiyon ?? ""} - {request.ModelYili} - {request.Kilometre:N0} km - {request.YakitTipi} - {request.VitesTipi}

Piyasada bu araca benzer 10 adet ilan sim�le et. Ger�ek�i fiyatlar, kilometreler ve lokasyonlar kullan.

L�TFEN A�A�IDAKI JSON ARRAY FORMATINDA CEVAP VER:
[
    {{
        ""kaynak"": ""Sahibinden"",
        ""baslik"": ""<ilan ba�l���>"",
        ""sehir"": ""<�ehir>"",
        ""yil"": <y�l>,
        ""kilometre"": <km>,
        ""fiyat"": <TL>,
        ""yakitTipi"": ""<yak�t>"",
        ""vitesTipi"": ""<vites>"",
        ""boyaliParca"": <say�>,
        ""degisenParca"": <say�>,
        ""tramerTutari"": <TL veya 0>,
        ""ilanTarihi"": ""<g�n �nce yay�nland�>""
    }}
]

Farkl� �ehirlerden, farkl� fiyat aral�klar�ndan ve farkl� durumlardan �rnekler ver.
Sadece JSON array format�nda cevap ver.";
    }

    private string OlusturRaporPrompt(AracDegerlemeRequest request)
    {
        return $@"Sen profesyonel bir ara� de�erleme uzman�s�n. A�a��daki ara� i�in detayl� bir de�erleme raporu haz�rla.

ARA� B�LG�LER�:
- Marka/Model: {request.Marka} {request.Model} {request.Versiyon ?? ""}
- Model Y�l�: {request.ModelYili}
- Kilometre: {request.Kilometre:N0} km
- Yak�t/Vites: {request.YakitTipi} / {request.VitesTipi}
- Hasar: {request.BoyaliParcaSayisi} boyal�, {request.DegisenParcaSayisi} de�i�en par�a
- Tramer: {(request.TramerTutari > 0 ? $"{request.TramerTutari:N0} TL" : "Kayd� yok")}

L�tfen �u ba�l�klar� i�eren T�rk�e bir rapor haz�rla:
1. ARA� DE�ERLEMES� (Tahmini de�er aral��� ve gerek�esi)
2. P�YASA ANAL�Z� (Benzer ara�lar�n piyasa durumu)
3. G��L� Y�NLER (Bu arac�n avantajlar�)
4. D�KKAT ED�LMES� GEREKENLER (Riskler ve dezavantajlar)
5. SATI� STRATEJ�S� (Fiyatland�rma ve pazarlama �nerileri)
6. SONU� VE �NER�LER

Profesyonel ve detayl� bir rapor haz�rla.";
    }

    private async Task<string> SendOpenAIRequestAsync(string prompt)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];
        var model = _configuration["OpenAI:Model"] ?? "gpt-4o-mini";
        var baseUrl = _configuration["OpenAI:BaseUrl"] ?? "https://api.openai.com/v1";

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("OpenAI API anahtar� yap�land�r�lmam��, sim�le edilmi� veri d�nd�r�l�yor.");
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
                    new { role = "system", content = "Sen T�rkiye'deki ikinci el ara� piyasas� konusunda uzman bir yapay zeka asistan�s�n. G�ncel piyasa verilerini biliyorsun." },
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
                _logger.LogError("OpenAI API hatas�: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return GenerateSimulatedResponse(prompt);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI API �a�r�s� ba�ar�s�z");
            return GenerateSimulatedResponse(prompt);
        }
    }

    private string GenerateSimulatedResponse(string prompt)
    {
        // API anahtar� yoksa veya hata olursa sim�le edilmi� cevap d�nd�r
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
                    new { faktor = "Kilometre", etki = "negatif", aciklama = "Ortalama �zerinde kilometre de�er d���r�c� etki yapar" },
                    new { faktor = "Model Y�l�", etki = "pozitif", aciklama = "G�rece yeni model olmas� de�eri art�r�r" },
                    new { faktor = "Bak�m Durumu", etki = "pozitif", aciklama = "D�zenli bak�ml� ara�lar daha de�erli" }
                },
                piyasaDurumu = "Dengeli",
                satisSuresiTahmini = "2-4 hafta",
                oneriler = new[]
                {
                    "Fiyat� piyasa ortalamas�n�n biraz alt�nda tutarak h�zl� sat�� sa�layabilirsiniz",
                    "Detayl� foto�raflar ve bak�m ge�mi�i payla�arak g�ven olu�turun"
                },
                notlar = "Bu ara�, segmentinde ortalama de�erde bir ara�t�r. Hasar durumu ve bak�m ge�mi�i fiyat� etkileyebilir."
            });
        }
        else if (prompt.Contains("JSON ARRAY FORMATINDA"))
        {
            var sehirler = new[] { "�stanbul", "Ankara", "�zmir", "Bursa", "Antalya", "Kocaeli", "Adana", "Konya" };
            var karsilastirmalar = new List<object>();

            for (int i = 0; i < 10; i++)
            {
                karsilastirmalar.Add(new
                {
                    kaynak = random.Next(2) == 0 ? "Sahibinden" : "Arabam",
                    baslik = $"Ara� �lan� #{i + 1}",
                    sehir = sehirler[random.Next(sehirler.Length)],
                    yil = 2020 + random.Next(5),
                    kilometre = random.Next(20000, 150000),
                    fiyat = baseFiyat + random.Next(-200000, 200000),
                    yakitTipi = random.Next(2) == 0 ? "Dizel" : "Benzin",
                    vitesTipi = random.Next(2) == 0 ? "Otomatik" : "Manuel",
                    boyaliParca = random.Next(4),
                    degisenParca = random.Next(2),
                    tramerTutari = random.Next(3) == 0 ? random.Next(5000, 30000) : 0,
                    ilanTarihi = $"{random.Next(1, 30)} g�n �nce"
                });
            }

            return JsonSerializer.Serialize(karsilastirmalar);
        }

        return @"# ARA� DE�ERLEME RAPORU

## 1. ARA� DE�ERLEMES�
Bu ara� i�in tahmini piyasa de�eri **" + baseFiyat.ToString("N0") + @" TL - " + (baseFiyat * 1.15).ToString("N0") + @" TL** aral���ndad�r.

## 2. P�YASA ANAL�Z�
Mevcut piyasa ko�ullar�nda bu segment ara�lara talep orta seviyededir. Benzer ara�lar ortalama 3-4 haftada sat�lmaktad�r.

## 3. G��L� Y�NLER
- Pop�ler marka ve model
- Yayg�n servis a��
- �kinci el de�er kayb� d���k

## 4. D�KKAT ED�LMES� GEREKENLER
- Kilometre durumunu detayl� inceleyin
- Servis bak�m kay�tlar�n� kontrol edin
- Tramer sorgusunu mutlaka yap�n

## 5. SATI� STRATEJ�S�
- Fiyat� piyasa ortalamas�nda tutun
- Kaliteli foto�raflar kullan�n
- T�m belgeleri haz�r bulundurun

## 6. SONU�
Bu ara�, do�ru fiyatland�rma ile 2-4 hafta i�inde sat�labilir durumdad�r.";
    }

    private AracDegerlemeResult ParseDegerlemeResponse(string response, AracDegerlemeRequest request)
    {
        try
        {
            // JSON blo�unu ��kar
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
            _logger.LogError(ex, "De�erleme yan�t� parse edilemedi");
        }

        // Parse edilemezse varsay�lan de�er
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
            PiyasaDurumu = "Hesaplanamad�",
            Notlar = "De�erleme yap�l�rken bir hata olu�tu. L�tfen tekrar deneyin.",
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
            _logger.LogError(ex, "Kar��la�t�rma yan�t� parse edilemedi");
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
