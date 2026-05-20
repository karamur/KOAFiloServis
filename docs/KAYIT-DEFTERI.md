# 📒 KOAFiloServis — Kayıt Defteri

> Bu dosya, geliştirme sürecinde alınan kararları, yapılan tartışmaları ve hazırlanan raporları
> kronolojik olarak kayıt altına alır. Her oturum sonunda güncellenir.

---

## 📅 14.05.2026 — AI Asistan + Mimari Karar Oturumu

### Commit: `952a546`
```
feat(ai-asistan): DeepSeek V3/R1 model katalogu + docs guncelleme
```

---

### 🤖 AI Asistan Model Kataloğu

**Konu:** DeepSeek V4 yapay zeka listesine eklenebilir mi?

**Araştırma Sonucu:**
- Ollama public registry'de `deepseek-v4` tag'i **mevcut değil** (14.05.2026 tarihi itibarıyla)
- Mevcut resmi DeepSeek sürümleri:
  - `deepseek-v3` — Genel amaçlı, güçlü model
  - `deepseek-r1` — Reasoning (akıl yürütme) modeli
  - `deepseek-coder-v2` — Kod odaklı model

**Yapılan Değişiklik:**
- `AIAsistan.razor` → `GetBirlesikModelListesi()` metoduna `deepseek-v3` ve `deepseek-r1` eklendi
- Dropdown artık iki grup gösteriyor:
  - **Yerel (Ollama):** Makinede `ollama pull` ile yüklenmiş modeller
  - **Önerilen (yüklü değil):** Katalogdaki ama henüz indirilmemiş modeller
- Yüklü olmayan model seçilince `ollama pull <model>` komutu ipucu olarak gösteriliyor

**Dosya:** `KOAFiloServis.Web/Components/Pages/Ayarlar/AIAsistan.razor`

---

### 🏗️ MİMARİ KARAR — Database Per Firma

#### Sorun Tanımı
Müşteri 3 firma ile çalışıyor. Mevcut "Shared Database + FirmaId row-level isolation"
mimarisinde kullanıcıların hatalı firma seçimi veya filter kaçağı durumunda firmaların
verileri birbirine karışabiliyor. Zirve Müşavirlik gibi referans sistemlerde her firma
ayrı veritabanında çalışıyor.

#### Mevcut Mimari (Shared DB)
```
PostgreSQL: DestekCRMServisBlazorDb (TEK DB)
  Araclar   → FirmaId=1, FirmaId=2, FirmaId=3  (hepsi aynı tabloda)
  Cariler   → FirmaId=1, FirmaId=2, FirmaId=3
  Faturalar → FirmaId=1, FirmaId=2, FirmaId=3

Koruma mekanizması: HasQueryFilter("FirmaId == aktifFirma")
Zayıf nokta: Kullanıcı hatalı firma seçimi → yanlış veri görme/yazma riski
```

#### Hedef Mimari (Database Per Firma)
```
PostgreSQL Server
  db_global   → Kullanıcılar, Lisans, Firma katalogu
  db_firma_1  → Firma A'nın TÜM verileri (tam izolasyon)
  db_firma_2  → Firma B'nın TÜM verileri (tam izolasyon)
  db_firma_3  → Firma C'nın TÜM verileri (tam izolasyon)
  db_holding  → Ortak/Holding konsolidasyon DB
```

#### Gerekçe
1. **Veri güvenliği:** DB seviyesinde fiziksel izolasyon — filter bypass imkansız
2. **Müşteri talebi:** Zirve Müşavirlik benzeri yapı isteniyor
3. **Holding ihtiyacı:** 3 firmayı birleştiren ortak raporlama / bütçe konsolidasyonu
4. **Yedekleme:** Firma bazlı backup/restore kolaylaşır
5. **KVKK/Hukuki:** Firma verisi fiziksel olarak ayrı

---

### 🏢 HOLDİNG / ORTAK FİRMA MODÜLÜ

#### Kavram
3 (veya daha fazla) operasyonel firmanın finansal verilerini **özetleyerek** tek bir
"Holding" veritabanında konsolide eden yeni modül.

#### Holding'e Ne Aktarılır?
| Veri Türü | Aktarılır | Not |
|-----------|:---------:|-----|
| Bütçe gerçekleşmesi | ✅ | Gelir/gider toplam |
| Fatura toplamları | ✅ | KDV dahil/hariç |
| Banka/Kasa bakiyesi | ✅ | Dönem sonu snapshot |
| Personel gider özeti | ✅ | Bordro toplamı |
| Araç maliyet özeti | ✅ | Bakım+yakıt toplam |
| Hakediş ödemeleri | ✅ | Tedarikçi toplamı |
| Tekil fatura detayı | ❌ | Gizlilik/boyut |
| Personel özlük | ❌ | KVKK |
| Cari kart detayı | ❌ | Firma içi bilgi |

#### Holding Rapor Türleri
- Firma Karşılaştırma (Gelir/Gider/Kâr yan yana)
- Bütçe Konsolidasyonu (tüm firmalar toplam)
- Ödeme Planı (tüm firmaların vadesi gelenler)
- Araç Maliyet Özeti (firma bazlı)
- Personel Gider Özeti (firma bazlı bordro toplam)
- Hakediş Özeti (tedarikçi ödemeleri)

---

### 🔀 GITHUB — YENİ REPO KARARI

#### Soru
> "GitHub'da bu yapıya dokunmadan yeni proje gibi açıp projeyi oraya kopyalayıp
> oradan devam etsek olur mu?"

#### Yanıt ve Değerlendirme
**Evet, tamamen uygulanabilir.** İki yöntem var:

---

**Yöntem 1 — Fork (Önerilen)**
```bash
# GitHub web arayüzünde:
# 1. https://github.com/karamur/KOAFiloServis → "Fork" butonu
# 2. Yeni repo adı: KOAFiloServis-v2  (veya KOAFiloServis-MultiDb)
# 3. Sadece main branch'i fork et

# Yerel:
git clone https://github.com/karamur/KOAFiloServis-v2
cd KOAFiloServis-v2
# Upstream'i orijinal repo olarak ekle (gelecekte senkronizasyon için)
git remote add upstream https://github.com/karamur/KOAFiloServis
```
✅ Orijinal repo **aynen korunur** (production backup)
✅ Yeni repoda Database-Per-Firma geçişi yapılır
✅ İleride orijinale patch geri alınabilir (`git cherry-pick`)

---

**Yöntem 2 — Yeni Boş Repo + Kopyalama**
```bash
# GitHub'da yeni repo oluştur: KOAFiloServis-v2

# Yerel — mevcut kodu yeni remote'a bağla:
cd "C:\Users\muratk\Desktop\d yedek\calisma\Claude-Code\KOAFiloServis"
git remote add v2 https://github.com/karamur/KOAFiloServis-v2
git push v2 main

# Yeni çalışma klasörü:
cd C:\Users\muratk\Desktop\d yedek\calisma\
git clone https://github.com/karamur/KOAFiloServis-v2 KOAFiloServis-v2
```
✅ Temiz başlangıç
⚠️ Commit geçmişi taşınır (arzu edilmezse `--depth 1` veya squash)

---

**Önerilen Akış:**
```
karamur/KOAFiloServis        → Mevcut production kodu (dokunulmaz, korunur)
karamur/KOAFiloServis-MultiDb → Yeni Database-Per-Firma mimarisi geliştirme
```

Geçiş tamamlanıp test edilince `KOAFiloServis-MultiDb` → `KOAFiloServis`'e merge edilir
veya doğrudan production'a alınır.

---

### 📋 Uygulama Fazları (Özet)

| Faz | İçerik | Tahmini Süre |
|-----|--------|:---:|
| **Faz 1** | GlobalDbContext + TenantDbContext ayrımı + ITenantDbResolver | 3–4 gün |
| **Faz 2** | Mevcut veri göçü (3 firma DB'sine taşıma) | 1–2 gün |
| **Faz 3** | Holding modülü + konsolidasyon raporları | 3–4 gün |
| **Faz 4** | IFirmaTenant + FirmaId temizliği | 1–2 gün |
| **Test** | Stabilizasyon | 2–3 gün |
| **TOPLAM** | | **~10–15 gün** |

---

### ⚠️ Riskler

| Risk | Önlem |
|------|-------|
| Veri göçü sırasında kayıp | Tam backup → row count doğrulama |
| FirmaKopyalama çoklu DB'de kırılma | Önce refactor, sonra göç |
| Migration yönetimi karmaşıklığı | Tek TenantDbContext migration path |
| Holding raporu performansı | Task.WhenAll ile paralel DB sorgusu |

---

### 🎯 Sonraki Adım (Onay Bekliyor)

Yeni repo açma ve Faz 1'e başlama kararı alınırsa:
1. GitHub'da `KOAFiloServis-MultiDb` reposu oluştur
2. Mevcut kodu oraya kopyala (`git push v2 main`)
3. `GlobalDbContext` ve `TenantDbContext` dosyalarını oluştur
4. `ITenantDbResolver` interface + implementasyonunu yaz
5. `appsettings.json`'a `TenantDb:Template` bölümü ekle

---

## 📚 İlgili Dosyalar

| Dosya | Konu |
|-------|------|
| `docs/TENANT_MIGRATION_PLAN.md` | Mevcut tenant migrasyonu (tamamlanmış) |
| `docs/CALISMA-NOTLARI-2026-05-13.md` | Önceki oturum notları |
| `docs/CALISMA-NOTLARI-2026-05-14.md` | Bu oturum notları |
| `docs/OTURUM_NOTLARI_2026-05-19.md` | Tenant v1.0.21 tamamlandı notu |
| `KOAFiloServis.Shared/Entities/IFirmaTenant.cs` | Mevcut tenant interface |
| `KOAFiloServis.Web/Data/TenantAwareDbContextFactory.cs` | Mevcut factory |
| `KOAFiloServis.Web/Services/IAktifFirmaProvider.cs` | Aktif firma state servisi |
