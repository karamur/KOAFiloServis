$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "CRMFiloServis.Installer\CRMFiloServis.Installer.csproj"
$outputDir = Join-Path $root "artifacts\installer"

Write-Host "Kurulum programi olusturuluyor..." -ForegroundColor Cyan

if (Test-Path $outputDir) { Remove-Item $outputDir -Recurse -Force }
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

dotnet publish $project -c Release -r win-x64 --self-contained true -o $outputDir

Write-Host "Tamamlandi: $outputDir\CRMFiloServisKurulum.exe" -ForegroundColor Green
