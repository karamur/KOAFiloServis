-- CRM Filo Servis - PostgreSQL Veritabanı Başlatma Scripti

-- Uzantılar
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- Türkçe collation desteği
-- ALTER DATABASE crmfilo SET lc_collate = 'tr_TR.UTF-8';

-- Yorum: EF Core migration'lar tabloları otomatik oluşturacaktır.
-- Bu dosya sadece PostgreSQL ek ayarları için kullanılır.

-- Performans ayarları için önerilen indexler migration sonrası eklenecektir.

SELECT 'CRM Filo Servis veritabani hazir.' as status;
