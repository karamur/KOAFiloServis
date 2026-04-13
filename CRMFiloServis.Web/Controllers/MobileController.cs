using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CRMFiloServis.Web.Services;
using CRMFiloServis.Web.Services.Interfaces;
using CRMFiloServis.Web.Data;
using CRMFiloServis.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CRMFiloServis.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MobileController : ControllerBase
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IKullaniciService _kullaniciService;
    private readonly IAracTakipService? _aracTakipService;
    private readonly IAracTakipBildirimService? _bildirimService;
    private readonly ILogger<MobileController> _logger;

    public MobileController(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        IKullaniciService kullaniciService,
        ILogger<MobileController> logger,
        IAracTakipService? aracTakipService = null,
        IAracTakipBildirimService? bildirimService = null)
    {
        _contextFactory = contextFactory;
        _kullaniciService = kullaniciService;
        _logger = logger;
        _aracTakipService = aracTakipService;
        _bildirimService = bildirimService;
    }

    #region Yardımcı Metodlar

    private int? GetCurrentKullaniciId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var id) ? id : null;
    }

    private async Task<int?> GetSoforIdAsync(ApplicationDbContext context)
    {
        // Şoför-Kullanıcı ilişkisi için email eşleştirmesi
        var kullaniciId = GetCurrentKullaniciId();
        if (!kullaniciId.HasValue) return null;

        // Kullanıcı bilgisini al
        var kullanici = await context.Kullanicilar
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.Id == kullaniciId.Value);

        if (kullanici == null) return null;

        // Email ile şoför bul
        var sofor = await context.Soforler
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Email == kullanici.Email);

        return sofor?.Id;
    }

    #endregion

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var now = DateTime.UtcNow;
        var ayBaslangic = new DateTime(now.Year, now.Month, 1);
        var ayBitis = ayBaslangic.AddMonths(1).AddDays(-1);

        // Genel istatistikler
        var toplamArac = await context.Araclar.CountAsync();
        var aktifArac = await context.Araclar.CountAsync(a => a.Aktif);
        var toplamSofor = await context.Soforler.CountAsync(s => s.Aktif);
        var servistekiArac = await context.Araclar.CountAsync(a => !a.Aktif);

        // Fatura istatistikleri
        var bekleyenFatura = await context.Faturalar
            .CountAsync(f => f.Durum != FaturaDurum.Odendi && !f.IsDeleted);

        // Evrak uyarilari (30 gun icinde bitecekler)
        var evrakUyariTarihi = now.AddDays(30);
        var yaklaşanEvrak = await context.AracEvraklari
            .CountAsync(e => e.BitisTarihi.HasValue && 
                           e.BitisTarihi <= evrakUyariTarihi && 
                           e.BitisTarihi > now);

        // Aylik gelir/gider
        var aylikGelir = await context.Faturalar
            .Where(f => f.FaturaTarihi >= ayBaslangic && 
                       f.FaturaTarihi <= ayBitis && 
                       f.FaturaTipi == FaturaTipi.SatisFaturasi)
            .SumAsync(f => (decimal?)f.GenelToplam) ?? 0;

        var aylikGider = await context.AracMasraflari
            .Where(m => m.MasrafTarihi >= ayBaslangic && m.MasrafTarihi <= ayBitis)
            .SumAsync(m => (decimal?)m.Tutar) ?? 0;

        // Banka bakiye - HareketTipi'ne gore hesapla
        var girisler = await context.BankaKasaHareketleri
            .Where(b => b.HareketTipi == HareketTipi.Giris)
            .SumAsync(b => (decimal?)b.Tutar) ?? 0;
        var cikislar = await context.BankaKasaHareketleri
            .Where(b => b.HareketTipi == HareketTipi.Cikis)
            .SumAsync(b => (decimal?)b.Tutar) ?? 0;
        var bankaBakiye = girisler - cikislar;

        // Son islemler
        var sonIslemler = await context.AktiviteLoglar
            .OrderByDescending(a => a.CreatedAt)
            .Take(10)
            .Select(a => new
            {
                Baslik = a.Aciklama,
                Tip = a.IslemTipi,
                Tarih = a.CreatedAt
            })
            .ToListAsync();

        // Odenmemis faturalar
        var odenmemisFaturalar = await context.Faturalar
            .Include(f => f.Cari)
            .Where(f => f.Durum != FaturaDurum.Odendi && !f.IsDeleted)
            .OrderByDescending(f => f.FaturaTarihi)
            .Take(5)
            .Select(f => new
            {
                FaturaNo = f.FaturaNo,
                CariUnvan = f.Cari.Unvan,
                Tutar = f.GenelToplam,
                Tarih = f.FaturaTarihi
            })
            .ToListAsync();

        // Son masraflar
        var sonMasraflar = await context.AracMasraflari
            .Include(m => m.MasrafKalemi)
            .OrderByDescending(m => m.MasrafTarihi)
            .Take(5)
            .Select(m => new
            {
                Aciklama = m.MasrafKalemi.MasrafAdi,
                Tutar = m.Tutar,
                Tarih = m.MasrafTarihi
            })
            .ToListAsync();

        // Gunun guzergahlari
        var bugun = DateTime.Today;
        var gununGuzergahlari = await context.ServisCalismalari
            .Include(s => s.Guzergah)
            .Include(s => s.Sofor)
            .Include(s => s.Arac)
            .Where(s => s.CalismaTarihi.Date == bugun)
            .Take(5)
            .Select(s => new
            {
                GuzergahAdi = s.Guzergah.GuzergahAdi,
                SoforAdi = s.Sofor.Ad + " " + s.Sofor.Soyad,
                Plaka = s.Arac != null ? s.Arac.AktifPlaka : "",
                Durum = s.Durum
            })
            .ToListAsync();

        // Evrak uyarilari listesi
        var evrakUyarilari = await context.AracEvraklari
            .Include(e => e.Arac)
            .Where(e => e.BitisTarihi.HasValue && 
                       e.BitisTarihi <= evrakUyariTarihi && 
                       e.BitisTarihi > now)
            .OrderBy(e => e.BitisTarihi)
            .Take(5)
            .Select(e => new
            {
                EvrakTipi = e.EvrakKategorisi,
                Plaka = e.Arac != null ? e.Arac.AktifPlaka : "",
                BitisTarihi = e.BitisTarihi,
                KalanGun = e.BitisTarihi.HasValue ? 
                    (e.BitisTarihi.Value.Date - now.Date).Days : 0
            })
            .ToListAsync();

        return Ok(new
        {
            ToplamArac = toplamArac,
            AktifArac = aktifArac,
            ToplamSofor = toplamSofor,
            ServistekiArac = servistekiArac,
            BekleyenFatura = bekleyenFatura,
            YaklaşanEvrak = yaklaşanEvrak,
            AylikGelir = aylikGelir,
            AylikGider = aylikGider,
            BankaBakiye = bankaBakiye,
            SonIslemler = sonIslemler,
            OdenmemisFaturalar = odenmemisFaturalar,
            SonMasraflar = sonMasraflar,
            GununGuzergahlari = gununGuzergahlari,
            EvrakUyarilari = evrakUyarilari
        });
    }

    [HttpGet("araclar")]
    public async Task<IActionResult> GetAraclar()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var araclar = await context.Araclar
            .OrderBy(a => a.AktifPlaka)
            .Select(a => new
            {
                a.Id,
                Plaka = a.AktifPlaka ?? a.SaseNo,
                a.Marka,
                a.Model,
                a.ModelYili,
                a.KmDurumu,
                a.Aktif
            })
            .ToListAsync();

        return Ok(araclar);
    }

    [HttpGet("soforler")]
    public async Task<IActionResult> GetSoforler()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var soforler = await context.Soforler
            .Where(s => s.Aktif)
            .OrderBy(s => s.Ad)
            .Select(s => new
            {
                s.Id,
                s.SoforKodu,
                AdSoyad = s.Ad + " " + s.Soyad,
                s.Telefon
            })
            .ToListAsync();

        return Ok(soforler);
    }

    [HttpGet("guzergahlar")]
    public async Task<IActionResult> GetGuzergahlar()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var guzergahlar = await context.Guzergahlar
            .OrderBy(g => g.GuzergahAdi)
            .Select(g => new
            {
                g.Id,
                g.GuzergahKodu,
                g.GuzergahAdi,
                g.BaslangicNoktasi,
                g.BitisNoktasi,
                g.Mesafe
            })
            .ToListAsync();

        return Ok(guzergahlar);
    }

    #region Sefer Endpoint'leri

    /// <summary>
    /// Şoförün aktif seferlerini getirir
    /// </summary>
    [HttpGet("seferler/aktif")]
    public async Task<IActionResult> GetAktifSeferler()
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var soforId = await GetSoforIdAsync(context);

            if (!soforId.HasValue)
            {
                // Şoför değilse tüm aktif seferleri göster (yönetici)
                var tumSeferler = await context.ServisCalismalari
                    .AsNoTracking()
                    .Include(s => s.Arac)
                    .Include(s => s.Guzergah)
                    .Where(s => s.CalismaTarihi.Date == DateTime.Today && s.Durum == CalismaDurum.Planli)
                    .OrderByDescending(s => s.BaslangicSaati)
                    .Take(20)
                    .Select(s => new MobileSeferOzet
                    {
                        Id = s.Id,
                        AracId = s.AracId,
                        AracPlaka = s.Arac != null ? s.Arac.AktifPlaka ?? "" : "",
                        GuzergahId = s.GuzergahId,
                        GuzergahAdi = s.Guzergah != null ? s.Guzergah.GuzergahAdi ?? "" : "",
                        BaslangicSaati = s.BaslangicSaati,
                        BitisSaati = s.BitisSaati,
                        Durum = s.Durum.ToString(),
                        BaslangicKm = s.KmBaslangic,
                        BitisKm = s.KmBitis
                    })
                    .ToListAsync();
                return Ok(tumSeferler);
            }

            var seferler = await context.ServisCalismalari
                .AsNoTracking()
                .Include(s => s.Arac)
                .Include(s => s.Guzergah)
                .Where(s => s.SoforId == soforId.Value && 
                           s.CalismaTarihi.Date == DateTime.Today &&
                           s.Durum == CalismaDurum.Planli)
                .OrderByDescending(s => s.BaslangicSaati)
                .Select(s => new MobileSeferOzet
                {
                    Id = s.Id,
                    AracId = s.AracId,
                    AracPlaka = s.Arac != null ? s.Arac.AktifPlaka ?? "" : "",
                    GuzergahId = s.GuzergahId,
                    GuzergahAdi = s.Guzergah != null ? s.Guzergah.GuzergahAdi ?? "" : "",
                    BaslangicSaati = s.BaslangicSaati,
                    BitisSaati = s.BitisSaati,
                    Durum = s.Durum.ToString(),
                    BaslangicKm = s.KmBaslangic,
                    BitisKm = s.KmBitis
                })
                .ToListAsync();

            return Ok(seferler);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Aktif seferler getirilirken hata");
            return StatusCode(500, new { Error = "Seferler yüklenirken hata oluştu" });
        }
    }

    /// <summary>
    /// Yeni sefer başlatır
    /// </summary>
    [HttpPost("seferler/baslat")]
    public async Task<IActionResult> SeferBaslat([FromBody] MobileSeferBaslatRequest request)
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var soforId = await GetSoforIdAsync(context);

            if (!soforId.HasValue)
            {
                return BadRequest(new { Error = "Şoför bilgisi bulunamadı" });
            }

            // Araç kontrolü
            var arac = await context.Araclar.FindAsync(request.AracId);
            if (arac == null || !arac.Aktif)
            {
                return BadRequest(new { Error = "Araç bulunamadı veya aktif değil" });
            }

            // Güzergah kontrolü
            var guzergah = await context.Guzergahlar.FindAsync(request.GuzergahId);
            if (guzergah == null)
            {
                return BadRequest(new { Error = "Güzergah bulunamadı" });
            }

            // Aktif sefer kontrolü
            var aktifSefer = await context.ServisCalismalari
                .FirstOrDefaultAsync(s => s.SoforId == soforId.Value && 
                                          s.CalismaTarihi.Date == DateTime.Today &&
                                          s.Durum == CalismaDurum.Planli);
            if (aktifSefer != null)
            {
                return BadRequest(new { Error = "Zaten aktif bir seferiniz var" });
            }

            // Yeni sefer oluştur
            var sefer = new ServisCalisma
            {
                AracId = request.AracId,
                SoforId = soforId.Value,
                GuzergahId = request.GuzergahId,
                CalismaTarihi = DateTime.Today,
                BaslangicSaati = DateTime.Now.TimeOfDay,
                KmBaslangic = request.BaslangicKm,
                ServisTuru = ServisTuru.Ozel,
                Durum = CalismaDurum.Planli,
                Notlar = request.Notlar,
                CreatedAt = DateTime.Now
            };

            context.ServisCalismalari.Add(sefer);

            // Araç KM'sini güncelle
            if (request.BaslangicKm > (arac.KmDurumu ?? 0))
            {
                arac.KmDurumu = request.BaslangicKm;
            }

            await context.SaveChangesAsync();

            _logger.LogInformation("Sefer başlatıldı: {SeferId}, Şoför: {SoforId}", sefer.Id, soforId);

            return Ok(new MobileSeferDetay
            {
                Id = sefer.Id,
                AracId = sefer.AracId,
                AracPlaka = arac.AktifPlaka ?? "",
                GuzergahId = sefer.GuzergahId,
                GuzergahAdi = guzergah.GuzergahAdi ?? "",
                BaslangicSaati = sefer.BaslangicSaati,
                Durum = sefer.Durum.ToString(),
                BaslangicKm = sefer.KmBaslangic,
                Notlar = sefer.Notlar
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sefer başlatılırken hata");
            return StatusCode(500, new { Error = "Sefer başlatılırken hata oluştu" });
        }
    }

    /// <summary>
    /// Seferi bitirir
    /// </summary>
    [HttpPost("seferler/{seferId}/bitir")]
    public async Task<IActionResult> SeferBitir(int seferId, [FromBody] MobileSeferBitirRequest request)
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var soforId = await GetSoforIdAsync(context);

            var sefer = await context.ServisCalismalari
                .Include(s => s.Arac)
                .Include(s => s.Guzergah)
                .FirstOrDefaultAsync(s => s.Id == seferId && (soforId == null || s.SoforId == soforId.Value));

            if (sefer == null)
            {
                return NotFound(new { Error = "Sefer bulunamadı" });
            }

            if (sefer.Durum == CalismaDurum.Tamamlandi)
            {
                return BadRequest(new { Error = "Sefer zaten bitirilmiş" });
            }

            // Seferi bitir
            sefer.BitisSaati = DateTime.Now.TimeOfDay;
            sefer.KmBitis = request.BitisKm;
            sefer.Durum = CalismaDurum.Tamamlandi;
            sefer.Notlar = string.IsNullOrEmpty(sefer.Notlar) ? request.Notlar : $"{sefer.Notlar}\n{request.Notlar}";

            if (request.YakitTuketimi.HasValue)
            {
                // YakitTuketimi için masraf kaydı oluştur
                var yakitMasrafKalemi = await context.MasrafKalemleri
                    .FirstOrDefaultAsync(m => m.Kategori == MasrafKategori.Yakit);
                if (yakitMasrafKalemi != null)
                {
                    var yakitMasraf = new AracMasraf
                    {
                        AracId = sefer.AracId,
                        MasrafKalemiId = yakitMasrafKalemi.Id,
                        MasrafTarihi = DateTime.Today,
                        Tutar = request.YakitTuketimi.Value,
                        Aciklama = $"Sefer #{sefer.Id} yakıt tüketimi",
                        ServisCalismaId = sefer.Id,
                        CreatedAt = DateTime.Now
                    };
                    context.AracMasraflari.Add(yakitMasraf);
                }
            }

            // Araç KM güncelle
            if (sefer.Arac != null && request.BitisKm > (sefer.Arac.KmDurumu ?? 0))
            {
                sefer.Arac.KmDurumu = request.BitisKm;
            }

            // Fiyat hesapla
            if (sefer.Guzergah != null)
            {
                sefer.Fiyat = sefer.Guzergah.BirimFiyat;
            }

            await context.SaveChangesAsync();

            _logger.LogInformation("Sefer bitirildi: {SeferId}", seferId);

            return Ok(new MobileSeferDetay
            {
                Id = sefer.Id,
                AracId = sefer.AracId,
                AracPlaka = sefer.Arac?.AktifPlaka ?? "",
                GuzergahId = sefer.GuzergahId,
                GuzergahAdi = sefer.Guzergah?.GuzergahAdi ?? "",
                BaslangicSaati = sefer.BaslangicSaati,
                BitisSaati = sefer.BitisSaati,
                Durum = sefer.Durum.ToString(),
                BaslangicKm = sefer.KmBaslangic,
                BitisKm = sefer.KmBitis,
                Notlar = sefer.Notlar
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sefer bitirilirken hata");
            return StatusCode(500, new { Error = "Sefer bitirilirken hata oluştu" });
        }
    }

    #endregion

    #region Konum Endpoint'leri

    /// <summary>
    /// Konum verisi gönderir
    /// </summary>
    [HttpPost("konum")]
    public async Task<IActionResult> KonumGonder([FromBody] MobileKonumGonderRequest request)
    {
        try
        {
            if (!request.AracId.HasValue && !request.SeferId.HasValue)
            {
                return BadRequest(new { Error = "Araç veya sefer ID gerekli" });
            }

            using var context = await _contextFactory.CreateDbContextAsync();
            int? aracId = request.AracId;

            if (request.SeferId.HasValue)
            {
                var sefer = await context.ServisCalismalari.AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == request.SeferId.Value);
                if (sefer != null) aracId = sefer.AracId;
            }

            if (!aracId.HasValue)
            {
                return BadRequest(new { Error = "Araç belirtilmeli" });
            }

            // GPS cihazı bul
            var cihaz = await context.AracTakipCihazlar
                .FirstOrDefaultAsync(c => c.AracId == aracId.Value && c.Aktif);

            if (cihaz != null && _aracTakipService != null)
            {
                var konum = new AracKonum
                {
                    AracTakipCihazId = cihaz.Id,
                    Latitude = request.Enlem,
                    Longitude = request.Boylam,
                    Hiz = request.Hiz,
                    Yon = request.Yon,
                    KontakDurumu = request.KontakDurumu,
                    MotorDurumu = request.MotorDurumu,
                    YakitSeviyesi = (int?)request.YakitSeviyesi,
                    OlayTipi = KonumOlayTipi.Normal,
                    KayitZamani = DateTime.Now,
                    CreatedAt = DateTime.Now
                };

                await _aracTakipService.KaydetKonumAsync(konum);

                // SignalR bildirim
                if (_bildirimService != null)
                {
                    var arac = await context.Araclar.AsNoTracking()
                        .FirstOrDefaultAsync(a => a.Id == aracId.Value);

                    var guncelleme = new CRMFiloServis.Web.Hubs.AracKonumGuncelleme
                    {
                        AracId = aracId.Value,
                        Plaka = arac?.AktifPlaka ?? "",
                        Enlem = request.Enlem,
                        Boylam = request.Boylam,
                        Hiz = request.Hiz,
                        Yon = request.Yon,
                        KontakDurumu = request.KontakDurumu ?? false,
                        MotorDurumu = request.MotorDurumu ?? false,
                        ZamanDamgasi = DateTime.Now,
                        Durum = "Hareket"
                    };
                    await _bildirimService.KonumGuncellemesiGonderAsync(guncelleme);
                }
            }

            return Ok(new { Success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Konum gönderilirken hata");
            return StatusCode(500, new { Error = "Konum kaydedilirken hata oluştu" });
        }
    }

    #endregion

    #region Masraf Endpoint'leri

    /// <summary>
    /// Masraf kalemlerini getirir
    /// </summary>
    [HttpGet("masraf-kalemleri")]
    public async Task<IActionResult> GetMasrafKalemleri()
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var kalemler = await context.MasrafKalemleri
                .AsNoTracking()
                .Where(k => k.Aktif)
                .OrderBy(k => k.MasrafAdi)
                .Select(k => new MobileMasrafKalemiOzet
                {
                    Id = k.Id,
                    Ad = k.MasrafAdi ?? "",
                    Kategori = k.Kategori.ToString(),
                    Ikon = MasrafKalemiIkon(k.MasrafAdi)
                })
                .ToListAsync();

            return Ok(kalemler);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Masraf kalemleri getirilirken hata");
            return StatusCode(500, new { Error = "Masraf kalemleri yüklenirken hata oluştu" });
        }
    }

    /// <summary>
    /// Masraf kaydeder
    /// </summary>
    [HttpPost("masraf")]
    public async Task<IActionResult> MasrafKaydet([FromBody] MobileMasrafKayitRequest request)
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var arac = await context.Araclar.FindAsync(request.AracId);
            if (arac == null)
            {
                return BadRequest(new { Error = "Araç bulunamadı" });
            }

            var kalem = await context.MasrafKalemleri.FindAsync(request.MasrafKalemiId);
            if (kalem == null)
            {
                return BadRequest(new { Error = "Masraf kalemi bulunamadı" });
            }

            var masraf = new AracMasraf
            {
                AracId = request.AracId,
                MasrafKalemiId = request.MasrafKalemiId,
                Tutar = request.Tutar,
                MasrafTarihi = request.Tarih,
                Aciklama = request.Aciklama,
                CreatedAt = DateTime.Now
            };

            context.AracMasraflari.Add(masraf);

            if (request.KmDurumu.HasValue && request.KmDurumu > (arac.KmDurumu ?? 0))
            {
                arac.KmDurumu = request.KmDurumu;
            }

            await context.SaveChangesAsync();

            // Fiş görseli için - Entity'de FisGorseli alanı yok, Aciklama'ya ekle
            if (!string.IsNullOrEmpty(request.FisGorseliBase64))
            {
                var dosyaAdi = $"masraf_{masraf.Id}_{Guid.NewGuid():N}.jpg";
                var klasor = Path.Combine("wwwroot", "uploads", "masraf");

                if (!Directory.Exists(klasor))
                    Directory.CreateDirectory(klasor);

                var dosyaYolu = Path.Combine(klasor, dosyaAdi);
                var bytes = Convert.FromBase64String(request.FisGorseliBase64);
                await System.IO.File.WriteAllBytesAsync(dosyaYolu, bytes);

                // Fiş yolunu açıklamaya ekle
                masraf.Aciklama = $"{masraf.Aciklama ?? ""}\n[Fiş: /uploads/masraf/{dosyaAdi}]".Trim();
                await context.SaveChangesAsync();
            }

            _logger.LogInformation("Masraf kaydedildi: {MasrafId}, Araç: {AracId}", masraf.Id, request.AracId);

            return Ok(new { Success = true, MasrafId = masraf.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Masraf kaydedilirken hata");
            return StatusCode(500, new { Error = "Masraf kaydedilirken hata oluştu" });
        }
    }

    private static string MasrafKalemiIkon(string? ad)
    {
        if (string.IsNullOrEmpty(ad)) return "bi-receipt";

        return ad.ToLowerInvariant() switch
        {
            var a when a.Contains("yakıt") || a.Contains("benzin") || a.Contains("mazot") => "bi-fuel-pump",
            var a when a.Contains("yol") || a.Contains("hgs") || a.Contains("ogs") => "bi-signpost",
            var a when a.Contains("park") => "bi-p-circle",
            var a when a.Contains("yıkama") || a.Contains("temizlik") => "bi-droplet",
            var a when a.Contains("bakım") || a.Contains("servis") => "bi-wrench",
            var a when a.Contains("lastik") => "bi-circle",
            _ => "bi-receipt"
        };
    }

    #endregion

    #region Arıza Bildirimi

    /// <summary>
    /// Şoförün sefer geçmişini getirir
    /// </summary>
    [HttpGet("seferler")]
    public async Task<IActionResult> GetSeferGecmisi([FromQuery] int sayfa = 1, [FromQuery] int adet = 20)
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var soforId = await GetSoforIdAsync(context);

            var query = context.ServisCalismalari
                .AsNoTracking()
                .Include(s => s.Arac)
                .Include(s => s.Guzergah)
                .AsQueryable();

            // Şoför ise sadece kendi seferlerini göster
            if (soforId.HasValue)
            {
                query = query.Where(s => s.SoforId == soforId.Value);
            }

            var seferler = await query
                .OrderByDescending(s => s.CalismaTarihi)
                .ThenByDescending(s => s.BaslangicSaati)
                .Skip((sayfa - 1) * adet)
                .Take(adet)
                .Select(s => new MobileSeferGecmisOzet
                {
                    Id = s.Id,
                    AracId = s.AracId,
                    AracPlaka = s.Arac != null ? s.Arac.AktifPlaka ?? "" : "",
                    GuzergahId = s.GuzergahId,
                    GuzergahAdi = s.Guzergah != null ? s.Guzergah.GuzergahAdi ?? "" : "",
                    BaslangicZamani = s.CalismaTarihi.Add(s.BaslangicSaati ?? TimeSpan.Zero),
                    BitisZamani = s.BitisSaati.HasValue ? s.CalismaTarihi.Add(s.BitisSaati.Value) : (DateTime?)null,
                    Durum = s.Durum.ToString(),
                    BaslangicKm = s.KmBaslangic,
                    BitisKm = s.KmBitis,
                    Tamamlandi = s.Durum == CalismaDurum.Tamamlandi,
                    ToplamKm = (s.KmBitis ?? 0) - (s.KmBaslangic ?? 0)
                })
                .ToListAsync();

            return Ok(seferler);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sefer geçmişi getirilirken hata");
            return StatusCode(500, new { Error = "Sefer geçmişi yüklenirken hata oluştu" });
        }
    }

    /// <summary>
    /// Belirli bir seferin detaylarını getirir
    /// </summary>
    [HttpGet("seferler/{seferId}")]
    public async Task<IActionResult> GetSefer(int seferId)
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var soforId = await GetSoforIdAsync(context);

            var sefer = await context.ServisCalismalari
                .AsNoTracking()
                .Include(s => s.Arac)
                .Include(s => s.Guzergah)
                .Where(s => s.Id == seferId && (soforId == null || s.SoforId == soforId.Value))
                .Select(s => new MobileSeferGecmisOzet
                {
                    Id = s.Id,
                    AracId = s.AracId,
                    AracPlaka = s.Arac != null ? s.Arac.AktifPlaka ?? "" : "",
                    GuzergahId = s.GuzergahId,
                    GuzergahAdi = s.Guzergah != null ? s.Guzergah.GuzergahAdi ?? "" : "",
                    BaslangicZamani = s.CalismaTarihi.Add(s.BaslangicSaati ?? TimeSpan.Zero),
                    BitisZamani = s.BitisSaati.HasValue ? s.CalismaTarihi.Add(s.BitisSaati.Value) : (DateTime?)null,
                    Durum = s.Durum.ToString(),
                    BaslangicKm = s.KmBaslangic,
                    BitisKm = s.KmBitis,
                    Tamamlandi = s.Durum == CalismaDurum.Tamamlandi,
                    ToplamKm = (s.KmBitis ?? 0) - (s.KmBaslangic ?? 0)
                })
                .FirstOrDefaultAsync();

            if (sefer == null)
            {
                return NotFound(new { Error = "Sefer bulunamadı" });
            }

            return Ok(sefer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sefer detayı getirilirken hata: {SeferId}", seferId);
            return StatusCode(500, new { Error = "Sefer detayı yüklenirken hata oluştu" });
        }
    }

    /// <summary>
    /// Arıza bildirimi gönderir
    /// </summary>
    [HttpPost("ariza")]
    public async Task<IActionResult> ArizaBildir([FromBody] MobileArizaBildirimRequest request)
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var arac = await context.Araclar.FindAsync(request.AracId);
            if (arac == null)
            {
                return BadRequest(new { Error = "Araç bulunamadı" });
            }

            // Varsayılan departmanı bul veya ilk departmanı al
            var departman = await context.DestekDepartmanlari.FirstOrDefaultAsync();
            if (departman == null)
            {
                // Departman yoksa oluştur
                departman = new DestekDepartman
                {
                    Ad = "Teknik Destek",
                    Aciklama = "Araç arıza ve teknik destek talepleri",
                    Aktif = true,
                    CreatedAt = DateTime.Now
                };
                context.DestekDepartmanlari.Add(departman);
                await context.SaveChangesAsync();
            }

            // Talep numarası oluştur
            var talepNo = $"TKT-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

            var talep = new DestekTalebi
            {
                TalepNo = talepNo,
                Konu = $"Araç Arızası: {request.ArizaTipi}",
                Aciklama = $"Araç: {arac.AktifPlaka}\n" +
                          $"Arıza Tipi: {request.ArizaTipi}\n" +
                          $"Açıklama: {request.Aciklama}\n" +
                          $"Konum: {request.Enlem?.ToString("F5")}, {request.Boylam?.ToString("F5")}",
                Oncelik = request.OncelikSeviyesi switch
                {
                    "Acil" => DestekOncelik.Acil,
                    "Yuksek" => DestekOncelik.Yuksek,
                    "Dusuk" => DestekOncelik.Dusuk,
                    _ => DestekOncelik.Normal
                },
                Durum = DestekDurum.Yeni,
                Kaynak = DestekKaynak.MobilUygulama,
                DepartmanId = departman.Id,
                OlusturanKullaniciId = GetCurrentKullaniciId(),
                MusteriAdi = "Mobil Uygulama",
                MusteriEmail = "mobil@sistem.local",
                SonAktiviteTarihi = DateTime.UtcNow,
                CreatedAt = DateTime.Now
            };

            context.DestekTalepleri.Add(talep);
            await context.SaveChangesAsync();

            // Fotoğrafları kaydet
            if (request.FotografBase64?.Count > 0)
            {
                var klasor = Path.Combine("wwwroot", "uploads", "ariza");
                if (!Directory.Exists(klasor))
                    Directory.CreateDirectory(klasor);

                foreach (var foto in request.FotografBase64.Where(f => !string.IsNullOrEmpty(f)))
                {
                    var dosyaAdi = $"ariza_{talep.Id}_{Guid.NewGuid():N}.jpg";
                    var dosyaYolu = Path.Combine(klasor, dosyaAdi);
                    var bytes = Convert.FromBase64String(foto);
                    await System.IO.File.WriteAllBytesAsync(dosyaYolu, bytes);

                    var ek = new DestekTalebiEk
                    {
                        DestekTalebiId = talep.Id,
                        DosyaAdi = dosyaAdi,
                        OrijinalDosyaAdi = dosyaAdi,
                        DosyaYolu = $"/uploads/ariza/{dosyaAdi}",
                        DosyaBoyutu = bytes.Length,
                        MimeTipi = "image/jpeg",
                        CreatedAt = DateTime.Now
                    };
                    context.DestekTalebiEkleri.Add(ek);
                }
                await context.SaveChangesAsync();
            }

            _logger.LogInformation("Arıza bildirimi oluşturuldu: {TalepId}, Araç: {AracId}", talep.Id, request.AracId);

            return Ok(new { Success = true, TalepId = talep.Id, TalepNo = talep.TalepNo });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Arıza bildirimi gönderilirken hata");
            return StatusCode(500, new { Error = "Arıza bildirimi kaydedilirken hata oluştu" });
        }
    }

    #endregion
}

#region DTO Sınıfları

public class MobileSeferOzet
{
    public int Id { get; set; }
    public int AracId { get; set; }
    public string AracPlaka { get; set; } = string.Empty;
    public int GuzergahId { get; set; }
    public string GuzergahAdi { get; set; } = string.Empty;
    public TimeSpan? BaslangicSaati { get; set; }
    public TimeSpan? BitisSaati { get; set; }
    public string Durum { get; set; } = string.Empty;
    public int? BaslangicKm { get; set; }
    public int? BitisKm { get; set; }
}

public class MobileSeferDetay : MobileSeferOzet
{
    public string? Notlar { get; set; }
    public decimal? YakitTuketimi { get; set; }
}

public class MobileSeferGecmisOzet
{
    public int Id { get; set; }
    public int AracId { get; set; }
    public string AracPlaka { get; set; } = string.Empty;
    public int GuzergahId { get; set; }
    public string GuzergahAdi { get; set; } = string.Empty;
    public DateTime BaslangicZamani { get; set; }
    public DateTime? BitisZamani { get; set; }
    public string Durum { get; set; } = string.Empty;
    public int? BaslangicKm { get; set; }
    public int? BitisKm { get; set; }
    public bool Tamamlandi { get; set; }
    public int ToplamKm { get; set; }
}

public class MobileSeferBaslatRequest
{
    public int AracId { get; set; }
    public int GuzergahId { get; set; }
    public int BaslangicKm { get; set; }
    public double? BaslangicEnlem { get; set; }
    public double? BaslangicBoylam { get; set; }
    public string? Notlar { get; set; }
}

public class MobileSeferBitirRequest
{
    public int BitisKm { get; set; }
    public double? BitisEnlem { get; set; }
    public double? BitisBoylam { get; set; }
    public decimal? YakitTuketimi { get; set; }
    public string? Notlar { get; set; }
}

public class MobileKonumGonderRequest
{
    public int? SeferId { get; set; }
    public int? AracId { get; set; }
    public double Enlem { get; set; }
    public double Boylam { get; set; }
    public double? Hiz { get; set; }
    public double? Yon { get; set; }
    public bool? KontakDurumu { get; set; }
    public bool? MotorDurumu { get; set; }
    public decimal? YakitSeviyesi { get; set; }
}

public class MobileArizaBildirimRequest
{
    public int AracId { get; set; }
    public int? SeferId { get; set; }
    public string ArizaTipi { get; set; } = string.Empty;
    public string Aciklama { get; set; } = string.Empty;
    public string? OncelikSeviyesi { get; set; }
    public double? Enlem { get; set; }
    public double? Boylam { get; set; }
    public List<string>? FotografBase64 { get; set; }
}

public class MobileMasrafKayitRequest
{
    public int AracId { get; set; }
    public int MasrafKalemiId { get; set; }
    public decimal Tutar { get; set; }
    public DateTime Tarih { get; set; }
    public string? Aciklama { get; set; }
    public int? KmDurumu { get; set; }
    public double? Enlem { get; set; }
    public double? Boylam { get; set; }
    public string? FisGorseliBase64 { get; set; }
}

public class MobileMasrafKalemiOzet
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? Kategori { get; set; }
    public string? Ikon { get; set; }
}

#endregion
