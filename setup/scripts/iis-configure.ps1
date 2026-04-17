<#
    KOAFiloServis — IIS otomatik yapilandirma
    - Web sitesi + AppPool olusturur/gunceller
    - ACL: IIS AppPool\<SiteName> kullanicisina yazma izni
    - Idempotent (tekrar tekrar calistirilsa sorun cikmaz)
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $InstallPath,
    [Parameter(Mandatory)] [string] $SiteName,
    [Parameter(Mandatory)] [int]    $Port
)

$ErrorActionPreference = 'Stop'

function Ensure-Module {
    if (-not (Get-Module -ListAvailable -Name WebAdministration)) {
        Write-Host "IIS WebAdministration modulu bulunamadi. IIS kurulu mu?" -ForegroundColor Yellow
        exit 1
    }
    Import-Module WebAdministration -ErrorAction Stop
}

function Ensure-AppPool {
    param([string] $Name)
    if (-not (Test-Path "IIS:\AppPools\$Name")) {
        New-WebAppPool -Name $Name | Out-Null
        Write-Host "AppPool olusturuldu: $Name"
    } else {
        Write-Host "AppPool zaten var: $Name"
    }
    Set-ItemProperty "IIS:\AppPools\$Name" -Name managedRuntimeVersion -Value ''   # No Managed Code
    Set-ItemProperty "IIS:\AppPools\$Name" -Name processModel.identityType -Value 'ApplicationPoolIdentity'
    Set-ItemProperty "IIS:\AppPools\$Name" -Name startMode -Value 'AlwaysRunning'
    Set-ItemProperty "IIS:\AppPools\$Name" -Name autoStart -Value $true
}

function Ensure-Site {
    param(
        [string] $Name,
        [string] $Path,
        [int]    $Port,
        [string] $AppPool
    )
    if (-not (Test-Path "IIS:\Sites\$Name")) {
        New-Website -Name $Name -PhysicalPath $Path -Port $Port -ApplicationPool $AppPool -Force | Out-Null
        Write-Host "Site olusturuldu: $Name (:$Port)"
    } else {
        Set-ItemProperty "IIS:\Sites\$Name" -Name physicalPath -Value $Path
        Set-ItemProperty "IIS:\Sites\$Name" -Name applicationPool -Value $AppPool
        # Port kontrol
        $bindings = Get-WebBinding -Name $Name
        if (-not ($bindings | Where-Object { $_.bindingInformation -like "*:$Port:*" })) {
            New-WebBinding -Name $Name -Protocol http -Port $Port | Out-Null
        }
        Write-Host "Site guncellendi: $Name (:$Port)"
    }
}

function Grant-Acl {
    param([string] $Path, [string] $AppPool)
    $ident = "IIS AppPool\$AppPool"
    try {
        $acl = Get-Acl $Path
        $rule = New-Object System.Security.AccessControl.FileSystemAccessRule(
            $ident, 'Modify', 'ContainerInherit,ObjectInherit', 'None', 'Allow')
        $acl.SetAccessRule($rule)
        Set-Acl $Path $acl
        Write-Host "ACL: $ident icin Modify izni verildi -> $Path"
    } catch {
        Write-Host "ACL atanamadi ($Path): $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

try {
    Write-Host "=== KOAFiloServis IIS yapilandirma ==="
    Write-Host "Klasor : $InstallPath"
    Write-Host "Site   : $SiteName"
    Write-Host "Port   : $Port"

    Ensure-Module
    Ensure-AppPool -Name $SiteName
    Ensure-Site -Name $SiteName -Path $InstallPath -Port $Port -AppPool $SiteName

    foreach ($sub in @($InstallPath, "$InstallPath\data", "$InstallPath\uploads", "$InstallPath\logs", "$InstallPath\Backups")) {
        if (Test-Path $sub) { Grant-Acl -Path $sub -AppPool $SiteName }
    }

    Start-WebAppPool -Name $SiteName -ErrorAction SilentlyContinue
    Start-Website    -Name $SiteName -ErrorAction SilentlyContinue

    Write-Host "=== IIS yapilandirma tamam ===" -ForegroundColor Green
    exit 0
}
catch {
    Write-Host "HATA: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host $_.ScriptStackTrace
    exit 1
}
