<div align="center">

# 🚚 KOAFiloServis

**Kurumsal Filo Yönetimi, Servis ve ERP Platformu**

.NET 10 • Blazor Server • PostgreSQL • Modüler ERP

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-Server-512BD4?style=flat-square&logo=blazor&logoColor=white)](https://blazor.net/)
[![EF Core](https://img.shields.io/badge/EF%20Core-10-68217A?style=flat-square)](https://learn.microsoft.com/ef/core/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-14+-4169E1?style=flat-square&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Quartz](https://img.shields.io/badge/Quartz-Scheduler-orange?style=flat-square)](https://www.quartz-scheduler.net/)
[![Build](https://img.shields.io/badge/build-passing-brightgreen?style=flat-square)](#)
[![License](https://img.shields.io/badge/license-Proprietary-red?style=flat-square)](#-lisans)

<sub>Taşımacılık ve filo operasyonları için uçtan uca dijital yönetim çözümü.</sub>

[✨ Özellikler](#-öne-çıkan-özellikler) • [🧩 Mimari](#-mimari) • [🚀 Kurulum](#-kurulum) • [⚙️ Yapılandırma](#️-yapılandırma) • [🧪 Test](#-test) • [📦 Yayınlama](#-yayınlama) • [🛡️ Güvenlik](#️-güvenlik)

</div>

---

## 📖 Genel Bakış

**KOAFiloServis**, filo ve taşımacılık şirketlerinin günlük operasyonlarını tek platformdan yönetebilmesi için geliştirilmiş, **modüler** ve **çok firmalı (multi-tenant)** bir kurumsal çözümdür. Araç, sürücü, servis, cari, muhasebe, bordro, ihale ve raporlama süreçlerini modern bir **Blazor Server** arayüzü ile sunar; zamanlanmış işler, yedekleme, AI destekli analiz ve masaüstü kurulum/lisans araçları ile paketlenmiştir.

> 🎯 Hedef kitle: filo yönetim şirketleri, lojistik firmaları, servis/operasyon ekipleri, finans/muhasebe departmanları.

---

## ✨ Öne Çıkan Özellikler

### 🚗 Filo & Araç Yönetimi
- Araç envanteri, evrak/belge takibi (ruhsat, sigorta, muayene)
- Kilometre, yakıt ve bakım/servis takibi
- Araç alım/satım, plaka dönüşümü, komisyonculuk süreçleri
- Canlı araç takip altyapısı ve GPS simülasyon

### 👥 Personel & Bordro
- Personel özlük ve evrak merkezi
- Normal / AR-GE bordro, hesap pusulası, bordro icmali
- Puantaj, izin, avans/borç takibi
- Banka ödeme listesi üretimi

### 💰 Muhasebe & Finans
- Cari hesap, cari mutabakat ve risk analizi
- Fatura, proforma, e-fatura akışları
- Banka/kasa hareketleri, ödeme eşleştirme
- Hesap planı, bütçe takvimi, hedef–gerçekleşen analizi

### 📊 Raporlama & BI
- Araç kârlılık, masraf, yakıt verimlilik raporları
- Cari ekstre, yaşlandırma ve ödeme raporları
- Dashboard grafik bileşenleri
- Excel (ClosedXML/EPPlus) ve PDF (QuestPDF) dışa aktarım

### 🧾 EBYS & Belge Yönetimi
- Gelen/giden evrak takibi ve atama iş akışı
- Evrak kategorileri, gelişmiş arama ve detay görüntüleme
- Çoklu dosya yükleme ve versiyon geçmişi (asıl nüsha takibi)
- Kullanıcı/departman bazlı evrak atama ve hareket geçmişi
- **Dashboard entegrasyonu:** Giriş yapan kullanıcıya atanmış bekleyen evraklar için uyarı banner'ı (acil/gecikmiş ayırımı) ve hızlı erişim modal'ı
- Belge uyarı ve hatırlatma motoru (ruhsat, sigorta, muayene vb.)

### 🤖 AI & Otomasyon
- Fatura AI içe aktarımı
- Araç değerleme ve piyasa araştırma (HTTP + Playwright scraper)
- OpenAI ve Ollama (local LLM) entegrasyonu
- WhatsApp bildirimleri, e-posta bildirim servisi

### 🕒 Zamanlanmış İşler
- Otomatik veritabanı yedekleme
- Belge uyarı kontrolü
- Quartz tabanlı job altyapısı

### 🔐 Güvenlik & Çoklu Firma
- ASP.NET Core Identity + JWT
- Multi-tenant (firma bazlı izolasyon)
- Data Protection anahtar yönetimi
- Rol/yetki tabanlı erişim

---

## 🧩 Mimari

```
┌──────────────────────────────────────────────────────────────┐
│                    KOAFiloServis.Web                         │
│   (Blazor Server • Controllers • Hubs • Jobs • Services)     │
└───────────────┬───────────────────────────┬──────────────────┘
                │                           │
                ▼                           ▼
   ┌────────────────────┐       ┌────────────────────────┐
   │  KOAFiloServis.    │       │  KOAFiloServis.Shared  │
   │  LisansDesktop /   │       │  (Entity, DTO, Common) │
   │  DataSync          │       └────────────────────────┘
   │  (WinForms Tools)  │
   └────────────────────┘
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
