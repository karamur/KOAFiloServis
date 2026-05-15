---
marp: true
theme: default
paginate: true
size: 16:9
header: "KOA Filo Servis · Kurumsal Sunum"
footer: "© Karamur Yazılım · Tüm hakları saklıdır"
style: |
  section {
    background: linear-gradient(135deg, #ffffff 0%, #f1f5f9 100%);
    color: #0f172a;
    font-family: "Segoe UI", "Inter", sans-serif;
  }
  h1, h2, h3 { color: #1e3a8a; }
  section.lead {
    background: linear-gradient(135deg, #1e3a8a 0%, #2563eb 60%, #38bdf8 100%);
    color: #f8fafc;
    text-align: center;
  }
  section.lead h1 { color: #ffffff; font-size: 3rem; }
  section.lead h2 { color: #e0f2fe; font-weight: 400; }
  table { width: 100%; border-collapse: collapse; }
  th { background: #1e3a8a; color: #ffffff; }
  td, th { padding: 8px 12px; border: 1px solid #cbd5e1; }
  blockquote { border-left: 6px solid #2563eb; background: #eff6ff; padding: 8px 16px; }
  code { background: #0f172a; color: #f8fafc; padding: 2px 6px; border-radius: 4px; }
---

<!-- _class: lead -->

# 🚍 KOA Filo Servis

## Kurumsal Personel Servis Taşımacılığı & Filo Yönetim Platformu

**Blazor · .NET 10 · EF Core · PostgreSQL**

---

## 📌 Bir Bakışta

> Personel taşımacılığı yapan firmaların **filo · operasyon · puantaj · hakediş · muhasebe** süreçlerini tek panelden yöneten, **çok şirketli (multi-tenant)** kurumsal platform.

- 🚐 **Filo** — araç, plaka geçmişi, evraklar, bakım
- 🛣️ **Güzergah & Puantaj** — hiyerarşik liste, toplu onay
- 💰 **Hakediş & Fatura** — sütun filtreli, tek tıkla geçiş
- 🗂️ **EBYS & Uyarılar** — şifreli arşiv, merkezi takip
- 🤖 **AI** — semantik arama, araç değerleme

---

## 🎯 Hedef Kullanıcı

| Profil | Fayda |
| --- | --- |
| **Filo Müdürü** | Tek ekrandan araç & evrak durumu, süre uyarıları |
| **Operasyon Sorumlusu** | Güzergah-araç eşleşmesi, hızlı puantaj, toplu onay |
| **Muhasebe / Finans** | Hakediş → fatura akışı, banka/kasa, mali analiz |
| **Tedarikçi Yöneticisi** | Tedarikçi araç & personel kontrolü, sözleşme takibi |
| **Yönetim Kurulu** | Kârlılık, masraf, performans raporları |

---

## 🧩 Modüler Mimari

<style scoped>section { font-size: 26px; }</style>

```
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

> Akış doğrusal değil; her adım ileri-geri veri besler.

---

## 🚐 Filo & Araç Yönetimi

- **Şase bazlı tekil araç kartı**, plaka geçmişi
- Sahiplik tipleri: **Özmal · Kiralık · Tedarikçi**
- Kira / komisyon hesaplama tipleri
- **Çok versiyonlu evrak arşivi** (Ruhsat, Sigorta, Kasko, Muayene, …)
- **Tek tarih kaynağı**: AracEvrak ↔ Arac.* alanları **otomatik senkron**
  - Evrakta tarih değişti → uyarı, rapor, listeler **otomatik güncellenir**

---

## 🛣️ Güzergah & Puantaj

- **Kurum** ve **Cari** ayrı kavramlar — Cari'siz Kurum desteklenir
- Hiyerarşik UI: **Güzergah → Araç → Günlük Satır**
- **Ay seçici** + tarih filtresi
- **Rota bazlı toplu onay**
- Otomatik puantaj üretimi (varsayılan araç/şoför/tedarikçi)
- Tedarikçi araç bilgileri otomatik doldurulur

---

## 💰 Hakediş

- Güzergah-araç eşleştirmelerinden türeyen **canlı liste**
- Üst filtre: Kurum / Cari / Güzergah / Araç / Sahiplik / Servis Türü / Kapasite
- **Sütun bazlı filtre satırı**: her kolon kendi kutusundan aranır
- Özet kartları: eşleştirme adedi, gelir/gider toplamı, tedarikçi araç sayısı
- **Detay modal** + Güzergah Düzenle + **Puantaj geçişi**

---

## 🗂️ EBYS & Belge Uyarıları

**EBYS Modülü**
- Gelen / Giden / Personel Özlük / Araç Evrak
- AI tabanlı **belge tipi tanıma** ve **semantik arama**
- Diskte şifreli depolama (`SecureFileService`)
- Versiyon yönetimi

**Merkezi Uyarı Paneli**
- Araç evrakları, şoför belgeleri, tedarikçi sözleşmeleri, kiralık plakalar
- Quartz job ile periyodik tarama
- E-posta + uygulama içi bildirim

---

## 🤖 AI & Otomasyon

- **Araç Piyasa Araştırma & Değerleme** servisleri
  - Microsoft.Extensions.AI · OllamaSharp
- **Semantik belge arama** (içerik bazlı)
- Otomatik veri senkron servisi (`KOAFiloServis.DataSync`)
- (Yol haritası) Puantaj anomali tespiti

---

## 🛡️ Kurumsal Altyapı

| Konu | Detay |
| --- | --- |
| Multi-tenant | `SirketId` bazlı veri ayrımı |
| Yetkilendirme | `Permissions` + `RolePermissions` + menü bazlı erişim |
| API | JWT tabanlı REST uçları |
| Loglama | Kapsamlı aktivite & oturum logu |
| Cache | Redis + InMemory |
| Job | Quartz.NET 3 |
| Test | xUnit · Playwright · Selenium |

---

## 🏗️ Teknoloji Yığını

<style scoped>section { font-size: 24px; }</style>

| Katman | Teknoloji |
| --- | --- |
| UI | **Blazor Interactive Server** · Bootstrap 5 |
| Backend | **ASP.NET Core 10** · Razor Components · REST API |
| Veri | **EF Core 10** (PostgreSQL · SQL Server · SQLite · MySQL) |
| Belge | ClosedXML · EPPlus · QuestPDF |
| Mail | MailKit |
| AI | Microsoft.Extensions.AI · OllamaSharp |
| Lisans | WinForms (`KOAFiloServis.LisansDesktop`) |

---

## 📦 Çözüm Yapısı

```
KOAFiloServis.sln
├── KOAFiloServis.Web/           ← Blazor uygulaması (UI + API + servisler)
├── KOAFiloServis.Shared/        ← Domain entity'leri & DTO'lar
├── KOAFiloServis.DataSync/      ← Otomatik veri senkron servisi
├── KOAFiloServis.LisansDesktop/ ← Masaüstü lisans yöneticisi
└── KOAFiloServis.Tests/         ← xUnit & entegrasyon testleri
```

---

## 🚀 Hızlı Kurulum

```pwsh
git clone https://github.com/karamur/KOAFiloServis.git
cd KOAFiloServis

dotnet restore
dotnet build

# appsettings.json -> ConnectionStrings düzenle
dotnet ef database update --project KOAFiloServis.Web

dotnet run --project KOAFiloServis.Web
```

> Varsayılan: `https://localhost:5001`

---

## 📈 Değer Önerisi

- ⏱️ **Operasyonel hız**: tek ekran puantaj + toplu onay
- 🔁 **Tek doğru kaynak**: evrak tarihleri tüm modüllerde tutarlı
- 💸 **Mali şeffaflık**: hakediş → fatura → muhasebe izi
- 🧠 **AI ile zaman tasarrufu**: belge tanıma + semantik arama
- 🛡️ **Kurumsal güvenlik**: rol/yetki + şifreli arşiv + log

---

## 🗺️ Yol Haritası

- 📱 **MAUI** ile şoför mobil uygulaması
- 📊 **PowerBI / OData** uçları
- 🤝 Tedarikçi self-servis portalı
- 🧾 e-Fatura / SAP entegrasyonu
- 🤖 Puantaj **anomali tespiti** (AI)

---

<!-- _class: lead -->

# Teşekkürler 🙏

**KOA Filo Servis**
_Operasyondan muhasebeye, filodan hakedişe — tek panelden yönetim._

🌐 https://github.com/karamur/KOAFiloServis
✉️ info@karamur.com
