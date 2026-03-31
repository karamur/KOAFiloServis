# CRM Filo Servis

Filo Yonetimi, Muhasebe ve CRM Uygulamasi - .NET 10 Blazor Server

## ?? Ozellikler

### ?? Ana Moduller

#### 1. **Cari Yonetimi**
- Musteri ve tedarikci kaydi
- Cari hesap takibi
- Cari ekstre raporlari

#### 2. **Filo Servis**
- Arac kaydi ve takibi (Ozmal, Kiralik, Komisyon)
- Sofor yonetimi
- Guzergah tanimlama
- Servis calismasi kayitlari
- Toplu calisma girisi
- Arac masraflari takibi

#### 3. **E-Fatura / E-Arsiv**
- **Gelen Faturalar (Alis)**
  - Odeme tarihi belirleme ve degistirme
  - Odeme durumu takibi (Odendi, Kismi Odendi, Odenmedi)
  - Vade gecmis uyarilari
  - Butce analize aktarim

- **Giden Faturalar (Satis)**
  - Tahsilat durumu (Tahsil Edildi, Kismi Tahsilat, Tahsil Edilmedi)
  - Tahsilat raporu
  - Vade takibi

#### 4. **Banka / Kasa**
- Banka hesaplari yonetimi
- Kasa takibi
- Tahsilat ve odeme islemleri
- Fatura eslestirme
- Hareket silme ve duzenleme

#### 5. **Muhasebe**
- Standart hesap plani (otomatik yukleme)
- Muhasebe fisleri
- Gelir tablosu
- Bilanco

#### 6. **Satis Modulu**
- Arac ilanlari yonetimi
- **Piyasa Arastirma**
  - Sahibinden / Arabam / Cargratis karsilastirma
  - Sadece aktif ilanlar (Satilmis/Kaldirilmis/Rezerve filtreleme)
  - 1 yildan eski ilanlar filtrelenir
  - Fiyat analizi
  - Excel/PDF export
- Satis personeli yonetimi
- Komisyon takibi

#### 7. **Personel Yonetimi**
- Personel kaydi
- Maas yonetimi
- Izin takibi
- Belge uyarilari (Ehliyet, SRC, Psikoteknik, Saglik Raporu)

#### 8. **Butce ve Raporlar**
- Butce analizi (odeme yapilinca listeden kalkar)
- Mali analiz
- Aylik checklist
- Ozmal/Kiralik arac raporlari
- Komisyon raporlari
- Fatura odeme takvimleri

#### 9. **Sistem Yonetimi**
- Kullanici yonetimi
- Rol ve yetki sistemi
- Lisans yonetimi
- Otomatik yedekleme
- Aktivite loglari

## ??? Teknolojiler

- **.NET 10** - Ana framework
- **Blazor Server** - UI framework
- **Entity Framework Core** - ORM
- **PostgreSQL** - Veritabani
- **Bootstrap 5** - CSS framework
- **Bootstrap Icons** - Ikonlar
- **EPPlus** - Excel islemleri

## ?? Gereksinimler

- .NET 10 SDK
- PostgreSQL 14+
- Visual Studio 2022 veya VS Code

## ?? Kurulum

### Masaustu Kurulum Programi ile

1. `CRMFiloServisKurulum.exe` calistirin
2. Kurulum tipi secin:
   - **Normal**: Windows Server / IIS
   - **Docker**: Docker Compose
3. Kurulum modu secin:
   - **Sifir Kurulum**: Yeni veritabani
   - **Mevcut Yedek Ile**: Var olan veriyi tasima
4. Kurulum paketini secin (.zip)
5. Kurulumu baslatin

### Manuel Kurulum

1. **Repository'yi klonlayin:**
```bash
git clone https://github.com/karamur/CRMFiloServis.git
cd CRMFiloServis
```

2. **PostgreSQL baglantisini ayarlayin:**
`CRMFiloServis.Web/appsettings.json` dosyasinda:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=crmfiloservis;Username=postgres;Password=yourpassword"
  }
}
```

3. **Uygulamayi calistirin:**
```bash
cd CRMFiloServis.Web
dotnet run
```

4. **Tarayicida acin:**
```
http://localhost:5190
```

## ?? Proje Yapisi

```
CRMFiloServis/
??? CRMFiloServis.Shared/        # Paylasilan entity ve modeller
??? CRMFiloServis.Web/           # Ana Blazor uygulamasi
??? CRMFiloServis.Installer/     # Masaustu kurulum programi
??? CRMFiloServis.LisansDesktop/ # Lisans olusturucu
??? deploy/                      # Yayin scriptleri
```

## ?? Yayin Paketi Olusturma

```powershell
# Tum paketler
.\deploy\Build-All.ps1 -Version "1.0.0"

# Sadece web paketi
.\deploy\Build-WebPackage.ps1 -Version "1.0.0"

# Sadece kurulum programi
.\deploy\Build-Installer.ps1
```

## ?? Program Ici Guncelleme

1. `Ayarlar > Uygulama Guncelleme` menusune gidin
2. Yeni versiyon ZIP dosyasini yukleyin
3. Guncellemeyi baslatin
4. Uygulama otomatik yeniden baslar

## ?? Lisanslama

1. `Ayarlar > Lisans` menusunden PC Kodunu kopyalayin
2. PC Kodunu lisans yetkilisine gonderin
3. Alinan aktivasyon kodunu girin
4. Lisans aktive edilir

## ?? Varsayilan Giris

- **Kullanici Adi:** admin
- **Sifre:** admin123

## ?? Lisans Turleri

| Ozellik | Trial | Basic | Professional | Enterprise |
|---------|-------|-------|--------------|------------|
| Sure | 30 gun | 1 yil | 1 yil | 1 yil |
| Kullanici | 5 | 5 | 10 | Sinirsiz |
| Excel Export | ? | ? | ? | ? |
| PDF Export | ? | ? | ? | ? |
| Raporlama | ? | ? | ? | ? |
| Yedekleme | ? | ? | ? | ? |
| Muhasebe | ? | - | ? | ? |
| Satis Modulu | ? | - | ? | ? |

## ?? Son Guncellemeler

### v1.2.0 (2025)
- ? Masaustu kurulum programi eklendi (Normal / Docker secenekli)
- ? Program ici guncelleme destegi
- ? PC Kodu lisans menusunden alinabiliyor
- ? Stok turu eslestirmede stok kodu arama/sec/manuel yaz
- ? Gecmis odenmemis borclar ana sayfada

### v1.1.0 (2024)
- ? Butce analiz - odeme yapilinca listeden kaldirilir
- ? Butce analiz - duzenleme ve silme butonlari eklendi
- ? Mali analiz - para birimi TL formatina duzeltildi
- ? Banka/Kasa hareketleri - silme ve para birimi duzeltildi
- ? Piyasa arastirma - 1 yildan eski ilanlar filtrelenir
- ? Piyasa arastirma - satilmis/kaldirilmis/rezerve ilanlar filtrelenir

### v1.0.0 (2024)
- ? Gelen fatura odeme tarihi ve durumu
- ? Giden fatura tahsilat durumu
- ? Butce analize otomatik aktarim
- ? Takvimde odeme gosterimi
- ? Piyasa arastirma - aktif ilan filtreleme
- ? Profesyonel login sayfasi
- ? Kullanici yonetimi
- ? Standart hesap plani otomatik yukleme

## ?? Katkida Bulunma

1. Fork yapin
2. Feature branch olusturun (`git checkout -b feature/amazing-feature`)
3. Commit yapin (`git commit -m 'Add some amazing feature'`)
4. Push yapin (`git push origin feature/amazing-feature`)
5. Pull Request acin

## ?? Ilgili Projeler

| Proje | Aciklama | Link |
|-------|----------|------|
| CRM Filo Servis | Filo Yonetimi, Muhasebe ve CRM Uygulamasi | [GitHub](https://github.com/karamur/CRMFiloServis) |
| AYDA Analiz | Mali Analiz ve Muhasebe Uygulamasi | [GitHub](https://github.com/karamur/AYDAAnaliz) |

## ?? Iletisim

Sorulariniz icin: [GitHub Issues](https://github.com/karamur/CRMFiloServis/issues)

## ?? Lisans

Bu proje MIT lisansi altinda lisanslanmistir - detaylar icin [LICENSE](LICENSE) dosyasina bakin.

---

**© 2025 Allbatros Global Teknoloji - Tum Haklari Saklidir**
