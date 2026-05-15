<div align="center">

# 🚍 KOA Filo Servis

**Kurumsal Personel Servis Taşımacılığı ve Filo Yönetim Platformu**

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com)
[![Blazor](https://img.shields.io/badge/Blazor-Interactive%20Server-512BD4?logo=blazor&logoColor=white)](https://learn.microsoft.com/aspnet/core/blazor/)
[![EF Core](https://img.shields.io/badge/EF%20Core-10.0-68217A?logo=microsoft&logoColor=white)](https://learn.microsoft.com/ef/core/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Npgsql-336791?logo=postgresql&logoColor=white)](https://www.postgresql.org)
[![License](https://img.shields.io/badge/License-Proprietary-red.svg)](#-lisans)

_Filo, personel servisleri, hakediş, puantaj, evrak ve mali süreçleri tek çatı altında toplayan uçtan uca işletme yönetim sistemi._

</div>

---

## 📌 Proje Hakkında

**KOA Filo Servis**, personel taşımacılığı yapan firmalar için tasarlanmış, kurumsal seviyede çoklu modülden oluşan bir **Blazor (Interactive Server)** uygulamasıdır. Araç, şoför, güzergah ve müşteri (kurum) verilerinden başlayıp **günlük puantaj → hakediş → fatura → muhasebe** zincirini tek bir akışta yönetir. Tedarikçi araç, kiralık plaka, evrak süresi takibi ve belge uyarıları gibi operasyonel ihtiyaçlar yerleşik olarak gelir.

> Çok şirketli (multi-tenant) altyapı, rol bazlı yetkilendirme, EBYS arşivi ve AI destekli arama/değerleme servisleri ile sahada gerçek operasyon yüküne göre ölçeklenecek şekilde kurgulanmıştır.

---

## ✨ Öne Çıkan Yetenekler

### 🚐 Filo & Araç Yönetimi
- Şase numarası bazında tekil araç kartı, **plaka geçmişi** ve sahiplik tipleri (Özmal / Kiralık / Tedarikçi).
- Kiralık ve komisyonlu araçlar için detaylı kira/komisyon hesaplama tipleri.
- Araç evrakları (ruhsat, sigorta, kasko, muayene, koltuk sigortası, yetki belgesi, emisyon vb.) ve **çok versiyonlu dosya arşivi**.
- Evrak bitiş tarihleri **tek noktadan tekilleştirilir** — bir kez güncellendiğinde uyarılar, raporlar ve listelere yansır.

### 👥 Personel & Şoför Operasyonu
- Şoför özlük, ehliyet, MYK, psikoteknik, sağlık raporu takibi.
- Personel-araç atama, izin, devamsızlık ve performans takibi.
- Tedarikçi personeli için ayrı işleyiş (alt yüklenici takibi).

### 🛣️ Güzergah & Puantaj
- Kurum/Cari ayrı kavramlar olarak modellenir; bir Kurum Cari'siz de güzergah açabilir.
- Hiyerarşik **Güzergah → Araç → Günlük Satır** puantaj ekranı, ay filtresi ve **rota bazlı toplu onay**.
- Otomatik puantaj üretimi: varsayılan araç/şoför ve tedarikçi bilgilerini şablonlar üzerinden doldurur.

### 💰 Hakediş, Fatura & Muhasebe
- Güzergah-araç eşleştirmelerinden türeyen **Hakediş** ekranı; sütun bazlı filtreler, gelir/gider özeti, detay & puantaj geçişi.
- Fatura kalemleri, tahsilat, banka/kasa hareketleri, masraflar ve mali analiz.
- Aylık ve dönemsel **Excel/PDF rapor** çıktıları.

### 🗂️ Belge Yönetim Sistemi (EBYS)
- Gelen / Giden / Personel Özlük / Araç Evrak başta olmak üzere belge kategorileri.
- AI destekli **belge tipi tanıma** ve **semantik arama**.
- Versiyonlu dosya saklama, şifreli güvenli depolama (`SecureFileService`).

### 🔔 Belge & Sözleşme Uyarı Sistemi
- Araç evrakları, şoför belgeleri, tedarikçi sözleşmeleri ve kiralık plakalar için **merkezi uyarı paneli**.
- Background job tabanlı periyodik tarama ve bildirim üretimi.

### 🤖 AI / Otomasyon
- Araç piyasa araştırma ve değerleme servisleri (Ollama / Microsoft.Extensions.AI entegrasyonu).
- Belge AI servisi: arşivde içerik bazlı arama ve sınıflandırma.
- Otomatik veri senkronizasyon servisi (`KOAFiloServis.DataSync`).

### 🛡️ Kurumsal Altyapı
- **Multi-tenant** Şirket bazlı veri ayrımı.
- Detaylı **rol & yetki** (`Permissions`, `RolePermissions`, menü-bazlı erişim).
- Aktivite logları, oturum izleme, JWT ile API erişimi.
- Quartz.NET tabanlı zamanlanmış işler.

---

## 🏗️ Mimari ve Teknolojiler

| Katman | Teknoloji |
| --- | --- |
| UI | **Blazor Interactive Server** (.NET 10), Bootstrap 5, Bootstrap Icons |
| Backend | ASP.NET Core 10, Razor Components, REST API Controllers |
| Veri Erişimi | **Entity Framework Core 10** (PostgreSQL · SQL Server · SQLite · MySQL) |
| Cache | StackExchange Redis (opsiyonel) + InMemory |
| Arka Plan İşler | **Quartz.NET 3** Hosting |
| Belge | ClosedXML / EPPlus / QuestPDF |
| Mail | MailKit |
| AI | Microsoft.Extensions.AI · OllamaSharp |
| Test | xUnit · Microsoft.Playwright · Selenium |
| Lisans Modülü | WinForms Desktop (`KOAFiloServis.LisansDesktop`) |

### Çözüm Yapısı

```
KOAFiloServis.sln
├── KOAFiloServis.Web/           # Ana Blazor uygulaması (UI + API + servisler)
│   ├── Components/Pages/        # Tüm modüllerin Razor sayfaları
│   ├── Services/                # İş kuralları & EF Core servisleri
│   ├── Controllers/             # REST API uçları
│   ├── Data/Migrations/         # EF Core migration tarihçesi
│   └── Jobs/                    # Quartz background jobs
├── KOAFiloServis.Shared/        # Domain entity'leri ve DTO'lar
├── KOAFiloServis.DataSync/      # Otomatik veri senkron servisi
├── KOAFiloServis.LisansDesktop/ # Masaüstü lisans yöneticisi
└── KOAFiloServis.Tests/         # xUnit & entegrasyon testleri
```

### Akış (Yüksek Düzey)

```text
   Kurum / Müşteri ──► Güzergah ──► Araç + Şoför Eşleşmesi
                                          │
                                          ▼
                                  Günlük Puantaj
                                          │
                          ┌───────────────┼─────────────────┐
                          ▼               ▼                 ▼
                     Toplu Onay      Hakediş          Fatura / Muhasebe
                                          │
                                          ▼
                                Belge Uyarıları & Raporlar
```

---

## 🚀 Hızlı Başlangıç

### Önkoşullar

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL 14+ (varsayılan); SQL Server / SQLite / MySQL alternatif
- (Opsiyonel) Redis 7+, Ollama / Azure OpenAI

### Adımlar

```pwsh
# 1) Repoyu klonla
git clone https://github.com/karamur/KOAFiloServis.git
cd KOAFiloServis

# 2) Bağımlılıkları yükle ve derle
dotnet restore
dotnet build

# 3) Veritabanı bağlantısını yapılandır
#    KOAFiloServis.Web/appsettings.json içindeki "ConnectionStrings" düzenleyin

# 4) Veritabanı şemasını oluştur (EF Core)
dotnet ef database update --project KOAFiloServis.Web

# 5) Uygulamayı çalıştır
dotnet run --project KOAFiloServis.Web
```

Uygulama varsayılan olarak `https://localhost:5001` adresinde başlar.

### Test Çalıştırma

```pwsh
dotnet test KOAFiloServis.Tests/KOAFiloServis.Tests.csproj
```

---

## 📸 Modül Haritası

| Modül | Açıklama |
| --- | --- |
| **Filo Yönetimi** | Araç kartları, plaka geçmişi, evraklar, bakım periyodu, alım/satım |
| **Güzergah & Puantaj** | Güzergah CRUD, eşleştirme, günlük puantaj, toplu onay |
| **Hakediş** | Filtreli liste, sütun bazlı arama, detay, fatura/puantaj geçişi |
| **Cari & Kurum** | Müşteri kartları, kurum-cari ayrı yönetim, sözleşme takibi |
| **Tedarikçi Operasyon** | Taşıma tedarikçisi yönetimi, tedarikçi araç & personel |
| **Stok & Bakım** | Lastik, parça, depo, bakım planı |
| **Mali & Muhasebe** | Banka/kasa, masraf, fatura, mali analiz, kârlılık raporları |
| **EBYS** | Belge arşivi, AI tabanlı arama, semantik arama |
| **Uyarılar** | Belge/sözleşme/plaka süre uyarıları |
| **Yönetim** | Kullanıcı, rol, yetki, şirket, parametre yönetimi |

---

## 🔐 Güvenlik

- ASP.NET Core Identity tabanlı kullanıcı/rol modeli + özel `AppAuthenticationStateProvider`.
- `Permissions` ve `RolePermissions` ile menü ve aksiyon bazlı yetkilendirme.
- JWT tabanlı API erişimi.
- `SecureFileService` ile **diskte şifrelenmiş** belge saklama.
- Kapsamlı **aktivite logu** (görüntüleme, oluşturma, düzenleme, silme).

---

## 🗺️ Yol Haritası

- [ ] Mobil (MAUI) şoför uygulaması
- [ ] PowerBI bağlantısı için OData uçları
- [ ] Daha kapsamlı tedarikçi self-servis portalı
- [ ] Otomatik fatura SAP/e-Fatura entegrasyonu
- [ ] Gelişmiş AI tabanlı puantaj anomali tespiti

---

## 🤝 Katkıda Bulunma

Bu repo özel bir projeye aittir. Katkı talepleri için lütfen önce bir _issue_ açarak iletişime geçin.

```pwsh
git checkout -b feature/yeni-ozellik
git commit -m "feat: yeni özellik"
git push origin feature/yeni-ozellik
```

---

## 📄 Lisans

© Karamur Yazılım. Tüm hakları saklıdır. Bu yazılım yalnızca lisanslı kullanım için sunulur; izinsiz kopyalanması, dağıtılması veya türev çalışma üretilmesi yasaktır.

---

<div align="center">

**KOA Filo Servis** — _Operasyondan muhasebeye, filodan hakedişe tek panelden yönetim._

</div>
