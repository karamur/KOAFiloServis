# CRM Filo Servis - Windows Servisi Kurulum Betiği
# Yönetici yetkisi gerektirir

#Requires -RunAsAdministrator

$ErrorActionPreference = "Stop"

$serviceName = "CRMFiloServis"
$appPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$exePath = Join-Path $appPath "CRMFiloServis.Web.exe"

Write-Host "======================================" -ForegroundColor Cyan
Write-Host " Windows Servisi Kurulumu" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Exe dosyası kontrolü
if (-not (Test-Path $exePath)) {
    Write-Host "HATA: $exePath bulunamadi!" -ForegroundColor Red
    Write-Host "Oncelikle install.ps1 betigini calistirin." -ForegroundColor Yellow
    exit 1
}

# Mevcut servisi kontrol et
$existingService = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
if ($existingService) {
    Write-Host "Mevcut servis durduruluyor..." -ForegroundColor Yellow
    Stop-Service -Name $serviceName -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
    
    Write-Host "Mevcut servis kaldiriliyor..." -ForegroundColor Yellow
    sc.exe delete $serviceName | Out-Null
    Start-Sleep -Seconds 2
}

# Yeni servis oluştur
Write-Host "Yeni servis olusturuluyor..." -ForegroundColor Yellow

$params = @{
    Name = $serviceName
    BinaryPathName = "`"$exePath`" --contentRoot `"$appPath`""
    DisplayName = "CRM Filo Servis"
    Description = "CRM Filo Servis - Filo Yonetim ve CRM Uygulamasi"
    StartupType = "Automatic"
}

New-Service @params | Out-Null

# Servis recovery ayarları
$actions = "restart/60000/restart/60000/restart/60000"
sc.exe failure $serviceName reset= 86400 actions= $actions | Out-Null

# Servisi başlat
Write-Host "Servis baslatiliyor..." -ForegroundColor Yellow
Start-Service -Name $serviceName

# Durum kontrolü
$service = Get-Service -Name $serviceName
Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host " Kurulum Tamamlandi!" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Servis Adi: $serviceName" -ForegroundColor White
Write-Host "Durum: $($service.Status)" -ForegroundColor White
Write-Host ""
Write-Host "Yonetim komutlari:" -ForegroundColor Yellow
Write-Host "  Start-Service $serviceName   # Baslat"
Write-Host "  Stop-Service $serviceName    # Durdur"
Write-Host "  Restart-Service $serviceName # Yeniden baslat"
Write-Host ""
Write-Host "Uygulama URL: http://localhost:5190" -ForegroundColor Green
Write-Host ""
