# ============================================================
# CRM Filo Servis - GitHub Artifacts Build Script
# Müşteri kurulum paketi oluşturur
# ============================================================

param(
    [string]$Version = "1.0.0",
    [string]$OutputDir = "D:\calisma\Claude-Code\CRMFiloServis\artifacts",
    [switch]$CreateZip = $true
)

$ErrorActionPreference = "Stop"
$ProjectRoot = Split-Path -Parent $PSScriptRoot
$CustomerArtifactsDir = Join-Path $OutputDir "customer"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "   CRM Filo Servis - Build Script" -ForegroundColor Cyan
Write-Host "   Versiyon: $Version" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Çıktı klasörünü hazırla
$publishDir = Join-Path $CustomerArtifactsDir "publish"
$packageDir = Join-Path $CustomerArtifactsDir "CRMFiloServis-v$Version"
$zipPath = Join-Path $CustomerArtifactsDir "CRMFiloServis-v$Version.zip"

New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
New-Item -ItemType Directory -Path $CustomerArtifactsDir -Force | Out-Null
if (Test-Path $publishDir) {
    Remove-Item -Path $publishDir -Recurse -Force
}
if (Test-Path $packageDir) {
    Remove-Item -Path $packageDir -Recurse -Force
}
if (Test-Path $zipPath) {
    Remove-Item -Path $zipPath -Force
}
New-Item -ItemType Directory -Path $publishDir -Force | Out-Null
New-Item -ItemType Directory -Path $packageDir -Force | Out-Null

# 1. Projeyi derle
Write-Host "[1/5] Proje derleniyor..." -ForegroundColor Yellow
Push-Location $ProjectRoot

dotnet publish "CRMFiloServis.Web\CRMFiloServis.Web.csproj" `
    -c Release `
    -o $publishDir `
    --self-contained false `
    -p:PublishSingleFile=false `
    -p:DebugType=None `
    -p:DebugSymbols=false

if ($LASTEXITCODE -ne 0) {
    Write-Host "HATA: Derleme başarısız!" -ForegroundColor Red
    Pop-Location
    exit 1
}
Write-Host "  OK - Derleme tamamlandı" -ForegroundColor Green

# 2. Dosyaları pakete kopyala
Write-Host "[2/5] Dosyalar kopyalanıyor..." -ForegroundColor Yellow
Copy-Item -Path "$publishDir\*" -Destination $packageDir -Recurse -Force

# Gereksiz dosyaları temizle
$removePatterns = @("*.pdb", "*.Development.json", "appsettings.json")
foreach ($pattern in $removePatterns) {
    Get-ChildItem -Path $packageDir -Filter $pattern -Recurse | Remove-Item -Force -ErrorAction SilentlyContinue
}
Write-Host "  OK - Dosyalar kopyalandı" -ForegroundColor Green

# 3. Production ayarlarını ana ayar olarak kopyala
Write-Host "[3/5] Ayar dosyaları hazırlanıyor..." -ForegroundColor Yellow

# SQLite için production ayarları
$productionSettings = @{
    DatabaseProvider = "SQLite"
    ConnectionStrings = @{
        DefaultConnection = "Data Source=CRMFiloServis.db"
    }
    OpenAI = @{
        ApiKey = ""
        Model = "gpt-4o-mini"
        BaseUrl = "https://api.openai.com/v1"
    }
    PythonScraper = @{
        BaseUrl = "http://localhost:5050"
        Enabled = $false
    }
    Logging = @{
        LogLevel = @{
            Default = "Warning"
            "Microsoft.AspNetCore" = "Warning"
            "Microsoft.EntityFrameworkCore" = "Warning"
        }
    }
    AllowedHosts = "*"
}

$productionSettings | ConvertTo-Json -Depth 10 | Out-File -FilePath "$packageDir\appsettings.json" -Encoding UTF8
Write-Host "  OK - appsettings.json oluşturuldu (SQLite)" -ForegroundColor Green

# 4. Kurulum dosyalarını ekle
Write-Host "[4/5] Kurulum dosyaları ekleniyor..." -ForegroundColor Yellow

# start.bat
Copy-Item -Path "$ProjectRoot\deploy\start.bat" -Destination $packageDir -Force -ErrorAction SilentlyContinue

# install.ps1
Copy-Item -Path "$ProjectRoot\deploy\install.ps1" -Destination $packageDir -Force -ErrorAction SilentlyContinue

# README
$readmeContent = @"
# CRM Filo Servis v$Version

## Hızlı Başlangıç

### 1. Uygulamayı Başlat
``start.bat`` dosyasını çift tıklayın.

### 2. Tarayıcıda Aç
http://localhost:5190

### 3. Giriş Yap
- Kullanıcı: admin
- Şifre: Admin123!

## Gereksinimler
- Windows 10/11 veya Windows Server 2016+
- .NET 10 Runtime (https://dotnet.microsoft.com/download/dotnet/10.0)

## Dosyalar
- CRMFiloServis.Web.exe : Ana uygulama
- appsettings.json      : Ayarlar
- CRMFiloServis.db      : Veritabanı (otomatik oluşur)
- start.bat             : Başlatıcı

## Yedekleme
Uygulama içinden Ayarlar > Yedekleme menüsünden yedek alabilirsiniz.
Yedekler ``yedekleme\database`` klasörüne kaydedilir.

## Destek
GitHub: https://github.com/karamur/CRMFiloServis
"@
$readmeContent | Out-File -FilePath "$packageDir\BENIOKU.txt" -Encoding UTF8

# Boş klasörleri oluştur
@("yedekleme", "yedekleme\database", "yedekleme\uploads", "yedekleme\keys", "yedekleme\logs") | ForEach-Object {
    $folderPath = Join-Path $packageDir $_
    New-Item -ItemType Directory -Path $folderPath -Force | Out-Null
    # .gitkeep dosyası ekle (boş klasör GitHub'da görünsün)
    "" | Out-File -FilePath "$folderPath\.gitkeep" -Encoding UTF8
}

Write-Host "  OK - Kurulum dosyaları eklendi" -ForegroundColor Green

# 5. ZIP oluştur
if ($CreateZip) {
    Write-Host "[5/5] ZIP paketi oluşturuluyor..." -ForegroundColor Yellow
    Compress-Archive -Path "$packageDir\*" -DestinationPath $zipPath -Force
    Write-Host "  OK - ZIP oluşturuldu: $zipPath" -ForegroundColor Green
}
else {
    Write-Host "[5/5] ZIP oluşturma atlandı" -ForegroundColor Gray
}

Pop-Location

# Özet
Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "   BUILD TAMAMLANDI!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""
Write-Host "Çıktı Klasörü: $CustomerArtifactsDir" -ForegroundColor White
Write-Host "Paket Klasörü: $packageDir" -ForegroundColor White
if ($CreateZip) {
    $zipSize = [math]::Round((Get-Item $zipPath).Length / 1MB, 2)
    Write-Host "ZIP Dosyası:   $zipPath ($zipSize MB)" -ForegroundColor White
}
Write-Host ""
Write-Host "Artefact Konumu:" -ForegroundColor Yellow
Write-Host "  $CustomerArtifactsDir" -ForegroundColor Cyan
Write-Host ""
