using KOAFiloServis.Shared.Entities;
using KOAFiloServis.Web.Data;
using KOAFiloServis.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace KOAFiloServis.Web.Services;

public class KolayMuhasebeService : IKolayMuhasebeService
{
    private readonly ApplicationDbContext _context;
    private readonly IMuhasebeService _muhasebeService;
    private readonly ICariService _cariService;

    public KolayMuhasebeService(
        ApplicationDbContext context,
        IMuhasebeService muhasebeService,
        ICariService cariService)
    {
        _context = context;
        _muhasebeService = muhasebeService;
        _cariService = cariService;
    }

    #region Önizleme Oluşturma

    public async Task<MuhasebeOnizleme> OnizlemeOlusturAsync(KolayMuhasebeGiris giris)
    {
        var ayar = await GetMuhasebeAyarAsync();
        var fisTipi = GetFisTipi(giris.IslemTuru);
        var fisNo = await _muhasebeService.GenerateNextFisNoAsync(fisTipi);

        var onizleme = new MuhasebeOnizleme
        {
            FisNo = fisNo,
            FisTarihi = giris.IslemTarihi,
            FisTipi = fisTipi,
            Aciklama = OlusturAciklama(giris)
        };

        // İşlem türüne göre kalemler oluştur
        var kalemler = giris.IslemTuru switch
        {
            KolayIslemTuru.GelirFatura => await OlusturGelirFaturaKalemleri(giris, ayar),
            KolayIslemTuru.GiderFatura => await OlusturGiderFaturaKalemleri(giris, ayar),
            KolayIslemTuru.MasrafGirisi => await OlusturMasrafKalemleri(giris, ayar),
            KolayIslemTuru.TahsilatGirisi => await OlusturTahsilatKalemleri(giris, ayar),
            KolayIslemTuru.OdemeGirisi => await OlusturOdemeKalemleri(giris, ayar),
            KolayIslemTuru.MahsupKaydi => await OlusturMahsupKalemleri(giris, ayar),
            KolayIslemTuru.AvansGirisi => await OlusturAvansKalemleri(giris, ayar),
            _ => new List<MuhasebeKalemOnizleme>()
        };

        onizleme.Kalemler = kalemler;
        return onizleme;
    }

    private async Task<List<MuhasebeKalemOnizleme>> OlusturGelirFaturaKalemleri(KolayMuhasebeGiris giris, MuhasebeAyar ayar)
    {
        var kalemler = new List<MuhasebeKalemOnizleme>();
        var siraNo = 1;

        // Cari hesap bilgisi al
        var cariHesapKodu = "120.01"; // Varsayılan alıcılar
        var cariUnvan = giris.CariUnvan ?? "Bilinmeyen Müşteri";

        if (giris.CariId.HasValue)
        {
            var cari = await _context.Cariler.AsNoTracking().Include(c => c.MuhasebeHesap).FirstOrDefaultAsync(c => c.Id == giris.CariId);
            if (cari != null)
            {
                cariUnvan = cari.Unvan;
                if (cari.MuhasebeHesap != null)
                    cariHesapKodu = cari.MuhasebeHesap.HesapKodu;
            }
        }

        var alicidanAlinacak = giris.TevkifatliMi ? giris.GenelToplam - giris.TevkifatTutar : giris.GenelToplam;

        // 120 Alıcılar BORÇ
        kalemler.Add(new MuhasebeKalemOnizleme
        {
            SiraNo = siraNo++,
            HesapKodu = cariHesapKodu,
            HesapAdi = $"Alıcılar - {cariUnvan}",
            HesapId = await GetHesapIdAsync(cariHesapKodu),
            Borc = alicidanAlinacak,
            Alacak = 0,
            Aciklama = $"Fatura: {giris.BelgeNo}",
            CariId = giris.CariId,
            CariUnvan = cariUnvan
        });

        // Tevkifat varsa - 136 Diğer Çeşitli Alacaklar BORÇ
        if (giris.TevkifatliMi && giris.TevkifatTutar > 0)
        {
            kalemler.Add(new MuhasebeKalemOnizleme
            {
                SiraNo = siraNo++,
                HesapKodu = ayar.TevkifatAlacakHesabi,
                HesapAdi = "Tevkifat Alacağı",
                HesapId = await GetHesapIdAsync(ayar.TevkifatAlacakHesabi),
                Borc = giris.TevkifatTutar,
                Alacak = 0,
                Aciklama = $"Tevkifat ({giris.TevkifatKodu})"
            });
        }

        // 600 Satışlar ALACAK
        kalemler.Add(new MuhasebeKalemOnizleme
        {
            SiraNo = siraNo++,
            HesapKodu = ayar.SatisGelirHesabi,
            HesapAdi = "Yurtiçi Satışlar",
            HesapId = await GetHesapIdAsync(ayar.SatisGelirHesabi),
            Borc = 0,
            Alacak = giris.AraToplam,
            Aciklama = "Satış Geliri"
        });

        // 391 Hesaplanan KDV ALACAK
        if (giris.KdvTutar > 0)
        {
            kalemler.Add(new MuhasebeKalemOnizleme
            {
                SiraNo = siraNo++,
                HesapKodu = ayar.HesaplananKdvHesabi,
                HesapAdi = "Hesaplanan KDV",
                HesapId = await GetHesapIdAsync(ayar.HesaplananKdvHesabi),
                Borc = 0,
                Alacak = giris.KdvTutar,
                Aciklama = $"KDV %{giris.KdvOrani}"
            });
        }

        return kalemler;
    }

    private async Task<List<MuhasebeKalemOnizleme>> OlusturGiderFaturaKalemleri(KolayMuhasebeGiris giris, MuhasebeAyar ayar)
    {
        var kalemler = new List<MuhasebeKalemOnizleme>();
        var siraNo = 1;

        // Cari hesap bilgisi al
        var cariHesapKodu = "320.01"; // Varsayılan satıcılar
        var cariUnvan = giris.CariUnvan ?? "Bilinmeyen Tedarikçi";

        if (giris.CariId.HasValue)
        {
            var cari = await _context.Cariler.AsNoTracking().Include(c => c.MuhasebeHesap).FirstOrDefaultAsync(c => c.Id == giris.CariId);
            if (cari != null)
            {
                cariUnvan = cari.Unvan;
                if (cari.MuhasebeHesap != null)
                    cariHesapKodu = cari.MuhasebeHesap.HesapKodu;
            }
        }

        var saticiyaOdenecek = giris.TevkifatliMi ? giris.GenelToplam - giris.TevkifatTutar : giris.GenelToplam;

        // 770 Genel Yönetim Giderleri / 153 Ticari Mallar BORÇ
        var giderHesabi = ayar.AlisGiderHesabi;
        kalemler.Add(new MuhasebeKalemOnizleme
        {
            SiraNo = siraNo++,
            HesapKodu = giderHesabi,
            HesapAdi = "Gider/Mal Alışı",
            HesapId = await GetHesapIdAsync(giderHesabi),
            Borc = giris.AraToplam,
            Alacak = 0,
            Aciklama = giris.Aciklama ?? "Alış"
        });

        // 191 İndirilecek KDV BORÇ
        if (giris.KdvTutar > 0)
        {
            kalemler.Add(new MuhasebeKalemOnizleme
            {
                SiraNo = siraNo++,
                HesapKodu = ayar.IndirilecekKdvHesabi,
                HesapAdi = "İndirilecek KDV",
                HesapId = await GetHesapIdAsync(ayar.IndirilecekKdvHesabi),
                Borc = giris.KdvTutar,
                Alacak = 0,
                Aciklama = $"KDV %{giris.KdvOrani}"
            });
        }

        // 320 Satıcılar ALACAK
        kalemler.Add(new MuhasebeKalemOnizleme
        {
            SiraNo = siraNo++,
            HesapKodu = cariHesapKodu,
            HesapAdi = $"Satıcılar - {cariUnvan}",
            HesapId = await GetHesapIdAsync(cariHesapKodu),
            Borc = 0,
            Alacak = saticiyaOdenecek,
            Aciklama = $"Fatura: {giris.BelgeNo}",
            CariId = giris.CariId,
            CariUnvan = cariUnvan
        });

        // Tevkifat varsa - 360 Sorumlu Sıfatıyla Ödenen KDV ALACAK
        if (giris.TevkifatliMi && giris.TevkifatTutar > 0)
        {
            kalemler.Add(new MuhasebeKalemOnizleme
            {
                SiraNo = siraNo++,
                HesapKodu = ayar.TevkifatKdvHesabi,
                HesapAdi = "Sorumlu Sıfatıyla Ödenen KDV",
                HesapId = await GetHesapIdAsync(ayar.TevkifatKdvHesabi),
                Borc = 0,
                Alacak = giris.TevkifatTutar,
                Aciklama = $"Tevkifat KDV ({giris.TevkifatKodu})"
            });
        }

        return kalemler;
    }

    private async Task<List<MuhasebeKalemOnizleme>> OlusturMasrafKalemleri(KolayMuhasebeGiris giris, MuhasebeAyar ayar)
    {
        var kalemler = new List<MuhasebeKalemOnizleme>();
        var siraNo = 1;

        // Masraf kalemi hesap kodu
        var masrafHesapKodu = "770.01"; // Varsayılan genel yönetim giderleri
        var masrafAdi = "Genel Masraf";

        if (giris.MasrafKalemiId.HasValue)
        {
            var masrafKalemi = await _context.MasrafKalemleri.AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == giris.MasrafKalemiId);
            if (masrafKalemi != null)
            {
                masrafAdi = masrafKalemi.MasrafAdi;
                // MasrafKalemi'nde MuhasebeHesapKodu yok, kategori bazlı eşleme yapalım
                masrafHesapKodu = GetMasrafHesapKodu(masrafKalemi.Kategori);
            }
        }

        // 7xx Gider Hesabı BORÇ
        kalemler.Add(new MuhasebeKalemOnizleme
        {
            SiraNo = siraNo++,
            HesapKodu = masrafHesapKodu,
            HesapAdi = masrafAdi,
            HesapId = await GetHesapIdAsync(masrafHesapKodu),
            Borc = giris.AraToplam,
            Alacak = 0,
            Aciklama = giris.Aciklama ?? masrafAdi
        });

        // 191 İndirilecek KDV BORÇ (varsa)
        if (giris.KdvTutar > 0)
        {
            kalemler.Add(new MuhasebeKalemOnizleme
            {
                SiraNo = siraNo++,
                HesapKodu = ayar.IndirilecekKdvHesabi,
                HesapAdi = "İndirilecek KDV",
                HesapId = await GetHesapIdAsync(ayar.IndirilecekKdvHesabi),
                Borc = giris.KdvTutar,
                Alacak = 0,
                Aciklama = $"KDV %{giris.KdvOrani}"
            });
        }

        var (odemeHesapKodu, odemeHesapAdi, odemeCariId, odemeCariUnvan) = await GetMasrafOdemeHesabiAsync(giris);

        kalemler.Add(new MuhasebeKalemOnizleme
        {
            SiraNo = siraNo++,
            HesapKodu = odemeHesapKodu,
            HesapAdi = odemeHesapAdi,
            HesapId = await GetHesapIdAsync(odemeHesapKodu),
            Borc = 0,
            Alacak = giris.GenelToplam,
            Aciklama = $"Masraf ödemesi: {giris.BelgeNo}",
            CariId = odemeCariId,
            CariUnvan = odemeCariUnvan
        });

        return kalemler;
    }

    private async Task<List<MuhasebeKalemOnizleme>> OlusturTahsilatKalemleri(KolayMuhasebeGiris giris, MuhasebeAyar ayar)
    {
        var kalemler = new List<MuhasebeKalemOnizleme>();
        var siraNo = 1;

        // Banka/Kasa hesabı BORÇ
        var bankaHesapKodu = "100.01"; // Varsayılan kasa
        var bankaHesapAdi = "Kasa";

        if (giris.BankaHesapId.HasValue)
        {
            var bankaHesap = await _context.BankaHesaplari.AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == giris.BankaHesapId);
            if (bankaHesap != null)
            {
                bankaHesapAdi = bankaHesap.HesapAdi;
                bankaHesapKodu = bankaHesap.VarsayilanMuhasebeKodu ?? GetBankaHesapKodu(bankaHesap.HesapTipi);
            }
        }

        kalemler.Add(new MuhasebeKalemOnizleme
        {
            SiraNo = siraNo++,
            HesapKodu = bankaHesapKodu,
            HesapAdi = bankaHesapAdi,
            HesapId = await GetHesapIdAsync(bankaHesapKodu),
            Borc = giris.GenelToplam,
            Alacak = 0,
            Aciklama = $"Tahsilat: {giris.BelgeNo}"
        });

        // 120 Alıcılar ALACAK
        var cariHesapKodu = "120.01";
        var cariUnvan = giris.CariUnvan ?? "Müşteri";

        if (giris.CariId.HasValue)
        {
            var cari = await _context.Cariler.AsNoTracking().Include(c => c.MuhasebeHesap).FirstOrDefaultAsync(c => c.Id == giris.CariId);
            if (cari != null)
            {
                cariUnvan = cari.Unvan;
                if (cari.MuhasebeHesap != null)
                    cariHesapKodu = cari.MuhasebeHesap.HesapKodu;
            }
        }

        kalemler.Add(new MuhasebeKalemOnizleme
        {
            SiraNo = siraNo++,
            HesapKodu = cariHesapKodu,
            HesapAdi = $"Alıcılar - {cariUnvan}",
            HesapId = await GetHesapIdAsync(cariHesapKodu),
            Borc = 0,
            Alacak = giris.GenelToplam,
            Aciklama = $"Tahsilat: {giris.BelgeNo}",
            CariId = giris.CariId,
            CariUnvan = cariUnvan
        });

        return kalemler;
    }

    private async Task<List<MuhasebeKalemOnizleme>> OlusturOdemeKalemleri(KolayMuhasebeGiris giris, MuhasebeAyar ayar)
    {
        var kalemler = new List<MuhasebeKalemOnizleme>();
        var siraNo = 1;

        // 320 Satıcılar BORÇ
        var cariHesapKodu = "320.01";
        var cariUnvan = giris.CariUnvan ?? "Tedarikçi";

        if (giris.CariId.HasValue)
        {
            var cari = await _context.Cariler.AsNoTracking().Include(c => c.MuhasebeHesap).FirstOrDefaultAsync(c => c.Id == giris.CariId);
            if (cari != null)
            {
                cariUnvan = cari.Unvan;
                if (cari.MuhasebeHesap != null)
                    cariHesapKodu = cari.MuhasebeHesap.HesapKodu;
            }
        }

        kalemler.Add(new MuhasebeKalemOnizleme
        {
            SiraNo = siraNo++,
            HesapKodu = cariHesapKodu,
            HesapAdi = $"Satıcılar - {cariUnvan}",
            HesapId = await GetHesapIdAsync(cariHesapKodu),
            Borc = giris.GenelToplam,
            Alacak = 0,
            Aciklama = $"Ödeme: {giris.BelgeNo}",
            CariId = giris.CariId,
            CariUnvan = cariUnvan
        });

        // Banka/Kasa hesabı ALACAK
        var bankaHesapKodu = "100.01";
        var bankaHesapAdi = "Kasa";

        if (giris.BankaHesapId.HasValue)
        {
            var bankaHesap = await _context.BankaHesaplari.AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == giris.BankaHesapId);
            if (bankaHesap != null)
            {
                bankaHesapAdi = bankaHesap.HesapAdi;
                bankaHesapKodu = bankaHesap.VarsayilanMuhasebeKodu ?? GetBankaHesapKodu(bankaHesap.HesapTipi);
            }
        }

        kalemler.Add(new MuhasebeKalemOnizleme
        {
            SiraNo = siraNo++,
            HesapKodu = bankaHesapKodu,
            HesapAdi = bankaHesapAdi,
            HesapId = await GetHesapIdAsync(bankaHesapKodu),
            Borc = 0,
            Alacak = giris.GenelToplam,
            Aciklama = $"Ödeme: {giris.BelgeNo}"
        });

        return kalemler;
    }

    private async Task<List<MuhasebeKalemOnizleme>> OlusturMahsupKalemleri(KolayMuhasebeGiris giris, MuhasebeAyar ayar)
    {
        var kalemler = new List<MuhasebeKalemOnizleme>();
        var siraNo = 1;

        // Cari bilgisini al
        var cariHesapKodu = "120.01"; // Varsayılan alıcılar veya satıcılar
        var cariUnvan = giris.CariUnvan ?? "Cari";
        CariTipi? cariTipi = null;

        if (giris.CariId.HasValue)
        {
            var cari = await _context.Cariler.AsNoTracking().Include(c => c.MuhasebeHesap).FirstOrDefaultAsync(c => c.Id == giris.CariId);
            if (cari != null)
            {
                cariUnvan = cari.Unvan;
                cariTipi = cari.CariTipi;
                if (cari.MuhasebeHesap != null)
                    cariHesapKodu = cari.MuhasebeHesap.HesapKodu;
                else
                {
                    // Cari tipine göre varsayılan hesap
                    cariHesapKodu = cari.CariTipi switch
                    {
                        CariTipi.Musteri => "120.01", // Alıcılar
                        CariTipi.Tedarikci => "320.01", // Satıcılar
                        CariTipi.MusteriTedarikci => "120.01", // Varsayılan alıcı
                        CariTipi.Personel => "335.01", // Personele Borçlar
                        _ => "120.01"
                    };
                }
            }
        }

        // Banka/Kasa hesabı bilgisini al
        var bankaHesapKodu = "100.01";
        var bankaHesapAdi = "Kasa";

        if (giris.BankaHesapId.HasValue)
        {
            var bankaHesap = await _context.BankaHesaplari.AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == giris.BankaHesapId);
            if (bankaHesap != null)
            {
                bankaHesapAdi = bankaHesap.HesapAdi;
                bankaHesapKodu = bankaHesap.VarsayilanMuhasebeKodu ?? GetBankaHesapKodu(bankaHesap.HesapTipi);
            }
        }

        // Mahsup işlemi: Cari tipine göre borç/alacak belirlenir
        // Müşteri ise: Müşteriden tahsilat yapılıyor (Alıcılar azalıyor)
        // Tedarikçi ise: Tedarikçiye ödeme yapılıyor (Satıcılar azalıyor)
        bool musteriMahsup = cariTipi == CariTipi.Musteri || cariTipi == CariTipi.MusteriTedarikci;

        if (musteriMahsup)
        {
            // Müşteri mahsubu: Banka/Kasa BORÇ, Alıcılar ALACAK
            kalemler.Add(new MuhasebeKalemOnizleme
            {
                SiraNo = siraNo++,
                HesapKodu = bankaHesapKodu,
                HesapAdi = bankaHesapAdi,
                HesapId = await GetHesapIdAsync(bankaHesapKodu),
                Borc = giris.GenelToplam,
                Alacak = 0,
                Aciklama = $"Mahsup tahsilat: {giris.BelgeNo}"
            });

            kalemler.Add(new MuhasebeKalemOnizleme
            {
                SiraNo = siraNo++,
                HesapKodu = cariHesapKodu,
                HesapAdi = $"Alıcılar - {cariUnvan}",
                HesapId = await GetHesapIdAsync(cariHesapKodu),
                Borc = 0,
                Alacak = giris.GenelToplam,
                Aciklama = $"Mahsup: {giris.BelgeNo}",
                CariId = giris.CariId,
                CariUnvan = cariUnvan
            });
        }
        else
        {
            // Tedarikçi/Personel mahsubu: Satıcılar BORÇ, Banka/Kasa ALACAK
            kalemler.Add(new MuhasebeKalemOnizleme
            {
                SiraNo = siraNo++,
                HesapKodu = cariHesapKodu,
                HesapAdi = $"Satıcılar - {cariUnvan}",
                HesapId = await GetHesapIdAsync(cariHesapKodu),
                Borc = giris.GenelToplam,
                Alacak = 0,
                Aciklama = $"Mahsup ödeme: {giris.BelgeNo}",
                CariId = giris.CariId,
                CariUnvan = cariUnvan
            });

            kalemler.Add(new MuhasebeKalemOnizleme
            {
                SiraNo = siraNo++,
                HesapKodu = bankaHesapKodu,
                HesapAdi = bankaHesapAdi,
                HesapId = await GetHesapIdAsync(bankaHesapKodu),
                Borc = 0,
                Alacak = giris.GenelToplam,
                Aciklama = $"Mahsup: {giris.BelgeNo}"
            });
        }

        return kalemler;
    }

    private async Task<List<MuhasebeKalemOnizleme>> OlusturAvansKalemleri(KolayMuhasebeGiris giris, MuhasebeAyar ayar)
    {
        var kalemler = new List<MuhasebeKalemOnizleme>();
        var siraNo = 1;

        // 195 İş Avansları BORÇ
        kalemler.Add(new MuhasebeKalemOnizleme
        {
            SiraNo = siraNo++,
            HesapKodu = "195.01",
            HesapAdi = "Personel Avansları",
            HesapId = await GetHesapIdAsync("195.01"),
            Borc = giris.GenelToplam,
            Alacak = 0,
            Aciklama = giris.Aciklama ?? "Personel avansı",
            CariId = giris.CariId,
            CariUnvan = giris.CariUnvan
        });

        // Banka/Kasa ALACAK
        var bankaHesapKodu = "100.01";
        var bankaHesapAdi = "Kasa";

        if (giris.BankaHesapId.HasValue)
        {
            var bankaHesap = await _context.BankaHesaplari.AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == giris.BankaHesapId);
            if (bankaHesap != null)
            {
                bankaHesapAdi = bankaHesap.HesapAdi;
                bankaHesapKodu = bankaHesap.VarsayilanMuhasebeKodu ?? GetBankaHesapKodu(bankaHesap.HesapTipi);
            }
        }

        kalemler.Add(new MuhasebeKalemOnizleme
        {
            SiraNo = siraNo++,
            HesapKodu = bankaHesapKodu,
            HesapAdi = bankaHesapAdi,
            HesapId = await GetHesapIdAsync(bankaHesapKodu),
            Borc = 0,
            Alacak = giris.GenelToplam,
            Aciklama = "Avans ödemesi"
        });

        return kalemler;
    }

    #endregion

    #region Kaydetme

    public async Task<KolayMuhasebeSonuc> KaydetAsync(KolayMuhasebeGiris giris, MuhasebeOnizleme? manuelOnizleme = null)
    {
        var sonuc = new KolayMuhasebeSonuc();

        try
        {
            // Yeni cari oluştur (gerekirse)
            if (!giris.CariId.HasValue && !string.IsNullOrWhiteSpace(giris.CariUnvan) && giris.YeniCariTipi.HasValue)
            {
                var yeniCari = await HizliCariOlusturAsync(giris.CariUnvan, giris.YeniCariTipi.Value);
                giris.CariId = yeniCari.Id;
                sonuc.CariId = yeniCari.Id;
                sonuc.Uyarilar.Add($"Yeni cari oluşturuldu: {yeniCari.Unvan}");
            }

            if (CariMuhasebeEslemeGerekliMi(giris) && giris.CariId.HasValue)
            {
                await _cariService.EnsureMuhasebeHesapAsync(giris.CariId.Value);
            }

            // Önizleme oluştur (verilmemişse)
            var onizleme = await OnizlemeOlusturAsync(giris);

            // Dengeli mi kontrol et
            if (!onizleme.Dengeli)
            {
                sonuc.Basarili = false;
                sonuc.Mesaj = $"Muhasebe kaydı dengeli değil! Borç: {onizleme.ToplamBorc:N2}, Alacak: {onizleme.ToplamAlacak:N2}";
                return sonuc;
            }

            // İşlem türüne göre kayıtları oluştur
            switch (giris.IslemTuru)
            {
                case KolayIslemTuru.GelirFatura:
                    sonuc = await KaydetGelirFatura(giris, onizleme);
                    break;
                case KolayIslemTuru.GiderFatura:
                    sonuc = await KaydetGiderFatura(giris, onizleme);
                    break;
                case KolayIslemTuru.MasrafGirisi:
                    sonuc = await KaydetMasraf(giris, onizleme);
                    break;
                case KolayIslemTuru.TahsilatGirisi:
                    sonuc = await KaydetTahsilat(giris, onizleme);
                    break;
                case KolayIslemTuru.OdemeGirisi:
                    sonuc = await KaydetOdeme(giris, onizleme);
                    break;
                case KolayIslemTuru.AvansGirisi:
                    sonuc = await KaydetAvans(giris, onizleme);
                    break;
                default:
                    // Sadece muhasebe fişi oluştur
                    sonuc.MuhasebeFisId = await KaydetMuhasebeFisi(onizleme);
                    sonuc.Basarili = true;
                    sonuc.Mesaj = "Muhasebe fişi oluşturuldu.";
                    break;
            }
        }
        catch (Exception ex)
        {
            sonuc.Basarili = false;
            sonuc.Mesaj = $"Hata: {ex.Message}";
        }

        return sonuc;
    }

    private async Task<KolayMuhasebeSonuc> KaydetGelirFatura(KolayMuhasebeGiris giris, MuhasebeOnizleme onizleme)
    {
        var sonuc = new KolayMuhasebeSonuc();

        // Fatura oluştur
        var fatura = new Fatura
        {
            FaturaNo = giris.BelgeNo ?? await GenerateFaturaNo("SF"),
            FaturaTarihi = DateTime.SpecifyKind(giris.IslemTarihi, DateTimeKind.Utc),
            VadeTarihi = giris.VadeTarihi.HasValue ? DateTime.SpecifyKind(giris.VadeTarihi.Value, DateTimeKind.Utc) : null,
            FaturaTipi = FaturaTipi.SatisFaturasi,
            FaturaYonu = FaturaYonu.Giden,
            EFaturaTipi = EFaturaTipi.EArsiv,
            CariId = giris.CariId ?? 0,
            AraToplam = giris.AraToplam,
            KdvOrani = giris.KdvOrani,
            KdvTutar = giris.KdvTutar,
            GenelToplam = giris.GenelToplam,
            TevkifatliMi = giris.TevkifatliMi,
            TevkifatOrani = giris.TevkifatOrani,
            TevkifatKodu = giris.TevkifatKodu,
            TevkifatTutar = giris.TevkifatTutar,
            Aciklama = giris.Aciklama,
            Notlar = giris.Notlar,
            Durum = FaturaDurum.Beklemede,
            ImportKaynak = "KolayGiris",
            CreatedAt = DateTime.UtcNow
        };

        _context.Faturalar.Add(fatura);
        await _context.SaveChangesAsync();
        sonuc.FaturaId = fatura.Id;

        // Muhasebe fişi oluştur
        sonuc.MuhasebeFisId = await KaydetMuhasebeFisi(onizleme, FisKaynak.Fatura, fatura.Id);

        // Faturayı güncelle
        fatura.MuhasebeFisiOlusturuldu = true;
        fatura.MuhasebeFisId = sonuc.MuhasebeFisId;
        _context.Faturalar.Update(fatura);
        await _context.SaveChangesAsync();

        sonuc.Basarili = true;
        sonuc.Mesaj = $"Gelir faturası ve muhasebe kaydı oluşturuldu. Fatura No: {fatura.FaturaNo}";
        return sonuc;
    }

    private async Task<KolayMuhasebeSonuc> KaydetGiderFatura(KolayMuhasebeGiris giris, MuhasebeOnizleme onizleme)
    {
        var sonuc = new KolayMuhasebeSonuc();

        // Fatura oluştur
        var fatura = new Fatura
        {
            FaturaNo = giris.BelgeNo ?? await GenerateFaturaNo("AF"),
            FaturaTarihi = DateTime.SpecifyKind(giris.IslemTarihi, DateTimeKind.Utc),
            VadeTarihi = giris.VadeTarihi.HasValue ? DateTime.SpecifyKind(giris.VadeTarihi.Value, DateTimeKind.Utc) : null,
            FaturaTipi = FaturaTipi.AlisFaturasi,
            FaturaYonu = FaturaYonu.Gelen,
            EFaturaTipi = EFaturaTipi.EFatura,
            CariId = giris.CariId ?? 0,
            AraToplam = giris.AraToplam,
            KdvOrani = giris.KdvOrani,
            KdvTutar = giris.KdvTutar,
            GenelToplam = giris.GenelToplam,
            TevkifatliMi = giris.TevkifatliMi,
            TevkifatOrani = giris.TevkifatOrani,
            TevkifatKodu = giris.TevkifatKodu,
            TevkifatTutar = giris.TevkifatTutar,
            Aciklama = giris.Aciklama,
            Notlar = giris.Notlar,
            Durum = FaturaDurum.Beklemede,
            ImportKaynak = "KolayGiris",
            CreatedAt = DateTime.UtcNow
        };

        _context.Faturalar.Add(fatura);
        await _context.SaveChangesAsync();
        sonuc.FaturaId = fatura.Id;

        // Muhasebe fişi oluştur
        sonuc.MuhasebeFisId = await KaydetMuhasebeFisi(onizleme, FisKaynak.Fatura, fatura.Id);

        // Faturayı güncelle
        fatura.MuhasebeFisiOlusturuldu = true;
        fatura.MuhasebeFisId = sonuc.MuhasebeFisId;
        _context.Faturalar.Update(fatura);
        await _context.SaveChangesAsync();

        sonuc.Basarili = true;
        sonuc.Mesaj = $"Gider faturası ve muhasebe kaydı oluşturuldu. Fatura No: {fatura.FaturaNo}";
        return sonuc;
    }

    private async Task<KolayMuhasebeSonuc> KaydetMasraf(KolayMuhasebeGiris giris, MuhasebeOnizleme onizleme)
    {
        var sonuc = new KolayMuhasebeSonuc();

        // Araç masrafı mı?
        if (giris.AracId.HasValue && giris.MasrafKalemiId.HasValue)
        {
            var masraf = new AracMasraf
            {
                MasrafTarihi = DateTime.SpecifyKind(giris.IslemTarihi, DateTimeKind.Utc),
                AracId = giris.AracId.Value,
                MasrafKalemiId = giris.MasrafKalemiId.Value,
                Tutar = giris.GenelToplam,
                BelgeNo = giris.BelgeNo,
                Aciklama = giris.Aciklama,
                CariId = giris.CariId,
                CreatedAt = DateTime.UtcNow
            };

            _context.AracMasraflari.Add(masraf);
            await _context.SaveChangesAsync();
            sonuc.MasrafId = masraf.Id;

            // Muhasebe fişi
            sonuc.MuhasebeFisId = await KaydetMuhasebeFisi(onizleme, FisKaynak.Otomatik, masraf.Id, "AracMasraf");
            masraf.MuhasebeFisId = sonuc.MuhasebeFisId;
            _context.AracMasraflari.Update(masraf);
            await _context.SaveChangesAsync();
        }
        else
        {
            // Genel masraf - sadece muhasebe fişi
            sonuc.MuhasebeFisId = await KaydetMuhasebeFisi(onizleme, FisKaynak.Manuel, null, "Masraf");
        }

        // Banka hareketi oluştur (ödeme yapıldıysa)
        if (giris.MasrafOdemeKaynagi == MasrafOdemeKaynagi.KasaBanka && giris.BankaHesapId.HasValue)
        {
            var hareket = new BankaKasaHareket
            {
                BankaHesapId = giris.BankaHesapId.Value,
                IslemNo = await GenerateIslemNo(),
                IslemTarihi = DateTime.SpecifyKind(giris.IslemTarihi, DateTimeKind.Utc),
                Tutar = giris.GenelToplam,
                HareketTipi = HareketTipi.Cikis,
                Aciklama = $"Masraf: {giris.Aciklama ?? giris.BelgeNo}",
                CariId = giris.CariId,
                IslemKaynak = IslemKaynak.Manuel,
                CreatedAt = DateTime.UtcNow
            };

            _context.BankaKasaHareketleri.Add(hareket);
            await _context.SaveChangesAsync();
            sonuc.BankaHareketId = hareket.Id;
        }

        sonuc.Basarili = true;
        sonuc.Mesaj = giris.MasrafOdemeKaynagi switch
        {
            MasrafOdemeKaynagi.Personel => "Masraf personel alacağı olarak muhasebeleştirildi.",
            MasrafOdemeKaynagi.Cari => "Masraf cari alacağı olarak muhasebeleştirildi.",
            _ => "Masraf ve muhasebe kaydı oluşturuldu."
        };
        return sonuc;
    }

    private async Task<KolayMuhasebeSonuc> KaydetTahsilat(KolayMuhasebeGiris giris, MuhasebeOnizleme onizleme)
    {
        var sonuc = new KolayMuhasebeSonuc();

        if (!giris.BankaHesapId.HasValue)
        {
            sonuc.Basarili = false;
            sonuc.Mesaj = "Tahsilat için banka/kasa hesabı seçilmeli.";
            return sonuc;
        }

        // Banka hareketi oluştur
        var hareket = new BankaKasaHareket
        {
            BankaHesapId = giris.BankaHesapId.Value,
            IslemNo = await GenerateIslemNo(),
            IslemTarihi = DateTime.SpecifyKind(giris.IslemTarihi, DateTimeKind.Utc),
            Tutar = giris.GenelToplam,
            HareketTipi = HareketTipi.Giris,
            Aciklama = $"Tahsilat: {giris.CariUnvan} - {giris.BelgeNo}",
            CariId = giris.CariId,
            IslemKaynak = IslemKaynak.FaturaTahsilat,
            CreatedAt = DateTime.UtcNow
        };

        _context.BankaKasaHareketleri.Add(hareket);
        await _context.SaveChangesAsync();
        sonuc.BankaHareketId = hareket.Id;

        // Muhasebe fişi
        sonuc.MuhasebeFisId = await KaydetMuhasebeFisi(onizleme, FisKaynak.BankaHareket, hareket.Id);

        sonuc.Basarili = true;
        sonuc.Mesaj = $"Tahsilat kaydedildi. Tutar: {giris.GenelToplam:N2} TL";
        return sonuc;
    }

    private async Task<KolayMuhasebeSonuc> KaydetOdeme(KolayMuhasebeGiris giris, MuhasebeOnizleme onizleme)
    {
        var sonuc = new KolayMuhasebeSonuc();

        if (!giris.BankaHesapId.HasValue)
        {
            sonuc.Basarili = false;
            sonuc.Mesaj = "Ödeme için banka/kasa hesabı seçilmeli.";
            return sonuc;
        }

        // Banka hareketi oluştur
        var hareket = new BankaKasaHareket
        {
            BankaHesapId = giris.BankaHesapId.Value,
            IslemNo = await GenerateIslemNo(),
            IslemTarihi = DateTime.SpecifyKind(giris.IslemTarihi, DateTimeKind.Utc),
            Tutar = giris.GenelToplam,
            HareketTipi = HareketTipi.Cikis,
            Aciklama = $"Ödeme: {giris.CariUnvan} - {giris.BelgeNo}",
            CariId = giris.CariId,
            IslemKaynak = IslemKaynak.FaturaOdeme,
            CreatedAt = DateTime.UtcNow
        };

        _context.BankaKasaHareketleri.Add(hareket);
        await _context.SaveChangesAsync();
        sonuc.BankaHareketId = hareket.Id;

        // Muhasebe fişi
        sonuc.MuhasebeFisId = await KaydetMuhasebeFisi(onizleme, FisKaynak.BankaHareket, hareket.Id);

        sonuc.Basarili = true;
        sonuc.Mesaj = $"Ödeme kaydedildi. Tutar: {giris.GenelToplam:N2} TL";
        return sonuc;
    }

    private async Task<KolayMuhasebeSonuc> KaydetAvans(KolayMuhasebeGiris giris, MuhasebeOnizleme onizleme)
    {
        var sonuc = new KolayMuhasebeSonuc();

        if (!giris.BankaHesapId.HasValue)
        {
            sonuc.Basarili = false;
            sonuc.Mesaj = "Avans için banka/kasa hesabı seçilmeli.";
            return sonuc;
        }

        // Banka hareketi oluştur
        var hareket = new BankaKasaHareket
        {
            BankaHesapId = giris.BankaHesapId.Value,
            IslemNo = await GenerateIslemNo(),
            IslemTarihi = DateTime.SpecifyKind(giris.IslemTarihi, DateTimeKind.Utc),
            Tutar = giris.GenelToplam,
            HareketTipi = HareketTipi.Cikis,
            Aciklama = $"Personel Avansı: {giris.CariUnvan}",
            CariId = giris.CariId,
            IslemKaynak = IslemKaynak.Manuel,
            CreatedAt = DateTime.UtcNow
        };

        _context.BankaKasaHareketleri.Add(hareket);
        await _context.SaveChangesAsync();
        sonuc.BankaHareketId = hareket.Id;

        // Muhasebe fişi
        sonuc.MuhasebeFisId = await KaydetMuhasebeFisi(onizleme, FisKaynak.BankaHareket, hareket.Id);

        sonuc.Basarili = true;
        sonuc.Mesaj = $"Avans kaydedildi. Tutar: {giris.GenelToplam:N2} TL";
        return sonuc;
    }

    private async Task<int> KaydetMuhasebeFisi(MuhasebeOnizleme onizleme, FisKaynak kaynak = FisKaynak.Manuel, int? kaynakId = null, string? kaynakTip = null)
    {
        var fis = new MuhasebeFis
        {
            FisNo = onizleme.FisNo,
            FisTarihi = DateTime.SpecifyKind(onizleme.FisTarihi, DateTimeKind.Utc),
            FisTipi = onizleme.FisTipi,
            Aciklama = onizleme.Aciklama,
            ToplamBorc = onizleme.ToplamBorc,
            ToplamAlacak = onizleme.ToplamAlacak,
            Durum = FisDurum.Onaylandi,
            Kaynak = kaynak,
            KaynakId = kaynakId,
            KaynakTip = kaynakTip ?? GetKaynakTip(kaynak),
            CreatedAt = DateTime.UtcNow
        };

        _context.MuhasebeFisleri.Add(fis);
        await _context.SaveChangesAsync();

        // Kalemleri ekle
        foreach (var kalem in onizleme.Kalemler)
        {
            var fisKalem = new MuhasebeFisKalem
            {
                FisId = fis.Id,
                HesapId = kalem.HesapId ?? 1, // Varsayılan hesap
                SiraNo = kalem.SiraNo,
                Borc = kalem.Borc,
                Alacak = kalem.Alacak,
                Aciklama = kalem.Aciklama,
                CariId = kalem.CariId,
                Tarih = DateTime.SpecifyKind(onizleme.FisTarihi, DateTimeKind.Utc),
                CreatedAt = DateTime.UtcNow
            };

            _context.MuhasebeFisKalemleri.Add(fisKalem);
        }

        await _context.SaveChangesAsync();
        return fis.Id;
    }

    #endregion

    #region Yardımcı Metodlar

    public async Task<List<Cari>> GetCarilerAsync(CariTipi? tip = null, string? arama = null)
    {
        var query = _context.Cariler.AsNoTracking().Where(c => c.Aktif);

        if (tip.HasValue)
        {
            // Müşteri seçilmişse: Müşteri ve MüşteriTedarikçi
            // Tedarikçi seçilmişse: Tedarikçi ve MüşteriTedarikçi
            // Personel seçilmişse: Sadece Personel
            if (tip.Value == CariTipi.Personel)
            {
                query = query.Where(c => c.CariTipi == CariTipi.Personel);
            }
            else
            {
                query = query.Where(c => c.CariTipi == tip.Value || c.CariTipi == CariTipi.MusteriTedarikci);
            }
        }

        if (!string.IsNullOrWhiteSpace(arama))
            query = query.Where(c => c.Unvan.Contains(arama) || c.CariKodu.Contains(arama));

        return await query.OrderBy(c => c.Unvan).Take(50).ToListAsync();
    }

    public async Task<List<MasrafKalemiBasit>> GetMasrafKalemleriAsync()
    {
        return await _context.MasrafKalemleri
            .AsNoTracking()
            .Where(m => m.Aktif)
            .OrderBy(m => m.MasrafAdi)
            .Select(m => new MasrafKalemiBasit
            {
                Id = m.Id,
                Ad = m.MasrafAdi,
                MuhasebeHesapKodu = GetMasrafHesapKodu(m.Kategori)
            })
            .ToListAsync();
    }

    public async Task<List<BankaHesapBasit>> GetBankaHesaplariAsync()
    {
        return await _context.BankaHesaplari
            .AsNoTracking()
            .Where(b => b.Aktif)
            .OrderBy(b => b.HesapAdi)
            .Select(b => new BankaHesapBasit
            {
                Id = b.Id,
                HesapAdi = b.HesapAdi,
                MuhasebeHesapKodu = b.VarsayilanMuhasebeKodu,
                Bakiye = b.AcilisBakiye // Gerçek bakiye hesaplanmalı
            })
            .ToListAsync();
    }

    public async Task<List<Arac>> GetAraclarAsync()
    {
        return await _context.Araclar
            .AsNoTracking()
            .Where(a => a.Aktif)
            .OrderBy(a => a.AktifPlaka)
            .ToListAsync();
    }

    public async Task<Cari> HizliCariOlusturAsync(string unvan, CariTipi tip)
    {
        var cari = new Cari
        {
            CariKodu = await _cariService.GenerateNextKodAsync(),
            Unvan = unvan,
            CariTipi = tip,
            Aktif = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Cariler.Add(cari);
        await _context.SaveChangesAsync();

        if (tip == CariTipi.Personel)
        {
            cari = await _cariService.EnsureMuhasebeHesapAsync(cari.Id);
        }

        return cari;
    }

    private async Task<(string HesapKodu, string HesapAdi, int? CariId, string? CariUnvan)> GetMasrafOdemeHesabiAsync(KolayMuhasebeGiris giris)
    {
        if (giris.MasrafOdemeKaynagi == MasrafOdemeKaynagi.KasaBanka)
        {
            var odemeHesapKodu = "100.01";
            var odemeHesapAdi = "Kasa";

            if (giris.BankaHesapId.HasValue)
            {
                var bankaHesap = await _context.BankaHesaplari.AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Id == giris.BankaHesapId);
                if (bankaHesap != null)
                {
                    odemeHesapAdi = bankaHesap.HesapAdi;
                    odemeHesapKodu = bankaHesap.VarsayilanMuhasebeKodu ?? GetBankaHesapKodu(bankaHesap.HesapTipi);
                }
            }

            return (odemeHesapKodu, odemeHesapAdi, null, null);
        }

        if (!giris.CariId.HasValue)
        {
            return giris.MasrafOdemeKaynagi == MasrafOdemeKaynagi.Personel
                ? ("335.01", "Personele Borçlar", null, null)
                : ("320.01", "Satıcılar", null, null);
        }

        var cari = await _context.Cariler
            .AsNoTracking()
            .Include(c => c.MuhasebeHesap)
            .FirstOrDefaultAsync(c => c.Id == giris.CariId.Value);

        if (cari == null)
        {
            return giris.MasrafOdemeKaynagi == MasrafOdemeKaynagi.Personel
                ? ("335.01", "Personele Borçlar", giris.CariId, giris.CariUnvan)
                : ("320.01", "Satıcılar", giris.CariId, giris.CariUnvan);
        }

        var varsayilanKod = giris.MasrafOdemeKaynagi == MasrafOdemeKaynagi.Personel
            ? "335.01"
            : cari.CariTipi == CariTipi.Personel
                ? "335.01"
                : "320.01";

        var hesapKodu = cari.MuhasebeHesap?.HesapKodu ?? varsayilanKod;
        var hesapAdi = cari.MuhasebeHesap?.HesapAdi
            ?? (giris.MasrafOdemeKaynagi == MasrafOdemeKaynagi.Personel ? $"Personele Borçlar - {cari.Unvan}" : $"Cari Alacak - {cari.Unvan}");

        return (hesapKodu, hesapAdi, cari.Id, cari.Unvan);
    }

    private static bool CariMuhasebeEslemeGerekliMi(KolayMuhasebeGiris giris)
    {
        return giris.CariId.HasValue &&
               (giris.IslemTuru == KolayIslemTuru.AvansGirisi
                || (giris.IslemTuru == KolayIslemTuru.MasrafGirisi && giris.MasrafOdemeKaynagi != MasrafOdemeKaynagi.KasaBanka));
    }

    public async Task<Cari> HizliCariOlusturDetayliAsync(HizliCariModel model)
    {
        var cari = new Cari
        {
            CariKodu = await _cariService.GenerateNextKodAsync(),
            Unvan = model.Unvan,
            CariTipi = model.CariTipi,
            Aktif = true,
            CreatedAt = DateTime.UtcNow
        };

        // Personel için TC, diğerleri için Vergi No
        if (model.CariTipi == CariTipi.Personel)
        {
            cari.TcKimlikNo = model.VergiNo;
        }
        else
        {
            cari.VergiNo = model.VergiNo;
        }

        cari.Telefon = model.Telefon;
        cari.Email = model.Email;
        cari.Adres = model.Adres;

        _context.Cariler.Add(cari);
        await _context.SaveChangesAsync();

        if (model.CariTipi == CariTipi.Personel)
        {
            cari = await _cariService.EnsureMuhasebeHesapAsync(cari.Id);
        }

        return cari;
    }

    public async Task<List<MuhasebeHesap>> GetMuhasebeHesaplariAsync()
    {
        return await _context.MuhasebeHesaplari
            .AsNoTracking()
            .Where(h => h.Aktif)
            .OrderBy(h => h.HesapKodu)
            .ToListAsync();
    }

    public async Task<MuhasebeAyar> GetMuhasebeAyarAsync()
    {
        return await _context.MuhasebeAyarlari.FirstOrDefaultAsync() ?? new MuhasebeAyar();
    }

    private async Task<int?> GetHesapIdAsync(string hesapKodu)
    {
        var hesap = await _context.MuhasebeHesaplari
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.HesapKodu == hesapKodu || h.HesapKodu.StartsWith(hesapKodu.Split('.')[0]));
        return hesap?.Id;
    }

    private static FisTipi GetFisTipi(KolayIslemTuru islemTuru)
    {
        return islemTuru switch
        {
            KolayIslemTuru.TahsilatGirisi => FisTipi.Tahsilat,
            KolayIslemTuru.OdemeGirisi or KolayIslemTuru.AvansGirisi => FisTipi.Tediye,
            _ => FisTipi.Mahsup
        };
    }

    private static string OlusturAciklama(KolayMuhasebeGiris giris)
    {
        var islemAdi = giris.IslemTuru switch
        {
            KolayIslemTuru.GelirFatura => "Satış Faturası",
            KolayIslemTuru.GiderFatura => "Alış Faturası",
            KolayIslemTuru.MasrafGirisi => "Masraf",
            KolayIslemTuru.TahsilatGirisi => "Tahsilat",
            KolayIslemTuru.OdemeGirisi => "Ödeme",
            KolayIslemTuru.MahsupKaydi => "Mahsup",
            KolayIslemTuru.AvansGirisi => "Avans",
            _ => "İşlem"
        };

        return $"{islemAdi}: {giris.BelgeNo ?? giris.CariUnvan ?? giris.Aciklama ?? "-"}";
    }

    private static string GetKaynakTip(FisKaynak kaynak)
    {
        return kaynak switch
        {
            FisKaynak.Fatura => "Fatura",
            FisKaynak.BankaHareket => "BankaHareket",
            FisKaynak.Butce => "Butce",
            _ => "Manuel"
        };
    }

    private static string GetMasrafHesapKodu(MasrafKategori kategori)
    {
        return kategori switch
        {
            MasrafKategori.Yakit => "770.01",
            MasrafKategori.Bakim => "770.02",
            MasrafKategori.Tamir => "770.03",
            MasrafKategori.Sigorta => "770.04",
            MasrafKategori.Vergi => "770.05",
            MasrafKategori.Personel => "770.06",
            MasrafKategori.Lastik => "770.07",
            MasrafKategori.YedekParca => "770.08",
            _ => "770.99"
        };
    }

    private static string GetBankaHesapKodu(HesapTipi tip)
    {
        return tip switch
        {
            HesapTipi.Kasa => "100.01",
            HesapTipi.VadesizHesap or HesapTipi.VadeliHesap => "102.01",
            HesapTipi.KrediHesabi => "300.01",
            HesapTipi.KrediKarti => "103.01",
            _ => "102.01"
        };
    }

    private async Task<string> GenerateFaturaNo(string prefix)
    {
        var yil = DateTime.Now.Year;
        var lastNo = await _context.Faturalar
            .Where(f => f.FaturaNo.StartsWith($"{prefix}{yil}"))
            .OrderByDescending(f => f.FaturaNo)
            .Select(f => f.FaturaNo)
            .FirstOrDefaultAsync();

        var nextNum = 1;
        if (lastNo != null && int.TryParse(lastNo.Substring(prefix.Length + 4), out var num))
            nextNum = num + 1;

        return $"{prefix}{yil}{nextNum:D6}";
    }

    private async Task<string> GenerateIslemNo()
    {
        var yil = DateTime.Now.Year;
        var ay = DateTime.Now.Month;
        var prefix = $"ISL{yil}{ay:D2}";

        var lastNo = await _context.BankaKasaHareketleri
            .Where(h => h.IslemNo.StartsWith(prefix))
            .OrderByDescending(h => h.IslemNo)
            .Select(h => h.IslemNo)
            .FirstOrDefaultAsync();

        var nextNum = 1;
        if (lastNo != null && int.TryParse(lastNo.Substring(prefix.Length), out var num))
            nextNum = num + 1;

        return $"{prefix}{nextNum:D4}";
    }

    #endregion
}
