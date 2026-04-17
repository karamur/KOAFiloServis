<#
.SYNOPSIS
    KOAFiloServis tam kurulum paketi uretir.

.DESCRIPTION
    1) KOAFiloServis.Web        -> publish (framework-dependent, IIS)
    2) KOAFiloServis.LisansDesktop -> publish (self-contained, win-x64, SingleFile)
    3) KOAFiloServis.DataSync      -> publish (self-contained, win-x64, SingleFile)
    4) Inno Setup ile tek EXE paket uretir: setup\output\KOAFiloServisKurulum-<version>.exe
    5) (opsiyonel) -CopyToPublish ile F:\publish\Installer\ altina kopyalar.

.PARAMETER Version
    Paket versiyon numarasi. Varsayilan 1.0.0

.PARAMETER SkipPublish
    Eger daha once publish yapildiysa ve sadece Inno Setup'i yeniden cagirmak istiyorsaniz.

.PARAMETER CopyToPublish
    Ciktiyi F:\publish\Installer\ altina da kopyalar.

.EXAMPLE
    .\build.ps1 -Version 1.0.3
    .\build.ps1 -Version 1.0.3 -CopyToPublish
#>
[CmdletBinding()]
param(
    [string] $Version = '1.0.0',
    [switch] $SkipPublish,
    [switch] $CopyToPublish
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

$Root      = Split-Path -Parent $MyInvocation.MyCommand.Definition
$RepoRoot  = Split-Path -Parent $Root
$Payload   = Join-Path $Root 'payload'
$Output    = Join-Path $Root 'output'

$Web       = Join-Path $RepoRoot 'KOAFiloServis.Web\KOAFiloServis.Web.csproj'
$Lisans    = Join-Path $RepoRoot 'KOAFiloServis.LisansDesktop\KOAFiloServis.LisansDesktop.csproj'
$DataSync  = Join-Path $RepoRoot 'KOAFiloServis.DataSync\KOAFiloServis.DataSync.csproj'

$IsccExe = @(
    "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
    "C:\Program Files\Inno Setup 6\ISCC.exe",
    "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $IsccExe) {
    throw "Inno Setup (ISCC.exe) bulunamadi. 'winget install JRSoftware.InnoSetup' ile kurun."
}

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "KOAFiloServis Paket Uretim - v$Version" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "Kaynak  : $RepoRoot"
Write-Host "Payload : $Payload"
Write-Host "Output  : $Output"
Write-Host "ISCC    : $IsccExe"
Write-Host ""

if (-not $SkipPublish) {
    if (Test-Path $Payload) { Remove-Item $Payload -Recurse -Force }
    New-Item -ItemType Directory -Force $Payload, $Output | Out-Null

    Write-Host "[1/4] Web publish..." -ForegroundColor Green
    dotnet publish $Web -c Release -o "$Payload\Web" /p:Version=$Version /p:UseAppHost=true --nologo | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "Web publish basarisiz." }

    Write-Host "[2/4] LisansDesktop publish..." -ForegroundColor Green
    dotnet publish $Lisans -c Release -r win-x64 --self-contained `
        -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
        /p:Version=$Version -o "$Payload\LisansDesktop" --nologo | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "LisansDesktop publish basarisiz." }

    Write-Host "[3/4] DataSync publish..." -ForegroundColor Green
    dotnet publish $DataSync -c Release -r win-x64 --self-contained `
        -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
        /p:Version=$Version -o "$Payload\DataSync" --nologo | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "DataSync publish basarisiz." }
} else {
    Write-Host "[PUBLISH ATLANDI] -SkipPublish" -ForegroundColor Yellow
}

Write-Host "[4/4] Inno Setup derleme..." -ForegroundColor Green
& $IsccExe "/DMyAppVersion=$Version" (Join-Path $Root 'Setup.iss')
if ($LASTEXITCODE -ne 0) { throw "Inno Setup derleme basarisiz." }

$exeAdi = "KOAFiloServisKurulum-$Version.exe"
$exePath = Join-Path $Output $exeAdi

if (-not (Test-Path $exePath)) {
    throw "Beklenen cikti bulunamadi: $exePath"
}

$boyut = [math]::Round((Get-Item $exePath).Length / 1MB, 2)
Write-Host ""
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "BASARILI: $exePath ($boyut MB)" -ForegroundColor Green
Write-Host "==================================================" -ForegroundColor Cyan

if ($CopyToPublish) {
    $hedef = 'F:\publish\Installer'
    if (-not (Test-Path 'F:\publish')) {
        Write-Host "UYARI: F:\publish yok, kopyalama atlandi." -ForegroundColor Yellow
    } else {
        New-Item -ItemType Directory -Force $hedef | Out-Null
        Copy-Item $exePath $hedef -Force
        Write-Host "Kopyalandi: $hedef\$exeAdi" -ForegroundColor Green
    }
}
