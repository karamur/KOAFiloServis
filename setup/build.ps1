<#
.SYNOPSIS
    KOAFiloServis kurulum paketleri uretir.

.DESCRIPTION
    1) KOAFiloServis.Web           -> publish (framework-dependent, IIS)
    2) KOAFiloServis.LisansDesktop -> publish (self-contained, win-x64, SingleFile)
    3) KOAFiloServis.DataSync      -> publish (self-contained, win-x64, SingleFile)
    4) Inno Setup — Setup.iss      -> KOAFiloServisKurulum-<version>.exe (tam paket)
    5) Inno Setup — LisansSetup.iss-> KOALisansArac-<version>.exe      (sadece lisans araci)
    6) (opsiyonel) -CopyToPublish ile F:\publish\Installer\ altina kopyalar.

.PARAMETER Version
    Paket versiyon numarasi. Varsayilan 1.0.0

.PARAMETER SkipPublish
    Eger daha once publish yapildiysa ve sadece Inno Setup'i yeniden cagirmak istiyorsaniz.

.PARAMETER LisansOnly
    Sadece LisansDesktop'u publish + LisansSetup.iss ile EXE uretir (Web/DataSync atlanir).

.PARAMETER CopyToPublish
    Ciktiyi F:\publish\Installer\ altina da kopyalar.

.EXAMPLE
    .\build.ps1 -Version 1.0.3
    .\build.ps1 -Version 1.0.3 -LisansOnly
    .\build.ps1 -Version 1.0.3 -CopyToPublish
#>
[CmdletBinding()]
param(
    [string] $Version = '1.0.0',
    [switch] $SkipPublish,
    [switch] $LisansOnly,
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
    if ($LisansOnly) {
        # Sadece LisansDesktop payload klasorunu temizle
        $lPayload = Join-Path $Payload 'LisansDesktop'
        if (Test-Path $lPayload) { Remove-Item $lPayload -Recurse -Force }
        New-Item -ItemType Directory -Force $lPayload, $Output | Out-Null
    } else {
        if (Test-Path $Payload) { Remove-Item $Payload -Recurse -Force }
        New-Item -ItemType Directory -Force $Payload, $Output | Out-Null
    }

    if (-not $LisansOnly) {
        Write-Host "[1/5] Web publish..." -ForegroundColor Green
        dotnet publish $Web -c Release -o "$Payload\Web" /p:Version=$Version /p:UseAppHost=true --nologo | Out-Host
        if ($LASTEXITCODE -ne 0) { throw "Web publish basarisiz." }

        # web.config: stdoutLogEnabled=true (IIS sorunlarini tanilayabilmek icin)
        $webConfigPath = Join-Path $Payload 'Web\web.config'
        if (Test-Path $webConfigPath) {
            $wc = Get-Content $webConfigPath -Raw
            $wc2 = $wc -replace 'stdoutLogEnabled="false"', 'stdoutLogEnabled="true"'
            if ($wc -ne $wc2) {
                Set-Content -Path $webConfigPath -Value $wc2 -Encoding UTF8 -NoNewline
                Write-Host "       web.config: stdoutLogEnabled=true yapildi" -ForegroundColor DarkGray
            }
        }
    } # end -not LisansOnly

    Write-Host "[2/5] LisansDesktop publish..." -ForegroundColor Green
    dotnet publish $Lisans -c Release -r win-x64 --self-contained `
        -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
        /p:Version=$Version -o "$Payload\LisansDesktop" --nologo | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "LisansDesktop publish basarisiz." }

    if (-not $LisansOnly) {
        Write-Host "[3/5] DataSync publish..." -ForegroundColor Green
        dotnet publish $DataSync -c Release -r win-x64 --self-contained `
            -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
            /p:Version=$Version -o "$Payload\DataSync" --nologo | Out-Host
        if ($LASTEXITCODE -ne 0) { throw "DataSync publish basarisiz." }
    }
} else {
    Write-Host "[PUBLISH ATLANDI] -SkipPublish" -ForegroundColor Yellow
}

# ---- Inno Setup derlemeleri ----
if (-not $LisansOnly) {
    Write-Host "[4/6] Inno Setup - Ana paket (Setup.iss)..." -ForegroundColor Green
    & $IsccExe "/DMyAppVersion=$Version" (Join-Path $Root 'Setup.iss')
    if ($LASTEXITCODE -ne 0) { throw "Inno Setup (Setup.iss) derleme basarisiz." }

    Write-Host "[5/6] Inno Setup - Guncelleme paketi (GuncelleSetup.iss)..." -ForegroundColor Green
    & $IsccExe "/DMyAppVersion=$Version" (Join-Path $Root 'GuncelleSetup.iss')
    if ($LASTEXITCODE -ne 0) { throw "Inno Setup (GuncelleSetup.iss) derleme basarisiz." }

    Write-Host "[6/7] Inno Setup - Musteri paketi (MusteriSetup.iss)..." -ForegroundColor Green
    & $IsccExe "/DMyAppVersion=$Version" (Join-Path $Root 'MusteriSetup.iss')
    if ($LASTEXITCODE -ne 0) { throw "Inno Setup (MusteriSetup.iss) derleme basarisiz." }
}

Write-Host "[7/7] Inno Setup - Lisans araci (LisansSetup.iss)..." -ForegroundColor Green
& $IsccExe "/DLisansAppVersion=$Version" (Join-Path $Root 'LisansSetup.iss')
if ($LASTEXITCODE -ne 0) { throw "Inno Setup (LisansSetup.iss) derleme basarisiz." }

$sonuclar = @()

if (-not $LisansOnly) {
    $exeAdi = "KOAFiloServisKurulum-$Version.exe"
    $exePath = Join-Path $Output $exeAdi
    if (-not (Test-Path $exePath)) { throw "Beklenen cikti bulunamadi: $exePath" }
    $boyut = [math]::Round((Get-Item $exePath).Length / 1MB, 2)
    $sonuclar += "  Ana paket       : $exePath ($boyut MB)"

    $guncelleAdi = "KOAFiloServisGuncelle-$Version.exe"
    $guncellePath = Join-Path $Output $guncelleAdi
    if (Test-Path $guncellePath) {
        $guncelleBoyut = [math]::Round((Get-Item $guncellePath).Length / 1MB, 2)
        $sonuclar += "  Guncelleme paketi: $guncellePath ($guncelleBoyut MB)"
    }

    $musteriAdi = "KOAFiloServisKurulumMusteri-$Version.exe"
    $musteriPath = Join-Path $Output $musteriAdi
    if (Test-Path $musteriPath) {
        $musteriBoyut = [math]::Round((Get-Item $musteriPath).Length / 1MB, 2)
        $sonuclar += "  Musteri paketi  : $musteriPath ($musteriBoyut MB)"
    }
}

$lisansExeAdi = "KOALisansArac-$Version.exe"
$lisansExePath = Join-Path $Output $lisansExeAdi
if (-not (Test-Path $lisansExePath)) { throw "Beklenen cikti bulunamadi: $lisansExePath" }
$lisansBoyut = [math]::Round((Get-Item $lisansExePath).Length / 1MB, 2)
$sonuclar += "  Lisans araci    : $lisansExePath ($lisansBoyut MB)"

Write-Host ""
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "BASARILI - Uretilen paketler:" -ForegroundColor Green
$sonuclar | ForEach-Object { Write-Host $_ -ForegroundColor Green }
Write-Host "==================================================" -ForegroundColor Cyan

if ($CopyToPublish) {
    $hedef = 'F:\publish\Installer'
    if (-not (Test-Path 'F:\publish')) {
        Write-Host "UYARI: F:\publish yok, kopyalama atlandi." -ForegroundColor Yellow
    } else {
        New-Item -ItemType Directory -Force $hedef | Out-Null
        if (-not $LisansOnly) {
            foreach ($dosyaAdi in @("KOAFiloServisKurulum-$Version.exe", "KOAFiloServisGuncelle-$Version.exe", "KOAFiloServisKurulumMusteri-$Version.exe")) {
                $src = Join-Path $Output $dosyaAdi
                if (Test-Path $src) {
                    Copy-Item $src $hedef -Force
                    Write-Host "Kopyalandi: $hedef\$dosyaAdi" -ForegroundColor Green
                }
            }
        }
        Copy-Item $lisansExePath $hedef -Force
        Write-Host "Kopyalandi: $hedef\$lisansExeAdi" -ForegroundColor Green
    }
}
