# CRM Filo Servis - Başlatma Betiği

$ErrorActionPreference = "Stop"

$appPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$exePath = Join-Path $appPath "CRMFiloServis.Web.exe"
$dllPath = Join-Path $appPath "CRMFiloServis.Web.dll"

Write-Host "CRM Filo Servis baslatiliyor..." -ForegroundColor Cyan

# Lisans kontrolü
$licensePath = Join-Path $appPath "license.key"
if (-not (Test-Path $licensePath)) {
    Write-Host "UYARI: Lisans dosyasi bulunamadi!" -ForegroundColor Yellow
    Write-Host "Lutfen license.key dosyasini $appPath dizinine kopyalayin." -ForegroundColor Yellow
    Write-Host ""
}

# Çalışma dizinini ayarla
Set-Location $appPath

# Ortam değişkenlerini ayarla
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ASPNETCORE_URLS = "http://localhost:5190"

# Uygulamayı başlat
if (Test-Path $exePath) {
    Write-Host "Uygulama baslatiliyor: $exePath" -ForegroundColor Green
    Write-Host "URL: http://localhost:5190" -ForegroundColor Green
    Write-Host ""
    Write-Host "Durdurmak icin Ctrl+C tuslarina basin." -ForegroundColor Yellow
    Write-Host ""
    & $exePath
} elseif (Test-Path $dllPath) {
    Write-Host "Uygulama baslatiliyor: dotnet $dllPath" -ForegroundColor Green
    Write-Host "URL: http://localhost:5190" -ForegroundColor Green
    Write-Host ""
    Write-Host "Durdurmak icin Ctrl+C tuslarina basin." -ForegroundColor Yellow
    Write-Host ""
    dotnet $dllPath
} else {
    Write-Host "HATA: Uygulama dosyasi bulunamadi!" -ForegroundColor Red
    exit 1
}
