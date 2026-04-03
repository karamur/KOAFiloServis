$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "CRMFiloServis.Installer\CRMFiloServis.Installer.csproj"
$artifactsRoot = "D:\calisma\Claude-Code\CRMFiloServis\artifacts"
$outputDir = Join-Path $artifactsRoot "installer"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Koa Filo Servis - Kurulum Programi" -ForegroundColor Cyan
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

$exeFile = Join-Path $outputDir "CRMFiloServisKurulum.exe"
$fileSize = (Get-Item $exeFile).Length / 1MB

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  BASARILI!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "Dosya: $exeFile" -ForegroundColor White
Write-Host "Boyut: $([math]::Round($fileSize, 2)) MB" -ForegroundColor White
Write-Host ""
