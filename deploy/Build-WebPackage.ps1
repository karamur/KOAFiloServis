param(
    [string]$Version = ""
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "CRMFiloServis.Web\CRMFiloServis.Web.csproj"
$artifactsRoot = "D:\calisma\Claude-Code\CRMFiloServis\artifacts"
$outputDir = Join-Path $artifactsRoot "web"
$publishDir = Join-Path $outputDir "publish"

# Versiyon belirleme
if ([string]::IsNullOrEmpty($Version)) {
    $today = Get-Date
    $Version = "$($today.Year).$($today.Month.ToString('00')).$($today.Day.ToString('00'))"
}

$packageFile = Join-Path $outputDir "CRMFiloServis.Web-$Version.zip"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Koa Filo Servis - Web Paketi" -ForegroundColor Cyan
Write-Host "  Versiyon: $Version" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "[1/5] Onceki build temizleniyor..." -ForegroundColor Gray
if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

Write-Host "[2/5] Proje derleniyor..." -ForegroundColor Gray
dotnet publish $project -c Release -o $publishDir

if ($LASTEXITCODE -ne 0) {
    Write-Host "HATA: Build basarisiz!" -ForegroundColor Red
    exit 1
}

Write-Host "[3/5] Versiyon dosyasi olusturuluyor..." -ForegroundColor Gray
$manifest = @{
    product = "CRMFiloServis.Web"
    version = $Version
    buildDate = (Get-Date -Format "yyyy-MM-dd HH:mm:ss")
    buildMachine = $env:COMPUTERNAME
} | ConvertTo-Json -Depth 2

Set-Content -Path (Join-Path $publishDir "version.json") -Value $manifest -Encoding UTF8

Write-Host "[4/5] Production config ekleniyor..." -ForegroundColor Gray
$prodExample = Join-Path $root "CRMFiloServis.Web\appsettings.Production.json.example"
if (Test-Path $prodExample) {
    Copy-Item $prodExample (Join-Path $publishDir "appsettings.Production.json.example") -Force
}

Write-Host "[5/5] ZIP paketi olusturuluyor..." -ForegroundColor Gray
if (Test-Path $packageFile) { Remove-Item $packageFile -Force }
Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $packageFile -Force

$fileSize = (Get-Item $packageFile).Length / 1MB

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  BASARILI!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "Paket: $packageFile" -ForegroundColor White
Write-Host "Boyut: $([math]::Round($fileSize, 2)) MB" -ForegroundColor White
Write-Host "Versiyon: $Version" -ForegroundColor Yellow
Write-Host ""
