using System.IO.Compression;
using KOAFiloServis.Shared.Entities;
using KOAFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace KOAFiloServis.Web.Services;

public class BelgeUyariService : IBelgeUyariService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IPersonelOzlukService _ozlukService;
    private readonly ISecureFileService _secureFileService;

    public BelgeUyariService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        IPersonelOzlukService ozlukService,
        ISecureFileService secureFileService)
    {
        _contextFactory = contextFactory;
        _ozlukService = ozlukService;
        _secureFileService = secureFileService;
    }

    public async Task<BelgeUyariOzet> GetBelgeUyarilarAsync(int yaklasanGunSayisi = 30)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var ozet = new BelgeUyariOzet();
        var bugun = DateTime.Today;
        var limitTarih = bugun.AddDays(yaklasanGunSayisi);

        // Aktif tüm personeli al
        var soforler = await context.Soforler
            .Where(s => s.Aktif && !s.IsDeleted)
            .ToListAsync();

        // Tüm personel özlük evraklarını tek sorguda al (GecerlilikBitisTarihi olan ve yaklaşan/geçmiş)
        var tumOzlukEvraklar = await context.PersonelOzlukEvraklar
            .AsNoTracking()
            .Include(e => e.Sofor)
            .Include(e => e.EvrakTanim)
            .Where(e => !e.IsDeleted
                && e.Sofor != null && e.Sofor.Aktif && !e.Sofor.IsDeleted
                && e.GecerlilikBitisTarihi.HasValue
                && e.GecerlilikBitisTarihi.Value <= limitTarih)
            .OrderBy(e => e.GecerlilikBitisTarihi)
            .ToListAsync();

        // Özlük evraklarından GecerlilikBitisTarihi olan tüm uyarıları kategoriye göre dağıt
        // Sofor entity alanlarına sahip olanlar için özlük evrak kaydı varsa onu kullan, yoksa fallback
        var soforIdEhliyetEvrakVar = tumOzlukEvraklar
            .Where(e => e.EvrakTanim?.EvrakAdi != null &&
                        e.EvrakTanim.EvrakAdi.Contains("Ehliyet", StringComparison.OrdinalIgnoreCase))
            .Select(e => e.SoforId).ToHashSet();
        var soforIdSrcEvrakVar = tumOzlukEvraklar
            .Where(e => e.EvrakTanim?.EvrakAdi != null &&
                        e.EvrakTanim.EvrakAdi.Contains("SRC", StringComparison.OrdinalIgnoreCase))
            .Select(e => e.SoforId).ToHashSet();
        var soforIdPsikoteknikEvrakVar = tumOzlukEvraklar
            .Where(e => e.EvrakTanim?.EvrakAdi != null &&
                        e.EvrakTanim.EvrakAdi.Contains("Psikoteknik", StringComparison.OrdinalIgnoreCase))
            .Select(e => e.SoforId).ToHashSet();
        var soforIdSaglikEvrakVar = tumOzlukEvraklar
            .Where(e => e.EvrakTanim?.EvrakAdi != null &&
                        (e.EvrakTanim.EvrakAdi.Contains("Sağlık", StringComparison.OrdinalIgnoreCase) ||
                         e.EvrakTanim.EvrakAdi.Contains("Saglik", StringComparison.OrdinalIgnoreCase)))
            .Select(e => e.SoforId).ToHashSet();

        // Özlük evraklarını uyarı listelerine dağıt
        foreach (var evrak in tumOzlukEvraklar)
        {
            var evrakAdi = evrak.EvrakTanim?.EvrakAdi ?? string.Empty;
            var uyari = new BelgeUyari
            {
                Id = evrak.Id,
                Kaynak = "Personel",
                Baslik = evrak.Sofor?.TamAd ?? "Personel",
                BelgeTuru = evrakAdi,
                BitisTarihi = evrak.GecerlilikBitisTarihi!.Value,
                DetayUrl = $"/personel/{evrak.SoforId}"
            };

            if (evrakAdi.Contains("Ehliyet", StringComparison.OrdinalIgnoreCase))
                ozet.EhliyetUyarilari.Add(uyari);
            else if (evrakAdi.Contains("SRC", StringComparison.OrdinalIgnoreCase))
                ozet.SrcUyarilari.Add(uyari);
            else if (evrakAdi.Contains("Psikoteknik", StringComparison.OrdinalIgnoreCase))
                ozet.PsikoteknikUyarilari.Add(uyari);
            else if (evrakAdi.Contains("Sağlık", StringComparison.OrdinalIgnoreCase) ||
                     evrakAdi.Contains("Saglik", StringComparison.OrdinalIgnoreCase))
                ozet.SaglikRaporuUyarilari.Add(uyari);
            else
                ozet.DigerPersonelEvrakUyarilari.Add(uyari);
        }

        // Özlük evrak kaydı olmayan personel için Sofor entity alanlarından fallback
        foreach (var sofor in soforler)
        {
            if (!soforIdEhliyetEvrakVar.Contains(sofor.Id)
                && sofor.EhliyetGecerlilikTarihi.HasValue
                && sofor.EhliyetGecerlilikTarihi.Value <= limitTarih)
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

            if (!soforIdSrcEvrakVar.Contains(sofor.Id)
                && sofor.SrcBelgesiGecerlilikTarihi.HasValue
                && sofor.SrcBelgesiGecerlilikTarihi.Value <= limitTarih)
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

            if (!soforIdPsikoteknikEvrakVar.Contains(sofor.Id)
                && sofor.PsikoteknikGecerlilikTarihi.HasValue
                && sofor.PsikoteknikGecerlilikTarihi.Value <= limitTarih)
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

            if (!soforIdSaglikEvrakVar.Contains(sofor.Id)
                && sofor.SaglikRaporuGecerlilikTarihi.HasValue
                && sofor.SaglikRaporuGecerlilikTarihi.Value <= limitTarih)
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

        // Tüm aktif araç evraklarını tek sorguda çek (AracEvrak tablosu tek kaynak)
        var tumAracEvraklari = await context.AracEvraklari
            .AsNoTracking()
            .Include(x => x.Arac)
            .Where(x => !x.IsDeleted
                && x.Arac != null
                && !x.Arac.IsDeleted
                && x.Arac.Aktif
                && x.Durum != EvrakDurum.Pasif
                && x.BitisTarihi.HasValue
                && x.BitisTarihi.Value <= limitTarih)
            .OrderBy(x => x.BitisTarihi)
            .ToListAsync();

        // AracEvrak tablosunda kaydı bulunmayan araçlar için Arac entity alanlarından fallback uyarı üret
        var araclar = await context.Araclar
            .Where(a => a.Aktif && !a.IsDeleted)
            .ToListAsync();

        var muayeneEvrakliAracIds = tumAracEvraklari
            .Where(e => e.EvrakKategorisi == EvrakKategorileri.Muayene)
            .Select(e => e.AracId).ToHashSet();
        var kaskoEvrakliAracIds = tumAracEvraklari
            .Where(e => e.EvrakKategorisi == EvrakKategorileri.Kasko)
            .Select(e => e.AracId).ToHashSet();
        var sigortaEvrakliAracIds = tumAracEvraklari
            .Where(e => e.EvrakKategorisi == EvrakKategorileri.TrafikSigortasi)
            .Select(e => e.AracId).ToHashSet();

        foreach (var arac in araclar)
        {
            // Muayene: AracEvrak kaydı yoksa Arac entity alanından fallback
            if (!muayeneEvrakliAracIds.Contains(arac.Id)
                && arac.MuayeneBitisTarihi.HasValue
                && arac.MuayeneBitisTarihi.Value <= limitTarih)
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

            // Kasko: AracEvrak kaydı yoksa Arac entity alanından fallback
            if (!kaskoEvrakliAracIds.Contains(arac.Id)
                && arac.KaskoBitisTarihi.HasValue
                && arac.KaskoBitisTarihi.Value <= limitTarih)
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

            // Trafik Sigortası: AracEvrak kaydı yoksa Arac entity alanından fallback
            if (!sigortaEvrakliAracIds.Contains(arac.Id)
                && arac.TrafikSigortaBitisTarihi.HasValue
                && arac.TrafikSigortaBitisTarihi.Value <= limitTarih)
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

        // AracEvrak tablosundan gelen tüm evrakleri kategoriye göre dağıt
        foreach (var evrak in tumAracEvraklari)
        {
            var baslik = evrak.Arac?.AktifPlaka ?? evrak.Arac?.SaseNo ?? "Araç";
            var belgeTuru = string.IsNullOrWhiteSpace(evrak.EvrakAdi) ? evrak.EvrakKategorisi : evrak.EvrakAdi!;
            var detayUrl = $"/araclar/{evrak.AracId}/evraklar";

            BelgeUyari uyari = new()
            {
                Id = evrak.Id,
                Kaynak = "Araç",
                Baslik = baslik,
                BelgeTuru = belgeTuru,
                BitisTarihi = evrak.BitisTarihi!.Value,
                DetayUrl = detayUrl
            };

            if (evrak.EvrakKategorisi == EvrakKategorileri.Muayene)
                ozet.MuayeneUyarilari.Add(uyari);
            else if (evrak.EvrakKategorisi == EvrakKategorileri.Kasko)
                ozet.KaskoUyarilari.Add(uyari);
            else if (evrak.EvrakKategorisi == EvrakKategorileri.TrafikSigortasi)
                ozet.TrafikSigortasiUyarilari.Add(uyari);
            else
                ozet.DigerAracEvrakUyarilari.Add(uyari);
        }


        // Tum personeller icin "Diger" kategorisindeki evrak durumlarini cek (uyari filtresi yok - tam liste)
        var digerEvrakTanimlari = await context.OzlukEvrakTanimlari
            .AsNoTracking()
            .Where(t => t.Aktif && t.Kategori == OzlukEvrakKategori.Diger)
            .OrderBy(t => t.SiraNo)
            .ThenBy(t => t.EvrakAdi)
            .ToListAsync();

        if (digerEvrakTanimlari.Count > 0)
        {
            var digerEvrakTanimIds = digerEvrakTanimlari.Select(t => t.Id).ToHashSet();

            var mevcutDigerEvraklar = await context.PersonelOzlukEvraklar
                .AsNoTracking()
                .Include(e => e.Sofor)
                .Where(e => !e.IsDeleted
                    && e.Sofor != null && e.Sofor.Aktif && !e.Sofor.IsDeleted
                    && digerEvrakTanimIds.Contains(e.EvrakTanimId))
                .ToListAsync();

            foreach (var sofor in soforler.OrderBy(s => s.Ad).ThenBy(s => s.Soyad))
            {
                foreach (var tanim in digerEvrakTanimlari)
                {
                    var kayit = mevcutDigerEvraklar
                        .FirstOrDefault(e => e.SoforId == sofor.Id && e.EvrakTanimId == tanim.Id);

                    ozet.DigerTumPersonelBelgeler.Add(new PersonelBelgeDetay
                    {
                        EvrakId = kayit?.Id ?? 0,
                        SoforId = sofor.Id,
                        PersonelAdi = sofor.TamAd,
                        PersonelKodu = sofor.SoforKodu ?? sofor.Id.ToString(),
                        EvrakAdi = tanim.EvrakAdi,
                        Kategori = tanim.Kategori,
                        Tamamlandi = kayit?.Tamamlandi ?? false,
                        TamamlanmaTarihi = kayit?.TamamlanmaTarihi,
                        GecerlilikBitisTarihi = kayit?.GecerlilikBitisTarihi,
                        Zorunlu = tanim.Zorunlu,
                        DosyaYolu = kayit?.DosyaYolu,
                        DetayUrl = $"/personel/ozluk-evrak"
                    });
                }
            }
        }
        // Özet sayıları hesapla
        ozet.ToplamKritikUyari = ozet.TumUyarilar.Count(u => u.Seviye == BelgeUyariSeviye.Kritik || u.Seviye == BelgeUyariSeviye.Acil);
        ozet.ToplamUyari = ozet.TumUyarilar.Count;

        return ozet;
    }

    public async Task<List<PersonelBelgeTabloKalemi>> GetPersonelBelgeTablosuAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var soforler = await context.Soforler
            .AsNoTracking()
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.SiralamaNo == 0 ? int.MaxValue : s.SiralamaNo)
            .ThenBy(s => s.Ad)
            .ToListAsync();

        var result = new List<PersonelBelgeTabloKalemi>();
        foreach (var s in soforler)
        {
            var evrakDurum = await _ozlukService.GetPersonelEvrakDurumuAsync(s.Id);
            var dosyalar = evrakDurum.Evraklar
                .Select(e => new OzlukEvrakDosyaBilgisi
                {
                    EvrakTanimId = e.EvrakTanimId,
                    EvrakAdi = e.EvrakAdi,
                    DosyaYolu = e.DosyaYolu,
                    DosyaAdi = e.DosyaYolu != null ? Path.GetFileName(e.DosyaYolu) : null
                }).ToList();

            result.Add(new PersonelBelgeTabloKalemi
            {
                SoforId = s.Id,
                PersonelAdi = s.TamAd,
                PersonelKodu = s.SoforKodu,
                Gorev = s.Gorev.ToString(),
                Aktif = s.Aktif,
                ToplamEvrakSayisi = evrakDurum.ToplamEvrak,
                YuklenmisEvrakSayisi = evrakDurum.TamamlananEvrak,
                EvrakDosyalari = dosyalar,
                EhliyetGecerlilik = s.EhliyetGecerlilikTarihi,
                KimlikGecerlilik = s.KimlikGecerlilikTarihi,
                SrcGecerlilik = s.SrcBelgesiGecerlilikTarihi,
                PsikoteknikGecerlilik = s.PsikoteknikGecerlilikTarihi,
                AdliSicilGecerlilik = s.AdliSicilGecerlilikTarihi,
                SaglikRaporuGecerlilik = s.SaglikRaporuGecerlilikTarihi,
                SuruculCezaBarkodGecerlilik = s.SuruculCezaBarkodluBelgeTarihi
            });
        }
        return result;
    }

    public async Task<PersonelBelgeTabloKalemi?> GetTekPersonelBelgeAsync(int soforId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var s = await context.Soforler
            .AsNoTracking()
            .Where(x => x.Id == soforId && !x.IsDeleted)
            .FirstOrDefaultAsync();
        if (s == null) return null;

        var evrakDurum = await _ozlukService.GetPersonelEvrakDurumuAsync(s.Id);
        var dosyalar = evrakDurum.Evraklar
            .Select(e => new OzlukEvrakDosyaBilgisi
            {
                EvrakTanimId = e.EvrakTanimId,
                EvrakAdi = e.EvrakAdi,
                DosyaYolu = e.DosyaYolu,
                DosyaAdi = e.DosyaYolu != null ? Path.GetFileName(e.DosyaYolu) : null
            }).ToList();

        return new PersonelBelgeTabloKalemi
        {
            SoforId = s.Id,
            PersonelAdi = s.TamAd,
            PersonelKodu = s.SoforKodu,
            Gorev = s.Gorev.ToString(),
            Aktif = s.Aktif,
            ToplamEvrakSayisi = evrakDurum.ToplamEvrak,
            YuklenmisEvrakSayisi = evrakDurum.TamamlananEvrak,
            EvrakDosyalari = dosyalar,
            EhliyetGecerlilik = s.EhliyetGecerlilikTarihi,
            KimlikGecerlilik = s.KimlikGecerlilikTarihi,
            SrcGecerlilik = s.SrcBelgesiGecerlilikTarihi,
            PsikoteknikGecerlilik = s.PsikoteknikGecerlilikTarihi,
            AdliSicilGecerlilik = s.AdliSicilGecerlilikTarihi,
            SaglikRaporuGecerlilik = s.SaglikRaporuGecerlilikTarihi,
            SuruculCezaBarkodGecerlilik = s.SuruculCezaBarkodluBelgeTarihi
        };
    }

    public async Task<bool> PersonelBelgeTarihGuncelleAsync(int soforId, string belgeAlani, DateTime? tarih)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var sofor = await context.Soforler.FindAsync(soforId);
        if (sofor == null) return false;

        switch (belgeAlani)
        {
            case "Ehliyet": sofor.EhliyetGecerlilikTarihi = tarih; break;
            case "Kimlik": sofor.KimlikGecerlilikTarihi = tarih; break;
            case "Src": sofor.SrcBelgesiGecerlilikTarihi = tarih; break;
            case "Psikoteknik": sofor.PsikoteknikGecerlilikTarihi = tarih; break;
            case "AdliSicil": sofor.AdliSicilGecerlilikTarihi = tarih; break;
            case "SaglikRaporu": sofor.SaglikRaporuGecerlilikTarihi = tarih; break;
            case "SuruculCezaBarkod": sofor.SuruculCezaBarkodluBelgeTarihi = tarih; break;
            default: return false;
        }

        sofor.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<byte[]> PersonelBelgePdfAsync(int soforId)
    {
        return await _ozlukService.ExportPersonelDosyaPdfAsync(soforId);
    }

    public async Task<byte[]> SeciliPersonelBelgelerZipAsync(List<int> soforIdler, List<string>? seciliDosyaYollari = null)
    {
        using var zipMs = new MemoryStream();
        using (var archive = new ZipArchive(zipMs, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var soforId in soforIdler)
            {
                var evrakDurum = await _ozlukService.GetPersonelEvrakDurumuAsync(soforId);
                var personelKlasoru = string.Join("_",
                    (evrakDurum.PersonelAdi?.Length > 0 ? evrakDurum.PersonelAdi : evrakDurum.PersonelKodu ?? soforId.ToString())
                    .Split(Path.GetInvalidFileNameChars()));

                // Seçili dosya yolu filtresi varsa uygula
                var evraklar = evrakDurum.Evraklar
                    .Where(e => !string.IsNullOrEmpty(e.DosyaYolu))
                    .Where(e => seciliDosyaYollari == null || seciliDosyaYollari.Contains(e.DosyaYolu!))
                    .ToList();

                foreach (var evrak in evraklar)
                {
                    var icerik = await _secureFileService.ReadDecryptedAsync(evrak.DosyaYolu);
                    if (icerik == null || icerik.Length == 0) continue;

                    var uzanti = Path.GetExtension(evrak.DosyaYolu);
                    var guvenliEvrakAd = string.Join("_", evrak.EvrakAdi.Split(Path.GetInvalidFileNameChars()));
                    var zipYolu = soforIdler.Count > 1
                        ? $"{personelKlasoru}/{guvenliEvrakAd}{uzanti}"
                        : $"{guvenliEvrakAd}{uzanti}";

                    var entry = archive.CreateEntry(zipYolu, CompressionLevel.Optimal);
                    await using var entryStream = entry.Open();
                    await entryStream.WriteAsync(icerik);
                }
            }
        }
        zipMs.Position = 0;
        return zipMs.ToArray();
    }
}


