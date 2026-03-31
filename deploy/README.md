# Deploy - Yayın Oluşturma

Bu klasör uygulama yayın paketlerini oluşturmak için kullanılır.

## Web Uygulaması Paketi Oluştur

```powershell
.\Build-WebPackage.ps1 -Version "1.0.0"
```

## Kurulum Programı Oluştur

```powershell
.\Build-Installer.ps1
```

## Tüm Paketleri Oluştur

```powershell
.\Build-All.ps1 -Version "1.0.0"
```

## Çıktılar

- `artifacts\web\` - Web uygulama paketi (.zip)
- `artifacts\installer\` - Kurulum programı (.exe)
- `artifacts\lisans\` - Lisans oluşturucu (.exe)
