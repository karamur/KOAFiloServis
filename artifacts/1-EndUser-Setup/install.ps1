# CRM Filo Servis - Windows Kurulum Betiği
# PowerShell 5.1+ gerektirir

param(
    [string]$InstallPath = "C:\CRMFiloServis",
    [string]$DatabaseProvider = "SQLite",
    [switch]$InstallAsService
)

$ErrorActionPreference = "Stop"

Write-Host "======================================" -ForegroundColor Cyan
Write-Host " CRM Filo Servis Kurulum Sihirbazi" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# .NET Runtime kontrolü
Write-Host "[1/5] .NET Runtime kontrolu yapiliyor..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version 2>$null
if (-not $dotnetVersion -or $dotnetVersion -notlike "10.*") {
    Write-Host "HATA: .NET 10 Runtime bulunamadi!" -ForegroundColor Red
    Write-Host "Lutfen https://dotnet.microsoft.com/download/dotnet/10.0 adresinden indirin." -ForegroundColor Yellow
    exit 1
}
Write-Host "  .NET $dotnetVersion bulundu" -ForegroundColor Green

# Kurulum dizini oluştur
Write-Host "[2/5] Kurulum dizini hazirlaniyor..." -ForegroundColor Yellow
if (-not (Test-Path $InstallPath)) {
    New-Item -ItemType Directory -Path $InstallPath -Force | Out-Null
}
Write-Host "  Dizin: $InstallPath" -ForegroundColor Green

# Dosyaları kopyala
Write-Host "[3/5] Dosyalar kopyalaniyor..." -ForegroundColor Yellow
$sourceDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Copy-Item -Path "$sourceDir\app\*" -Destination $InstallPath -Recurse -Force
Write-Host "  Dosyalar kopyalandi" -ForegroundColor Green

# Alt dizinleri oluştur
Write-Host "[4/5] Dizin yapisi olusturuluyor..." -ForegroundColor Yellow
$subDirs = @("data", "logs", "backups")
foreach ($dir in $subDirs) {
    $path = Join-Path $InstallPath $dir
    if (-not (Test-Path $path)) {
        New-Item -ItemType Directory -Path $path -Force | Out-Null
    }
}
Write-Host "  Alt dizinler olusturuldu" -ForegroundColor Green

# Yapılandırma dosyası
Write-Host "[5/5] Yapilandirma hazirlaniyor..." -ForegroundColor Yellow
$configFile = Join-Path $InstallPath "appsettings.Production.json"
$config = @{
    "ConnectionStrings" = @{
        "DefaultConnection" = if ($DatabaseProvider -eq "PostgreSQL") {
            "Host=localhost;Database=crmfilo;Username=postgres;Password=your_password"
        } else {
            "Data Source=data/crm_filo.db"
        }
    }
    "DatabaseProvider" = $DatabaseProvider
    "Logging" = @{
        "LogLevel" = @{
            "Default" = "Information"
            "Microsoft.AspNetCore" = "Warning"
        }
    }
}
$config | ConvertTo-Json -Depth 4 | Set-Content $configFile -Encoding UTF8
Write-Host "  Yapilandirma dosyasi olusturuldu" -ForegroundColor Green

# Servis kurulumu (opsiyonel)
if ($InstallAsService) {
    Write-Host ""
    Write-Host "Windows Servisi kuruluyor..." -ForegroundColor Yellow
    $serviceName = "CRMFiloServis"
    $exePath = Join-Path $InstallPath "CRMFiloServis.Web.exe"
    
    # Mevcut servisi kaldır
    if (Get-Service -Name $serviceName -ErrorAction SilentlyContinue) {
        Stop-Service -Name $serviceName -Force -ErrorAction SilentlyContinue
        sc.exe delete $serviceName | Out-Null
        Start-Sleep -Seconds 2
    }
    
    # Yeni servis oluştur
    New-Service -Name $serviceName `
                -BinaryPathName $exePath `
                -DisplayName "CRM Filo Servis" `
                -Description "CRM Filo Servis Uygulamasi" `
                -StartupType Automatic | Out-Null
    
    Write-Host "  Servis kuruldu: $serviceName" -ForegroundColor Green
}

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host " Kurulum Tamamlandi!" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Sonraki adimlar:" -ForegroundColor Yellow
Write-Host "  1. license.key dosyasini $InstallPath dizinine kopyalayin"
Write-Host "  2. Uygulamayi baslatin: .\start.ps1"
Write-Host "  3. Tarayicida acin: http://localhost:5190"
Write-Host ""
Write-Host "Varsayilan giris bilgileri:" -ForegroundColor Yellow
Write-Host "  Kullanici: admin"
Write-Host "  Sifre: Admin123!"
Write-Host ""
