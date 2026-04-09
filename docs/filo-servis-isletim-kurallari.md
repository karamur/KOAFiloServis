# Filo Servis İşletim Kuralları

Bu dosya, uygulamadaki menü düzeni ve filo sahiplik kurallarının ekip tarafından aynı şekilde uygulanması için oluşturulmuştur.

## Menü Düzeni

### EBYS Belge Yönetimi
Aşağıdaki sayfalar `EBYS Belge Yönetimi` başlığı altında birlikte konumlandırılmalıdır:
- `ebys` → EBYS Belge Merkezi
- `ebys/gelen` → Gelen Evraklar
- `ebys/giden` → Giden Evraklar
- `ebys/takip` → Evrak Takip
- `ebys/kategoriler` → Evrak Kategorileri

### Ayarlar Altına Taşınan Bölümler
Aşağıdaki bölümler `Ayarlar` altında gruplanmalıdır:
- Destek
- Entegrasyon

## Hata Yönetimi

Program kırılmadan kullanıcıya rapor ekranı gösterilmelidir.

### Beklenen davranış
- Hata alınan sayfa kaydedilir.
- Hata öncesindeki sayfa kaydedilir.
- Kullanıcı `ters-giden-bir-sey` sayfasında hata detayını görür.
- Kullanıcı isterse önceki sayfaya geri dönebilir.

## Filo Sahiplik Kuralları

### 1. Özmal
- Plaka firmaya ait
- Şoför firmaya ait
- Araç firmaya ait
- Operasyon ve masraf takibi firma sorumluluğunda değerlendirilir

### 2. Kiralık
- Plaka kiralık/dış kaynaklı olabilir
- Şoför firmaya ait
- Araç firmaya ait operasyon ekibi tarafından yönetilir
- Şoför ve operasyon yönetimi firma tarafında, plaka/kiralama bedeli kiralık yapıya göre değerlendirilir

### 3. Komisyon
- Plaka kişiye ait
- Şoför kişiye ait
- Araç kişiye ait
- Masraflar kişiye ait
- Bu tip yapılarda taşeron/komisyon mantığı ile tahakkuk ve ödeme akışları değerlendirilmelidir

## Uygulama Notu
Bu kurallar yeni ekran, rapor, puantaj, tahakkuk, araç atama ve maliyet hesaplama geliştirmelerinde referans kabul edilmelidir.
