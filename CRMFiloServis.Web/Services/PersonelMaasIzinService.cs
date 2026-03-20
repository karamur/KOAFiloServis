using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class PersonelMaasIzinService : IPersonelMaasIzinService
{
    private readonly ApplicationDbContext _context;

    public PersonelMaasIzinService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Maaþ Ýþlemleri

    public async Task<List<PersonelMaas>> GetMaaslarAsync(int yil, int ay)
    {
        return await _context.PersonelMaaslari
            .Include(m => m.Sofor)
            .Where(m => m.Yil == yil && m.Ay == ay)
            .OrderBy(m => m.Sofor.Ad)
            .ToListAsync();
    }

    public async Task<PersonelMaas?> GetMaasByIdAsync(int id)
    {
        return await _context.PersonelMaaslari
            .Include(m => m.Sofor)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<PersonelMaas?> GetMaasBySoforAsync(int soforId, int yil, int ay)
    {
        return await _context.PersonelMaaslari
            .Include(m => m.Sofor)
            .FirstOrDefaultAsync(m => m.SoforId == soforId && m.Yil == yil && m.Ay == ay);
    }

    public async Task<PersonelMaas> CreateMaasAsync(PersonelMaas maas)
    {
        _context.PersonelMaaslari.Add(maas);
        await _context.SaveChangesAsync();
        return maas;
    }

    public async Task<PersonelMaas> UpdateMaasAsync(PersonelMaas maas)
    {
        _context.PersonelMaaslari.Update(maas);
        await _context.SaveChangesAsync();
        return maas;
    }

    public async Task DeleteMaasAsync(int id)
    {
        var maas = await _context.PersonelMaaslari.FindAsync(id);
        if (maas != null)
        {
            maas.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<PersonelMaas>> GetSoforMaasGecmisiAsync(int soforId)
    {
        return await _context.PersonelMaaslari
            .Include(m => m.Sofor)
            .Where(m => m.SoforId == soforId)
            .OrderByDescending(m => m.Yil)
            .ThenByDescending(m => m.Ay)
            .ToListAsync();
    }

    public async Task MaasOdemeYapAsync(int maasId, DateTime odemeTarihi)
    {
        var maas = await _context.PersonelMaaslari.FindAsync(maasId);
        if (maas != null)
        {
            maas.OdemeTarihi = odemeTarihi;
            maas.OdemeDurum = MaasOdemeDurum.Odendi;
            await _context.SaveChangesAsync();
        }
    }

    public async Task TopluMaasOlusturAsync(int yil, int ay)
    {
        var aktifSoforler = await _context.Soforler
            .Where(s => s.Aktif)
            .ToListAsync();

        foreach (var sofor in aktifSoforler)
        {
            var mevcutMaas = await GetMaasBySoforAsync(sofor.Id, yil, ay);
            if (mevcutMaas == null)
            {
                var yeniMaas = new PersonelMaas
                {
                    SoforId = sofor.Id,
                    Yil = yil,
                    Ay = ay,
                    BrutMaas = sofor.BrutMaas,
                    NetMaas = sofor.NetMaas,
                    CalismaGunu = 26,
                    OdemeDurum = MaasOdemeDurum.Bekliyor
                };

                // SGK hesaplamalarý (2024 oranlarý)
                yeniMaas.SGKIsciPayi = yeniMaas.BrutMaas * 0.14m;
                yeniMaas.SGKIsverenPayi = yeniMaas.BrutMaas * 0.205m;
                yeniMaas.IssizlikPrimi = yeniMaas.BrutMaas * 0.01m;
                yeniMaas.DamgaVergisi = yeniMaas.BrutMaas * 0.00759m;

                // Gelir vergisi matrahý ve vergi hesaplamasý
                var vergiMatrahi = yeniMaas.BrutMaas - yeniMaas.SGKIsciPayi - yeniMaas.IssizlikPrimi;
                yeniMaas.GelirVergisi = vergiMatrahi * 0.15m; // Basit hesaplama

                _context.PersonelMaaslari.Add(yeniMaas);
            }
        }

        await _context.SaveChangesAsync();
    }

    #endregion

    #region Ýzin Ýþlemleri

    public async Task<List<PersonelIzin>> GetIzinlerAsync(int? soforId = null, DateTime? baslangic = null, DateTime? bitis = null)
    {
        var query = _context.PersonelIzinleri
            .Include(i => i.Sofor)
            .AsQueryable();

        if (soforId.HasValue)
            query = query.Where(i => i.SoforId == soforId.Value);

        if (baslangic.HasValue)
            query = query.Where(i => i.BitisTarihi >= baslangic.Value);

        if (bitis.HasValue)
            query = query.Where(i => i.BaslangicTarihi <= bitis.Value);

        return await query.OrderByDescending(i => i.BaslangicTarihi).ToListAsync();
    }

    public async Task<PersonelIzin?> GetIzinByIdAsync(int id)
    {
        return await _context.PersonelIzinleri
            .Include(i => i.Sofor)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<PersonelIzin> CreateIzinAsync(PersonelIzin izin)
    {
        _context.PersonelIzinleri.Add(izin);
        await _context.SaveChangesAsync();

        // Yýllýk izinse, izin hakkýndan düþ
        if (izin.IzinTipi == IzinTipi.YillikIzin && izin.Durum == IzinDurum.Onaylandi)
        {
            await UpdateIzinHakkiKullanýmAsync(izin.SoforId, izin.BaslangicTarihi.Year, izin.ToplamGun);
        }

        return izin;
    }

    public async Task<PersonelIzin> UpdateIzinAsync(PersonelIzin izin)
    {
        _context.PersonelIzinleri.Update(izin);
        await _context.SaveChangesAsync();
        return izin;
    }

    public async Task DeleteIzinAsync(int id)
    {
        var izin = await _context.PersonelIzinleri.FindAsync(id);
        if (izin != null)
        {
            izin.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task IzinOnaylaAsync(int izinId, string onaylayanKisi)
    {
        var izin = await _context.PersonelIzinleri.FindAsync(izinId);
        if (izin != null)
        {
            izin.Durum = IzinDurum.Onaylandi;
            izin.OnaylayanKisi = onaylayanKisi;
            izin.OnayTarihi = DateTime.Now;

            // Yýllýk izinse kullanýmý güncelle
            if (izin.IzinTipi == IzinTipi.YillikIzin)
            {
                await UpdateIzinHakkiKullanýmAsync(izin.SoforId, izin.BaslangicTarihi.Year, izin.ToplamGun);
            }

            await _context.SaveChangesAsync();
        }
    }

    public async Task IzinReddetAsync(int izinId, string redNedeni)
    {
        var izin = await _context.PersonelIzinleri.FindAsync(izinId);
        if (izin != null)
        {
            izin.Durum = IzinDurum.Reddedildi;
            izin.RedNedeni = redNedeni;
            await _context.SaveChangesAsync();
        }
    }

    private async Task UpdateIzinHakkiKullanýmAsync(int soforId, int yil, int gun)
    {
        var izinHakki = await GetIzinHakkiAsync(soforId, yil);
        if (izinHakki != null)
        {
            izinHakki.KullanilanIzin += gun;
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Ýzin Hakký Ýþlemleri

    public async Task<PersonelIzinHakki?> GetIzinHakkiAsync(int soforId, int yil)
    {
        return await _context.PersonelIzinHaklari
            .Include(h => h.Sofor)
            .FirstOrDefaultAsync(h => h.SoforId == soforId && h.Yil == yil);
    }

    public async Task<PersonelIzinHakki> CreateOrUpdateIzinHakkiAsync(PersonelIzinHakki izinHakki)
    {
        var mevcut = await GetIzinHakkiAsync(izinHakki.SoforId, izinHakki.Yil);
        if (mevcut == null)
        {
            _context.PersonelIzinHaklari.Add(izinHakki);
        }
        else
        {
            mevcut.YillikIzinHakki = izinHakki.YillikIzinHakki;
            mevcut.DevirenIzin = izinHakki.DevirenIzin;
            mevcut.Notlar = izinHakki.Notlar;
        }
        await _context.SaveChangesAsync();
        return izinHakki;
    }

    public async Task YillikIzinHaklariOlusturAsync(int yil)
    {
        var aktifSoforler = await _context.Soforler
            .Where(s => s.Aktif)
            .ToListAsync();

        foreach (var sofor in aktifSoforler)
        {
            var mevcutHak = await GetIzinHakkiAsync(sofor.Id, yil);
            if (mevcutHak == null)
            {
                // Önceki yýldan devreden izin
                var oncekiYilHak = await GetIzinHakkiAsync(sofor.Id, yil - 1);
                var devirenIzin = oncekiYilHak?.KalanIzin ?? 0;

                // Kýdem yýlýna göre izin hakký hesapla
                var kidemYili = sofor.IseBaslamaTarihi.HasValue 
                    ? (yil - sofor.IseBaslamaTarihi.Value.Year) 
                    : 0;

                var yillikHak = kidemYili switch
                {
                    < 1 => 14,
                    < 5 => 14,
                    < 15 => 20,
                    _ => 26
                };

                var yeniHak = new PersonelIzinHakki
                {
                    SoforId = sofor.Id,
                    Yil = yil,
                    YillikIzinHakki = yillikHak,
                    DevirenIzin = devirenIzin,
                    KullanilanIzin = 0
                };

                _context.PersonelIzinHaklari.Add(yeniHak);
            }
        }

        await _context.SaveChangesAsync();
    }

    #endregion

    #region Raporlar

    public async Task<MaasRaporOzet> GetMaasRaporuAsync(int yil, int ay)
    {
        var maaslar = await GetMaaslarAsync(yil, ay);

        var ozet = new MaasRaporOzet
        {
            Yil = yil,
            Ay = ay,
            PersonelSayisi = maaslar.Count,
            ToplamBrutMaas = maaslar.Sum(m => m.BrutMaas),
            ToplamNetMaas = maaslar.Sum(m => m.NetMaas),
            ToplamSGKIsci = maaslar.Sum(m => m.SGKIsciPayi),
            ToplamSGKIsveren = maaslar.Sum(m => m.SGKIsverenPayi),
            ToplamGelirVergisi = maaslar.Sum(m => m.GelirVergisi),
            ToplamOdeme = maaslar.Sum(m => m.OdenecekTutar),
            OdenmeyenSayisi = maaslar.Count(m => m.OdemeDurum != MaasOdemeDurum.Odendi),
            Detaylar = maaslar.Select(m => new MaasDetay
            {
                MaasId = m.Id,
                SoforId = m.SoforId,
                SoforAdSoyad = m.Sofor.TamAd,
                BrutMaas = m.BrutMaas,
                NetMaas = m.NetMaas,
                ToplamEklemeler = m.ToplamEklemeler,
                ToplamKesintiler = m.ToplamKesintiler,
                OdenecekTutar = m.OdenecekTutar,
                OdemeDurum = m.OdemeDurum.ToString(),
                OdemeTarihi = m.OdemeTarihi
            }).ToList()
        };

        return ozet;
    }

    public async Task<IzinRaporOzet> GetIzinRaporuAsync(int yil)
    {
        var izinHaklari = await _context.PersonelIzinHaklari
            .Include(h => h.Sofor)
            .Where(h => h.Yil == yil)
            .ToListAsync();

        var izinler = await _context.PersonelIzinleri
            .Include(i => i.Sofor)
            .Where(i => i.BaslangicTarihi.Year == yil && i.Durum == IzinDurum.Onaylandi)
            .ToListAsync();

        var ozet = new IzinRaporOzet
        {
            Yil = yil,
            ToplamPersonel = izinHaklari.Count,
            ToplamKullanilanIzin = izinHaklari.Sum(h => h.KullanilanIzin),
            ToplamKalanIzin = izinHaklari.Sum(h => h.KalanIzin),
            Detaylar = izinHaklari.Select(h => new IzinDetay
            {
                SoforId = h.SoforId,
                SoforAdSoyad = h.Sofor.TamAd,
                YillikHak = h.YillikIzinHakki,
                DevirenIzin = h.DevirenIzin,
                KullanilanIzin = h.KullanilanIzin,
                KalanIzin = h.KalanIzin,
                IzinKayitlari = izinler.Where(i => i.SoforId == h.SoforId).ToList()
            }).ToList(),
            TipBazliOzet = izinler
                .GroupBy(i => i.IzinTipi)
                .Select(g => new IzinTipiOzet
                {
                    IzinTipi = g.Key,
                    IzinTipiAdi = GetIzinTipiAdi(g.Key),
                    Adet = g.Count(),
                    ToplamGun = g.Sum(i => i.ToplamGun)
                }).ToList()
        };

        return ozet;
    }

    public async Task<List<PersonelOzet>> GetPersonelOzetListesiAsync()
    {
        var buAy = DateTime.Today.Month;
        var buYil = DateTime.Today.Year;

        var soforler = await _context.Soforler
            .Where(s => s.Aktif)
            .ToListAsync();

        var izinHaklari = await _context.PersonelIzinHaklari
            .Where(h => h.Yil == buYil)
            .ToListAsync();

        var seferler = await _context.ServisCalismalari
            .Where(s => s.CalismaTarihi.Month == buAy && s.CalismaTarihi.Year == buYil)
            .GroupBy(s => s.SoforId)
            .Select(g => new { SoforId = g.Key, Sayi = g.Count() })
            .ToListAsync();

        return soforler.Select(s => new PersonelOzet
        {
            SoforId = s.Id,
            SoforKodu = s.SoforKodu,
            AdSoyad = s.TamAd,
            IseBaslamaTarihi = s.IseBaslamaTarihi,
            BrutMaas = s.BrutMaas,
            NetMaas = s.NetMaas,
            KalanIzin = izinHaklari.FirstOrDefault(h => h.SoforId == s.Id)?.KalanIzin ?? 0,
            BuAySeferSayisi = seferler.FirstOrDefault(x => x.SoforId == s.Id)?.Sayi ?? 0,
            Aktif = s.Aktif
        }).ToList();
    }

    private string GetIzinTipiAdi(IzinTipi tip)
    {
        return tip switch
        {
            IzinTipi.YillikIzin => "Yýllýk Ýzin",
            IzinTipi.UcretsizIzin => "Ücretsiz Ýzin",
            IzinTipi.RaporluIzin => "Raporlu Ýzin",
            IzinTipi.MazeretIzni => "Mazeret Ýzni",
            IzinTipi.EvlilikIzni => "Evlilik Ýzni",
            IzinTipi.DogumIzni => "Doðum Ýzni",
            IzinTipi.OlumIzni => "Ölüm Ýzni",
            IzinTipi.IdariIzin => "Ýdari Ýzin",
            _ => tip.ToString()
        };
    }

    #endregion
}
