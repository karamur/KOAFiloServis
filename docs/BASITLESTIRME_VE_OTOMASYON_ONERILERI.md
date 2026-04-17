# 🚀 KOAFiloServis — Basitleştirme ve Otomasyon Önerileri

> **Amaç:** Kullanıcının günlük iş yükünü azaltmak, tıklama/form sayısını düşürmek, manuel hataları önlemek ve sık tekrar eden işleri arka plana almak.
>
> **Durum:** ⏳ Onay bekliyor — Hiçbir madde henüz uygulanmadı. Onayladığın maddeler için tek tek implementasyon yapılacak.
>
> **Tarih:** 2026-04-16
> **Hazırlayan:** GitHub Copilot Agent

---

## 📋 Onay Sistemi

Her maddenin sonunda bir checkbox var:

- `[ ]` — Henüz onaylanmadı
- `[x]` — Onayla (uygulanacak)
- `[-]` — Reddet (atlanacak)

Onayladıktan sonra "şu maddeyi uygula" demen yeterli.

---

## 🎯 1. KULLANICI DENEYİMİ (UX) BASİTLEŞTİRMELERİ

### 1.1 Global Arama Kısayolu (`Ctrl+K` Komut Paleti) `[ ]`
**Sorun:** `GlobalSearchService` mevcut ama menüden ulaşmak için fareyle gezilmesi gerekiyor.
**Öneri:** Tüm sayfalarda `Ctrl+K` ile açılan, araç/cari/fatura/evrak/personel arayan komut paleti (VS Code stili).
**Etki:** ⭐⭐⭐⭐⭐ — En sık kullanılan özellik haline gelebilir.
**Maliyet:** Düşük (1 component + JS interop).

### 1.2 Dashboard Widget Kişiselleştirme `[ ]`
**Sorun:** Home.razor'da herkese aynı widget'lar gösteriliyor (sürücüye fatura, muhasebeciye araç vs. lazımsız).
**Öneri:** Drag & drop ile widget gizle/göster + sıralama (kullanıcı bazlı `KullaniciAyarlari` tablosu).
**Etki:** ⭐⭐⭐⭐
**Maliyet:** Orta (yeni tablo + UI).

### 1.3 Hızlı Aksiyon FAB Butonu (Sağ Alt) `[ ]`
**Sorun:** "Yeni Sefer / Yeni Fatura / Yeni Araç" için hep menüye gidiliyor.
**Öneri:** Tüm sayfalarda sağ altta floating button — context'e göre en sık kullanılan 3-4 aksiyonu açar.
**Etki:** ⭐⭐⭐⭐
**Maliyet:** Düşük.

### 1.4 Listelerde Sticky Filtre Çubuğu + URL Senkronizasyonu `[ ]`
**Sorun:** Filtre yapılınca sayfa yenilenince kayboluyor. Linki kimseye paylaşılamıyor.
**Öneri:** Filtreler URL query string'e yansısın. F5'te kalsın, link kopyalanabilsin.
**Etki:** ⭐⭐⭐⭐
**Maliyet:** Orta (her listeye uygulanmalı).

### 1.5 Inline Düzenleme (Tablolarda Hücreye Çift Tıklayınca) `[ ]`
**Sorun:** Küçük bir düzeltme için detay sayfasına gidip form doldurmak gerekiyor.
**Öneri:** Cariler, Araçlar, Personel listelerinde "Telefon", "Email", "Notlar" gibi alanları inline düzenleme.
**Etki:** ⭐⭐⭐⭐
**Maliyet:** Orta.

### 1.6 Toplu İşlem Çubuğu (Multi-select) `[ ]`
**Sorun:** 10 faturayı tek tek "Ödendi" yapmak gerekiyor.
**Öneri:** Listelerde checkbox + üstte "Seçilenleri: Ödendi yap | Sil | Etiketle | PDF indir" toplu aksiyon barı.
**Etki:** ⭐⭐⭐⭐⭐
**Maliyet:** Orta.

---

## 🤖 2. OTOMATİKLEŞTİRMELER (Background & Workflow)

### 2.1 Otomatik Belge Yenileme Hatırlatması — E-posta/WhatsApp `[ ]`
**Mevcut:** `BelgeUyariBackgroundService` zaten belge sürelerini kontrol ediyor.
**Eksik:** Sadece dashboard'da gösteriliyor. Kullanıcı sisteme girmezse görmüyor.
**Öneri:** 30/15/7/1 gün kala otomatik e-posta + opsiyonel WhatsApp (`WhatsAppService` mevcut).
**Etki:** ⭐⭐⭐⭐⭐
**Maliyet:** Düşük (mevcut servislere job ekleme).

### 2.2 Tekrarlayan Faturaları Otomatik Üret `[ ]`
**Mevcut:** `TekrarlayanOdemeService` var — ama sadece ödeme tarafı.
**Öneri:** Aylık kira/sigorta/yakıt gibi sabit faturaları her ay otomatik oluştur, sadece "Onayla" diye dashboard'a düşür.
**Etki:** ⭐⭐⭐⭐⭐
**Maliyet:** Orta (Quartz job + onay UI).

### 2.3 Banka Ekstresi Otomatik Eşleştirme (AI) `[ ]`
**Mevcut:** `OdemeEslestirmeService` var.
**Öneri:** Banka ekstresi yüklendiğinde AI ile cariye otomatik eşleştir (tutar + tarih + IBAN + açıklama benzerliği), sadece şüpheli olanları kullanıcıya sor.
**Etki:** ⭐⭐⭐⭐⭐
**Maliyet:** Yüksek (OpenAI/Ollama prompt + UI).

### 2.4 Fatura OCR Geliştirmesi — Sürükle-Bırak Onay Akışı `[ ]`
**Mevcut:** `FaturaAIImportService` var.
**Öneri:** Outlook/Gmail'den e-posta ekini doğrudan sürükle → AI ayrıştırsın → tek tıkla onayla. (PDF'i opsiyonel kayıt.)
**Etki:** ⭐⭐⭐⭐
**Maliyet:** Orta.

### 2.5 Sürücü Servis Tamamlama → Otomatik Hakediş `[ ]`
**Sorun:** Servis tamamlanınca hakediş ayrı sayfada manuel hesaplanıyor.
**Öneri:** `ServisCalisma` "Tamamlandı" yapıldığında km × fiyat × yolcu kuralı ile hakediş kaydı otomatik üretilsin.
**Etki:** ⭐⭐⭐⭐
**Maliyet:** Orta.

### 2.6 Araç Bakım Periyot Otomatik Bildirimi (Km Bazlı) `[ ]`
**Sorun:** "10.000 km'de yağ değişimi" gibi periyodik bakımları kimse takip etmiyor.
**Öneri:** Araç son km girildiğinde otomatik kontrol → bakım periyodu yaklaşıyorsa bildirim.
**Etki:** ⭐⭐⭐⭐
**Maliyet:** Orta (yeni tablo: `BakimPeriyot`).

### 2.7 Otomatik Yedek Sağlığı Kontrolü `[ ]`
**Mevcut:** `AutoBackupService` çalışıyor.
**Öneri:** Yedek alındıktan sonra otomatik test-restore (mock DB'ye), başarısız olursa admine alarm.
**Etki:** ⭐⭐⭐⭐
**Maliyet:** Düşük.

---

## ⚡ 3. FORM & VERİ GİRİŞ KOLAYLIKLARI

### 3.1 Akıllı Form Doldurma — Önceki Değerlerden Hatırlama `[ ]`
**Öneri:** "Yeni Cari" formunda son 5 ekleme baz alınarak şehir/vergi dairesi/ülke öneri olarak çıksın (cookie yok, kullanıcı bazlı kayıt).
**Etki:** ⭐⭐⭐
**Maliyet:** Düşük.

### 3.2 Plaka OCR — Fotoğraftan Araç Ekleme `[ ]`
**Öneri:** Telefon kamerasıyla plaka çek → OCR → otomatik araç sorgusu → form prefill.
**Etki:** ⭐⭐⭐⭐
**Maliyet:** Yüksek (OCR API + mobil uyum).

### 3.3 TC Kimlik / Vergi No Doğrulama (Online) `[ ]`
**Öneri:** Cari/Personel formunda TC veya VKN girilince GIB API ile isim/unvan otomatik gelsin.
**Etki:** ⭐⭐⭐⭐
**Maliyet:** Orta (3rd party API).

### 3.4 Kısayollar Cheat Sheet (`?` Tuşu) `[ ]`
**Öneri:** Her sayfada `?` basınca o sayfanın klavye kısayollarını gösteren overlay.
**Etki:** ⭐⭐⭐
**Maliyet:** Düşük.

### 3.5 Formlarda Otomatik Taslak Kaydetme (LocalStorage) `[ ]`
**Sorun:** Uzun fatura/teklif formunda tarayıcı kapanırsa veri kaybı oluyor.
**Öneri:** 5 saniyede bir LocalStorage'a kaydet, sayfa açılınca "Devam etmek ister misiniz?" sor.
**Etki:** ⭐⭐⭐⭐
**Maliyet:** Düşük.

---

## 📊 4. RAPORLAMA & GÖRSELLEŞTİRME

### 4.1 Tek Tıkla "Aylık Yönetim Özeti" PDF `[ ]`
**Öneri:** Dashboard'da bir buton — gelir/gider/araç performansı/personel/belge uyarıları içeren tek sayfalık PDF üretsin (`QuestPDF` zaten var).
**Etki:** ⭐⭐⭐⭐⭐
**Maliyet:** Orta.

### 4.2 WhatsApp ile Günlük Özet Gönderimi (Patrona) `[ ]`
**Öneri:** Her sabah 08:00'de "Bugün: 3 sefer planlı, 2 fatura vadesi geçti, 1 belge süresi doldu" özetini WhatsApp'tan yöneticiye yolla.
**Etki:** ⭐⭐⭐⭐⭐
**Maliyet:** Düşük (mevcut servislerle).

### 4.3 Cari Bazlı Otomatik Aylık Ekstre E-postası `[ ]`
**Öneri:** Her ay başı, bakiyesi olan carilere otomatik PDF ekstre + hatırlatma mailı (`EmailService` var).
**Etki:** ⭐⭐⭐⭐
**Maliyet:** Düşük.

### 4.4 Heatmap — Araç Kullanım Yoğunluğu `[ ]`
**Öneri:** Hangi aracın hangi gün/saatte ne kadar kullanıldığını gösteren ısı haritası — atıl araç tespiti için.
**Etki:** ⭐⭐⭐
**Maliyet:** Orta.

---

## 🔔 5. BİLDİRİM SİSTEMİ TOPLULAŞTIRMA

### 5.1 Tek Bildirim Merkezi (Bell Icon) `[ ]`
**Sorun:** Belge uyarısı, evrak ataması, fatura hatırlatması, GPS alarmı ayrı ayrı yerlerde.
**Öneri:** Üst menüde tek 🔔 butonu — tüm bildirimler toplu, okundu/okunmadı, kategori filtresi.
**Etki:** ⭐⭐⭐⭐⭐
**Maliyet:** Orta (`BildirimService` var, UI eksik).

### 5.2 Browser Push Notification `[ ]`
**Öneri:** Tarayıcı kapalıyken bile masaüstüne bildirim (Web Push API).
**Etki:** ⭐⭐⭐
**Maliyet:** Yüksek (Service Worker + sertifika).

### 5.3 Bildirim Tercihleri Sayfası `[ ]`
**Öneri:** Kullanıcı hangi olayları hangi kanaldan (mail/WA/push/in-app) almak istediğini seçebilsin.
**Etki:** ⭐⭐⭐⭐
**Maliyet:** Orta.

---

## 🛠 6. TEKNİK BORÇ AZALTMA / PERFORMANS

### 6.1 Tüm Servislerde IDbContextFactory Pattern Standardizasyonu `[ ]`
**Sorun:** Bugün `EbysEvrakService`'te yaşadığımız "concurrency" hatası — başka servislerde de var olabilir.
**Öneri:** Tüm servisleri tarayıp scoped DbContext kullananları factory pattern'ine çevir (otomatik refactor script).
**Etki:** ⭐⭐⭐⭐⭐ (kararlılık)
**Maliyet:** Orta.

### 6.2 Dashboard Yüklemelerini Paralel Yap `[ ]`
**Sorun:** Home.razor'da bölümler sırayla yükleniyor — toplam 5-10 saniye sürebilir.
**Öneri:** `Task.WhenAll` ile paralel yükle (factory pattern düzeltmesinden sonra güvenli olur).
**Etki:** ⭐⭐⭐⭐
**Maliyet:** Düşük.

### 6.3 Listelerde Server-Side Pagination `[ ]`
**Sorun:** Bazı sayfalar (Faturalar, Cariler) tüm veriyi memory'ye çekiyor.
**Öneri:** `Skip/Take` + sayfa sayısı (özellikle 1000+ kayıt olan listelerde).
**Etki:** ⭐⭐⭐⭐
**Maliyet:** Orta.

### 6.4 Cache Stratejisi (Kategoriler, Kurullar, Sabit Listeler) `[ ]`
**Mevcut:** `CacheService` var ama az kullanılıyor.
**Öneri:** Kategori/şehir/birim/vergi oranı gibi nadiren değişen veriler IMemoryCache'e alınsın.
**Etki:** ⭐⭐⭐⭐
**Maliyet:** Düşük.

---

## 🎓 7. KULLANICI EĞİTİMİ & ONBOARDING

### 7.1 İlk Giriş Tour'u (Driver.js / Shepherd) `[ ]`
**Öneri:** Yeni kullanıcı girince adım adım "Burası dashboard, buradan araç ekle, buradan sefer aç" gibi interaktif tour.
**Etki:** ⭐⭐⭐⭐
**Maliyet:** Orta.

### 7.2 Boş Sayfa Empty State'leri `[ ]`
**Sorun:** Yeni kullanıcı "Cariler" sayfasını açtığında boş tablo görüyor, ne yapacağını anlamıyor.
**Öneri:** Boş listelerde "Henüz cari eklemediniz. [Yeni Cari Ekle] butonuna tıklayın veya [Excel'den İçe Aktar] ile başlayın." gibi açıklayıcı görseller.
**Etki:** ⭐⭐⭐
**Maliyet:** Düşük.

### 7.3 Bağlam Yardımı (?) İkonları `[ ]`
**Öneri:** Karmaşık alanların yanında `?` ikonu — tıklayınca tooltip ile açıklama.
**Etki:** ⭐⭐⭐
**Maliyet:** Düşük.

---

## 📱 8. MOBİL & UZAKTAN ERİŞİM

### 8.1 PWA (Progressive Web App) Desteği `[ ]`
**Öneri:** Telefondan "Ana Ekrana Ekle" → uygulama gibi açılsın, offline cache.
**Etki:** ⭐⭐⭐⭐
**Maliyet:** Orta (manifest + service worker).

### 8.2 Şoför Mobil Sayfası — Sefer Başlat/Bitir `[ ]`
**Öneri:** Şoför login olunca sade bir sayfa: "Bugünkü seferleriniz / Başlat / Tamamla / Yakıt ekle". Karmaşık menü olmasın.
**Etki:** ⭐⭐⭐⭐⭐
**Maliyet:** Orta.

---

## 🧹 9. NAV MENÜ BASİTLEŞTİRME

### 9.1 Sık Kullanılanlar Bölümü (Otomatik) `[ ]`
**Öneri:** Kullanıcının son 7 günde en çok ziyaret ettiği 5 sayfa menünün üstünde "⭐ Sık Kullanılanlar" altında listelensin.
**Etki:** ⭐⭐⭐⭐
**Maliyet:** Düşük.

### 9.2 Menü Arama (Filtre Kutusu) `[ ]`
**Sorun:** 32 menü kategorisi var, gereksiz uzun.
**Öneri:** Menünün üstünde küçük "Menüde ara..." kutusu.
**Etki:** ⭐⭐⭐
**Maliyet:** Düşük.

### 9.3 Yetkisiz Menüleri Tamamen Gizle (Disable Değil) `[ ]`
**Öneri:** Kullanıcının yetkisi olmadığı menüler hiç görünmesin (clutter azaltma).
**Etki:** ⭐⭐⭐
**Maliyet:** Düşük (zaten yetki sistemi var).

---

## 🎯 ÖNERİLEN ÖNCELİK SIRASI

> **İlk dalga (Hızlı kazanç, düşük maliyet):**
> 1. **6.1** — IDbContextFactory standardizasyonu (kararlılık)
> 2. **2.1** — Belge yenileme otomatik mail/WA
> 3. **5.1** — Tek bildirim merkezi
> 4. **3.5** — Form taslak otomatik kayıt
> 5. **9.3** — Yetkisiz menü gizleme
>
> **İkinci dalga (Orta vadeli, yüksek etki):**
> 6. **1.1** — Ctrl+K komut paleti
> 7. **1.6** — Toplu işlem çubuğu
> 8. **2.2** — Tekrarlayan fatura otomatik üretim
> 9. **4.1** — Aylık özet PDF
> 10. **4.2** — WhatsApp günlük özet
>
> **Üçüncü dalga (AI & büyük yatırım):**
> 11. **2.3** — AI ile banka eşleştirme
> 12. **3.2** — Plaka OCR
> 13. **8.2** — Şoför mobil sayfası

---

## ✅ ONAY KONTROL LİSTESİ

Onayladığın maddeleri `[ ]` → `[x]` yaparsan, "şu maddeleri uygula" demen yeterli. Toplu da seçebilirsin (örn. "1.1, 2.1, 5.1, 6.1 uygula").

**Sorularım:**
1. Hangi dalgayla başlayalım? (1, 2, veya 3)
2. WhatsApp/E-posta entegrasyonları için API anahtarları hazır mı?
3. Mobil tarafa öncelik var mı yoksa önce masaüstü mü?

---

_Bu doküman canlı bir tekliftir. Onayladıkça maddeleri tek tek uygulayacağım, her uygulamadan sonra commit & push ile GitHub'a göndereceğim._
