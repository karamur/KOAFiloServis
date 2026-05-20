@echo off
setlocal
set "VERSION=1.0.21"
if not "%~1"=="" set "VERSION=%~1"

set "MODE=Update"
if not "%~2"=="" set "MODE=%~2"

pushd "%~dp0"
where pwsh >nul 2>&1
if errorlevel 1 (
    echo [HATA] pwsh bulunamadi. PowerShell 7+ yukleyin.
    popd
    exit /b 1
)

pwsh -NoProfile -ExecutionPolicy Bypass -File ".\build-setup.ps1" -Version "%VERSION%" -Mode "%MODE%"
set "RC=%ERRORLEVEL%"
popd
exit /b %RC%
