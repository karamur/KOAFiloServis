<#
    KOAFiloServis — Kurulum oncesi gereksinim kontrolu
    - IIS yuklu mu?
    - ASP.NET Core 10 Hosting Bundle yuklu mu?
    Eksik olan varsa uyari gosterir ama kurulumu engellemez (sadece log).
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'SilentlyContinue'
$sorunlar = @()

# IIS kontrol
$iis = Get-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole
if (-not $iis -or $iis.State -ne 'Enabled') {
    $sorunlar += 'IIS (Internet Information Services) yuklu degil. "Windows ozellikleri"nden "Internet Information Services"i aktif edin.'
}

# ASP.NET Core Module
$ancm = Get-ItemProperty "HKLM:\SYSTEM\CurrentControlSet\Services\W3SVC" -ErrorAction SilentlyContinue
if (Test-Path "HKLM:\SOFTWARE\Microsoft\IIS Extensions\IIS AspNetCore Module V2") {
    Write-Host "OK: ASP.NET Core Module V2 yuklu."
} else {
    $sorunlar += '.NET 10 Hosting Bundle bulunamadi. https://dotnet.microsoft.com/download/dotnet/10.0 -> "Hosting Bundle" indirip kurun.'
}

if ($sorunlar.Count -eq 0) {
    Write-Host "Gereksinimler tamam." -ForegroundColor Green
    exit 0
}

Write-Host "-- EKSIKLER --" -ForegroundColor Yellow
$sorunlar | ForEach-Object { Write-Host " * $_" -ForegroundColor Yellow }
Write-Host "Kurulum devam edecek ama uygulama calismayabilir. Eksikleri tamamladiktan sonra Setup.exe'yi tekrar calistirin." -ForegroundColor Yellow
exit 0
