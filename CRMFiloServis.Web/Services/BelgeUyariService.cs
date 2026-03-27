using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

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

        // Aktif þoförleri al
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
                    Baslik = sofor.TamAd,
                    BelgeTuru = "Ehliyet",
                    BitisTarihi = sofor.EhliyetGecerlilikTarihi.Value
                });
            }

            // SRC Belgesi kontrolü
            if (sofor.SrcBelgesiGecerlilikTarihi.HasValue && sofor.SrcBelgesiGecerlilikTarihi.Value <= limitTarih)
            {
                ozet.SrcUyarilari.Add(new BelgeUyari
                {
                    Id = sofor.Id,
                    Baslik = sofor.TamAd,
                    BelgeTuru = "SRC Belgesi",
                    BitisTarihi = sofor.SrcBelgesiGecerlilikTarihi.Value
                });
            }

            // Psikoteknik kontrolü
            if (sofor.PsikoteknikGecerlilikTarihi.HasValue && sofor.PsikoteknikGecerlilikTarihi.Value <= limitTarih)
            {
                ozet.PsikoteknikUyarilari.Add(new BelgeUyari
                {
                    Id = sofor.Id,
                    Baslik = sofor.TamAd,
                    BelgeTuru = "Psikoteknik",
                    BitisTarihi = sofor.PsikoteknikGecerlilikTarihi.Value
                });
            }

            // Saðlýk Raporu kontrolü
            if (sofor.SaglikRaporuGecerlilikTarihi.HasValue && sofor.SaglikRaporuGecerlilikTarihi.Value <= limitTarih)
            {
                ozet.SaglikRaporuUyarilari.Add(new BelgeUyari
                {
                    Id = sofor.Id,
                    Baslik = sofor.TamAd,
                    BelgeTuru = "Saðlýk Raporu",
                    BitisTarihi = sofor.SaglikRaporuGecerlilikTarihi.Value
                });
            }
        }

        // Aktif araçlarý al
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
                    Baslik = arac.AktifPlaka ?? arac.SaseNo,
                    BelgeTuru = "Araç Muayenesi",
                    BitisTarihi = arac.MuayeneBitisTarihi.Value
                });
            }

            // Kasko kontrolü
            if (arac.KaskoBitisTarihi.HasValue && arac.KaskoBitisTarihi.Value <= limitTarih)
            {
                ozet.KaskoUyarilari.Add(new BelgeUyari
                {
                    Id = arac.Id,
                    Baslik = arac.AktifPlaka ?? arac.SaseNo,
                    BelgeTuru = "Kasko",
                    BitisTarihi = arac.KaskoBitisTarihi.Value
                });
            }

            // Trafik Sigortasý kontrolü
            if (arac.TrafikSigortaBitisTarihi.HasValue && arac.TrafikSigortaBitisTarihi.Value <= limitTarih)
            {
                ozet.TrafikSigortasiUyarilari.Add(new BelgeUyari
                {
                    Id = arac.Id,
                    Baslik = arac.AktifPlaka ?? arac.SaseNo,
                    BelgeTuru = "Trafik Sigortasý",
                    BitisTarihi = arac.TrafikSigortaBitisTarihi.Value
                });
            }
        }

        // Özet sayýlarý hesapla
        ozet.ToplamKritikUyari = ozet.TumUyarilar.Count(u => u.Seviye == BelgeUyariSeviye.Kritik || u.Seviye == BelgeUyariSeviye.Acil);
        ozet.ToplamUyari = ozet.TumUyarilar.Count;

        return ozet;
    }
}
