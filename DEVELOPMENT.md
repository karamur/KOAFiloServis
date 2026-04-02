# DEVELOPMENT

## Proje Adı
`Koa Filo Servis`

## Amaç
Filo yönetimi, muhasebe, bütçe, e-fatura, cari yönetimi, operasyon puantajı ve yardımcı CRM süreçlerini tek uygulamada toplamak.

---

## Kullanım Amacı
Bu dosya artık sadece genel özet değil, aynı zamanda proje için merkezi geliştirme kayıt dosyası olarak kullanılmalıdır.

Bu dosyada aşağıdakiler birlikte tutulur:
- kullanıcıdan gelen her yeni talep
- talep için yapılan işlemler
- tamamlanan işler
- bekleyen işler
- teknik borçlar
- sonraki adımlar

Her yeni geliştirme sonrasında bu dosya güncellenmelidir.

---

## Aktif Geliştirme Kayıt Yapısı

### 1. Gelen İstekler
Buraya kullanıcıdan gelen talepler tarih sırasıyla eklenir.

### 2. Yapılanlar
Tamamlanan geliştirmeler kısa ve net şekilde yazılır.

### 3. Yapılacaklar
Henüz tamamlanmamış ama planlanan işler burada tutulur.

### 4. Blokajlar / Riskler
Sorun çıkaran, tekrar kontrol edilmesi gereken veya teknik risk barındıran konular burada tutulur.

---

## İstek Kayıtları

### Kayıt 001 - E-Fatura PDF eşleştirme sorunu
**Talep:** PDF dosyasına sürekli aynı faturanın PDF'inin eklenmesi sorununun çözülmesi.

**Yapılanlar:**
- XML + PDF eşleştirme mantığı düzeltildi.
- import sırasında yanlış PDF eşleşmesi engellendi.
- PDF dosya adı üretimi benzersiz hale getirildi.

**Durum:** Tamamlandı

### Kayıt 002 - Taşıma hizmetlerinden güzergah üretimi
**Talep:** Stok türü eşleştirmede `Hizmet > Taşıma` seçildiğinde güzergah listesi hazırlanması ve kullanıcı onayı sonrası firma bazlı güzergah açılması.

**Yapılanlar:**
- güzergah için yeni alanlar eklendi
- sefer tipi ve personel sayısı alanları genişletildi
- önizleme ve oluşturma akışı başlatıldı

**Durum:** Devam ediyor

### Kayıt 010 - Repo temizliği ve uploads takibinin kapatılması
**Talep:** Çalışma zamanında oluşan yükleme dosyalarının tekrar git takibine girmesinin engellenmesi.

**Yapılanlar:**
- kök `.gitignore` dosyasına `CRMFiloServis.Web/wwwroot/uploads/**` kuralı eklendi
- yükleme çıktıları için repo temizliği maddesi aktif takip listesine işlendi
- git takibine girmiş upload dosyaları index'ten çıkarıldı
- klasörü korumak için `CRMFiloServis.Web/wwwroot/uploads/.gitkeep` eklendi

**Yapılacaklar:**
- diğer çalışma zamanı klasörleri de taranmalı

**Etkilenen Dosyalar:**
- `.gitignore`
- `DEVELOPMENT.md`

**Durum:** Tamamlandı

### Kayıt 009 - Proje geneli marka adı tutarlılığı
**Talep:** Görünür ekranlar ve dokümantasyonda eski marka adlarının taranıp güncellenmesi.

**Yapılanlar:**
- login ekranı `Koa Filo Servis` olarak güncellendi
- sol menü marka adı güncellendi
- `README.md` başlığı güncellendi
- `KURULUM.md` başlığı güncellendi

**Yapılacaklar:**
- proje genelinde görünen eski marka adları taranmalı
- masaüstü uygulama ve diğer görünür başlıklar kontrol edilmeli
- uygun yerlerde repo adı ile görünen ürün adı ayrıştırılmalı

**Etkilenen Dosyalar:**
- `CRMFiloServis.Web/Components/Pages/Login.razor`
- `CRMFiloServis.Web/Components/Layout/NavMenu.razor`
- `README.md`
- `KURULUM.md`

**Durum:** Devam ediyor

### Kayıt 003 - Servis puantaj sistemi
**Talep:** Güzergah / araç / şoför eşleştirme ve günlük puantaj tablosu oluşturulması.

**Yapılanlar:**
- `FiloGuzergahEslestirme` ve `FiloGunlukPuantaj` yapıları devreye alındı
- puantaj ekranı için temel sayfa oluşturuldu

**Durum:** Devam ediyor

### Kayıt 004 - Mahsup işlemleri
**Talep:** Kasa / banka / kredi kartı ve cari hesaplar arası mahsup işlemleri.

**Yapılanlar:**
- hesaplar arası transfer mantığı geliştirildi
- cari mahsup yapısı eklendi
- mahsup ekranı oluşturuldu

**Durum:** Kısmen tamamlandı

### Kayıt 005 - Muhasebe eşleştirme kodları
**Talep:** Girilen banka hareketlerinde kullanıcı tarafından muhasebe eşleştirme kodlarının girilebilmesi.

**Yapılanlar:**
- banka/kasa hareketlerine muhasebe alanları eklendi
- hesap bazlı varsayılan muhasebe kodu alanları eklendi
- kost merkezi ve proje tanımları için altyapı eklendi

**Durum:** Altyapı tamamlandı, yönetim ekranları eksik

### Kayıt 006 - Bütçe ödeme kesintileri
**Talep:** Bütçe analiz ödeme ekranında masraf/ceza kesintileri ve ödeme şekline göre kayıt yapılması.

**Yapılanlar:**
- kesinti alanları eklendi
- net ödeme hesabı eklendi
- ödeme tipi alanları genişletildi

**Durum:** Kısmen tamamlandı

### Kayıt 007 - Login ekranı sorunları
**Talep:** Şifre göster, beni hatırla ve giriş yap butonunun çalışmaması sorunlarının giderilmesi.

**Yapılanlar:**
- login sayfası birkaç kez düzenlendi
- route çakışması giderildi
- input binding ve giriş akışı üzerinde düzeltmeler yapıldı
- sayfa yeniden yapılandırıldı

**Durum:** Tekrar doğrulanmalı

### Kayıt 008 - Marka adı güncellemesi
**Talep:** Proje görünen adının `Koa Filo Servis` olarak düzenlenmesi.

**Yapılanlar:**
- login ekranı başlığı güncellendi
- menü marka adı güncellendi
- bazı görünür başlıklarda düzenleme yapıldı

**Durum:** Kısmen tamamlandı

---

## Açık İş Özeti

| No | Konu | Durum | Öncelik | Not |
|---|---|---|---|---|
| 001 | Login akışı | Devam ediyor | Yüksek | Giriş, auth state ve yönlendirme tekrar doğrulanmalı |
| 002 | Taşıma → Güzergah akışı | Kısmen tamamlandı | Yüksek | Oluşturma ve doğrulama akışı tamamlanmalı |
| 003 | Servis puantaj sistemi | Devam ediyor | Yüksek | Ekran var, iş akışı eksik |
| 004 | Mahsup işlemleri | Kısmen tamamlandı | Yüksek | Fiş üretimi ve bütçe entegrasyonu eksik |
| 005 | Muhasebe eşleştirme ekranları | Bekliyor | Orta | Alanlar var, yönetim ekranları eksik |
| 006 | Bütçe + cari mahsup | Devam ediyor | Yüksek | Uçtan uca kayıt akışı tamamlanmalı |
| 007 | Marka adı tutarlılığı | Devam ediyor | Orta | Proje genelinde eski isimler taranmalı |
| 008 | Repo temizliği / uploads | Tamamlandı | Orta | Ignore + cached dosya temizliği yapıldı |
| 009 | Dokümantasyon marka güncellemesi | Devam ediyor | Düşük | README ve kurulum başlıkları güncellendi |
| 010 | Çalışma zamanı dosya disiplini | Kısmen tamamlandı | Düşük | Upload klasörü ignore edildi |

---

## Çözüm Yapısı

### Projeler
- `CRMFiloServis.Web`
  - Blazor Server ana uygulama
  - sayfalar, servisler, veri erişimi, migrationlar
- `CRMFiloServis.Shared`
  - ortak entity modelleri
  - enumlar, yardımcı sınıflar
- `CRMFiloServis.LisansDesktop`
  - lisans yönetimi / masaüstü yardımcı araç

### Teknoloji
- `.NET 10`
- `Blazor Server`
- `Entity Framework Core`
- `SQLite` / `PostgreSQL` desteği
- `Bootstrap` + `Bootstrap Icons`

---

## Şu Ana Kadar Yapılanlar

## 1. Temel Modüller
Aşağıdaki ana modüller projede mevcut durumda:
- Kullanıcı / rol / yetki yönetimi
- Cari yönetimi
- Filo servis yönetimi
- Araç, şoför, güzergah, servis çalışma kayıtları
- Bütçe ve ödeme takibi
- Muhasebe hesap planı / fiş yapısı
- Banka / kasa hareketleri
- E-fatura / XML import altyapısı
- Stok / envanter mantığı
- CRM yardımcı modülleri
  - bildirim
  - mesaj
  - WhatsApp
  - randevu / hatırlatıcı

## 2. Son Dönemde Tamamlanan İşler

### Login ekranı
- giriş ekranı düzenlendi
- şifre göster/gizle davranışı iyileştirildi
- “beni hatırla” yaklaşımı üzerinde düzenleme yapıldı
- login route tekrarları temizlendi
- login sayfası birkaç kez sadeleştirilip yeniden düzenlendi
- marka adı giriş ekranında `Koa Filo Servis` olarak güncellendi

### Marka / Görünüm
- giriş ekranı başlığı `Koa Filo Servis` yapıldı
- sol menü üst marka adı güncellendi
- giriş ekranı daha kurumsal görünecek şekilde elden geçirildi

### E-Fatura / XML + PDF import
- XML ile birlikte PDF yükleme desteği eklendi
- tek PDF’in tüm faturalara bağlanması sorunu düzeltildi
- XML-PDF eşleştirme mantığı iyileştirildi
- dosya adı benzersizleştirildi

### Stok Türü Eşleştirme / Güzergah hazırlığı
- `Hizmet` + `Taşıma` tipi için güzergah üretim hazırlığı yapıldı
- fatura kalemlerinden güzergah önizleme mantığı eklendi
- güzergah için sefer tipi ve personel sayısı alanları genişletildi

### Filo operasyon / puantaj
- `FiloGuzergahEslestirme` ve `FiloGunlukPuantaj` yapıları kullanılmaya başlandı
- servis puantaj ekranı için temel sayfa oluşturuldu
- güzergah / araç / şoför eşleştirme akışı geliştirildi

### Mahsup işlemleri
- hesaplar arası transfer mantığı eklendi
- cari mahsup mantığı eklendi
- mahsup hareketlerini gruplamak için alanlar eklendi:
  - `MahsupGrupId`
  - `MahsupHareketId`
- `Mahsup İşlemleri` sayfası oluşturuldu
- kasa / banka / kredi kartı hesapları arası transfer altyapısı geliştirildi

### Muhasebe eşleştirme alanları
- banka / kasa hareketlerine kullanıcı tarafından girilebilecek muhasebe alanları eklendi:
  - `MuhasebeHesapKodu`
  - `MuhasebeAltHesapKodu`
  - `KostMerkeziKodu`
  - `ProjeKodu`
  - `MuhasebeAciklama`
- hesap bazında varsayılan muhasebe kodu alanları eklendi
- `KostMerkezi` ve `MuhasebeProje` tanımları eklendi

### Bütçe modülü
- ödeme yaparken kesinti alanları eklendi:
  - masraf kesintisi
  - ceza kesintisi
  - diğer kesinti
- net ödeme hesabı yapıldı
- ödeme tipi seçenekleri genişletildi:
  - kasa
  - banka
  - kredi kartı
  - mahsup
  - cari mahsup hazırlığı

### Migrationlar
Yakın dönemde eklenen migrationlar:
- `GuzergahGenisletme`
- `BudgetOdemeKesintiler`
- `MahsupMuhasebeKodlari`

---

## Aktif Olarak Dikkat Edilmesi Gerekenler

## 1. Login akışı
Login ekranı üzerinde birkaç iterasyon yapıldı. Kod tarafında düzeltmeler uygulanmış olsa da bu alanın tekrar uçtan uca test edilmesi gerekiyor.

### Kontrol edilmesi gerekenler
- kullanıcı adı / şifre ile giriş
- başarılı girişten sonra ana sayfaya yönlenme
- auth state’in sayfaya yansıması
- “beni hatırla” davranışı
- şifre göster/gizle butonu
- farklı tarayıcılarda davranış
- authentication state'in circuit bazlı kalıcılığı
- giriş sonrası yönlendirmede kullanıcı oturumunun korunması

## 2. Marka adı tutarlılığı
`Koa Filo Servis` adı login ve menüde güncellendi.
Ancak proje genelinde eski isimlerin (`CRM Filo Servis`, `CRMFiloServis`) geçtiği başka yerler olabilir.

### Tarama yapılmalı
- `PageTitle`
- navbar / footer
- GitHub link açıklamaları
- versiyon / yayın metinleri
- lisans / masaüstü uygulama başlıkları
- deploy scriptleri / paket isimleri

## 3. Repo temizliği
Geçmişte `wwwroot/uploads/...` altındaki PDF dosyaları repoya eklenmiş durumda.
Bu dosyaların sürüm kontrolünde tutulması doğru değil.

### Yapılması gereken
- `uploads` için `.gitignore` kuralı eklemek
- repodaki gereksiz yüklenen dosyaları temizlemek
- gerekiyorsa geçmişi düzenlemek ya da en azından yeni commitlerde takibi bırakmak

## 4. Modül tamamlama riski
Bazı ekranlar oluşturuldu ancak tam iş akışı kapanmadı.

Özellikle:
- servis puantaj
- bütçe + cari mahsup entegrasyonu
- muhasebe eşleştirme yönetim ekranları
- login stabilizasyonu

---

## Yapılması Gerekenler

## A. Öncelikli İşler

### 1. Login stabilizasyonu
- login akışı tam test edilmeli
- auth state provider ve yönlendirme davranışı netleştirilmeli
- gerekiyorsa login mekanizması tek bir yaklaşıma indirgenmeli
- local storage / remember me akışı doğrulanmalı
- giriş sonrası yetkili sayfalara erişim zinciri doğrulanmalı

### 2. Mahsup ekranının tamamlanması
- transfer kayıtlarında muhasebe alanlarının gerçekten veri tabanına yazıldığı uçtan uca test edilmeli
- cari mahsup ekranı bütçe ödemesi ile tam entegre edilmeli
- mahsup iptalinin muhasebe ve bakiye etkileri doğrulanmalı
- mahsup için muhasebe fişi üretim akışı bağlanmalı

### 3. Bütçe + cari mahsup entegrasyonu
- bütçe ödemesinde `CariMahsup` seçildiğinde gerçek kayıt akışı tamamlanmalı
- ilgili cari ve banka/kasa hareketleri doğru kapanmalı
- kesinti + cari mahsup kombinasyonu test edilmeli
- kredi kartı ödeme akışı ayrı doğrulanmalı

## B. Filo Tarafı

### 4. Güzergah üretim akışını tamamla
- taşıma kalemlerinden oluşturulan güzergahların firma bazında kaydı doğrulanmalı
- güzergah adı parse kuralları iyileştirilmeli
- sabah / akşam / saatlik tespitleri netleştirilmeli

### 5. Servis puantaj ekranını tamamla
- `ServisPuantaj.razor` ekranı işlevsel olarak bitirilmeli
- aylık toplu puantaj üretimi gerçek senaryolarla test edilmeli
- araç / şoför / güzergah ataması kalıcı iş akışına bağlanmalı
- excel export / raporlama tamamlanmalı

### 6. Filo eşleştirme ekranları
- araç-şoför öncelikli eşleştirme
- güzergah-araç eşleştirme
- varsayılan atamalar
- firma bazlı operasyon planlama

## C. Muhasebe Tarafı

### 7. Muhasebe eşleştirme tanım ekranları
Henüz veri alanları eklendi ancak yönetim ekranları eksik olabilir.

Yapılması gereken:
- muhasebe hesap kodu seçim / giriş ekranı
- kost merkezi tanım ekranı
- proje kodu tanım ekranı
- banka hesaplarına varsayılan muhasebe kodu tanımlama ekranı
- otomatik eşleştirme kuralları ekranı
- banka hareket giriş ekranında bu alanların görünürlüğü doğrulanmalı

### 8. Mahsup fişi üretimi
- banka / kasa transferlerinden otomatik mahsup fişi üretimi
- cari mahsuptan otomatik muhasebe fişi üretimi
- hatalı veya eksik fişlerin raporlanması

## D. E-Fatura / Stok Tarafı

### 9. XML/PDF import testleri
- toplu yükleme testleri
- eksik PDF senaryosu
- aynı isimli / benzer isimli dosya senaryoları
- hata loglarının iyileştirilmesi

### 10. Stok türü eşleştirme sonrası otomasyon
- taşıma hizmeti seçilen kayıtların doğrudan güzergah hazırlığına aktarılması
- kullanıcı onay akışının netleştirilmesi

## E. Kalite / Teknik Borç

### 11. Test altyapısı
- servis katmanı için unit testler
- kritik senaryolar için integration testler
- login, bütçe, mahsup, e-fatura import için test seti

### 12. Dokümantasyon
- kullanıcı kılavuzu
- rol / yetki matrisi
- modül bağımlılıkları
- veri akış diyagramları
- deploy rehberi

### 13. Kod temizliği
- login sayfası son hali sadeleştirilmeli
- tekrar eden route / state / event hataları gözden geçirilmeli
- migration helper yapıları gözden geçirilmeli
- servis isimlendirmelerinde tutarlılık sağlanmalı

---

## Önerilen Kısa Yol Haritası

## Faz 1
- login stabilizasyonu
- mahsup ekranı doğrulama
- bütçe + cari mahsup entegrasyonu
- uploads klasörü git temizliği

## Faz 2
- servis puantaj sistemini işlevsel bitirme
- güzergah üretimi ve eşleştirme akışı
- muhasebe eşleştirme yönetim ekranları

## Faz 3
- otomatik muhasebe fişi üretimi
- raporlama / export
- test ve teknik borç azaltma

---

## Son Commitlerden Özet
- login ekranı yeniden yazıldı
- login route düzeltildi
- marka adı `Koa Filo Servis` olarak güncellendi
- mahsup işlemleri eklendi
- muhasebe eşleştirme kodları eklendi
- bütçe kesinti alanları eklendi
- XML + PDF import geliştirildi
- servis puantaj / güzergah tarafında temel altyapı geliştirildi

---

## Güncel Kısa Durum Özeti

### Tamamlanan Ana Başlıklar
- e-fatura XML + PDF import düzeltmeleri
- mahsup altyapısı
- bütçe ödeme kesinti altyapısı
- marka adı güncelleme başlangıcı

### Kısmen Tamamlananlar
- login ekranı
- taşıma hizmetinden güzergah üretimi
- servis puantaj sistemi
- cari mahsup entegrasyonu
- muhasebe eşleştirme yönetimi

### Kritik Açık Başlıklar
- login akışının tam stabil hale getirilmesi
- bütçe + cari mahsup akışının tamamlanması
- muhasebe yönetim ekranlarının tamamlanması
- uploads klasörünün repodan ayrılması
- proje genelinde marka adı taraması

---

## Yeni Kayıt Şablonu

Yeni bir kullanıcı talebi geldiğinde aşağıdaki format kullanılmalıdır:

### Kayıt 00X - Talep başlığı
**Talep:**
- kullanıcı isteğinin kısa özeti

**Yapılanlar:**
- yapılan adım 1
- yapılan adım 2

**Yapılacaklar:**
- eksik adım 1
- eksik adım 2

**Etkilenen Dosyalar:**
- `dosya/yolu`
- `dosya/yolu`

**Durum:** Bekliyor / Devam ediyor / Kısmen tamamlandı / Tamamlandı

**Not:**
- varsa risk, karar veya bağımlılık

---

## Güncelleme Kuralları

- Her yeni kullanıcı isteğinde önce `İstek Kayıtları` güncellenmeli.
- İş tamamlandıysa `Açık İş Özeti` tablosundaki durum da güncellenmeli.
- Teknik olarak önemli değişiklikler `Şu Ana Kadar Yapılanlar` bölümüne eklenmeli.
- Büyük eksikler `Yapılması Gerekenler` altında ilgili başlığa taşınmalı.
- İş bittiğinde mümkünse ilgili commit mesajı ayrıca not edilmeli.

---

## Not
Bu dosya canlı durum özeti olarak kullanılmalı.
Yeni modül veya önemli değişikliklerden sonra güncellenmesi önerilir.

Her yeni kullanıcı talebinde aşağıdaki 3 başlık mutlaka güncellenmelidir:
- `İstek Kayıtları`
- `Yapılanlar`
- `Yapılması Gerekenler`
