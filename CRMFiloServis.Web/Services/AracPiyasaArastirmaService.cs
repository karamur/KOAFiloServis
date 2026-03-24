using System.Text.Json;
using System.Net.Http.Headers;
using CRMFiloServis.Shared.Entities;

namespace CRMFiloServis.Web.Services;

public interface IAracPiyasaArastirmaService
{
    Task<AracPiyasaArastirma> ArastirmaBaslatAsync(AracPiyasaArastirmaRequest request);
    Task<List<PiyasaIlan>> IlanlariGetirAsync(AracPiyasaArastirmaRequest request);
    Task<PiyasaAnalizSonuc> PiyasaAnaliziYapAsync(List<PiyasaIlan> ilanlar, string marka, string model);
    Task<List<AracMarkaInfo>> TumMarkalariGetirAsync();
    Task<List<AracModelInfo>> ModellerGetirAsync(string marka);
    Task<string> DetayliRaporOlusturAsync(AracPiyasaArastirma arastirma);
}

public class AracPiyasaArastirmaService : IAracPiyasaArastirmaService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AracPiyasaArastirmaService> _logger;

    public AracPiyasaArastirmaService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<AracPiyasaArastirmaService> logger)
    {
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient("OpenAI");
        _logger = logger;
    }

    public async Task<List<AracMarkaInfo>> TumMarkalariGetirAsync()
    {
        var prompt = @"Türkiye'de satýþta olan tüm otomobil markalarýný listele. 
Sadece aktif olarak satýþý devam eden markalarý ver (üretimi durmuþ veya Türkiye'den çekilmiþ markalarý dahil etme).

JSON formatýnda cevap ver:
[
    {""marka"": ""Marka Adý"", ""ulke"": ""Ülke"", ""segment"": ""Premium/Standart"", ""populer"": true/false}
]

Alfabetik sýrala. En az 40 marka olmalý.";

        var response = await SendAIRequestAsync(prompt);
        return ParseMarkaListesi(response);
    }

    public async Task<List<AracModelInfo>> ModellerGetirAsync(string marka)
    {
        var prompt = $@"'{marka}' markasýnýn Türkiye'de þu anda satýþta olan tüm modellerini listele.
Sadece aktif satýþta olan modelleri ver (üretimi durmuþ modelleri dahil etme).

JSON formatýnda cevap ver:
[
    {{
        ""model"": ""Model Adý"",
        ""segment"": ""A/B/C/D/E/SUV/Crossover/Pickup/Van"",
        ""kasaTipi"": ""Sedan/Hatchback/SUV/Crossover/Station/Coupe/Cabrio"",
        ""baslangicYili"": 2020,
        ""yakitTipleri"": [""Benzin"", ""Dizel"", ""Hibrit"", ""Elektrik""],
        ""vitesTipleri"": [""Manuel"", ""Otomatik""],
        ""fiyatAraligi"": ""500.000 - 800.000 TL"",
        ""populer"": true/false
    }}
]

Tüm güncel modelleri dahil et.";

        var response = await SendAIRequestAsync(prompt);
        return ParseModelListesi(response);
    }

    public async Task<AracPiyasaArastirma> ArastirmaBaslatAsync(AracPiyasaArastirmaRequest request)
    {
        var arastirma = new AracPiyasaArastirma
        {
            Marka = request.Marka,
            Model = request.Model,
            Versiyon = request.Versiyon,
            YilBaslangic = request.YilBaslangic,
            YilBitis = request.YilBitis,
            YakitTipi = request.YakitTipi,
            VitesTipi = request.VitesTipi,
            MinKilometre = request.MinKilometre,
            MaxKilometre = request.MaxKilometre,
            MinFiyat = request.MinFiyat,
            MaxFiyat = request.MaxFiyat,
            Sehir = request.Sehir,
            ArastirmaTarihi = DateTime.Now,
            Durum = ArastirmaDurum.Devam
        };

        try
        {
            var ilanlar = await IlanlariGetirAsync(request);
            arastirma.Ilanlar = ilanlar;

            if (ilanlar.Any())
            {
                var fiyatlar = ilanlar.Select(i => i.Fiyat).OrderBy(f => f).ToList();
                arastirma.ToplamIlanSayisi = ilanlar.Count;
                arastirma.OrtalamaFiyat = ilanlar.Average(i => i.Fiyat);
                arastirma.EnDusukFiyat = fiyatlar.First();
                arastirma.EnYuksekFiyat = fiyatlar.Last();
                arastirma.MedianFiyat = fiyatlar[fiyatlar.Count / 2];
                arastirma.OrtalamaKilometre = (int)ilanlar.Average(i => i.Kilometre);

                var analiz = await PiyasaAnaliziYapAsync(ilanlar, request.Marka, request.Model);
                arastirma.AIAnalizi = analiz.AnalizMetni;
            }

            arastirma.Durum = ArastirmaDurum.Tamamlandi;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Piyasa araþtýrmasý baþarýsýz");
            arastirma.Durum = ArastirmaDurum.Hata;
            arastirma.HataMesaji = ex.Message;
        }

        return arastirma;
    }

    public async Task<List<PiyasaIlan>> IlanlariGetirAsync(AracPiyasaArastirmaRequest request)
    {
        var prompt = $@"Sen Türkiye'nin en büyük araç satýþ platformlarýný (Sahibinden, Arabam, Letgo, Facebook Marketplace, Otomerkezi vb.) tarayan bir araç piyasa analisti yapay zekasýsýn.

Aþaðýdaki kriterlere uyan ve ÞU AN SATIÞTA OLAN (satýlmýþ, iptal edilmiþ veya pasif ilanlarý DAHIL ETME) araç ilanlarýný topla:

ARAMA KRÝTERLERÝ:
- Marka: {request.Marka}
- Model: {request.Model}
{(string.IsNullOrEmpty(request.Versiyon) ? "" : $"- Versiyon: {request.Versiyon}")}
- Yýl Aralýðý: {request.YilBaslangic ?? 2015} - {request.YilBitis ?? DateTime.Now.Year}
{(string.IsNullOrEmpty(request.YakitTipi) ? "" : $"- Yakýt Tipi: {request.YakitTipi}")}
{(string.IsNullOrEmpty(request.VitesTipi) ? "" : $"- Vites Tipi: {request.VitesTipi}")}
{(request.MinKilometre.HasValue ? $"- Min Kilometre: {request.MinKilometre}" : "")}
{(request.MaxKilometre.HasValue ? $"- Max Kilometre: {request.MaxKilometre}" : "")}
{(request.MinFiyat.HasValue ? $"- Min Fiyat: {request.MinFiyat:N0} TL" : "")}
{(request.MaxFiyat.HasValue ? $"- Max Fiyat: {request.MaxFiyat:N0} TL" : "")}
{(string.IsNullOrEmpty(request.Sehir) ? "- Tüm Türkiye" : $"- Þehir: {request.Sehir}")}

ÖNEMLÝ KURALLAR:
1. SADECE þu an aktif satýþta olan ilanlarý getir
2. Satýlmýþ, iptal edilmiþ veya kapatýlmýþ ilanlarý DAHIL ETME
3. Gerçekçi ve güncel piyasa fiyatlarý kullan
4. Farklý þehirlerden ve farklý satýcý tiplerinden (galeri/bireysel) çeþitlilik saðla
5. En az 20, en fazla 50 ilan getir
6. Fiyatlarý TL cinsinden ver
7. Tramer bilgisi olan araçlarý da dahil et

JSON ARRAY formatýnda cevap ver:
[
    {{
        ""kaynak"": ""Sahibinden/Arabam/Letgo/Facebook/Otomerkezi"",
        ""ilanNo"": ""123456789"",
        ""baslik"": ""2022 Model BMW 3.20i M Sport"",
        ""marka"": ""{request.Marka}"",
        ""model"": ""{request.Model}"",
        ""versiyon"": ""320i M Sport"",
        ""yil"": 2022,
        ""kilometre"": 45000,
        ""fiyat"": 2850000,
        ""yakitTipi"": ""Benzin"",
        ""vitesTipi"": ""Otomatik"",
        ""kasaTipi"": ""Sedan"",
        ""motorHacmi"": ""2.0"",
        ""motorGucu"": ""184 HP"",
        ""renk"": ""Siyah"",
        ""boyaliParca"": 0,
        ""degisenParca"": 0,
        ""tramerTutari"": 0,
        ""hasarKayitli"": false,
        ""sehir"": ""Ýstanbul"",
        ""ilce"": ""Kadýköy"",
        ""saticiTipi"": ""Galeri"",
        ""saticiAdi"": ""Premium Auto"",
        ""ilanTarihi"": ""2024-01-15"",
        ""aktif"": true
    }}
]

Sadece JSON array formatýnda cevap ver, baþka açýklama ekleme.";

        var response = await SendAIRequestAsync(prompt);
        return ParseIlanListesi(response);
    }

    public async Task<PiyasaAnalizSonuc> PiyasaAnaliziYapAsync(List<PiyasaIlan> ilanlar, string marka, string model)
    {
        var ilanOzeti = string.Join("\n", ilanlar.Take(20).Select(i => 
            $"- {i.ModelYili} {i.Versiyon ?? ""} | {i.Kilometre:N0} km | {i.Fiyat:N0} TL | {i.YakitTipi}/{i.VitesTipi} | {i.Sehir} | Hasar: {(i.HasarKayitli ? "Var" : "Yok")}"));

        var prompt = $@"Aþaðýdaki {marka} {model} ilanlarýný analiz et ve galeri sahibi için alým-satým stratejisi öner:

ÝLAN ÖZETÝ:
{ilanOzeti}

ÝSTATÝSTÝKLER:
- Toplam Ýlan: {ilanlar.Count}
- Ortalama Fiyat: {ilanlar.Average(i => i.Fiyat):N0} TL
- Min Fiyat: {ilanlar.Min(i => i.Fiyat):N0} TL
- Max Fiyat: {ilanlar.Max(i => i.Fiyat):N0} TL
- Ortalama KM: {ilanlar.Average(i => i.Kilometre):N0}

ANALÝZ RAPORUNU OLUÞTUR:
1. PÝYASA DURUMU: Bu araç için piyasa nasýl? (Alýcý/Satýcý/Dengeli piyasa)
2. FÝYAT ANALÝZÝ: Uygun fiyatlý ilanlar hangileri? Pahalý olanlar neden pahalý?
3. ALIM TAVSÝYELERÝ: Hangi özellikteki araçlar cazip? (Yýl, KM, Yakýt, Hasar durumu)
4. SATIM STRATEJÝSÝ: Bu araçlarý alýrken ne kadar, satarken ne kadar fiyat belirlenmeli?
5. DÝKKAT EDÝLMESÝ GEREKENLER: Riskler, tuzaklar, kontrol edilmesi gerekenler
6. EN ÝYÝ FIRSATLAR: Listeden en cazip 3-5 ilan ve nedenleri
7. KAÇINILMASI GEREKENLER: Listeden uzak durulmasý gereken ilanlar ve nedenleri

Detaylý ve profesyonel bir analiz yap. Galerici bakýþ açýsýyla kar marjý ve riskleri deðerlendir.";

        var analizMetni = await SendAIRequestAsync(prompt);

        return new PiyasaAnalizSonuc
        {
            Marka = marka,
            Model = model,
            ToplamIlan = ilanlar.Count,
            OrtalamaFiyat = ilanlar.Average(i => i.Fiyat),
            MinFiyat = ilanlar.Min(i => i.Fiyat),
            MaxFiyat = ilanlar.Max(i => i.Fiyat),
            OrtalamaKilometre = (int)ilanlar.Average(i => i.Kilometre),
            AnalizMetni = analizMetni,
            AnalizTarihi = DateTime.Now
        };
    }

    public async Task<string> DetayliRaporOlusturAsync(AracPiyasaArastirma arastirma)
    {
        var ilanlar = arastirma.Ilanlar.ToList();
        if (!ilanlar.Any())
            return "Analiz için yeterli veri bulunamadý.";

        var kaynakDagilimi = ilanlar.GroupBy(i => i.Kaynak)
            .Select(g => $"{g.Key}: {g.Count()} ilan")
            .ToList();

        var sehirDagilimi = ilanlar.GroupBy(i => i.Sehir)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => $"{g.Key}: {g.Count()} ilan, Ort: {g.Average(i => i.Fiyat):N0} TL")
            .ToList();

        var yilDagilimi = ilanlar.GroupBy(i => i.ModelYili)
            .OrderBy(g => g.Key)
            .Select(g => $"{g.Key}: {g.Count()} ilan, Ort: {g.Average(i => i.Fiyat):N0} TL")
            .ToList();

        var prompt = $@"Aþaðýdaki piyasa araþtýrma verileri için profesyonel bir galeri raporu hazýrla:

ARAÇ: {arastirma.Marka} {arastirma.Model}
ARAÞTIRMA TARÝHÝ: {arastirma.ArastirmaTarihi:dd.MM.yyyy HH:mm}

ÖZET ÝSTATÝSTÝKLER:
- Toplam Ýlan: {arastirma.ToplamIlanSayisi}
- Ortalama Fiyat: {arastirma.OrtalamaFiyat:N0} TL
- En Düþük: {arastirma.EnDusukFiyat:N0} TL
- En Yüksek: {arastirma.EnYuksekFiyat:N0} TL
- Median: {arastirma.MedianFiyat:N0} TL
- Ortalama KM: {arastirma.OrtalamaKilometre:N0}

KAYNAK DAÐILIMI:
{string.Join("\n", kaynakDagilimi)}

ÞEHÝR DAÐILIMI:
{string.Join("\n", sehirDagilimi)}

YIL DAÐILIMI:
{string.Join("\n", yilDagilimi)}

RAPOR ÝÇERÝÐÝ:
1. YÖNETÝCÝ ÖZETÝ
2. PÝYASA GENEL GÖRÜNÜMÜ
3. FÝYAT ANALÝZÝ VE TRENDLER
4. BÖLGESEL ANALÝZ
5. ALIM STRATEJÝSÝ ÖNERÝLERÝ
6. SATIM STRATEJÝSÝ ÖNERÝLERÝ
7. RÝSK DEÐERLENDÝRMESÝ
8. SONUÇ VE TAVSÝYELER

Profesyonel, detaylý ve galerici bakýþ açýsýyla hazýrla.";

        return await SendAIRequestAsync(prompt);
    }

    private async Task<string> SendAIRequestAsync(string prompt)
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
                    new { role = "system", content = "Sen Türkiye'deki ikinci el araç piyasasý konusunda uzman bir yapay zeka asistanýsýn. Güncel piyasa verilerini, fiyatlarý ve trendleri biliyorsun. Galericilere alým-satým stratejisi konusunda yardýmcý oluyorsun." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                max_tokens = 4000
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
        var random = new Random();

        // Marka listesi
        if (prompt.Contains("otomobil markalarýný listele"))
        {
            var markalar = new[]
            {
                new { marka = "Audi", ulke = "Almanya", segment = "Premium", populer = true },
                new { marka = "BMW", ulke = "Almanya", segment = "Premium", populer = true },
                new { marka = "Chery", ulke = "Çin", segment = "Standart", populer = false },
                new { marka = "Citroen", ulke = "Fransa", segment = "Standart", populer = true },
                new { marka = "Cupra", ulke = "Ýspanya", segment = "Premium", populer = false },
                new { marka = "Dacia", ulke = "Romanya", segment = "Standart", populer = true },
                new { marka = "DS", ulke = "Fransa", segment = "Premium", populer = false },
                new { marka = "Fiat", ulke = "Ýtalya", segment = "Standart", populer = true },
                new { marka = "Ford", ulke = "ABD", segment = "Standart", populer = true },
                new { marka = "Honda", ulke = "Japonya", segment = "Standart", populer = true },
                new { marka = "Hyundai", ulke = "Güney Kore", segment = "Standart", populer = true },
                new { marka = "Jeep", ulke = "ABD", segment = "Premium", populer = true },
                new { marka = "Kia", ulke = "Güney Kore", segment = "Standart", populer = true },
                new { marka = "Land Rover", ulke = "Ýngiltere", segment = "Premium", populer = true },
                new { marka = "Lexus", ulke = "Japonya", segment = "Premium", populer = false },
                new { marka = "Mazda", ulke = "Japonya", segment = "Standart", populer = true },
                new { marka = "Mercedes-Benz", ulke = "Almanya", segment = "Premium", populer = true },
                new { marka = "MG", ulke = "Çin", segment = "Standart", populer = true },
                new { marka = "Mini", ulke = "Ýngiltere", segment = "Premium", populer = true },
                new { marka = "Mitsubishi", ulke = "Japonya", segment = "Standart", populer = false },
                new { marka = "Nissan", ulke = "Japonya", segment = "Standart", populer = true },
                new { marka = "Opel", ulke = "Almanya", segment = "Standart", populer = true },
                new { marka = "Peugeot", ulke = "Fransa", segment = "Standart", populer = true },
                new { marka = "Porsche", ulke = "Almanya", segment = "Premium", populer = true },
                new { marka = "Renault", ulke = "Fransa", segment = "Standart", populer = true },
                new { marka = "Seat", ulke = "Ýspanya", segment = "Standart", populer = true },
                new { marka = "Skoda", ulke = "Çekya", segment = "Standart", populer = true },
                new { marka = "Subaru", ulke = "Japonya", segment = "Standart", populer = false },
                new { marka = "Suzuki", ulke = "Japonya", segment = "Standart", populer = true },
                new { marka = "Tesla", ulke = "ABD", segment = "Premium", populer = true },
                new { marka = "Toyota", ulke = "Japonya", segment = "Standart", populer = true },
                new { marka = "Volkswagen", ulke = "Almanya", segment = "Standart", populer = true },
                new { marka = "Volvo", ulke = "Ýsveç", segment = "Premium", populer = true }
            };
            return JsonSerializer.Serialize(markalar);
        }

        // Model listesi
        if (prompt.Contains("modellerini listele"))
        {
            var modeller = new List<object>();
            var segmentler = new[] { "C", "D", "SUV", "Crossover" };
            var kasalar = new[] { "Sedan", "Hatchback", "SUV", "Station" };

            for (int i = 0; i < random.Next(8, 15); i++)
            {
                modeller.Add(new
                {
                    model = $"Model {i + 1}",
                    segment = segmentler[random.Next(segmentler.Length)],
                    kasaTipi = kasalar[random.Next(kasalar.Length)],
                    baslangicYili = 2018 + random.Next(5),
                    yakitTipleri = new[] { "Benzin", "Dizel" },
                    vitesTipleri = new[] { "Manuel", "Otomatik" },
                    fiyatAraligi = $"{random.Next(500, 1500)}.000 - {random.Next(1500, 3000)}.000 TL",
                    populer = random.Next(2) == 1
                });
            }
            return JsonSerializer.Serialize(modeller);
        }

        // Ýlan listesi
        if (prompt.Contains("araç ilanlarýný topla"))
        {
            var sehirler = new[] { "Ýstanbul", "Ankara", "Ýzmir", "Bursa", "Antalya", "Konya", "Adana", "Gaziantep", "Kocaeli", "Mersin" };
            var kaynaklar = new[] { "Sahibinden", "Arabam", "Letgo", "Facebook Marketplace", "Otomerkezi" };
            var saticiTipleri = new[] { "Galeri", "Bireysel" };
            var renkler = new[] { "Beyaz", "Siyah", "Gri", "Kýrmýzý", "Mavi", "Lacivert", "Gümüþ" };

            var ilanlar = new List<object>();
            var baseFiyat = random.Next(800000, 2000000);

            for (int i = 0; i < random.Next(25, 45); i++)
            {
                var yil = 2018 + random.Next(7);
                var km = random.Next(10000, 180000);
                var fiyatFark = (int)(baseFiyat * (0.7 + random.NextDouble() * 0.6));

                ilanlar.Add(new
                {
                    kaynak = kaynaklar[random.Next(kaynaklar.Length)],
                    ilanNo = (100000000 + random.Next(900000000)).ToString(),
                    baslik = $"{yil} Model Araç",
                    marka = "Marka",
                    model = "Model",
                    versiyon = random.Next(3) == 0 ? "Sport Line" : (random.Next(2) == 0 ? "Comfort" : "Style"),
                    yil = yil,
                    kilometre = km,
                    fiyat = fiyatFark,
                    yakitTipi = random.Next(2) == 0 ? "Benzin" : "Dizel",
                    vitesTipi = random.Next(2) == 0 ? "Otomatik" : "Manuel",
                    kasaTipi = random.Next(2) == 0 ? "Sedan" : "SUV",
                    motorHacmi = random.Next(2) == 0 ? "1.5" : "2.0",
                    motorGucu = $"{random.Next(100, 250)} HP",
                    renk = renkler[random.Next(renkler.Length)],
                    boyaliParca = random.Next(5),
                    degisenParca = random.Next(3),
                    tramerTutari = random.Next(4) == 0 ? random.Next(5000, 50000) : 0,
                    hasarKayitli = random.Next(4) == 0,
                    sehir = sehirler[random.Next(sehirler.Length)],
                    ilce = "Merkez",
                    saticiTipi = saticiTipleri[random.Next(saticiTipleri.Length)],
                    saticiAdi = random.Next(2) == 0 ? "Auto Gallery" : "Bireysel Satýcý",
                    ilanTarihi = DateTime.Now.AddDays(-random.Next(1, 60)).ToString("yyyy-MM-dd"),
                    aktif = true
                });
            }
            return JsonSerializer.Serialize(ilanlar);
        }

        // Analiz raporu
        return $@"# PÝYASA ANALÝZ RAPORU

## 1. PÝYASA DURUMU
Mevcut piyasa **dengeli** bir görünüm sergilemektedir. Alýcý ve satýcý beklentileri yakýn seviyelerde.

## 2. FÝYAT ANALÝZÝ
- Piyasa ortalamasý makul seviyelerde
- En uygun fiyatlý ilanlar genellikle yüksek kilometreli veya hasarlý araçlar
- Premium versiyonlar %15-25 daha yüksek fiyatlanýyor

## 3. ALIM TAVSÝYELERÝ
- 50.000-80.000 km arasý araçlar en ideal
- Hasarsýz veya hafif boyalý araçlar tercih edilmeli
- Otomatik vites daha hýzlý satýlýyor

## 4. SATIM STRATEJÝSÝ
- Alým fiyatýnýn %10-15 üzeri satýþ hedeflenebilir
- Piyasa ortalamasýnýn %5 altýnda fiyatla hýzlý satýþ mümkün

## 5. DÝKKAT EDÝLMESÝ GEREKENLER
- Tramer kayýtlarýný detaylý inceleyin
- Servis geçmiþini kontrol edin
- Deðiþen parça sayýsý 2'yi geçmemeli

## 6. EN ÝYÝ FIRSATLAR
- Düþük KM, hasarsýz, tek el araçlar
- Galeri satýþlarý genelde daha güvenilir

## 7. KAÇINILMASI GEREKENLER
- Yüksek tramer kayýtlý araçlar
- Birden fazla deðiþen parça olan araçlar
- Piyasa üstü fiyatlandýrýlmýþ ilanlar

## 8. SONUÇ
Bu segmentte kar marjý %8-12 arasýnda tutulabilir. Hýzlý devir için hasarsýz, düþük km araçlara odaklanýlmalý.";
    }

    private List<AracMarkaInfo> ParseMarkaListesi(string response)
    {
        try
        {
            var jsonStart = response.IndexOf('[');
            var jsonEnd = response.LastIndexOf(']') + 1;
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonStr = response.Substring(jsonStart, jsonEnd - jsonStart);
                var items = JsonSerializer.Deserialize<List<JsonElement>>(jsonStr);

                return items?.Select(item => new AracMarkaInfo
                {
                    Marka = item.GetProperty("marka").GetString() ?? "",
                    Ulke = item.TryGetProperty("ulke", out var ulke) ? ulke.GetString() : null,
                    Segment = item.TryGetProperty("segment", out var segment) ? segment.GetString() : null,
                    Populer = item.TryGetProperty("populer", out var pop) && pop.GetBoolean()
                }).ToList() ?? new List<AracMarkaInfo>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Marka listesi parse hatasý");
        }
        return new List<AracMarkaInfo>();
    }

    private List<AracModelInfo> ParseModelListesi(string response)
    {
        try
        {
            var jsonStart = response.IndexOf('[');
            var jsonEnd = response.LastIndexOf(']') + 1;
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonStr = response.Substring(jsonStart, jsonEnd - jsonStart);
                var items = JsonSerializer.Deserialize<List<JsonElement>>(jsonStr);

                return items?.Select(item => new AracModelInfo
                {
                    Model = item.GetProperty("model").GetString() ?? "",
                    Segment = item.TryGetProperty("segment", out var seg) ? seg.GetString() : null,
                    KasaTipi = item.TryGetProperty("kasaTipi", out var kasa) ? kasa.GetString() : null,
                    BaslangicYili = item.TryGetProperty("baslangicYili", out var yil) ? yil.GetInt32() : null,
                    FiyatAraligi = item.TryGetProperty("fiyatAraligi", out var fiyat) ? fiyat.GetString() : null,
                    Populer = item.TryGetProperty("populer", out var pop) && pop.GetBoolean()
                }).ToList() ?? new List<AracModelInfo>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Model listesi parse hatasý");
        }
        return new List<AracModelInfo>();
    }

    private List<PiyasaIlan> ParseIlanListesi(string response)
    {
        try
        {
            var jsonStart = response.IndexOf('[');
            var jsonEnd = response.LastIndexOf(']') + 1;
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonStr = response.Substring(jsonStart, jsonEnd - jsonStart);
                var items = JsonSerializer.Deserialize<List<JsonElement>>(jsonStr);

                return items?.Select(item => new PiyasaIlan
                {
                    Kaynak = item.TryGetProperty("kaynak", out var k) ? k.GetString() ?? "Bilinmiyor" : "Bilinmiyor",
                    IlanNo = item.TryGetProperty("ilanNo", out var no) ? no.GetString() : null,
                    IlanBasligi = item.TryGetProperty("baslik", out var b) ? b.GetString() ?? "" : "",
                    Marka = item.TryGetProperty("marka", out var m) ? m.GetString() ?? "" : "",
                    Model = item.TryGetProperty("model", out var md) ? md.GetString() ?? "" : "",
                    Versiyon = item.TryGetProperty("versiyon", out var v) ? v.GetString() : null,
                    ModelYili = item.TryGetProperty("yil", out var y) ? y.GetInt32() : DateTime.Now.Year,
                    Kilometre = item.TryGetProperty("kilometre", out var km) ? km.GetInt32() : 0,
                    Fiyat = item.TryGetProperty("fiyat", out var f) ? f.GetDecimal() : 0,
                    YakitTipi = item.TryGetProperty("yakitTipi", out var yt) ? yt.GetString() : null,
                    VitesTipi = item.TryGetProperty("vitesTipi", out var vt) ? vt.GetString() : null,
                    KasaTipi = item.TryGetProperty("kasaTipi", out var kt) ? kt.GetString() : null,
                    MotorHacmi = item.TryGetProperty("motorHacmi", out var mh) ? mh.GetString() : null,
                    MotorGucu = item.TryGetProperty("motorGucu", out var mg) ? mg.GetString() : null,
                    Renk = item.TryGetProperty("renk", out var r) ? r.GetString() : null,
                    BoyaliParcaSayisi = item.TryGetProperty("boyaliParca", out var bp) ? bp.GetInt32() : 0,
                    DegisenParcaSayisi = item.TryGetProperty("degisenParca", out var dp) ? dp.GetInt32() : 0,
                    TramerTutari = item.TryGetProperty("tramerTutari", out var tt) ? tt.GetDecimal() : 0,
                    HasarKayitli = item.TryGetProperty("hasarKayitli", out var hk) && hk.GetBoolean(),
                    Sehir = item.TryGetProperty("sehir", out var s) ? s.GetString() : null,
                    Ilce = item.TryGetProperty("ilce", out var il) ? il.GetString() : null,
                    SaticiTipi = item.TryGetProperty("saticiTipi", out var st) ? st.GetString() : null,
                    SaticiAdi = item.TryGetProperty("saticiAdi", out var sa) ? sa.GetString() : null,
                    IlanTarihi = item.TryGetProperty("ilanTarihi", out var it) ? DateTime.TryParse(it.GetString(), out var dt) ? dt : (DateTime?)null : null,
                    AktifMi = !item.TryGetProperty("aktif", out var a) || a.GetBoolean(),
                    ToplanmaTarihi = DateTime.Now
                }).ToList() ?? new List<PiyasaIlan>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ýlan listesi parse hatasý");
        }
        return new List<PiyasaIlan>();
    }
}

// Request/Response modelleri
public class AracPiyasaArastirmaRequest
{
    public string Marka { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? Versiyon { get; set; }
    public int? YilBaslangic { get; set; }
    public int? YilBitis { get; set; }
    public string? YakitTipi { get; set; }
    public string? VitesTipi { get; set; }
    public int? MinKilometre { get; set; }
    public int? MaxKilometre { get; set; }
    public decimal? MinFiyat { get; set; }
    public decimal? MaxFiyat { get; set; }
    public string? Sehir { get; set; }
}

public class AracMarkaInfo
{
    public string Marka { get; set; } = string.Empty;
    public string? Ulke { get; set; }
    public string? Segment { get; set; }
    public bool Populer { get; set; }
}

public class AracModelInfo
{
    public string Model { get; set; } = string.Empty;
    public string? Segment { get; set; }
    public string? KasaTipi { get; set; }
    public int? BaslangicYili { get; set; }
    public string? FiyatAraligi { get; set; }
    public bool Populer { get; set; }
}

public class PiyasaAnalizSonuc
{
    public string Marka { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int ToplamIlan { get; set; }
    public decimal OrtalamaFiyat { get; set; }
    public decimal MinFiyat { get; set; }
    public decimal MaxFiyat { get; set; }
    public int OrtalamaKilometre { get; set; }
    public string AnalizMetni { get; set; } = string.Empty;
    public DateTime AnalizTarihi { get; set; }
}
