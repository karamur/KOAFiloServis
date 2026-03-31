using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using CRMFiloServis.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class FaturaHazirlikService : IFaturaHazirlikService
{
    private readonly ApplicationDbContext _context;

    public FaturaHazirlikService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FaturaHazirlikListesi> GetFaturaHazirlikListesiAsync(DateTime baslangicTarihi, DateTime bitisTarihi)
    {
        return await GetFaturaHazirlikListesiAsync(baslangicTarihi, bitisTarihi, null);
    }

    public async Task<FaturaHazirlikListesi> GetFaturaHazirlikListesiAsync(DateTime baslangicTarihi, DateTime bitisTarihi, int? cariId)
    {
        var liste = new FaturaHazirlikListesi
        {
            BaslangicTarihi = baslangicTarihi,
            BitisTarihi = bitisTarihi
        };

        // Tamamlanmış servis çalışmalarını getir
        var query = _context.ServisCalismalari
            .Include(s => s.Arac)
                .ThenInclude(a => a.KiralikCari)
            .Include(s => s.Arac)
                .ThenInclude(a => a.KomisyoncuCari)
            .Include(s => s.Sofor)
            .Include(s => s.Guzergah)
                .ThenInclude(g => g.Cari)
            .Where(s => s.CalismaTarihi >= baslangicTarihi && 
                        s.CalismaTarihi <= bitisTarihi &&
                        s.Durum == CalismaDurum.Tamamlandi);

        if (cariId.HasValue)
        {
            query = query.Where(s => s.Guzergah.CariId == cariId.Value);
        }

        var calismalar = await query.OrderBy(s => s.CalismaTarihi).ToListAsync();

        // 1. KELİLECEK FATURALAR (Müşterilere - Satış Faturaları)
        liste.KesilecekFaturalar = HesaplaKesilecekFaturalar(calismalar);

        // 2. GELECEK FATURALAR (Tedarikçilerden - Alış Faturaları)
        liste.GelecekFaturalar = HesaplaGelecekFaturalar(calismalar);

        return liste;
    }

    private List<KesilecekFaturaItem> HesaplaKesilecekFaturalar(List<ServisCalisma> calismalar)
    {
        var result = new List<KesilecekFaturaItem>();

        // Cari bazında grupla
        var cariGruplari = calismalar
            .GroupBy(c => c.Guzergah.CariId)
            .ToList();

        foreach (var cariGrup in cariGruplari)
        {
            var cari = cariGrup.First().Guzergah.Cari;

            var faturaItem = new KesilecekFaturaItem
            {
                CariId = cari.Id,
                CariUnvan = cari.Unvan,
                CariKodu = cari.CariKodu
            };

            // Güzergah bazında grupla
            var guzergahGruplari = cariGrup
                .GroupBy(c => c.GuzergahId)
                .ToList();

            foreach (var guzergahGrup in guzergahGruplari)
            {
                var guzergah = guzergahGrup.First().Guzergah;

                var detay = new KesilecekFaturaDetay
                {
                    GuzergahId = guzergah.Id,
                    GuzergahKodu = guzergah.GuzergahKodu,
                    GuzergahAdi = guzergah.GuzergahAdi,
                    BaslangicNoktasi = guzergah.BaslangicNoktasi,
                    BitisNoktasi = guzergah.BitisNoktasi,
                    SeferSayisi = guzergahGrup.Count(),
                    BirimFiyat = guzergah.BirimFiyat
                };

                // Her sefer için detay
                foreach (var calisma in guzergahGrup)
                {
                    var seferFiyat = calisma.Fiyat ?? guzergah.BirimFiyat;

                    var seferDetay = new SeferDetay
                    {
                        ServisCalismaId = calisma.Id,
                        Tarih = calisma.CalismaTarihi,
                        ServisTuru = calisma.ServisTuru.ToString(),
                        AracPlaka = calisma.Arac?.AktifPlaka ?? string.Empty,
                        SoforAdSoyad = $"{calisma.Sofor.Ad} {calisma.Sofor.Soyad}",
                        Fiyat = seferFiyat,
                        KiralikArac = calisma.Arac?.SahiplikTipi == AracSahiplikTipi.Kiralik,
                        KiralikAracSahibi = calisma.Arac?.KiralikCari?.Unvan,
                        KiraBedeli = calisma.Arac != null ? HesaplaKiraBedeli(calisma.Arac, seferFiyat) : 0,
                        KomisyonVar = calisma.Arac?.KomisyonVar ?? false,
                        KomisyoncuUnvan = calisma.Arac?.KomisyoncuCari?.Unvan,
                        KomisyonTutari = calisma.Arac != null ? HesaplaKomisyon(calisma.Arac, seferFiyat) : 0
                    };

                    detay.Seferler.Add(seferDetay);
                }

                detay.ToplamTutar = detay.Seferler.Sum(s => s.Fiyat);
                faturaItem.Detaylar.Add(detay);
            }

            result.Add(faturaItem);
        }

        return result.OrderBy(x => x.CariUnvan).ToList();
    }

    private List<GelecekFaturaItem> HesaplaGelecekFaturalar(List<ServisCalisma> calismalar)
    {
        var result = new List<GelecekFaturaItem>();

        // 1. KİRALIK ARAÇ ÖDEMELERİ
        var kiralikAracCalismalar = calismalar
            .Where(c => c.Arac.SahiplikTipi == AracSahiplikTipi.Kiralik && c.Arac.KiralikCariId.HasValue)
            .ToList();

        var kiraGruplari = kiralikAracCalismalar
            .GroupBy(c => c.Arac.KiralikCariId!.Value)
            .ToList();

        foreach (var kiraGrup in kiraGruplari)
        {
            var kiralikCari = kiraGrup.First().Arac.KiralikCari!;

            var faturaItem = new GelecekFaturaItem
            {
                CariId = kiralikCari.Id,
                CariUnvan = kiralikCari.Unvan,
                CariKodu = kiralikCari.CariKodu,
                FaturaTipi = GelecekFaturaTipi.AracKirasi
            };

            // Araç bazında grupla
            var aracGruplari = kiraGrup.GroupBy(c => c.AracId).ToList();

            foreach (var aracGrup in aracGruplari)
            {
                var arac = aracGrup.First().Arac;
                var plakaGosterim = arac?.AktifPlaka ?? arac?.SaseNo ?? "Bilinmeyen";

                var detay = new GelecekFaturaDetay
                {
                    AracPlaka = plakaGosterim,
                    SeferSayisi = aracGrup.Count(),
                    Aciklama = $"{plakaGosterim} plakalı araç kirası"
                };

                foreach (var calisma in aracGrup)
                {
                    var seferFiyat = calisma.Fiyat ?? calisma.Guzergah?.BirimFiyat ?? 0;
                    var kiraBedeli = arac != null ? HesaplaKiraBedeli(arac, seferFiyat) : 0;

                    detay.Seferler.Add(new SeferOzet
                    {
                        ServisCalismaId = calisma.Id,
                        Tarih = calisma.CalismaTarihi,
                        GuzergahAdi = calisma.Guzergah.GuzergahAdi,
                        SeferFiyati = seferFiyat,
                        HesaplananTutar = kiraBedeli
                    });
                }

                detay.BirimTutar = detay.Seferler.Average(s => s.HesaplananTutar);
                detay.ToplamTutar = detay.Seferler.Sum(s => s.HesaplananTutar);
                detay.MusteriUnvan = string.Join(", ", aracGrup.Select(c => c.Guzergah.Cari.Unvan).Distinct());

                faturaItem.Detaylar.Add(detay);
            }

            result.Add(faturaItem);
        }

        // 2. KOMİSYON ÖDEMELERİ
        var komisyonluCalismalar = calismalar
            .Where(c => c.Arac.KomisyonVar && c.Arac.KomisyoncuCariId.HasValue)
            .ToList();

        var komisyonGruplari = komisyonluCalismalar
            .GroupBy(c => c.Arac.KomisyoncuCariId!.Value)
            .ToList();

        foreach (var komisyonGrup in komisyonGruplari)
        {
            var komisyoncuCari = komisyonGrup.First().Arac.KomisyoncuCari!;

            var faturaItem = new GelecekFaturaItem
            {
                CariId = komisyoncuCari.Id,
                CariUnvan = komisyoncuCari.Unvan,
                CariKodu = komisyoncuCari.CariKodu,
                FaturaTipi = GelecekFaturaTipi.Komisyon
            };

            // Araç bazında grupla
            var aracGruplari = komisyonGrup.GroupBy(c => c.AracId).ToList();

            foreach (var aracGrup in aracGruplari)
            {
                var arac = aracGrup.First().Arac;
                var plakaGosterim = arac?.AktifPlaka ?? arac?.SaseNo ?? "Bilinmeyen";

                var detay = new GelecekFaturaDetay
                {
                    AracPlaka = plakaGosterim,
                    SeferSayisi = aracGrup.Count(),
                    Aciklama = $"{plakaGosterim} plakalı araç komisyonu"
                };

                foreach (var calisma in aracGrup)
                {
                    var seferFiyat = calisma.Fiyat ?? calisma.Guzergah?.BirimFiyat ?? 0;
                    var komisyonTutari = arac != null ? HesaplaKomisyon(arac, seferFiyat) : 0;

                    detay.Seferler.Add(new SeferOzet
                    {
                        ServisCalismaId = calisma.Id,
                        Tarih = calisma.CalismaTarihi,
                        GuzergahAdi = calisma.Guzergah?.GuzergahAdi ?? "Bilinmeyen",
                        SeferFiyati = seferFiyat,
                        HesaplananTutar = komisyonTutari
                    });
                }

                detay.BirimTutar = detay.Seferler.Any() ? detay.Seferler.Average(s => s.HesaplananTutar) : 0;
                detay.ToplamTutar = detay.Seferler.Sum(s => s.HesaplananTutar);
                detay.MusteriUnvan = string.Join(", ", aracGrup.Where(c => c.Guzergah?.Cari != null).Select(c => c.Guzergah!.Cari!.Unvan).Distinct());
                detay.GuzergahAdi = string.Join(", ", aracGrup.Where(c => c.Guzergah != null).Select(c => c.Guzergah!.GuzergahAdi).Distinct().Take(3));

                faturaItem.Detaylar.Add(detay);
            }

            result.Add(faturaItem);
        }

        return result.OrderBy(x => x.FaturaTipi).ThenBy(x => x.CariUnvan).ToList();
    }

    private decimal HesaplaKiraBedeli(Arac arac, decimal seferFiyati)
    {
        if (arac.SahiplikTipi != AracSahiplikTipi.Kiralik)
            return 0;

        return arac.KiraHesaplamaTipi switch
        {
            KiraHesaplamaTipi.SeferBasina => arac.SeferBasinaKiraBedeli ?? 0,
            KiraHesaplamaTipi.Gunluk => arac.GunlukKiraBedeli ?? 0,
            KiraHesaplamaTipi.Aylik => (arac.AylikKiraBedeli ?? 0) / 30, // Günlük ortalama
            _ => arac.SeferBasinaKiraBedeli ?? 0
        };
    }

    private decimal HesaplaKomisyon(Arac arac, decimal seferFiyati)
    {
        if (!arac.KomisyonVar)
            return 0;

        return arac.KomisyonHesaplamaTipi switch
        {
            KomisyonHesaplamaTipi.YuzdeOrani => seferFiyati * (arac.KomisyonOrani ?? 0) / 100,
            KomisyonHesaplamaTipi.SabitTutar => arac.SabitKomisyonTutari ?? 0,
            _ => 0
        };
    }
}
