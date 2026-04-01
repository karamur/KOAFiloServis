param(
    [string]$Version = ""
)

$ErrorActionPreference = 'Stop'

# Versiyon belirleme
if ([string]::IsNullOrEmpty($Version)) {
    $today = Get-Date
    $Version = "$($today.Year).$($today.Month.ToString('00')).$($today.Day.ToString('00'))"
}

Write-Host ""
Write-Host "###############################################" -ForegroundColor Magenta
Write-Host "#                                             #" -ForegroundColor Magenta
Write-Host "#     CRM FILO SERVIS - TUM PAKETLER         #" -ForegroundColor Magenta
Write-Host "#     Versiyon: $Version                      " -ForegroundColor Yellow
Write-Host "#                                             #" -ForegroundColor Magenta
Write-Host "###############################################" -ForegroundColor Magenta
Write-Host ""

$startTime = Get-Date

# 1. Web paketi
Write-Host ">>> [1/3] Web Paketi olusturuluyor..." -ForegroundColor Yellow
Write-Host ""
& (Join-Path $PSScriptRoot "Build-WebPackage.ps1") -Version $Version
Write-Host ""

# 2. Kurulum programi
Write-Host ">>> [2/3] Kurulum Programi olusturuluyor..." -ForegroundColor Yellow
Write-Host ""
& (Join-Path $PSScriptRoot "Build-Installer.ps1")
Write-Host ""

# 3. Lisans olusturucu
Write-Host ">>> [3/3] Lisans Olusturucu olusturuluyor..." -ForegroundColor Yellow
Write-Host ""
& (Join-Path $PSScriptRoot "Build-LisansDesktop.ps1")
Write-Host ""

$endTime = Get-Date
$duration = $endTime - $startTime

Write-Host "###############################################" -ForegroundColor Green
Write-Host "#                                             #" -ForegroundColor Green
Write-Host "#     TUM PAKETLER TAMAMLANDI!               #" -ForegroundColor Green
Write-Host "#                                             #" -ForegroundColor Green
Write-Host "###############################################" -ForegroundColor Green
Write-Host ""
Write-Host "Versiyon: $Version" -ForegroundColor Yellow
Write-Host "Sure: $($duration.TotalSeconds.ToString('0.0')) saniye" -ForegroundColor Cyan
Write-Host ""
Write-Host "Ciktilar:" -ForegroundColor White
Write-Host "  - artifacts\web\CRMFiloServis.Web-$Version.zip" -ForegroundColor Gray
Write-Host "  - artifacts\installer\CRMFiloServisKurulum.exe" -ForegroundColor Gray
Write-Host "  - artifacts\lisans\" -ForegroundColor Gray
Write-Host ""
