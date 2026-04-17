# KOAFiloServis Kurulum Paketi (Inno Setup)

Tek komutla tam kurulum paketi üretir.

## Gereksinimler

- **.NET 10 SDK** (publish için)
- **Inno Setup 6** — `winget install JRSoftware.InnoSetup`

## Tek Komut

```powershell
cd setup
.\build.ps1 -Version 1.0.3 -CopyToPublish
```

Çıktı: `setup\output\KOAFiloServisKurulum-1.0.3.exe` (~250 MB)

Ayrıca `-CopyToPublish` verirseniz `F:\publish\Installer\` altına kopyalanır.

## Yapısı

```
setup\
├── Setup.iss              Inno Setup script (ana mantık)
├── build.ps1              Tam üretim pipeline'ı
├── scripts\
│   ├── iis-configure.ps1  IIS site + AppPool + ACL (idempotent)
│   ├── iis-remove.ps1     Uninstall temizliği
│   └── preinstall-check.ps1  IIS + Hosting Bundle kontrol
├── assets\                İkonlar, wizard görselleri
├── payload\               dotnet publish çıktıları (otomatik)
└── output\                Inno Setup EXE'si (otomatik)
```

## Kurulum Sonrası Dizin

Hedef PC'de:

```
C:\KOAFiloServis\
├── KOAFiloServis.Web.exe
├── wwwroot\...
├── dbsettings.json         ← Sadece ilk kurulumda yazılır, güncellemede korunur
├── data\                   ← SQLite DB
├── uploads\                ← Kullanıcı dosyaları (PDF vs.)
├── logs\                   ← Loglar
├── Backups\                ← Yedekler
├── Lisans\                 ← Lisans aracı
├── DataSync\               ← Veri aktarım aracı
└── scripts\                ← IIS betikleri
```

## Güncelleme

Aynı EXE çift tıklanır → Inno Setup mevcut kurulumu tespit eder →
**`dbsettings.json`, `data\*.db`, `uploads\`, `logs\`, `Backups\`** DOKUNULMAZ, diğer dosyalar yenilenir.

## Kaldırma

Denetim Masası → Programlar → KOAFiloServis → Kaldır.
Kullanıcı verileri (uploads, logs, db, Backups) elle silinmelidir — kasıtlı olarak KORUNUR.

## Veri Aktarımı (PostgreSQL → SQLite)

Kurulum sonrası Start Menu'den "Veri Aktarim" kısayoluna tıklayın, veya:

```powershell
C:\KOAFiloServis\DataSync\KOAFiloServis.DataSync.exe export `
  --source "Host=DEV-PC;Port=5432;Database=DestekCRMServisBlazorDb;Username=postgres;Password=xxx" `
  --target "C:\KOAFiloServis\data\koa.db"
```
