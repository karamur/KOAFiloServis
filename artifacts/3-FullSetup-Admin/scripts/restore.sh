#!/bin/bash
#===============================================================================
# CRM Filo Servis - Veritabanı Geri Yükleme Scripti
# Versiyon: 1.0.0
#===============================================================================

set -e

# Renkli çıktı
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Varsayılan değerler
BACKUP_DIR="/var/backups/crmfiloservis"
DB_HOST="${DB_HOST:-localhost}"
DB_PORT="${DB_PORT:-5432}"
DB_NAME="${DB_NAME:-crmfiloservis}"
DB_USER="${DB_USER:-postgres}"
CONTAINER_NAME="${CONTAINER_NAME:-crmfiloservis-db}"

print_header() {
    echo -e "${BLUE}"
    echo "╔══════════════════════════════════════════════════════════════╗"
    echo "║        CRM Filo Servis - Veritabanı Geri Yükleme             ║"
    echo "╚══════════════════════════════════════════════════════════════╝"
    echo -e "${NC}"
}

print_usage() {
    echo -e "${YELLOW}Kullanım:${NC}"
    echo "  $0 <backup_dosyası.sql.gz>"
    echo ""
    echo -e "${YELLOW}Örnekler:${NC}"
    echo "  $0 /var/backups/crmfiloservis/crmfiloservis_20240115_120000.sql.gz"
    echo "  $0 backup.sql.gz"
    echo ""
    echo -e "${YELLOW}Ortam Değişkenleri:${NC}"
    echo "  DB_HOST         - Veritabanı sunucusu (varsayılan: localhost)"
    echo "  DB_PORT         - Veritabanı portu (varsayılan: 5432)"
    echo "  DB_NAME         - Veritabanı adı (varsayılan: crmfiloservis)"
    echo "  DB_USER         - Veritabanı kullanıcısı (varsayılan: postgres)"
    echo "  CONTAINER_NAME  - Docker container adı (varsayılan: crmfiloservis-db)"
    echo ""
}

check_backup_file() {
    local backup_file="$1"
    
    if [ ! -f "$backup_file" ]; then
        echo -e "${RED}HATA: Yedek dosyası bulunamadı: $backup_file${NC}"
        exit 1
    fi
    
    if [[ ! "$backup_file" =~ \.(sql|sql\.gz)$ ]]; then
        echo -e "${RED}HATA: Geçersiz dosya formatı. .sql veya .sql.gz dosyası bekleniyor.${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}✓ Yedek dosyası bulundu: $backup_file${NC}"
}

list_backups() {
    echo -e "${BLUE}Mevcut yedekler:${NC}"
    echo "───────────────────────────────────────────────────────────────"
    
    if [ -d "$BACKUP_DIR" ]; then
        ls -lh "$BACKUP_DIR"/*.sql.gz 2>/dev/null | awk '{print $9, "(" $5 ")"}'
        if [ $? -ne 0 ]; then
            echo -e "${YELLOW}Yedek bulunamadı.${NC}"
        fi
    else
        echo -e "${YELLOW}Yedek dizini bulunamadı: $BACKUP_DIR${NC}"
    fi
    echo ""
}

confirm_restore() {
    local backup_file="$1"
    
    echo -e "${YELLOW}═══════════════════════════════════════════════════════════════${NC}"
    echo -e "${YELLOW}DİKKAT: Bu işlem mevcut veritabanını tamamen silecek!${NC}"
    echo -e "${YELLOW}═══════════════════════════════════════════════════════════════${NC}"
    echo ""
    echo -e "Hedef Veritabanı: ${RED}$DB_NAME${NC}"
    echo -e "Yedek Dosyası:    ${GREEN}$backup_file${NC}"
    echo ""
    
    read -p "Devam etmek istiyor musunuz? (evet/hayır): " confirm
    
    if [[ "$confirm" != "evet" ]]; then
        echo -e "${YELLOW}İşlem iptal edildi.${NC}"
        exit 0
    fi
}

stop_application() {
    echo -e "${BLUE}▶ Uygulama durduruluyor...${NC}"
    
    # Docker Compose ile çalışıyorsa
    if command -v docker-compose &> /dev/null; then
        docker-compose stop app 2>/dev/null || true
    fi
    
    # Systemd servisi olarak çalışıyorsa
    if systemctl is-active --quiet crmfiloservis 2>/dev/null; then
        sudo systemctl stop crmfiloservis
    fi
    
    sleep 2
    echo -e "${GREEN}✓ Uygulama durduruldu${NC}"
}

restore_database() {
    local backup_file="$1"
    local is_compressed=false
    
    [[ "$backup_file" == *.gz ]] && is_compressed=true
    
    echo -e "${BLUE}▶ Veritabanı geri yükleniyor...${NC}"
    
    # Docker container kullanılıyorsa
    if docker ps --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
        echo -e "${BLUE}  Docker container kullanılıyor: $CONTAINER_NAME${NC}"
        
        # Mevcut bağlantıları sonlandır
        docker exec -i "$CONTAINER_NAME" psql -U "$DB_USER" -d postgres -c \
            "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '$DB_NAME' AND pid <> pg_backend_pid();" \
            > /dev/null 2>&1 || true
        
        # Veritabanını sil ve yeniden oluştur
        docker exec -i "$CONTAINER_NAME" psql -U "$DB_USER" -d postgres -c "DROP DATABASE IF EXISTS $DB_NAME;" > /dev/null 2>&1
        docker exec -i "$CONTAINER_NAME" psql -U "$DB_USER" -d postgres -c "CREATE DATABASE $DB_NAME;" > /dev/null 2>&1
        
        # Geri yükle
        if $is_compressed; then
            gunzip -c "$backup_file" | docker exec -i "$CONTAINER_NAME" psql -U "$DB_USER" -d "$DB_NAME"
        else
            docker exec -i "$CONTAINER_NAME" psql -U "$DB_USER" -d "$DB_NAME" < "$backup_file"
        fi
        
    else
        echo -e "${BLUE}  Doğrudan PostgreSQL bağlantısı kullanılıyor${NC}"
        
        # Mevcut bağlantıları sonlandır
        PGPASSWORD="$PGPASSWORD" psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d postgres -c \
            "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '$DB_NAME' AND pid <> pg_backend_pid();" \
            > /dev/null 2>&1 || true
        
        # Veritabanını sil ve yeniden oluştur
        PGPASSWORD="$PGPASSWORD" psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d postgres -c "DROP DATABASE IF EXISTS $DB_NAME;" > /dev/null 2>&1
        PGPASSWORD="$PGPASSWORD" psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d postgres -c "CREATE DATABASE $DB_NAME;" > /dev/null 2>&1
        
        # Geri yükle
        if $is_compressed; then
            gunzip -c "$backup_file" | PGPASSWORD="$PGPASSWORD" psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME"
        else
            PGPASSWORD="$PGPASSWORD" psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" < "$backup_file"
        fi
    fi
    
    echo -e "${GREEN}✓ Veritabanı geri yüklendi${NC}"
}

start_application() {
    echo -e "${BLUE}▶ Uygulama başlatılıyor...${NC}"
    
    # Docker Compose ile çalışıyorsa
    if command -v docker-compose &> /dev/null; then
        docker-compose start app 2>/dev/null || true
    fi
    
    # Systemd servisi olarak çalışıyorsa
    if systemctl list-units --full -all | grep -q "crmfiloservis.service"; then
        sudo systemctl start crmfiloservis
    fi
    
    sleep 3
    echo -e "${GREEN}✓ Uygulama başlatıldı${NC}"
}

verify_restore() {
    echo -e "${BLUE}▶ Geri yükleme doğrulanıyor...${NC}"
    
    local table_count
    
    if docker ps --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
        table_count=$(docker exec -i "$CONTAINER_NAME" psql -U "$DB_USER" -d "$DB_NAME" -t -c \
            "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';")
    else
        table_count=$(PGPASSWORD="$PGPASSWORD" psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -t -c \
            "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';")
    fi
    
    table_count=$(echo "$table_count" | tr -d ' ')
    
    if [ "$table_count" -gt 0 ]; then
        echo -e "${GREEN}✓ Doğrulama başarılı: $table_count tablo geri yüklendi${NC}"
    else
        echo -e "${RED}✗ UYARI: Tablolar bulunamadı!${NC}"
    fi
}

# Ana işlem
print_header

if [ $# -eq 0 ]; then
    list_backups
    print_usage
    exit 1
fi

BACKUP_FILE="$1"

check_backup_file "$BACKUP_FILE"
confirm_restore "$BACKUP_FILE"

echo ""
echo -e "${BLUE}Geri yükleme başlatılıyor...${NC}"
echo "───────────────────────────────────────────────────────────────"

stop_application
restore_database "$BACKUP_FILE"
start_application
verify_restore

echo ""
echo -e "${GREEN}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}       Veritabanı geri yükleme işlemi tamamlandı!              ${NC}"
echo -e "${GREEN}═══════════════════════════════════════════════════════════════${NC}"
