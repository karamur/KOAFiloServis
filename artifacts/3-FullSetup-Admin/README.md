# CRM Filo Servis - Tam Kurulum Rehberi (Yönetim Paneli Dahil)

## 📋 Bu Paket Neleri İçerir?

- ✅ Ana uygulama (Son kullanıcı özellikleri)
- ✅ Yönetim paneli (`/admin`)
- ✅ Lisans yönetimi
- ✅ Multi-tenant desteği
- ✅ API erişimi
- ✅ Gelişmiş raporlama
- ✅ Özelleştirme seçenekleri
- ✅ Kaynak kod (sadece Enterprise lisans)

---

## 🚀 Kurulum Adımları

### 1. Gereksinimler

```
- Windows Server 2019+ veya Ubuntu 20.04+
- .NET 10 SDK
- PostgreSQL 15+ (önerilen) veya SQLite
- Node.js 18+ (web yönetim paneli için)
- Redis (opsiyonel - cache için)
```

### 2. Kaynak Koddan Derleme

```bash
# Repo'yu klonlayın
git clone https://github.com/karamur/CRMFiloServis.git
cd CRMFiloServis

# Bağımlılıkları yükleyin
dotnet restore

# Release olarak derleyin
dotnet publish -c Release -o ./publish

# Veritabanı migration
cd publish
dotnet CRMFiloServis.Web.dll --migrate
```

### 3. Yapılandırma

`appsettings.Production.json` dosyasını düzenleyin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=crmfilo;Username=postgres;Password=SecurePass123!"
  },
  "DatabaseProvider": "PostgreSQL",
  "AdminPanel": {
    "Enabled": true,
    "RequireAuth": true,
    "AllowedIPs": ["127.0.0.1", "192.168.1.0/24"]
  },
  "Redis": {
    "Enabled": true,
    "ConnectionString": "localhost:6379"
  },
  "Licensing": {
    "Mode": "Server",
    "ServerUrl": "https://license.crmfiloservis.com"
  }
}
```

### 4. Servisi Başlatın

```bash
# Linux systemd servisi olarak
sudo cp crmfiloservis.service /etc/systemd/system/
sudo systemctl enable crmfiloservis
sudo systemctl start crmfiloservis

# Windows servisi olarak
.\install-service.ps1
```

---

## 🔐 Yönetim Paneli

### Erişim
```
URL: http://your-server:5190/admin
```

### Varsayılan Yönetici
| Kullanıcı | Şifre | Rol |
|-----------|-------|-----|
| superadmin | SuperAdmin2026! | Süper Yönetici |

⚠️ **İlk girişte şifreyi değiştirin!**

### Yönetim Özellikleri

#### Kullanıcı Yönetimi
- Kullanıcı ekleme/düzenleme/silme
- Rol ve yetki yönetimi
- Oturum takibi
- IP kısıtlamaları

#### Lisans Yönetimi
- Lisans durumu görüntüleme
- Yeni lisans aktivasyonu
- Lisans geçmişi
- Kullanım istatistikleri

#### Sistem Yönetimi
- Veritabanı yedekleme/geri yükleme
- Log görüntüleme
- Sistem sağlık durumu
- Performans metrikleri

#### Multi-Tenant (Enterprise)
- Tenant ekleme/yönetme
- Tenant bazlı konfigürasyon
- Veri izolasyonu

---

## 🌐 Web Sayfası Kurulumu

### Marketing/Landing Page

```bash
cd web-landing
npm install
npm run build
```

Çıktı: `web-landing/dist/` dizinine kopyalanır.

### Nginx Yapılandırması

```nginx
server {
    listen 80;
    server_name www.crmfiloservis.com;
    
    # Landing page
    location / {
        root /var/www/crmfiloservis-landing;
        index index.html;
        try_files $uri $uri/ /index.html;
    }
    
    # Uygulama
    location /app {
        proxy_pass http://localhost:5190;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
    
    # Admin panel
    location /admin {
        proxy_pass http://localhost:5190/admin;
        # IP kısıtlaması
        allow 192.168.1.0/24;
        deny all;
    }
}
```

---

## 📊 API Kullanımı

### API Anahtarı Oluşturma
1. Admin panele giriş yapın
2. Ayarlar > API Anahtarları
3. "Yeni Anahtar" butonuna tıklayın
4. Yetkileri seçin ve oluşturun

### Örnek İstek
```bash
curl -X GET "http://localhost:5190/api/v1/vehicles" \
  -H "Authorization: Bearer YOUR_API_KEY" \
  -H "Content-Type: application/json"
```

### API Dokümantasyonu
```
Swagger UI: http://localhost:5190/swagger
OpenAPI Spec: http://localhost:5190/swagger/v1/swagger.json
```

---

## 🔄 Yedekleme ve Geri Yükleme

### Otomatik Yedekleme
```json
// appsettings.json
{
  "Backup": {
    "Enabled": true,
    "Schedule": "0 2 * * *",  // Her gece 02:00
    "RetentionDays": 30,
    "Path": "/backups/",
    "IncludeLogs": false
  }
}
```

### Manuel Yedekleme
```bash
# Admin panelden
http://localhost:5190/admin/backup

# Komut satırından
dotnet CRMFiloServis.Web.dll --backup

# PostgreSQL direkt
pg_dump -U postgres crmfilo > backup_$(date +%Y%m%d).sql
```

### Geri Yükleme
```bash
# Admin panelden
http://localhost:5190/admin/restore

# Komut satırından
dotnet CRMFiloServis.Web.dll --restore backup_20260401.zip

# PostgreSQL direkt
psql -U postgres crmfilo < backup_20260401.sql
```

---

## 📈 Monitöring

### Health Check Endpoints
```
/health         - Genel durum
/health/ready   - Hazırlık durumu
/health/live    - Canlılık durumu
```

### Prometheus Metrikleri
```
/metrics
```

### Grafana Dashboard
`monitoring/grafana-dashboard.json` dosyasını import edin.

---

## 🛠️ Sorun Giderme

### Log Dosyaları
```bash
# Uygulama logları
tail -f /var/log/crmfiloservis/app.log

# Sistem logları (systemd)
journalctl -u crmfiloservis -f
```

### Sık Karşılaşılan Sorunlar

#### Admin panele erişilemiyor
1. `AdminPanel.Enabled: true` olduğunu kontrol edin
2. IP adresinizin `AllowedIPs` listesinde olduğunu doğrulayın
3. Firewall ayarlarını kontrol edin

#### Lisans hatası
1. `license.key` dosyasının doğru konumda olduğunu kontrol edin
2. Lisans süresini doğrulayın
3. Hardware ID değişmişse yeni lisans talep edin

#### Veritabanı bağlantı hatası
1. PostgreSQL servisinin çalıştığını kontrol edin
2. Connection string'i doğrulayın
3. Firewall ve pg_hba.conf ayarlarını kontrol edin

---

## 📞 Destek

- 📧 E-posta: enterprise@crmfiloservis.com
- 📱 Acil Destek: +90 XXX XXX XX XX
- 🎫 Ticket: https://support.crmfiloservis.com
- 📖 Dokümantasyon: https://docs.crmfiloservis.com

---

**Versiyon:** 1.0.0  
**Son Güncelleme:** Nisan 2026  
**Lisans:** Enterprise
