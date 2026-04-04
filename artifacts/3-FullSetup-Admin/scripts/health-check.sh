#!/bin/bash
#===============================================================================
# CRM Filo Servis - Sağlık Kontrolü Scripti
# Tüm servislerin durumunu kontrol eder
#===============================================================================

set -e

# Renkli çıktı
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Konfigürasyon
APP_URL="${APP_URL:-http://localhost:5000}"
DB_HOST="${DB_HOST:-localhost}"
DB_PORT="${DB_PORT:-5432}"
REDIS_HOST="${REDIS_HOST:-localhost}"
REDIS_PORT="${REDIS_PORT:-6379}"

print_header() {
    echo -e "${BLUE}"
    echo "╔══════════════════════════════════════════════════════════════╗"
    echo "║           CRM Filo Servis - Sağlık Kontrolü                  ║"
    echo "╠══════════════════════════════════════════════════════════════╣"
    echo "║  $(date '+%Y-%m-%d %H:%M:%S')                                         ║"
    echo "╚══════════════════════════════════════════════════════════════╝"
    echo -e "${NC}"
}

check_service() {
    local name="$1"
    local status="$2"
    local details="$3"
    
    if [ "$status" = "OK" ]; then
        echo -e "  ${GREEN}✓${NC} $name: ${GREEN}$status${NC} $details"
    elif [ "$status" = "WARN" ]; then
        echo -e "  ${YELLOW}⚠${NC} $name: ${YELLOW}$status${NC} $details"
    else
        echo -e "  ${RED}✗${NC} $name: ${RED}$status${NC} $details"
    fi
}

check_app() {
    echo -e "${BLUE}▶ Web Uygulaması${NC}"
    
    # Health endpoint kontrolü
    local response
    local http_code
    
    http_code=$(curl -s -o /dev/null -w "%{http_code}" "${APP_URL}/health" 2>/dev/null || echo "000")
    
    if [ "$http_code" = "200" ]; then
        response=$(curl -s "${APP_URL}/health" 2>/dev/null)
        check_service "Health Endpoint" "OK" "(${APP_URL}/health)"
        
        # Detaylı sağlık bilgisi varsa göster
        if echo "$response" | grep -q "status"; then
            echo "    └─ Response: $response"
        fi
    elif [ "$http_code" = "000" ]; then
        check_service "Health Endpoint" "FAIL" "(Bağlantı kurulamadı)"
    else
        check_service "Health Endpoint" "WARN" "(HTTP $http_code)"
    fi
    
    # Ana sayfa kontrolü
    http_code=$(curl -s -o /dev/null -w "%{http_code}" "${APP_URL}/" 2>/dev/null || echo "000")
    
    if [ "$http_code" = "200" ]; then
        check_service "Ana Sayfa" "OK" ""
    else
        check_service "Ana Sayfa" "FAIL" "(HTTP $http_code)"
    fi
    echo ""
}

check_database() {
    echo -e "${BLUE}▶ PostgreSQL Veritabanı${NC}"
    
    # Docker container kontrolü
    if docker ps --format '{{.Names}}' | grep -q "crmfiloservis-db"; then
        local container_status
        container_status=$(docker inspect --format='{{.State.Health.Status}}' crmfiloservis-db 2>/dev/null || echo "unknown")
        
        if [ "$container_status" = "healthy" ]; then
            check_service "Container" "OK" "(healthy)"
        elif [ "$container_status" = "starting" ]; then
            check_service "Container" "WARN" "(starting...)"
        else
            check_service "Container" "WARN" "($container_status)"
        fi
    fi
    
    # Port kontrolü
    if nc -z "$DB_HOST" "$DB_PORT" 2>/dev/null; then
        check_service "Port $DB_PORT" "OK" ""
    else
        check_service "Port $DB_PORT" "FAIL" "(Bağlantı yok)"
    fi
    
    # Bağlantı sayısı (Docker varsa)
    if docker ps --format '{{.Names}}' | grep -q "crmfiloservis-db"; then
        local conn_count
        conn_count=$(docker exec crmfiloservis-db psql -U postgres -t -c \
            "SELECT count(*) FROM pg_stat_activity WHERE datname = 'crmfiloservis';" 2>/dev/null | tr -d ' ')
        
        if [ -n "$conn_count" ]; then
            check_service "Aktif Bağlantı" "OK" "($conn_count)"
        fi
    fi
    echo ""
}

check_redis() {
    echo -e "${BLUE}▶ Redis Cache${NC}"
    
    # Docker container kontrolü
    if docker ps --format '{{.Names}}' | grep -q "crmfiloservis-redis"; then
        check_service "Container" "OK" "(running)"
    fi
    
    # Port kontrolü
    if nc -z "$REDIS_HOST" "$REDIS_PORT" 2>/dev/null; then
        check_service "Port $REDIS_PORT" "OK" ""
    else
        check_service "Port $REDIS_PORT" "FAIL" "(Bağlantı yok)"
    fi
    
    # Redis PING
    if docker ps --format '{{.Names}}' | grep -q "crmfiloservis-redis"; then
        local pong
        pong=$(docker exec crmfiloservis-redis redis-cli PING 2>/dev/null)
        
        if [ "$pong" = "PONG" ]; then
            check_service "PING" "OK" ""
        else
            check_service "PING" "FAIL" ""
        fi
        
        # Bellek kullanımı
        local memory
        memory=$(docker exec crmfiloservis-redis redis-cli INFO memory 2>/dev/null | grep "used_memory_human" | cut -d: -f2 | tr -d '\r')
        if [ -n "$memory" ]; then
            check_service "Bellek" "OK" "($memory)"
        fi
    fi
    echo ""
}

check_nginx() {
    echo -e "${BLUE}▶ Nginx Reverse Proxy${NC}"
    
    # Docker container kontrolü
    if docker ps --format '{{.Names}}' | grep -q "crmfiloservis-nginx"; then
        check_service "Container" "OK" "(running)"
    elif systemctl is-active --quiet nginx 2>/dev/null; then
        check_service "Systemd" "OK" "(active)"
    fi
    
    # Port 80 kontrolü
    if nc -z localhost 80 2>/dev/null; then
        check_service "Port 80" "OK" ""
    else
        check_service "Port 80" "WARN" "(Kapalı)"
    fi
    
    # Port 443 kontrolü
    if nc -z localhost 443 2>/dev/null; then
        check_service "Port 443" "OK" ""
    else
        check_service "Port 443" "WARN" "(Kapalı - SSL yok)"
    fi
    echo ""
}

check_monitoring() {
    echo -e "${BLUE}▶ Monitoring${NC}"
    
    # Prometheus
    if nc -z localhost 9090 2>/dev/null; then
        check_service "Prometheus" "OK" "(:9090)"
    else
        check_service "Prometheus" "WARN" "(Çalışmıyor)"
    fi
    
    # Grafana
    if nc -z localhost 3000 2>/dev/null; then
        check_service "Grafana" "OK" "(:3000)"
    else
        check_service "Grafana" "WARN" "(Çalışmıyor)"
    fi
    echo ""
}

check_disk() {
    echo -e "${BLUE}▶ Disk Alanı${NC}"
    
    local usage
    usage=$(df -h / | awk 'NR==2 {print $5}' | tr -d '%')
    
    if [ "$usage" -lt 70 ]; then
        check_service "Root (/)" "OK" "(${usage}% kullanılıyor)"
    elif [ "$usage" -lt 85 ]; then
        check_service "Root (/)" "WARN" "(${usage}% kullanılıyor)"
    else
        check_service "Root (/)" "FAIL" "(${usage}% kullanılıyor - DİKKAT!)"
    fi
    
    # Docker volumes
    if command -v docker &> /dev/null; then
        local docker_size
        docker_size=$(docker system df --format '{{.Size}}' 2>/dev/null | head -1)
        if [ -n "$docker_size" ]; then
            check_service "Docker" "OK" "($docker_size)"
        fi
    fi
    echo ""
}

check_system() {
    echo -e "${BLUE}▶ Sistem Kaynakları${NC}"
    
    # CPU
    local cpu_load
    cpu_load=$(uptime | awk -F'load average:' '{print $2}' | awk -F',' '{print $1}' | tr -d ' ')
    check_service "CPU Load" "OK" "($cpu_load)"
    
    # Memory
    local mem_used
    local mem_total
    mem_used=$(free -m | awk 'NR==2 {print $3}')
    mem_total=$(free -m | awk 'NR==2 {print $2}')
    local mem_percent=$((mem_used * 100 / mem_total))
    
    if [ "$mem_percent" -lt 70 ]; then
        check_service "Bellek" "OK" "(${mem_used}MB / ${mem_total}MB - ${mem_percent}%)"
    elif [ "$mem_percent" -lt 85 ]; then
        check_service "Bellek" "WARN" "(${mem_used}MB / ${mem_total}MB - ${mem_percent}%)"
    else
        check_service "Bellek" "FAIL" "(${mem_used}MB / ${mem_total}MB - ${mem_percent}%)"
    fi
    
    # Uptime
    local uptime_str
    uptime_str=$(uptime -p)
    check_service "Uptime" "OK" "($uptime_str)"
    echo ""
}

# Ana işlem
print_header
check_app
check_database
check_redis
check_nginx
check_monitoring
check_disk
check_system

echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}Sağlık kontrolü tamamlandı.${NC}"
