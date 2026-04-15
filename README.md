# KOA Filo Servis

Turkiye'deki filo yonetimi ve tasimacilik sirketleri icin kapsamli kurumsal kaynak planlama (ERP) cozumu.

## Proje Yapisi

- **KOAFiloServis.Web/** - Ana Blazor Server web uygulamasi
- **KOAFiloServis.Mobile/** - .NET MAUI mobil uygulama
- **KOAFiloServis.Shared/** - Paylasilan entity ve modeller
- **KOAFiloServis.Installer/** - Windows Forms kurulum/yedekleme araci

## Moduller

### Arac Yonetimi
- Arac kaydi ve envanter takibi
- Arac bakim planlamasi
- Kilometre ve yakit takibi
- Arac belge yonetimi

### Personel Yonetimi
- Personel kaydi ve ozluk dosyasi
- Maas yonetimi (resmi/gercek maas ayrimi)
- Bordro olusturma (Normal/AR-GE)
- Hesap pusulasi ve bordro icmali
- Odeme durumu takibi
- Avans ve borc yonetimi
- Izin yonetimi
- Puantaj takibi

### Muhasebe
- Cari hesap yonetimi
- Fatura yonetimi (gelen/giden)
- E-Fatura entegrasyonu
- Banka hesap takibi
- Kasa hareketleri
- Mali raporlar

### Raporlama
- Cari ekstre raporlari
- Muhasebe raporlari
- Personel raporlari
- Arac raporlari

### Diger Ozellikler
- Coklu firma destegi
- Rol tabanli yetkilendirme
- Dashboard ve grafikler
- Excel import/export
- Yazdirma destegi

## Teknolojiler

- **.NET 10** - Ana framework
- **Blazor Server** - Web arayuzu
- **.NET MAUI** - Mobil uygulama
- **Entity Framework Core** - ORM
- **PostgreSQL** - Veritabani
- **Bootstrap 5** - UI framework
- **ClosedXML** - Excel islemleri
- **QuestPDF** - PDF olusturma

## Kurulum

### Gereksinimler
- .NET 10 SDK
- PostgreSQL 14+

### Veritabani Kurulumu
```bash
createdb koafiloservis
dotnet ef database update --project KOAFiloServis.Web
```

### Uygulamayi Calistirma
```bash
cd KOAFiloServis.Web
dotnet run
```

## Lisans

Bu proje ozel lisans altindadir. Ticari kullanim icin izin gereklidir.

---

2024-2025 KOA Yazilim. Tum haklari saklidir.
