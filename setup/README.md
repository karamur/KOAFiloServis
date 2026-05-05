# KOAFiloServis Kurulum Paketi (Inno Setup)

Tek komutla tam kurulum paketi üretir. Çıktı, **çalıştırılabilir `.exe` setup dosyası** olarak verilir ve hedef makinede **IIS'i otomatik yapılandırır** (isteğe bağlı olarak IIS rolünü ve .NET 10 Hosting Bundle'ı da kurar).

## Gereksinimler (build makinesi)

- **.NET 10 SDK** — `winget install Microsoft.DotNet.SDK.10`
- **Inno Setup 6** — `winget install JRSoftware.InnoSetup`

## Hızlı Kullanım

### Seçenek 1 — Çift tıklayarak (önerilen)

```text
setup\make.cmd 1.0.3
```

`make.cmd` çift tıklandığında:
1. Otomatik admin'e yükselir
2. `.NET SDK` ve `ISCC.exe` varlığını kontrol eder
3. `build.ps1` ile publish + Inno Setup paketleme yapar
4. Çıktı klasörünü Explorer'da açar

### Seçenek 2 — PowerShell

```powershell
cd setup
.\build.ps1 -Version 1.0.3 -CopyToPublish
```

## Üretilen Çıktılar

`setup\output\v<Version>\` altında:

| Dosya | Açıklama |
|---|---|
| `KOAFiloServisKurulum-<v>.exe` | **Tam kurulum** — Web (IIS) + Lisans + DataSync |
| `KOAFiloServisGuncelle-<v>.exe` | Güncelleme paketi (mevcut kurulumun üstüne) |
| `KOAFiloServisKurulumMusteri-<v>.exe` | Müşteri dağıtım paketi |
| `KOALisansArac-<v>.exe` | Bağımsız Lisans Yönetim Aracı |

`-CopyToPublish` parametresi verilirse `F:\publish\Installer\` altına da kopyalanır.

## Yapı

```text
setup\
├── make.cmd                       Çift tıkla EXE üret (admin'e yükseltir)
├── build.ps1                      Publish + Inno Setup pipeline'ı
├── Setup.iss                      Ana kurulum (Web + Lisans + DataSync)
├── GuncelleSetup.iss              Güncelleme paketi
├── MusteriSetup.iss               Müşteri paketi
├── LisansSetup.iss                Sadece Lisans aracı
├── scripts\
│   ├── iis-install-features.ps1   IIS rolü + Hosting Bundle otomatik kurulum
│   ├── iis-configure.ps1          IIS site + AppPool + ACL (idempotent)
│   ├── iis-remove.ps1             Uninstall temizliği
│   ├── preinstall-check.ps1       IIS + Hosting Bundle ön kontrolü
│   └── backup-db.ps1              SQLite veritabanı yedekleme
├── assets\                        İkonlar, wizard görselleri
├── payload\                       dotnet publish çıktıları (otomatik)
└── output\v<Version>\             Inno Setup EXE'leri (otomatik)
```

## Kurulum Akışı (Hedef PC'de)

`KOAFiloServisKurulum-<v>.exe` çift tıklandığında sihirbaz şu adımları sunar:

1. **Ön Hazırlık** *(opsiyonel)*
   - ☐ **IIS rolu/Hosting Bundle eksikse otomatik kur** (Internet gerekir)
2. **Bileşenler**
   - ☑ KOAFiloServis Web (IIS) — *zorunlu*
   - ☑ Lisans Yönetim Aracı
   - ☑ Veri Aktarım Aracı (PostgreSQL → SQLite)
3. **IIS**
   - ☑ IIS Site ve AppPool'u otomatik yapılandır
4. **Firewall**
   - ☑ Windows Güvenlik Duvarında 5190/TCP aç
5. **Son adım**
   - ☐ Tarayıcıda aç (`http://localhost:5190`)

> Kurulum başlamadan önce `preinstall-check.ps1` otomatik çalışır. IIS veya Hosting Bundle eksikse uyarı çıkar; "On Hazırlık" görevini işaretleyerek otomatik kurulum yaptırabilirsiniz.

## IIS Otomatik Kurulum Detayları

`scripts\iis-install-features.ps1` betiği:

- Windows Server: `Install-WindowsFeature` ile IIS rolü ve gerekli alt özellikler
- Windows 10/11: `Enable-WindowsOptionalFeature` ile IIS özellikleri
- ASP.NET Core 10 Hosting Bundle yoksa `https://aka.ms/dotnet/10.0/dotnet-hosting-win.exe` adresinden indirir ve sessiz kurar
- Çıkış kodları: `0` OK, `1` IIS kurulamadı, `2` Hosting Bundle indirilemedi

Bağımsız çalıştırmak için (admin):
```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:\KOAFiloServis\scripts\iis-install-features.ps1
```

## Kurulum Sonrası Dizin

Hedef PC'de:

```text
C:\KOAFiloServis\
├── KOAFiloServis.Web.exe
├── wwwroot\...
├── web.config              ← AspNetCoreModuleV2 + stdoutLogEnabled=true
├── dbsettings.json         ← Sadece ilk kurulumda yazılır, güncellemede korunur
├── data\                   ← SQLite DB (kullanıcı verisi — KORUNUR)
├── uploads\                ← Kullanıcı dosyaları (KORUNUR)
├── logs\                   ← stdout logları
├── Backups\                ← Otomatik DB yedekleri
├── Lisans\                 ← Lisans aracı
├── DataSync\               ← Veri aktarım aracı
├── scripts\                ← IIS / preinstall betikleri
└── active-port.txt         ← Çalışan port (5190 dolu ise alternatif)
```

## Güncelleme

Aynı EXE çift tıklanır → Inno Setup mevcut kurulumu tespit eder → otomatik onay ister.

- **Korunur:** `dbsettings.json`, `appsettings.Production.json`, `data\*`, `uploads\`, `logs\`, `Backups\`
- **Yenilenir:** `KOAFiloServis.Web.exe`, `wwwroot\`, DLL'ler, `scripts\`
- **Otomatik:** Yedek alınır (`Backups\db-<tarih>\`), IIS site durdurulur, dosyalar değiştirilir, IIS site başlatılır.

## Kaldırma

Denetim Masası → Programlar → KOAFiloServis → Kaldır
- IIS site + AppPool kaldırılır (`iis-remove.ps1`)
- Firewall kuralı kaldırılır
- Kullanıcı verileri (uploads, logs, db, Backups) **kasıtlı olarak korunur** — manuel silinmelidir

## Veri Aktarımı (PostgreSQL → SQLite)

Kurulum sonrası Start Menu'den "Veri Aktarim" kısayoluna tıklayın, veya:

```powershell
C:\KOAFiloServis\DataSync\KOAFiloServis.DataSync.exe export `
  --source "Host=DEV-PC;Port=5432;Database=DestekCRMServisBlazorDb;Username=postgres;Password=xxx" `
  --target "C:\KOAFiloServis\data\koa.db"
```

## Sorun Giderme

| Belirti | Çözüm |
|---|---|
| Setup "Hosting Bundle bulunamadı" diyor | "On Hazırlık" görevini işaretleyin veya https://aka.ms/dotnet/10.0/dotnet-hosting-win.exe adresinden indirip kurun, sonra `iisreset` |
| `http://localhost:5190` açılmıyor | `C:\KOAFiloServis\logs\stdout*.log` son satırlara bakın; `active-port.txt` farklı bir port göstermiş olabilir |
| 5190 portu dolu | `iis-configure.ps1` otomatik 5191/5192'yi dener; `active-port.txt` içine yazar |
| `502.5` hatası | Hosting Bundle eksik veya AppPool .NET CLR sürümü yanlış (`No Managed Code` olmalı) |
| AppPool 503 / dosya kilidi | Önce `iisreset /noforce`, sonra setup'i tekrar çalıştırın |

