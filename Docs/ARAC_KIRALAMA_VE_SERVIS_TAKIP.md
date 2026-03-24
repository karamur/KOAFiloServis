# Araç Kiralama ve Servis Çalýþma Takip Sistemi

## ?? Genel Bakýþ

Bu sistem, filo yönetiminde hem **kendi araçlarý** hem de **dýþarýdan kiralanan araçlarýn** servis çalýþmalarýný takip etmek için tasarlanmýþtýr. Ayrýca **baþka firmalarýn güzergahlarýnda** çalýþan araçlarýn da kaydýný tutar.

---

## ?? 1. Kiralama Araç Sistemi

### Entity: `KiralamaArac`

Dýþarýdan kiralanan araçlarýn kaydýný tutar.

#### Özellikler:
- **Plaka, Marka, Model:** Araç bilgileri
- **Kiralayan Cari:** Aracý kiralayan firma/kiþi
- **Kiralama Dönemi:** Baþlangýç ve bitiþ tarihleri
- **Kira Bedeli Seçenekleri:**
  - Günlük kira bedeli
  - Sefer baþýna kira bedeli
  - Aylýk sabit kira bedeli
- **Komisyon:** Oran (%) veya sabit tutar
- **Sözleþme No:** Referans için

### Kullaným Örneði:
```csharp
var kiralamaArac = new KiralamaArac
{
    FirmaId = 1,
    KiralayýcýCariId = 5,
    Plaka = "34 ABC 123",
    Marka = "Mercedes",
    Model = "Sprinter",
    AracTipi = AracTipi.Midibus,
    KoltukSayisi = 19,
    KiralamaBaslangic = new DateTime(2026, 1, 1),
    SeferBasinaKiraBedeli = 500m, // Sefer baþý 500 TL
    KomisyonOrani = 10, // %10 komisyon
    Aktif = true
};

await _servisKiralamaService.CreateKiralamaAracAsync(kiralamaArac);
```

---

## ?? 2. Servis Çalýþma Takip Sistemi

### Entity: `ServisCalismaKiralama`

Hem kendi araçlarý hem kiralýk araçlar için **ortak** servis çalýþma kaydý.

#### Temel Alanlar:
- **Tarih:** Çalýþma tarihi
- **Servis Türü:** Sabah, Akþam, Sabah+Akþam, Özel
- **Araç Sahiplik Türü:**
  - `KendiArac`: Kendi filomuzdan
  - `KiralýkArac`: Dýþarýdan kiralanan

#### Araç Bilgileri:
- **Kendi Aracýmýz ise:** `AracId` (Arac tablosundan)
- **Kiralýk Araç ise:** `KiralamaAracId` (KiralamaArac tablosundan)

#### Çalýþma Detaylarý:
- **Þoför:** Hangi þoför çalýþtý
- **Güzergah:** Hangi güzergahta çalýþtý
- **Müþteri Firma:** Baþka firma için mi çalýþtý?

#### Finansal Bilgiler:
- **Çalýþma Bedeli:** Müþteriden alýnan ücret
- **Araç Kira Bedeli:** Kiralýk araç ise ödenen kira
- **Komisyon Tutarý:** Varsa komisyon
- **Net Kazanç:** Otomatik hesaplanýr
  ```
  Net Kazanç = Çalýþma Bedeli - Kira Bedeli - Komisyon
  ```

### Kullaným Örneði:
```csharp
// Kiralýk araçla çalýþma
var calisma = new ServisCalismaKiralama
{
    FirmaId = 1,
    CalismaTarihi = DateTime.Today,
    ServisTuru = ServisTuru.SabahAksam,
    AracSahiplikTuru = AracSahiplikTuru.KiralýkArac,
    KiralamaAracId = 10, // Kiralýk araç
    SoforId = 5,
    GuzergahId = 3,
    MusteriFirmaId = 2, // Baþka firma için çalýþtýk
    CalismaBedeli = 1500m, // Müþteriden 1500 TL aldýk
    KmBaslangic = 10000,
    KmBitis = 10150,
    Durum = CalismaDurum.Tamamlandi
};

await _servisKiralamaService.CreateServisCalismaAsync(calisma);
// Otomatik olarak kira bedeli ve net kazanç hesaplanýr
```

### Otomatik Hesaplama:
```csharp
// Net kazanç hesaplama
await _servisKiralamaService.HesaplaAsync(calismaId);

// Sonuç:
// Çalýþma Bedeli: 1500 TL
// Kira Bedeli: 500 TL (sefer baþý)
// Komisyon: 150 TL (%10)
// Net Kazanç: 850 TL
```

---

## ?? 3. Raporlar ve Sorgular

### Tarih Aralýðýnda Çalýþmalar:
```csharp
var baslangic = new DateTime(2026, 3, 1);
var bitis = new DateTime(2026, 3, 31);

// Mart ayý çalýþmalarý
var calismalari = await _servisKiralamaService.GetServisCalismalariAsync(
    firmaId: 1, 
    baslangic, 
    bitis
);

// Sadece kiralýk araçlar
var kiralamaRapor = await _servisKiralamaService.GetServisCalismaRaporuAsync(
    firmaId: 1,
    baslangic,
    bitis,
    sahiplikTuru: AracSahiplikTuru.KiralýkArac
);
```

### Araç Bazýnda Kazanç:
```csharp
var aracKazanc = await _servisKiralamaService.GetAracBazindaKazancAsync(
    firmaId: 1,
    baslangic,
    bitis
);

// Sonuç:
// { "34 ABC 123": 25000, "34 XYZ 789": 18500, ... }
```

### Güzergah Bazýnda Kazanç:
```csharp
var guzergahKazanc = await _servisKiralamaService.GetGuzergahBazindaKazancAsync(
    firmaId: 1,
    baslangic,
    bitis
);

// Sonuç:
// { "Beþiktaþ-Sarýyer": 45000, "Kadýköy-Ataþehir": 38000, ... }
```

---

## ?? 4. Excel Raporlarý

### 4.1. Servis Çalýþma Raporu

**Kolonlar:**
- Tarih
- Plaka
- Sahiplik (Kendi/Kiralýk)
- Þoför
- Güzergah
- Müþteri Firma
- Servis Türü
- Çalýþma Bedeli
- Kira Bedeli
- Komisyon
- Net Kazanç
- Km
- Baþlangýç/Bitiþ Saati
- Durum

**Özellikler:**
- Kiralýk araçlar sarý arka plan
- Toplam satýrlarý
- Para birimi formatý
- Otomatik geniþlik

```csharp
byte[] excel = await _servisKiralamaService.ExportServisCalismaRaporuAsync(
    firmaId: 1,
    baslangic: new DateTime(2026, 3, 1),
    bitis: new DateTime(2026, 3, 31)
);

File.WriteAllBytes("servis_raporu_mart2026.xlsx", excel);
```

### 4.2. Kiralama Araç Listesi

Tüm kiralýk araçlarýn listesi:
- Plaka, Marka/Model
- Araç Tipi
- Kiralayan
- Kiralama Dönemi
- Kira Bedelleri (Günlük/Sefer/Aylýk)
- Durum

```csharp
byte[] excel = await _servisKiralamaService.ExportKiralamaAracListesiAsync(firmaId: 1);
```

### 4.3. Aylýk Özet

Belirli bir ay için özet rapor:
- Toplam Servis Sayýsý
- Kendi Araç / Kiralýk Araç Sayýsý
- Toplam Gelir
- Toplam Kira Gideri
- Toplam Net Kazanç

```csharp
byte[] excel = await _servisKiralamaService.ExportAylikOzetAsync(
    firmaId: 1,
    yil: 2026,
    ay: 3
);
```

---

## ?? 5. Aylýk Ödeme Tablosu (Güncellenmiþ)

### Excel Raporlarý Eklendi:

#### 5.1. Aylýk Ödeme Tablosu
**Özellik:** Firma kýsýtlamasý YOK - Tüm firmalar bir arada

```csharp
byte[] excel = await _aylikOdemeService.ExportAylikOdemeTablosuAsync(
    yil: 2026,
    ay: 3
);
```

**Kolonlar:**
- Firma
- Ödeme Türü
- Ödeme Adý
- Gün
- Planlanan
- Ödenen
- Kalan
- Durum

#### 5.2. Yýllýk Ödeme Tablosu
12 ay sütunlu yýllýk görünüm:

```csharp
byte[] excel = await _aylikOdemeService.ExportYillikOdemeTablosuAsync(yil: 2026);
```

**Format:**
```
| Firma | Ödeme | Ocak | Þubat | Mart | ... | Toplam |
|-------|-------|------|-------|------|-----|--------|
| Ana   | Kira  | 5000 | 5000  | 5000 | ... | 60000  |
```

---

## ?? 6. Kullaným Senaryolarý

### Senaryo 1: Dýþarýdan Araç Kiralama

```csharp
// 1. Kiralýk aracý sisteme ekle
var kiralamaArac = await _servisKiralamaService.CreateKiralamaAracAsync(new KiralamaArac
{
    FirmaId = 1,
    Plaka = "34 TEST 123",
    KiralayýcýCariId = 10,
    KiralamaBaslangic = DateTime.Today,
    SeferBasinaKiraBedeli = 600m
});

// 2. Bu araçla çalýþma kaydet
var calisma = await _servisKiralamaService.CreateServisCalismaAsync(new ServisCalismaKiralama
{
    FirmaId = 1,
    CalismaTarihi = DateTime.Today,
    AracSahiplikTuru = AracSahiplikTuru.KiralýkArac,
    KiralamaAracId = kiralamaArac.Id,
    SoforId = 5,
    GuzergahId = 3,
    CalismaBedeli = 1200m
});

// Net kazanç otomatik hesaplanýr: 1200 - 600 = 600 TL
```

### Senaryo 2: Baþka Firma Ýçin Çalýþma

```csharp
var calisma = await _servisKiralamaService.CreateServisCalismaAsync(new ServisCalismaKiralama
{
    FirmaId = 1, // Bizim firmamýz
    MusteriFirmaId = 2, // Müþteri firma
    CalismaTarihi = DateTime.Today,
    AracSahiplikTuru = AracSahiplikTuru.KendiArac,
    AracId = 5, // Kendi aracýmýz
    SoforId = 3,
    GuzergahId = 10, // Müþterinin güzergahý
    CalismaBedeli = 2000m
});
```

### Senaryo 3: Haftalýk Plan Oluþtur

```csharp
// Önceki haftanýn çalýþmalarýndan otomatik plan oluþtur
var haftaBaslangic = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1);
var planlananCalisma = await _servisKiralamaService.HaftalikPlanOlusturAsync(
    firmaId: 1,
    haftaBaslangic
);

// 7 günlük plan oluþturulur (tekrar eden güzergahlar için)
```

---

## ?? 7. Ýstatistikler ve Dashboard

```csharp
// Toplam kiralýk araç sayýsý
int kiralamaAracSayisi = await _servisKiralamaService.GetToplamKiralamaAracSayisiAsync(firmaId);

// Bu ay servis sayýsý
int servisSayisi = await _servisKiralamaService.GetAylikServisSayisiAsync(firmaId, 2026, 3);

// Bu ay toplam kazanç
decimal kazanc = await _servisKiralamaService.GetAylikToplamKazancAsync(firmaId, 2026, 3);

// Bu ay kira gideri
decimal kiraGideri = await _servisKiralamaService.GetAylikKiraBedeliAsync(firmaId, 2026, 3);

// Yýllýk servis daðýlýmý (12 ay)
var aylikServis = await _servisKiralamaService.GetAylikServisSayisiAsync(firmaId, 2026);
// { 1: 120, 2: 115, 3: 130, ... }
```

---

## ??? 8. Veritabaný Yapýsý

### DbSets (ApplicationDbContext):
```csharp
public DbSet<KiralamaArac> KiralamaAraclar { get; set; }
public DbSet<ServisCalismaKiralama> ServisCalismaKiralamalar { get; set; }
```

### Migration:
```bash
cd CRMFiloServis.Web
dotnet ef migrations add KiralamaVeServisTakip
dotnet ef database update
```

### Service Registration (Program.cs):
```csharp
builder.Services.AddScoped<IServisKiralamaService, ServisKiralamaService>();
```

---

## ?? 9. Öne Çýkan Özellikler

? **Kendi ve Kiralýk Araç Ayrýmý:** Tek sistemde her iki türü yönet
? **Baþka Firma Çalýþmalarý:** Müþteri firma takibi
? **Otomatik Hesaplamalar:** Net kazanç, kira, komisyon
? **Esnek Kira Bedeli:** Günlük, sefer veya aylýk
? **Detaylý Raporlar:** Araç, güzergah, tarih bazlý
? **Excel Export:** Profesyonel raporlar
? **Tarih Aralýðý Sorgularý:** Ýstenen dönem için liste
? **Haftalýk Plan:** Otomatik tekrar planý
? **Dashboard Ýstatistikleri:** Hýzlý özet bilgiler

---

## ?? Dosya Yapýsý

```
CRMFiloServis/
??? Shared/
?   ??? Entities/
?       ??? KiralamaVeServis.cs
?           ??? KiralamaArac
?           ??? ServisCalismaKiralama
?           ??? AracSahiplikTuru
?           ??? ServisCalismaRapor
??? Web/
    ??? Services/
        ??? ServisKiralamaService.cs
        ??? AylikOdemeService.cs (güncellendi)
        ??? Interfaces/
            ??? IServisKiralamaService.cs
            ??? IAylikOdemeService.cs (güncellendi)
```

---

## ?? Sonraki Adýmlar

1. ? Entity ve Service oluþturuldu
2. ? Migration çalýþtýrýlacak
3. ? UI Sayfalarý:
   - `/filo/kiralama-araclar` - Kiralýk araç listesi
   - `/filo/servis-calisma` - Servis çalýþma kayýtlarý
   - `/filo/servis-plan` - Haftalýk plan görünümü
4. ? Dashboard Widget'larý
5. ? Mobil Uygulama Entegrasyonu

Sistem hazýr ve test edilmeye hazýr!
