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

### Kayıt 023 - Dashboard Grafikleri (Chart.js)
**Talep:** Dashboard'a görsel grafikler eklenmesi - Aylık gelir/gider, masraf dağılımı, bütçe takibi.

**Yapılanlar:**
- `ChartDataModels.cs`: Yeni grafik veri modelleri oluşturuldu (AylikGelirGiderVeri, CariTipDagilimi, MasrafKategoriDagilimi, AylikButceVeri)
- `IDashboardGrafikService.cs`: 3 yeni metod eklendi (GetMasrafKategoriDagilimiAsync, GetCariTipDagilimiAsync, GetAylikButceAsync)
- `DashboardGrafikService.cs`: Yeni metodların implementasyonu eklendi
- `Home.razor`: Chart.js entegrasyonu yapıldı (IJSRuntime ile JS interop)
- `Home.razor`: 4 grafik kartı eklendi (Aylık Gelir/Gider bar chart, Masraf Dağılımı doughnut chart, Bütçe Takibi line chart, Cari Dağılımı tablosu)
- `Home.razor`: OnAfterRenderAsync ve RenderChartsAsync metodları eklendi
- Mevcut `dashboard-charts.js` fonksiyonları kullanıldı (createBarChart, createLineChart, createDoughnutChart)

**Etkilenen Dosyalar:**
- `CRMFiloServis.Web/Models/ChartDataModels.cs` (yeni)
- `CRMFiloServis.Web/Services/Interfaces/IDashboardGrafikService.cs`
- `CRMFiloServis.Web/Services/DashboardGrafikService.cs`
- `CRMFiloServis.Web/Components/Pages/Home.razor`

**Durum:** Tamamlandı

### Kayıt 022 - Sayfalama (Pagination) altyapısı
**Talep:** Liste sayfalarında performans iyileştirmesi için server-side sayfalama altyapısının eklenmesi.

**Yapılanlar:**
- `Pagination.razor`: Yeniden kullanılabilir sayfalama komponenti oluşturuldu
- `PagedResult.cs`: Generic `PagedResult<T>` ve `PagingParameters` modelleri oluşturuldu
- `ICariService.cs`: `GetPagedAsync` metodu ve `CariFilterParams` sınıfı eklendi
- `CariService.cs`: Sayfalama implementasyonu ve bakiye hesaplama eklendi
- `CariList.razor`: Server-side sayfalama entegrasyonu ve debounce arama eklendi
- `IFaturaService.cs`: `GetPagedAsync` metodu ve `FaturaFilterParams` sınıfı eklendi
- `FaturaService.cs`: Sayfalama implementasyonu eklendi
- `FaturaList.razor`: Sayfalama destekli yeniden oluşturuldu (tip, durum, tarih filtreleri)
- `IBankaKasaHareketService.cs`: `GetPagedAsync` metodu ve `BankaHareketFilterParams` sınıfı eklendi
- `BankaKasaHareketService.cs`: Sayfalama implementasyonu eklendi
- `BankaHareketList.razor`: Sayfalama destekli güncellendi (hesap, tip, tarih filtreleri, toplam özeti)

**Etkilenen Dosyalar:**
- `CRMFiloServis.Web/Components/Shared/Pagination.razor`
- `CRMFiloServis.Web/Models/PagedResult.cs`
- `CRMFiloServis.Web/Services/Interfaces/ICariService.cs`
- `CRMFiloServis.Web/Services/CariService.cs`
- `CRMFiloServis.Web/Components/Pages/Cariler/CariList.razor`
- `CRMFiloServis.Web/Services/Interfaces/IFaturaService.cs`
- `CRMFiloServis.Web/Services/FaturaService.cs`
- `CRMFiloServis.Web/Components/Pages/Faturalar/FaturaList.razor`
- `CRMFiloServis.Web/Services/Interfaces/IBankaKasaHareketService.cs`
- `CRMFiloServis.Web/Services/BankaKasaHareketService.cs`
- `CRMFiloServis.Web/Components/Pages/BankaHareketleri/BankaHareketList.razor`

**Durum:** Tamamlandı

### Kayıt 021 - Çalışma zamanı klasör disiplininin genişletilmesi
**Talep:** Sadece `uploads` değil, diğer çalışma zamanı klasörlerinin de git takibinden ayrılması.

**Yapılanlar:**
- `.gitignore`: `CRMFiloServis.Web/Backups/**` ignore kuralı eklendi
- `.gitignore`: `deploy/Backups/**`, `deploy/Logs/**`, `deploy/Uploads/**` ignore kapsamına alındı
- runtime klasörlerini repo içinde korumak için `.gitkeep` istisnaları tanımlandı
- `010` kaydı ile açık iş özeti tutarlı hale getirildi

**Etkilenen Dosyalar:**
- `.gitignore`
- `DEVELOPMENT.md`
- `CRMFiloServis.Web/Backups/.gitkeep`
- `CRMFiloServis.Web/wwwroot/uploads/.gitkeep`
- `deploy/Backups/.gitkeep`
- `deploy/Logs/.gitkeep`
- `deploy/Uploads/.gitkeep`

**Durum:** Tamamlandı

### Kayıt 020 - Servis katmanında okuma ve soft delete tutarlılığı
**Talep:** Servis katmanındaki güvenli refaktör işlerinin tamamlanması.

**Yapılanlar:**
- `GuzergahService.cs`: soft delete işleminde `UpdatedAt` güncellemesi eklendi
- `GuzergahService.cs`: kod üretiminde okuma sorguları `AsNoTracking()` ile güvenli hale getirildi
- `MasrafKalemiService.cs`: soft delete işleminde `UpdatedAt` güncellemesi eklendi
- `PiyasaKaynakService.cs`: `DateTime.Now` yerine `DateTime.UtcNow` standardı uygulandı
- `PiyasaKaynakService.cs`: kod kontrolü ve seed sayım sorgularında `AsNoTracking()` eklendi
- `SoforService.cs`: aktif kayıt sayımı `AsNoTracking()` ile güncellendi
- `SoforService.cs`: soft delete işleminde `UpdatedAt` güncellemesi eklendi

**Etkilenen Dosyalar:**
- `CRMFiloServis.Web/Services/GuzergahService.cs`
- `CRMFiloServis.Web/Services/MasrafKalemiService.cs`
- `CRMFiloServis.Web/Services/PiyasaKaynakService.cs`
- `CRMFiloServis.Web/Services/SoforService.cs`

**Durum:** Tamamlandı

### Kayıt 019 - Marka adı görünür metin taraması
**Talep:** Proje genelinde görünür marka adlarının taranması ve `Koa Filo Servis` ile tutarlı hale getirilmesi.

**Yapılanlar:**
- `Login.razor`: footer içindeki GitHub bağlantı etiketi `Koa Filo Servis` olarak güncellendi
- `README.md`: ilgili projeler tablosundaki eski marka adı güncellendi
- `ROADMAP.md`: doküman başlığındaki eski marka adı güncellendi

**Etkilenen Dosyalar:**
- `CRMFiloServis.Web/Components/Pages/Login.razor`
- `README.md`
- `ROADMAP.md`

**Durum:** Tamamlandı

### Kayıt 018 - Muhasebe eşleştirme yönetim ekranları
**Talep:** Banka/kasa hesap ve hareketlerinde muhasebe eşleştirme alanlarının yönetilebilir hale getirilmesi.

**Sorun:**
Muhasebe eşleştirme alanları entity ve servis katmanında vardı; ancak banka hesap ve banka hareket ekranlarında bu alanlar yönetilemiyordu.

**Yapılanlar:**
- `BankaHesapForm.razor`: varsayılan muhasebe kodu ve kost merkezi alanları eklendi
- `BankaHesapForm.razor`: hesap tipine göre önerilen varsayılan muhasebe kodu ataması eklendi (`100` / `102` / `300`)
- `BankaHesapList.razor`: hesap kartlarında varsayılan muhasebe kodu ve kost merkezi görünür hale getirildi
- `BankaHareketForm.razor`: hareket bazlı muhasebe hesap kodu, alt hesap, kost merkezi, proje kodu ve muhasebe açıklama alanları eklendi
- `BankaHareketForm.razor`: seçilen hesaptan varsayılan muhasebe değerlerini doldurma desteği eklendi
- `BankaHareketList.razor`: hareket listesine muhasebe özeti kolonu eklendi
- `BankaKasaHareketService.cs`: create/update sırasında hesap varsayılanlarını servis katmanında otomatik uygulama eklendi

**Etkilenen Dosyalar:**
- `CRMFiloServis.Web/Components/Pages/BankaHesaplari/BankaHesapForm.razor`
- `CRMFiloServis.Web/Components/Pages/BankaHesaplari/BankaHesapList.razor`
- `CRMFiloServis.Web/Components/Pages/BankaHareketleri/BankaHareketForm.razor`
- `CRMFiloServis.Web/Components/Pages/BankaHareketleri/BankaHareketList.razor`
- `CRMFiloServis.Web/Services/BankaKasaHareketService.cs`

**Durum:** Tamamlandı

### Kayıt 017 - Servis Puantaj firma filtresi düzeltmesi
**Talep:** Servis puantaj ekranında "Tüm Firmalar" filtrelemesi ve toplu puantaj akışı düzeltmesi.

**Sorunlar:**
1. `YenileAsync` metodunda `firmaId = 0` olduğunda hard-coded `firmaId = 1` kullanılıyordu
2. Tüm firmalar seçildiğinde sadece FirmaId=1 olan eşleştirmeler geliyordu
3. Toplu puantaj üretiminde firma seçimi zorunlu değildi

**Yapılanlar:**
- `IFiloKomisyonService.cs`: `GetEslestirmelerAsync` ve `GetPuantajlarByTarihAraligiAsync` parametreleri nullable yapıldı
- `FiloKomisyonService.cs`: firmaId null veya 0 ise tüm firmaları getir
- `ServisPuantaj.razor`:
  - `YenileAsync`: Hard-coded değer kaldırıldı, nullable int kullanımı
  - `TopluPuantajOlustur`: Firma seçimi zorunlu hale getirildi (toplu üretim için)

**Etkilenen Dosyalar:**
- `CRMFiloServis.Web/Services/Interfaces/IFiloKomisyonService.cs`
- `CRMFiloServis.Web/Services/FiloKomisyonService.cs`
- `CRMFiloServis.Web/Components/Pages/Filo/ServisPuantaj.razor`

**Durum:** Tamamlandı

### Kayıt 016 - Bütçe ödemelerinde cari mahsup entegrasyonu
**Talep:** Bütçe ödemelerinde CariMahsup tipi seçildiğinde otomatik hareket ve muhasebe fişi üretimi.

**Sorun:**
BudgetService.OdemeYapAsync metodunda sadece `OdemeTipi.Mahsup` kontrol ediliyordu, `OdemeTipi.CariMahsup` için ayrı işlem yapılmıyordu. Bu durumda:
- Cari mahsup seçildiğinde BankaKasaHareket oluşturulmuyordu
- Muhasebe fişi üretilmiyordu

**Yapılanlar:**
- `BudgetService.cs`: IBankaKasaHareketService dependency injection eklendi
- `BudgetService.OdemeYapAsync`: CariMahsup tipi için ayrı branch eklendi
  - CariId ve BankaHesapId validasyonu
  - BankaKasaHareketService.CariMahsupAsync çağrısı
  - CaridenTahsilat yönü desteği
  - Otomatik muhasebe fişi zinciri (CariMahsupAsync → CreateCariMahsupFisiAsync)
  - Hareket ID'sini BudgetOdeme kaydına bağlama

**Akış:**
```
BudgetAnaliz → OdemeTipi.CariMahsup seç → OdemeYapAsync
  → BankaKasaHareketService.CariMahsupAsync
    → BankaKasaHareket oluştur
    → MuhasebeService.CreateCariMahsupFisiAsync
      → MuhasebeFis + MuhasebeFisKalem kayıtları
```

**Etkilenen Dosyalar:**
- `CRMFiloServis.Web/Services/BudgetService.cs`

**Durum:** Tamamlandı

### Kayıt 015 - Taşıma → Güzergah akışı doğrulama ve düzeltme
**Talep:** Fatura kaleminden güzergah oluşturma akışının doğrulanması ve düzeltilmesi.

**Sorunlar:**
1. CariId yanlış atanıyordu (Firma.Id yerine Fatura.CariId olmalı)
2. Aynı fatura kaleminden tekrar güzergah oluşturulabiliyordu (kontrol yoktu)
3. Aynı firma + güzergah adı kombinasyonu için benzersizlik kontrolü eksikti

**Yapılanlar:**
- `IGuzergahService.cs`: 3 yeni doğrulama metodu eklendi
  - FaturaKalemdenGuzergahVarMiAsync: Fatura kaleminden daha önce güzergah oluşturulmuş mu
  - GetByFaturaKalemIdAsync: Fatura kaleminden oluşturulan güzergahı getir
  - BenzersizGuzergahMiAsync: Firma + güzergah adı benzersizlik kontrolü
- `GuzergahService.cs`: Doğrulama region'ı ile metodlar implemente edildi
- `StokTuruEslestir.razor`: Güzergah oluşturma akışı düzeltildi
  - GuzergahOnizlemeItem'a CariId ve ZatenMevcut alanları eklendi
  - MevcutGuzergahKontrolEtAsync: Modal açıldığında mevcut kontrol
  - GuzergahlariOlustur: Doğrulama kontrolleri ve bilgilendirme mesajları

**Etkilenen Dosyalar:**
- `CRMFiloServis.Web/Services/Interfaces/IGuzergahService.cs`
- `CRMFiloServis.Web/Services/GuzergahService.cs`
- `CRMFiloServis.Web/Components/Pages/EFatura/StokTuruEslestir.razor`

**Durum:** Tamamlandı

### Kayıt 014 - Mahsup işlemleri muhasebe fişi entegrasyonu
**Talep:** Mahsup işlemlerinde otomatik muhasebe fişi üretimi ve iptal kaydı.

**Yapılanlar:**
- `IMuhasebeService.cs`: 3 yeni metod eklendi (CreateHesapTransferFisiAsync, CreateCariMahsupFisiAsync, IptalFisiOlusturAsync)
- `MuhasebeService.cs`: Mahsup fişi oluşturma implementasyonu eklendi
  - Hesaplar arası transfer için çift taraflı fiş (kaynak ALACAK, hedef BORÇ)
  - Cari mahsup için tahsilat/ödeme fişi (Kasa/Banka vs Alıcılar/Satıcılar)
  - İptal için ters kayıt (storno) fişi oluşturma
- `BankaKasaHareketService.cs`: IMuhasebeService bağımlılığı eklendi
  - HesaplarArasiTransferAsync: Transfer sonrası otomatik fiş üretimi
  - CariMahsupAsync: Cari mahsup sonrası otomatik fiş üretimi
  - MahsupIptalAsync: İptal öncesi ters kayıt fişi oluşturma
- Hesap tipine göre varsayılan muhasebe kodu eşleştirmesi (Kasa:100, Banka:102, Kredi:300)

**Etkilenen Dosyalar:**
- `CRMFiloServis.Web/Services/Interfaces/IMuhasebeService.cs`
- `CRMFiloServis.Web/Services/MuhasebeService.cs`
- `CRMFiloServis.Web/Services/BankaKasaHareketService.cs`

**Durum:** Tamamlandı

### Kayıt 013 - Login stabilizasyonu ve Servis Puantaj Excel export
**Talep:** Login akışı stabilitesi artırımı ve Servis Puantaj ekranı Excel export özelliği.

**Yapılanlar:**
- `RedirectToLogin.razor`: `forceLoad: true` yerine `false` yapıldı - circuit korunarak auth state kaybı önlendi
- `Login.razor`: Input değerleri trim edilerek gereksiz boşluk temizliği eklendi
- `Login.razor`: Auth state propagation bekleme süresi 100ms'den 150ms'ye artırıldı
- `ServisPuantaj.razor`: Excel export özelliği tamamlandı (ClosedXML ile puantaj tablosu export)

**Etkilenen Dosyalar:**
- `CRMFiloServis.Web/Components/Shared/RedirectToLogin.razor`
- `CRMFiloServis.Web/Components/Pages/Login.razor`
- `CRMFiloServis.Web/Components/Pages/Filo/ServisPuantaj.razor`

**Durum:** Tamamlandı

### Kayıt 012 - Bütçe Analiz ödeme ve hareket düzeltmeleri
**Talep:** Bütçe analizde ödeme yaparken kesinti/ek masraf hesaplama ve listeden kaldırma sorunları.

**Sorunlar:**
1. 791 TL ceza + 3,90 TL masraf kesintisi = Net 794,90 TL olması gerekirken (-) işaretsiz net rakam gelmiyordu
2. Kredi kartı listesinde net rakama eklenmesi gerekirken eksiltme yapılıyordu
3. Ödeme yapıldıktan sonra sağ taraftaki bekleyen ödemeler tablosundan kayıt kaldırılmıyordu
4. Kasa/banka hareketi silindiğinde ilişkili bütçe ödeme durumu geri alınmıyordu

**Yapılanlar:**
- Ek masraf değerleri için `Math.Abs()` ile mutlak değer alınması sağlandı (BudgetAnaliz.razor, BudgetService.cs, IBudgetService.cs)
- Net ödeme tutarı hesaplaması düzeltildi (her zaman tutar + ek masraf)
- Ödeme yapıldıktan sonra `bekleyenOdemeler` listesinden kayıt kaldırma eklendi
- `BankaKasaHareketService.DeleteAsync()` metodunda ilişkili bütçe ödeme durumunu "Bekliyor"a geri alma eklendi

**Etkilenen Dosyalar:**
- `CRMFiloServis.Web/Components/Pages/Budget/BudgetAnaliz.razor`
- `CRMFiloServis.Web/Services/BudgetService.cs`
- `CRMFiloServis.Web/Services/Interfaces/IBudgetService.cs`
- `CRMFiloServis.Web/Services/BankaKasaHareketService.cs`

**Durum:** Tamamlandı

### Kayıt 001 - E-Fatura PDF eşleştirme sorunu
**Talep:** PDF dosyasına sürekli aynı faturanın PDF'inin eklenmesi sorununun çözülmesi.

**Yapılanlar:**
- XML + PDF eşleştirme mantığı düzeltildi.
- import sırasında yanlış PDF eşleşmesi engellendi.
- PDF dosya adı üretimi benzersiz hale getirildi.

**Durum:** Tamamlandı

### Kayıt 011 - Servis katmanında takip/izleme güvenliği refaktörü
**Talep:** Bekleyen servis değişikliklerinin sınıflandırılması ve güvenli hale getirilmesi.

**Yapılanlar:**
- bekleyen servis değişikliklerinin büyük ölçüde aynı refaktör grubunda olduğu tespit edildi
- sorgu tarafında `AsNoTracking()` kullanımı yaygınlaştırıldı
- güncelleme işlemlerinde doğrudan `Update(entity)` yerine mevcut kaydı bulup alan bazlı güncelleme yaklaşımı uygulanmaya başlandı
- çözüm dosyasına yanlışlıkla eklenen harici proje referansı geri alındı

**Yapılacaklar:**
- davranış değişikliği riski düşük tutularak refaktör tamamlandı; kritik akışlarda manuel doğrulama önerilir

**Etkilenen Dosyalar:**
- `CRMFiloServis.Web/Services/BankaHesapService.cs`
- `CRMFiloServis.Web/Services/BankaKasaHareketService.cs`
- `CRMFiloServis.Web/Services/GuzergahService.cs`
- `CRMFiloServis.Web/Services/MasrafKalemiService.cs`
- `CRMFiloServis.Web/Services/PiyasaKaynakService.cs`
- `CRMFiloServis.Web/Services/SoforService.cs`
- `CRMFiloServis.slnx`

**Durum:** Tamamlandı

### Kayıt 002 - Taşıma hizmetlerinden güzergah üretimi
**Talep:** Stok türü eşleştirmede `Hizmet > Taşıma` seçildiğinde güzergah listesi hazırlanması ve kullanıcı onayı sonrası firma bazlı güzergah açılması.

**Yapılanlar:**
- güzergah için yeni alanlar eklendi
- sefer tipi ve personel sayısı alanları genişletildi
- önizleme ve oluşturma akışı başlatıldı
- CariId düzeltildi, doğrulama ve benzersizlik kontrolü eklendi (Kayıt 015)

**Durum:** Tamamlandı

### Kayıt 010 - Repo temizliği ve uploads takibinin kapatılması
**Talep:** Çalışma zamanında oluşan yükleme dosyalarının tekrar git takibine girmesinin engellenmesi.

**Yapılanlar:**
- kök `.gitignore` dosyasına `CRMFiloServis.Web/wwwroot/uploads/**` kuralı eklendi
- yükleme çıktıları için repo temizliği maddesi aktif takip listesine işlendi
- git takibine girmiş upload dosyaları index'ten çıkarıldı
- klasörü korumak için `CRMFiloServis.Web/wwwroot/uploads/.gitkeep` eklendi
- `CRMFiloServis.Web/Backups`, `deploy/Backups`, `deploy/Logs`, `deploy/Uploads` çalışma zamanı klasörleri ignore kapsamına alındı
- runtime klasörleri için `.gitkeep` istisna yaklaşımı genişletildi

**Yapılacaklar:**
- ek çalışma zamanı klasörü oluşursa aynı ignore + `.gitkeep` standardı uygulanmalı

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
- deploy dokümantasyonu ve build script başlıkları güncellendi
- lisans masaüstü uygulama başlıkları güncellendi
- lisans kodu ön eki `KOA-` olarak güncellendi
- login footer GitHub etiketi `Koa Filo Servis` olarak güncellendi
- `README.md` proje tablosundaki eski marka adı güncellendi
- `ROADMAP.md` başlığı güncellendi

**Yapılacaklar:**
- teknik namespace, repo adı ve veritabanı adı gibi ürün adı olmayan teknik referanslar korunarak ayrıştırma yaklaşımı sürdürülmeli

**Etkilenen Dosyalar:**
- `CRMFiloServis.Web/Components/Pages/Login.razor`
- `CRMFiloServis.Web/Components/Layout/NavMenu.razor`
- `README.md`
- `KURULUM.md`
- `ROADMAP.md`

**Durum:** Tamamlandı

### Kayıt 003 - Servis puantaj sistemi
**Talep:** Güzergah / araç / şoför eşleştirme ve günlük puantaj tablosu oluşturulması.

**Yapılanlar:**
- `FiloGuzergahEslestirme` ve `FiloGunlukPuantaj` yapıları devreye alındı
- puantaj ekranı için temel sayfa oluşturuldu
- Firma filtresi düzeltildi, toplu kayıt akışı tamamlandı (Kayıt 017)
- Excel export özelliği eklendi (Kayıt 013)

**Durum:** Tamamlandı

### Kayıt 004 - Mahsup işlemleri
**Talep:** Kasa / banka / kredi kartı ve cari hesaplar arası mahsup işlemleri.

**Yapılanlar:**
- hesaplar arası transfer mantığı geliştirildi
- cari mahsup yapısı eklendi
- mahsup ekranı oluşturuldu
- Fiş üretimi ve iptal kaydı eklendi (Kayıt 014)

**Durum:** Tamamlandı

### Kayıt 005 - Muhasebe eşleştirme kodları
**Talep:** Girilen banka hareketlerinde kullanıcı tarafından muhasebe eşleştirme kodlarının girilebilmesi.

**Yapılanlar:**
- banka/kasa hareketlerine muhasebe alanları eklendi
- hesap bazlı varsayılan muhasebe kodu alanları eklendi
- kost merkezi ve proje tanımları için altyapı eklendi
- banka hesap form ve liste ekranlarına muhasebe eşleştirme alanları eklendi
- banka hareket form ve liste ekranlarına muhasebe eşleştirme yönetimi eklendi
- hesap varsayılanlarının hareket kayıtlarında otomatik uygulanması eklendi

**Durum:** Tamamlandı

### Kayıt 006 - Bütçe ödeme kesintileri
**Talep:** Bütçe analiz ödeme ekranında masraf/ceza kesintileri ve ödeme şekline göre kayıt yapılması.

**Yapılanlar:**
- kesinti alanları eklendi
- net ödeme hesabı eklendi
- ödeme tipi alanları genişletildi
- CariMahsup entegrasyonu ve muhasebe fişi zinciri eklendi (Kayıt 016)
- Net tutar hesaplama düzeltildi, bekleyen ödemeler listesinden kaldırma eklendi (Kayıt 012)

**Durum:** Tamamlandı

### Kayıt 007 - Login ekranı sorunları
**Talep:** Şifre göster, beni hatırla ve giriş yap butonunun çalışmaması sorunlarının giderilmesi.

**Yapılanlar:**
- login sayfası birkaç kez düzenlendi
- route çakışması giderildi
- input binding ve giriş akışı üzerinde düzeltmeler yapıldı
- sayfa yeniden yapılandırıldı
- forceLoad düzeltildi, input trim eklendi, delay artırıldı (Kayıt 013)

**Durum:** Tamamlandı

### Kayıt 008 - Marka adı güncellemesi
**Talep:** Proje görünen adının `Koa Filo Servis` olarak düzenlenmesi.

**Yapılanlar:**
- login ekranı başlığı güncellendi
- menü marka adı güncellendi
- bazı görünür başlıklarda düzenleme yapıldı
- Görünür marka metinleri ve doküman başlıkları güncellendi (Kayıt 009, 019)

**Durum:** Tamamlandı

---

## Açık İş Özeti

| No | Konu | Durum | Öncelik | Not |
|---|---|---|---|---|
| 001 | Login akışı | Tamamlandı | Yüksek | forceLoad düzeltildi, input trim eklendi, delay artırıldı |
| 002 | Taşıma → Güzergah akışı | Tamamlandı | Yüksek | CariId düzeltildi, doğrulama ve benzersizlik kontrolü eklendi |
| 003 | Servis puantaj sistemi | Tamamlandı | Yüksek | Firma filtresi düzeltildi, toplu kayıt akışı tamamlandı |
| 004 | Mahsup işlemleri | Tamamlandı | Yüksek | Fiş üretimi ve iptal kaydı eklendi |
| 005 | Muhasebe eşleştirme ekranları | Tamamlandı | Orta | Hesap ve hareket ekranlarında yönetim alanları eklendi |
| 006 | Bütçe + cari mahsup | Tamamlandı | Yüksek | CariMahsup entegrasyonu ve muhasebe fişi zinciri eklendi |
| 007 | Marka adı tutarlılığı | Tamamlandı | Orta | Görünür marka metinleri ve doküman başlıkları güncellendi |
| 008 | Repo temizliği / uploads | Tamamlandı | Orta | Ignore + cached dosya temizliği yapıldı |
| 009 | Dokümantasyon marka güncellemesi | Tamamlandı | Düşük | README, kurulum, roadmap ve deploy başlıkları güncellendi |
| 010 | Çalışma zamanı dosya disiplini | Tamamlandı | Düşük | Upload, backup, log ve deploy runtime klasörleri ignore edildi |
| 011 | Servis refaktörlerinin sınıflandırılması | Tamamlandı | Orta | AsNoTracking, UTC ve soft delete audit tutarlılığı tamamlandı |
| 012 | Sayfalama altyapısı | Tamamlandı | Yüksek | CariList, FaturaList, BankaHareketList sayfalama destekli |

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

## Login Doğrulama Checklist

### Fonksiyonel Kontroller
- [ ] `/login` sayfası tek endpoint olarak açılıyor mu
- [ ] kullanıcı adı alanı veri alıyor mu
- [ ] şifre alanı veri alıyor mu
- [ ] `Giriş Yap` butonu tıklanınca servis çağrısı çalışıyor mu
- [ ] hatalı kullanıcı adı doğru hata mesajı veriyor mu
- [ ] hatalı şifre doğru hata mesajı veriyor mu
- [ ] başarılı giriş sonrası ana sayfaya yönleniyor mu
- [ ] giriş sonrası kullanıcı yetkili sayfalara erişebiliyor mu
- [ ] tarayıcı yenilendiğinde oturum davranışı beklenen şekilde mi

### UI Kontrolleri
- [ ] şifre göster / gizle çalışıyor mu
- [ ] `Beni Hatırla` seçimi kalıyor mu
- [ ] başarı ve hata mesajları görünüyor mu
- [ ] mobil görünümde form bozulmuyor mu

### Teknik Kontroller
- [ ] `AuthProvider` giriş sonrası state yayıyor mu
- [ ] `KullaniciService.GirisYapAsync` sonucu beklenen kullanıcıyı döndürüyor mu
- [ ] yönlendirme sonrası `AuthorizeRouteView` kullanıcıyı anonim görmüyor mu
- [ ] local storage erişiminde hata oluşursa login akışı kırılmıyor mu

---

## Not
Bu dosya canlı durum özeti olarak kullanılmalı.
Yeni modül veya önemli değişikliklerden sonra güncellenmesi önerilir.

Her yeni kullanıcı talebinde aşağıdaki 3 başlık mutlaka güncellenmelidir:
- `İstek Kayıtları`
- `Yapılanlar`
- `Yapılması Gerekenler`
