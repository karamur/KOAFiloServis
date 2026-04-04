# CRM Filo Servis - Son Kullanıcı Kurulum Rehberi

## 📋 Gereksinimler

### Windows Kurulumu
- Windows 10/11 veya Windows Server 2019+
- .NET 10 Runtime
- SQLite (dahili) veya PostgreSQL 15+

### Docker Kurulumu
- Docker Desktop veya Docker Engine
- Docker Compose v2+

---

## 🚀 Hızlı Kurulum

### Seçenek 1: Windows Kurulumu

1. **Uygulamayı indirin ve çıkarın**
   ```powershell
   Expand-Archive -Path CRMFiloServis-Setup.zip -DestinationPath C:\CRMFiloServis
   ```

2. **Kurulum betiğini çalıştırın**
   ```powershell
   cd C:\CRMFiloServis
   .\install.ps1
   ```

3. **Servisi başlatın**
   ```powershell
   .\start.ps1
   ```

4. **Tarayıcıda açın**
   ```
   http://localhost:5190
   ```

### Seçenek 2: Docker Kurulumu

1. **Docker Compose ile başlatın**
   ```bash
   docker-compose up -d
   ```

2. **Tarayıcıda açın**
   ```
   http://localhost:5190
   ```

---

## ⚙️ Yapılandırma

### Veritabanı Ayarları

`appsettings.json` dosyasında veritabanı bağlantısını yapılandırın:

**SQLite (Varsayılan):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=crm_filo.db"
  },
  "DatabaseProvider": "SQLite"
}
```

**PostgreSQL:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=crmfilo;Username=postgres;Password=your_password"
  },
  "DatabaseProvider": "PostgreSQL"
}
```

### Lisans Aktivasyonu

İlk çalıştırmada lisans dosyası istenecektir:
1. Lisans dosyanızı (`license.key`) uygulama dizinine kopyalayın
2. Uygulamayı yeniden başlatın

---

## 🔧 Servis Yönetimi

### Windows Servisi Olarak Kurulum
```powershell
.\install-service.ps1
```

### Servis Komutları
```powershell
# Başlat
Start-Service CRMFiloServis

# Durdur
Stop-Service CRMFiloServis

# Durum
Get-Service CRMFiloServis
```

### Docker Komutları
```bash
# Başlat
docker-compose up -d

# Durdur
docker-compose down

# Logları görüntüle
docker-compose logs -f
```

---

## 📊 Varsayılan Giriş Bilgileri

| Kullanıcı | Şifre | Rol |
|-----------|-------|-----|
| admin | Admin123! | Yönetici |

⚠️ **İlk girişten sonra şifreyi mutlaka değiştirin!**

---

## 🔍 Sorun Giderme

### Uygulama başlamıyor
1. .NET 10 Runtime'ın yüklü olduğunu kontrol edin
2. Port 5190'ın kullanılabilir olduğunu kontrol edin
3. Logları inceleyin: `logs/` klasörü

### Veritabanı bağlantı hatası
1. Connection string'i kontrol edin
2. Veritabanı sunucusunun çalıştığını doğrulayın
3. Firewall ayarlarını kontrol edin

### Lisans hatası
1. `license.key` dosyasının uygulama dizininde olduğunu kontrol edin
2. Lisans süresinin dolmadığını doğrulayın
3. Destek ile iletişime geçin

---

## 📞 Destek

- 📧 E-posta: info@allglb.com
- 📱 Telefon: +90 XXX XXX XX XX
- 🌐 Web: http://www.allglb.com

---

**Versiyon:** 1.0.0  
**Son Güncelleme:** Nisan 2026
