<div align="center">

<img src="https://img.shields.io/badge/-KOA%20Filo%20Servis-1f6feb?style=for-the-badge&logo=bus&logoColor=white" alt="KOA Filo Servis" />

# 🚍 KOA Filo Servis

**Kurumsal Personel Servis Taşımacılığı & Filo Yönetim Platformu**

_Filo · Operasyon · Hakediş · Muhasebe · EBYS — tek panelden, uçtan uca._

<br />

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com)
[![Blazor](https://img.shields.io/badge/Blazor-Interactive%20Server-512BD4?style=flat-square&logo=blazor&logoColor=white)](https://learn.microsoft.com/aspnet/core/blazor/)
[![EF Core 10](https://img.shields.io/badge/EF%20Core-10.0-68217A?style=flat-square&logo=microsoft&logoColor=white)](https://learn.microsoft.com/ef/core/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-14%2B-336791?style=flat-square&logo=postgresql&logoColor=white)](https://www.postgresql.org)
[![Quartz.NET](https://img.shields.io/badge/Quartz.NET-3.x-FB7A24?style=flat-square)](https://www.quartz-scheduler.net)
[![License](https://img.shields.io/badge/License-Proprietary-red?style=flat-square)](#lisans)
[![Version](https://img.shields.io/badge/Version-1.0.21-success?style=flat-square)](CHANGELOG.md)

</div>

---

## 📚 İçindekiler

- [Proje Hakkında](#proje-hakkinda)
- [Öne Çıkan Yetenekler](#one-cikan-yetenekler)
- [Mimari Genel Bakış](#mimari-genel-bakis)
- [Proje Yapısı](#proje-yapisi)
- [Teknoloji Yığını](#teknoloji-yigini)
- [Güzergah ve Sefer Yönetimi](#guzergah-ve-sefer-yonetimi)
- [Kurulum](#kurulum)
- [Geliştirme Ortamı](#gelistirme-ortami)
- [Migration Stratejisi](#migration-stratejisi)
- [Test Stratejisi](#test-stratejisi)
- [Deploy](#deploy)
- [Güvenlik](#guvenlik)
- [Yol Haritası](#yol-haritasi)
- [Katkıda Bulunma](#katki)
- [Sürüm Geçmişi](#surum-gecmisi)
- [Lisans](#lisans)

---

## 🎯 Proje Hakkında

**KOA Filo Servis**, kurumsal personel servis taşımacılığı sektörüne yönelik uçtan uca filo ve operasyon yönetim platformudur.

Onlarca güzergah, yüzlerce araç, binlerce personel ve çok sayıda taşıma tedarikçisi olan büyük ölçekli işletmelerin ihtiyaçlarına yanıt verir. Günlük puantajdan aylık hakedişe, araç kira takibinden EBYS entegrasyonuna kadar tüm operasyon tek panelden yönetilir.

> **Bir veri, bir kez girilir — platform geri kalanını halleder.**

---

## ✨ Öne Çıkan Yetenekler

| Modül | Açıklama |
|---|---|
| 🗺️ Güzergah Yönetimi | Tanımlama, harita koordinatları, sefer detayları düzenleme ekranında |
| 🚌 Araç Yönetimi | Özmal / tedarikçi / kiralık takibi, plaka geçmişi, aktif atamalar |
| 👤 Şoför Yönetimi | Şoför kartı, ehliyet, firma bağlantısı |
| 📅 Puantaj | Günlük/aylık giriş, toplu onay, kurum bazlı görünüm |
| 💰 Hakediş | Operasyonel, tedarikçi ve araç hakediş otomasyonu |
| 🏢 Kurum ve Cari | Kurum–Cari bağlantısı, çok kurum desteği |
| 🔧 Destek Modülü | Servis geçmişi, bakım takvimi, lastik takibi |
| 📄 EBYS | Belge yönetimi, şifreli dosya saklama |
| 📊 Raporlama | Maliyet snapshot, hakediş raporları, özel filtreler |
| 🔐 Kimlik ve Yetki | RBAC, 2FA altyapısı, aktivite logu |
| ⏰ Zamanlayıcı | Quartz.NET tabanlı planlı görevler |

---

## 🏛️ Mimari Genel Bakış

```
Blazor Server UI (.razor — InteractiveServer)
        |
Application Services (DI)
GuzergahService · PuantajService · HakedisService · AracService ...
        |
ApplicationDbContext (IDbContextFactory — scoped per circuit)
Global Query Filters — FirmaId tenant izolasyonu
        |
   PostgreSQL 14+ (üretim) / SQLite (geliştirme)
```

### Multi-Tenant Modeli

- Her entity **IFirmaTenant** implemente eder (FirmaId kolonu).
- DbContext global query filter ile aktif firmaya ait kayıtları otomatik filtreler.
- Blazor devrelerinde tenant kimliği **IAktifFirmaProvider** ile çözülür.
- **TenantAwareDbContextFactory** — Blazor circuit scope uyumlu, scoped provider kullanır.

---

## 📁 Proje Yapısı

```
KOAFiloServis/
├── KOAFiloServis.Shared/          # Entity modeller, interfaceler, DTOlar
├── KOAFiloServis.Web/             # Blazor Server uygulaması
│   ├── Components/Pages/          # Guzergahlar, Araclar, Soforler, Puantaj ...
│   ├── Data/                      # ApplicationDbContext, Migrations
│   ├── Services/                  # Servis implementasyonları
│   └── wwwroot/                   # CSS, JS, statik dosyalar
├── KOAFiloServis.Tests/           # xUnit + Playwright testleri
├── setup/                         # WiX kurulum paketi
├── scripts/                       # Deploy / yedekleme scriptleri
├── CHANGELOG.md
├── ROADMAP.md
└── README.md
```

---

## 🛠️ Teknoloji Yığını

| Katman | Teknoloji |
|---|---|
| Framework | .NET 10 / ASP.NET Core 10 |
| UI | Blazor Server (InteractiveServer) |
| ORM | Entity Framework Core 10 |
| Veritabanı | PostgreSQL 14+ / SQLite |
| Kimlik | ASP.NET Core Identity |
| Zamanlayıcı | Quartz.NET 3.x |
| Harita | Leaflet.js (JS interop) |
| UI Bileşenler | Bootstrap 5, Bootstrap Icons |
| Test | xUnit, Microsoft.Playwright, Selenium |
| Kurulum | WiX Toolset v4 |

---

## 🗺️ Güzergah ve Sefer Yönetimi

> v1.0.21 ile sefer yönetimi doğrudan **Güzergah Düzenle** kartına taşındı.

**Güzergah Listesi** — Sadece güzergah bilgileri: kod, ad, kurum, kapasite, fiyatlar, koordinat durumu, aktif/pasif.

**Güzergah Düzenle Formu** — Tüm detay burada:

- Temel bilgiler (ad, kod, kurum, cari, fiyat, mesafe, süre, notlar)
- Harita koordinatları (Leaflet entegrasyonu)
- Seferler Tablosu: sefer sayısı → her sefer için Sefer Tipi / Kapasite / Araç / Şoför / Telefon / Firma
- _1. Seferi Tüm Seferlere Uygula_ — tek tıkla tekrarlı seferleri doldurur
- Araç seçiminde şoför ve firma bilgisi aktif personel atamasından otomatik doldurulur
- Güzergah kaydedildiğinde tüm sefer kayıtları birlikte persist edilir

| Alan | Açıklama |
|---|---|
| GuzergahId | Güzergah FK |
| SeferTipi | Sabah / Akşam / SabahAkşam / Saatlik |
| KapasiteAdi | 16+1, 28+1 vb. |
| AracId | Araç FK (nullable) |
| SoforAd | Otomatik veya manuel |
| SoforTelefon | Serbest metin |
| FirmaAdiSerbest | Tedarikçi / özmal firma |
| Sira | Görüntülenme sırası |

---

## 🚀 Kurulum

| Gereksinim | Sürüm |
|---|---|
| .NET SDK | 10.0.x |
| PostgreSQL | 14.x |
| Node.js | 18+ (Playwright için) |
| Windows | 10 / Server 2019+ |

```pwsh
git clone https://github.com/karamur/KOAFiloServis.git
cd KOAFiloServis
# appsettings.Development.json — DefaultConnection ayarla
dotnet ef database update --project KOAFiloServis.Web
dotnet run --project KOAFiloServis.Web
# https://localhost:5200
```

Windows kurulum paketi:

```pwsh
.\setupolustur.bat
# Çıktı: setup\output\v1.0.21\KOAFiloServisKurulum-1.0.21.exe
```

---

## 💻 Geliştirme Ortamı

```pwsh
dotnet build KOAFiloServis.Web/KOAFiloServis.Web.csproj
dotnet watch --project KOAFiloServis.Web
dotnet ef migrations add <MigrationAdi> --project KOAFiloServis.Web
dotnet ef database update --project KOAFiloServis.Web
```

| Ayar | Açıklama |
|---|---|
| ConnectionStrings:DefaultConnection | PostgreSQL bağlantı dizesi |
| AppSettings:DbProvider | postgresql veya sqlite |
| AppSettings:Port | HTTP port (varsayılan 5200) |

---

## 🗄️ Migration Stratejisi

İdempotent PL/pgSQL yaklaşımı — aynı migration birden fazla kez güvenle çalışır.

```sql
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'Guzergahlar' AND column_name = 'YeniKolon'
    ) THEN
        ALTER TABLE "Guzergahlar" ADD COLUMN "YeniKolon" text NULL;
    END IF;
END $$;
```

> ⚠️ Yıkıcı migrationlardan önce mutlaka yedek alın.

```pwsh
pg_dump -h <host> -U <user> -d KOAFiloServisV2 -F c -f "backup.dump"
```

---

## 🧪 Test Stratejisi

| Tür | Çatı | Kapsam |
|---|---|---|
| Birim | xUnit | Servis kuralları, hesaplama |
| Entegrasyon | xUnit + EF InMemory | Repository + servis zinciri |
| E2E | Microsoft.Playwright | Kritik akışlar |
| Smoke | Selenium | Tarayıcı uyumluluk |

```pwsh
dotnet test --filter "Category=Unit"
pwsh KOAFiloServis.Tests/bin/Debug/net10.0/playwright.ps1 install
dotnet test --filter "Category=E2E"
```

---

## 📦 Deploy

```pwsh
.\scripts\deploy-iis-local.ps1
```

Varsayılan port: **5200** — Sürüm notları: [CHANGELOG.md](CHANGELOG.md) — Yol haritası: [ROADMAP.md](ROADMAP.md)

---

## 🔐 Güvenlik

- ASP.NET Core Identity tabanlı kullanıcı/rol yönetimi
- Permissions + RolePermissions ile RBAC
- JWT tabanlı API erişimi
- SecureFileService ile diskte şifreli belge saklama
- AuditLogService ile kapsamlı aktivite logu
- 2FA altyapısı hazır
- Soft-delete ile kalıcı kayıp önlenir
- FirmaId global query filter ile tenant izolasyonu

> 🐞 Güvenlik açığı için e-posta ile iletişime geçin — public issue açmayın.

---

## 🗺️ Yol Haritası

Detaylar: [ROADMAP.md](ROADMAP.md)

- [ ] 📱 Mobil (MAUI) şoför uygulaması
- [ ] 📊 PowerBI için OData uçları
- [ ] 🌐 Tedarikçi self-servis portalı
- [ ] 🧾 SAP / e-Fatura entegrasyonu
- [ ] 🤖 AI tabanlı puantaj anomali tespiti
- [ ] 🌍 Çoklu dil desteği (i18n)
- [ ] 📧 E-posta bildirim sistemi
- [ ] 🔄 REST API (mobil ve harici entegrasyon)

---

## 🤝 Katkıda Bulunma

Bu repo özel bir projeye aittir. Katkı için önce _issue_ açın.

```pwsh
git checkout -b feature/yeni-ozellik
dotnet build && dotnet test
git push origin feature/yeni-ozellik
```

| Tip | Anlam |
|---|---|
| feat | Yeni özellik |
| fix | Hata düzeltme |
| refactor | Davranış değiştirmeyen iyileştirme |
| tenant | Multi-tenant göç adımı |
| docs | Dokümantasyon |
| build / chore | Build/CI/setup |
| test | Test ekleme/güncelleme |

---

## 📋 Sürüm Geçmişi

Tüm detaylar: [CHANGELOG.md](CHANGELOG.md)

### v1.0.21 — Güzergah Sefer Yönetimi

- Sefer yönetimi güzergah düzenleme kartına taşındı; liste sadeleşti
- Güzergah kaydedildiğinde sefer kayıtları otomatik persist edilir
- Araç seçiminde şoför/firma otomatik dolduruluyor
- GuzergahList: sefer detay paneli kaldırıldı

### v1.0.20 — Tenant Migrasyonu ve Temizlik

- Legacy Sirket mimarisi kaldırıldı (~1470 satır)
- FirmaId tabanlı tenant izolasyonuna geçiş tamamlandı
- TenantAwareDbContextFactory Blazor scope hatası düzeltildi
- Destek modülü Npgsql async reader hatası düzeltildi
- Port 5200 olarak güncellendi

---

## 📄 Lisans

© **Karamur Yazılım**. Tüm hakları saklıdır.

Bu yazılım yalnızca lisanslı kullanım için sunulur; izinsiz kopyalanması, dağıtılması veya türev çalışma üretilmesi yasaktır.

---

<div align="center">

**KOA Filo Servis**

_Operasyondan muhasebeye, filodan hakedişe — tek panelden uçtan uca yönetim._

<sub>Made with ❤️ on .NET 10 and Blazor</sub>

</div>