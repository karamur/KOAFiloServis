# ?? Koa Filo Servis - Proje Faz Planı

## ?? Mevcut Durum Analizi

### Son Güncellemeler
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

## ?? FAZ 3 - İleri Seviye Özellikler

### 3.1 Entegrasyonlar
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| E-Fatura entegrasyonu (GİB) | ?? Yüksek | 5 gün | ? |
| SMS entegrasyonu | ?? Orta | 2 gün | ? |
| Harita entegrasyonu (güzergah gösterimi) | ?? Düşük | 3 gün | ? |
| Araç takip sistemi entegrasyonu | ?? Düşük | 4 gün | ? |

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
| Şoför mobil uygulaması (MAUI Blazor) | ?? Orta | 10 gün | ? |
| Sefer başlat/bitir | ?? Orta | 2 gün | ? |
| Arıza bildirimi | ?? Orta | 1 gün | ? |
| Masraf girişi (fotoğraflı) | ?? Orta | 2 gün | ? |

---

## ?? FAZ 4 - Kurumsal Özellikler

### 4.1 Çoklu Şirket Desteği
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Multi-tenant mimari | ?? Düşük | 5 gün | ? |
| Şirket bazlı veri izolasyonu | ?? Düşük | 3 gün | ? |
| Şirketler arası transfer | ?? Düşük | 2 gün | ? |

### 4.2 API & Entegrasyon
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| REST API oluşturma | ?? Orta | 4 gün | ? |
| API dokümantasyonu (Swagger) | ?? Orta | 1 gün | ? |
| Webhook desteği | ?? Düşük | 2 gün | ? |

### 4.3 Performans & Ölçekleme
| Özellik | Öncelik | Süre | Durum |
|---------|---------|------|-------|
| Redis cache entegrasyonu | 🟢 Düşük | 2 gün | ❌ |
| Sayfalama (pagination) | 🔴 Yüksek | 2 gün | ✅ Tamamlandı |
| Lazy loading optimizasyonu | 🟡 Orta | 1 gün | ❌ |

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
| Örnek personel verisi oluşturma | 🟡 Orta | 1 gün | ❌ |
| Örnek bordro verisi oluşturma | 🟡 Orta | 1 gün | ❌ |
| Örnek fatura verisi oluşturma | 🟡 Orta | 1 gün | ❌ |
| Test senaryoları | 🟢 Düşük | 2 gün | ❌ |

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

FAZ 8 (2-3 Hafta) - ✅ TAMAMLANDI
├── Hafta 1: İhale Proje CRUD + Maliyet Hesaplama
├── Hafta 2: AI Tahmin + Enflasyonlu Projeksiyon
└── Hafta 3: Kâr/Zarar Rapor + Proje Kopyalama
```

---

## 🎯 Hemen Başlanabilecek Öncelikli İşler

### 🔴 Güncel Açık Öncelikler
1. **Kullanıcı Yönetimi & Yetkilendirme (ASP.NET Core Identity)**

### 🟡 Devam Eden Başlıklar
1. **EBYS Temel Altyapı** ✅ TAMAMLANDI
   - belge kategorileri ✅
   - dosya yükleme/indirme ✅
   - metadata yönetimi ✅
   - versiyon kontrolü ✅
   - belge arama ✅
2. **EBYS AI Entegrasyonu** ✅ TAMAMLANDI
   - OCR (Tesseract) ✅
   - Belge sınıflandırma ✅
   - Belge özeti ✅
   - Anahtar kelime çıkarma ✅
   - Semantic search ✅
3. **EBYS Örnek Veri ve Test Senaryoları** ✅ TAMAMLANDI
   - Evrak kategorileri (10 adet) ✅
   - Özlük evrak tanımları (19 adet) ✅
   - Örnek gelen/giden evraklar (7 adet) ✅

### ✅ Son Tamamlanan
1. **Puantaj onay sistemi** (Kayıt 127)
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

### 🟢 Sonraki Mantıklı Adımlar
1. Kullanıcı Yönetimi & Yetkilendirme (ASP.NET Core Identity)

---

## 📝 Notlar

- Öncelik: 🔴🔴 Acil | 🔴 Yüksek | 🟡 Orta | 🟢 Düşük | 🤖 AI
- Durum: ❌ Bekliyor | 🔄 Devam Ediyor | ✅ Tamamlandı | 🆕 Yeni Eklendi
- Güncel odak: **Kullanıcı Yönetimi & Yetkilendirme**
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

*Son güncelleme: Haziran 2025*
