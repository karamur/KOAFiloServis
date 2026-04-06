#!/bin/bash

# CRM Filo Servis - Linux Sunucu Güncelleme Scripti
# Kullanım: ./update-server.sh /path/to/CRMFiloServis_Update_YYYYMMDD.zip

set -e

# Renkler
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Parametreler
ZIP_PATH=$1
APP_PATH=${2:-"/var/www/crmfiloservis"}
SERVICE_NAME=${3:-"crmfiloservis"}

echo -e "${CYAN}========================================"
echo -e "  CRM Filo Servis - Sunucu Güncellemesi"
echo -e "========================================${NC}"
echo ""

# ZIP dosyasını kontrol et
if [ -z "$ZIP_PATH" ]; then
    echo -e "${RED}HATA: ZIP dosyası belirtilmedi${NC}"
    echo "Kullanım: ./update-server.sh /path/to/update.zip [app_path] [service_name]"
    exit 1
fi

if [ ! -f "$ZIP_PATH" ]; then
    echo -e "${RED}HATA: ZIP dosyası bulunamadı: $ZIP_PATH${NC}"
    exit 1
fi

echo -e "${YELLOW}[1/6] Güncelleme paketi: $ZIP_PATH${NC}"

# Yedekleme
BACKUP_PATH="${APP_PATH}.backup_$(date +%Y%m%d_%H%M%S)"
echo -e "${YELLOW}[2/6] Yedekleme yapılıyor: $BACKUP_PATH${NC}"
if [ -d "$APP_PATH" ]; then
    sudo cp -r "$APP_PATH" "$BACKUP_PATH"
    echo -e "${GREEN}      Yedekleme tamamlandı${NC}"
fi

# Servisi durdur
echo -e "${YELLOW}[3/6] Servis durduruluyor: $SERVICE_NAME${NC}"
if systemctl is-active --quiet "$SERVICE_NAME" 2>/dev/null; then
    sudo systemctl stop "$SERVICE_NAME"
    echo -e "${GREEN}      Servis durduruldu${NC}"
else
    echo -e "${CYAN}      Servis zaten çalışmıyor${NC}"
fi

# ZIP'i aç
TEMP_PATH="/tmp/crmfiloservis_update_$$"
echo -e "${YELLOW}[4/6] Güncelleme paketi açılıyor...${NC}"
mkdir -p "$TEMP_PATH"
unzip -o "$ZIP_PATH" -d "$TEMP_PATH"
echo -e "${GREEN}      Paket açıldı${NC}"

# Dosyaları kopyala
echo -e "${YELLOW}[5/6] Dosyalar kopyalanıyor...${NC}"
sudo mkdir -p "$APP_PATH"
sudo cp -r "$TEMP_PATH"/* "$APP_PATH"/
sudo chown -R www-data:www-data "$APP_PATH" 2>/dev/null || true
sudo chmod -R 755 "$APP_PATH"
echo -e "${GREEN}      Dosyalar kopyalandı${NC}"

# Temp klasörünü temizle
rm -rf "$TEMP_PATH"

# Servisi başlat
echo -e "${YELLOW}[6/6] Servis başlatılıyor...${NC}"
if systemctl list-unit-files "$SERVICE_NAME.service" &>/dev/null; then
    sudo systemctl start "$SERVICE_NAME"
    echo -e "${GREEN}      Servis başlatıldı${NC}"
fi

echo ""
echo -e "${GREEN}========================================"
echo -e "  Güncelleme başarıyla tamamlandı!"
echo -e "========================================${NC}"
echo ""
echo -e "${CYAN}Yedek dizini: $BACKUP_PATH${NC}"
echo ""
