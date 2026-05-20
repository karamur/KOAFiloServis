# Çalışma Notları — 14.05.2026

> Önceki gün: `docs/CALISMA-NOTLARI-2026-05-13.md` (Cari→Firma shadow FK fix + mutabakat iyileştirmeleri).

---

## ✅ Bugün Yapılanlar

### 1. AI Asistan – Model Listesi UX İyileştirmesi (Kısmen tamamlandı, derlenmedi)
**Dosya:** `KOAFiloServis.Web/Components/Pages/Ayarlar/AIAsistan.razor`

- Mevcut durum: AIAsistan sayfasındaki model dropdown'ı yalnızca **Ollama'da yüklü** modelleri listeliyordu (`ChatService.GetAvailableModelsAsync()` → `OllamaApiClient.ListLocalModelsAsync()`).
- Yapılan değişiklik: Dropdown iki `<optgroup>` ile yeniden düzenlendi:
  - **Yerel (Ollama):** Makinede yüklü olanlar
  - **Önerilen (yüklü değil):** Katalogdaki ama henüz `ollama pull` edilmemiş modeller — option metninde `(indir: ollama pull <model>)` ipucu var
- Genişlik 200px → 240px yapıldı.

### 2. DeepSeek listesi araştırması
- Ollama public registry'de **`deepseek-v4` tag'i bulunmuyor**. Mevcut resmi sürümler:
  - `deepseek-v3`
  - `deepseek-r1` (reasoning)
  - `deepseek-coder-v2`
- Kullanıcının istediği "DeepSeek V4" için en uygun yer: katalog listesine `deepseek-v3` (veya v4 çıktığında değiştirmek üzere placeholder) ekle, yüklü değilse uyarı ile birlikte göster.

---

## 🚧 Yarım Kalanlar (İlk iş bunlar)

### A. AIAsistan.razor — Katalog metodu eklenmeli
Markup'ta `GetBirlesikModelListesi()` çağrısı eklendi ama **@code bloğunda metot henüz yok** → derleme HATA verecek.

Eklenecek (yaklaşık 295. satırdan sonra, `chatSuggestions` yakınına):

```csharp
// Önerilen / katalog modelleri (Ollama'da pull edilmeden de dropdown'da görünür)
private static readonly List<string> ModelKatalogu = new()
{
    "llama3.2",
    "llama3.1",
    "qwen2.5",
    "mistral",
    "phi3",
    "gemma2",
    "deepseek-v3",
    "deepseek-r1",
    "deepseek-coder-v2"
};

private List<string> GetBirlesikModelListesi()
{
    var birlesik = new List<string>();
    // Önce yüklü olanlar (orijinal sıra korunsun)
    birlesik.AddRange(availableModels);
    // Sonra katalogdaki yüklü olmayanlar
    foreach (var m in ModelKatalogu)
    {
        if (!birlesik.Any(x => string.Equals(x, m, StringComparison.OrdinalIgnoreCase)))
            birlesik.Add(m);
    }
    return birlesik;
}
```

### B. `OnModelChanged()` — yüklü olmayan model seçilirse uyarı
```csharp
private async Task OnModelChanged()
{
    ChatService.SetModel(selectedModel);
    if (availableModels.Any() && !availableModels.Contains(selectedModel, StringComparer.OrdinalIgnoreCase))
    {
        await JS.InvokeVoidAsync("alert",
            $"'{selectedModel}' yerel Ollama'da yüklü değil.\nTerminalde: ollama pull {selectedModel}");
    }
}
```
> Not: Metot artık `async Task` olduğu için markup'ta `@bind:after="OnModelChanged"` çalışmaya devam eder (Blazor async Task'i destekler).

### C. Build doğrulaması
- `dotnet build` çalıştırılıp 0 hata teyit edilecek.
- Sayfa açılıp dropdown'da DeepSeek modellerinin "Önerilen" grubunda göründüğü görsel olarak doğrulanacak.

### D. Commit & push
Önerilen mesaj:
```
feat(ai-asistan): DeepSeek dahil önerilen model katalogu + yüklü olmayan model uyarısı
```

---

## 📋 Sonraki Geliştirmeler (Sıra)

1. **P1** AI sağlayıcı sayısı arttıkça (OpenAI / OpenRouter / Ollama) tek bir "AI Sağlayıcı Ayarları" sayfası — şu an `appsettings.json`'da elle yönetiliyor.
2. **P2** Dünden kalan: `PuantajMutabakat.razor` detay modaline "Eşleşmemiş Faturalar" + "Faturalanmamış Puantajlar" + manuel eşleştir butonu.
3. **P3** Dashboard / Servis / Finans / Belge sayfalarının `FirmaId1` fix sonrası fiilen sorunsuz açıldığını doğrula (dün build OK, runtime smoke test atlanmıştı).
4. **P4** `PuantajEslestirmeService` için en az 2 birim test.

---

## ⚠️ Riskler / Hatırlatmalar
- AIAsistan.razor **şu an derlenmez** — markup'ta `GetBirlesikModelListesi()` var, metot yok. Yarın ilk iş bunu eklemek.
- Ollama registry'de `deepseek-v4` henüz yok; v3/r1 kullan. v4 yayınlanırsa sadece `ModelKatalogu` listesine ekle.
- `OnModelChanged` sync'ten async'e çevrilirse signature değişikliği var — markup tarafında `@bind:after` Blazor'da Task döndüren metotları destekler, sorun yok.

---

## 🔧 Hızlı Komutlar
```powershell
# Build
cd C:\Users\muratk\Desktop\d yedek\calisma\Claude-Code\KOAFiloServis
dotnet build

# Ollama'ya DeepSeek modellerini çek (opsiyonel)
ollama pull deepseek-v3
ollama pull deepseek-r1

# Çalıştır
dotnet run --project KOAFiloServis.Web

# Commit
git add -A
git commit -m "feat(ai-asistan): DeepSeek dahil önerilen model katalogu + yüklü olmayan model uyarısı"
git push
```
