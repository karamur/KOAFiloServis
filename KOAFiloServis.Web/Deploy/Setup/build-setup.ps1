<#
.SYNOPSIS
    KOAFiloServis kurulum paketi (setup) oluşturur.

.DESCRIPTION
    Publish çıktısını alır, IIS kurulum scriptleri ile birlikte versiyonlu bir
    paket klasörü ve dağıtılabilir bir ZIP arşivi oluşturur.

    Çıktı yapısı (varsayılan):
        artifacts/setup/publish/                       -> dotnet publish çıktısı
        artifacts/setup/KOAFiloServis-1.0.21/          -> paket klasörü
            VERSION.txt
            MODE.txt
            kur.ps1, kur.bat
            (tüm publish dosyaları)
        artifacts/setup/KOAFiloServis-Setup-1.0.21.zip -> dağıtılabilir paket

.PARAMETER Version
    Paket sürüm numarası. Varsayılan: 1.0.21

.PARAMETER Configuration
    Build configuration. Varsayılan: Release

.PARAMETER Runtime
    .NET runtime identifier. Varsayılan: win-x64

.PARAMETER Mode
    Kurulum modu (Install/Update). MODE.txt içine yazılır. Varsayılan: Update

.PARAMETER OutputRoot
    Çıktı kök klasörü. Varsayılan: ./artifacts/setup

.PARAMETER SkipPublish
    Mevcut publish çıktısını yeniden üretmeden paketle.

.PARAMETER SkipZip
    ZIP arşivi oluşturma adımını atla.

.EXAMPLE
    pwsh .\build-setup.ps1

.EXAMPLE
    pwsh .\build-setup.ps1 -Version 1.0.21 -Mode Update
#>
param(
    [string]$Version = "1.0.21",
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [ValidateSet("Install","Update")]
    [string]$Mode = "Update",
    [string]$OutputRoot = "./artifacts/setup",
    [switch]$SkipPublish,
    [switch]$SkipZip
)

$ErrorActionPreference = "Stop"

function Write-Step([string]$Message) {
    Write-Host "[SETUP] $Message" -ForegroundColor Cyan
}

# Yollar
$ScriptDir   = $PSScriptRoot
$ProjectFile = (Resolve-Path (Join-Path $ScriptDir "..\..\KOAFiloServis.Web.csproj")).Path
$DeployIis   = (Resolve-Path (Join-Path $ScriptDir "..\IIS")).Path

# Çıktı yolları (mutlak)
if (-not [System.IO.Path]::IsPathRooted($OutputRoot)) {
    $OutputRoot = Join-Path (Get-Location) $OutputRoot
}
$PublishDir = Join-Path $OutputRoot "publish"
$PackageDir = Join-Path $OutputRoot ("KOAFiloServis-{0}" -f $Version)
$ZipPath    = Join-Path $OutputRoot ("KOAFiloServis-Setup-{0}.zip" -f $Version)

Write-Host ""
Write-Host "================================================" -ForegroundColor Yellow
Write-Host " KOAFiloServis Setup Builder" -ForegroundColor Yellow
Write-Host " Versiyon      : $Version" -ForegroundColor Yellow
Write-Host " Configuration : $Configuration" -ForegroundColor Yellow
Write-Host " Runtime       : $Runtime" -ForegroundColor Yellow
Write-Host " Mod           : $Mode" -ForegroundColor Yellow
Write-Host " Çıktı kökü    : $OutputRoot" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Yellow
Write-Host ""

# 1) Publish
if (-not $SkipPublish) {
    Write-Step "[1/5] dotnet publish çalışıyor..."
    if (Test-Path $PublishDir) { Remove-Item $PublishDir -Recurse -Force }
    New-Item -ItemType Directory -Force -Path $PublishDir | Out-Null

    dotnet publish $ProjectFile `
        -c $Configuration `
        -r $Runtime `
        --self-contained false `
        -p:Version=$Version `
        -p:AssemblyVersion=$Version `
        -p:FileVersion=$Version `
        -p:InformationalVersion=$Version `
        -o $PublishDir
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish başarısız oldu. Çıkış kodu: $LASTEXITCODE"
    }
} else {
    Write-Step "[1/5] Publish atlandı (SkipPublish)."
    if (-not (Test-Path $PublishDir)) {
        throw "SkipPublish verildi ancak publish klasörü yok: $PublishDir"
    }
}

# 2) Paket klasörü
Write-Step "[2/5] Paket klasörü hazırlanıyor: $PackageDir"
if (Test-Path $PackageDir) { Remove-Item $PackageDir -Recurse -Force }
New-Item -ItemType Directory -Force -Path $PackageDir | Out-Null
Copy-Item "$PublishDir\*" $PackageDir -Recurse -Force

# 3) IIS kurulum scriptleri
Write-Step "[3/5] IIS kurulum scriptleri ekleniyor..."
if (Test-Path (Join-Path $DeployIis "kur.ps1")) {
    Copy-Item (Join-Path $DeployIis "kur.ps1") $PackageDir -Force
}
if (Test-Path (Join-Path $DeployIis "kur.bat")) {
    Copy-Item (Join-Path $DeployIis "kur.bat") $PackageDir -Force
}

# 4) VERSION.txt ve MODE.txt
Write-Step "[4/5] Sürüm/mod meta dosyaları yazılıyor..."
$BuildDate = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssK")
$VersionContent = @"
$Version
"@
Set-Content -Path (Join-Path $PackageDir "VERSION.txt") -Value $Version.Trim() -Encoding ASCII
Set-Content -Path (Join-Path $PackageDir "MODE.txt")    -Value $Mode.Trim()    -Encoding ASCII

# Yardımcı README
$ReadmeContent = @"
KOAFiloServis Kurulum Paketi
============================
Versiyon : $Version
Tarih    : $BuildDate
Mod      : $Mode
Runtime  : $Runtime

Kurulum / Güncelleme:
  - IIS sunucuda paket klasörünü kopyalayın.
  - Yönetici PowerShell ile:
      pwsh .\kur.ps1 -Mode Update
    veya ilk kurulum için:
      pwsh .\kur.ps1 -Mode Install
  - Alternatif: kur.bat
"@
Set-Content -Path (Join-Path $PackageDir "README.txt") -Value $ReadmeContent -Encoding UTF8

# 5) ZIP
if (-not $SkipZip) {
    Write-Step "[5/5] ZIP oluşturuluyor: $ZipPath"
    if (Test-Path $ZipPath) { Remove-Item $ZipPath -Force }
    Compress-Archive -Path (Join-Path $PackageDir "*") -DestinationPath $ZipPath -CompressionLevel Optimal
} else {
    Write-Step "[5/5] ZIP atlandı (SkipZip)."
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host " ✓ Setup paketi hazır" -ForegroundColor Green
Write-Host " Paket klasörü : $PackageDir" -ForegroundColor Green
if (-not $SkipZip) {
    Write-Host " ZIP arşivi    : $ZipPath" -ForegroundColor Green
}
Write-Host "================================================" -ForegroundColor Green
