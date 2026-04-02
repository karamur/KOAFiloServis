@echo off
chcp 65001 >nul
title CRM Filo Servis - Başlatıcı

echo ============================================
echo    CRM Filo Servis - Başlatılıyor...
echo ============================================
echo.

cd /d "%~dp0"

REM Veritabanı dosyası kontrolü
if not exist "CRMFiloServis.db" (
    echo [INFO] Veritabanı ilk kez oluşturulacak...
)

REM Gerekli klasörleri oluştur
if not exist "Backups" mkdir Backups
if not exist "Logs" mkdir Logs
if not exist "Uploads" mkdir Uploads

echo [INFO] Uygulama başlatılıyor...
echo [INFO] Tarayıcıda açmak için: http://localhost:5190
echo.
echo Kapatmak için Ctrl+C tuşlarına basın veya bu pencereyi kapatın.
echo ============================================
echo.

CRMFiloServis.Web.exe

if %errorlevel% neq 0 (
    echo.
    echo [HATA] Uygulama başlatılamadı!
    echo.
    echo Olası çözümler:
    echo 1. .NET 10 Runtime yüklü olmalı
    echo 2. Port 5190 başka uygulama tarafından kullanılıyor olabilir
    echo 3. Yönetici olarak çalıştırmayı deneyin
    echo.
    pause
)
