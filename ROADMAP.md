# ?? Koa Filo Servis - Proje Faz Planı

## ?? Mevcut Durum Analizi

### Son Güncellemeler
- ✅ `Şirketler Arası Transfer (FAZ 4.1)` tamamlandı - SirketTransferLog entity, ITenantService transfer metodları, 7 entity için transfer desteği (Cari, Araç, Şoför, Fatura, Güzergah, BankaHesap, BankaKasaHareket), SirketTransfer.razor yönetim UI.
- ✅ `Multi-tenant Veri İzolasyonu (FAZ 4.1)` tamamlandı - 7 temel entity'ye (Cari, Sofor, Arac, Fatura, Guzergah, BankaHesap, BankaKasaHareket) SirketId eklendi, Global Query Filter ile şirket bazlı otomatik filtreleme, SuperAdmin bypass desteği.
- ✅ `Şirket Yönetimi UI (FAZ 4.1)` tamamlandı - SirketYonetimi.razor sayfası, şirket CRUD, şirket geçişi, istatistik kartları, Admin/SuperAdmin yetki kontrolü.
- ✅ `Mobil Uygulama API Endpoint'leri (FAZ 3.3)` tamamlandı - Sefer geçmişi, tekil sefer getirme, health check endpoint'leri MobileController'a eklendi.
- ✅ `Mobil Uygulama Sayfaları (FAZ 3.3)` tamamlandı - SeferGecmisi.razor, SeferBitir.razor, Ayarlar.razor sayfaları ve IApiService/ApiService güncellemeleri.
- ✅ `Araç Takip Sistemi (GPS Entegrasyonu - FAZ 3.1)` tamamlandı - AracTakipCihaz, AracKonum, AracBolge entity'leri, canlı araç takip haritası, konum geçmişi ve raporlama, GPS cihaz yönetimi, Geofence (bölge) desteği, otomatik alarm sistemi.
- ✅ `Test Data Seeding (Demo Veri Oluşturma)` tamamlandı - TestDataSeeder servisi, [TEST] etiketleme sistemi, DemoVeri.razor yönetim sayfası, 15 şoför, 12 araç, 8 güzergah, 45 fatura, 30 gün sefer örnek verisi oluşturma.
- ✅ `Redis Cache Entegrasyonu (FAZ 4.3)` tamamlandı - IDistributedCache tabanlı cache servisi, Memory/Redis provider desteği, Dashboard grafik metodlarına cache entegrasyonu, CacheKeys ve CacheDurations yardımcı sınıfları eklendi.
- ✅ `EF Core Sorgu Optimizasyonu (FAZ 4.3)` tamamlandı - CariService N+1 sorunu çözüldü (toplu bakiye hesaplama), AsNoTracking yaygınlaştırıldı (Cari, Fatura, Araç servisleri).
- ✅ `Webhook Desteği (FAZ 4.2)` tamamlandı - WebhookEndpoint/WebhookLog entity'leri, IWebhookService servisi, HMAC imza, retry mekanizması, Webhook yönetim UI'ı (/ayarlar/webhooks) eklendi.
- ✅ `REST API + Swagger (FAZ 4.2)` tamamlandı - JWT Bearer Authentication, 6 API Controller (Auth, Cariler, Araclar, Soforler, Faturalar, Guzergahlar), Swagger/OpenAPI dokümantasyonu eklendi.
- ✅ `Veri Export (FAZ 9.1)` tamamlandı - `IDataExportService` içine `CSV / JSON / Parquet` export desteği eklendi, `OdemeYonetimi` ekranından filtrelenmiş ödeme listesi doğrudan indirilebilir hale getirildi.
- ✅ `E-Fatura entegrasyonu (GİB) - Durum Takibi` tamamlandı - XML sonrası gönderime hazırlık, gönderildi, kabul/red durum takibi ve UI aksiyonları eklendi.
- ✅ `Puantaj onay sistemi` tamamlandı - Personel puantaj kayıtları için taslak, onay bekliyor, onaylandı, reddedildi akışı ve UI aksiyonları eklendi.
- ✅ `Maaşa Mahsup (Masraf/Ödeme)` tamamlandı - Açık avansların maaştan kesinti olarak mahsup edilmesi ve maaş ekranından yönetimi eklendi.
- ✅ `ASP.NET Core Identity Entegrasyonu` tamamlandı - `UserManager` destekli kullanıcı store, legacy hash uyumlu password hasher ve mevcut kullanıcı sistemi ile uyumlu kimlik altyapısı eklendi.
- ✅ `Kullanıcı Kayıt/Giriş` tamamlandı - Self-servis kayıt ekranı, varsayılan kullanıcı rolü ile kayıt ve login ekranı bağlantısı eklendi.
- ✅ `Şifre Sıfırlama` tamamlandı - Login ekranından kullanıcı adı/e-posta ile geçici şifre üretimi ve e-posta gönderimi eklendi.
- ✅ `Excel Export İyileştirme` tamamlandı - Şoför Performans ve Araç Karlılık raporları ortak Excel servisine taşındı, indirme akışı standartlaştırıldı.
- ✅ `Dosya Yükleme (Ruhsat, Ehliyet, Sözleşme)` tamamlandı - AracForm ve SoforForm'a doğrudan belge yükleme/indirme özelliği eklendi.
- ✅ `E-posta Bildirimleri` tamamlandı - Kullanıcı bazlı e-posta ayarları, test e-postası, günlük otomatik gönderim.
- ✅ `Bildirim Sistemi` tamamlandı - Vade yaklaşan fatura bildirimleri, ehliyet/muayene/sigorta bitiş uyarıları, uygulama içi bildirimler.
- ✅ `EBYS Örnek Veri ve Test Senaryoları` tamamlandı - 10 evrak kategorisi, 19 özlük evrak tanımı, 7 örnek evrak.
- ✅ `EBYS Semantic Search` tamamlandı - Ollama embedding API, Cosine Similarity algoritması, vektör tabanlı akıllı belge arama.
- ✅ `EBYS AI Entegrasyonu` tamamlandı - OCR (Tesseract), belge otomatik sınıflandırma, özet oluşturma, anahtar kelime çıkarma.
- ✅ `EBYS Belge Arama` sistemi tamamlandı - 4 kaynakta gelişmiş arama, paralel arama, alaka skoru hesaplama.
- ✅ `EBYS Belge Versiyon Kontrolü` tamamlandı - 3 belge tipi, geri yükleme, versiyon karşılaştırma.
- ✅ `EBYS Belge Yönetimi` menü gruplaması tamamlandı.
- ✅ `Destek` ve `Entegrasyon` bağlantıları `Ayarlar` altına taşındı.
- ✅ Global hata yakalama için `ters-giden-bir-sey` rapor ekranı eklendi.
- ✅ `Gelen Faturalar` sayfasında firma bazlı `Excel / XML / XML+PDF` import akışı iyileştirildi; import butonu artık yalnızca firma ve uygun dosya seçildiğinde aktif oluyor.
- ✅ `MaliAnalizService` içinde `Özmal / Kiralık / Komisyon` segment hesapları sahiplik kurallarına göre güncellendi.
- ✅ `FiloKomisyonService` içinde puantaj ve eşleştirme tahakkukları sahiplik tipine göre otomatik uygulanıyor.
- ✅ `Eşleştirme Şablonları` ve `Operasyon Puantajı` ekranlarında sahiplik kuralı görünür hale getirildi ve hatalı manuel girişler sınırlandı.
- ✅ `FiloOperasyonService` içinde komisyonculuk iş atamaları sahiplik tipine göre normalize ediliyor.
- ✅ `Komisyonculuk` detay ekranında atama yönetimi, sahiplik kuralı uyarıları ve ödeme takibi UI seviyesinde tamamlandı.
- ✅ `Mali Analiz Dashboard` içinde aylık/yıllık karşılaştırmalı trend sekmesi tamamlandı.
- ✅ Filo sahiplik kurallarının (`Özmal / Kiralık / Komisyon`) servis, puantaj, tahakkuk ve raporlara tam yayılımı tamamlandı.

### ? Tamamlanan Modüller (Faz 1 - MVP)
| Modül | Durum | Açıklama |
|-------|-------|----------|
| Cari Yönetimi | ? Tamamlandı | CRUD, Liste, Form |
| Araç Yönetimi | ? Tamamlandı | CRUD, Liste, Form |
| Şoför Yönetimi | ? Tamamlandı | CRUD, Liste, Form |
| Güzergah Yönetimi | ? Tamamlandı | CRUD, Liste, Form |
| Masraf Kalemleri | ? Tamamlandı | CRUD, Liste, Form |
| Araç Masrafları | ? Tamamlandı | CRUD, Liste, Form |
| Servis Çalışmaları | ? Tamamlandı | CRUD, Toplu Giriş |
| Fatura Yönetimi | ? Tamamlandı | CRUD, Detay |
| Banka/Kasa Hesapları | ? Tamamlandı | CRUD |
| Banka Hareketleri | ? Tamamlandı | CRUD |
| Ödeme Eşleştirme | ? Tamamlandı | Fatura-Hareket eşleştirme |
| Raporlar | ? Tamamlandı | Servis, Fatura, Araç Masraf, Cari Ekstre |
| Dashboard | ? Tamamlandı | Özet kartlar, optimize sorgular |
| UI/UX | ? Tamamlandı | Açılır menü, okunabilir renkler |
| İhale Hazırlık | ✅ Tamamlandı | Proje bazlı maliyet analizi, AI tahmin, enflasyonlu projeksiyon |

---

## ?? FAZ 2 - Gelişmiş Özellikler

### 2.1 Kullanıcı Yönetimi & Yetkilendirme
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| ASP.NET Core Identity entegrasyonu | ?? Yüksek | 3 gün | ✅ Tamamlandı |
| Kullanıcı kayıt/giriş | ?? Yüksek | 2 gün | ✅ Tamamlandı |
| Rol tabanlı yetkilendirme (Admin, Muhasebe, Operasyon) | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| Şifre sıfırlama | ?? Orta | 1 gün | ✅ Tamamlandı |
| Kullanıcı profil sayfası | 🟢 Düşük | 1 gün | ✅ Tamamlandı |

### 2.2 Bildirim Sistemi
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Vade yaklaşan fatura bildirimleri | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| Ehliyet/Muayene/Sigorta bitiş uyarıları | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| E-posta bildirimleri | 🟡 Orta | 2 gün | ✅ Tamamlandı |
| Uygulama içi bildirimler (toast) | 🟢 Düşük | 1 gün | ✅ Tamamlandı |

### 2.3 Doküman Yönetimi
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Fatura PDF oluşturma | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| Excel export (mevcut, iyileştirme) | 🟡 Orta | 1 gün | ✅ Tamamlandı |
| Dosya yükleme (ruhsat, ehliyet, sözleşme) | 🟡 Orta | 2 gün | ✅ Tamamlandı |

### 2.4 Destek Talepleri (Biletleme Sistemi)
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Entity ve Service Altyapısı | 🔴 Yüksek | 3 gün | ✅ Tamamlandı |
| Talep Liste ve Detay Sayfaları | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| Departman ve Kategori Yönetimi | 🟡 Orta | 1 gün | ✅ Tamamlandı |
| SLA Yönetimi ve Takibi | 🟡 Orta | 1 gün | ✅ Tamamlandı |
| Hazır Yanıt Şablonları | 🟡 Orta | 1 gün | ✅ Tamamlandı |
| Dashboard ve Raporlama | 🟡 Orta | 1 gün | ✅ Tamamlandı |
| Dosya Eki Desteği | 🟢 Düşük | 1 gün | ✅ Tamamlandı |
| E-posta Entegrasyonu | 🟢 Düşük | 2 gün | ✅ Tamamlandı |
| **osTicket Benzeri Kullanıcı Arayüzü** | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| **Kanban Yetkili Yönetim Paneli** | 🔴 Yüksek | 1 gün | ✅ Tamamlandı |
| **Durum Akışı (Taslak→Gönderildi→İşlemde→Bitti→Onaylandı)** | 🔴 Yüksek | 1 gün | ✅ Tamamlandı |

---

## 🚀 FAZ 3 - İleri Seviye Özellikler

### 3.1 Entegrasyonlar
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| E-Fatura entegrasyonu (GİB) | 🔴 Yüksek | 5 gün | ✅ |
| SMS entegrasyonu | 🟡 Orta | 2 gün | ✅ Tamamlandı |
| Harita entegrasyonu (güzergah gösterimi) | 🟢 Düşük | 3 gün | ✅ Tamamlandı |
| Araç takip sistemi entegrasyonu | 🟢 Düşük | 4 gün | ✅ Tamamlandı |

### 3.2 Gelişmiş Raporlama
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Dashboard grafikleri (Chart.js) | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| Şoför performans raporu | 🟡 Orta | 2 gün | ✅ Tamamlandı |
| Araç karlılık analizi | 🟡 Orta | 2 gün | ✅ Tamamlandı |
| Cari bakiye yaşlandırma raporu | 🟡 Orta | 2 gün | ✅ Tamamlandı |
| Aylık/Yıllık karşılaştırmalı raporlar | 🟢 Düşük | 2 gün | ✅ Tamamlandı |

### 3.3 Mobil Uygulama
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Şoför mobil uygulaması (MAUI Blazor) | 🟡 Orta | 10 gün | ✅ Tamamlandı |
| Sefer başlat/bitir | 🟡 Orta | 2 gün | ✅ Tamamlandı |
| Arıza bildirimi | 🟡 Orta | 1 gün | ✅ Tamamlandı |
| Masraf girişi (fotoğraflı) | 🟡 Orta | 2 gün | ✅ Tamamlandı |

---

## 🏢 FAZ 4 - Kurumsal Özellikler

### 4.1 Çoklu Şirket Desteği
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Multi-tenant mimari | 🟢 Düşük | 5 gün | ✅ Tamamlandı (Entity+Servis+UI) |
| Şirket bazlı veri izolasyonu | 🟢 Düşük | 3 gün | ✅ Tamamlandı (Global Query Filter) |
| Şirketler arası transfer | 🟢 Düşük | 2 gün | ✅ Tamamlandı |

### 4.2 API & Entegrasyon
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| REST API oluşturma | 🟡 Orta | 4 gün | ✅ Tamamlandı |
| API dokümantasyonu (Swagger) | 🟡 Orta | 1 gün | ✅ Tamamlandı |
| Webhook desteği | 🟢 Düşük | 2 gün | ✅ Tamamlandı |

### 4.3 Performans & Ölçekleme
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Redis cache entegrasyonu | 🟢 Düşük | 2 gün | ✅ Tamamlandı |
| Sayfalama (pagination) | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| Lazy loading optimizasyonu | 🟡 Orta | 1 gün | ✅ Tamamlandı |

---

## 🛠️ FAZ 5 - Bug Fix & Personel/Maaş İyileştirmeleri

### 5.1 Kritik Bug Fix'ler
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Personel düzenleme DB'ye kaydetmiyor - BUG | 🔴 Yüksek | 1 gün | ✅ Tamamlandı |
| Maaş yönetimi "Ödeme Yap" butonu pasif - BUG | 🔴 Yüksek | 1 gün | ✅ Tamamlandı |

### 5.2 Maaş Yönetimi İyileştirmeleri
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Yol parası alanı ekleme | 🔴 Yüksek | 1 gün | ✅ Tamamlandı |
| Hareket listesi (ödeme geçmişi) | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| Toplu ödeme listesi (banka formatı) | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| Tekli ödeme girişi (liste üzerinden) | 🟡 Orta | 1 gün | ✅ Tamamlandı |
| Tüm aylar/yıllar filtreleme | 🟡 Orta | 1 gün | ✅ Tamamlandı |
| Hareket listesi yazdır (ön izleme) | 🟡 Orta | 1 gün | ✅ Tamamlandı |
| Hareket listesi Excel export | 🟡 Orta | 1 gün | ✅ Tamamlandı |
| **SGK Maaş / Kalan Maaş Ayrımı** | 🔴 Yüksek | 1 gün | ✅ Tamamlandı |
| **Banka Ödeme Listesi SGK/Kalan Gösterimi** | 🔴 Yüksek | 1 gün | ✅ Tamamlandı |
| **Maaşa Mahsup (Masraf/Ödeme)** | 🟡 Orta | 2 gün | ✅ Tamamlandı |

### 5.3 Bordro Yönetimi
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Normal bordro (personel bazlı düzenleme) | 🔴 Yüksek | 3 gün | ✅ Tamamlandı |
| AR-GE bordro (personel bazlı düzenleme) | 🔴 Yüksek | 3 gün | ✅ Tamamlandı |
| Bordro hesap pusulası (personel bazlı) | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| SGK personel normal/AR-GE ayırma | 🟡 Orta | 1 gün | ✅ Tamamlandı |

### 5.4 Personel Excel Import
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Excel'den personel yükleme | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| Mevcut personel kontrolü (kaydetmeme) | 🔴 Yüksek | 1 gün | ✅ Tamamlandı |
| SGK'lı normal/AR-GE otomatik ayırma | 🟡 Orta | 1 gün | ✅ Tamamlandı |

### 5.5 Muhasebe & Raporlama Geliştirmeleri (YENİ - ÖNCELİKLİ)
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Tarih bazlı Yevmiye Kayıtları Yazdır | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| Yevmiye Kayıtları Excel Export | 🔴 Yüksek | 1 gün | ✅ Tamamlandı |
| Fatura Toplu Muhasebeleştirme | 🔴 Yüksek | 2 gün | ✅ |
| Masraf Muhasebeleştirme | 🔴 Yüksek | 2 gün | ✅ |
| Muhasebe Kontrol Listesi | 🟡 Orta | 1 gün | ✅ |
| **Kolay Muhasebe Girişi** | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |

### 5.6 Modül Geliştirmeleri (YENİ - ÖNCELİKLİ)
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Bütçe Analiz - Kategori bazlı analiz | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| Bütçe Analiz - Trend grafikleri | 🔴 Yüksek | 1 gün | ✅ Tamamlandı |
| Bütçe Analiz - AI Analiz (Ollama) | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| Bütçe Analiz - Hedef/Gerçekleşen karşılaştırma | 🟡 Orta | 2 gün | ✅ Tamamlandı |
| Cari - Risk analizi kartları | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| Cari - İletişim geçmişi | 🟡 Orta | 1 gün | ✅ Tamamlandı |
| Cari - Otomatik hatırlatmalar | 🟡 Orta | 2 gün | ✅ Tamamlandı |
| **Fatura - Toplu fatura oluşturma** | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| **Fatura - E-Fatura XML (GİB UBL-TR)** | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| **Fatura - Luca Portal Entegrasyonu** | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| **Fatura - Fatura şablonları** | 🟡 Orta | 2 gün | ✅ Tamamlandı |
| Fatura - E-fatura hazırlık | 🟡 Orta | 2 gün | ✅ Tamamlandı |
| **Puantaj Excel Import Sistemi** | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| **Puantaj Günlük Kayıt (Gun01-31)** | 🔴 Yüksek | 1 gün | ✅ Tamamlandı |

---

## 📋 FAZ 6 - Puantaj & Fatura Yönetimi

### 6.1 Puantaj Sistemi
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Puantaj listesi (aylık görünüm) | 🔴 Yüksek | 3 gün | ✅ Tamamlandı |
| Son puantaj tablosu hazırlama | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| Puantaj → Bordro otomatik aktarım | 🟡 Orta | 2 gün | ✅ Tamamlandı |
| Puantaj onay sistemi | 🟢 Düşük | 2 gün | ✅ Tamamlandı |
| **Puantaj Excel şablon indirme** | 🔴 Yüksek | 1 gün | ✅ Tamamlandı |
| **Puantaj Excel import/önizleme** | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| **Güzergah/Şoför otomatik oluşturma** | 🟡 Orta | 1 gün | ✅ Tamamlandı |

### 6.2 Gelecek Fatura Yönetimi
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Kesilecek fatura listesi | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| Gelecek fatura listesi | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| Proforma fatura oluşturma | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| Proforma → Gerçek fatura dönüştürme | 🟡 Orta | 1 gün | ✅ Tamamlandı |
| Kesilen/gelen fatura eşleştirme | 🟡 Orta | 2 gün | ✅ Tamamlandı |

### 6.3 Resmi Raporlar
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| SGK bildirge raporu | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| Muhtasar beyanname raporu | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| Ücret bordro icmal raporu | 🟡 Orta | 2 gün | ✅ Tamamlandı |
| İşe giriş/çıkış bildirge | 🟡 Orta | 1 gün | ✅ Tamamlandı |
| Yıllık izin takip raporu | 🟢 Düşük | 1 gün | ✅ Tamamlandı |

---

## 🤖 FAZ 7 - Elektronik Belge Yönetim Sistemi (EBYS)

### 7.1 Temel EBYS Altyapısı
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Belge kategorileri (sözleşme, ehliyet, diploma vb.) | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| Dosya yükleme/indirme sistemi | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| Belge metadata yönetimi | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| Versiyon kontrolü | 🟡 Orta | 2 gün | ✅ Tamamlandı |
| Belge arama (içerik + metadata) | 🟡 Orta | 2 gün | ✅ Tamamlandı |

### 7.2 Personel Dosyaları
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Personel özlük dosyası | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| Ehliyet/Diploma/Sertifika yükleme | 🔴 Yüksek | 1 gün | ✅ Tamamlandı |
| Sağlık raporu yükleme | 🟡 Orta | 1 gün | ✅ Tamamlandı |
| İş sözleşmesi yönetimi | 🟡 Orta | 1 gün | ✅ Tamamlandı |
| Belge bitiş tarihi uyarıları | 🟡 Orta | 1 gün | ✅ Tamamlandı |

### 7.3 Gelen/Giden Evrak Yönetimi (YENİ)
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Gelen evrak girişi | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| Giden evrak girişi | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| Evrak atama sistemi | 🔴 Yüksek | 1 gün | ✅ Tamamlandı |
| Evrak takip paneli (Kanban) | 🔴 Yüksek | 1 gün | ✅ Tamamlandı |
| Evrak kategori yönetimi | 🟡 Orta | 1 gün | ✅ Tamamlandı |
| Evrak dosya ekleme | 🔴 Yüksek | 1 gün | ✅ Tamamlandı |
| Durum akışı yönetimi | 🔴 Yüksek | 1 gün | ✅ Tamamlandı |
| İşlem geçmişi/log | 🟡 Orta | 1 gün | ✅ Tamamlandı |
| Cevap süresi takibi | 🟡 Orta | 1 gün | ✅ Tamamlandı |
| Dashboard istatistikleri | 🟡 Orta | 1 gün | ✅ Tamamlandı |

### 7.4 Yapay Zeka Desteği (Offline Çalışabilir)
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Local LLM entegrasyonu (Ollama/LLaMA) | 🔴 Yüksek | 3 gün | ✅ Tamamlandı |
| **AI Asistan Floating Widget** | 🔴 Yüksek | 1 gün | ✅ Tamamlandı |
| Belge otomatik sınıflandırma | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| OCR ile belge içerik çıkarma | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| Akıllı belge arama (semantic search) | 🟡 Orta | 3 gün | ✅ Tamamlandı |
| Belge özeti oluşturma | 🟡 Orta | 2 gün | ✅ Tamamlandı |
| Offline mod (internetsiz çalışma) | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |

### 7.5 Örnek Veri & Test
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Örnek personel verisi oluşturma | 🟡 Orta | 1 gün | ✅ Tamamlandı |
| Örnek bordro verisi oluşturma | 🟡 Orta | 1 gün | ✅ Tamamlandı |
| Örnek fatura verisi oluşturma | 🟡 Orta | 1 gün | ✅ Tamamlandı |
| Test senaryoları | 🟢 Düşük | 2 gün | ✅ Tamamlandı |

---

## 📊 FAZ 8 - İhale Hazırlık & Teklif Yönetimi

### 8.1 İhale Proje Yönetimi
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Proje CRUD (sınırsız proje) | 🔴 Yüksek | 2 gün | ✅ |
| Proje kopyalama | 🔴 Yüksek | 1 gün | ✅ |
| Güzergah/hat kalem yönetimi | 🔴 Yüksek | 2 gün | ✅ |
| Özmal/Kiralık/Komisyon araç durumları | 🔴 Yüksek | 1 gün | ✅ |
| Sözleşme süresi bazlı hesaplama | 🔴 Yüksek | 1 gün | ✅ |
| Proje durum takibi (Taslak→Kazanıldı) | 🟡 Orta | 1 gün | ✅ |

### 8.2 Maliyet Hesaplama
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Yakıt maliyet hesaplama (mesafe×sefer×tüketim) | 🔴 Yüksek | 1 gün | ✅ |
| Araç masraf 7 kategori (bakım/lastik/sigorta/kasko/muayene/yedek parça/diğer) | 🔴 Yüksek | 1 gün | ✅ |
| Şoför maaş hesaplama (brüt+SGK %22.5) | 🔴 Yüksek | 1 gün | ✅ |
| Kira/komisyon maliyet | 🔴 Yüksek | 1 gün | ✅ |
| Amortisman hesaplama | 🟡 Orta | 1 gün | ✅ |
| Birim fiyatlar (aylık/sefer/saat/km) | 🔴 Yüksek | 1 gün | ✅ |
| Kâr marjı ve teklif fiyat hesaplama | 🔴 Yüksek | 1 gün | ✅ |

### 8.3 AI Destekli Tahminler
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| AI araç masraf tahmini (Ollama) | 🔴 Yüksek | 1 gün | ✅ |
| AI şoför maaş tahmini (enflasyon dahil) | 🔴 Yüksek | 1 gün | ✅ |
| AI proje stratejik analizi | 🟡 Orta | 1 gün | ✅ |
| Gerçek masraf verilerinden tahmin | 🟡 Orta | 1 gün | ✅ |
| Kullanıcının AI tahminini değiştirmesi | 🔴 Yüksek | 1 gün | ✅ |

### 8.4 Enflasyonlu Projeksiyon & Raporlama
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Aylık enflasyonlu maliyet projeksiyonu | 🔴 Yüksek | 1 gün | ✅ |
| Yakıt ayrı zam oranı hesaplama | 🔴 Yüksek | 1 gün | ✅ |
| Kâr/zarar/masraf tablosu | 🔴 Yüksek | 1 gün | ✅ |
| Kümülatif maliyet/kâr raporu | 🟡 Orta | 1 gün | ✅ |
| Proje özet kartları | 🟡 Orta | 1 gün | ✅ |
| **Örnek Veri Oluşturma (Test Data Seeding)** | 🟡 Orta | 1 gün | ✅ Tamamlandı |

### 8.5 Teklif Operasyonları & Karar Destek (YENİ)
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Teklif versiyonlama ve revizyon geçmişi | 🔴 Yüksek | 2 gün | ❌ Bekliyor |
| Teklif onay akışı (Hazırlayan → Yönetici → Onaylandı) | 🔴 Yüksek | 2 gün | ❌ Bekliyor |
| Teklif PDF / Excel export | 🔴 Yüksek | 1 gün | ❌ Bekliyor |
| Senaryo karşılaştırma (A/B teklif modeli) | 🟡 Orta | 2 gün | ❌ Bekliyor |
| Rakip / piyasa teklif benchmark alanları | 🟡 Orta | 2 gün | ❌ Bekliyor |
| Teklif notları ve karar günlüğü | 🟢 Düşük | 1 gün | ❌ Bekliyor |

### 8.6 Kazanılan Proje Gerçekleşen Takibi (YENİ)
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Kazanılan proje için gerçekleşen maliyet takibi | 🔴 Yüksek | 2 gün | ❌ Bekliyor |
| Tekliflenen vs gerçekleşen sapma analizi | 🔴 Yüksek | 2 gün | ❌ Bekliyor |
| Sözleşme revizyon ve ek protokol takibi | 🟡 Orta | 2 gün | ❌ Bekliyor |
| Hat/güzergah bazlı gerçekleşen kârlılık | 🟡 Orta | 2 gün | ❌ Bekliyor |
| AI tahmin geri besleme (teklif doğruluk skoru) | 🟡 Orta | 2 gün | ❌ Bekliyor |
| İhale sonrası operasyon dashboard kartları | 🟢 Düşük | 1 gün | ❌ Bekliyor |

### 8.7 FAZ 8.5 İlk Sprint Backlog'u (ÖNERİLEN)
| İş Paketi | İçerik | Öncelik | Süre | Durum |
|-----------|--------|---------|------|-------|
| Teklif versiyon entity/model tasarımı | `IhaleProje` ile ilişkili teklif versiyon tablosu, revizyon no, durum, açıklama, hazırlayan kullanıcı | 🔴 Yüksek | 1 gün | ❌ Bekliyor |
| Teklif versiyon servis katmanı | Yeni versiyon oluştur, aktif versiyonu kopyala, revizyon geçmişi listele, karşılaştırma için temel DTO'lar | 🔴 Yüksek | 1 gün | ❌ Bekliyor |
| Teklif revizyon UI | İhale detay ekranında “Versiyonlar” sekmesi, aktif versiyon rozetleri, revizyon notu modalı | 🔴 Yüksek | 1 gün | ❌ Bekliyor |
| Teklif onay akışı altyapısı | Hazırlandı / İncelemede / Onaylandı / Reddedildi durumları, onaylayan kullanıcı ve zaman damgası | 🔴 Yüksek | 1 gün | ❌ Bekliyor |
| Teklif onay UI aksiyonları | Yönetici onayla/reddet butonları, durum geçmişi ve karar notu alanı | 🔴 Yüksek | 1 gün | ❌ Bekliyor |
| Teklif export altyapısı | PDF/Excel çıktı servisi, teklif özeti + maliyet tablosu + kâr marjı bölümleri | 🟡 Orta | 1 gün | ❌ Bekliyor |

### 8.8 FAZ 8.5 Sprint Çıkış Kriterleri
- Aynı ihale projesi için birden fazla teklif versiyonu oluşturulabilmeli.
- Kullanıcı önceki teklif versiyonunu kopyalayarak yeni revizyon başlatabilmeli.
- Yönetici rolü teklif versiyonunu onaylayıp reddedebilmeli.
- Onay/red işlemlerinde karar notu ve işlem tarihi saklanmalı.
- Onaylı teklif PDF ve Excel olarak dışa aktarılabilmeli.
- İhale detay ekranında aktif teklif versiyonu ve versiyon geçmişi görünür olmalı.

### 8.9 FAZ 8.5 Teknik Taslak (Sprint 1)

#### Veri Modeli Önerisi
- `IhaleTeklifVersiyon`
  - `Id`
  - `IhaleProjeId`
  - `VersiyonNo`
  - `RevizyonKodu` (`V1`, `V2`, `REV-A` vb.)
  - `Durum` (`Taslak`, `Incelemede`, `Onaylandi`, `Reddedildi`)
  - `RevizyonNotu`
  - `KararNotu`
  - `HazirlayanKullaniciId`
  - `OnaylayanKullaniciId`
  - `HazirlamaTarihi`
  - `OnayTarihi`
  - `AktifVersiyon`
  - `ToplamMaliyet`
  - `TeklifTutari`
  - `KarMarjiTutari`
  - `KarMarjiOrani`
  - `CreatedAt / UpdatedAt / IsDeleted`

#### İlişkili Yardımcı Yapılar
- `IhaleTeklifVersiyonKalem` (opsiyonel ikinci adım)
  - hat/güzergah bazlı snapshot saklamak için
- `IhaleTeklifKararLog`
  - onay/red/geçiş işlemlerinin zaman çizelgesi için
- `IhaleTeklifKarsilastirmaDto`
  - iki versiyon arasındaki maliyet / teklif / kâr farklarını göstermek için

#### Servis Katmanı Önerisi
- `IIhaleTeklifVersiyonService`
  - `CreateInitialVersionAsync(int ihaleProjeId)`
  - `CreateRevisionAsync(int ihaleProjeId, int kaynakVersiyonId, string? revizyonNotu)`
  - `GetVersiyonlarAsync(int ihaleProjeId)`
  - `GetAktifVersiyonAsync(int ihaleProjeId)`
  - `SetIncelemeDurumuAsync(int versiyonId)`
  - `ApproveAsync(int versiyonId, string? kararNotu)`
  - `RejectAsync(int versiyonId, string? kararNotu)`
  - `CompareAsync(int solVersiyonId, int sagVersiyonId)`

#### UI Kırılımı
- `IhaleDetay.razor`
  - yeni `Teklif Versiyonları` sekmesi
  - aktif versiyon kartı
  - versiyon listesi + durum rozetleri
  - `Yeni Revizyon Oluştur` aksiyonu
- `TeklifVersiyonKarsilastirma.razor` veya modal
  - iki versiyonun maliyet / teklif / kâr karşılaştırması
- `TeklifOnayGecmisi` paneli
  - kim, ne zaman, hangi karar notuyla işlem yaptı

#### Teknik Uygulama Sırası
1. Entity + migration
2. DbContext kaydı + temel query/indexler
3. Service implementasyonu
4. İhale detay ekranı sekme entegrasyonu
5. Onay akışı ve karar logları
6. Export çıktıları

#### Sprint 1 Dışı Ama Yakın Devam İşleri
- teklif snapshot'ını kalem bazında saklama
- versiyon bazlı PDF şablon özelleştirme
- onay akışı için rol/yetki matrisi
- gerçekleşen vs tekliflenen sapma ekranının versiyonla bağlanması

### 8.10 FAZ 8.5 Kod Uygulama Haritası

#### Önerilen Dosya/Dizin Kırılımı
- `CRMFiloServis.Shared/Entities/`
  - `IhaleTeklifVersiyon.cs`
  - `IhaleTeklifKararLog.cs`
- `CRMFiloServis.Web/Data/`
  - `ApplicationDbContext.cs` içine `DbSet` tanımları
- `CRMFiloServis.Web/Data/Migrations/`
  - teklif versiyon tabloları için yeni migration
- `CRMFiloServis.Web/Services/Interfaces/`
  - `IIhaleTeklifVersiyonService.cs`
- `CRMFiloServis.Web/Services/`
  - `IhaleTeklifVersiyonService.cs`
- `CRMFiloServis.Web/Components/Pages/Ihale/`
  - mevcut `IhaleDetay.razor` içine sekme entegrasyonu
  - gerekirse `TeklifVersiyonKarsilastirma.razor` bileşeni

#### İlk Uygulama Iterasyonu
1. `Entity` tanımları oluşturulacak.
2. `DbContext` içine tablolar ve ilişkiler eklenecek.
3. `Migration` üretilecek ve mevcut PostgreSQL/SQLite akışıyla uyumu kontrol edilecek.
4. `Service` katmanında temel CRUD + revizyon kopyalama yazılacak.
5. `IhaleDetay` ekranında listeleme ve yeni revizyon oluşturma aksiyonu açılacak.

#### İlişki ve Kısıt Önerileri
- Bir `IhaleProje` için birden fazla `IhaleTeklifVersiyon` olabilir.
- Aynı projede yalnızca bir kayıt `AktifVersiyon = true` olmalı.
- `VersiyonNo` proje bazında artan sırada tutulmalı.
- `OnaylayanKullaniciId` ve `HazirlayanKullaniciId` mevcut `Kullanici` tablosuna bağlanmalı.
- `IsDeleted` kullanılan mevcut soft delete yaklaşımı korunmalı.

#### Teknik Riskler
- mevcut ihale modülü sorgularının aktif versiyon mantığına adapte edilmesi gerekebilir
- migration zincirindeki mevcut PostgreSQL hassasiyetleri nedeniyle yeni migration küçük ve izole tutulmalı
- PDF/Excel export tarafında mevcut ihale çıktı servisleri varsa tekrar kullanılmalı
- aktif versiyon değişiminde cache ve rapor ekranları etkilenebilir

#### Tamamlanma Kontrol Listesi
- [ ] Entity sınıfları eklendi
- [ ] `DbContext` ilişkileri tanımlandı
- [ ] Migration oluşturuldu
- [ ] Build başarılı
- [ ] Revizyon oluşturma servisi çalışıyor
- [ ] Aktif versiyon listelemesi UI'da görünüyor
- [ ] Onay durumu rozetleri gösteriliyor

### 8.11 FAZ 8.5 Sprint 2-3 Backlog'u

#### Sprint 2 - Karşılaştırma ve Çıktılar
| İş Paketi | İçerik | Öncelik | Süre | Durum |
|-----------|--------|---------|------|-------|
| Versiyon karşılaştırma servisi | iki teklif versiyonu arasında maliyet, teklif, kâr ve oran farkları | 🔴 Yüksek | 1 gün | ❌ Bekliyor |
| Karşılaştırma UI | yan yana versiyon karşılaştırma ekranı / modalı | 🔴 Yüksek | 1 gün | ❌ Bekliyor |
| Teklif PDF çıktısı | yönetim özeti, maliyet tablosu, varsayımlar, kâr marjı | 🔴 Yüksek | 1 gün | ❌ Bekliyor |
| Teklif Excel çıktısı | kalem bazlı teklif ve özet sayfaları | 🟡 Orta | 1 gün | ❌ Bekliyor |
| Karar günlüğü görünümü | onay/red/inceleme aksiyonlarının zaman çizelgesi | 🟡 Orta | 1 gün | ❌ Bekliyor |

#### Sprint 3 - Gerçekleşen ve Sapma Takibi
| İş Paketi | İçerik | Öncelik | Süre | Durum |
|-----------|--------|---------|------|-------|
| Gerçekleşen maliyet veri modeli | proje/hat/güzergah bazlı gerçekleşen kayıt yapısı | 🔴 Yüksek | 1 gün | ❌ Bekliyor |
| Teklif vs gerçekleşen hesaplama servisi | sapma tutarı, sapma oranı, kârlılık farkı | 🔴 Yüksek | 1 gün | ❌ Bekliyor |
| Sapma dashboard kartları | toplam sapma, en yüksek sapma, riskli projeler | 🔴 Yüksek | 1 gün | ❌ Bekliyor |
| Hat/güzergah bazlı sapma raporu | kırılım bazlı analiz tablosu | 🟡 Orta | 1 gün | ❌ Bekliyor |
| AI doğruluk geri besleme skoru | tahmin edilen ve gerçekleşen farklara göre skor üretimi | 🟡 Orta | 1 gün | ❌ Bekliyor |

### 8.12 FAZ 8.5 MVP Teslim Kapsamı
- teklif versiyon oluşturma
- önceki versiyondan revizyon kopyalama
- aktif versiyon seçimi
- onay / red akışı
- karar notu ve işlem geçmişi
- PDF / Excel teklif çıktısı
- temel versiyon karşılaştırma ekranı

### 8.13 FAZ 8.5 Sonrası Genişleme Alanları
- rakip teklif veri havuzu ve benchmark önerileri
- teklif şablon bazlı dinamik çıktı sistemi
- proje türüne göre farklı onay akışları
- şirket bazlı teklif numaralandırma kuralları
- mobil onay deneyimi

### 8.14 FAZ 8.5 Veri Kaynağı ve Snapshot Stratejisi

#### Snapshot'ta Dondurulacak Alanlar
- `ToplamMaliyet`
- `TeklifTutari`
- `KarMarjiTutari`
- `KarMarjiOrani`
- temel varsayımlar (`yakit`, `maas`, `kira/komisyon`, `enflasyon`, `sozlesme suresi`)
- özet hat/güzergah dağılımı

#### Canlıdan Okunacak Alanlar
- `IhaleProje` temel kimlik bilgileri
- proje sahibi kullanıcı ve organizasyon bilgileri
- yetki/rol kontrolleri
- export sırasında kullanılacak güncel firma bilgileri

#### Neden Snapshot Gerekli?
- teklif onaylandıktan sonra maliyet varsayımları değişse bile eski teklif bozulmamalı
- revizyonlar arasında karşılaştırma yapılabilmeli
- sapma analizi için referans teklif sayısı korunmalı
- PDF/Excel çıktılarında sonradan değişen veri nedeniyle tutarsızlık oluşmamalı

#### Gerçekleşen Veri Kaynağı Adayları
- `ServisCalisma`
- `AracMasraf`
- `Personel / Sofor` maliyet kayıtları
- `Fatura` ve tahakkuk kayıtları
- gerekiyorsa hat/güzergah bazlı özet tablo

#### Teknik Tasarım Notu
- ilk iterasyonda tam kalem snapshot yerine özet finansal snapshot yeterli olabilir
- ikinci iterasyonda `IhaleTeklifVersiyonKalem` ile satır bazlı snapshot açılmalı
- gerçekleşen maliyet analizi başlamadan önce hangi servislerin referans veri kaynağı olacağı netleştirilmeli

### 8.15 FAZ 8.5 Entegrasyon Noktaları ve Servis Sözleşmeleri

#### Entegrasyon Noktaları
- `IhaleProje` detay sayfası
- mevcut kullanıcı / rol / yetki altyapısı
- mevcut `Excel` ve `PDF` export servisleri
- maliyet hesaplama servisleri
- audit/log altyapısı

#### Önerilen Servis Sözleşmeleri
- `IIhaleTeklifVersiyonService`
  - `GetByIdAsync(int versiyonId)`
  - `GetListByIhaleProjeIdAsync(int ihaleProjeId)`
  - `CreateInitialAsync(int ihaleProjeId)`
  - `CreateRevisionAsync(int kaynakVersiyonId, string? revizyonNotu)`
  - `SetActiveAsync(int versiyonId)`
  - `SendToReviewAsync(int versiyonId)`
  - `ApproveAsync(int versiyonId, string? kararNotu)`
  - `RejectAsync(int versiyonId, string kararNotu)`

- `IIhaleTeklifKarsilastirmaService`
  - `CompareAsync(int solVersiyonId, int sagVersiyonId)`

- `IIhaleTeklifExportService`
  - `ExportPdfAsync(int versiyonId)`
  - `ExportExcelAsync(int versiyonId)`

#### DTO Önerileri
- `IhaleTeklifVersiyonListDto`
- `IhaleTeklifVersiyonDetayDto`
- `IhaleTeklifKarsilastirmaDto`
- `IhaleTeklifKararLogDto`

#### Entegrasyon Kuralları
- `IhaleDetay` ekranı doğrudan entity yerine servis DTO'ları kullanmalı
- export işlemleri yalnızca `Onaylandi` veya yetkili kullanıcı senaryosunda açılmalı
- onay/red akışları audit log ve karar log ile birlikte çalışmalı
- aktif versiyon güncellemesi sonrası ekran cache'leri temizlenmeli veya yenilenmeli

### 8.16 FAZ 8.5 Blazor UI Bileşen Haritası

#### Sayfa ve Bileşen Önerileri
- `IhaleDetay.razor`
  - `TeklifVersiyonlariTablosu`
  - `AktifTeklifVersiyonKarti`
  - `TeklifKararGecmisiPaneli`
- `TeklifRevizyonModal.razor`
  - revizyon notu girme
  - kaynak versiyon özeti gösterme
- `TeklifKarsilastirmaModal.razor`
  - iki versiyon seçimi
  - özet fark tablosu
- `TeklifOnayModal.razor`
  - onay / red karar notu girişi

#### UI Akışları
- kullanıcı `IhaleDetay` ekranında versiyon listesini görür
- aktif versiyon kartından teklif özeti okunur
- `Yeni Revizyon Oluştur` ile modal açılır
- `İncelemeye Gönder` sonrası durum rozeti güncellenir
- yönetici `Onayla / Reddet` aksiyonu ile karar sürecini tamamlar
- `Karşılaştır` aksiyonu ile iki versiyon yan yana açılır

#### Görsel Durum Rozetleri
- `Taslak` -> gri
- `Incelemede` -> sarı
- `Onaylandi` -> yeşil
- `Reddedildi` -> kırmızı
- `Aktif Versiyon` -> ek mavi vurgu

#### İlk UI Teslim Kapsamı
- versiyon listesi
- aktif versiyon kartı
- revizyon oluşturma modalı
- onay / red aksiyonları
- karar geçmişi listesi

#### Sonraki UI Genişleme Alanları
- sürükle-bırak senaryo karşılaştırma
- versiyon bazlı grafik fark görünümü
- mobil uyumlu onay ekranı
- export önizleme paneli

---

## 📅 Önerilen Uygulama Takvimi

```
FAZ 2 (4-5 Hafta)
├── Hafta 1-2: Kullanıcı Yönetimi & Yetkilendirme
├── Hafta 3: Bildirim Sistemi
└── Hafta 4-5: Doküman Yönetimi

FAZ 3 (6-8 Hafta)
├── Hafta 1-2: Dashboard Grafikleri & Gelişmiş Raporlar ✅
├── Hafta 3-4: E-Fatura Entegrasyonu
├── Hafta 5-6: SMS & E-posta Entegrasyonu
└── Hafta 7-8: Mobil Uygulama (opsiyonel)

FAZ 4 (4-6 Hafta)
├── Hafta 1-2: REST API
├── Hafta 3-4: Multi-tenant (opsiyonel)
└── Hafta 5-6: Performans optimizasyonları

FAZ 5 (3-4 Hafta) - ÖNCELİKLİ
├── Hafta 1: Bug Fix'ler (Personel düzenleme, Ödeme Yap butonu)
├── Hafta 2: Maaş Yönetimi İyileştirmeleri
├── Hafta 3: Bordro Yönetimi (Normal + AR-GE)
└── Hafta 4: Personel Excel Import

FAZ 6 (4-5 Hafta)
├── Hafta 1-2: Puantaj Sistemi
├── Hafta 3: Gelecek/Kesilecek Fatura Yönetimi
├── Hafta 4: Proforma Fatura
└── Hafta 5: Resmi Raporlar

FAZ 7 (5-6 Hafta)
├── Hafta 1-2: EBYS Temel Altyapı
├── Hafta 3: Personel Dosyaları
├── Hafta 4-5: AI Entegrasyonu (Local LLM + OCR)
└── Hafta 6: Örnek Veri & Test

FAZ 8 (2-3 Hafta) - ✅ ÇEKİRDEK KAPSAM TAMAMLANDI
├── Hafta 1: İhale Proje CRUD + Maliyet Hesaplama
├── Hafta 2: AI Tahmin + Enflasyonlu Projeksiyon
└── Hafta 3: Kâr/Zarar Rapor + Proje Kopyalama

FAZ 8.5 (2-3 Hafta) - 🆕 ÖNERİLEN DEVAM FAZI
├── Hafta 1: Teklif versiyonlama + onay akışı
├── Hafta 2: Teklif export + senaryo karşılaştırma
└── Hafta 3: Kazanılan proje gerçekleşen / sapma takibi
```

---

## 🚀 FAZ 9 - İleri Seviye Özellikler & Optimizasyonlar

### 9.1 Gelişmiş Analitik & BI
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Power BI / Grafana entegrasyonu | 🟡 Orta | 3 gün | ❌ Bekliyor |
| Özel dashboard widget oluşturma | 🟡 Orta | 2 gün | ❌ Bekliyor |
| Veri export (CSV/JSON/Parquet) | 🟢 Düşük | 1 gün | ✅ Tamamlandı |
| Scheduled report e-posta | 🟢 Düşük | 2 gün | ❌ Bekliyor |

### 9.2 Güvenlik & Uyumluluk
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| İki faktörlü doğrulama (2FA/MFA) | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| Audit log (tüm işlemler) | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| GDPR/KVKK uyumluluk araçları | 🟡 Orta | 3 gün | ❌ Bekliyor |
| IP beyaz liste / kara liste | 🟢 Düşük | 1 gün | ❌ Bekliyor |

### 9.3 Performans & Altyapı
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Background job sistemi (Hangfire/Quartz) | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| Database index optimizasyonu | 🟡 Orta | 1 gün | ✅ Tamamlandı |
| Harici object storage entegrasyonu (S3 vb.) | 🟡 Orta | 2 gün | ❌ Bekliyor |
| Health check & monitoring | 🟢 Düşük | 1 gün | ✅ Tamamlandı |

### 9.4 Kullanıcı Deneyimi
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Dark mode / tema özelleştirme | 🟢 Düşük | 1 gün | ❌ Bekliyor |
| Keyboard shortcuts | 🟢 Düşük | 1 gün | ❌ Bekliyor |
| Favoriler / hızlı erişim | 🟢 Düşük | 1 gün | ❌ Bekliyor |
| Çoklu dil desteği (i18n) | 🟡 Orta | 3 gün | ❌ Bekliyor |

### 9.5 Entegrasyon & Otomasyon
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Zapier / n8n entegrasyonu | 🟢 Düşük | 2 gün | ❌ Bekliyor |
| Microsoft Teams bildirimleri | 🟢 Düşük | 1 gün | ❌ Bekliyor |
| Slack entegrasyonu | 🟢 Düşük | 1 gün | ❌ Bekliyor |
| Otomatik yedekleme sistemi | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |

---

## 🎯 Hemen Başlanabilecek Öncelikli İşler

### ✅ FAZ 1-8 Çekirdek Kapsam Tamamlandı

Proje MVP'den kurumsal seviyeye kadar tüm temel özellikleri içermektedir:
- ✅ FAZ 1: MVP (Cari, Araç, Şoför, Fatura, Raporlar)
- ✅ FAZ 2: Gelişmiş Özellikler (Kullanıcı, Bildirim, Doküman, Destek)
- ✅ FAZ 3: İleri Seviye (E-Fatura, SMS, GPS, Mobil)
- ✅ FAZ 4: Kurumsal (Multi-tenant, API, Cache)
- ✅ FAZ 5: Personel/Maaş/Bordro
- ✅ FAZ 6: Puantaj & Fatura Yönetimi
- ✅ FAZ 7: EBYS & AI Entegrasyonu
- ✅ FAZ 8: İhale Hazırlık & Teklif (çekirdek kapsam)

### 🆕 FAZ 8'den Devam Önerisi
1. **Teklif versiyonlama ve revizyon geçmişi**
2. **Teklif onay akışı ve karar günlüğü**
3. **Teklif PDF / Excel export**
4. **Kazanılan projelerde gerçekleşen vs tekliflenen sapma analizi**
5. **AI tahmin doğruluk skoru ve geri besleme mekanizması**

#### Alt Kırılım Önerisi

**1. Teklif versiyonlama ve revizyon geçmişi**
- aynı `IhaleProje` altında birden fazla teklif revizyonu tutulması
- aktif versiyon işaretleme
- önceki versiyondan kopyalayarak yeni revizyon oluşturma
- versiyon bazlı maliyet / teklif / kâr verisinin dondurulması

**2. Teklif onay akışı ve karar günlüğü**
- `Taslak -> Incelemede -> Onaylandi / Reddedildi` durum geçişleri
- onaylayan kullanıcı, karar tarihi ve karar notu saklama
- yönetici rolü için onay/red aksiyonları
- işlem geçmişinin zaman çizelgesi şeklinde gösterimi

**3. Teklif PDF / Excel export**
- yönetici özeti içeren PDF çıktı
- kalem bazlı teklif detaylarını içeren Excel çıktı
- versiyon bazlı çıktı alma
- onaylı tekliflerin resmi paylaşım formatına dönüştürülmesi

**4. Kazanılan projelerde gerçekleşen vs tekliflenen sapma analizi**
- teklif maliyeti ile operasyon sırasında oluşan gerçek maliyetin karşılaştırılması
- toplam sapma, oran sapması ve kârlılık farkı hesaplanması
- hat / güzergah / araç tipi bazlı kırılımlar
- riskli projeler için uyarı kartları

**5. AI tahmin doğruluk skoru ve geri besleme mekanizması**
- AI tarafından önerilen maliyet ile gerçekleşen maliyet farkının izlenmesi
- tahmin doğruluk puanı üretilmesi
- proje türüne göre model başarımının ölçülmesi
- gelecekteki tekliflerde AI önerilerinin daha güvenilir hale getirilmesi

#### Bu Bölümün Hedef Çıktısı
- teklif hazırlama süreci denetlenebilir hale gelir
- hangi teklifin ne zaman ve kim tarafından onaylandığı izlenebilir
- satış / operasyon / yönetim ekipleri aynı versiyon üstünden çalışır
- kazanılan projelerde teklif kalitesinin ölçümü mümkün olur

### ▶️ Önerilen İlk Uygulama Sırası
1. `Teklif versiyon entity + migration`
2. `Teklif servis katmanı ve revizyon kopyalama`
3. `İhale detay ekranında versiyon sekmesi`
4. `Onay akışı ve karar günlüğü`
5. `PDF / Excel teklif çıktısı`
6. `Gerçekleşen vs tekliflenen sapma ekranı`

#### Önceliklendirilmiş Teknik Görev Listesi

**Aşama 1 - Veri Katmanı**
1. `IhaleTeklifVersiyon` entity sınıfını oluştur
2. `IhaleTeklifKararLog` entity sınıfını oluştur
3. `ApplicationDbContext` içine `DbSet` tanımlarını ekle
4. entity ilişkilerini ve index ihtiyaçlarını tanımla
5. migration üret ve mevcut veritabanı akışıyla uyumu doğrula

**Aşama 2 - İş Kuralları**
1. `IIhaleTeklifVersiyonService` arayüzünü tanımla
2. aktif versiyon getirme ve versiyon listeleme metodlarını yaz
3. önceki versiyondan revizyon kopyalama akışını ekle
4. onaya gönder / onayla / reddet iş kurallarını ekle
5. karar loglarını otomatik oluşturan yardımcı akışı yaz

**Aşama 3 - UI Katmanı**
1. `IhaleDetay.razor` içine `Teklif Versiyonları` sekmesini ekle
2. aktif versiyon kartı ve versiyon listesi göster
3. `Yeni Revizyon Oluştur` modalı ekle
4. onay / red aksiyon butonlarını ekle
5. karar geçmişi zaman çizelgesini göster

**Aşama 4 - Çıktı ve Analiz**
1. versiyon karşılaştırma DTO ve servis metodunu ekle
2. PDF teklif çıktısını üret
3. Excel teklif çıktısını üret
4. tekliflenen vs gerçekleşen sapma servis altyapısını başlat
5. ilk dashboard sapma kartlarını ekle

#### Teknik Öncelik Notu
- İlk teslim için kritik yol: `Entity -> DbContext -> Migration -> Service -> IhaleDetay UI`
- PDF/Excel ve sapma analizi ikinci aşamada açılabilir.
- PostgreSQL migration zinciri hassas olduğu için yeni migration küçük, tek amaçlı ve izole tutulmalı.

#### Bağımlılıklar
- `IhaleProje` detay ekranının mevcut veri modeli yeni versiyon yapısını desteklemeli.
- mevcut kullanıcı/rol altyapısı onay akışında yeniden kullanılmalı.
- PDF/Excel çıktıları için hâlihazırdaki export servisleri analiz edilmeli.
- operasyon verileriyle teklif sapma karşılaştırması için maliyet kaynakları netleştirilmeli.

#### Risk Azaltma Planı
- ilk migration sadece `teklif versiyon` ve `karar log` tablolarını içermeli.
- aktif versiyon mantığı önce servis seviyesinde güvence altına alınmalı, sonra UI'ya açılmalı.
- karşılaştırma ekranı ilk iterasyonda özet alanlarla sınırlı tutulmalı.
- sapma analizi başlamadan önce `gerçekleşen maliyet` veri kaynağı için doğrulama yapılmalı.
- export çıktılarında ilk teslimde sabit şablon kullanılmalı, dinamik şablon sonraya bırakılmalı.

#### Başarı Ölçütleri
- bir ihale için en az 2 teklif versiyonu hatasız oluşturulabiliyor olmalı.
- aktif versiyon değişimi sonrası detay ekranı doğru veriyi göstermeli.
- onay/red işlemleri log kaydı üretmeli.
- PDF ve Excel çıktısı onaylı versiyon üzerinden üretilebilmeli.
- tekliflenen ve gerçekleşen veri modeli bağlandığında ilk sapma metriği hesaplanabiliyor olmalı.

#### Kabul Test Senaryoları

**Senaryo 1 - İlk teklif versiyonu oluşturma**
- yeni bir `IhaleProje` açıldığında ilk teklif versiyonu oluşturulabilir olmalı
- varsayılan durum `Taslak` gelmeli
- `VersiyonNo = 1` olarak atanmalı

**Senaryo 2 - Revizyon kopyalama**
- mevcut aktif versiyondan yeni revizyon oluşturulmalı
- yeni kayıt bir önceki teklifin maliyet ve teklif özetini taşımalı
- yeni versiyonun `VersiyonNo` değeri artmalı

**Senaryo 3 - Onay akışı**
- teklif `Incelemede` durumuna alınabilmeli
- yönetici kullanıcı teklifi onaylayabilmeli veya reddedebilmeli
- karar notu ve işlem zamanı loglanmalı

**Senaryo 4 - Aktif versiyon gösterimi**
- ihale detay ekranında sadece bir versiyon aktif görünmeli
- aktif versiyon değiştiğinde ekran yeni veriyi göstermeli

**Senaryo 5 - Çıktı alma**
- onaylı teklif için PDF üretilebilmeli
- aynı teklif için Excel çıktısı alınabilmeli
- çıktı üzerinde doğru versiyon bilgisi görünmeli

**Senaryo 6 - Temel karşılaştırma**
- iki teklif versiyonu seçilip özet maliyet/teklif/kâr farkı görülebilmeli
- fark hesapları negatif ve pozitif durumlarda doğru çalışmalı

#### Rol Matrisi

| Rol | Taslak Oluştur | Revizyon Oluştur | İncelemeye Gönder | Onayla/Reddet | PDF/Excel Al | Karar Geçmişi Gör |
|-----|----------------|------------------|-------------------|---------------|--------------|-------------------|
| Admin | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Operasyon | ✅ | ✅ | ✅ | ❌ | ✅ | ✅ |
| Muhasebe | ❌ | ❌ | ❌ | ❌ | ✅ | ✅ |
| Yönetici | ❌ | ❌ | ❌ | ✅ | ✅ | ✅ |

#### Durum Geçiş Kuralları
- `Taslak -> Incelemede`
  - teklif sahibi veya yetkili kullanıcı tarafından yapılabilir
- `Incelemede -> Onaylandi`
  - yalnızca onay yetkisi olan kullanıcı yapabilir
- `Incelemede -> Reddedildi`
  - yalnızca onay yetkisi olan kullanıcı yapabilir
- `Reddedildi -> Taslak`
  - yeni revizyon açılarak dolaylı şekilde ilerlenmeli, aynı kayıt doğrudan geri alınmamalı
- `Onaylandi`
  - onaylanan versiyon salt-okunur kabul edilmeli

#### İş Kuralı Notları
- aynı anda birden fazla `Onaylandi` teklif versiyonu olabilir, ancak yalnızca bir tanesi `AktifVersiyon` olmalı
- `AktifVersiyon` değişikliği ayrıca loglanmalı
- `Onaylandi` durumundaki kayıtta düzenleme yerine yeni revizyon oluşturulmalı
- `Reddedildi` durumunda karar notu zorunlu olmalı

#### Yayınlama ve Geçiş Checklist'i

**Veritabanı Geçişi**
- yeni tablolar için migration alınmalı
- mevcut `IhaleProje` kayıtları için geriye dönük ilk versiyon üretim ihtiyacı değerlendirilmeli
- indexler üretim verisi üzerinde doğrulanmalı

**Uygulama Geçişi**
- eski ihale detay ekranı yeni versiyon yapısına uyarlanmalı
- aktif versiyon yoksa güvenli varsayılan akış tanımlanmalı
- yetkisiz kullanıcılar için onay butonları gizlenmeli

**Operasyonel Kontroller**
- üretime çıkmadan önce en az bir demo ihale üzerinde uçtan uca senaryo çalıştırılmalı
- PDF ve Excel çıktıları gerçek kullanıcı verisiyle kontrol edilmeli
- log kayıtları ve audit görünürlüğü doğrulanmalı

**Geri Dönüş Planı**
- migration geri alma komutu hazır olmalı
- yeni UI sekmesi feature flag benzeri kontrollü açılmalı veya pasif bırakılabilmeli
- ilk sürümde yalnızca çekirdek versiyonlama akışı aktif edilip sapma analizi daha sonra açılabilir

### 🔴 FAZ 9 Öncelikleri
1. ~~**İki faktörlü doğrulama (2FA/MFA)**~~ ✅ Tamamlandı
2. ~~**Audit log sistemi**~~ ✅ Tamamlandı
3. ~~**Background job sistemi**~~ ✅ Tamamlandı
4. ~~**Otomatik yedekleme**~~ ✅ Tamamlandı

### ✅ Son Tamamlanan
1. **Health Check & Monitoring (FAZ 9.3)** (Kayıt 134)
   - `SystemHealthService` yerel `SQLite` veritabanı ile uyumlu sağlık ölçümü üretecek şekilde güncellendi
   - Veritabanı sağlayıcısı, dosya yolu, dosya boyutu, disk ve bellek bilgileri gerçek verilerle raporlanıyor
   - `HealthController` ile `/api/health` ve `/api/health/details` endpoint'leri eklendi
   - Eski sahte `MobileController` health cevabı kaldırıldı ve merkezi endpoint yapısına geçildi
   - `SistemSaglik.razor` ekranında veritabanı sağlayıcısı ve yerel dosya yolu görünür hale getirildi

2. **Database Index Optimizasyonu (FAZ 9.3)** (Kayıt 133)
   - `ServisCalisma` için tarih, araç+tarih, şoför+tarih ve güzergah+tarih indexleri eklendi
   - `Fatura` için tarih, cari+tarih, durum+vade, tip+tarih ve şirket+tarih indexleri eklendi
   - `BankaKasaHareket` için hesap+tarih, cari+tarih, hareket tipi+tarih ve şirket+tarih indexleri eklendi
   - `AddDatabaseIndexesOptimization` migration'ı oluşturuldu

2. **Otomatik Yedekleme Sistemi (FAZ 9.5)** (Kayıt 132)
   - `BackupSettings` günlük / haftalık / saat bazlı planlamayı destekleyecek şekilde genişletildi
   - `AutoBackupService` yeni planlama kurallarını kullanacak şekilde güncellendi
   - `Yedekleme.razor` ekranına planlama tipi, saat/dakika, haftalık gün ve sonraki çalışma zamanı bilgisi eklendi
   - Otomatik yedekleme artık günlük/haftalık senaryolar için UI üzerinden yönetilebilir hale geldi

2. **Background Job Sistemi (Quartz) (FAZ 9.3)** (Kayıt 131)
   - `Quartz.Extensions.Hosting` paketi eklendi ve merkezi scheduler altyapısı kuruldu
   - `AutoBackupJob`, `CariHatirlatmaJob`, `BelgeUyariJob`, `DatabaseBackupJob` job sınıfları oluşturuldu
   - `Program.cs` içindeki dağınık `HostedService` kayıtları Quartz job tetiklerine taşındı
   - `AutoBackupService`, `CariHatirlatmaBackgroundService`, `BelgeUyariBackgroundService` tek seferlik çalıştırılabilir hale getirildi
   - `DatabaseBackupService` için retention temizliği dahil zamanlanmış yedek çalıştırma akışı eklendi

2. **İki Faktörlü Doğrulama (2FA/MFA) (FAZ 9.2)** (Kayıt 130)
   - `Kullanici` entity'sine `IkiFaktorAktif`, `IkiFaktorSecretKey`, `IkiFaktorEtkinlestirmeTarihi` alanları eklendi
   - `TwoFactorAuthenticatorHelper` ile `TOTP` tabanlı doğrulama altyapısı eklendi
   - `KullaniciService` içine 2FA kurulum, etkinleştirme, kapatma ve giriş tamamlama akışları eklendi
   - `Login.razor` şifre sonrası ikinci adım doğrulama akışıyla güncellendi
   - `Profil.razor` içine 2FA yönetim kartı eklendi
   - `AddTwoFactorAuthentication` migration'ı oluşturuldu

3. **Audit Log Sistemi (FAZ 9.2)** (Kayıt 129)
   - `AuditLog` entity'si oluşturuldu (işlem tipi, entity, kullanıcı, IP, eski/yeni değer JSON)
   - `IAuditLogService` interface ve `AuditLogService` implementasyonu eklendi
   - CRUD işlemleri için `LogCreateAsync`, `LogUpdateAsync`, `LogDeleteAsync` metodları
   - Login/Logout, Export/Import, özel işlem loglama desteği
   - `AuditLogYonetimi.razor` yönetim sayfası (filtreleme, sayfalama, dashboard, detay modal)
   - Log temizleme ve arşivleme özellikleri

3. **E-Fatura entegrasyonu (GİB) - Durum Takibi** (Kayıt 128)
   - `Fatura` entity'sine GİB gönderim durumu alanları eklendi
   - `EFaturaXmlService` içine GİB durum güncelleme akışı eklendi
   - XML oluşturulduğunda fatura otomatik `XML Hazırlandı` durumuna alınıyor
   - `EFaturaXml.razor` ekranına GİB durum filtresi, rozetleri ve durum aksiyonları eklendi

2. **Puantaj onay sistemi** (Kayıt 127)
   - `PersonelPuantaj` için onay durumu, onaylayan kullanıcı, onay tarihi ve onay notu alanları eklendi
   - `PuantajService` içine onaya gönder, onayla ve reddet akışları eklendi
   - `CalismaPuantaji.razor` içine onay durumu sütunu ve satır bazlı aksiyonlar eklendi
   - Onaylanan puantajların düzenlenmesi servis ve UI seviyesinde engellendi

2. **Maaşa Mahsup (Masraf/Ödeme)** (Kayıt 126)
   - `PersonelFinansService` içine açık avansların maaşa mahsup servisi eklendi
   - `MaasYonetimi.razor` içinde açık avans listesi ve "Maaşa Mahsup Et" aksiyonu eklendi
   - Mahsup yalnızca açık avans ve ödenmemiş maaşlar için uygulanıyor
   - Mahsup sonrası maaş kesintisi ve avans kalanları eş zamanlı güncelleniyor

2. **ASP.NET Core Identity Entegrasyonu** (Kayıt 125)
   - `KullaniciUserStore` ile mevcut `Kullanici` tablosu Identity user store olarak bağlandı
   - `KullaniciPasswordHasher` ile legacy SHA hash desteği ve otomatik rehash akışı eklendi
   - `Program.cs` içinde `IUserStore`, `IPasswordHasher` ve `IdentityCore` zinciri tamamlandı
   - `KullaniciService` içinde giriş, parola doğrulama ve parola değişim akışları `UserManager` tabanlı hale getirildi

2. **Kullanıcı Kayıt/Giriş** (Kayıt 124)
   - `Register.razor` self-servis kayıt ekranı eklendi
   - `Login.razor` içine kayıt ekranı linki eklendi
   - `KullaniciService` içinde varsayılan `Kullanici` rolü ile kayıt akışı eklendi
   - Lisans kullanıcı limiti ve benzersiz kullanıcı/e-posta kontrolleri kayıt akışına bağlandı

2. **Şifre Sıfırlama** (Kayıt 123)
   - `Login.razor` içine gerçek şifre sıfırlama paneli eklendi
   - Kullanıcı adı veya e-posta ile geçici şifre talebi yapılabiliyor
   - `KullaniciService` içinde geçici şifre üretimi ve e-posta gönderimi eklendi
   - Başarısız e-posta gönderiminde şifre geri alınarak veri tutarlılığı korundu

2. **Excel Export İyileştirme** (Kayıt 122)
   - `IExcelService` içine şoför performans ve araç karlılık export metodları eklendi
   - `ExcelService` içinde ortak servis tabanlı export üretimi tamamlandı
   - `SoforPerformansRapor.razor` generic export yerine servis export kullanacak şekilde güncellendi
   - `AracKarlilikRapor.razor` generic export yerine servis export kullanacak şekilde güncellendi
   - `downloadFile` çağrıları `fileName + base64 + mimeType` formatında standartlaştırıldı

2. **Dosya Yükleme (Ruhsat, Ehliyet, Sözleşme)** (Kayıt 121)
   - AracForm.razor'a "Araç Belgeleri" bölümü eklendi
   - Ruhsat, Trafik Sigortası, Kasko, Muayene, Sözleşme belge türleri
   - Belge ekleme, dosya yükleme, indirme ve silme işlemleri
   - Belge bitiş tarihi uyarıları (kırmızı: süresi dolmuş, sarı: 30 gün kala)
   - SoforForm.razor'a "Özlük Belgeleri" bölümü eklendi
   - Şoför belgeleri hızlı erişim kartları (Ehliyet, SRC, Psikoteknik)
   - Özlük evrak durumu progress bar ve istatistikleri
   - EBYS özlük dosyasına hızlı link

3. **Fatura PDF Oluşturma** (Kayıt 120)
   - FaturaDetay.razor'a PDF indirme ve e-posta gönderme butonları eklendi
   - IFaturaSablonService.FaturaPdfOlusturAsync entegrasyonu
   - IFaturaSablonService.FaturaEmailGonderAsync entegrasyonu
   - JavaScript downloadFile interop ile PDF indirme
   - Modal popup ile e-posta gönderimi
   - İşlem durumu göstergesi (yükleniyor, başarılı, hata)

4. **Rol Tabanlı Yetkilendirme** (Kayıt 118)
   - YetkiKontrol.razor sayfa seviyesi yetki bileşeni
   - Yetki, Yetkiler[], Rol, Roller[] parametreleri
   - KullaniciYonetimi.razor ve RolYonetimi.razor yetki koruması
   - Admin rolü otomatik yetkilendirme

5. **Bildirim Sistemi** (Kayıt 117)
   - IBildirimService + BildirimService implementasyonu
   - Vade yaklaşan fatura bildirimleri
   - Araç belge süresi uyarıları (Trafik Sigortası, Kasko, Muayene)
   - Şoför belge süresi uyarıları (Ehliyet, SRC, Psikoteknik, Sağlık Raporu)
   - BildirimPanel.razor (navbar dropdown)
   - Bildirimler.razor (yönetim sayfası + ayarlar)
   - Kullanıcı bazlı bildirim tercihleri
   - Dashboard istatistikleri

5. **Kullanıcı Profil Sayfası** (Kayıt 116)
   - `/ayarlar/profil` route'u ile Profil.razor sayfası
   - Profil bilgileri düzenleme (Ad Soyad, Email, Telefon)
   - Şifre değiştirme (mevcut/yeni şifre doğrulama)
   - Tema ve kompakt mod tercihleri
   - NavMenu'ye "Profilim" linki eklendi

6. **EBYS Örnek Veri ve Test Senaryoları** (Kayıt 114)
   - 10 farklı evrak kategorisi (renk kodlu, ikonlu)
   - 19 özlük evrak tanımı (7 kategori, zorunlu/opsiyonel ayrımı)
   - Şoför görevine özel evraklar (GecerliGorevler filtreleme)
   - 4 örnek gelen evrak (farklı durumlar, öncelikler, gizlilik seviyeleri)
   - 3 örnek giden evrak (farklı gönderim yöntemleri: KEP, Email, Elden)
   - Gerçekçi tarih ve belge numaraları

7. **EBYS Semantic Search (Akıllı Belge Arama)** (Kayıt 113)
   - ISemanticSearchService interface ve SemanticSearchService implementasyonu
   - Ollama embedding API entegrasyonu (nomic-embed-text modeli)
   - Cosine Similarity algoritması
   - EbysBelgeEmbedding entity (vektör JSON depolama)
   - BelgeArama.razor semantic search toggle UI
   - 4 belge kaynağı desteği (Personel, Araç, Gelen/Giden Evrak)

5. **EBYS AI Entegrasyonu (OCR, Belge Sınıflandırma)** (Kayıt 112)
   - IEbysAIService interface ve EbysAIService implementasyonu
   - Tesseract OCR entegrasyonu (offline çalışabilir)
   - Ollama AI ile belge sınıflandırma
   - Belge özeti oluşturma
   - Anahtar kelime çıkarma
   - EbysAIPanel.razor bileşeni
   - EvrakDetay.razor'a AI panel entegrasyonu
   - appsettings.json OCR konfigürasyonu

6. **EBYS Gelişmiş Belge Arama Sistemi** (Kayıt 111)
   - 4 kaynakta arama (Personel Özlük, Araç Evrak, Gelen/Giden Evrak)
   - EbysGelismisAramaFiltre, EbysAramaSonuc DTO'ları
   - EbysBelgeAramaService implementasyonu
   - BelgeArama.razor UI sayfası
   - Paralel arama, alaka skoru hesaplama
   - Arama geçmişi ve kaydedilmiş aramalar desteği

7. **EBYS Belge Versiyon Kontrolü** (Kayıt 110)
   - 3 versiyon entity'si (EBYS, Araç, Personel)
   - BelgeVersiyonService implementasyonu
   - EvrakDetay.razor versiyon UI
   - Dosya güncelleme ve geri yükleme

4. **osTicket Benzeri Destek Talebi Sistemi** (Kayıt 099)
   - Kullanıcı talep girişi sayfası (TalepGiris.razor)
   - Kullanıcı taleplerim listesi (Taleplerim.razor)
   - Talep takip/detay sayfası (TalepTakip.razor)
   - Yetkili Kanban yönetim paneli (TalepYonetim.razor)
   - Durum akışı: Taslak → Gönderildi → İşlemde → Bitti → Onaylandı

### 🟢 FAZ 9'dan Başlanabilecek Özellikler
1. ~~**İki faktörlü doğrulama (2FA)**~~ ✅ Tamamlandı
2. ~~**Audit log sistemi**~~ ✅ Tamamlandı
3. ~~**Background jobs (Hangfire/Quartz)**~~ ✅ Tamamlandı
4. ~~**Otomatik yedekleme**~~ ✅ Tamamlandı

---

## 📝 Notlar

- Öncelik: 🔴🔴 Acil | 🔴 Yüksek | 🟡 Orta | 🟢 Düşük | 🤖 AI
- Durum: ❌ Bekliyor | 🔄 Devam Ediyor | ✅ Tamamlandı | 🆕 Yeni Eklendi
- Güncel odak: **FAZ 8.5 - Teklif Operasyonları & Gerçekleşen Takibi**
- EBYS için Ollama/LLaMA kullanılarak internetsiz çalışabilirlik sağlandı
- Personel/Maaş/Bordro modülleri SGK mevzuatına uygun olmalı
- Local AI (Ollama) ile raporlama özellikleri offline çalışabilecek
- İhale Hazırlık modülü Ollama AI ile entegre (masraf tahmini, maaş tahmini, proje analizi)
- **Destek Talebi modülü osTicket benzeri kullanıcı ve yetkili arayüzleri ile tamamlandı**
- **EBYS Gelen/Giden Evrak modülü tamamlandı - atama, takip, dosya ekleme özellikleriyle**
- **EBYS Versiyon kontrolü tamamlandı - 3 belge tipi, geri yükleme, versiyon karşılaştırma**
- **EBYS Belge arama tamamlandı - 4 kaynakta paralel arama, alaka skoru, filtreler**
- **EBYS AI entegrasyonu tamamlandı - OCR (Tesseract), belge sınıflandırma, özet, anahtar kelime**
- **EBYS Semantic Search tamamlandı - Ollama embedding API, Cosine Similarity, vektör tabanlı akıllı arama**
- **EBYS Örnek Veri tamamlandı - 10 kategori, 19 özlük evrak tanımı, 7 örnek evrak (gelen/giden)**
- **Bildirim Sistemi tamamlandı - Vade yaklaşan fatura, ehliyet/muayene/sigorta bitiş uyarıları, uygulama içi bildirimler**

---

*Son güncelleme: Nisan 2026*
