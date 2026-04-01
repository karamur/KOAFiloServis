# Deploy - Yayın Paketleri

Bu klasör CRM Filo Servis yayın paketlerini oluşturur.

## Hızlı Kullanım

```powershell
# Tüm paketleri oluştur (otomatik versiyon: YYYY.MM.DD)
.\Build-All.ps1

# Belirli versiyon ile
.\Build-All.ps1 -Version "1.2.0"

# Sadece web paketi
.\Build-WebPackage.ps1 -Version "1.2.0"
```

## Scriptler

| Script | Açıklama |
|--------|----------|
| `Build-All.ps1` | Tüm paketleri oluşturur |
| `Build-WebPackage.ps1` | Web uygulama paketi (.zip) |
| `Build-Installer.ps1` | Masaüstü kurulum programı (.exe) |
| `Build-LisansDesktop.ps1` | Lisans oluşturucu programı |

## Çıktılar

```
artifacts/
├── web/
│   ├── CRMFiloServis.Web-2025.04.01.zip    # Web paketi
│   └── publish/                             # Publish klasörü
├── installer/
│   └── CRMFiloServisKurulum.exe            # Kurulum programı
└── lisans/
    └── CRMFiloServisLisans.exe             # Lisans oluşturucu
```

## Versiyon Formatı

Varsayılan format: `YYYY.MM.DD` (örn: 2025.04.01)

Özel versiyon:
```powershell
.\Build-All.ps1 -Version "1.0.0"
.\Build-All.ps1 -Version "2025.04.01-beta"
```

## Kurulum PC'ye Kopyalanacaklar

1. `CRMFiloServisKurulum.exe` - Kurulum programı
2. `CRMFiloServis.Web-X.X.X.zip` - Web paketi

## Güncelleme

Mevcut kuruluma güncelleme için sadece web paketini yükleyin:
1. `Ayarlar > Uygulama Güncelleme` menüsüne gidin
2. Yeni versiyon ZIP dosyasını yükleyin
3. Güncellemeyi başlatın
