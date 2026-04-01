$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "CRMFiloServis.LisansDesktop\CRMFiloServis.LisansDesktop.csproj"
$outputDir = Join-Path $root "artifacts\lisans"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  CRM Filo Servis - Lisans Olusturucu" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "[1/2] Onceki build temizleniyor..." -ForegroundColor Gray
if (Test-Path $outputDir) { Remove-Item $outputDir -Recurse -Force }
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

Write-Host "[2/2] Proje derleniyor..." -ForegroundColor Gray
dotnet publish $project -c Release -r win-x64 --self-contained true -o $outputDir

if ($LASTEXITCODE -ne 0) {
    Write-Host "HATA: Build basarisiz!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  BASARILI!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "Klasor: $outputDir" -ForegroundColor White
Write-Host ""
