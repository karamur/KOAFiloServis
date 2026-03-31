$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "CRMFiloServis.LisansDesktop\CRMFiloServis.LisansDesktop.csproj"
$outputDir = Join-Path $root "artifacts\lisans"

Write-Host "Lisans olusturucu programi olusturuluyor..." -ForegroundColor Cyan

if (Test-Path $outputDir) { Remove-Item $outputDir -Recurse -Force }
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

dotnet publish $project -c Release -r win-x64 --self-contained true -o $outputDir

Write-Host "Tamamlandi: $outputDir\CRMFiloServisLisans.exe" -ForegroundColor Green
