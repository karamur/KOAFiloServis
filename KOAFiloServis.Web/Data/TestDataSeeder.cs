using KOAFiloServis.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace KOAFiloServis.Web.Data;

/// <summary>
/// Demo ve test amaçlı örnek veri oluşturma servisi
/// </summary>
public class TestDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TestDataSeeder> _logger;
    private readonly Random _random = new();

    // Türkçe isimler
    private readonly string[] _erkekAdlari = { "Ahmet", "Mehmet", "Ali", "Mustafa", "Hüseyin", "İbrahim", "Hasan", "Osman", "Yusuf", "Kemal", "Murat", "Emre", "Burak", "Serkan", "Fatih" };
    private readonly string[] _kadinAdlari = { "Fatma", "Ayşe", "Zeynep", "Emine", "Hatice", "Elif", "Merve", "Büşra", "Seda", "Esra" };
    private readonly string[] _soyadlari = { "Yılmaz", "Kaya", "Demir", "Şahin", "Çelik", "Yıldız", "Arslan", "Koç", "Aydın", "Özdemir", "Kurt", "Aslan", "Erdoğan", "Kılıç", "Polat" };

    // İstanbul ilçeleri
    private readonly string[] _ilceler = { "Kadıköy", "Beşiktaş", "Şişli", "Bakırköy", "Ataşehir", "Üsküdar", "Maltepe", "Kartal", "Pendik", "Tuzla", "Beylikdüzü", "Esenyurt", "Başakşehir", "Sarıyer", "Beykoz" };

    public TestDataSeeder(ApplicationDbContext context, ILogger<TestDataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Tüm örnek verileri oluşturur
    /// </summary>
    public async Task<TestDataResult> SeedAllAsync(bool silinenleriTemizle = false)
    {
        var result = new TestDataResult();

        try
        {
            if (silinenleriTemizle)
            {
                await TemizleAsync();
                result.Mesajlar.Add("Mevcut test verileri temizlendi");
            }

            // Sırayla oluştur (bağımlılıklar nedeniyle)
            result.CariSayisi = await SeedCarilerAsync();
            result.SoforSayisi = await SeedSoforlerAsync();
            result.AracSayisi = await SeedAraclarAsync();
            result.GuzergahSayisi = await SeedGuzergahlarAsync();
            result.FaturaSayisi = await SeedFaturalarAsync();
            result.ServisCalismasiSayisi = await SeedServisCalismalarıAsync();

            result.Basarili = true;
            result.Mesajlar.Add($"Toplam {result.ToplamKayit} örnek kayıt oluşturuldu");

            _logger.LogInformation("Test verileri oluşturuldu: {ToplamKayit} kayıt", result.ToplamKayit);
        }
        catch (Exception ex)
        {
            result.Basarili = false;
            result.Mesajlar.Add($"Hata: {ex.Message}");
            _logger.LogError(ex, "Test verileri oluşturulurken hata");
        }

        return result;
    }

    /// <summary>
    /// Test verilerini temizler (sadece test verisi olarak işaretlenenler)
    /// </summary>
    public async Task TemizleAsync()
    {
        // Servis çalışmaları
        var servisler = await _context.ServisCalismalari
            .Where(x => x.Notlar != null && x.Notlar.Contains("[TEST]"))
            .ToListAsync();
        _context.ServisCalismalari.RemoveRange(servisler);

        // Faturalar ve kalemleri
        var faturalar = await _context.Faturalar
            .Include(x => x.FaturaKalemleri)
            .Where(x => x.Aciklama != null && x.Aciklama.Contains("[TEST]"))
            .ToListAsync();
        foreach (var fatura in faturalar)
        {
            _context.FaturaKalemleri.RemoveRange(fatura.FaturaKalemleri);
        }
        _context.Faturalar.RemoveRange(faturalar);

        // Güzergahlar
        var guzergahlar = await _context.Guzergahlar
            .Where(x => x.Notlar != null && x.Notlar.Contains("[TEST]"))
            .ToListAsync();
        _context.Guzergahlar.RemoveRange(guzergahlar);

        // Araçlar
        var araclar = await _context.Araclar
            .Where(x => x.Notlar != null && x.Notlar.Contains("[TEST]"))
            .ToListAsync();
        _context.Araclar.RemoveRange(araclar);

        // Şoförler
        var soforler = await _context.Soforler
            .Where(x => x.Notlar != null && x.Notlar.Contains("[TEST]"))
            .ToListAsync();
        _context.Soforler.RemoveRange(soforler);

        // Cariler
        var cariler = await _context.Cariler
            .Where(x => x.Notlar != null && x.Notlar.Contains("[TEST]"))
            .ToListAsync();
        _context.Cariler.RemoveRange(cariler);

        await _context.SaveChangesAsync();
        _logger.LogInformation("Test verileri temizlendi");
    }

    #region Cari Seed

    private async Task<int> SeedCarilerAsync()
    {
        if (await _context.Cariler.AnyAsync(c => c.Notlar != null && c.Notlar.Contains("[TEST]")))
            return 0;

        var firma = await _context.Firmalar.FirstOrDefaultAsync();
        var cariler = new List<Cari>();

        // 10 Müşteri
        var musteriAdlari = new[] { "ABC Lojistik", "XYZ Taşımacılık", "Mega Transport", "Hızlı Kargo", "Güven Nakliyat", 
            "Star Ulaşım", "Yıldız Servis", "İstanbul Filo", "Anadolu Transit", "Marmara Taşıma" };

        for (int i = 0; i < musteriAdlari.Length; i++)
        {
            cariler.Add(new Cari
            {
                FirmaId = firma?.Id,
                CariKodu = $"MUS{(i + 1):D4}",
                Unvan = musteriAdlari[i] + " A.Ş.",
                CariTipi = CariTipi.Musteri,
                VergiDairesi = "Kadıköy VD",
                VergiNo = $"{_random.Next(100, 999)}{_random.Next(1000000, 9999999)}",
                Adres = $"{_ilceler[i % _ilceler.Length]} / İstanbul",
                Telefon = $"0216 {_random.Next(100, 999)} {_random.Next(10, 99)} {_random.Next(10, 99)}",
                Email = $"info@{musteriAdlari[i].ToLower().Replace(" ", "")}.com.tr",
                YetkiliKisi = RastgeleIsim(),
                Aktif = true,
                Notlar = "[TEST] Demo müşteri verisi",
                CreatedAt = DateTime.Now.AddDays(-_random.Next(30, 365))
            });
        }

        // 5 Tedarikçi
        var tedarikciAdlari = new[] { "Petrol Ofisi", "BP Akaryakıt", "Shell Türkiye", "Oto Yedek Parça", "Lastik Dünyası" };

        for (int i = 0; i < tedarikciAdlari.Length; i++)
        {
            cariler.Add(new Cari
            {
                FirmaId = firma?.Id,
                CariKodu = $"TED{(i + 1):D4}",
                Unvan = tedarikciAdlari[i] + " Ltd. Şti.",
                CariTipi = CariTipi.Tedarikci,
                VergiDairesi = "Beşiktaş VD",
                VergiNo = $"{_random.Next(100, 999)}{_random.Next(1000000, 9999999)}",
                Adres = $"{_ilceler[(i + 5) % _ilceler.Length]} / İstanbul",
                Telefon = $"0212 {_random.Next(100, 999)} {_random.Next(10, 99)} {_random.Next(10, 99)}",
                Email = $"satis@{tedarikciAdlari[i].ToLower().Replace(" ", "")}.com.tr",
                YetkiliKisi = RastgeleIsim(),
                Aktif = true,
                Notlar = "[TEST] Demo tedarikçi verisi",
                CreatedAt = DateTime.Now.AddDays(-_random.Next(30, 365))
            });
        }

        _context.Cariler.AddRange(cariler);
        await _context.SaveChangesAsync();
        return cariler.Count;
    }

    #endregion

    #region Şoför Seed

    private async Task<int> SeedSoforlerAsync()
    {
        if (await _context.Soforler.AnyAsync(s => s.Notlar != null && s.Notlar.Contains("[TEST]")))
            return 0;

        var soforler = new List<Sofor>();

        for (int i = 0; i < 15; i++)
        {
            var erkek = _random.Next(100) < 90; // %90 erkek
            var ad = erkek ? _erkekAdlari[_random.Next(_erkekAdlari.Length)] : _kadinAdlari[_random.Next(_kadinAdlari.Length)];
            var soyad = _soyadlari[_random.Next(_soyadlari.Length)];

            soforler.Add(new Sofor
            {
                SoforKodu = $"SFR{(i + 1):D3}",
                Ad = ad,
                Soyad = soyad,
                TcKimlikNo = $"{_random.Next(10000, 99999)}{_random.Next(100000, 999999)}",
                Telefon = $"05{_random.Next(30, 59)} {_random.Next(100, 999)} {_random.Next(10, 99)} {_random.Next(10, 99)}",
                Email = $"{ad.ToLower()}.{soyad.ToLower()}@email.com",
                Adres = $"{_ilceler[_random.Next(_ilceler.Length)]} / İstanbul",
                Gorev = PersonelGorev.Sofor,
                EhliyetNo = $"{_random.Next(10, 99)}{ad.Substring(0, 2).ToUpper()}{_random.Next(10000, 99999)}",
                EhliyetGecerlilikTarihi = DateTime.Today.AddMonths(_random.Next(6, 60)),
                SrcBelgesiGecerlilikTarihi = _random.Next(100) < 80 ? DateTime.Today.AddMonths(_random.Next(6, 36)) : null,
                PsikoteknikGecerlilikTarihi = DateTime.Today.AddMonths(_random.Next(6, 24)),
                SaglikRaporuGecerlilikTarihi = DateTime.Today.AddMonths(_random.Next(6, 12)),
                IseBaslamaTarihi = DateTime.Today.AddDays(-_random.Next(30, 1825)), // Son 5 yıl
                NetMaas = _random.Next(25, 45) * 1000m,
                Aktif = _random.Next(100) < 90, // %90 aktif
                Notlar = "[TEST] Demo şoför verisi",
                CreatedAt = DateTime.Now.AddDays(-_random.Next(30, 365))
            });
        }

        _context.Soforler.AddRange(soforler);
        await _context.SaveChangesAsync();
        return soforler.Count;
    }

    #endregion

    #region Araç Seed

    private async Task<int> SeedAraclarAsync()
    {
        if (await _context.Araclar.AnyAsync(a => a.Notlar != null && a.Notlar.Contains("[TEST]")))
            return 0;

        var araclar = new List<Arac>();
        var plakaHarfler = new[] { "AA", "AB", "AC", "AD", "AE", "AF", "AG", "AH", "AJ", "AK" };
        var markalar = new[] { "Mercedes-Benz", "Ford", "Volkswagen", "Fiat", "Hyundai", "Iveco" };
        var modeller = new[] { "Sprinter", "Transit", "Crafter", "Ducato", "H350", "Daily" };

        for (int i = 0; i < 12; i++)
        {
            var yil = _random.Next(2018, 2025);
            var sahiplikTipi = (AracSahiplikTipi)(_random.Next(1, 4)); // 1-3

            var arac = new Arac
            {
                SaseNo = $"WDB{_random.Next(100000000, 999999999)}{_random.Next(100000, 999999)}",
                AktifPlaka = $"34 {plakaHarfler[i % plakaHarfler.Length]} {_random.Next(100, 999)}",
                Marka = markalar[_random.Next(markalar.Length)],
                Model = modeller[_random.Next(modeller.Length)],
                ModelYili = yil,
                AracTipi = (AracTipi)_random.Next(1, 5),
                SahiplikTipi = sahiplikTipi,
                KoltukSayisi = new[] { 9, 14, 16, 20, 27, 46 }[_random.Next(6)],
                KmDurumu = _random.Next(10000, 350000),
                TrafikSigortaBitisTarihi = DateTime.Today.AddMonths(_random.Next(-1, 12)),
                KaskoBitisTarihi = _random.Next(100) < 70 ? DateTime.Today.AddMonths(_random.Next(-1, 12)) : null,
                MuayeneBitisTarihi = DateTime.Today.AddMonths(_random.Next(-1, 24)),
                Aktif = _random.Next(100) < 90,
                Notlar = "[TEST] Demo araç verisi",
                CreatedAt = DateTime.Now.AddDays(-_random.Next(30, 365))
            };

            // Kiralık araçlar için kira bilgisi
            if (sahiplikTipi == AracSahiplikTipi.Kiralik)
            {
                arac.AylikKiraBedeli = _random.Next(15, 40) * 1000m;
            }

            araclar.Add(arac);
        }

        _context.Araclar.AddRange(araclar);
        await _context.SaveChangesAsync();
        return araclar.Count;
    }

    #endregion

    #region Güzergah Seed

    private async Task<int> SeedGuzergahlarAsync()
    {
        if (await _context.Guzergahlar.AnyAsync(g => g.Notlar != null && g.Notlar.Contains("[TEST]")))
            return 0;

        var musteriler = await _context.Cariler.Where(c => c.CariTipi == CariTipi.Musteri && c.Aktif).ToListAsync();
        if (!musteriler.Any())
        {
            _logger.LogWarning("Müşteri bulunamadı, güzergah seed atlanıyor");
            return 0;
        }

        var guzergahlar = new List<Guzergah>();

        // İstanbul'daki önemli noktalar (lat, lng)
        var noktalar = new (string Ad, double Lat, double Lng)[]
        {
            ("Kadıköy", 40.9908, 29.0259),
            ("Beşiktaş", 41.0422, 29.0067),
            ("Taksim", 41.0370, 28.9850),
            ("Şişli", 41.0602, 28.9877),
            ("Levent", 41.0819, 29.0131),
            ("Maslak", 41.1086, 29.0200),
            ("Ataşehir", 40.9923, 29.1244),
            ("Kartal", 40.8893, 29.1856),
            ("Pendik", 40.8756, 29.2333),
            ("Bakırköy", 40.9798, 28.8717),
            ("Beylikdüzü", 41.0022, 28.6444),
            ("Esenyurt", 41.0333, 28.6778)
        };

        var renkler = new[] { "#3498db", "#e74c3c", "#2ecc71", "#f39c12", "#9b59b6", "#1abc9c", "#e67e22", "#34495e" };

        for (int i = 0; i < 8; i++)
        {
            var baslangic = noktalar[_random.Next(noktalar.Length)];
            var bitis = noktalar.Where(n => n.Ad != baslangic.Ad).ElementAt(_random.Next(noktalar.Length - 1));
            var mesafe = Math.Round(5 + _random.NextDouble() * 35, 1); // 5-40 km
            var musteri = musteriler[_random.Next(musteriler.Count)];

            guzergahlar.Add(new Guzergah
            {
                GuzergahKodu = $"GZR{(i + 1):D3}",
                GuzergahAdi = $"{baslangic.Ad} - {bitis.Ad}",
                BaslangicNoktasi = baslangic.Ad,
                BitisNoktasi = bitis.Ad,
                Mesafe = (decimal)mesafe,
                TahminiSure = (int)(mesafe * 2.5), // Ortalama 25 km/saat
                BaslangicLatitude = baslangic.Lat,
                BaslangicLongitude = baslangic.Lng,
                BitisLatitude = bitis.Lat,
                BitisLongitude = bitis.Lng,
                RotaRengi = renkler[i % renkler.Length],
                BirimFiyat = _random.Next(300, 1000),
                CariId = musteri.Id,
                Aktif = true,
                Notlar = "[TEST] Demo güzergah verisi",
                CreatedAt = DateTime.Now.AddDays(-_random.Next(30, 180))
            });
        }

        _context.Guzergahlar.AddRange(guzergahlar);
        await _context.SaveChangesAsync();
        return guzergahlar.Count;
    }

    #endregion

    #region Fatura Seed

    private async Task<int> SeedFaturalarAsync()
    {
        if (await _context.Faturalar.AnyAsync(f => f.Aciklama != null && f.Aciklama.Contains("[TEST]")))
            return 0;

        var musteriler = await _context.Cariler.Where(c => c.CariTipi == CariTipi.Musteri && c.Aktif).ToListAsync();
        var tedarikciler = await _context.Cariler.Where(c => c.CariTipi == CariTipi.Tedarikci && c.Aktif).ToListAsync();
        var guzergahlar = await _context.Guzergahlar.Where(g => g.Aktif).ToListAsync();

        if (!musteriler.Any())
        {
            _logger.LogWarning("Müşteri bulunamadı, fatura seed atlanıyor");
            return 0;
        }

        var faturalar = new List<Fatura>();

        // Son 6 aylık satış faturaları
        for (int i = 0; i < 30; i++)
        {
            var musteri = musteriler[_random.Next(musteriler.Count)];
            var tarih = DateTime.Today.AddDays(-_random.Next(0, 180));
            var kalemSayisi = _random.Next(1, 5);

            var fatura = new Fatura
            {
                FaturaNo = $"SF{tarih:yyyyMM}-{(i + 1):D4}",
                FaturaTarihi = tarih,
                VadeTarihi = tarih.AddDays(30),
                CariId = musteri.Id,
                FaturaYonu = FaturaYonu.Giden,
                FaturaTipi = FaturaTipi.SatisFaturasi,
                EFaturaTipi = EFaturaTipi.EFatura,
                Durum = tarih < DateTime.Today.AddDays(-45) ? FaturaDurum.Odendi : 
                        tarih < DateTime.Today.AddDays(-15) ? FaturaDurum.KismiOdendi : FaturaDurum.Beklemede,
                Aciklama = "[TEST] Demo satış faturası",
                CreatedAt = tarih
            };

            // Fatura kalemleri
            for (int k = 0; k < kalemSayisi; k++)
            {
                var guzergah = guzergahlar.Any() ? guzergahlar[_random.Next(guzergahlar.Count)] : null;
                var birimFiyat = _random.Next(500, 3000);
                var miktar = _random.Next(1, 20);
                var netTutar = birimFiyat * miktar;
                var kdvTutar = netTutar * 0.20m;

                var kalem = new FaturaKalem
                {
                    SiraNo = k + 1,
                    Aciklama = guzergah != null ? $"Personel Servis - {guzergah.GuzergahAdi}" : "Personel Servis Hizmeti",
                    Miktar = miktar,
                    BirimFiyat = birimFiyat,
                    KdvOrani = 20,
                    KdvTutar = kdvTutar,
                    ToplamTutar = netTutar + kdvTutar,
                    CreatedAt = tarih
                };

                fatura.FaturaKalemleri.Add(kalem);
            }

            fatura.AraToplam = fatura.FaturaKalemleri.Sum(k => k.Miktar * k.BirimFiyat);
            fatura.KdvTutar = fatura.FaturaKalemleri.Sum(k => k.KdvTutar);
            fatura.GenelToplam = fatura.FaturaKalemleri.Sum(k => k.ToplamTutar);

            if (fatura.Durum == FaturaDurum.Odendi)
                fatura.OdenenTutar = fatura.GenelToplam;
            else if (fatura.Durum == FaturaDurum.KismiOdendi)
                fatura.OdenenTutar = Math.Round(fatura.GenelToplam * (decimal)(_random.Next(30, 70) / 100.0), 2);

            faturalar.Add(fatura);
        }

        // Son 6 aylık alış faturaları (tedarikçilerden)
        if (tedarikciler.Any())
        {
            for (int i = 0; i < 15; i++)
            {
                var tedarikci = tedarikciler[_random.Next(tedarikciler.Count)];
                var tarih = DateTime.Today.AddDays(-_random.Next(0, 180));

                var fatura = new Fatura
                {
                    FaturaNo = $"AF{tarih:yyyyMM}-{(i + 1):D4}",
                    FaturaTarihi = tarih,
                    VadeTarihi = tarih.AddDays(30),
                    CariId = tedarikci.Id,
                    FaturaYonu = FaturaYonu.Gelen,
                    FaturaTipi = FaturaTipi.AlisFaturasi,
                    EFaturaTipi = EFaturaTipi.EFatura,
                    Durum = tarih < DateTime.Today.AddDays(-30) ? FaturaDurum.Odendi : FaturaDurum.Beklemede,
                    Aciklama = "[TEST] Demo alış faturası",
                    CreatedAt = tarih
                };

                // Tek kalem (yakıt, bakım vb.)
                var aciklamalar = new[] { "Yakıt Alımı", "Araç Bakım", "Lastik Değişimi", "Yedek Parça", "Sigorta Primi" };
                var birimFiyat = _random.Next(2000, 15000);
                var kdvTutar = birimFiyat * 0.20m;

                var kalem = new FaturaKalem
                {
                    SiraNo = 1,
                    Aciklama = aciklamalar[_random.Next(aciklamalar.Length)],
                    Miktar = 1,
                    BirimFiyat = birimFiyat,
                    KdvOrani = 20,
                    KdvTutar = kdvTutar,
                    ToplamTutar = birimFiyat + kdvTutar,
                    CreatedAt = tarih
                };

                fatura.FaturaKalemleri.Add(kalem);
                fatura.AraToplam = birimFiyat;
                fatura.KdvTutar = kdvTutar;
                fatura.GenelToplam = birimFiyat + kdvTutar;

                if (fatura.Durum == FaturaDurum.Odendi)
                    fatura.OdenenTutar = fatura.GenelToplam;

                faturalar.Add(fatura);
            }
        }

        _context.Faturalar.AddRange(faturalar);
        await _context.SaveChangesAsync();
        return faturalar.Count;
    }

    #endregion

    #region Servis Çalışması Seed

    private async Task<int> SeedServisCalismalarıAsync()
    {
        if (await _context.ServisCalismalari.AnyAsync(s => s.Notlar != null && s.Notlar.Contains("[TEST]")))
            return 0;

        var araclar = await _context.Araclar.Where(a => a.Aktif).ToListAsync();
        var soforler = await _context.Soforler.Where(s => s.Aktif && s.Gorev == PersonelGorev.Sofor).ToListAsync();
        var guzergahlar = await _context.Guzergahlar.Where(g => g.Aktif).ToListAsync();

        if (!araclar.Any() || !guzergahlar.Any() || !soforler.Any())
        {
            _logger.LogWarning("Araç, güzergah veya şoför bulunamadı, servis çalışması seed atlanıyor");
            return 0;
        }

        var calismalar = new List<ServisCalisma>();

        // Son 30 günlük servis çalışmaları
        for (int gun = 0; gun < 30; gun++)
        {
            var tarih = DateTime.Today.AddDays(-gun);
            if (tarih.DayOfWeek == DayOfWeek.Sunday) continue; // Pazar hariç

            // Her gün için 5-10 sefer
            var seferSayisi = _random.Next(5, 11);
            for (int s = 0; s < seferSayisi; s++)
            {
                var arac = araclar[_random.Next(araclar.Count)];
                var sofor = soforler[_random.Next(soforler.Count)];
                var guzergah = guzergahlar[_random.Next(guzergahlar.Count)];

                calismalar.Add(new ServisCalisma
                {
                    CalismaTarihi = tarih,
                    AracId = arac.Id,
                    SoforId = sofor.Id,
                    GuzergahId = guzergah.Id,
                    ServisTuru = (ServisTuru)_random.Next(1, 4),
                    Fiyat = guzergah.BirimFiyat,
                    Durum = CalismaDurum.Tamamlandi,
                    Notlar = "[TEST] Demo servis çalışması",
                    CreatedAt = tarih
                });
            }
        }

        _context.ServisCalismalari.AddRange(calismalar);
        await _context.SaveChangesAsync();
        return calismalar.Count;
    }

    #endregion

    #region Yardımcı Metodlar

    private string RastgeleIsim()
    {
        var erkek = _random.Next(100) < 50;
        var ad = erkek ? _erkekAdlari[_random.Next(_erkekAdlari.Length)] : _kadinAdlari[_random.Next(_kadinAdlari.Length)];
        var soyad = _soyadlari[_random.Next(_soyadlari.Length)];
        return $"{ad} {soyad}";
    }

    #endregion
}

/// <summary>
/// Test veri oluşturma sonucu
/// </summary>
public class TestDataResult
{
    public bool Basarili { get; set; }
    public List<string> Mesajlar { get; set; } = new();
    public int CariSayisi { get; set; }
    public int SoforSayisi { get; set; }
    public int AracSayisi { get; set; }
    public int GuzergahSayisi { get; set; }
    public int FaturaSayisi { get; set; }
    public int ServisCalismasiSayisi { get; set; }

    public int ToplamKayit => CariSayisi + SoforSayisi + AracSayisi + GuzergahSayisi + FaturaSayisi + ServisCalismasiSayisi;
}
