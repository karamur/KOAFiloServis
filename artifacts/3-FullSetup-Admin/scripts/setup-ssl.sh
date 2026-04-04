#!/bin/bash
#===============================================================================
# CRM Filo Servis - SSL Sertifika Kurulum Scripti (Let's Encrypt)
#===============================================================================

set -e

# Renkli çıktı
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

print_header() {
    echo -e "${BLUE}"
    echo "╔══════════════════════════════════════════════════════════════╗"
    echo "║      CRM Filo Servis - SSL Sertifika Kurulumu                ║"
    echo "╚══════════════════════════════════════════════════════════════╝"
    echo -e "${NC}"
}

print_usage() {
    echo -e "${YELLOW}Kullanım:${NC}"
    echo "  $0 <domain> <email>"
    echo ""
    echo -e "${YELLOW}Örnek:${NC}"
    echo "  $0 crmfiloservis.com admin@crmfiloservis.com"
    echo ""
}

install_certbot() {
    echo -e "${BLUE}▶ Certbot kurulumu kontrol ediliyor...${NC}"
    
    if ! command -v certbot &> /dev/null; then
        echo -e "${YELLOW}Certbot yükleniyor...${NC}"
        
        if command -v apt-get &> /dev/null; then
            sudo apt-get update
            sudo apt-get install -y certbot python3-certbot-nginx
        elif command -v dnf &> /dev/null; then
            sudo dnf install -y certbot python3-certbot-nginx
        elif command -v yum &> /dev/null; then
            sudo yum install -y certbot python3-certbot-nginx
        else
            echo -e "${RED}Paket yöneticisi bulunamadı!${NC}"
            exit 1
        fi
    fi
    
    echo -e "${GREEN}✓ Certbot hazır${NC}"
}

obtain_certificate() {
    local domain="$1"
    local email="$2"
    
    echo -e "${BLUE}▶ SSL sertifikası alınıyor: $domain${NC}"
    
    # Nginx duruyorsa başlat
    if ! systemctl is-active --quiet nginx; then
        echo -e "${YELLOW}Nginx başlatılıyor...${NC}"
        sudo systemctl start nginx
    fi
    
    # Sertifika al
    sudo certbot --nginx \
        -d "$domain" \
        -d "www.$domain" \
        --email "$email" \
        --agree-tos \
        --non-interactive \
        --redirect
    
    echo -e "${GREEN}✓ SSL sertifikası başarıyla alındı${NC}"
}

setup_auto_renewal() {
    echo -e "${BLUE}▶ Otomatik yenileme ayarlanıyor...${NC}"
    
    # Cron job kontrol et / ekle
    if ! crontab -l 2>/dev/null | grep -q "certbot renew"; then
        (crontab -l 2>/dev/null; echo "0 3 * * * /usr/bin/certbot renew --quiet --post-hook 'systemctl reload nginx'") | crontab -
        echo -e "${GREEN}✓ Otomatik yenileme ayarlandı (her gün 03:00)${NC}"
    else
        echo -e "${YELLOW}Otomatik yenileme zaten ayarlanmış${NC}"
    fi
}

test_renewal() {
    echo -e "${BLUE}▶ Yenileme testi yapılıyor...${NC}"
    sudo certbot renew --dry-run
    echo -e "${GREEN}✓ Yenileme testi başarılı${NC}"
}

# Ana işlem
print_header

if [ $# -lt 2 ]; then
    print_usage
    exit 1
fi

DOMAIN="$1"
EMAIL="$2"

echo -e "Domain: ${GREEN}$DOMAIN${NC}"
echo -e "E-posta: ${GREEN}$EMAIL${NC}"
echo ""

install_certbot
obtain_certificate "$DOMAIN" "$EMAIL"
setup_auto_renewal
test_renewal

echo ""
echo -e "${GREEN}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}SSL sertifikası başarıyla kuruldu!${NC}"
echo -e "${GREEN}═══════════════════════════════════════════════════════════════${NC}"
echo ""
echo -e "Sertifika konumu: /etc/letsencrypt/live/$DOMAIN/"
echo -e "Geçerlilik: 90 gün (otomatik yenilenecek)"
