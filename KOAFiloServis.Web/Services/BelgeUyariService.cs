using KOAFiloServis.Shared.Entities;
using KOAFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace KOAFiloServis.Web.Services;

public class BelgeUyariService : IBelgeUyariService
{
    private readonly ApplicationDbContext _context;

    public BelgeUyariService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BelgeUyariOzet> GetBelgeUyarilarAsync(int yaklasanGunSayisi = 30)
    {
        var ozet = new BelgeUyariOzet();
        var bugun = DateTime.Today;
        var limitTarih = bugun.AddDays(yaklasanGunSayisi);

        // Aktif şoförleri al
        var soforler = await _context.Soforler
            .Where(s => s.Aktif && s.Gorev == PersonelGorev.Sofor)
            .ToListAsync();

        foreach (var sofor in soforler)
        {
            // Ehliyet kontrolü
            if (sofor.EhliyetGecerlilikTarihi.HasValue && sofor.EhliyetGecerlilikTarihi.Value <= limitTarih)
            {
                ozet.EhliyetUyarilari.Add(new BelgeUyari
                {
                    Id = sofor.Id,
                    Kaynak = "Personel",
                    Baslik = sofor.TamAd,
                    BelgeTuru = "Ehliyet",
                    BitisTarihi = sofor.EhliyetGecerlilikTarihi.Value,
                    DetayUrl = $"/personel/{sofor.Id}"
                });
            }

            // SRC Belgesi kontrolü
            if (sofor.SrcBelgesiGecerlilikTarihi.HasValue && sofor.SrcBelgesiGecerlilikTarihi.Value <= limitTarih)
            {
                ozet.SrcUyarilari.Add(new BelgeUyari
                {
                    Id = sofor.Id,
                    Kaynak = "Personel",
                    Baslik = sofor.TamAd,
                    BelgeTuru = "SRC Belgesi",
                    BitisTarihi = sofor.SrcBelgesiGecerlilikTarihi.Value,
                    DetayUrl = $"/personel/{sofor.Id}"
                });
            }

            // Psikoteknik kontrolü
            if (sofor.PsikoteknikGecerlilikTarihi.HasValue && sofor.PsikoteknikGecerlilikTarihi.Value <= limitTarih)
            {
                ozet.PsikoteknikUyarilari.Add(new BelgeUyari
                {
                    Id = sofor.Id,
                    Kaynak = "Personel",
                    Baslik = sofor.TamAd,
                    BelgeTuru = "Psikoteknik",
                    BitisTarihi = sofor.PsikoteknikGecerlilikTarihi.Value,
                    DetayUrl = $"/personel/{sofor.Id}"
                });
            }

            // Sağlık Raporu kontrolü
            if (sofor.SaglikRaporuGecerlilikTarihi.HasValue && sofor.SaglikRaporuGecerlilikTarihi.Value <= limitTarih)
            {
                ozet.SaglikRaporuUyarilari.Add(new BelgeUyari
                {
                    Id = sofor.Id,
                    Kaynak = "Personel",
                    Baslik = sofor.TamAd,
                    BelgeTuru = "Sağlık Raporu",
                    BitisTarihi = sofor.SaglikRaporuGecerlilikTarihi.Value,
                    DetayUrl = $"/personel/{sofor.Id}"
                });
            }
        }

        // Aktif araçları al
        var araclar = await _context.Araclar
            .Where(a => a.Aktif)
            .ToListAsync();

        foreach (var arac in araclar)
        {
            // Muayene kontrolü
            if (arac.MuayeneBitisTarihi.HasValue && arac.MuayeneBitisTarihi.Value <= limitTarih)
            {
                ozet.MuayeneUyarilari.Add(new BelgeUyari
                {
                    Id = arac.Id,
                    Kaynak = "Araç",
                    Baslik = arac.AktifPlaka ?? arac.SaseNo,
                    BelgeTuru = "Araç Muayenesi",
                    BitisTarihi = arac.MuayeneBitisTarihi.Value,
                    DetayUrl = $"/araclar/{arac.Id}/evraklar"
                });
            }

            // Kasko kontrolü
            if (arac.KaskoBitisTarihi.HasValue && arac.KaskoBitisTarihi.Value <= limitTarih)
            {
                ozet.KaskoUyarilari.Add(new BelgeUyari
                {
                    Id = arac.Id,
                    Kaynak = "Araç",
                    Baslik = arac.AktifPlaka ?? arac.SaseNo,
                    BelgeTuru = "Kasko",
                    BitisTarihi = arac.KaskoBitisTarihi.Value,
                    DetayUrl = $"/araclar/{arac.Id}/evraklar"
                });
            }

            // Trafik Sigortası kontrolü
            if (arac.TrafikSigortaBitisTarihi.HasValue && arac.TrafikSigortaBitisTarihi.Value <= limitTarih)
            {
                ozet.TrafikSigortasiUyarilari.Add(new BelgeUyari
                {
                    Id = arac.Id,
                    Kaynak = "Araç",
                    Baslik = arac.AktifPlaka ?? arac.SaseNo,
                    BelgeTuru = "Trafik Sigortası",
                    BitisTarihi = arac.TrafikSigortaBitisTarihi.Value,
                    DetayUrl = $"/araclar/{arac.Id}/evraklar"
                });
            }
        }

        var sabitAracKategorileri = new[]
        {
            EvrakKategorileri.Muayene,
            EvrakKategorileri.Kasko,
            EvrakKategorileri.TrafikSigortasi
        };

        var digerAracEvraklari = await _context.AracEvraklari
            .AsNoTracking()
            .Include(x => x.Arac)
            .Where(x => !x.IsDeleted
                && x.Arac != null
                && !x.Arac.IsDeleted
                && x.Arac.Aktif
                && x.BitisTarihi.HasValue
                && x.BitisTarihi.Value <= limitTarih
                && !sabitAracKategorileri.Contains(x.EvrakKategorisi))
            .OrderBy(x => x.BitisTarihi)
            .ToListAsync();

        foreach (var evrak in digerAracEvraklari)
        {
            ozet.DigerAracEvrakUyarilari.Add(new BelgeUyari
            {
                Id = evrak.Id,
                Kaynak = "Araç",
                Baslik = evrak.Arac?.AktifPlaka ?? evrak.Arac?.SaseNo ?? "Araç",
                BelgeTuru = string.IsNullOrWhiteSpace(evrak.EvrakAdi) ? evrak.EvrakKategorisi : evrak.EvrakAdi!,
                BitisTarihi = evrak.BitisTarihi!.Value,
                DetayUrl = evrak.AracId > 0 ? $"/araclar/{evrak.AracId}/evraklar" : "/araclar"
            });
        }

        // Özet sayıları hesapla
        ozet.ToplamKritikUyari = ozet.TumUyarilar.Count(u => u.Seviye == BelgeUyariSeviye.Kritik || u.Seviye == BelgeUyariSeviye.Acil);
        ozet.ToplamUyari = ozet.TumUyarilar.Count;

        return ozet;
    }
}
