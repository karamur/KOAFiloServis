# CRM Filo Servis - Lisans Sistemi

## 📜 Lisans Türleri

| Lisans | Kullanıcı | Araç | Süre | Özellikler |
|--------|-----------|------|------|------------|
| **Trial** | 2 | 10 | 30 gün | Temel özellikler |
| **Starter** | 5 | 50 | 1 yıl | Temel + Raporlar |
| **Professional** | 20 | 200 | 1 yıl | Tüm özellikler |
| **Enterprise** | Sınırsız | Sınırsız | 1 yıl | Tüm + API + Özelleştirme |

---

## 🔑 Lisans Aktivasyonu

### Adım 1: Lisans Dosyasını Kopyalayın
```bash
# Windows
copy license.key C:\CRMFiloServis\

# Linux/Docker
cp license.key /app/
```

### Adım 2: Uygulamayı Yeniden Başlatın
```bash
# Windows Servisi
Restart-Service CRMFiloServis

# Docker
docker-compose restart
```

### Adım 3: Lisansı Doğrulayın
Tarayıcıda: `http://localhost:5190/admin/license`

---

## 📝 Lisans Dosyası Formatı

```
-----BEGIN LICENSE-----
LicenseId: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
Type: Professional
Company: Şirket Adı
MaxUsers: 20
MaxVehicles: 200
IssuedDate: 2026-01-01
ExpiryDate: 2027-01-01
Features: All
Signature: [encrypted_signature]
-----END LICENSE-----
```

---

## ⚠️ Önemli Notlar

1. **Lisans dosyası taşınamaz** - Her kurulum için ayrı lisans gerekir
2. **Hardware ID** değişirse lisans geçersiz olur
3. **Lisans yenileme** son kullanma tarihinden 30 gün önce hatırlatılır
4. **Deneme süresi** uzatılamaz

---

## 🔄 Lisans Yenileme

1. Mevcut lisans bilgilerinizi `http://localhost:5190/admin/license` adresinden alın
2. Yenileme talebini destek@crmfiloservis.com adresine gönderin
3. Yeni lisans dosyasını alıp kuruluma kopyalayın

---

## 🛡️ Lisans Koruma

- Lisans dosyası şifrelenmiştir
- Değiştirme tespit edilir ve lisans geçersiz olur
- Çoklu kurulum tespitinde tüm lisanslar devre dışı bırakılır

---

## 📞 Destek

Lisans sorunları için:
- 📧 info@allglb.com
- 📱 +90 XXX XXX XX XX
