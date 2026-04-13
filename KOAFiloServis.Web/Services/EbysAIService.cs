using KOAFiloServis.Web.Services.Interfaces;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace KOAFiloServis.Web.Services;

/// <summary>
/// EBYS AI Servisi - OCR, belge sınıflandırma ve akıllı arama
/// Ollama (local LLM) ve Tesseract OCR kullanarak offline çalışabilir
/// </summary>
public class EbysAIService : IEbysAIService
{
    private readonly IOllamaService _ollamaService;
    private readonly ILogger<EbysAIService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    
    // OCR ayarları
    private readonly string _tesseractPath;
    private readonly string _tesseractDataPath;
    private readonly bool _ocrEnabled;

    public EbysAIService(
        IOllamaService ollamaService,
        ILogger<EbysAIService> logger,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _ollamaService = ollamaService;
        _logger = logger;
        _configuration = configuration;
        _environment = environment;

        // Tesseract OCR ayarları
        _tesseractPath = configuration["Ocr:TesseractPath"] ?? "tesseract";
        _tesseractDataPath = configuration["Ocr:TessDataPath"] ?? "";
        _ocrEnabled = configuration.GetValue("Ocr:Enabled", true);
    }

    #region OCR İşlemleri

    public async Task<OcrSonuc> MetinCikarAsync(Stream dosyaStream, string dosyaAdi)
    {
        var sw = Stopwatch.StartNew();
        var sonuc = new OcrSonuc();

        try
        {
            // Geçici dosya oluştur
            var tempPath = Path.Combine(Path.GetTempPath(), $"ocr_{Guid.NewGuid()}{Path.GetExtension(dosyaAdi)}");
            
            await using (var fileStream = new FileStream(tempPath, FileMode.Create))
            {
                await dosyaStream.CopyToAsync(fileStream);
            }

            try
            {
                sonuc = await MetinCikarAsync(tempPath);
            }
            finally
            {
                // Geçici dosyayı sil
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OCR stream işlemi hatası: {DosyaAdi}", dosyaAdi);
            sonuc.Basarili = false;
            sonuc.HataMesaji = $"OCR hatası: {ex.Message}";
        }

        sw.Stop();
        sonuc.IslemSuresi = sw.Elapsed;
        return sonuc;
    }

    public async Task<OcrSonuc> MetinCikarAsync(string dosyaYolu)
    {
        var sw = Stopwatch.StartNew();
        var sonuc = new OcrSonuc();

        if (!_ocrEnabled)
        {
            sonuc.Basarili = false;
            sonuc.HataMesaji = "OCR özelliği devre dışı bırakılmış.";
            return sonuc;
        }

        try
        {
            if (!File.Exists(dosyaYolu))
            {
                sonuc.Basarili = false;
                sonuc.HataMesaji = "Dosya bulunamadı.";
                return sonuc;
            }

            var uzanti = Path.GetExtension(dosyaYolu).ToLowerInvariant();

            // PDF için özel işlem
            if (uzanti == ".pdf")
            {
                sonuc = await PdfOcrAsync(dosyaYolu);
            }
            // Görsel dosyalar için Tesseract
            else if (new[] { ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".tif", ".gif" }.Contains(uzanti))
            {
                sonuc = await GorselOcrAsync(dosyaYolu);
            }
            // Metin tabanlı dosyalar
            else if (new[] { ".txt", ".csv" }.Contains(uzanti))
            {
                sonuc.Metin = await File.ReadAllTextAsync(dosyaYolu);
                sonuc.Basarili = true;
                sonuc.GuvenSkor = 100;
                sonuc.SayfaSayisi = 1;
            }
            else
            {
                sonuc.Basarili = false;
                sonuc.HataMesaji = $"Desteklenmeyen dosya formatı: {uzanti}";
            }

            if (sonuc.Basarili)
            {
                sonuc.Detay = new OcrDetayBilgi
                {
                    KarakterSayisi = sonuc.Metin.Length,
                    KelimeSayisi = sonuc.Metin.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length,
                    SatirSayisi = sonuc.Metin.Split('\n').Length
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OCR işlemi hatası: {DosyaYolu}", dosyaYolu);
            sonuc.Basarili = false;
            sonuc.HataMesaji = $"OCR hatası: {ex.Message}";
        }

        sw.Stop();
        sonuc.IslemSuresi = sw.Elapsed;
        return sonuc;
    }

    private async Task<OcrSonuc> GorselOcrAsync(string dosyaYolu)
    {
        var sonuc = new OcrSonuc { Dil = "tur" };

        try
        {
            // Tesseract komutunu çalıştır
            var outputFile = Path.Combine(Path.GetTempPath(), $"ocr_out_{Guid.NewGuid()}");
            
            var args = new StringBuilder();
            args.Append($"\"{dosyaYolu}\" \"{outputFile}\" -l tur+eng");
            
            if (!string.IsNullOrEmpty(_tesseractDataPath))
                args.Append($" --tessdata-dir \"{_tesseractDataPath}\"");

            var startInfo = new ProcessStartInfo
            {
                FileName = _tesseractPath,
                Arguments = args.ToString(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();
            
            var stdErr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            var txtFile = outputFile + ".txt";
            if (File.Exists(txtFile))
            {
                sonuc.Metin = await File.ReadAllTextAsync(txtFile);
                sonuc.Basarili = true;
                sonuc.SayfaSayisi = 1;
                
                // Güven skoru hesapla (basit metrik - kelime yoğunluğu)
                sonuc.GuvenSkor = HesaplaGuvenSkor(sonuc.Metin);
                
                File.Delete(txtFile);
            }
            else
            {
                sonuc.Basarili = false;
                sonuc.HataMesaji = $"Tesseract çıktı oluşturamadı. Hata: {stdErr}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Görsel OCR hatası");
            sonuc.Basarili = false;
            sonuc.HataMesaji = $"Tesseract hatası: {ex.Message}. Tesseract OCR kurulu olduğundan emin olun.";
        }

        return sonuc;
    }

    private async Task<OcrSonuc> PdfOcrAsync(string dosyaYolu)
    {
        var sonuc = new OcrSonuc { Dil = "tur" };

        try
        {
            // PDF'i görüntüye çevir ve OCR yap (pdftoppm + tesseract)
            // Alternatif: iTextSharp ile metin çıkarma dene
            
            // Önce doğrudan metin çıkarmayı dene
            var metin = await PdfMetinCikarAsync(dosyaYolu);
            
            if (!string.IsNullOrWhiteSpace(metin) && metin.Length > 50)
            {
                sonuc.Metin = metin;
                sonuc.Basarili = true;
                sonuc.GuvenSkor = 95; // Doğrudan metin çıkarma yüksek güven
                sonuc.SayfaSayisi = 1;
            }
            else
            {
                // Görüntü tabanlı PDF - OCR gerekli
                sonuc.Basarili = false;
                sonuc.HataMesaji = "PDF görüntü tabanlı. Tam OCR için pdftoppm gerekli.";
                sonuc.Metin = metin; // Kısmi metin varsa döndür
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDF OCR hatası");
            sonuc.Basarili = false;
            sonuc.HataMesaji = $"PDF işleme hatası: {ex.Message}";
        }

        return sonuc;
    }

    private async Task<string> PdfMetinCikarAsync(string dosyaYolu)
    {
        // Basit PDF metin çıkarma (iTextSharp olmadan)
        // Production için iText7 veya PdfPig kütüphanesi önerilir
        
        try
        {
            var bytes = await File.ReadAllBytesAsync(dosyaYolu);
            var text = Encoding.UTF8.GetString(bytes);
            
            // Basit PDF stream metin çıkarma
            var matches = Regex.Matches(text, @"\(([^)]+)\)");
            var sb = new StringBuilder();
            
            foreach (Match match in matches)
            {
                var content = match.Groups[1].Value;
                if (content.Length > 2 && !content.StartsWith("\\"))
                {
                    sb.AppendLine(content);
                }
            }
            
            return sb.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }

    private double HesaplaGuvenSkor(string metin)
    {
        if (string.IsNullOrWhiteSpace(metin))
            return 0;

        // Basit güven skoru hesaplama
        var kelimeler = metin.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var toplamKelime = kelimeler.Length;
        
        if (toplamKelime == 0) return 0;

        // Türkçe harf içeren kelime oranı
        var turkceKelimeSayisi = kelimeler.Count(k => 
            Regex.IsMatch(k, @"[a-zA-ZğüşöçıİĞÜŞÖÇ]{2,}"));

        var oran = (double)turkceKelimeSayisi / toplamKelime * 100;
        return Math.Min(100, Math.Max(0, oran));
    }

    #endregion

    #region Belge Sınıflandırma

    public async Task<BelgeSiniflandirmaSonuc> BelgeSiniflandirAsync(string metin, BelgeTipi belgeGrubu)
    {
        var sonuc = new BelgeSiniflandirmaSonuc();

        try
        {
            if (string.IsNullOrWhiteSpace(metin))
            {
                sonuc.Basarili = false;
                sonuc.HataMesaji = "Sınıflandırılacak metin boş.";
                return sonuc;
            }

            // Kategorileri belge grubuna göre al
            var kategoriler = GetKategoriler(belgeGrubu);

            // AI ile sınıflandırma yap
            var sistemPrompt = $@"Sen bir belge sınıflandırma uzmanısın. 
Verilen belge metnini analiz edip en uygun kategoriyi belirle.

Kullanılabilir kategoriler:
{string.Join("\n", kategoriler.Select(k => $"- {k.Key}: {k.Value}"))}

Yanıtını şu JSON formatında ver:
{{
    ""kategori"": ""kategori_kodu"",
    ""guven"": 85,
    ""aciklama"": ""Bu kategorinin seçilme nedeni""
}}

Sadece JSON döndür, başka açıklama ekleme.";

            var prompt = $"Belge metni:\n{metin.Substring(0, Math.Min(metin.Length, 2000))}";

            var aiYanit = await _ollamaService.AnalizYapAsync(prompt, sistemPrompt);

            // AI yanıtını parse et
            sonuc = ParseSiniflandirmaYaniti(aiYanit, kategoriler);
            sonuc.Basarili = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Belge sınıflandırma hatası");
            sonuc.Basarili = false;
            sonuc.HataMesaji = $"Sınıflandırma hatası: {ex.Message}";
        }

        return sonuc;
    }

    private Dictionary<string, string> GetKategoriler(BelgeTipi belgeGrubu)
    {
        return belgeGrubu switch
        {
            BelgeTipi.EbysEvrak => new Dictionary<string, string>
            {
                ["resmi_yazi"] = "Resmi Yazışma - Devlet kurumlarından gelen/giden resmi yazılar",
                ["sozlesme"] = "Sözleşme - İş sözleşmeleri, anlaşmalar",
                ["fatura"] = "Fatura/Mali Belge - Faturalar, makbuzlar",
                ["dilekce"] = "Dilekçe/Talep - Dilekçeler, başvurular",
                ["rapor"] = "Rapor - Teknik raporlar, denetim raporları",
                ["duyuru"] = "Duyuru/Genelge - Duyurular, sirkülerler",
                ["diger"] = "Diğer - Yukarıdaki kategorilere uymayan belgeler"
            },
            BelgeTipi.PersonelOzluk => new Dictionary<string, string>
            {
                ["kimlik"] = "Kimlik Belgesi - Nüfus cüzdanı, ehliyet",
                ["egitim"] = "Eğitim Belgesi - Diploma, sertifika",
                ["saglik"] = "Sağlık Belgesi - Sağlık raporu, işe giriş muayenesi",
                ["sofor"] = "Şoför Belgesi - SRC, psikoteknik",
                ["sgk"] = "SGK Belgesi - İşe giriş bildirgesi, hizmet dökümü",
                ["diger"] = "Diğer Özlük Belgesi"
            },
            BelgeTipi.AracEvrak => new Dictionary<string, string>
            {
                ["ruhsat"] = "Araç Ruhsatı",
                ["sigorta"] = "Sigorta Poliçesi - Kasko, trafik sigortası",
                ["muayene"] = "Muayene Belgesi - Araç muayene raporu",
                ["bakim"] = "Bakım Belgesi - Servis kayıtları",
                ["diger"] = "Diğer Araç Evrakı"
            },
            _ => new Dictionary<string, string>
            {
                ["genel"] = "Genel Belge"
            }
        };
    }

    private BelgeSiniflandirmaSonuc ParseSiniflandirmaYaniti(string aiYanit, Dictionary<string, string> kategoriler)
    {
        var sonuc = new BelgeSiniflandirmaSonuc();

        try
        {
            // JSON'ı bul ve parse et
            var jsonMatch = Regex.Match(aiYanit, @"\{[^{}]*\}", RegexOptions.Singleline);
            
            if (jsonMatch.Success)
            {
                var json = JsonDocument.Parse(jsonMatch.Value);
                var root = json.RootElement;

                if (root.TryGetProperty("kategori", out var kategoriEl))
                {
                    var kategoriKodu = kategoriEl.GetString() ?? "";
                    
                    if (kategoriler.TryGetValue(kategoriKodu, out var kategoriAdi))
                    {
                        sonuc.TahminEdilenKategori = kategoriAdi;
                    }
                    else
                    {
                        sonuc.TahminEdilenKategori = kategoriKodu;
                    }
                }

                if (root.TryGetProperty("guven", out var guvenEl))
                {
                    sonuc.GuvenSkor = guvenEl.TryGetDouble(out var g) ? g : 50;
                }

                if (root.TryGetProperty("aciklama", out var aciklamaEl))
                {
                    sonuc.AIAciklama = aciklamaEl.GetString();
                }
            }
            else
            {
                // JSON bulunamadı, metin analizi yap
                sonuc.TahminEdilenKategori = "Belirlenemedi";
                sonuc.GuvenSkor = 30;
                sonuc.AIAciklama = aiYanit;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI yanıtı parse edilemedi: {Yanit}", aiYanit);
            sonuc.TahminEdilenKategori = "Belirlenemedi";
            sonuc.GuvenSkor = 20;
            sonuc.AIAciklama = aiYanit;
        }

        return sonuc;
    }

    #endregion

    #region Belge Özeti ve Anahtar Kelimeler

    public async Task<string> BelgeOzetiOlusturAsync(string metin, int maxKarakter = 500)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(metin))
                return "Özet oluşturulamadı: Metin boş.";

            var sistemPrompt = @"Sen bir belge özetleme uzmanısın.
Verilen belge metninin kısa ve öz bir özetini Türkçe olarak yaz.
Özet en fazla 2-3 cümle olmalı.
Sadece özeti yaz, başka açıklama ekleme.";

            var kisaMetin = metin.Substring(0, Math.Min(metin.Length, 3000));
            var prompt = $"Belge metni:\n{kisaMetin}";

            var ozet = await _ollamaService.AnalizYapAsync(prompt, sistemPrompt);
            
            // Özeti kısalt
            if (ozet.Length > maxKarakter)
                ozet = ozet.Substring(0, maxKarakter) + "...";

            return ozet;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Belge özeti oluşturma hatası");
            return $"Özet oluşturulamadı: {ex.Message}";
        }
    }

    public async Task<List<string>> AnahtarKelimelerCikarAsync(string metin, int maxKelime = 10)
    {
        var kelimeler = new List<string>();

        try
        {
            if (string.IsNullOrWhiteSpace(metin))
                return kelimeler;

            var sistemPrompt = $@"Sen bir metin analiz uzmanısın.
Verilen belge metninden en önemli {maxKelime} anahtar kelimeyi çıkar.
Her kelimeyi virgülle ayırarak tek satırda yaz.
Sadece kelimeleri yaz, başka açıklama ekleme.";

            var kisaMetin = metin.Substring(0, Math.Min(metin.Length, 2000));
            var prompt = $"Belge metni:\n{kisaMetin}";

            var yanit = await _ollamaService.AnalizYapAsync(prompt, sistemPrompt);
            
            // Virgülle ayrılmış kelimeleri parse et
            kelimeler = yanit
                .Split(new[] { ',', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(k => k.Trim())
                .Where(k => k.Length > 1)
                .Take(maxKelime)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Anahtar kelime çıkarma hatası");
        }

        return kelimeler;
    }

    #endregion

    #region Belge Benzerliği

    public async Task<double> BelgeBenzerligiHesaplaAsync(string metin1, string metin2)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(metin1) || string.IsNullOrWhiteSpace(metin2))
                return 0;

            // Basit Jaccard benzerliği (kelime bazlı)
            var kelimeler1 = MetniKelimelereAyir(metin1);
            var kelimeler2 = MetniKelimelereAyir(metin2);

            var kesisim = kelimeler1.Intersect(kelimeler2).Count();
            var birlesim = kelimeler1.Union(kelimeler2).Count();

            if (birlesim == 0) return 0;

            return Math.Round((double)kesisim / birlesim * 100, 2);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Belge benzerliği hesaplama hatası");
            return 0;
        }
    }

    private HashSet<string> MetniKelimelereAyir(string metin)
    {
        // Stop words (Türkçe)
        var stopWords = new HashSet<string> 
        { 
            "ve", "veya", "ile", "için", "bir", "bu", "şu", "o", "de", "da", 
            "mi", "mu", "mı", "mü", "ki", "ama", "ancak", "fakat", "çünkü",
            "gibi", "kadar", "daha", "en", "çok", "az", "her", "hiç"
        };

        return metin
            .ToLowerInvariant()
            .Split(new[] { ' ', '\n', '\r', '\t', '.', ',', '!', '?', ':', ';', '"', '\'', '(', ')' }, 
                StringSplitOptions.RemoveEmptyEntries)
            .Where(k => k.Length > 2 && !stopWords.Contains(k))
            .ToHashSet();
    }

    #endregion

    #region Öneriler

    public async Task<BelgeOneriSonuc> OneriGetirAsync(string metin, BelgeTipi belgeGrubu)
    {
        var sonuc = new BelgeOneriSonuc();

        try
        {
            if (string.IsNullOrWhiteSpace(metin))
            {
                sonuc.Basarili = false;
                sonuc.HataMesaji = "Öneri için metin gerekli.";
                return sonuc;
            }

            var sistemPrompt = @"Sen bir belge yönetim uzmanısın.
Verilen belge metnini analiz edip öneriler sun.

Yanıtını şu JSON formatında ver:
{
    ""konu"": ""Belgenin tahmini konusu"",
    ""oncelik"": ""Dusuk/Normal/Yuksek/Acil"",
    ""vadeTarihi"": ""varsa YYYY-MM-DD formatında"",
    ""oneriler"": [""Öneri 1"", ""Öneri 2"", ""Öneri 3""]
}

Sadece JSON döndür.";

            var kisaMetin = metin.Substring(0, Math.Min(metin.Length, 2000));
            var prompt = $"Belge metni:\n{kisaMetin}";

            var aiYanit = await _ollamaService.AnalizYapAsync(prompt, sistemPrompt);
            
            // JSON parse et
            var jsonMatch = Regex.Match(aiYanit, @"\{[^{}]*""oneriler""[^{}]*\}", RegexOptions.Singleline);
            
            if (jsonMatch.Success)
            {
                var json = JsonDocument.Parse(jsonMatch.Value);
                var root = json.RootElement;

                if (root.TryGetProperty("konu", out var konuEl))
                    sonuc.TahminEdilenKonu = konuEl.GetString();

                if (root.TryGetProperty("oncelik", out var oncelikEl))
                    sonuc.TahminEdilenOncelik = oncelikEl.GetString();

                if (root.TryGetProperty("vadeTarihi", out var vadeEl))
                {
                    var vadeStr = vadeEl.GetString();
                    if (DateTime.TryParse(vadeStr, out var vade))
                        sonuc.TahminEdilenVadeTarihi = vade;
                }

                if (root.TryGetProperty("oneriler", out var onerilerEl) && onerilerEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var oneri in onerilerEl.EnumerateArray())
                    {
                        var oneriStr = oneri.GetString();
                        if (!string.IsNullOrWhiteSpace(oneriStr))
                            sonuc.Oneriler.Add(oneriStr);
                    }
                }
            }

            sonuc.Basarili = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Belge öneri hatası");
            sonuc.Basarili = false;
            sonuc.HataMesaji = $"Öneri hatası: {ex.Message}";
        }

        return sonuc;
    }

    #endregion

    #region Durum Kontrolü

    public async Task<AIDurumBilgi> DurumKontrolAsync()
    {
        var durum = new AIDurumBilgi
        {
            OllamaModel = _ollamaService.ModelAdi,
            OcrMotor = "Tesseract OCR"
        };

        try
        {
            // Ollama kontrolü
            durum.OllamaAktif = await _ollamaService.BaglantiKontrolAsync();

            // OCR kontrolü (Tesseract)
            durum.OcrAktif = await TesseractKontrolAsync();

            if (durum.OcrAktif)
            {
                durum.OcrVersiyon = await TesseractVersiyonAlAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI durum kontrolü hatası");
            durum.HataMesaji = ex.Message;
        }

        return durum;
    }

    private async Task<bool> TesseractKontrolAsync()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _tesseractPath,
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();
            await process.WaitForExitAsync();

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private async Task<string> TesseractVersiyonAlAsync()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _tesseractPath,
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            // "tesseract 5.3.0" gibi çıktıdan versiyon al
            var match = Regex.Match(output, @"tesseract\s+([\d.]+)");
            return match.Success ? match.Groups[1].Value : "Bilinmiyor";
        }
        catch
        {
            return "Bilinmiyor";
        }
    }

    #endregion
}
