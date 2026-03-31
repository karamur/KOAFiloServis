param(
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = 'Stop'

Write-Host "=== TUM PAKETLER OLUSTURULUYOR ===" -ForegroundColor Cyan
Write-Host ""

# Web paketi
Write-Host "1/3 - Web paketi..." -ForegroundColor Yellow
& (Join-Path $PSScriptRoot "Build-WebPackage.ps1") -Version $Version
Write-Host ""

# Kurulum programi
Write-Host "2/3 - Kurulum programi..." -ForegroundColor Yellow
& (Join-Path $PSScriptRoot "Build-Installer.ps1")
Write-Host ""

# Lisans olusturucu
Write-Host "3/3 - Lisans olusturucu..." -ForegroundColor Yellow
& (Join-Path $PSScriptRoot "Build-LisansDesktop.ps1")
Write-Host ""

Write-Host "=== TUM PAKETLER TAMAMLANDI ===" -ForegroundColor Green
Write-Host "Ciktilar: artifacts\" -ForegroundColor Cyan
