# KOAFiloServis Setup

Bu klasör, uygulamanın publish edilip kurulum / güncelleme paketine hazırlanması için
kullanılan scriptleri içerir.

## Hızlı Kullanım (Önerilen — versiyonlu setup)

### PowerShell
```powershell
pwsh .\build-setup.ps1                       # varsayılan: v1.0.21, Update
pwsh .\build-setup.ps1 -Version 1.0.21 -Mode Install
```

### Batch
```bat
build-setup.bat              :: v1.0.21, Update
build-setup.bat 1.0.21 Install
```

## `build-setup.ps1` Parametreleri

| Parametre | Varsayılan | Açıklama |
|-----------|-----------|----------|
| `Version` | `1.0.21` | Paket sürüm numarası. `VERSION.txt`'e yazılır, `AssemblyVersion`/`FileVersion`/`InformationalVersion` olarak publish'e geçilir. |
| `Configuration` | `Release` | dotnet publish configuration. |
| `Runtime` | `win-x64` | RID. |
| `Mode` | `Update` | `Install` veya `Update`. `MODE.txt`'e yazılır; `kur.ps1` ilk kurulumda `dbsettings.json`'ı sıfırlar. |
| `OutputRoot` | `./artifacts/setup` | Çıktı kök klasörü. |
| `-SkipPublish` | — | Mevcut publish çıktısını tekrar üretmeden paketle. |
| `-SkipZip` | — | ZIP arşivini oluşturma. |

## Çıktılar

```
artifacts/setup/
├── publish/                                 # dotnet publish çıktısı
├── KOAFiloServis-1.0.21/                    # paket klasörü
│   ├── VERSION.txt                          # "1.0.21"
│   ├── MODE.txt                             # "Update" / "Install"
│   ├── README.txt
│   ├── kur.ps1                              # IIS kurulum scripti
│   ├── kur.bat
│   └── (tüm publish dosyaları)
└── KOAFiloServis-Setup-1.0.21.zip           # dağıtılabilir paket
```

## Sunucuda Kurulum / Güncelleme

ZIP dosyasını sunucuya kopyalayın, açın ve yönetici PowerShell ile:

```powershell
# Güncelleme (varsayılan)
pwsh .\kur.ps1

# İlk kurulum (dbsettings.json paket içindekiyle değiştirilir, SQLite varsa sıfırlanır)
pwsh .\kur.ps1 -Mode Install

# Özel hedef
pwsh .\kur.ps1 -TargetDir 'C:\KOAFiloServis\IIS' -BackupRoot 'C:\KOAFiloServis_yedekleme\deploy'
```

`kur.ps1` her güncellemede mevcut yayın klasörünü ve veritabanını (PostgreSQL: `pg_dump`,
SQLite: dosya kopyası) `BackupRoot\<timestamp>` ve `BackupRoot\latest` altına yedekler.

## Eski (legacy) script

`setup.ps1` / `setup.bat` — sadece publish + tek `package/` klasörü üretir, sürümleme yapmaz.
Yeni paketler için `build-setup.ps1` kullanın.
