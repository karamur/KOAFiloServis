using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using CRMFiloServis.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class FiloKomisyonService : IFiloKomisyonService
{
    private readonly ApplicationDbContext _context;

    public FiloKomisyonService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<FiloGuzergahEslestirme>> GetEslestirmelerAsync(int firmaId, bool sadeceAktifler = true)
    {
        var query = _context.FiloGuzergahEslestirmeleri
            .Include(e => e.KurumFirma)
            .Include(e => e.Guzergah)
            .Include(e => e.Arac)
            .Include(e => e.Sofor)
            .Where(e => e.FirmaId == firmaId && !e.IsDeleted);

        if (sadeceAktifler)
        {
            query = query.Where(e => e.IsActive);
        }

        return await query.OrderBy(e => e.KurumFirmaId).ThenBy(e => e.Guzergah != null ? e.Guzergah.GuzergahAdi : string.Empty).ToListAsync();
    }

    public async Task<FiloGuzergahEslestirme?> GetEslestirmeByIdAsync(int id)
    {
        return await _context.FiloGuzergahEslestirmeleri
            .Include(e => e.KurumFirma)
            .Include(e => e.Guzergah)
            .Include(e => e.Arac)
            .Include(e => e.Sofor)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
    }

    public async Task<FiloGuzergahEslestirme> CreateEslestirmeAsync(FiloGuzergahEslestirme eslestirme)
    {
        _context.FiloGuzergahEslestirmeleri.Add(eslestirme);
        await _context.SaveChangesAsync();
        return eslestirme;
    }

    public async Task<FiloGuzergahEslestirme> UpdateEslestirmeAsync(FiloGuzergahEslestirme eslestirme)
    {
        var existing = await _context.FiloGuzergahEslestirmeleri.FindAsync(eslestirme.Id);
        if (existing != null)
        {
            existing.KurumFirmaId = eslestirme.KurumFirmaId;
            existing.GuzergahId = eslestirme.GuzergahId;
            existing.AracId = eslestirme.AracId;
            existing.SoforId = eslestirme.SoforId;
            existing.ServisTuru = eslestirme.ServisTuru;
            existing.KurumaKesilecekUcret = eslestirme.KurumaKesilecekUcret;
            existing.TaseronaOdenenUcret = eslestirme.TaseronaOdenenUcret;
            existing.IsActive = eslestirme.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
        return existing ?? eslestirme;
    }

    public async Task DeleteEslestirmeAsync(int id)
    {
        var existing = await _context.FiloGuzergahEslestirmeleri.FindAsync(id);
        if (existing != null)
        {
            existing.IsDeleted = true;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task TopluPuantajUretAsync(int firmaId, int yil, int ay)
    {
        // 1. Ayın günlerini belirle
        var baslangic = new DateTime(yil, ay, 1);
        var bitis = baslangic.AddMonths(1).AddDays(-1);

        // 2. Halihazırda var olan o aya ait puantaj kayıtlarını al (mükerrer kayıt oluşmaması için)
        var mevcutPuantajlar = await _context.FiloGunlukPuantajlar
            .Where(p => p.FirmaId == firmaId && p.Tarih >= baslangic && p.Tarih <= bitis && !p.IsDeleted)
            .ToListAsync();

        // 3. Aktif eşleştirmeleri çek
        var aktifEslestirmeler = await GetEslestirmelerAsync(firmaId, sadeceAktifler: true);

        // 4. Yeni puantajları oluştur
        var yeniKavitlar = new List<FiloGunlukPuantaj>();

        for (int day = 1; day <= bitis.Day; day++)
        {
            var currentDate = new DateTime(yil, ay, day);
            bool isWeekend = currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday;

            foreach (var eslestirme in aktifEslestirmeler)
            {
                // Eğer o günde bu eşleşmeye ait bir kayıt yoksa
                bool varMi = mevcutPuantajlar.Any(p => p.FiloGuzergahEslestirmeId == eslestirme.Id && p.Tarih.Date == currentDate.Date);

                if (!varMi)
                {
                    yeniKavitlar.Add(new FiloGunlukPuantaj
                    {
                        FirmaId = firmaId,
                        Tarih = currentDate,
                        FiloGuzergahEslestirmeId = eslestirme.Id,
                        KurumFirmaId = eslestirme.KurumFirmaId,
                        GuzergahId = eslestirme.GuzergahId,
                        AracId = eslestirme.AracId,
                        SoforId = eslestirme.SoforId,
                        Durum = isWeekend ? OperasyonDurumu.Gitmedi_Mazeretli : OperasyonDurumu.Gitti,
                        PuantajCarpani = isWeekend ? 0m : 1.0m,
                        TahakkukEdenKurumUcreti = isWeekend ? 0m : eslestirme.KurumaKesilecekUcret,
                        TahakkukEdenTaseronUcreti = isWeekend ? 0m : eslestirme.TaseronaOdenenUcret
                    });
                }
            }
        }

        if (yeniKavitlar.Any())
        {
            await _context.FiloGunlukPuantajlar.AddRangeAsync(yeniKavitlar);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<FiloGunlukPuantaj>> GetGunlukPuantajlarSiraliAsync(int firmaId, DateTime tarih)
    {
        return await _context.FiloGunlukPuantajlar
            .Include(p => p.KurumFirma)
            .Include(p => p.Guzergah)
            .Include(p => p.Arac)
            .Include(p => p.Sofor)
            .Where(p => p.FirmaId == firmaId && p.Tarih.Date == tarih.Date && !p.IsDeleted)
            .OrderBy(p => p.KurumFirma!.FirmaAdi)
            .ThenBy(p => p.Guzergah!.GuzergahAdi)
            .ToListAsync();
    }

    public async Task<List<FiloGunlukPuantaj>> GetPuantajlarByTarihAraligiAsync(int firmaId, DateTime baslangic, DateTime bitis, int? kurumId = null, int? aracId = null)
    {
        var query = _context.FiloGunlukPuantajlar
            .Include(p => p.KurumFirma)
            .Include(p => p.Guzergah)
            .Include(p => p.Arac)
            .Include(p => p.Sofor)
            .Where(p => p.FirmaId == firmaId && p.Tarih >= baslangic && p.Tarih <= bitis && !p.IsDeleted);

        if (kurumId.HasValue && kurumId.Value > 0)
            query = query.Where(p => p.KurumFirmaId == kurumId.Value);

        if (aracId.HasValue && aracId.Value > 0)
            query = query.Where(p => p.AracId == aracId.Value);

        return await query.OrderBy(p => p.Tarih).ThenBy(p => p.KurumFirma!.FirmaAdi).ToListAsync();
    }

    public async Task<FiloGunlukPuantaj> CreatePuantajAsync(FiloGunlukPuantaj puantaj)
    {
        _context.FiloGunlukPuantajlar.Add(puantaj);
        await _context.SaveChangesAsync();
        return puantaj;
    }

    public async Task<FiloGunlukPuantaj> UpdateGunlukPuantajAsync(FiloGunlukPuantaj puantaj)
    {
        var existing = await _context.FiloGunlukPuantajlar.FindAsync(puantaj.Id);
        if(existing != null)
        {
            existing.Durum = puantaj.Durum;
            existing.PuantajCarpani = puantaj.PuantajCarpani;
            existing.TahakkukEdenKurumUcreti = puantaj.TahakkukEdenKurumUcreti;
            existing.TahakkukEdenTaseronUcreti = puantaj.TahakkukEdenTaseronUcreti;
            existing.TaksiKullanildiMi = puantaj.TaksiKullanildiMi;
            existing.TaksiFisTutari = puantaj.TaksiFisTutari;
            existing.TaksiFisAciklama = puantaj.TaksiFisAciklama;
            existing.ArizaYaptiMi = puantaj.ArizaYaptiMi;
            existing.ArizaAciklamasi = puantaj.ArizaAciklamasi;
            existing.Notlar = puantaj.Notlar;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
        return existing ?? puantaj;
    }

    public async Task KurumFaturalastirAsync(List<int> puantajIds)
    {
        var puantajlar = await _context.FiloGunlukPuantajlar
            .Where(p => puantajIds.Contains(p.Id))
            .ToListAsync();

        foreach(var p in puantajlar)
        {
            p.KurumFaturaKesildiMi = true;
            p.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task TaseronOdeAsync(List<int> puantajIds)
    {
        var puantajlar = await _context.FiloGunlukPuantajlar
            .Where(p => puantajIds.Contains(p.Id))
            .ToListAsync();

        foreach(var p in puantajlar)
        {
            p.TaseronOdemeYapildiMi = true;
            p.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<Arac>> GetAraclarAsync(int firmaId)
    {
        return await _context.Araclar
            .Where(a => !a.IsDeleted)
            .OrderBy(a => a.AktifPlaka)
            .ThenBy(a => a.SaseNo)
            .ToListAsync();
    }

    public async Task<List<Cari>> GetKurumlarAsync(int firmaId)
    {
        return await _context.Cariler
            .Where(c => !c.IsDeleted && (c.CariTipi == CariTipi.Musteri || c.CariTipi == CariTipi.MusteriTedarikci))
            .OrderBy(c => c.Unvan)
            .ToListAsync();
    }

    public async Task<List<Sofor>> GetSoforlerAsync()
    {
        return await _context.Soforler
            .Where(s => !s.IsDeleted && s.Aktif)
            .OrderBy(s => s.Ad)
            .ThenBy(s => s.Soyad)
            .ToListAsync();
    }

    public async Task<List<Guzergah>> GetGuzergahlarAsync()
    {
        return await _context.Guzergahlar
            .Where(g => !g.IsDeleted && g.Aktif)
            .OrderBy(g => g.GuzergahAdi)
            .ToListAsync();
    }
}
