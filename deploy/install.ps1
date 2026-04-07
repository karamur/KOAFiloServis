# ============================================================
# CRM Filo Servis - Kurulum Scripti
# Windows PowerShell icin
# ============================================================

#Requires -RunAsAdministrator

param(
    [string]$InstallPath = "C:\KOAFiloServis",
    [switch]$InstallService = $false,
    [switch]$OpenFirewall = $true
)

$ErrorActionPreference = "Stop"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "   CRM Filo Servis - Kurulum Scripti" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# .NET Runtime kontrolu
Write-Host "[1/6] .NET Runtime kontrol ediliyor..." -ForegroundColor Yellow
$dotnetVersion = $null
try {
    $dotnetVersion = dotnet --list-runtimes | Where-Object { $_ -match "Microsoft.AspNetCore.App 10" }
}
catch {
    $dotnetVersion = $null
}

if (-not $dotnetVersion) {
    Write-Host "  ! .NET 10 Runtime bulunamadi!" -ForegroundColor Red
    Write-Host "  Lutfen asagidaki linkten .NET 10 Runtime indirip kurun:" -ForegroundColor Yellow
    Write-Host "  https://dotnet.microsoft.com/download/dotnet/10.0" -ForegroundColor Cyan
    Write-Host ""
    
    $response = Read-Host "Kurulum devam etsin mi? (E/H)"
    if ($response -ne "E" -and $response -ne "e") {
        Write-Host "Kurulum iptal edildi." -ForegroundColor Red
        exit 1
    }
}
else {
    Write-Host "  OK - .NET 10 Runtime mevcut" -ForegroundColor Green
}

# Kurulum klasoru olustur
Write-Host "[2/6] Kurulum klasoru hazirlaniyor..." -ForegroundColor Yellow
if (-not (Test-Path $InstallPath)) {
    New-Item -ItemType Directory -Path $InstallPath -Force | Out-Null
    Write-Host "  OK - Klasor olusturuldu: $InstallPath" -ForegroundColor Green
}
else {
    Write-Host "  OK - Klasor mevcut: $InstallPath" -ForegroundColor Green
}

# Dosyalari kopyala
Write-Host "[3/6] Dosyalar kopyalaniyor..." -ForegroundColor Yellow
$sourceDir = Split-Path -Parent $PSScriptRoot
if (Test-Path "$sourceDir\CRMFiloServis.Web.exe") {
    $sourceDir = $PSScriptRoot
}
Copy-Item -Path "$sourceDir\*" -Destination $InstallPath -Recurse -Force -Exclude @("*.pdb", "*.Development.json")
Write-Host "  OK - Dosyalar kopyalandi" -ForegroundColor Green

# Gerekli klasorleri olustur
Write-Host "[4/6] Gerekli klasorler olusturuluyor..." -ForegroundColor Yellow
$storageRoot = "C:\KOAFiloServis_yedekleme"
$folders = @(
    $storageRoot,
    (Join-Path $storageRoot "database"),
    (Join-Path $storageRoot "uploads"),
    (Join-Path $storageRoot "pdf"),
    (Join-Path $storageRoot "keys"),
    (Join-Path $storageRoot "logs"),
    (Join-Path $InstallPath "Temp")
)
foreach ($folder in $folders) {
    if (-not (Test-Path $folder)) {
        New-Item -ItemType Directory -Path $folder -Force | Out-Null
    }
}
Write-Host "  OK - Yedekleme koku hazirlandi: $storageRoot" -ForegroundColor Green

$sqlitePath = Join-Path $InstallPath "CRMFiloServis.db"
if (-not (Test-Path $sqlitePath)) {
    New-Item -ItemType File -Path $sqlitePath -Force | Out-Null
}

$prodJson = @'
{
  "DatabaseProvider": "SQLite",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=CRMFiloServis.db;"
  },
  "PythonScraper": {
    "BaseUrl": "http://localhost:5050",
    "Enabled": false
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Warning",
      "Microsoft.EntityFrameworkCore.Infrastructure": "Warning",
      "System.Net.Http.HttpClient": "Warning"
    }
  },
  "AllowedHosts": "*"
}
'@

$dbSettingsJson = @'
{
  "Id": 0,
  "Provider": 1,
  "Host": "",
  "Port": 0,
  "DatabaseName": "CRMFiloServis.db",
  "Username": "",
  "Password": "",
  "UseIntegratedSecurity": false,
  "AdditionalOptions": null
}
'@

$backupSettingsJson = @'
{
  "AutoBackupEnabled": true,
  "AutoBackupIntervalHours": 24,
  "KeepBackupCount": 10,
  "BackupFolder": "database",
  "LastBackupTime": null
}
'@

Set-Content -Path (Join-Path $InstallPath "appsettings.Production.json") -Value $prodJson -Encoding UTF8
Set-Content -Path (Join-Path $InstallPath "dbsettings.json") -Value $dbSettingsJson -Encoding UTF8
Set-Content -Path (Join-Path $InstallPath "backup_settings.json") -Value $backupSettingsJson -Encoding UTF8
Write-Host "  OK - SQLite ve depolama ayarlari yazildi" -ForegroundColor Green

# Firewall kurali
if ($OpenFirewall) {
    Write-Host "[5/6] Firewall kurali ekleniyor..." -ForegroundColor Yellow
    try {
        $existingRule = Get-NetFirewallRule -DisplayName "CRMFiloServis" -ErrorAction SilentlyContinue
        if (-not $existingRule) {
            New-NetFirewallRule -DisplayName "CRMFiloServis" `
                -Direction Inbound `
                -Protocol TCP `
                -LocalPort 5190 `
                -Action Allow `
                -Profile Any | Out-Null
            Write-Host "  OK - Firewall kurali eklendi (Port 5190)" -ForegroundColor Green
        }
        else {
            Write-Host "  OK - Firewall kurali zaten mevcut" -ForegroundColor Green
        }
    }
    catch {
        Write-Host "  ! Firewall kurali eklenemedi: $_" -ForegroundColor Yellow
    }
}
else {
    Write-Host "[5/6] Firewall kurali atlandi" -ForegroundColor Gray
}

# Windows Servisi
if ($InstallService) {
    Write-Host "[6/6] Windows Servisi kuruluyor..." -ForegroundColor Yellow
    try {
        $serviceName = "CRMFiloServis"
        $existingService = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
        
        if ($existingService) {
            Stop-Service -Name $serviceName -Force -ErrorAction SilentlyContinue
            sc.exe delete $serviceName | Out-Null
            Start-Sleep -Seconds 2
        }
        
        $exePath = Join-Path $InstallPath "CRMFiloServis.Web.exe"
        New-Service -Name $serviceName `
            -BinaryPathName $exePath `
            -DisplayName "CRM Filo Servis" `
            -Description "Filo Yonetimi, Muhasebe ve CRM Uygulamasi" `
            -StartupType Automatic | Out-Null
        
        Start-Service -Name $serviceName
        Write-Host "  OK - Servis kuruldu ve baslatildi" -ForegroundColor Green
    }
    catch {
        Write-Host "  ! Servis kurulamadi: $_" -ForegroundColor Red
    }
}
else {
    Write-Host "[6/6] Windows Servisi atlandi (Manuel baslat icin)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "   KURULUM TAMAMLANDI!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""
Write-Host "Kurulum Klasoru: $InstallPath" -ForegroundColor White
Write-Host ""
Write-Host "UYGULAMAYI BASLATMAK ICIN:" -ForegroundColor Yellow
Write-Host "  cd $InstallPath" -ForegroundColor Cyan
Write-Host "  .\CRMFiloServis.Web.exe" -ForegroundColor Cyan
Write-Host ""
Write-Host "TARAYICIDA ACMAK ICIN:" -ForegroundColor Yellow
Write-Host "  http://localhost:5190" -ForegroundColor Cyan
Write-Host ""
Write-Host "VARSAYILAN GIRIS:" -ForegroundColor Yellow
Write-Host "  Kullanici: admin" -ForegroundColor White
Write-Host "  Sifre: Admin123!" -ForegroundColor White
Write-Host ""
Write-Host "ONEMLI: Ilk giristen sonra sifrenizi degistirin!" -ForegroundColor Red
Write-Host ""
