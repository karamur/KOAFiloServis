using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using CRMFiloServis.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class MaliAnalizService : IMaliAnalizService
{
    private readonly ApplicationDbContext _context;

    public MaliAnalizService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MaliAnalizDashboard> GetDashboardAsync(int yil, int ay)
    {
        var dashboard = new MaliAnalizDashboard { Yil = yil, Ay = ay };

        var ayBaslangic = new DateTime(yil, ay, 1);
        var ayBitis = ayBaslangic.AddMonths(1).AddDays(-1);
        var oncekiAyBaslangic = ayBaslangic.AddMonths(-1);
        var oncekiAyBitis = ayBaslangic.AddDays(-1);

        // Özmal Araç Analizi
        dashboard.OzmalAracAnaliz = await GetOzmalSegmentAnalizAsync(ayBaslangic, ayBitis);

        // Kiralık Araç Analizi
        dashboard.KiralikAracAnaliz = await GetKiralikSegmentAnalizAsync(ayBaslangic, ayBitis);

        // Komisyon Analizi
        dashboard.KomisyonAnaliz = await GetKomisyonSegmentAnalizAsync(ayBaslangic, ayBitis);

        // Toplam hesaplamalar
        dashboard.ToplamGelir = dashboard.OzmalAracAnaliz.Gelir + 
                                dashboard.KiralikAracAnaliz.Gelir + 
                                dashboard.KomisyonAnaliz.Gelir;

        dashboard.ToplamGider = dashboard.OzmalAracAnaliz.Gider + 
                                dashboard.KiralikAracAnaliz.Gider + 
                                dashboard.KomisyonAnaliz.Gider;

        // Önceki ay karşılaştırma
        var oncekiOzmal = await GetOzmalSegmentAnalizAsync(oncekiAyBaslangic, oncekiAyBitis);
        var oncekiKiralik = await GetKiralikSegmentAnalizAsync(oncekiAyBaslangic, oncekiAyBitis);
        var oncekiKomisyon = await GetKomisyonSegmentAnalizAsync(oncekiAyBaslangic, oncekiAyBitis);

        dashboard.OncekiAyGelir = oncekiOzmal.Gelir + oncekiKiralik.Gelir + oncekiKomisyon.Gelir;
        dashboard.OncekiAyGider = oncekiOzmal.Gider + oncekiKiralik.Gider + oncekiKomisyon.Gider;

        // Grafik verileri
        dashboard.GelirDagilimi = new List<GrafikVeri>
        {
            new() { Etiket = "Özmal Araçlar", Deger = dashboard.OzmalAracAnaliz.Gelir, Renk = "#28a745" },
            new() { Etiket = "Kiralık Araçlar", Deger = dashboard.KiralikAracAnaliz.Gelir, Renk = "#ffc107" },
            new() { Etiket = "Komisyon İşleri", Deger = dashboard.KomisyonAnaliz.Gelir, Renk = "#17a2b8" }
        };

        dashboard.GiderDagilimi = await GetGiderDagilimiAsync(ayBaslangic, ayBitis);
        dashboard.EnKarliGuzergahlar = await GetEnKarliGuzergahlarAsync(ayBaslangic, ayBitis, 5);
        dashboard.AracBazliKarlilik = await GetAracBazliKarlilikAsync(ayBaslangic, ayBitis, 5);
        dashboard.AylikTrend = await GetYillikTrendAsync(yil);

        return dashboard;
    }

    public async Task<OzmalAracRaporu> GetOzmalAracRaporuAsync(int yil, int ay)
    {
        var rapor = new OzmalAracRaporu { Yil = yil, Ay = ay };
        var ayBaslangic = new DateTime(yil, ay, 1);
        var ayBitis = ayBaslangic.AddMonths(1).AddDays(-1);

        // Özmal araçları getir
        var ozmalAraclar = await _context.Araclar
            .Where(a => a.SahiplikTipi == AracSahiplikTipi.Ozmal && a.Aktif)
            .ToListAsync();

        foreach (var arac in ozmalAraclar)
        {
            var detay = new OzmalAracDetay
            {
                AracId = arac.Id,
                Plaka = arac.AktifPlaka ?? string.Empty,
                Marka = arac.Marka,
                Model = arac.Model
            };

            // Sefer gelirleri
            var seferler = await _context.ServisCalismalari
                .Include(s => s.Guzergah)
                    .ThenInclude(g => g.Cari)
                .Include(s => s.Sofor)
                .Where(s => s.AracId == arac.Id && 
                           s.CalismaTarihi >= ayBaslangic && 
                           s.CalismaTarihi <= ayBitis &&
                           s.Durum == CalismaDurum.Tamamlandi)
                .ToListAsync();

            detay.SeferSayisi = seferler.Count;
            detay.SeferGeliri = seferler.Sum(s => s.Fiyat ?? s.Guzergah.BirimFiyat);

            // En çok çalışan şoförü bul
            var soforGrup = seferler.GroupBy(s => s.SoforId)
                                    .OrderByDescending(g => g.Count())
                                    .FirstOrDefault();
            if (soforGrup != null)
            {
                var sofor = soforGrup.First().Sofor;
                detay.AtananSofor = $"{sofor.Ad} {sofor.Soyad}";
            }

            // Çalışılan güzergahlar
            detay.CalistigiGuzergahlar = seferler
                .GroupBy(s => s.GuzergahId)
                .Select(g => new GuzergahOzet
                {
                    GuzergahId = g.Key,
                    GuzergahAdi = g.First().Guzergah.GuzergahAdi,
                    MusteriUnvan = g.First().Guzergah.Cari.Unvan,
                    SeferSayisi = g.Count(),
                    Gelir = g.Sum(s => s.Fiyat ?? s.Guzergah.BirimFiyat)
                })
                .OrderByDescending(g => g.Gelir)
                .ToList();

            // Masraflar
            var masraflar = await _context.AracMasraflari
                .Include(m => m.MasrafKalemi)
                .Where(m => m.AracId == arac.Id &&
                           m.MasrafTarihi >= ayBaslangic &&
                           m.MasrafTarihi <= ayBitis)
                .ToListAsync();

            foreach (var masraf in masraflar)
            {
                var kategori = masraf.MasrafKalemi?.MasrafKodu?.ToUpper() ?? "";

                if (kategori.Contains("YAKIT") || kategori.Contains("AKARYA"))
                    detay.AkaryakitMasrafi += masraf.Tutar;
                else if (kategori.Contains("BAKIM") || kategori.Contains("SERVIS") || kategori.Contains("ONARIM"))
                    detay.BakimMasrafi += masraf.Tutar;
                else if (kategori.Contains("SIGORTA") || kategori.Contains("KASKO"))
                    detay.SigortaMasrafi += masraf.Tutar;
                else
                    detay.DigerMasraflar += masraf.Tutar;
            }

            rapor.AracDetaylari.Add(detay);
        }

        return rapor;
    }

    public async Task<KiralikAracRaporu> GetKiralikAracRaporuAsync(int yil, int ay)
    {
        var rapor = new KiralikAracRaporu { Yil = yil, Ay = ay };
        var ayBaslangic = new DateTime(yil, ay, 1);
        var ayBitis = ayBaslangic.AddMonths(1).AddDays(-1);

        // Kiralık araçların çalışmalarını getir
        var kiralikCalismalar = await _context.ServisCalismalari
            .Include(s => s.Arac)
                .ThenInclude(a => a.KiralikCari)
            .Include(s => s.Sofor)
            .Include(s => s.Guzergah)
                .ThenInclude(g => g.Cari)
            .Where(s => s.Arac.SahiplikTipi == AracSahiplikTipi.Kiralik &&
                       s.Arac.KiralikCariId.HasValue &&
                       s.CalismaTarihi >= ayBaslangic &&
                       s.CalismaTarihi <= ayBitis &&
                       s.Durum == CalismaDurum.Tamamlandi)
            .ToListAsync();

        // Firma bazında grupla
        var firmaGruplari = kiralikCalismalar
            .GroupBy(c => c.Arac.KiralikCariId!.Value)
            .ToList();

        foreach (var firmaGrup in firmaGruplari)
        {
            var firma = firmaGrup.First().Arac.KiralikCari!;

            var firmaDetay = new KiralikFirmaDetay
            {
                FirmaId = firma.Id,
                FirmaUnvan = firma.Unvan,
                FirmaKodu = firma.CariKodu
            };

            // Araç bazında grupla
            var aracGruplari = firmaGrup.GroupBy(c => c.AracId).ToList();

            foreach (var aracGrup in aracGruplari)
            {
                var arac = aracGrup.First().Arac;
                var sofor = aracGrup.First().Sofor;

                var aracDetay = new KiralikAracDetay
                {
                    Plaka = arac?.AktifPlaka ?? arac?.SaseNo ?? "Bilinmeyen",
                    SoforAdSoyad = $"{sofor?.Ad} {sofor?.Soyad}"
                };

                // Güzergah bazında grupla
                var guzergahGruplari = aracGrup.GroupBy(c => c.GuzergahId).ToList();

                foreach (var guzergahGrup in guzergahGruplari)
                {
                    var guzergah = guzergahGrup.First().Guzergah;
                    var seferSayisi = guzergahGrup.Count();
                    var birimFiyat = guzergah?.BirimFiyat ?? 0;
                    var seferGeliri = guzergahGrup.Sum(c => c.Fiyat ?? birimFiyat);
                    var kiraBedeli = seferSayisi * (arac.SeferBasinaKiraBedeli ?? 0);

                    aracDetay.GuzergahDetaylari.Add(new KiralikGuzergahDetay
                    {
                        GuzergahAdi = guzergah?.GuzergahAdi ?? string.Empty,
                        MusteriUnvan = guzergah?.Cari?.Unvan ?? string.Empty,
                        SeferSayisi = seferSayisi,
                        BirimFiyat = birimFiyat,
                        KiraBedeli = arac.SeferBasinaKiraBedeli ?? 0,
                        MusteridenAlinacak = seferGeliri,
                        FirmayaOdenecek = kiraBedeli
                    });
                }

                firmaDetay.AracDetaylari.Add(aracDetay);
            }

            rapor.FirmaDetaylari.Add(firmaDetay);
        }

        return rapor;
    }

    public async Task<KomisyonRaporu> GetKomisyonRaporuAsync(int yil, int ay)
    {
        var rapor = new KomisyonRaporu { Yil = yil, Ay = ay };
        var ayBaslangic = new DateTime(yil, ay, 1);
        var ayBitis = ayBaslangic.AddMonths(1).AddDays(-1);

        // Komisyonlu çalışmaları getir
        var komisyonluCalismalar = await _context.ServisCalismalari
            .Include(s => s.Arac)
                .ThenInclude(a => a.KomisyoncuCari)
            .Include(s => s.Guzergah)
                .ThenInclude(g => g.Cari)
            .Where(s => s.Arac.KomisyonVar &&
                       s.Arac.KomisyoncuCariId.HasValue &&
                       s.CalismaTarihi >= ayBaslangic &&
                       s.CalismaTarihi <= ayBitis &&
                       s.Durum == CalismaDurum.Tamamlandi)
            .ToListAsync();

        // Komisyoncu bazında grupla
        var komisyoncuGruplari = komisyonluCalismalar
            .GroupBy(c => c.Arac.KomisyoncuCariId!.Value)
            .ToList();

        foreach (var komisyoncuGrup in komisyoncuGruplari)
        {
            var komisyoncu = komisyoncuGrup.First().Arac.KomisyoncuCari!;
            var arac = komisyoncuGrup.First().Arac;

            var komisyoncuDetay = new KomisyoncuDetay
            {
                KomisyoncuId = komisyoncu.Id,
                KomisyoncuUnvan = komisyoncu.Unvan,
                KomisyonOrani = arac.KomisyonOrani ?? 0
            };

            // Araç ve güzergah bazında grupla
            var gruplar = komisyoncuGrup
                .GroupBy(c => new { c.AracId, c.GuzergahId })
                .ToList();

            foreach (var grup in gruplar)
            {
                var calisma = grup.First();
                var seferSayisi = grup.Count();
                var seferGeliri = grup.Sum(c => c.Fiyat ?? c.Guzergah.BirimFiyat);

                decimal komisyonTutari = arac.KomisyonHesaplamaTipi switch
                {
                    KomisyonHesaplamaTipi.YuzdeOrani => seferGeliri * (arac.KomisyonOrani ?? 0) / 100,
                    KomisyonHesaplamaTipi.SabitTutar => seferSayisi * (arac.SabitKomisyonTutari ?? 0),
                    _ => 0
                };

                komisyoncuDetay.IsDetaylari.Add(new KomisyonIsDetay
                {
                    AracPlaka = calisma.Arac?.AktifPlaka ?? calisma.Arac?.SaseNo ?? "Bilinmeyen",
                    GuzergahAdi = calisma.Guzergah?.GuzergahAdi ?? "Bilinmeyen",
                    MusteriUnvan = calisma.Guzergah?.Cari?.Unvan ?? "Bilinmeyen",
                    SeferSayisi = seferSayisi,
                    SeferGeliri = seferGeliri,
                    KomisyonTutari = komisyonTutari
                });
            }

            rapor.KomisyoncuDetaylari.Add(komisyoncuDetay);
        }

        return rapor;
    }

    public async Task<ChecklistOzet> GetChecklistOzetAsync(int yil, int ay)
    {
        var ozet = new ChecklistOzet { Yil = yil, Ay = ay };
        var bugun = DateTime.Today;
        var uyariGunSayisi = 30; // 30 gün kala uyarı

        // Şoför Checklist
        var soforler = await _context.Soforler.Where(s => s.Aktif).ToListAsync();
        foreach (var sofor in soforler)
        {
            var soforChecklist = new SoforChecklistOzet
            {
                SoforId = sofor.Id,
                AdSoyad = $"{sofor.Ad} {sofor.Soyad}",
                SoforKodu = sofor.SoforKodu
            };

            // Ehliyet kontrolü (şimdilik entity'de bu alanlar yok, ileride eklenebilir)
            soforChecklist.EhliyetDurum = "Tamam";
            soforChecklist.SrcDurum = "Tamam";
            soforChecklist.PsikoteknikDurum = "Tamam";
            soforChecklist.SaglikDurum = "Tamam";
            soforChecklist.GenelDurum = "Tamam";

            ozet.SoforChecklists.Add(soforChecklist);
        }

        // Araç Checklist
        var araclar = await _context.Araclar.Where(a => a.Aktif).ToListAsync();
        foreach (var arac in araclar)
        {
            var aracChecklist = new AracChecklistOzet
            {
                AracId = arac.Id,
                Plaka = arac.AktifPlaka ?? arac.SaseNo,
                MarkaModel = $"{arac.Marka} {arac.Model}"
            };

            // Muayene
            aracChecklist.MuayeneBitisTarihi = arac.MuayeneBitisTarihi;
            aracChecklist.MuayeneDurum = GetTarihDurum(arac.MuayeneBitisTarihi, bugun, uyariGunSayisi);

            // Sigorta
            aracChecklist.SigortaBitisTarihi = arac.TrafikSigortaBitisTarihi;
            aracChecklist.SigortaDurum = GetTarihDurum(arac.TrafikSigortaBitisTarihi, bugun, uyariGunSayisi);

            // Kasko
            aracChecklist.KaskoBitisTarihi = arac.KaskoBitisTarihi;
            aracChecklist.KaskoDurum = GetTarihDurum(arac.KaskoBitisTarihi, bugun, uyariGunSayisi);

            // Bakım
            aracChecklist.BakimDurum = "Tamam";

            // Genel durum
            var durumlar = new[] { aracChecklist.MuayeneDurum, aracChecklist.SigortaDurum, 
                                   aracChecklist.KaskoDurum, aracChecklist.BakimDurum };

            if (durumlar.Any(d => d == "Kritik"))
                aracChecklist.GenelDurum = "Kritik";
            else if (durumlar.Any(d => d == "Uyari"))
                aracChecklist.GenelDurum = "Uyari";
            else
                aracChecklist.GenelDurum = "Tamam";

            ozet.AracChecklists.Add(aracChecklist);
        }

        // Güzergah Checklist
        var guzergahlar = await _context.Guzergahlar
            .Include(g => g.Cari)
            .Where(g => g.Aktif)
            .ToListAsync();

        var ayBaslangic = new DateTime(yil, ay, 1);
        var ayBitis = ayBaslangic.AddMonths(1).AddDays(-1);

        foreach (var guzergah in guzergahlar)
        {
            var guzergahChecklist = new GuzergahChecklistOzet
            {
                GuzergahId = guzergah.Id,
                GuzergahAdi = guzergah.GuzergahAdi,
                MusteriUnvan = guzergah.Cari.Unvan
            };

            // Sözleşme durumu (şimdilik entity'de yok)
            guzergahChecklist.SozlesmeDurum = "Tamam";
            guzergahChecklist.FiyatDurum = "Tamam";

            // Sefer durumu
            var seferSayisi = await _context.ServisCalismalari
                .Where(s => s.GuzergahId == guzergah.Id &&
                           s.CalismaTarihi >= ayBaslangic &&
                           s.CalismaTarihi <= ayBitis &&
                           s.Durum == CalismaDurum.Tamamlandi)
                .CountAsync();

            guzergahChecklist.GerceklesenSefer = seferSayisi;
            guzergahChecklist.SeferDurum = "Tamam";

            // Ödeme durumu
            var bekleyenFaturalar = await _context.Faturalar
                .Where(f => f.CariId == guzergah.CariId && 
                           f.KalanTutar > 0 &&
                           f.Durum != FaturaDurum.IptalEdildi)
                .SumAsync(f => f.KalanTutar);

            guzergahChecklist.BekleyenOdeme = bekleyenFaturalar;
            guzergahChecklist.OdemeDurum = bekleyenFaturalar > 0 ? "Uyari" : "Tamam";

            guzergahChecklist.GenelDurum = guzergahChecklist.OdemeDurum;

            ozet.GuzergahChecklists.Add(guzergahChecklist);
        }

        return ozet;
    }

    public async Task<List<GrafikVeri>> GetYillikTrendAsync(int yil)
    {
        var trend = new List<GrafikVeri>();

        for (int ay = 1; ay <= 12; ay++)
        {
            var ayBaslangic = new DateTime(yil, ay, 1);
            var ayBitis = ayBaslangic.AddMonths(1).AddDays(-1);

            if (ayBaslangic > DateTime.Today)
                break;

            var gelir = await _context.ServisCalismalari
                .Include(s => s.Guzergah)
                .Where(s => s.CalismaTarihi >= ayBaslangic &&
                           s.CalismaTarihi <= ayBitis &&
                           s.Durum == CalismaDurum.Tamamlandi)
                .SumAsync(s => s.Fiyat ?? s.Guzergah.BirimFiyat);

            var gider = await _context.AracMasraflari
                .Where(m => m.MasrafTarihi >= ayBaslangic &&
                           m.MasrafTarihi <= ayBitis)
                .SumAsync(m => m.Tutar);

            trend.Add(new GrafikVeri
            {
                Etiket = ayBaslangic.ToString("MMM"),
                Deger = gelir - gider,
                EkBilgi = $"Gelir: {gelir:N0}?, Gider: {gider:N0}?"
            });
        }

        return trend;
    }

    #region Private Methods

    private async Task<SegmentAnaliz> GetOzmalSegmentAnalizAsync(DateTime baslangic, DateTime bitis)
    {
        var analiz = new SegmentAnaliz { SegmentAdi = "Özmal Araçlar" };

        var ozmalAracIds = await _context.Araclar
            .Where(a => a.SahiplikTipi == AracSahiplikTipi.Ozmal)
            .Select(a => a.Id)
            .ToListAsync();

        // Gelirler
        analiz.Gelir = await _context.ServisCalismalari
            .Include(s => s.Guzergah)
            .Where(s => ozmalAracIds.Contains(s.AracId) &&
                       s.CalismaTarihi >= baslangic &&
                       s.CalismaTarihi <= bitis &&
                       s.Durum == CalismaDurum.Tamamlandi)
            .SumAsync(s => s.Fiyat ?? s.Guzergah.BirimFiyat);

        // Giderler
        analiz.Gider = await _context.AracMasraflari
            .Where(m => ozmalAracIds.Contains(m.AracId) &&
                       m.MasrafTarihi >= baslangic &&
                       m.MasrafTarihi <= bitis)
            .SumAsync(m => m.Tutar);

        analiz.SeferSayisi = await _context.ServisCalismalari
            .Where(s => ozmalAracIds.Contains(s.AracId) &&
                       s.CalismaTarihi >= baslangic &&
                       s.CalismaTarihi <= bitis &&
                       s.Durum == CalismaDurum.Tamamlandi)
            .CountAsync();

        analiz.AracSayisi = ozmalAracIds.Count;

        return analiz;
    }

    private async Task<SegmentAnaliz> GetKiralikSegmentAnalizAsync(DateTime baslangic, DateTime bitis)
    {
        var analiz = new SegmentAnaliz { SegmentAdi = "Kiralık Araçlar" };

        var kiralikCalismalar = await _context.ServisCalismalari
            .Include(s => s.Arac)
            .Include(s => s.Guzergah)
            .Where(s => s.Arac.SahiplikTipi == AracSahiplikTipi.Kiralik &&
                       s.CalismaTarihi >= baslangic &&
                       s.CalismaTarihi <= bitis &&
                       s.Durum == CalismaDurum.Tamamlandi)
            .ToListAsync();

        analiz.Gelir = kiralikCalismalar.Sum(s => s.Fiyat ?? s.Guzergah.BirimFiyat);
        analiz.Gider = kiralikCalismalar.Sum(s => s.Arac.SeferBasinaKiraBedeli ?? 0);
        analiz.SeferSayisi = kiralikCalismalar.Count;
        analiz.AracSayisi = kiralikCalismalar.Select(s => s.AracId).Distinct().Count();

        return analiz;
    }

    private async Task<SegmentAnaliz> GetKomisyonSegmentAnalizAsync(DateTime baslangic, DateTime bitis)
    {
        var analiz = new SegmentAnaliz { SegmentAdi = "Komisyon İşleri" };

        var komisyonluCalismalar = await _context.ServisCalismalari
            .Include(s => s.Arac)
            .Include(s => s.Guzergah)
            .Where(s => s.Arac.KomisyonVar &&
                       s.CalismaTarihi >= baslangic &&
                       s.CalismaTarihi <= bitis &&
                       s.Durum == CalismaDurum.Tamamlandi)
            .ToListAsync();

        foreach (var calisma in komisyonluCalismalar)
        {
            var seferFiyat = calisma.Fiyat ?? calisma.Guzergah.BirimFiyat;
            analiz.Gelir += seferFiyat;

            var komisyon = calisma.Arac.KomisyonHesaplamaTipi switch
            {
                KomisyonHesaplamaTipi.YuzdeOrani => seferFiyat * (calisma.Arac.KomisyonOrani ?? 0) / 100,
                KomisyonHesaplamaTipi.SabitTutar => calisma.Arac.SabitKomisyonTutari ?? 0,
                _ => 0
            };
            analiz.Gider += komisyon;
        }

        analiz.SeferSayisi = komisyonluCalismalar.Count;
        analiz.AracSayisi = komisyonluCalismalar.Select(s => s.AracId).Distinct().Count();

        return analiz;
    }

    private async Task<List<GrafikVeri>> GetGiderDagilimiAsync(DateTime baslangic, DateTime bitis)
    {
        var masraflar = await _context.AracMasraflari
            .Include(m => m.MasrafKalemi)
            .Where(m => m.MasrafTarihi >= baslangic && m.MasrafTarihi <= bitis)
            .ToListAsync();

        var gruplar = masraflar
            .GroupBy(m => m.MasrafKalemi?.MasrafAdi ?? "Diğer")
            .Select(g => new GrafikVeri
            {
                Etiket = g.Key,
                Deger = g.Sum(m => m.Tutar)
            })
            .OrderByDescending(g => g.Deger)
            .Take(5)
            .ToList();

        return gruplar;
    }

    private async Task<List<GrafikVeri>> GetEnKarliGuzergahlarAsync(DateTime baslangic, DateTime bitis, int adet)
    {
        var calismalar = await _context.ServisCalismalari
            .Include(s => s.Guzergah)
            .Where(s => s.CalismaTarihi >= baslangic &&
                       s.CalismaTarihi <= bitis &&
                       s.Durum == CalismaDurum.Tamamlandi)
            .ToListAsync();

        var gruplar = calismalar
            .GroupBy(s => s.GuzergahId)
            .Select(g => new GrafikVeri
            {
                Etiket = g.First().Guzergah.GuzergahAdi,
                Deger = g.Sum(s => s.Fiyat ?? s.Guzergah.BirimFiyat)
            })
            .OrderByDescending(g => g.Deger)
            .Take(adet)
            .ToList();

        return gruplar;
    }

    private async Task<List<GrafikVeri>> GetAracBazliKarlilikAsync(DateTime baslangic, DateTime bitis, int adet)
    {
        var calismalar = await _context.ServisCalismalari
            .Include(s => s.Arac)
            .Include(s => s.Guzergah)
            .Where(s => s.CalismaTarihi >= baslangic &&
                       s.CalismaTarihi <= bitis &&
                       s.Durum == CalismaDurum.Tamamlandi)
            .ToListAsync();

        var masraflar = await _context.AracMasraflari
            .Where(m => m.MasrafTarihi >= baslangic && m.MasrafTarihi <= bitis)
            .GroupBy(m => m.AracId)
            .Select(g => new { AracId = g.Key, Toplam = g.Sum(m => m.Tutar) })
            .ToListAsync();

        var gruplar = calismalar
            .GroupBy(s => s.AracId)
            .Select(g => 
            {
                var gelir = g.Sum(s => s.Fiyat ?? s.Guzergah?.BirimFiyat ?? 0);
                var gider = masraflar.FirstOrDefault(m => m.AracId == g.Key)?.Toplam ?? 0;
                return new GrafikVeri
                {
                    Etiket = g.First().Arac?.AktifPlaka ?? g.First().Arac?.SaseNo ?? "Bilinmeyen",
                    Deger = gelir - gider,
                    EkBilgi = $"Gelir: {gelir:N0}?"
                };
            })
            .OrderByDescending(g => g.Deger)
            .Take(adet)
            .ToList();

        return gruplar;
    }

    private string GetTarihDurum(DateTime? tarih, DateTime bugun, int uyariGunSayisi)
    {
        if (!tarih.HasValue)
            return "Bekliyor";

        var kalanGun = (tarih.Value - bugun).Days;

        if (kalanGun < 0)
            return "Kritik";
        else if (kalanGun <= uyariGunSayisi)
            return "Uyari";
        else
            return "Tamam";
    }

    #endregion
}
