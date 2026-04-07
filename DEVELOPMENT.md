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

### Kayıt 043 - İhale Hazırlık Modülü (AI Destekli Maliyet Analizi)
**Talep:** Personel servisi firmasının süresine göre, güzergah/mesafe, özmal/kiralık/komisyon durumlarında, araç model ve yakıt ortalamasına göre araç masrafları AI tahmini (kullanıcı değiştirebilsin), şoför maaş AI tahmini (geriye dönük ve enflasyon), aylık/saatlik/sefer başı birim fiyatlar, kâr/zarar/masraf tablosu. Sınırsız proje oluşturma.

**Yapılanlar:**
- `IhaleHazirlik.cs` entity oluşturuldu:
  - `IhaleProje`: ProjeKodu (IHL-YYYY-NNNN), ProjeAdi, CariId, FirmaId, BaslangicTarihi, BitisTarihi, SozlesmeSuresiAy, EnflasyonOrani, YakitZamOrani, AylikCalismGunu, GunlukCalismaSaati, Durum, AIAnaliz
  - `IhaleGuzergahKalem`: Hat bilgileri (ad/başlangıç/bitiş/mesafe/süre/sefer), araç bilgileri (sahiplik/model/koltuk/yakıt), 7 masraf kategorisi, kira/komisyon, şoför (brüt/net/SGK %22.5), amortisman, maliyet/kâr/teklif hesaplamaları, birim fiyatlar (aylık/sefer/saat/km)
  - `AylikProjeksiyon`: Enflasyonlu aylık maliyet projeksiyon detayları
  - `IhaleProjeDurum` enum: Taslak/Hazirlaniyor/TeklifVerildi/Kazanildi/Kaybedildi/IptalEdildi
  - `AracSahiplikKalem` enum: Ozmal/Kiralik/Komisyon
- `IhaleHazirlikModels.cs` oluşturuldu - DTO'lar:
  - `IhaleMaliyetTahminIstek/Sonuc`: AI masraf tahmin request/response
  - `IhaleSoforMaasTahmin`: Brüt/net/SGK/toplam/enflasyonlu maaş tahmini
  - `IhaleProjeOzet`: Proje toplamları + kalem özetleri + aylık projeksiyon
- `IIhaleHazirlikService.cs` interface oluşturuldu - 17 metot
- `IhaleHazirlikService.cs` implementasyon oluşturuldu (~470 satır):
  - **Proje CRUD**: Auto ProjeKodu (IHL-2025-0001), deep copy kopyalama
  - **Kalem CRUD**: Güzergah/araç/şoför bilgi otomatik aktarımı
  - **Maliyet Hesaplama**: Yakıt (mesafe×sefer×tüketim×fiyat), komisyon, SGK %22.5, amortisman, toplam, kâr, teklif, birim fiyatlar
  - **Enflasyonlu Projeksiyon**: Bileşik faiz formülü, yakıt ayrı zam oranı, amortisman sabit
  - **AI Araç Masraf Tahmini**: Gerçek masraf DB ortalaması + Ollama JSON prompt → 7 masraf kalemi
  - **AI Şoför Maaş Tahmini**: Mevcut şoför ortalaması + asgari ücret + Ollama → brüt/net/SGK/enflasyonlu
  - **AI Proje Analizi**: Proje özet → Ollama stratejik analiz (kâr marjı, risk, rekabet, öneri)
- `IhaleHazirlik.razor` oluşturuldu (~700 satır):
  - **Proje Listesi**: Kart grid, durum badge'leri (renk kodlu), düzenle/kopyala/sil
  - **Proje Detay**: Bilgi kartları, hat tablosu, birim fiyat kartları
  - **Proje Modal**: Tüm proje bilgileri CRUD formu
  - **Kalem Modal**: Güzergah + araç + masraflar + şoför + kâr marjı, AI Tahmin butonları
  - **Rapor**: Enflasyonlu projeksiyon tablosu, kümülatif hesap, toplam kartları
- `ApplicationDbContext.cs` güncellendi - IhaleProjeleri, IhaleGuzergahKalemleri DbSet eklendi
- `Program.cs` güncellendi - IIhaleHazirlikService DI kaydı eklendi
- `NavMenu.razor` güncellendi - İhale Hazırlık menü bölümü eklendi
- EF Core migration oluşturuldu (IhaleHazirlikModulu)

**Etkilenen Dosyalar:**
- `CRMFiloServis.Shared/Entities/IhaleHazirlik.cs` (yeni)
- `CRMFiloServis.Web/Models/IhaleHazirlikModels.cs` (yeni)
- `CRMFiloServis.Web/Services/Interfaces/IIhaleHazirlikService.cs` (yeni)
- `CRMFiloServis.Web/Services/IhaleHazirlikService.cs` (yeni)
- `CRMFiloServis.Web/Components/Pages/Ihale/IhaleHazirlik.razor` (yeni)
- `CRMFiloServis.Web/Data/ApplicationDbContext.cs` (güncellendi)
- `CRMFiloServis.Web/Program.cs` (güncellendi)
- `CRMFiloServis.Web/Components/Layout/NavMenu.razor` (güncellendi)

**Durum:** ✅ Tamamlandı

### Kayıt 042 - AI Destekli Fatura Import ve Cari Geliştirme
**Talep:** Cari modülden kesilen/gelen faturaları XML yüklerken yapay zeka ile cari kart kontrolü, güzergah eşleştirme, stok kartı kontrolü, kalem sınıflandırma ve puantaj entegrasyonu.

**Yapılanlar:**
- `FaturaAIImportModels.cs` oluşturuldu - AI fatura import DTO'ları:
  - `FaturaAIAnalizSonuc`: Fatura bilgileri, satıcı/alıcı, cari eşleşme, kalemler, AI yorum
  - `FaturaAICariBilgi`: Unvan, VergiNo, TcKimlikNo, VergiDairesi, Adres, İl/İlçe
  - `CariEslesmeSonuc`: Mevcut/yeni cari, eşleşme yöntemi (VergiNo/TcKimlikNo/Unvan)
  - `FaturaAIKalem`: AI kalem tipi, alt tipi, güven skoru, kullanıcı düzeltme, güzergah/stok eşleşme
  - `GuzergahEslesmeSonuc`, `StokEslesmeSonuc`: Benzer kayıtlar, otomatik eşleşme
- `IFaturaAIImportService.cs` oluşturuldu - 7 metot interface
- `FaturaAIImportService.cs` oluşturuldu (~550 satır):
  - **XML Parse**: UBL 2.1 e-fatura formatı (cbc/cac namespace), satıcı/alıcı party, kalemler, tevkifat, vade
  - **Cari Eşleştirme**: VergiNo → TcKimlikNo → Unvan tam → Unvan kısmi → yeni oluştur
  - **AI Kalem Sınıflandırma**: Ollama ile JSON format sınıflandırma (Hizmet/Mal/Kiralama/Servis)
  - **Güzergah Eşleştirme**: Kelime benzerlik skoru, cari bonus +20%, >70% otomatik eşleşme
  - **Stok Eşleştirme**: Ürün kodu tam eşleşme, açıklama benzerlik
  - **Kaydet**: Transaction (cari → güzergah → fatura+kalemler → güzergah FaturaKalemId güncelle)
  - UBL birim kodları normalizasyonu (C62→Adet, KGM→Kg, LTR→Lt, HUR→Saat vb.)
- `FaturaAIImport.razor` oluşturuldu (~550 satır) - 4 adımlı wizard:
  - Adım 1: XML dosya yükleme (boyut kontrolü, AI bağlantı uyarısı)
  - Adım 2: Cari kontrol (mevcut eşleşme/yeni oluşturma/farklı cari seçme)
  - Adım 3: Kalem analizi tablosu (AI tipi, kullanıcı düzeltme, güzergah/stok eşleşme dropdown)
  - Adım 4: Kaydet sonucu ve yönlendirme
- `CariService.cs` güncellendi - İletişim notu, hatırlatıcı ve vade uyarı implementasyonları:
  - `GetIletisimNotlariAsync`, `AddIletisimNotuAsync`, `UpdateIletisimNotuAsync`, `DeleteIletisimNotuAsync`
  - `GetCariHatirlaticilariAsync`, `AddCariHatirlaticiAsync`
  - `GetVadeUyarilariAsync` (kritik/gecikmiş/bugün/yaklaşan vade sınıflandırma)
- `NavMenu.razor` güncellendi - Fatura menüsüne "AI Fatura Import" linki eklendi
- `Program.cs` güncellendi - `IFaturaAIImportService` DI kaydı eklendi

**Etkilenen Dosyalar:**
- `CRMFiloServis.Web/Models/FaturaAIImportModels.cs` (yeni)
- `CRMFiloServis.Web/Services/Interfaces/IFaturaAIImportService.cs` (yeni)
- `CRMFiloServis.Web/Services/FaturaAIImportService.cs` (yeni)
- `CRMFiloServis.Web/Components/Pages/Cariler/FaturaAIImport.razor` (yeni)
- `CRMFiloServis.Web/Services/CariService.cs` (güncellendi)
- `CRMFiloServis.Web/Components/Layout/NavMenu.razor` (güncellendi)
- `CRMFiloServis.Web/Program.cs` (güncellendi)

**Durum:** ✅ Tamamlandı

### Kayıt 040 - AI Destekli Muhasebeleştirme ve Puantaj Analizi
**Talep:** Muhasebeleştirme ve puantaj kısımlarında yapay zeka desteği ile öneri, tahmin, kontrol bulguları sunma ve kullanıcıya aksiyon alma imkanı.

**Yapılanlar:**
- `OllamaService.cs` güncellendi - `AnalizYapAsync(prompt, sistemPrompt)` metodu eklendi:
  - Özelleştirilebilir sistem prompt desteği
  - 2048 token yanıt limiti (rapor yorumlamadan daha uzun)
  - `IOllamaService` interface'e yeni metot eklendi
- `MuhasebeleştirmeModels.cs` güncellendi - `AIAksiyon` ve `PuantajAIAksiyon` sınıfları eklendi
- `Muhasbelestirme.razor` - AI muhasebe analizi eklendi:
  - **AI Analiz butonu** fatura ve masraf sekmelerinde (mor renkli, robot ikonu)
  - **AI Analiz Modalı**: Ollama model adı göstergesi, analiz süresi, temizleme
  - **Fatura AI Analizi**: Kontrol bulgularını + fatura detaylarını AI'ya gönderir
    - Tutarlılık analizi (aynı cariye birden fazla fatura, olağandışı tutarlar)
    - KDV ve tevkifat doğruluğu kontrolü
    - Vergisel risk uyarıları
    - Muhasebe kaydı oluşturma önerileri
  - **Masraf AI Analizi**: Kategori dağılımı + araç bazlı dağılım + kontrol bulgularını AI'ya gönderir
    - Anomali tespiti (olağandışı tutarlar, sık tekrarlar)
    - Gider hesap eşleştirme önerileri (770.06, 770.07 vb.)
    - Maliyet optimizasyonu önerileri
  - **Aksiyon Listesi**: AI yanıtından otomatik parse edilen YÜKSEK/ORTA/DÜŞÜK öncelikli aksiyonlar
    - Checkbox ile seçilebilir aksiyonlar
    - Renkli öncelik badge'leri
- `CalismaPuantaji.razor` - AI puantaj analizi eklendi:
  - **AI Analiz butonu** toolbar'da (mor renkli)
  - **AI Puantaj Analiz Modalı**: Ay/yıl bilgisi, personel sayısı göstergesi
  - **Puantaj AI Analizi**: Personel bazlı detay + günlük dağılım + fazla mesai detayı gönderir
    - Devamsızlık pattern analizi (sık izin/mazeret, ardışık günler)
    - Fazla mesai analizi (İş Kanunu uygunluğu: haftalık 45 saat, aylık 270 saat)
    - Anomali tespiti (belirli günlerde toplu izin, olağandışı çalışma düzeni)
    - Verimlilik değerlendirmesi
    - Gelecek ay tahmini (trend bazlı devamsızlık/fazla mesai beklentisi)
  - **Aksiyon Listesi**: Aynı parse mekanizması ile öncelikli aksiyonlar

**Etkilenen Dosyalar:**
- `CRMFiloServis.Web/Services/OllamaService.cs` (güncellendi - AnalizYapAsync eklendi)
- `CRMFiloServis.Web/Models/MuhasebeleştirmeModels.cs` (güncellendi - AIAksiyon, PuantajAIAksiyon)
- `CRMFiloServis.Web/Components/Pages/Muhasebe/Muhasbelestirme.razor` (güncellendi)
- `CRMFiloServis.Web/Components/Pages/Personel/CalismaPuantaji.razor` (güncellendi)

**Durum:** ✅ Tamamlandı

### Kayıt 039 - Fatura/Masraf Muhasebeleştirme Geliştirme
**Talep:** Fatura ve masraf muhasebeleştirme sayfasına kontrol listesi, işlenmiş kayıtlar görüntüleme, geri alma ve Excel export özellikleri ekle.

**Yapılanlar:**
- `MuhasebeleştirmeModels.cs` güncellendi - 3 yeni model + 1 enum eklendi:
  - `MuhasbelestirmeKontrol`: Kontrol sonucu (HazirMi, Maddeler, UyariSayisi, HataSayisi, BilgiSayisi)
  - `KontrolMaddesi`: Kontrol maddesi detayı (Baslik, Aciklama, Seviye, IlgiliKayit)
  - `KontrolSeviye` enum: Bilgi, Uyari, Hata
  - `MuhasbelestirilmisKayit`: İşlenmiş kayıt DTO (KaynakId, KaynakTip, Tutar, FisId, FisNo, Secildi)
- `IMuhasebeService.cs` güncellendi - 4 yeni metot:
  - `KontrolYapAsync`: Muhasebeleştirme öncesi kontrol (hesap planı, ayarlar, dönem, cari, tevkifat, hesap eşleşme)
  - `GetMuhasbelestirilmisKayitlarAsync`: İşlenmiş fatura+masraf birleşik liste (fiş bilgileriyle)
  - `TopluGeriAlAsync`: Fiş silme + fatura/masraf muhasebeleştirme durumu geri alma
  - `ExportMuhasbelestirmeKontrolExcelAsync`: 3 sayfalı Excel export (Faturalar/Masraflar/Kontrol Listesi)
- `MuhasebeService.cs` güncellendi (~300 satır yeni kod):
  - `KontrolYapAsync`: Hesap planı boş mu, muhasebe ayarları var mı, aktif dönem var mı, fatura cari eksik mi, tevkifatlı fatura var mı, 120/320/770 hesapları tanımlı mı
  - `GetMuhasbelestirilmisKayitlarAsync`: Fatura (MuhasebeFisiOlusturuldu=true) ve masraf (MuhasebeFisId!=null) birleşik listesi, fiş numaraları dahil
  - `TopluGeriAlAsync`: Fiş kalemleri + fiş silme, ilişkili fatura/masraf muhasebe bağlantısı temizleme
  - `ExportMuhasbelestirmeKontrolExcelAsync`: ClosedXML ile 3 sayfalı Excel (koşullu renklendirme)
- `Muhasbelestirme.razor` tamamen güncellendi:
  - **İşlenmiş Kayıtlar Sekmesi** (yeni): Fatura/Masraf filtre, fiş detayına link, toplu geri alma
  - **Kontrol Listesi Modalı** (yeni): Hata/Uyarı/Bilgi kartları, madde listesi, kontrol sonrası muhasebeleştirmeye devam
  - **Excel Export butonları**: Fatura/masraf kontrol listesi Excel indirme
  - **Geri Alma**: Seçili işlenmiş kayıtların muhasebe fişlerini silip durumu geri alma
  - **Gelişmiş buton grubu**: "Kontrol Et", "Excel", "Muhasebeleştir" butonları her sekmede
- ROADMAP: #10 ve #11 tamamlandı olarak işaretlendi

**Etkilenen Dosyalar:**
- `CRMFiloServis.Web/Models/MuhasebeleştirmeModels.cs` (güncellendi)
- `CRMFiloServis.Web/Services/Interfaces/IMuhasebeService.cs` (güncellendi)
- `CRMFiloServis.Web/Services/MuhasebeService.cs` (güncellendi)
- `CRMFiloServis.Web/Components/Pages/Muhasebe/Muhasbelestirme.razor` (güncellendi)

**Durum:** ✅ Tamamlandı

### Kayıt 035 - Bordro Personel Bazlı Düzenleme
**Talep:** Bordro detaylarında personel bazlı maaş/kesinti/ek ödeme düzenleme özelliği. Normal ve AR-GE bordrolarda ayrı ayrı.

**Yapılanlar:**
- `NormalBordro.razor`: Detay tablosuna "Düzenle" butonu ve tam düzenleme modalı eklendi
  - Maaş bilgileri: Brüt, Net, SGK Maaşı, Toplu Maaş, Ek Ödeme (fark)
  - Kesintiler: SGK+İşsizlik, Gelir Vergisi, Damga Vergisi
  - Ek Ödemeler: Yemek, Yol, Prim, Diğer
  - Notlar alanı, canlı toplam hesaplama (Toplam Kesinti, Toplam Ek Ödeme, Toplam Ödenecek)
  - Eski JS `prompt` düzenleme kaldırıldı, modal ile değiştirildi
  - Kalan Ödeme sekmesindeki düzenleme butonu da modalı kullanıyor
  - Onaylı bordrolarda düzenleme engellendi
- `ArgeBordro.razor`: Aynı düzenleme modalı ve buton yapısı eklendi (AR-GE etiketli)
- ROADMAP: #18 tamamlandı olarak işaretlendi

**Durum:** ✅ Tamamlandı

### Kayıt 036 - Bordro Hesap Pusulası
**Talep:** Bordro hesap pusulası - Personel bazlı yazdırılabilir maaş makbuzu (pay slip).

**Yapılanlar:**
- `HesapPusulasi.razor` oluşturuldu (`/personel/bordro/hesap-pusulasi`)
  - Filtre: Yıl, Firma, Bordro Tipi (Normal/AR-GE), Dönem seçimi
  - Personel listesi tablosu: checkbox seçim, maaş özet bilgileri
  - Tek personel / seçili personeller / tüm personeller yazdırma
  - A4 print-ready pusula formatı (CSS @media print):
    - Firma bilgileri (ünvan, adres, vergi dairesi/no)
    - Personel bilgileri (sicil no, TC, görev/departman, işe başlama, banka/IBAN)
    - Kazançlar: Brüt maaş, SGK matrah, net maaş, toplu maaş, ek ödeme farkı
    - Kesintiler: SGK+İşsizlik, gelir vergisi, damga vergisi, toplam
    - Ek ödemeler: Yemek, yol, prim, diğer, toplam
    - Ödeme durumu: Banka/Ek ödeme yapıldı/bekliyor
    - Toplam ödenecek tutar (büyük font, vurgulu)
    - İmza alanları (İşveren + Personel)
    - Onay bilgisi ve düzenlenme tarihi
  - Her personel ayrı sayfada (page-break-after) - toplu yazdırmada
- NavMenu'ya "Hesap Pusulası" linki eklendi (Bordro altına)
- ROADMAP: #19 tamamlandı olarak işaretlendi

**Durum:** ✅ Tamamlandı

### Kayıt 038 - Personel Excel Import
**Talep:** Excel dosyasından toplu personel yükleme (import) özelliği.

**Yapılanlar:**
- `PersonelImport.razor` oluşturuldu - Toplu personel Excel import sayfası
  - **Excel Şablon İndirme**: ClosedXML ile hazır şablon (16 kolon), örnek veriler ve açıklama sayfası
  - **Dosya Yükleme**: InputFile ile .xlsx yükleme (10MB limit)
  - **Önizleme**: Excel parse → satır bazlı durum tespiti (Yeni/Güncelleme/Atla/Hata)
  - **Mevcut Personel Kontrolü**: TC Kimlik No veya Ad+Soyad ile eşleşme tespiti
  - **Güncelleme Modu**: Checkbox ile mevcut personeli güncelleme opsiyonu
  - **Toplu Kaydetme**: ISoforService.CreateAsync/UpdateAsync ile yeni ekleme veya güncelleme
  - **Otomatik Kod Üretimi**: GenerateNextKodAsync(gorev) ile görev bazlı personel kodu
  - **Bordro Tipi Otomatik Ayarma**: Yok/Normal/Arge → SGKBordroDahilMi senkronizasyonu
  - **Durum Filtreleme**: Yeni/Güncelleme/Atlanan/Hatalı filtre dropdown
  - **Özet Kartları**: Toplam/Yeni/Güncelleme/Atla/Hata/İşlenecek sayıları
  - **Tarih/Para Birimi Parse**: Türkçe format desteği (GG.AA.YYYY, virgül/nokta)
  - **Görev Parse**: Şoför/Ofis/Muhasebe/Yönetici/Teknik/Diğer (case-insensitive, alias destekli)
- NavMenu'ya "Excel Import" linki eklendi (Personel bölümü)
- ROADMAP: #20 tamamlandı olarak işaretlendi

**Etkilenen Dosyalar:**
- `CRMFiloServis.Web/Components/Pages/Personel/PersonelImport.razor` (yeni)
- `CRMFiloServis.Web/Components/Layout/NavMenu.razor`

**Durum:** ✅ Tamamlandı

---

### Kayıt 037 - Bütçe Analiz Geliştirme + AI Rapor Yorumlama (Ollama)
**Talep:** Bütçe Analiz sayfasına kategori bazlı analiz, aylık trend grafikleri ve Ollama (internetsiz AI) ile akıllı rapor yorumlama ekle.

**Yapılanlar:**
- `OllamaService.cs` oluşturuldu - Local LLM entegrasyonu (Ollama REST API)
  - `IOllamaService` interface: `RaporYorumlaAsync`, `BaglantiKontrolAsync`
  - Ollama `/api/generate` endpoint kullanımı
  - Configurable model (appsettings: `Ollama:Model`, default: `llama3.2`)
  - Configurable base URL (default: `http://localhost:11434`)
  - Sistem promptu: Türk mali müşavir rolü, kısa/öz/aksiyona yönelik
  - 3 dakika timeout, hata yönetimi
- `appsettings.json`: Ollama konfigürasyonu eklendi
- `Program.cs`: HttpClient("Ollama") + IOllamaService DI kaydı
- `BudgetAnaliz.razor` güncellemeleri:
  - **Kategori Dağılımı paneli**: Progress bar'lı kategori listesi, yüzde oranları, toplam
  - **Aylık Harcama Trendi paneli**: Tablo + stacked progress bar (ödenen/bekleyen oranı), ortalama/en yüksek/en düşük ay istatistikleri
  - **AI Bütçe Analizi paneli**: Ollama bağlantı durumu göstergesi, 5 analiz türü (Genel/Kategori/Trend/Tasarruf/Anomali), analiz süresi göstergesi, sonuç temizleme
  - Dinamik prompt oluşturma: Bütçe özeti + Kategori dağılımı + Aylık trend + Kredi/taksit + Gecikmiş ödemeler otomatik derleniyor
  - OnInitializedAsync'te kategori/trend/Ollama bağlantı kontrolü
  - YenileDataAsync'te kategori/trend otomatik güncelleme
- ROADMAP: #12, #25, #26, #27 tamamlandı olarak işaretlendi

**Durum:** ✅ Tamamlandı

---

### Kayıt 034 - Toplu Ödeme Listesi Banka EFT Export
**Talep:** Banka ödeme listesine banka portalına yüklenebilir toplu ödeme (EFT/Havale) dosyası export özelliği.

**Yapılanlar:**
- `BankaOdemeListesi.razor`: Mevcut sayfaya "EFT Dosyası" butonu eklendi
  - Semicolon-delimited CSV formatı (Türk bankaları genel uyumu)
  - Header satırı: Tarih, toplam adet, toplam tutar, para birimi, açıklama
  - Veri satırları: IBAN, Ad Soyad, Tutar, Açıklama, Personel Kodu
  - IBAN’sız personel uyarısı (eksik IBAN bildirimi)
  - UTF-8 BOM encoding (Türkçe karakter desteği)
- ROADMAP: #17 tamamlandı olarak işaretlendi

**Durum:** ✅ Tamamlandı

---

### Kayıt 033 - Maaş Hareket Listesi
**Talep:** Personel maaş ödeme geçmişi görüntüleme sayfası. Tüm aylara ait maaş kayıtlarının filtrelenerek listelenmesi, detay görüntüleme ve Excel export.

**Yapılanlar:**
- `MaasHareketleri.razor`: Tam sayfa oluşturuldu (`/personel/maas-hareketleri`)
  - Personel, Yıl, Ay, Ödeme Durumu filtreleri
  - Özet kartları: Toplam Kayıt, Toplam Ödenen, Bekleyen, Genel Toplam
  - Detaylı hareket tablosu: Brüt/Net maaş, eklemeler, kesintiler, ödenecek, çalışma günü, ödeme durumu
  - Personel bazlı özet tablosu (tüm personel görünümünde)
  - Detay modalı: Dönem bilgisi, ödeme bilgisi, ek ödemeler, kesintiler detayı
  - Excel export (ClosedXML)
  - Toast bildirim sistemi
- NavMenu: "Maaş Hareketleri" linki eklendi (Personel menüsü altına)
- ROADMAP: #16 tamamlandı olarak işaretlendi

**Durum:** ✅ Tamamlandı

---

### Kayıt 032 - Personel Servis Çalışma Puantajı
**Talep:** Personel bazlı günlük/aylık çalışma puantaj takibi sayfası. Hangi gün çalıştı, izinli, mazeretli olduğu takip edilecek.

**Yapılanlar:**
- `CalismaPuantaji.razor`: Tam sayfa oluşturuldu (`/personel/puantaj`)
  - Firma + Yıl + Ay filtreleri
  - Personel arama (ad soyad / şoför kodu)
  - Özet kartlar: Personel sayısı, Ort. çalışılan gün, Toplam izin/mazeret, F. mesai, Net ödeme
  - Takvim grid tablosu (personel × gün matrisi):
    - Satırlar: Personel adı ve kodu
    - Sütunlar: Ayın günleri (1-28/30/31) + Ç/İ/M/FM özet sütunları
    - Haftasonu renklendirme (Cumartesi sarı, Pazar kırmızı)
    - Durum badge'leri: Ç (Çalıştı), İ (İzinli), M (Mazeret), FM (Fazla Mesai)
    - Sticky header ve sol sütun (kaydırma desteği)
  - Footer toplam satırı (günlük çalışan personel sayısı)
  - Hücre tıklama ile günlük düzenleme modalı:
    - Çalıştı/İzinli/Mazeret toggle (karşılıklı exclusive)
    - Fazla mesai saat girişi
    - Not alanı
  - "Ay Puantajı Oluştur" butonu (aktif şoförlere otomatik puantaj ve günlük kayıt oluşturma)
  - Günlük verilerden aylık özet otomatik hesaplama
  - "Hesapla" butonu (maaş kesintileri hesaplama)
  - Excel export (mevcut PuantajService.ExportPuantajListesiAsync kullanarak)
  - Toast bildirim sistemi, IDisposable implementasyonu
- `NavMenu.razor`: "Çalışma Puantajı" linki eklendi (Personel menüsü altına, İzin Yönetimi sonrası)
- `ROADMAP.md`: "Personel Servis Çalışma Puantajı" tamamlandı olarak işaretlendi

**Mevcut Altyapı Kullanımı:**
- `PersonelPuantaj` entity: Aylık özet (CalisilanGun, IzinGunu, MazeretGunu, FazlaMesaiSaat, maaş alanları)
- `GunlukPuantaj` entity: Günlük detay (Calisti, Izinli, Mazeret, FazlaMesaiSaat, ServisCalismaId)
- `IPuantajService / PuantajService`: CRUD, günlük puantaj, otomatik oluşturma, hesaplama, Excel export

**Etkilenen Dosyalar:**
- `CRMFiloServis.Web/Components/Pages/Personel/CalismaPuantaji.razor` (yeni)
- `CRMFiloServis.Web/Components/Layout/NavMenu.razor` (güncelleme)
- `ROADMAP.md` (güncelleme)

**Durum:** Tamamlandı

### Kayıt 031 - Fatura/Masraf Resmi Muhasebe Kaydı (Toplu Muhasebeleştirme)
**Talep:** Girilen fatura ve masrafların toplu olarak resmi yevmiye kaydı (muhasebe fişi) oluşturulması.

**Yapılanlar:**
- `MuhasebeleştirmeModels.cs`: DTO modeller oluşturuldu
  - `MuhasebeFaturaOzet`: Fatura özet bilgileri (seçim desteği ile)
  - `MuhasebeMasrafOzet`: Masraf özet bilgileri (seçim desteği ile)
  - `MuhasbelestirmeSonuc`: Toplu işlem sonucu (başarılı/hatalı sayısı, hatalar)
  - `MuhasbelestirmeDurum`: Genel durum özeti (bekleyen/işlenmiş sayıları)
- `IMuhasebeService.cs`: Yeni metotlar eklendi
  - GetMuhasbelestirmeDurumuAsync: Durum özeti
  - GetMuhasbelestirilmemisFaturalarAsync: Bekleyen fatura listesi (filtre desteği)
  - GetMuhasbelestirilmemisMasraflarAsync: Bekleyen masraf listesi (filtre desteği)
  - TopluFaturaMuhasbelestirAsync: Toplu fatura muhasebeleştirme
  - TopluMasrafMuhasbelestirAsync: Toplu masraf muhasebeleştirme
- `MuhasebeService.cs`: Implementasyon
  - Toplu fatura muhasebeleştirme (mevcut CreateFaturaFisiAsync kullanılarak)
  - Toplu masraf muhasebeleştirme (yeni CreateMasrafMuhasebeFisiAsync)
  - Masraf kategorisine göre gider hesabı eşleme (770.06-770.09)
  - Karşı hesap otomatik belirleme (cari/personel/kasa)
- `Muhasbelestirme.razor`: Tam sayfa oluşturuldu
  - Özet kartları (bekleyen/işlenmiş fatura ve masraf sayıları)
  - Tarih ve fatura yönü filtreleri
  - Fatura/Masraf sekme navigasyonu
  - Tümünü seç/kaldır, tekli seçim
  - Seçili toplamlar (footer)
  - Toplu muhasebeleştir butonu (loading state)
  - Sonuç modalı (başarılı/hatalı ayrıntılı gösterim)
  - Toast bildirimleri
- `NavMenu.razor`: "Muhasebeleştirme" linki eklendi (Muhasebe menüsü altına)

**Etkilenen Dosyalar:**
- `CRMFiloServis.Web/Models/MuhasebeleştirmeModels.cs` (güncelleme)
- `CRMFiloServis.Web/Services/Interfaces/IMuhasebeService.cs` (güncelleme)
- `CRMFiloServis.Web/Services/MuhasebeService.cs` (güncelleme)
- `CRMFiloServis.Web/Components/Pages/Muhasebe/Muhasbelestirme.razor` (yeni)
- `CRMFiloServis.Web/Components/Layout/NavMenu.razor` (güncelleme)

**Durum:** Tamamlandı

### Kayıt 030 - Maaş Yönetimi "Ödeme Yap" Butonu Bug Fix
**Talep:** Maaş yönetimi sayfasındaki "Ödeme Yap" butonu pasif durumda, düzgün çalışmıyordu.

**Yapılanlar:**
- `MaasYonetimi.razor`: Ödeme Yap butonu tamamen yeniden tasarlandı
  - Onay modalı eklendi (tarih seçimi, açıklama girişi)
  - Loading durumu eklendi (işlem sırasında spinner gösterimi)
  - Başarı/hata toast bildirimleri eklendi (3 sn otomatik kapanma)
  - Ödeme iptal etme özelliği eklendi (ödendi → bekliyor geri alma)
  - Ödendi durumunda ödeme tarihi tooltip olarak gösteriliyor
  - Toplu maaş oluşturma sonrası bildirim eklendi
  - Maaş kaydetme sonrası bildirim eklendi
  - `IDisposable` implementasyonu eklendi (timer temizliği)

**Etkilenen Dosyalar:**
- `CRMFiloServis.Web/Components/Pages/Personel/MaasYonetimi.razor` (güncelleme)

**Durum:** Tamamlandı

### Kayıt 029 - Cari Borç/Alacak Detaylı Takip
**Talep:** Cari hesaplar için detaylı borç/alacak analizi, risk skorlaması ve tahsilat planlaması.

**Yapılanlar:**
- `CariHareketTakipModels.cs`: Yeni modeller oluşturuldu
  - `CariHareketTakipRapor`: Tek cari detaylı rapor (bakiye, vade analizi, risk skoru, trend)
  - `CariHareketDetay`: Hareket listesi (fatura + ödeme birleşik)
  - `CariAcikFatura`: Açık fatura detayı (vade durumu, öncelik)
  - `CariAylikTrend`: Aylık trend verisi
  - `TahsilatPlanItem`: Tahsilat planı öğesi
  - `CariBorcAlacakOzet`: Tüm cariler özet raporu
  - `CariHareketTakipOzet`: Cari özet satırı
  - `CariTipiBakiyeDagilimi`, `GenelAylikTrend`
- `ICariHareketTakipService.cs`: Interface oluşturuldu
  - GetBorcAlacakOzetAsync: Tüm cariler özet
  - GetCariDetayAsync: Tek cari detay
  - GetCariHareketlerAsync: Hareket listesi
  - GetAcikFaturalarAsync: Açık faturalar
  - GetAylikTrendAsync: Aylık trend
  - HesaplaRiskSkoruAsync: Risk skoru hesaplama
  - OlusturTahsilatPlaniAsync: Tahsilat planı
  - ExportToExcelAsync: Excel export
- `CariHareketTakipService.cs`: Tam implementasyon
  - Risk skoru hesaplama (0-100 arası)
  - Vade analizi (0-30, 31-60, 61-90, 90+ gün)
  - Ortalama ödeme süresi hesaplama
  - Tahsilat planı öneri sistemi
- `CariHareketTakip.razor`: Ana sayfa oluşturuldu
  - Tüm cariler özet görünümü (filtreler, vade analizi, cari tipi dağılımı)
  - Tek cari detay görünümü (bilgiler, bakiye, risk, hareket listesi, açık faturalar, tahsilat planı)
  - Hareket detay modal
  - Excel export
- `Program.cs`: Servis DI kaydı eklendi
- `NavMenu.razor`: "Borç/Alacak Takip" linki eklendi (Cari Modülü altına)

**Etkilenen Dosyalar:**
- `CRMFiloServis.Web/Models/CariHareketTakipModels.cs` (yeni)
- `CRMFiloServis.Web/Services/Interfaces/ICariHareketTakipService.cs` (yeni)
- `CRMFiloServis.Web/Services/CariHareketTakipService.cs` (yeni)
- `CRMFiloServis.Web/Components/Pages/Cariler/CariHareketTakip.razor` (yeni)
- `CRMFiloServis.Web/Program.cs`
- `CRMFiloServis.Web/Components/Layout/NavMenu.razor`

**Durum:** Tamamlandı

### Kayıt 028 - Proforma Fatura Sistemi
**Talep:** Fatura kesilmeden önce müşteriye proforma fatura gönderme sistemi.

**Yapılanlar:**
- `ProformaFatura.cs`: Yeni entity oluşturuldu (ProformaFatura, ProformaFaturaKalem sınıfları)
- `ProformaDurum` enum eklendi (Taslak, Gonderildi, Onaylandi, Reddedildi, FaturayaDonusturuldu, SuresiDoldu)
- `ApplicationDbContext.cs`: DbSet ve OnModelCreating konfigürasyonları eklendi
- `IProformaFaturaService.cs`: Interface oluşturuldu
- `ProformaFaturaService.cs`: Tam implementasyon
  - CRUD işlemleri
  - Numara otomatik üretimi (PRF-YYYYMM-XXXX)
  - Faturaya dönüştürme (FaturayaDonusturAsync)
  - Süresi dolan proformaları güncelleme
  - Excel export desteği
- `Program.cs`: Servis DI kaydı eklendi
- `ProformaList.razor`: Liste sayfası oluşturuldu
  - Arama, filtreleme (durum, tarih aralığı)
  - İstatistik kartları (toplam, onaylanan, bekleyen, reddedilen)
  - Hızlı işlem butonları (faturaya dönüştür, onayla, reddet)
- `ProformaForm.razor`: Form sayfası oluşturuldu
  - Cari seçimi
  - Kalem ekleme/silme
  - KDV ve toplam otomatik hesaplama
  - Geçerlilik tarihi
- `ProformaDetay.razor`: Detay sayfası oluşturuldu
  - Durum badge'leri
  - Kalem listesi
  - İşlem butonları (onayla, reddet, faturaya dönüştür, Excel)
- `NavMenu.razor`: Proforma Faturalar linki eklendi
- Migration oluşturuldu: `AddProformaFatura`

**Etkilenen Dosyalar:**
- `CRMFiloServis.Shared/Entities/ProformaFatura.cs` (yeni)
- `CRMFiloServis.Web/Data/ApplicationDbContext.cs`
- `CRMFiloServis.Web/Services/Interfaces/IProformaFaturaService.cs` (yeni)
- `CRMFiloServis.Web/Services/ProformaFaturaService.cs` (yeni)
- `CRMFiloServis.Web/Program.cs`
- `CRMFiloServis.Web/Components/Pages/ProformaFaturalar/ProformaList.razor` (yeni)
- `CRMFiloServis.Web/Components/Pages/ProformaFaturalar/ProformaForm.razor` (yeni)
- `CRMFiloServis.Web/Components/Pages/ProformaFaturalar/ProformaDetay.razor` (yeni)
- `CRMFiloServis.Web/Components/Layout/NavMenu.razor`

**Durum:** Tamamlandı

### Kayıt 024 - Şoför Performans Raporu
**Talep:** Şoförlerin performansını analiz eden detaylı rapor sayfası oluşturulması.

**Yapılanlar:**
- `SoforPerformansRaporModels.cs`: Yeni rapor modelleri oluşturuldu (SoforPerformansOzet, SoforAracPerformansi, SoforGuzergahPerformansi, SoforAylikPerformans, SoforKarsilastirmaOzeti)
- `IRaporService.cs`: 2 yeni metod eklendi (GetSoforPerformansAsync, GetSoforKarsilastirmaAsync)
- `RaporService.cs`: Şoför performans metodları implementasyonu eklendi
- `SoforPerformansRapor.razor`: Şoför performans raporu sayfası oluşturuldu
  - Bireysel şoför detaylı performans özeti
  - Tüm şoförler karşılaştırma tablosu
  - Özet kartları (toplam sefer, kazanç, çalışılan gün, arıza oranı)
  - Aylık performans grafiği (Chart.js entegrasyonu)
  - Araç ve güzergah bazlı analiz tabloları
  - Excel export desteği
- `NavMenu.razor`: Raporlar menüsüne "Şoför Performans" linki eklendi

**Etkilenen Dosyalar:**
- `CRMFiloServis.Web/Models/SoforPerformansRaporModels.cs` (yeni)
- `CRMFiloServis.Web/Services/Interfaces/IRaporService.cs`
- `CRMFiloServis.Web/Services/RaporService.cs`
- `CRMFiloServis.Web/Components/Pages/Raporlar/SoforPerformansRapor.razor` (yeni)
- `CRMFiloServis.Web/Components/Layout/NavMenu.razor`

**Durum:** Tamamlandı

### Kayıt 025 - Araç Karlılık Raporu
**Talep:** Araç bazlı karlılık analizi raporu - gelir/gider/kar hesaplama, masraf dağılımı, aylık trend grafiği.

**Yapılanlar:**
- `AracKarlilikRaporModels.cs`: Yeni rapor modelleri oluşturuldu (AracKarlilikOzet, AracMasrafDetay, AracAylikKarlilik, AracGuzergahPerformansi, AracKarsilastirmaOzeti)
- `IRaporService.cs`: 2 yeni metod eklendi (GetAracKarlilikAsync, GetAracKarsilastirmaAsync)
- `RaporService.cs`: Araç karlılık metodları implementasyonu eklendi
  - Gelir hesaplama: ServisCalisma.HesaplananFiyat
  - Gider hesaplama: AracMasraf.Tutar + KiraBedeli + Komisyon
  - Kiralık araçlar için aylık kira bedeli hesaplama
  - Komisyonlu araçlar için oran veya sabit komisyon hesaplama
- `AracKarlilikRapor.razor`: Araç karlılık raporu sayfası oluşturuldu
  - Tekil araç detaylı karlılık analizi
  - Tüm araçlar karşılaştırma tablosu
  - Özet kartları (gelir, gider, net kar, arıza oranı)
  - Aylık karlılık grafiği (multi-bar chart - gelir/gider/kar)
  - Masraf dağılımı doughnut chart
  - Güzergah bazlı performans tablosu
  - Masraf detay tablosu
  - Excel export desteği
- `NavMenu.razor`: Raporlar menüsüne "Araç Karlılık" linki eklendi
- `dashboard-charts.js`: createMultiBarChart fonksiyonu eklendi

**Etkilenen Dosyalar:**
- `CRMFiloServis.Web/Models/AracKarlilikRaporModels.cs` (yeni)
- `CRMFiloServis.Web/Services/Interfaces/IRaporService.cs`
- `CRMFiloServis.Web/Services/RaporService.cs`
- `CRMFiloServis.Web/Components/Pages/Raporlar/AracKarlilikRapor.razor` (yeni)
- `CRMFiloServis.Web/Components/Layout/NavMenu.razor`
- `CRMFiloServis.Web/wwwroot/js/dashboard-charts.js`

**Durum:** Tamamlandı

### Kayıt 026 - Cari Bakiye Yaşlandırma Raporu
**Talep:** Cari bakiyelerin vade tarihine göre yaşlandırma analizi - 0-30, 31-60, 61-90, 90+ gün bantları.

**Yapılanlar:**
- `CariYaslandirmaRaporModels.cs`: Yeni rapor modelleri oluşturuldu
  - `CariYaslandirmaRapor`: Genel rapor özeti (toplam bakiye, bant toplamları, cari sayıları)
  - `CariYaslandirmaOzet`: Cari bazlı yaşlandırma özeti (bakiye bantları, risk seviyesi)
  - `YaslandirmaBandi`: Yaşlandırma bandı özeti (tutar, fatura/cari sayısı, oran)
  - `CariTipiDagilimi`: Cari tipi bazlı dağılım
  - `YaslandirmaFaturaDetay`: Fatura bazlı yaşlandırma detayı
- `IRaporService.cs`: 2 yeni metod eklendi
  - `GetCariYaslandirmaAsync`: Genel yaşlandırma raporu
  - `GetCariYaslandirmaDetayAsync`: Tek cari detaylı yaşlandırma
- `RaporService.cs`: Yaşlandırma metodları implementasyonu eklendi
  - Vade tarihine göre gecikme günü hesaplama
  - Yaşlandırma bantlarına dağıtım (0-30, 31-60, 61-90, 90+ gün)
  - Risk seviyesi hesaplama (Normal, Düşük, Orta, Yüksek)
  - Cari tipi ve fatura bazlı gruplama
- `CariYaslandirmaRapor.razor`: Cari yaşlandırma raporu sayfası oluşturuldu
  - Filtre alanı (rapor tarihi, cari tipi, cari seçimi, sadece borçlu cariler)
  - Özet kartları (toplam bakiye, güncel, vadesi geçmiş, kritik)
  - Pie chart: Yaşlandırma dağılımı
  - Horizontal bar chart: Yaşlandırma bantları
  - Bant özet tablosu (tutar, fatura/cari sayısı, oran, progress bar)
  - Cari bazlı detay tablosu (risk seviyesi renklendirmeli)
  - Fatura detay modal (cari tıklandığında)
  - Excel export desteği
- `NavMenu.razor`: Raporlar menüsüne "Cari Yaşlandırma" linki eklendi
- `dashboard-charts.js`: 2 yeni fonksiyon eklendi
  - `createPieChart`: Pasta grafik
  - `createYaslandirmaBarChart`: Horizontal bar chart

**Etkilenen Dosyalar:**
- `CRMFiloServis.Web/Models/CariYaslandirmaRaporModels.cs` (yeni)
- `CRMFiloServis.Web/Services/Interfaces/IRaporService.cs`
- `CRMFiloServis.Web/Services/RaporService.cs`
- `CRMFiloServis.Web/Components/Pages/Raporlar/CariYaslandirmaRapor.razor` (yeni)
- `CRMFiloServis.Web/Components/Layout/NavMenu.razor`
- `CRMFiloServis.Web/wwwroot/js/dashboard-charts.js`

**Durum:** Tamamlandı

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
| 015 | Proforma Fatura Sistemi | Tamamlandı | Yüksek | Entity, servis, 3 sayfa, faturaya dönüştürme, Excel export |
| 016 | Cari Borç/Alacak Takip | Tamamlandı | Yüksek | Risk skorlama, vade analizi, tahsilat planı, detaylı rapor |
| 012 | Sayfalama altyapısı | Tamamlandı | Yüksek | CariList, FaturaList, BankaHareketList sayfalama destekli |
| 013 | Şoför Performans Raporu | Tamamlandı | Orta | Bireysel/karşılaştırma, grafik, Excel export |
| 014 | Araç Karlılık Raporu | Tamamlandı | Orta | Gelir/gider/kar analizi, masraf dağılımı, trend grafikleri |

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
