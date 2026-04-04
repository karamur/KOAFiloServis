#!/bin/bash
# CRM Filo Servis - Veritabanı Yedekleme Scripti

set -e

# Yapılandırma
BACKUP_DIR="/backups"
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="crmfilo_backup_${DATE}.sql"
RETENTION_DAYS=30

echo "[$(date)] Yedekleme başlatılıyor..."

# Yedekleme dizinini oluştur
mkdir -p ${BACKUP_DIR}

# PostgreSQL yedekleme
pg_dump -Fc > "${BACKUP_DIR}/${BACKUP_FILE}.gz"

# Yedekleme boyutunu kontrol et
BACKUP_SIZE=$(du -h "${BACKUP_DIR}/${BACKUP_FILE}.gz" | cut -f1)
echo "[$(date)] Yedekleme tamamlandı: ${BACKUP_FILE}.gz (${BACKUP_SIZE})"

# Eski yedekleri temizle
echo "[$(date)] ${RETENTION_DAYS} günden eski yedekler temizleniyor..."
find ${BACKUP_DIR} -name "crmfilo_backup_*.sql.gz" -mtime +${RETENTION_DAYS} -delete

# Kalan yedek sayısı
BACKUP_COUNT=$(ls -1 ${BACKUP_DIR}/crmfilo_backup_*.sql.gz 2>/dev/null | wc -l)
echo "[$(date)] Toplam yedek sayısı: ${BACKUP_COUNT}"

echo "[$(date)] İşlem tamamlandı."
