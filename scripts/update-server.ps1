# CRM Filo Servis - Sunucu Güncelleme Scripti
# Kullanım: .\update-server.ps1 -ZipPath "C:\path\to\CRMFiloServis_Update_YYYYMMDD.zip"

param(
    [Parameter(Mandatory=$true)]
    [string]$ZipPath,
    
    [string]$AppPath = "C:\inetpub\wwwroot\CRMFiloServis",  # IIS için varsayılan
    [string]$ServiceName = "CRMFiloServis",                 # Windows Service adı (varsa)
    [string]$AppPoolName = "CRMFiloServis",                 # IIS App Pool adı (varsa)
    [switch]$NoBackup
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  CRM Filo Servis - Sunucu Güncellemesi" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ZIP dosyasını kontrol et
if (-not (Test-Path $ZipPath)) {
    Write-Host "HATA: ZIP dosyası bulunamadı: $ZipPath" -ForegroundColor Red
    exit 1
}

Write-Host "[1/6] Güncelleme paketi: $ZipPath" -ForegroundColor Yellow

# Yedekleme
$BackupPath = "$AppPath.backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
if (-not $NoBackup) {
    Write-Host "[2/6] Yedekleme yapılıyor: $BackupPath" -ForegroundColor Yellow
    if (Test-Path $AppPath) {
        Copy-Item -Path $AppPath -Destination $BackupPath -Recurse -Force
        Write-Host "      Yedekleme tamamlandı" -ForegroundColor Green
    }
} else {
    Write-Host "[2/6] Yedekleme atlandı (-NoBackup)" -ForegroundColor Gray
}

# IIS App Pool durdur
$appPoolStopped = $false
try {
    Import-Module WebAdministration -ErrorAction SilentlyContinue
    if (Get-IISAppPool -Name $AppPoolName -ErrorAction SilentlyContinue) {
        Write-Host "[3/6] IIS App Pool durduruluyor: $AppPoolName" -ForegroundColor Yellow
        Stop-WebAppPool -Name $AppPoolName
        $appPoolStopped = $true
        Start-Sleep -Seconds 3
        Write-Host "      App Pool durduruldu" -ForegroundColor Green
    }
} catch {
    Write-Host "[3/6] IIS App Pool bulunamadı veya durdurulamadı" -ForegroundColor Gray
}

# Windows Service durdur
$serviceStopped = $false
try {
    $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if ($service -and $service.Status -eq 'Running') {
        Write-Host "[3/6] Windows Service durduruluyor: $ServiceName" -ForegroundColor Yellow
        Stop-Service -Name $ServiceName -Force
        $serviceStopped = $true
        Start-Sleep -Seconds 3
        Write-Host "      Service durduruldu" -ForegroundColor Green
    }
} catch {
    Write-Host "[3/6] Windows Service bulunamadı" -ForegroundColor Gray
}

# ZIP'i aç
Write-Host "[4/6] Güncelleme paketi açılıyor..." -ForegroundColor Yellow
$tempPath = "$env:TEMP\CRMFiloServis_Update_$(Get-Date -Format 'yyyyMMddHHmmss')"
Expand-Archive -Path $ZipPath -DestinationPath $tempPath -Force
Write-Host "      Paket açıldı: $tempPath" -ForegroundColor Green

# Dosyaları kopyala
Write-Host "[5/6] Dosyalar kopyalanıyor..." -ForegroundColor Yellow
if (-not (Test-Path $AppPath)) {
    New-Item -ItemType Directory -Path $AppPath -Force | Out-Null
}
Copy-Item -Path "$tempPath\*" -Destination $AppPath -Recurse -Force
Write-Host "      Dosyalar kopyalandı" -ForegroundColor Green

# Temp klasörünü temizle
Remove-Item -Path $tempPath -Recurse -Force

# Servisleri başlat
Write-Host "[6/6] Servisler başlatılıyor..." -ForegroundColor Yellow
if ($appPoolStopped) {
    Start-WebAppPool -Name $AppPoolName
    Write-Host "      IIS App Pool başlatıldı" -ForegroundColor Green
}
if ($serviceStopped) {
    Start-Service -Name $ServiceName
    Write-Host "      Windows Service başlatıldı" -ForegroundColor Green
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Güncelleme başarıyla tamamlandı!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Yedek dizini: $BackupPath" -ForegroundColor Cyan
Write-Host ""
