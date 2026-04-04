#!/bin/bash
#===============================================================================
# CRM Filo Servis - Tam Kurulum Scripti
# Tüm servisleri Docker ile kurar ve yapılandırır
#===============================================================================

set -e

# Renkli çıktı
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

INSTALL_DIR="/opt/crmfiloservis"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

print_header() {
    echo -e "${BLUE}"
    echo "╔══════════════════════════════════════════════════════════════╗"
    echo "║     CRM Filo Servis - Kurumsal Kurulum                       ║"
    echo "║     Versiyon: 1.0.0                                          ║"
    echo "╚══════════════════════════════════════════════════════════════╝"
    echo -e "${NC}"
}

check_requirements() {
    echo -e "${BLUE}▶ Gereksinimler kontrol ediliyor...${NC}"
    
    # Docker
    if ! command -v docker &> /dev/null; then
        echo -e "${YELLOW}Docker yükleniyor...${NC}"
        curl -fsSL https://get.docker.com | sh
        sudo usermod -aG docker $USER
    fi
    echo -e "${GREEN}✓ Docker${NC}"
    
    # Docker Compose
    if ! command -v docker-compose &> /dev/null; then
        echo -e "${YELLOW}Docker Compose yükleniyor...${NC}"
        sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
        sudo chmod +x /usr/local/bin/docker-compose
    fi
    echo -e "${GREEN}✓ Docker Compose${NC}"
    
    # Git
    if ! command -v git &> /dev/null; then
        echo -e "${YELLOW}Git yükleniyor...${NC}"
        if command -v apt-get &> /dev/null; then
            sudo apt-get update && sudo apt-get install -y git
        elif command -v dnf &> /dev/null; then
            sudo dnf install -y git
        fi
    fi
    echo -e "${GREEN}✓ Git${NC}"
    
    echo ""
}

create_directories() {
    echo -e "${BLUE}▶ Dizinler oluşturuluyor...${NC}"
    
    sudo mkdir -p "$INSTALL_DIR"/{data,logs,backups,ssl}
    sudo mkdir -p "$INSTALL_DIR"/nginx/conf.d
    sudo mkdir -p "$INSTALL_DIR"/monitoring/{prometheus,grafana}
    
    sudo chown -R $USER:$USER "$INSTALL_DIR"
    
    echo -e "${GREEN}✓ Dizinler oluşturuldu: $INSTALL_DIR${NC}"
    echo ""
}

copy_files() {
    echo -e "${BLUE}▶ Dosyalar kopyalanıyor...${NC}"
    
    # Docker Compose
    cp "$SCRIPT_DIR/../docker-compose.yml" "$INSTALL_DIR/"
    cp "$SCRIPT_DIR/../appsettings.Production.json" "$INSTALL_DIR/"
    
    # Nginx
    cp "$SCRIPT_DIR/../nginx/nginx.conf" "$INSTALL_DIR/nginx/"
    
    # Monitoring
    cp "$SCRIPT_DIR/../monitoring/prometheus.yml" "$INSTALL_DIR/monitoring/prometheus/"
    cp "$SCRIPT_DIR/../monitoring/grafana-dashboard.json" "$INSTALL_DIR/monitoring/grafana/"
    cp "$SCRIPT_DIR/../monitoring/grafana-datasources.yml" "$INSTALL_DIR/monitoring/grafana/"
    
    # Scripts
    cp "$SCRIPT_DIR"/*.sh "$INSTALL_DIR/"
    chmod +x "$INSTALL_DIR"/*.sh
    
    echo -e "${GREEN}✓ Dosyalar kopyalandı${NC}"
    echo ""
}

configure_environment() {
    echo -e "${BLUE}▶ Ortam değişkenleri yapılandırılıyor...${NC}"
    
    # .env dosyası oluştur
    cat > "$INSTALL_DIR/.env" << EOF
# CRM Filo Servis - Ortam Değişkenleri
# Oluşturulma: $(date)

# Veritabanı
POSTGRES_USER=postgres
POSTGRES_PASSWORD=$(openssl rand -base64 32 | tr -dc 'a-zA-Z0-9' | head -c 24)
POSTGRES_DB=crmfiloservis

# Redis
REDIS_PASSWORD=$(openssl rand -base64 32 | tr -dc 'a-zA-Z0-9' | head -c 24)

# Uygulama
ASPNETCORE_ENVIRONMENT=Production
JWT_SECRET=$(openssl rand -base64 64 | tr -dc 'a-zA-Z0-9' | head -c 64)

# Grafana
GF_SECURITY_ADMIN_PASSWORD=$(openssl rand -base64 16 | tr -dc 'a-zA-Z0-9' | head -c 16)
EOF

    chmod 600 "$INSTALL_DIR/.env"
    
    echo -e "${GREEN}✓ .env dosyası oluşturuldu${NC}"
    echo -e "${YELLOW}  ⚠ Parolalar .env dosyasında saklanmaktadır${NC}"
    echo ""
}

pull_images() {
    echo -e "${BLUE}▶ Docker imajları indiriliyor...${NC}"
    
    cd "$INSTALL_DIR"
    docker-compose pull
    
    echo -e "${GREEN}✓ İmajlar indirildi${NC}"
    echo ""
}

start_services() {
    echo -e "${BLUE}▶ Servisler başlatılıyor...${NC}"
    
    cd "$INSTALL_DIR"
    docker-compose up -d
    
    # Servislerin başlamasını bekle
    echo -e "${YELLOW}Servisler başlatılıyor, lütfen bekleyin...${NC}"
    sleep 30
    
    echo -e "${GREEN}✓ Servisler başlatıldı${NC}"
    echo ""
}

show_status() {
    echo -e "${BLUE}▶ Servis durumu:${NC}"
    
    cd "$INSTALL_DIR"
    docker-compose ps
    
    echo ""
}

print_summary() {
    echo -e "${GREEN}"
    echo "╔══════════════════════════════════════════════════════════════╗"
    echo "║                 KURULUM TAMAMLANDI!                          ║"
    echo "╠══════════════════════════════════════════════════════════════╣"
    echo "║                                                              ║"
    echo "║  Web Uygulaması:  http://localhost                           ║"
    echo "║  Grafana:         http://localhost:3000                      ║"
    echo "║  Prometheus:      http://localhost:9090                      ║"
    echo "║                                                              ║"
    echo "║  Kurulum Dizini:  $INSTALL_DIR                        ║"
    echo "║  Parolalar:       $INSTALL_DIR/.env                   ║"
    echo "║                                                              ║"
    echo "╠══════════════════════════════════════════════════════════════╣"
    echo "║  Yardımcı Komutlar:                                          ║"
    echo "║                                                              ║"
    echo "║  • Durum:      docker-compose ps                             ║"
    echo "║  • Loglar:     docker-compose logs -f                        ║"
    echo "║  • Yeniden:    docker-compose restart                        ║"
    echo "║  • Durdur:     docker-compose down                           ║"
    echo "║  • Yedekle:    ./backup.sh                                   ║"
    echo "║  • Sağlık:     ./health-check.sh                             ║"
    echo "║                                                              ║"
    echo "╚══════════════════════════════════════════════════════════════╝"
    echo -e "${NC}"
}

# Ana işlem
print_header
check_requirements
create_directories
copy_files
configure_environment
pull_images
start_services
show_status
print_summary
