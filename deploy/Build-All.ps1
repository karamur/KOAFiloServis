param(
    [string]$Version = ""
)

$ErrorActionPreference = 'Stop'
$artifactsRoot = 'D:\calisma\Claude-Code\CRMFiloServis\artifacts'

# Versiyon belirleme
if ([string]::IsNullOrEmpty($Version)) {
    $today = Get-Date
    $Version = "$($today.Year).$($today.Month.ToString('00')).$($today.Day.ToString('00'))"
}

Write-Host ""
Write-Host "###############################################" -ForegroundColor Magenta
Write-Host "#                                             #" -ForegroundColor Magenta
Write-Host "#     KOA FILO SERVIS - TUM PAKETLER         #" -ForegroundColor Magenta
Write-Host "#     Versiyon: $Version                      " -ForegroundColor Yellow
Write-Host "#                                             #" -ForegroundColor Magenta
Write-Host "###############################################" -ForegroundColor Magenta
Write-Host ""

$startTime = Get-Date

# 1. Web paketi
Write-Host ">>> [1/4] Web Paketi olusturuluyor..." -ForegroundColor Yellow
Write-Host ""
& (Join-Path $PSScriptRoot "Build-WebPackage.ps1") -Version $Version
Write-Host ""

# 2. Musteri paketi
Write-Host ">>> [2/4] Musteri Paketi olusturuluyor..." -ForegroundColor Yellow
Write-Host ""
& (Join-Path $PSScriptRoot "Build-CustomerPackage.ps1") -Version $Version -OutputDir $artifactsRoot
Write-Host ""

# 3. Kurulum programi
Write-Host ">>> [3/4] Kurulum Programi olusturuluyor..." -ForegroundColor Yellow
Write-Host ""
& (Join-Path $PSScriptRoot "Build-Installer.ps1")
Write-Host ""

# 4. Lisans olusturucu
Write-Host ">>> [4/4] Lisans Olusturucu olusturuluyor..." -ForegroundColor Yellow
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
Write-Host "Artefact Koku: $artifactsRoot" -ForegroundColor White
Write-Host "Ciktilar:" -ForegroundColor White
Write-Host "  - $artifactsRoot\web\CRMFiloServis.Web-$Version.zip" -ForegroundColor Gray
Write-Host "  - $artifactsRoot\customer\CRMFiloServis-v$Version.zip" -ForegroundColor Gray
Write-Host "  - $artifactsRoot\installer\CRMFiloServisKurulum.exe" -ForegroundColor Gray
Write-Host "  - $artifactsRoot\lisans\" -ForegroundColor Gray
Write-Host ""
