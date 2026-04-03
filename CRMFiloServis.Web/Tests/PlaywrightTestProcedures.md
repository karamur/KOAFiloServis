# Playwright Test Procedures

## Amaç
Bu prosedürler, `CRMFiloServis.Web` uygulamasında kullanıcı girişini, public landing sayfasını ve personel ekranı erişimini `Playwright` ile doğrulamak için hazırlanmıştır.

## Kapsam
`CRMFiloServis.PlaywrightSmoke` altındaki smoke testler şu akışları kontrol eder:

1. Anonim kullanıcı `dashboard` açınca `login` sayfasına yönlenir.
2. Geçerli kullanıcı ile giriş yapılabilir.
3. `Personel` sayfası açılabilir.
4. Public landing sayfası (`/`) beklenen kurumsal içeriği gösterir.

## Varsayılan test kullanıcısı
- Kullanıcı adı: `admin`
- Şifre: `admin123`

## Çalıştırma
Uygulama çalışırken aşağıdaki komut kullanılabilir:

```powershell
dotnet run --project CRMFiloServis.PlaywrightSmoke\CRMFiloServis.PlaywrightSmoke.csproj -- http://127.0.0.1:5190
```

## Ortam değişkenleri
İstenirse test kullanıcı bilgileri ortam değişkenleriyle verilebilir:

- `CRMFILO_BASE_URL`
- `CRMFILO_TEST_USER`
- `CRMFILO_TEST_PASSWORD`

## Playwright kurulumu
İlk çalıştırmadan önce gerekirse tarayıcı paketlerini yükleyin:

```powershell
pwsh bin\Debug\net10.0\playwright.ps1 install
```

veya paket geri yükleme sonrasında Playwright aracı üzerinden kurulum yapın.

## Not
Eski `Selenium` prosedürleri yerine bu akış kullanılmalıdır.
