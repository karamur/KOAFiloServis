<div align="center">

<img src="https://capsule-render.vercel.app/api?type=waving&color=0:1e3a5f,100:2563eb&height=160&section=header&text=KOAFiloServis&fontSize=48&fontColor=ffffff&fontAlignY=38&desc=Kurumsal%20Filo%20Yönetimi%20%26%20ERP%20Platformu&descSize=18&descAlignY=60&descColor=bfdbfe" width="100%" alt="KOAFiloServis" />

<br/>

[![.NET](https://img.shields.io/badge/.NET_10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor_Server-512BD4?style=for-the-badge&logo=blazor&logoColor=white)](https://blazor.net/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL_14+-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![EF Core](https://img.shields.io/badge/EF_Core_10-68217A?style=for-the-badge)](https://learn.microsoft.com/ef/core/)
[![License](https://img.shields.io/badge/Lisans-Ticari-dc2626?style=for-the-badge)](#-lisans)

<br/>

> **Taşımacılık ve lojistik firmaları için** araç, sürücü, muhasebe, bordro, EBYS ve ihale süreçlerini tek platformda birleştiren modüler kurumsal çözüm.

<br/>

[🚀 Kurulum](#-kurulum) &nbsp;|&nbsp; [✨ Özellikler](#-öne-çıkan-özellikler) &nbsp;|&nbsp; [🧩 Mimari](#-mimari) &nbsp;|&nbsp; [⚙️ Yapılandırma](#️-yapılandırma) &nbsp;|&nbsp; [🧪 Test](#-test) &nbsp;|&nbsp; [📦 Yayınlama](#-yayınlama)

<br/>

</div>

---

## 📖 Genel Bakış

**KOAFiloServis**, filo ve taşımacılık şirketlerinin tüm operasyonlarını tek platformdan yönetebilmesi için geliştirilmiş **modüler** ve **çok firmalı (multi-tenant)** bir kurumsal ERP çözümüdür.

Araç yönetiminden bordro hesaplamaya, ihale süreçlerinden e-fatura entegrasyonuna kadar **200+ ekran** ile tam kapsamlı bir dijital dönüşüm platformu sunar.

<table>
<tr>
<td align="center" width="200">

### 🏢 Kimler İçin?
Filo yönetim firmaları, lojistik şirketleri, taşımacılık operatörleri, servis ve muhasebe ekipleri

</td>
<td align="center" width="200">

### 🧩 Ne Sunar?
Araç & sürücü, muhasebe, bordro, EBYS, ihale, raporlama ve AI destekli analiz

</td>
<td align="center" width="200">

### ⚡ Teknoloji
.NET 10 Blazor Server, PostgreSQL, Quartz.NET, SignalR, Redis, OpenAI/Ollama

</td>
</tr>
</table>

---

## ✨ Öne Çıkan Özellikler

<table>
<tr>
<td valign="top" width="50%">

### 🚗 Filo & Araç Yönetimi
- Araç envanteri ve evrak takibi (ruhsat, sigorta, muayene)
- Kilometre, yakıt, bakım ve servis geçmişi
- Araç alım/satım ve plaka dönüşüm süreçleri
- Canlı GPS takip, Geofence ve alarm altyapısı
- Komisyonculuk iş atamaları (Özmal / Kiralık / Komisyon)

### 👥 Personel & Bordro
- Personel özlük ve evrak merkezi
- Normal / AR-GE bordro ve hesap pusulası
- Puantaj, izin, avans/borç ve maaşa mahsup
- Banka ödeme listesi üretimi

### 💰 Muhasebe & Finans
- Cari hesap, ekstre ve mutabakat
- Fatura, proforma ve e-fatura (GİB) akışları
- Banka/kasa hareketleri ve ödeme eşleştirme
- Hesap planı, bütçe takvimi, hedef-gerçekleşen analizi

### 🏗️ İhale & Proje Yönetimi
- Proje bazlı maliyet ve AI destekli teklif tahmini
- Teklif versiyonlama ve onay akışı
- Rakip/piyasa benchmark karşılaştırması
- Kazanılan proje gerçekleşen maliyet takibi

</td>
<td valign="top" width="50%">

### 🧾 EBYS & Belge Yönetimi
- Gelen/giden evrak takibi ve atama iş akışı
- OCR, otomatik sınıflandırma ve özet oluşturma
- Vektör tabanlı semantik belge arama (Ollama)
- Versiyon geçmişi, geri yükleme ve karşılaştırma

### 📊 Raporlama & BI
- Araç kârlılık, yakıt verimlilik raporları
- Cari yaşlandırma ve ödeme raporları
- Excel (ClosedXML/EPPlus) ve PDF (QuestPDF) export
- CSV / JSON / Parquet veri dışa aktarım

### 🤖 AI & Otomasyon
- Fatura AI içe aktarımı ve sınıflandırma
- Araç değerleme ve piyasa araştırması
- OpenAI + Ollama (yerel LLM) entegrasyonu
- WhatsApp ve e-posta bildirim servisleri

### 🔐 Güvenlik & Çoklu Firma
- ASP.NET Core Identity + JWT Bearer
- Multi-tenant firma izolasyonu (Global Query Filters)
- IP beyaz/kara liste ve KVKK uyumluluk araçları
- Rol tabanlı yetkilendirme (Admin, Muhasebe, Operasyon)

</td>
</tr>
</table>

---

## 🧩 Mimari

```
┌─────────────────────────────────────────────────────────────────────┐
│                        KOAFiloServis.Web                            │
│          Blazor Server · REST API · SignalR Hubs · Quartz Jobs      │
│          Controllers · Services · Middleware · Background Jobs      │
└───────────┬────────────────────────────────────┬────────────────────┘
            │                                    │
            ▼                                    ▼
 ┌─────────────────────┐              ┌──────────────────────────┐
 │  KOAFiloServis.     │              │   KOAFiloServis.Shared   │
 │  LisansDesktop      │              │   Entity · DTO · Common  │
 │  DataSync (WinForms)│              └──────────────────────────┘
 └─────────────────────┘
            │
            ▼
 ┌──────────────────────────────────────────────────────────────┐
 │   PostgreSQL · SQLite · MySQL · SQL Server · Redis (Cache)   │
 └──────────────────────────────────────────────────────────────┘
```

### 📁 Çözüm Yapısı

| Proje | Açıklama | Framework |
|---|---|:---:|
| **`KOAFiloServis.Web`** | Ana Blazor Server uygulaması · REST API · SignalR · Quartz Jobs | `net10.0` |
| **`KOAFiloServis.Shared`** | Entity, DTO ve yardımcı sınıflar | `net10.0` |
| **`KOAFiloServis.Tests`** | xUnit birim ve entegrasyon testleri | `net10.0` |
| **`KOAFiloServis.LisansDesktop`** | HWID tabanlı lisans üretim/aktivasyon aracı (WinForms) | `net10.0-windows` |
| **`KOAFiloServis.DataSync`** | PostgreSQL → SQLite veri aktarım aracı (WinForms + CLI) | `net10.0-windows` |
| **`setup/`** | Inno Setup 6 paketleme betikleri ve IIS otomasyonu | — |

---
                │
                ▼
   ┌────────────────────────────────────────┐
   │  PostgreSQL / SQLite • Redis (opsiyonel)│
   └────────────────────────────────────────┘
```

### 📁 Çözüm Yapısı

| Proje | Açıklama | Hedef Framework |
|---|---|---|
| **`KOAFiloServis.Web`** | Ana Blazor Server uygulaması, API, Hub'lar, zamanlanmış işler | `net10.0` |
| **`KOAFiloServis.Shared`** | Ortak entity, DTO ve yardımcı bileşenler | `net10.0` |
| **`KOAFiloServis.Tests`** | xUnit birim/integrasyon testleri | `net10.0` |
| **`KOAFiloServis.LisansDesktop`** | Lisans üretim/aktivasyon aracı (WinForms) | `net10.0-windows` |
| **`KOAFiloServis.DataSync`** | PostgreSQL → SQLite veri aktarım aracı (WinForms + CLI) | `net10.0-windows` |
| **`setup/`** | Inno Setup 6 paketleme script'leri ve IIS otomasyonu | — |
| **`archive/`** | Eski kurulum artıfaktı (referans) | — |

---

## 🧰 Teknoloji Yığını

**Platform & Runtime**
- .NET 10, ASP.NET Core, Blazor Server
- SignalR (Hubs)

**Veri & Altyapı**
- Entity Framework Core 10 (Pooled DbContextFactory)
- PostgreSQL (Npgsql) / SQLite / MySQL / SQL Server (çoklu provider)
- Redis veya Memory Distributed Cache
- Quartz.NET zamanlanmış işler
- ASP.NET Core Data Protection

**Kimlik & Güvenlik**
- ASP.NET Core Identity
- JWT Bearer Authentication
- Multi-tenant firma izolasyonu

**Belge / Rapor / Entegrasyon**
- ClosedXML, EPPlus (Excel)
- QuestPDF (PDF)
- MailKit (SMTP)
- Parquet.Net (veri dışa aktarım)
- Microsoft.Extensions.AI + OllamaSharp (AI)
- Playwright, Selenium (web scraping / UI test)

**Test**
- xUnit, coverlet, Playwright, Selenium

---

## 🚀 Kurulum

### Gereksinimler
- ✅ [.NET 10 SDK](https://dotnet.microsoft.com/download)
- ✅ PostgreSQL 14+ (önerilen) *veya* SQLite
- ⚙️ (Opsiyonel) Redis 6+
- ⚙️ (Opsiyonel) Docker Desktop
- 🧰 Visual Studio 2022/2026 ya da VS Code + C# Dev Kit
- 🪟 Masaüstü araçlar için Windows 10/11

### Hızlı Başlangıç

```bash
# 1) Kaynak kodu al
git clone https://github.com/karamur/KOAFiloServis.git
cd KOAFiloServis

# 2) Paketleri geri yükle
dotnet restore

# 3) Derle
dotnet build -c Release

# 4) Çalıştır
dotnet run --project KOAFiloServis.Web
```

Uygulama başlatıldığında **5190** portundan itibaren boş bir port seçip `http://0.0.0.0:{port}` üzerinde dinlemeye başlar. Farklı bir port için:

```bash
dotnet run --project KOAFiloServis.Web --urls "http://0.0.0.0:8080"
```

### 🖥️ Son Kullanıcı Kurulumu (Windows)

1. [Releases](https://github.com/karamur/KOAFiloServis/releases) sayfasından son **`KOAFiloServisKurulum-<sürüm>.exe`** paketini indir.
2. **Yönetici olarak çalıştır.**
3. Bileşenleri seç (Web zorunlu, Lisans + DataSync opsiyonel) → IIS ve firewall görevlerini işaretli bırak.
4. Kurulum biter → tarayıcı: `http://localhost:5190`

**Gereksinimler (hedef PC):**
- Windows 10/11 x64
- IIS + ASP.NET Core 10 **Hosting Bundle** — [indir](https://dotnet.microsoft.com/download/dotnet/10.0)
- .NET 10 Desktop Runtime (Lisans + DataSync UI için)

**Güncelleme:** Yeni sürüm EXE'sini aynı PC'de çalıştırmanız yeter. `dbsettings.json`, `data\*.db`, `uploads\`, `logs\`, `Backups\` **korunur**.

Detaylı kurulum adımları için → [`setup/README.md`](setup/README.md)

---

## ⚙️ Yapılandırma

Temel ayarlar `KOAFiloServis.Web/appsettings.json` içinde bulunur.

### 🔌 Veritabanı Sağlayıcı Seçimi

```json
{
  "DatabaseProvider": "PostgreSQL",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=KOAFiloServisDb;Username=postgres;Password=***"
  }
}
```

Desteklenen değerler: `PostgreSQL`, `MySQL`, `SQLServer`, `SQLite`.

> 💡 `dbsettings.json` dosyası varsa **öncelikli** olarak o kullanılır. Kurulum aracı bu dosyayı otomatik üretir.

### 🔐 JWT

```json
{
  "Jwt": {
    "Secret": "En-Az-32-Karakterlik-Gizli-Anahtar",
    "Issuer": "KOAFiloServis",
    "Audience": "KOAFiloServis-API",
    "ExpirationHours": 24
  }
}
```

### 🗄️ Cache (Memory / Redis)

```json
{
  "Cache": {
    "Provider": "Memory",
    "Redis": { "ConnectionString": "localhost:6379", "InstanceName": "KOAFilo:" }
  }
}
```

### 🤖 AI (Opsiyonel)

```json
{
  "OpenAI": { "ApiKey": "", "Model": "gpt-4o-mini" },
  "Ollama": { "BaseUrl": "http://localhost:11434", "Model": "llama3.2" }
}
```

### 💾 Yedekleme

```json
{
  "Backup": {
    "Enabled": true,
    "Path": "backups",
    "RetentionDays": 30,
    "ScheduleHour": 3
  }
}
```

> ⚠️ Üretimde hassas bilgileri (parola, anahtar) **ortam değişkenleri** veya **User Secrets / Azure Key Vault** ile yönetin.

---

## 🗃️ Veritabanı Migrasyonu

```bash
dotnet ef database update --project KOAFiloServis.Web
```

Yeni migration ekleme:

```bash
dotnet ef migrations add MigrationAdi --project KOAFiloServis.Web
```

---

## 🧪 Test

```bash
# Tüm testleri çalıştır
dotnet test

# Kapsama (coverage) ile
dotnet test --collect:"XPlat Code Coverage"
```

- **Birim/Integrasyon:** `KOAFiloServis.Tests` (xUnit)
- **E2E Smoke:** `KOAFiloServis.PlaywrightSmoke`
- **UI Regression:** `KOAFiloServis.SeleniumTests`

---

## 📦 Yayınlama

### Tek Komutla Installer Üretme

```powershell
cd setup
.\build.ps1 -Version 1.0.2 -CopyToPublish
```

Çıktı:
- `setup\output\KOAFiloServisKurulum-1.0.2.exe`
- (opsiyonel) `F:\publish\Installer\KOAFiloServisKurulum-1.0.2.exe`

Pipeline: Web publish → LisansDesktop publish (SingleFile, self-contained) → DataSync publish → **Inno Setup 6** derleme.

### Manuel Web Publish (IIS / Linux)
```bash
dotnet publish KOAFiloServis.Web -c Release -o ./publish/web
```

### Masaüstü Araçları Manuel
```bash
dotnet publish KOAFiloServis.LisansDesktop -c Release
dotnet publish KOAFiloServis.DataSync      -c Release
```

Her iki araç da `PublishSingleFile + SelfContained (win-x64)` olarak paketlenir.

---

## 🔐 Lisans Yönetimi (KOAFiloServis.LisansDesktop)

HWID (donanım parmak izi) bazlı offline lisans aktivasyonu:

- Müşteri PC'sinde **HWID** üretilir → satıcı tarafına gönderilir.
- Satıcı LisansDesktop ile imzalı `lisans.key` üretir → müşteriye gönderilir.
- Web uygulaması `lisans.key`'i doğrular; HWID uyuşmazsa reddeder.

Kurulum sonrası Başlat Menüsü: **KOAFiloServis → Lisans Yonetimi**

---

## 💾 Veritabanı Yedekleme & Geri Yükleme

### Otomatik Yedek (Quartz Job)
`appsettings.json` → `Backup` bloğu:

```json
{
  "Backup": {
    "Enabled": true,
    "Path": "Backups",
    "RetentionDays": 30,
    "ScheduleHour": 3
  }
}
```

- Her gün 03:00'te PostgreSQL/SQLite tam yedek.
- 30 günden eski yedekler otomatik temizlenir.
- Varsayılan konum: `C:\KOAFiloServis\Backups\YYYY-MM-DD\`

### PG → SQLite Veri Aktarımı (KOAFiloServis.DataSync)

Offline/test PC'lere canlı PostgreSQL verisini taşımak için:

**UI:** Başlat Menüsü → KOAFiloServis → *Veri Aktarim (PG - SQLite)* → Host/Port/DB/User/Pass gir → **Test** → **Başlat**.

**CLI (otomasyon):**
```powershell
& "C:\KOAFiloServis\DataSync\KOAFiloServis.DataSync.exe" export `
    --pg  "Host=10.0.0.5;Port=5432;Database=koa;Username=postgres;Password=***" `
    --sqlite "C:\KOAFiloServis\data\koa.db"
```

Kaynak PostgreSQL'den tüm tabloları (146+ DbSet) ortak şema temelinde hedef SQLite'a kopyalar.

### Manuel Geri Yükleme
- **PostgreSQL:** `psql -U postgres -d KOAFiloServisDb -f backup.sql`
- **SQLite:** `Backups\koa-YYYYMMDD.db` → `C:\KOAFiloServis\data\koa.db` (Web durdurulmuşken)

---

## 🔌 Modüller & Sayfalar (Kısaca)

`KOAFiloServis.Web/Components/Pages` altında **200+ Blazor sayfası** organize edilmiştir:

- `Araclar`, `AracMasraflari`, `AracTakip`, `Guzergahlar`
- `Cariler`, `Faturalar`, `EFatura`, `BankaHesaplari`, `BankaHareketleri`
- `Muhasebe`, `Butce`, `Budget`, `Hakedis`
- `Personel`, `Bordro`, `EBYS`
- `FiloOperasyon`, `IlanYayin`, `Ihale`, `Stok`
- `CRM`, `DestekTalepleri`, `Bildirimler`
- `Raporlar`, `Ayarlar`

---

## 🛡️ Güvenlik

- 🔒 Hassas yapılandırmayı kaynak kodla **paylaşmayın**.
- 🔑 JWT `Secret` değeri **en az 32 karakter** olmalı ve çevreye özgü üretilmelidir.
- 🌐 Üretimde **HTTPS** zorunlu kılın ve güvenli cookie / HSTS ayarlarını etkinleştirin.
- 🧪 NuGet paketlerini düzenli güncelleyin; güvenlik uyarılarını `dotnet list package --vulnerable` ile izleyin.
- 🧰 Data Protection anahtarları `AppStoragePaths.GetDataProtectionKeysRoot` altında saklanır; yedekleyin.

---

## 🗺️ Yol Haritası

Detaylı yol haritası için [`ROADMAP.md`](ROADMAP.md), kurulum için [`INSTALL.md`](INSTALL.md) ve [`KURULUM_REHBERI.md`](KURULUM_REHBERI.md), geliştirme notları için [`DEVELOPMENT.md`](DEVELOPMENT.md) dosyalarına bakınız.

---

## 🤝 Katkı

1. Projeyi fork'layın
2. Yeni bir özellik dalı oluşturun: `git checkout -b feature/harika-ozellik`
3. Değişikliklerinizi commit'leyin: `git commit -m "feat: harika özellik"`
4. Dalı push'layın: `git push origin feature/harika-ozellik`
5. Bir **Pull Request** açın

Lütfen PR açmadan önce:
- `dotnet build` başarılı olmalı
- Yeni kod için uygun testler eklenmeli
- Mevcut kod stiline (`.editorconfig`) uyulmalı

---

## 📄 Lisans

Bu proje **Allbatros Global Teknoloji** tarafından geliştirilmekte olup ticari kullanım **yazılı izne** tabidir. Lisans ve iş birliği talepleri için iletişime geçin.

---

## 📬 İletişim

- 🏢 **Allbatros Global Teknoloji**
- 🌐 [www.allbatros.com](https://www.allbatros.com)
- 🐙 [github.com/karamur](https://github.com/karamur)
- 📦 Repo: [github.com/karamur/KOAFiloServis](https://github.com/karamur/KOAFiloServis)

---

<div align="center">

⭐ Beğendiyseniz **star** vermeyi unutmayın!

<sub>© 2024–2026 Allbatros Global Teknoloji — Tüm hakları saklıdır.</sub>

</div>

