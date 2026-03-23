# CRM Filo Servis - Kurulum Kilavuzu

## Sistem Gereksinimleri

### Yazilim Gereksinimleri
- **.NET 10 SDK** veya uzeri
- **PostgreSQL 14+** veritabani
- **Visual Studio 2022** veya **VS Code**
- Modern web tarayici (Chrome, Firefox, Edge)

### Donanim Gereksinimleri (Minimum)
- **CPU:** 2 Core
- **RAM:** 4 GB
- **Disk:** 10 GB bos alan

### Donanim Gereksinimleri (Onerilen)
- **CPU:** 4 Core
- **RAM:** 8 GB
- **Disk:** 50 GB SSD

---

## Adim 1: PostgreSQL Kurulumu

### Windows
1. [PostgreSQL indir](https://www.postgresql.org/download/windows/)
2. Kurulum sihirbazini calistir
3. Sifre belirle (ornek: `postgres123`)
4. Port: `5432` (varsayilan)
5. Kurulum tamamlaninca pgAdmin acilir

### Veritabani Olusturma
```sql
CREATE DATABASE crmfiloservis;
```

---

## Adim 2: .NET 10 SDK Kurulumu

1. [.NET 10 SDK indir](https://dotnet.microsoft.com/download/dotnet/10.0)
2. Kurulum sihirbazini calistir
3. Kurulumu dogrula:
```bash
dotnet --version
```

---

## Adim 3: Projeyi Indirme

### Git ile
```bash
git clone https://github.com/karamur/CRMFiloServis.git
cd CRMFiloServis
```

### ZIP ile
1. GitHub sayfasindan ZIP indir
2. Istediginiz klasore cikartin

---

## Adim 4: Veritabani Baglantisi Ayarlama

`CRMFiloServis.Web/appsettings.json` dosyasini acin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=crmfiloservis;Username=postgres;Password=SIFRENIZ"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**SIFRENIZ** yerine PostgreSQL sifrenizi yazin.

---

## Adim 5: Veritabani Migration

Terminal/Komut satirindan:

```bash
cd CRMFiloServis.Web
dotnet ef database update
```

> **Not:** EF Core Tools kurulu degilse:
> ```bash
> dotnet tool install --global dotnet-ef
> ```

---

## Adim 6: Uygulamayi Calistirma

### Visual Studio ile
1. `CRMFiloServis.sln` dosyasini acin
2. `F5` tusuna basin veya "Start Debugging" tiklayin

### Komut Satirindan
```bash
cd CRMFiloServis.Web
dotnet run
```

### Varsayilan Adres
```
http://localhost:5190
```

---

## Adim 7: Ilk Giris

### Varsayilan Kullanici Bilgileri
- **Kullanici Adi:** `admin`
- **Sifre:** `admin123`

### Test Kullanicisi (Hizli Giris)
- **Kullanici Adi:** `test`
- **Sifre:** `test123`

> **Onemli:** Uretim ortaminda bu sifreleri mutlaka degistirin!

---

## Lisans Aktivasyonu

1. **Ayarlar > Lisans** sayfasina gidin
2. **Lisans Anahtari** alana lisans kodunu girin
3. **Aktive Et** butonuna tiklayin

### Trial Lisans
- 30 gun gecerli
- Tum ozellikler acik
- 5 kullanici limiti

---

## Sorun Giderme

### Veritabani Baglanti Hatasi
```
Npgsql.NpgsqlException: Connection refused
```
**Cozum:**
1. PostgreSQL servisinin calistigini kontrol edin
2. Baglanti bilgilerini kontrol edin
3. Firewall ayarlarini kontrol edin

### Migration Hatasi
```
No migrations were applied
```
**Cozum:**
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Port Cakismasi
```
System.IO.IOException: Address already in use
```
**Cozum:**
`launchSettings.json` dosyasinda portu degistirin:
```json
"applicationUrl": "http://localhost:5191"
```

---

## Uretim Ortami Kurulumu

### IIS Kurulumu (Windows Server)
1. .NET Hosting Bundle yukleyin
2. IIS'te yeni site olusturun
3. Application Pool: "No Managed Code"
4. Publish klasorunu site dizinine kopyalayin

### Docker ile Kurulum
```bash
docker-compose up -d
```

### Nginx ile (Linux)
```nginx
server {
    listen 80;
    server_name crm.sirketiniz.com;

    location / {
        proxy_pass http://localhost:5190;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
    }
}
```

---

## Yedekleme

### Veritabani Yedegi
```bash
pg_dump -U postgres -d crmfiloservis > backup_$(date +%Y%m%d).sql
```

### Geri Yukleme
```bash
psql -U postgres -d crmfiloservis < backup_20240101.sql
```

### Otomatik Yedekleme
Uygulama icerisinden:
**Ayarlar > Yedekleme** sayfasindan otomatik yedekleme ayarlayabilirsiniz.

---

## Destek ve Iletisim

- **GitHub Issues:** https://github.com/karamur/CRMFiloServis/issues
- **Dokumantasyon:** https://github.com/karamur/CRMFiloServis/wiki

---

## Surum Gecmisi

### v1.1.0 (2024)
- Hizli giris (Test modu)
- Cari filtreleme gelistirmeleri
- Odeme yonetimi sayfasi
- Kurulum kilavuzu

### v1.0.0 (2024)
- Ilk surum
- Tum temel moduller
