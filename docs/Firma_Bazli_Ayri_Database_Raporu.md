# Firma Bazlı Ayrı Database Mimarisine Geçiş — Analiz ve Yol Haritası Raporu

**Tarih:** 20 Mayıs 2026
**Proje:** KOAFiloServis (V1) & KOAFiloServisV2
**Konu:** Her firma için ayrı PostgreSQL database kullanımı (Database-per-Tenant)

---

## 1. Mevcut Durum (Shared Database + FirmaId)

Her iki projede de şu anda **tek veritabanı, satır bazlı tenant izolasyonu** (shared-database/shared-schema) kullanılmaktadır.

### 1.1. Ortak Mimari

```
┌──────────────────────────────────────┐
│  PostgreSQL (tek database)            │
│  ├── Firmalar (tenant'ların kendisi)   │
│  ├── Cariler       (FirmaId=1,2,3..)  │
│  ├── Kurumlar      (FirmaId=1,2,3..)  │
│  ├── Araclar       (FirmaId=1,2,3..)  │
│  ├── ... (tüm tablolar aynı DB'de)    │
│  └── Kullanicilar  (tenant üstü)      │
└──────────────────────────────────────┘
         ↑
    Tek Connection String
    "Host=localhost;Database=KOAFiloServisV2;..."
```

### 1.2. Mevcut Tenant Mekanizması

| Bileşen | V1 (KOAFiloServis) | V2 (KOAFiloServisV2) |
|---------|-------------------|---------------------|
| Arayüz | `IFirmaTenant` | `IFirmaTenant` |
| Filtre | EF Core Global Query Filter | EF Core Global Query Filter |
| Provider | `IAktifFirmaProvider` (Scoped) | `IAktifFirmaProvider` (Scoped) |
| Context Üretimi | `TenantAwareDbContextFactory` | `IAppDbContextFactory` |
| FirmaId Atama | `SaveChanges` override | `SaveChanges` override |
| Admin Tüm Firma | `FirmaTenantDisabled` flag | `_aktifFirmaId == null` |
| Connection String | Tek, sabit | Tek, sabit |

### 1.3. Mevcut Avantajlar ve Dezavantajlar

**Avantajlar:**
- Basit yönetim (tek connection string, tek migration)
- Firmalar arası geçiş anlık (sadece FirmaId değişir)
- Ortak tablolar (MuhasebeHesap, Kullanici) doğal olarak paylaşımlı
- Admin kullanıcılar tüm firmaları tek sorguda görebilir

**Dezavantajlar:**
- Veri izolasyonu yalnızca uygulama seviyesinde (query filter hatası = veri sızıntısı)
- Büyük firmalar küçük firmaların performansını etkiler
- Firma bazlı yedekleme/geri yükleme zor
- Firma bazlı scale edilemez (sharding yok)
- Yasal/compliance gereksinimlerini karşılamayabilir (verilerin fiziksel ayrımı)

---

## 2. Hedef Mimari (Database-per-Tenant)

```
┌─────────────────────────────────────────────────┐
│  PostgreSQL Server                               │
│                                                   │
│  ┌─────────────────────┐  ┌─────────────────────┐│
│  │ Database: firma_001  │  │ Database: firma_002  ││
│  │ ├── Cariler          │  │ ├── Cariler          ││
│  │ ├── Kurumlar         │  │ ├── Kurumlar         ││
│  │ ├── Araclar          │  │ ├── Araclar          ││
│  │ ├── PuantajKayitlari │  │ ├── PuantajKayitlari ││
│  │ └── ...              │  │ └── ...              ││
│  └─────────────────────┘  └─────────────────────┘│
│                                                   │
│  ┌─────────────────────────────────────────────┐ │
│  │ Database: KOAFiloServis_Master (ortak)       │ │
│  │ ├── Firmalar (tenant kayıtları)              │ │
│  │ ├── Kullanicilar (kullanıcı hesapları)        │ │
│  │ ├── MuhasebeHesaplari (ortak hesap planı)     │ │
│  │ └── Lisans (lisans bilgileri)                │ │
│  └─────────────────────────────────────────────┘│
└─────────────────────────────────────────────────┘
```

Her firma için **ayrı bir PostgreSQL database** açılır. Firma kaydı ve kullanıcı bilgileri **Master database**'te tutulur.

---

## 3. Yapılması Gereken Değişiklikler

### 3.1. Firma Entity'sine Connection String / Database Adı Ekleme

`Firma` entity'sine firmanın hangi database'te olduğunu belirten alan(lar) eklenmeli:

```csharp
public class Firma : BaseEntity
{
    // ... mevcut alanlar ...

    // YENİ ALANLAR
    public string DatabaseName { get; set; }    // Örn: "kofa_firma_001"
    public string? ConnectionString { get; set; } // Örn: tam connection string (opsiyonel)
    public bool DatabaseCreated { get; set; }     // Migration yapıldı mı?
}
```

### 3.2. Connection String Yönetimi (ConnectionStringProvider)

Mevcut sistemde tek bir connection string var. Database-per-tenant'ta **her firma için dinamik connection string** gerekir.

**Yapılacak:** `ITenantConnectionStringProvider` arayüzü:

```csharp
public interface ITenantConnectionStringProvider
{
    string GetConnectionString(int firmaId);
    string GetMasterConnectionString();
}
```

Bu provider:
- Master database'ten firmanın `DatabaseName` bilgisini okur
- Template connection string'in `Database=` kısmını değiştirir
- Connection pool'ları yönetir (her database için ayrı pool)

### 3.3. DbContextFactory Değişikliği

Mevcut `IAppDbContextFactory` (V2) / `TenantAwareDbContextFactory` (V1) değişmeli:

```csharp
public class TenantDbContextFactory : ITenantDbContextFactory
{
    private readonly ITenantConnectionStringProvider _connProvider;
    private readonly IAktifFirmaProvider _firmaProvider;

    public ApplicationDbContext Create()
    {
        var firmaId = _firmaProvider.AktifFirmaId;
        if (firmaId == null)
            return CreateMasterContext(); // Admin tüm firma modu

        var connStr = _connProvider.GetConnectionString(firmaId.Value);
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connStr)
            .Options;
        return new ApplicationDbContext(options);
    }

    public ApplicationDbContext CreateMasterContext()
    {
        var connStr = _connProvider.GetMasterConnectionString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connStr)
            .Options;
        return new ApplicationDbContext(options);
    }
}
```

### 3.4. Global Query Filter Kaldırma

Database-per-tenant mimarisinde **tenant filtresine gerek kalmaz** çünkü her firma zaten kendi database'inde. Bu şunları basitleştirir:

- `ApplyFirmaTenantQueryFilter()` metodu kaldırılır
- `AssignFirmaTenantId()` metodu kaldırılır (veya her kayda FirmaId=0 yazılır)
- `IFirmaTenant` interface'i opsiyonel hale gelir

**Ancak dikkat:** Geçiş sürecinde her iki modu da desteklemek istenirse `IFirmaTenant` korunabilir.

### 3.5. Firma Oluşturma Akışı

Yeni firma kaydı yapıldığında:

```
1. Admin "Yeni Firma" oluşturur
2. Master database'te Firma kaydı oluşturulur (DatabaseCreated = false)
3. Yeni PostgreSQL database'i oluşturulur:
   CREATE DATABASE kofa_firma_XXX OWNER postgres;
4. Migration'lar yeni database'e uygulanır:
   dotnet ef database update --connection "<yeni firma conn str>"
5. Seed data yeni database'e eklenir
6. Firma.DatabaseCreated = true
7. Firma.DatabaseName = "kofa_firma_XXX"
```

### 3.6. Migration Stratejisi

İki seçenek var:

| Yaklaşım | Açıklama | Avantaj | Dezavantaj |
|----------|---------|---------|------------|
| **A) Program.cs'te otomatik migration** | Her firma için `db.Database.Migrate()` | Basit, manuel müdahale yok | Startup yavaş |
| **B) Yönetim panelinden tetikleme** | Admin panelden "Migration Uygula" butonu | Kontrollü | Unutulabilir |

**Öneri:** İkisi birden — startup'ta master DB her zaman migrate edilir. Firma database'leri için `Database.MigrateAsync()` ilk bağlantıda otomatik çalışır (lazy migration).

### 3.7. Seed Data Yönetimi

Her yeni firma database'ine uygulanacak seed data:

- Muhasebe hesap planı (34 hesap)
- Varsayılan ayarlar
- Firma bazlı başlangıç verileri

Master database seed data:
- Admin kullanıcısı
- Lisans bilgisi

### 3.8. Cross-Firma İşlemleri

Bazı işlemler birden fazla firmayı ilgilendirir:

| İşlem | Çözüm |
|-------|-------|
| Admin tüm firmaları görme | Master DB'deki Firma listesi, detay için firma DB'sine bağlan |
| Raporlama (konsolide) | Her firma DB'sinden ayrı sorgu, uygulama tarafında birleştir |
| Firma kopyalama (K8) | Kaynak DB'den oku, hedef DB'ye yaz |
| Kullanıcı oturumu | Master DB'den kimlik doğrula, firma DB'sine yönlendir |

### 3.9. DI Kayıtları Değişikliği

```csharp
// Program.cs - V2 için örnek

// Master DB (Firmalar, Kullanicilar, Lisans)
builder.Services.AddDbContextFactory<MasterDbContext>(options =>
    options.UseNpgsql(masterConnectionString));

// Tenant DB Factory - her seferinde aktif firmaya göre yeni context
builder.Services.AddScoped<ITenantDbContextFactory, TenantDbContextFactory>();
builder.Services.AddScoped<ApplicationDbContext>(sp =>
{
    var factory = sp.GetRequiredService<ITenantDbContextFactory>();
    return factory.Create(); // Aktif firmaya göre doğru DB'ye bağlanır
});

// Connection string provider
builder.Services.AddSingleton<ITenantConnectionStringProvider, TenantConnectionStringProvider>();
```

### 3.10. IAktifFirmaProvider Değişikliği

Provider mevcut haliyle büyük ölçüde korunur. Tek fark: firma değiştiğinde yeni DbContext'in yeni database'e bağlanması gerekir. `AktifFirmaDegisti` event'i tetiklendiğinde DbContext yeniden oluşturulmalıdır.

---

## 4. Veritabanı ve Migration Planı

### 4.1. Master Database Şeması

Master database'te sadece tenant-üstü tablolar kalır:

| Tablo | Açıklama |
|-------|----------|
| `Firmalar` | Tüm firma kayıtları + `DatabaseName` alanı |
| `Kullanicilar` | Kullanıcı hesapları (FirmaId opsiyonel) |
| `Lisans` | Lisans bilgileri |
| `__EFMigrationsHistory` | Master DB migration geçmişi |

### 4.2. Tenant Database Şeması

Her firma database'i mevcut şemanın aynısıdır, tek fark:
- `Firmalar` tablosu **yok** (veya tek satırlık local cache)
- `IFirmaTenant` interface'i isteğe bağlı korunur
- Tenant filtresi uygulanmaz (zaten tek firma var)

### 4.3. Migration Komutları

```bash
# Master DB için
dotnet ef migrations add InitialMaster --context MasterDbContext -o Migrations/Master

# Tenant DB için
dotnet ef migrations add InitialTenant --context ApplicationDbContext -o Migrations/Tenant

# Yeni firma oluşturma script'i (örnek)
psql -h localhost -U postgres -c "CREATE DATABASE kofa_firma_$(FIRMA_ID) OWNER postgres;"
dotnet ef database update --context ApplicationDbContext --connection "Host=localhost;Database=kofa_firma_$(FIRMA_ID);..."
```

---

## 5. Riskler ve Zorluklar

| Risk | Şiddet | Çözüm |
|------|--------|-------|
| **Migration senkronizasyonu** | Yüksek | Tüm tenant DB'leri aynı migration seviyesinde olmalı. Otomatik `MigrateAsync()` ile çözülür. |
| **Connection pool patlaması** | Orta | Her tenant DB için ayrı pool. Çok fazla firma varsa (50+) pool sayısı artar. `MaxPoolSize` düşürülür. |
| **Cross-firma sorgu performansı** | Orta | Admin tüm firma raporları için paralel sorgu + uygulama tarafı aggregate. |
| **Yedekleme karmaşıklığı** | Düşük | Her DB ayrı yedeklenir. Otomatik script ile tüm tenant DB'leri döngüyle yedeklenir. |
| **Firma silme** | Orta | Firma silindiğinde database DROP edilir. Geri dönüşü yok. Soft-delete + manuel DROP önerilir. |
| **Connection string sızıntısı** | Düşük | Tenant connection string'leri master DB'de saklanır. Hassas bilgi içermez (aynı host/port, sadece database adı değişir). |

---

## 6. Uygulama Adımları (V2 Projesi için Önerilen Yol Haritası)

### Faz 1: Altyapı (3-5 gün)
1. `Firma` entity'sine `DatabaseName` alanı ekle
2. `MasterDbContext` oluştur (sadece Firma + Kullanici + Lisans)
3. `ITenantConnectionStringProvider` ve implementasyonunu yaz
4. `TenantDbContextFactory`'i yaz (IDbContextFactory yerine)
5. DI kayıtlarını güncelle
6. Master DB migration'ını oluştur

### Faz 2: Migration ve Geçiş (2-3 gün)
7. Tenant DB migration'ını oluştur (IFirmaTenant'siz, filtresiz)
8. Mevcut verileri aktarma script'i yaz (tek DB'deki verileri firma bazlı DB'lere böl)
9. `Firma.DatabaseName` alanını doldur
10. Eski single-DB modunu "legacy" olarak işaretle

### Faz 3: Test ve Geçiş (2-3 gün)
11. Her endpoint'in doğru DB'ye bağlandığını test et
12. Firma oluşturma → DB oluşturma → migration → seed akışını test et
13. Admin tüm firma görüntüleme testi
14. Cross-firma kopyalama testi
15. Firma değiştirme (geçiş) testi

### Faz 4: Deployment (1-2 gün)
16. Docker Compose güncellemesi (master + tenant DB tanımları)
17. Release notları
18. Production geçiş planı

---

## 7. V1 vs V2 — Hangi Projede Uygulanmalı?

| Kriter | V1 (KOAFiloServis) | V2 (KOAFiloServisV2) |
|--------|-------------------|---------------------|
| Kod temizliği | Karmaşık, 130+ servis | Temiz, 11 servis |
| Entity sayısı | 70+ entity | 24 entity |
| Tenant geçiş durumu | Devam ediyor (yarım) | Tamamlanmış, temiz |
| Migration sayısı | 10+ migration | 6 migration |
| Risk | Yüksek (çok fazla bağımlılık) | Düşük (sade ve yönetilebilir) |
| Öneri | V1'de uygulama ZOR | **V2'de uygulama KOLAY** |

**Öneri:** Bu değişiklik V2 projesinde uygulanmalıdır. V1 mevcut haliyle kullanılmaya devam ederken, V2 database-per-tenant mimarisiyle yükseltilir. V1'den V2'ye geçiş yapıldığında firmalar yeni mimariye taşınmış olur.

---

## 8. V2 Projesinde Etkilenecek Dosyalar (Özet)

```
DEĞİŞECEK:
├── src/KOAFiloServisV2.Domain/
│   ├── Entities/Firma.cs                    ← DatabaseName, ConnectionString ekle
│   └── Interfaces/IFirmaTenant.cs           ← Opsiyonel kalabilir
├── src/KOAFiloServisV2.Infrastructure/
│   ├── Data/ApplicationDbContext.cs          ← Tenant filtresi kaldır, tenant DB'ye bağlan
│   ├── Data/IAppDbContextFactory.cs          ← ITenantDbContextFactory olarak güncelle
│   ├── Data/AppDbContextFactory.cs           ← Dinamik connection string ile yeniden yaz
│   ├── Data/SeedData.cs                     ← Master + Tenant seed ayır
│   └── DependencyInjection.cs               ← DI kayıtlarını güncelle
├── src/KOAFiloServisV2.Web/
│   ├── Program.cs                           ← DI yapılandırması
│   ├── appsettings.json                     ← Master + template connection string
│   └── Services/AktifFirmaProvider.cs       ← Minör güncelleme

YENİ DOSYALAR:
├── src/KOAFiloServisV2.Infrastructure/
│   ├── Data/MasterDbContext.cs              ← Master DB context
│   ├── Data/TenantDbContextFactory.cs        ← Tenant DB factory
│   ├── Data/ITenantConnectionStringProvider.cs
│   └── Data/TenantConnectionStringProvider.cs
├── src/KOAFiloServisV2.Infrastructure/
│   └── Migrations/Master/                   ← Master migration'lar
└── src/KOAFiloServisV2.Infrastructure/
    └── Migrations/Tenant/                   ← Tenant migration'lar (filtresiz)
```

---

## 9. Alternatif Yaklaşımlar

### 9.1. Hybrid (Önerilen Ara Geçiş)

Hem shared-DB hem database-per-tenant modunu aynı anda desteklemek:

- `Firma.DatabaseName == null` → Eski mod (shared DB, FirmaId filtresi)
- `Firma.DatabaseName != null` → Yeni mod (ayrı DB, filtre yok)

Bu sayede kademeli geçiş yapılabilir, tüm firmalar aynı anda taşınmak zorunda kalmaz.

### 9.2. PostgreSQL Schema-per-Tenant

Database-per-tenant'a alternatif: Her firma için ayrı PostgreSQL **schema**'sı (örn: `firma_001.Cariler`, `firma_002.Cariler`).

- Avantaj: Tek database, tek connection pool, kolay cross-firma sorgu
- Dezavantaj: Fiziksel izolasyon yok, schema sayısı arttıkça yönetim zorlaşır

---

## 10. Sonuç ve Öneri

**Önerilen yaklaşım:** V2 projesinde **Hybrid model** ile database-per-tenant'a kademeli geçiş.

- Kısa vadede (1-2 hafta): Altyapı kurulur, yeni firmalar ayrı DB'de açılır
- Orta vadede: Mevcut firmalar taşınır
- Uzun vadede: Shared-DB modu tamamen kaldırılır, `IFirmaTenant` ve tenant filtresi kod tabanından çıkarılır

**Toplam tahmini efor:** 10-15 iş günü
