param(
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "CRMFiloServis.Web\CRMFiloServis.Web.csproj"
$outputDir = Join-Path $root "artifacts\web"
$publishDir = Join-Path $outputDir "publish"
$packageFile = Join-Path $outputDir "CRMFiloServis.Web-$Version.zip"

Write-Host "Web paketi olusturuluyor... Version=$Version" -ForegroundColor Cyan

if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

dotnet publish $project -c Release -o $publishDir

# Manifest dosyasi olustur
$manifest = @{
    product = "CRMFiloServis.Web"
    version = $Version
    buildDate = (Get-Date -Format "yyyy-MM-dd HH:mm:ss")
} | ConvertTo-Json

Set-Content -Path (Join-Path $publishDir "version.json") -Value $manifest -Encoding UTF8

# Production config ornek
$prodExample = Join-Path $root "CRMFiloServis.Web\appsettings.Production.json.example"
if (Test-Path $prodExample) {
    Copy-Item $prodExample (Join-Path $publishDir "appsettings.Production.json.example") -Force
}

# ZIP olustur
if (Test-Path $packageFile) { Remove-Item $packageFile -Force }
Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $packageFile -Force

Write-Host "Tamamlandi: $packageFile" -ForegroundColor Green
