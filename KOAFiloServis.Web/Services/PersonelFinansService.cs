using KOAFiloServis.Shared.Entities;
using KOAFiloServis.Web.Data;
using KOAFiloServis.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;

namespace KOAFiloServis.Web.Services;

public class PersonelFinansService : IPersonelFinansService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IMuhasebeService _muhasebeService;

    public PersonelFinansService(IDbContextFactory<ApplicationDbContext> contextFactory, IMuhasebeService muhasebeService)
    {
        _contextFactory = contextFactory;
        _muhasebeService = muhasebeService;
    }

    #region Avans İşlemleri

    public async Task<List<PersonelAvans>> GetAvanslarAsync(int? firmaId = null, int? personelId = null, DateTime? baslangic = null, DateTime? bitis = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Set<PersonelAvans>()
            .Include(a => a.Personel)
            .Include(a => a.Firma)
            .Include(a => a.BankaHesap)
            .Include(a => a.Mahsuplasmalar)
            .AsNoTracking()
            .AsQueryable();

        if (firmaId.HasValue)
            query = query.Where(a => a.FirmaId == firmaId);

        if (personelId.HasValue)
            query = query.Where(a => a.PersonelId == personelId);

        if (baslangic.HasValue)
            query = query.Where(a => a.AvansTarihi >= baslangic.Value);

        if (bitis.HasValue)
            query = query.Where(a => a.AvansTarihi <= bitis.Value);

        return await query.OrderByDescending(a => a.AvansTarihi).ToListAsync();
    }

    public async Task<PersonelAvans?> GetAvansByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Set<PersonelAvans>()
            .Include(a => a.Personel)
            .Include(a => a.Firma)
            .Include(a => a.BankaHesap)
            .Include(a => a.Mahsuplasmalar)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<PersonelAvans> CreateAvansAsync(PersonelAvans avans, bool muhasebeKaydiOlustur = true)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        avans.Durum = AvansDurum.Verildi;
        avans.MahsupEdilen = 0;
        avans.CreatedAt = DateTime.UtcNow;

        context.Set<PersonelAvans>().Add(avans);
        await context.SaveChangesAsync();

        // Muhasebe kaydı oluştur
        if (muhasebeKaydiOlustur)
        {
            await CreateAvansMuhasebeFisiAsync(avans);
        }

        return avans;
    }

    public async Task<PersonelAvans> UpdateAvansAsync(PersonelAvans avans)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.Set<PersonelAvans>().FindAsync(avans.Id);
        if (existing == null)
            throw new InvalidOperationException($"Avans bulunamadı. Id: {avans.Id}");

        existing.AvansTarihi = avans.AvansTarihi;
        existing.Tutar = avans.Tutar;
        existing.Aciklama = avans.Aciklama;
        existing.OdemeSekli = avans.OdemeSekli;
        existing.BankaHesapId = avans.BankaHesapId;
        existing.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAvansAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var avans = await context.Set<PersonelAvans>()
            .Include(a => a.Mahsuplasmalar)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (avans == null)
            throw new InvalidOperationException($"Avans bulunamadı. Id: {id}");

        if (avans.Mahsuplasmalar.Any())
            throw new InvalidOperationException("Mahsuplaşması olan avans silinemez!");

        avans.IsDeleted = true;
        await context.SaveChangesAsync();
    }

    public async Task<PersonelAvans> IptalEtAvansAsync(int id, string iptalNedeni)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var avans = await context.Set<PersonelAvans>().FindAsync(id);
        if (avans == null)
            throw new InvalidOperationException($"Avans bulunamadı. Id: {id}");

        avans.Durum = AvansDurum.IptalEdildi;
        avans.Aciklama = (avans.Aciklama ?? "") + $" [İPTAL: {iptalNedeni}]";
        avans.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return avans;
    }

    #endregion

    #region Avans Mahsup

    public async Task<PersonelAvansMahsup> MahsupEtAvansAsync(int avansId, PersonelAvansMahsup mahsup)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var avans = await context.Set<PersonelAvans>().FindAsync(avansId);
        if (avans == null)
            throw new InvalidOperationException($"Avans bulunamadı. Id: {avansId}");

        if (mahsup.MahsupTutari > avans.Kalan)
            throw new InvalidOperationException("Mahsup tutarı kalan avanstan fazla olamaz!");

        mahsup.AvansId = avansId;
        mahsup.CreatedAt = DateTime.UtcNow;

        context.Set<PersonelAvansMahsup>().Add(mahsup);

        // Avans mahsup bilgisini güncelle
        avans.MahsupEdilen += mahsup.MahsupTutari;
        if (avans.Kalan <= 0)
        {
            avans.Durum = AvansDurum.TamamenMahsup;
            avans.MahsupTarihi = mahsup.MahsupTarihi;
        }
        else
        {
            avans.Durum = AvansDurum.KismenMahsup;
        }
        avans.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return mahsup;
    }

    public async Task<List<PersonelAvansMahsup>> GetAvansMahsuplasmalarAsync(int avansId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Set<PersonelAvansMahsup>()
            .Include(m => m.BankaHesap)
            .Include(m => m.Maas)
            .Where(m => m.AvansId == avansId)
            .OrderByDescending(m => m.MahsupTarihi)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<decimal> MaasaAcikAvansMahsupEtAsync(int maasId, DateTime? mahsupTarihi = null, string? aciklama = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var maas = await context.PersonelMaaslari
            .FirstOrDefaultAsync(m => m.Id == maasId && !m.IsDeleted);

        if (maas == null)
            throw new InvalidOperationException($"Maaş kaydı bulunamadı. Id: {maasId}");

        if (maas.OdemeDurum == MaasOdemeDurum.Odendi)
            throw new InvalidOperationException("Ödenmiş maaşa mahsup uygulanamaz.");

        var mahsupEdilebilirTutar = Math.Max(0, maas.OdenecekTutar);
        if (mahsupEdilebilirTutar <= 0)
            throw new InvalidOperationException("Maaş üzerinde mahsup edilebilecek tutar bulunmuyor.");

        var acikAvanslar = await context.Set<PersonelAvans>()
            .Where(a => !a.IsDeleted &&
                        a.PersonelId == maas.SoforId &&
                        a.Durum != AvansDurum.IptalEdildi &&
                        a.MahsupEdilen < a.Tutar)
            .OrderBy(a => a.AvansTarihi)
            .ToListAsync();

        if (!acikAvanslar.Any())
            throw new InvalidOperationException("Mahsup edilecek açık avans bulunamadı.");

        var toplamMahsup = 0m;
        var islemTarihi = mahsupTarihi?.Date ?? DateTime.Today;

        foreach (var avans in acikAvanslar)
        {
            var kalanKapasite = mahsupEdilebilirTutar - toplamMahsup;
            if (kalanKapasite <= 0)
                break;

            var mahsupTutari = Math.Min(avans.Kalan, kalanKapasite);
            if (mahsupTutari <= 0)
                continue;

            var mahsup = new PersonelAvansMahsup
            {
                AvansId = avans.Id,
                MaasId = maas.Id,
                MahsupTarihi = islemTarihi,
                MahsupTutari = mahsupTutari,
                Aciklama = aciklama,
                MahsupSekli = MahsupSekli.MaastanKesinti,
                CreatedAt = DateTime.UtcNow
            };

            context.Set<PersonelAvansMahsup>().Add(mahsup);

            avans.MahsupEdilen += mahsupTutari;
            avans.MahsupTarihi = islemTarihi;
            avans.MahsupAciklamasi = aciklama;
            avans.Durum = avans.Kalan <= 0 ? AvansDurum.TamamenMahsup : AvansDurum.KismenMahsup;
            avans.UpdatedAt = DateTime.UtcNow;

            toplamMahsup += mahsupTutari;
        }

        if (toplamMahsup <= 0)
            throw new InvalidOperationException("Maaşa uygulanabilecek avans mahsubu bulunamadı.");

        maas.Avans += toplamMahsup;
        maas.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(aciklama))
        {
            var yeniNot = $"{islemTarihi:dd.MM.yyyy} maaş mahsubu: {toplamMahsup:N2} ₺ - {aciklama}";
            maas.Notlar = string.IsNullOrWhiteSpace(maas.Notlar)
                ? yeniNot
                : $"{maas.Notlar}{Environment.NewLine}{yeniNot}";
        }

        await context.SaveChangesAsync();
        return toplamMahsup;
    }

    public async Task DeleteMahsupAsync(int mahsupId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var mahsup = await context.Set<PersonelAvansMahsup>()
            .Include(m => m.Avans)
            .FirstOrDefaultAsync(m => m.Id == mahsupId);

        if (mahsup == null)
            throw new InvalidOperationException($"Mahsup kaydı bulunamadı. Id: {mahsupId}");

        // Avans mahsup bilgisini güncelle
        var avans = mahsup.Avans;
        avans.MahsupEdilen -= mahsup.MahsupTutari;
        avans.Durum = avans.Kalan > 0 ? AvansDurum.KismenMahsup : AvansDurum.TamamenMahsup;
        if (avans.MahsupEdilen == 0)
            avans.Durum = AvansDurum.Verildi;

        mahsup.IsDeleted = true;
        await context.SaveChangesAsync();
    }

    #endregion

    #region Borç İşlemleri

    public async Task<List<PersonelBorc>> GetBorclarAsync(int? firmaId = null, int? personelId = null, BorcOdemeDurum? durum = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Set<PersonelBorc>()
            .Include(b => b.Personel)
            .Include(b => b.Firma)
            .Include(b => b.BankaHesap)
            .Include(b => b.Odemeler)
            .AsNoTracking()
            .AsQueryable();

        if (firmaId.HasValue)
            query = query.Where(b => b.FirmaId == firmaId);

        if (personelId.HasValue)
            query = query.Where(b => b.PersonelId == personelId);

        if (durum.HasValue)
            query = query.Where(b => b.OdemeDurum == durum);

        return await query.OrderByDescending(b => b.BorcTarihi).ToListAsync();
    }

    public async Task<PersonelBorc?> GetBorcByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Set<PersonelBorc>()
            .Include(b => b.Personel)
            .Include(b => b.Firma)
            .Include(b => b.BankaHesap)
            .Include(b => b.Odemeler)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<PersonelBorc> CreateBorcAsync(PersonelBorc borc, bool muhasebeKaydiOlustur = true)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        borc.OdemeDurum = BorcOdemeDurum.Bekliyor;
        borc.OdenenTutar = 0;
        borc.CreatedAt = DateTime.UtcNow;

        context.Set<PersonelBorc>().Add(borc);
        await context.SaveChangesAsync();

        // Muhasebe kaydı oluştur
        if (muhasebeKaydiOlustur)
        {
            await CreateBorcMuhasebeFisiAsync(borc);
        }

        return borc;
    }

    public async Task<PersonelBorc> UpdateBorcAsync(PersonelBorc borc)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.Set<PersonelBorc>().FindAsync(borc.Id);
        if (existing == null)
            throw new InvalidOperationException($"Borç bulunamadı. Id: {borc.Id}");

        existing.BorcTarihi = borc.BorcTarihi;
        existing.Tutar = borc.Tutar;
        existing.BorcNedeni = borc.BorcNedeni;
        existing.Aciklama = borc.Aciklama;
        existing.BorcTipi = borc.BorcTipi;
        existing.PlanlananOdemeTarihi = borc.PlanlananOdemeTarihi;
        existing.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteBorcAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var borc = await context.Set<PersonelBorc>()
            .Include(b => b.Odemeler)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (borc == null)
            throw new InvalidOperationException($"Borç bulunamadı. Id: {id}");

        if (borc.Odemeler.Any())
            throw new InvalidOperationException("Ödemesi olan borç silinemez!");

        borc.IsDeleted = true;
        await context.SaveChangesAsync();
    }

    public async Task<PersonelBorc> IptalEtBorcAsync(int id, string iptalNedeni)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var borc = await context.Set<PersonelBorc>().FindAsync(id);
        if (borc == null)
            throw new InvalidOperationException($"Borç bulunamadı. Id: {id}");

        borc.OdemeDurum = BorcOdemeDurum.IptalEdildi;
        borc.Aciklama = (borc.Aciklama ?? "") + $" [İPTAL: {iptalNedeni}]";
        borc.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return borc;
    }

    #endregion

    #region Borç Ödeme

    public async Task<PersonelBorcOdeme> OdemeYapBorcAsync(int borcId, PersonelBorcOdeme odeme, bool muhasebeKaydiOlustur = true)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var borc = await context.Set<PersonelBorc>().FindAsync(borcId);
        if (borc == null)
            throw new InvalidOperationException($"Borç bulunamadı. Id: {borcId}");

        if (odeme.OdemeTutari > borc.KalanBorc)
            throw new InvalidOperationException("Ödeme tutarı kalan borçtan fazla olamaz!");

        odeme.BorcId = borcId;
        odeme.CreatedAt = DateTime.UtcNow;

        context.Set<PersonelBorcOdeme>().Add(odeme);

        // Borç ödeme bilgisini güncelle
        borc.OdenenTutar += odeme.OdemeTutari;
        if (borc.KalanBorc <= 0)
        {
            borc.OdemeDurum = BorcOdemeDurum.TamamenOdendi;
            borc.GerceklesenOdemeTarihi = odeme.OdemeTarihi;
        }
        else
        {
            borc.OdemeDurum = BorcOdemeDurum.KismenOdendi;
        }
        borc.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        // Muhasebe kaydı oluştur
        if (muhasebeKaydiOlustur)
        {
            await CreateBorcOdemeMuhasebeFisiAsync(odeme, borc);
        }

        return odeme;
    }

    public async Task<List<PersonelBorcOdeme>> GetBorcOdemelerAsync(int borcId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Set<PersonelBorcOdeme>()
            .Include(o => o.BankaHesap)
            .Where(o => o.BorcId == borcId)
            .OrderByDescending(o => o.OdemeTarihi)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task DeleteBorcOdemeAsync(int odemeId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var odeme = await context.Set<PersonelBorcOdeme>()
            .Include(o => o.Borc)
            .FirstOrDefaultAsync(o => o.Id == odemeId);

        if (odeme == null)
            throw new InvalidOperationException($"Ödeme kaydı bulunamadı. Id: {odemeId}");

        // Borç ödeme bilgisini güncelle
        var borc = odeme.Borc;
        borc.OdenenTutar -= odeme.OdemeTutari;
        borc.OdemeDurum = borc.KalanBorc > 0 ? BorcOdemeDurum.KismenOdendi : BorcOdemeDurum.TamamenOdendi;
        if (borc.OdenenTutar == 0)
            borc.OdemeDurum = BorcOdemeDurum.Bekliyor;

        odeme.IsDeleted = true;
        await context.SaveChangesAsync();
    }

    #endregion

    #region Personel Özet

    public async Task<PersonelFinansOzet> GetPersonelFinansOzetAsync(int personelId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var personel = await context.Soforler.AsNoTracking().FirstOrDefaultAsync(p => p.Id == personelId);
        if (personel == null)
            throw new InvalidOperationException($"Personel bulunamadı. Id: {personelId}");

        var avanslar = await context.Set<PersonelAvans>()
            .Where(a => a.PersonelId == personelId && a.Durum != AvansDurum.IptalEdildi)
            .AsNoTracking()
            .ToListAsync();

        var borclar = await context.Set<PersonelBorc>()
            .Where(b => b.PersonelId == personelId && b.OdemeDurum != BorcOdemeDurum.IptalEdildi)
            .AsNoTracking()
            .ToListAsync();

        // Personelin cebinden yaptığı ödenmemiş masraflar
        var aracMasrafToplam = await context.AracMasraflari
            .Where(m => m.PersonelCebindenId == personelId && !m.PersoneleOdendi)
            .AsNoTracking()
            .SumAsync(m => (decimal?)m.Tutar) ?? 0;

        var aracMasrafAdet = await context.AracMasraflari
            .Where(m => m.PersonelCebindenId == personelId && !m.PersoneleOdendi)
            .AsNoTracking()
            .CountAsync();

        var bankaHareketToplam = await context.BankaKasaHareketleri
            .Where(h => h.PersonelCebindenId == personelId && !h.PersoneleOdendi)
            .AsNoTracking()
            .SumAsync(h => (decimal?)h.Tutar) ?? 0;

        var bankaHareketAdet = await context.BankaKasaHareketleri
            .Where(h => h.PersonelCebindenId == personelId && !h.PersoneleOdendi)
            .AsNoTracking()
            .CountAsync();

        return new PersonelFinansOzet
        {
            PersonelId = personel.Id,
            PersonelKodu = personel.SoforKodu,
            PersonelAdSoyad = personel.TamAd,
            Departman = personel.Departman,
            Aktif = personel.Aktif,

            ToplamAvansSayisi = avanslar.Count,
            ToplamAvans = avanslar.Sum(a => a.Tutar),
            MahsupEdilenAvans = avanslar.Sum(a => a.MahsupEdilen),
            AcikAvansSayisi = avanslar.Count(a => a.Kalan > 0),

            ToplamBorcSayisi = borclar.Count,
            ToplamBorc = borclar.Sum(b => b.Tutar),
            OdenenBorc = borclar.Sum(b => b.OdenenTutar),
            OdenmemişBorcSayisi = borclar.Count(b => b.KalanBorc > 0),

            ToplamHarcama = aracMasrafToplam + bankaHareketToplam,
            HarcamaAdet = aracMasrafAdet + bankaHareketAdet
        };
    }

    public async Task<List<PersonelCebindenHarcamaItem>> GetPersonelCebindenHarcamalarAsync(int personelId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var aracMasraflar = await context.AracMasraflari
            .Include(m => m.Arac)
            .Where(m => m.PersonelCebindenId == personelId)
            .AsNoTracking()
            .ToListAsync();

        var bankaHareketler = await context.BankaKasaHareketleri
            .Where(h => h.PersonelCebindenId == personelId)
            .AsNoTracking()
            .ToListAsync();

        var liste = new List<PersonelCebindenHarcamaItem>();

        foreach (var m in aracMasraflar)
        {
            var plakaAciklama = m.Arac?.AktifPlaka != null ? $" [{m.Arac.AktifPlaka}]" : "";
            liste.Add(new PersonelCebindenHarcamaItem
            {
                Tarih = m.MasrafTarihi,
                Aciklama = (m.Aciklama ?? "Araç Masrafı") + plakaAciklama,
                Tutar = m.Tutar,
                Kaynak = "AracMasraf",
                KaynakId = m.Id,
                PersoneleOdendi = m.PersoneleOdendi
            });
        }

        foreach (var h in bankaHareketler)
        {
            liste.Add(new PersonelCebindenHarcamaItem
            {
                Tarih = h.IslemTarihi,
                Aciklama = h.Aciklama ?? "Banka/Kasa Hareketi",
                Tutar = h.Tutar,
                Kaynak = "BankaHareket",
                KaynakId = h.Id,
                PersoneleOdendi = h.PersoneleOdendi
            });
        }

        return liste.OrderByDescending(x => x.Tarih).ToList();
    }

    public async Task<List<PersonelFinansOzet>> GetTumPersonelFinansOzetAsync(int? firmaId = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var personeller = await context.Soforler
            .Where(p => p.Aktif)
            .AsNoTracking()
            .ToListAsync();

        var ozetler = new List<PersonelFinansOzet>();

        foreach (var personel in personeller)
        {
            var query1 = context.Set<PersonelAvans>().Where(a => a.PersonelId == personel.Id && a.Durum != AvansDurum.IptalEdildi);
            var query2 = context.Set<PersonelBorc>().Where(b => b.PersonelId == personel.Id && b.OdemeDurum != BorcOdemeDurum.IptalEdildi);

            if (firmaId.HasValue)
            {
                query1 = query1.Where(a => a.FirmaId == firmaId);
                query2 = query2.Where(b => b.FirmaId == firmaId);
            }

            var avanslar = await query1.AsNoTracking().ToListAsync();
            var borclar = await query2.AsNoTracking().ToListAsync();

            var ozet = new PersonelFinansOzet
            {
                PersonelId = personel.Id,
                PersonelKodu = personel.SoforKodu,
                PersonelAdSoyad = personel.TamAd,
                Departman = personel.Departman,
                Aktif = personel.Aktif,

                ToplamAvansSayisi = avanslar.Count,
                ToplamAvans = avanslar.Sum(a => a.Tutar),
                MahsupEdilenAvans = avanslar.Sum(a => a.MahsupEdilen),
                AcikAvansSayisi = avanslar.Count(a => a.Kalan > 0),

                ToplamBorcSayisi = borclar.Count,
                ToplamBorc = borclar.Sum(b => b.Tutar),
                OdenenBorc = borclar.Sum(b => b.OdenenTutar),
                OdenmemişBorcSayisi = borclar.Count(b => b.KalanBorc > 0)
            };

            if (ozet.ToplamAvansSayisi > 0 || ozet.ToplamBorcSayisi > 0)
                ozetler.Add(ozet);
        }

        return ozetler.OrderByDescending(o => Math.Abs(o.NetDurum)).ToList();
    }

    #endregion

    #region Ayarlar

    public async Task<PersonelFinansAyar?> GetAyarlarAsync(int? firmaId = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var ayar = await context.Set<PersonelFinansAyar>()
            .Include(a => a.PersonelAvanslariHesap)
            .Include(a => a.PersoneleBorclarHesap)
            .Include(a => a.KasaHesap)
            .Include(a => a.BankaHesap)
            .FirstOrDefaultAsync(a => a.FirmaId == firmaId);

        if (ayar == null)
        {
            // Varsayılan ayarları oluştur
            ayar = new PersonelFinansAyar
            {
                FirmaId = firmaId,
                OtomatikFisOlustur = true,
                AvansVerildigindeFisOlustur = true,
                AvansMahsupFisOlustur = true,
                BorcOdendigindeFisOlustur = true
            };
        }

        return ayar;
    }

    public async Task<PersonelFinansAyar> SaveAyarlarAsync(PersonelFinansAyar ayar)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.Set<PersonelFinansAyar>()
            .FirstOrDefaultAsync(a => a.FirmaId == ayar.FirmaId);

        if (existing == null)
        {
            context.Set<PersonelFinansAyar>().Add(ayar);
        }
        else
        {
            existing.PersonelAvanslariHesapId = ayar.PersonelAvanslariHesapId;
            existing.PersoneleBorclarHesapId = ayar.PersoneleBorclarHesapId;
            existing.KasaHesapId = ayar.KasaHesapId;
            existing.BankaHesapId = ayar.BankaHesapId;
            existing.OtomatikFisOlustur = ayar.OtomatikFisOlustur;
            existing.AvansVerildigindeFisOlustur = ayar.AvansVerildigindeFisOlustur;
            existing.AvansMahsupFisOlustur = ayar.AvansMahsupFisOlustur;
            existing.BorcOdendigindeFisOlustur = ayar.BorcOdendigindeFisOlustur;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
        return ayar;
    }

    #endregion

    #region Muhasebe Entegrasyonu

    private Task CreateAvansMuhasebeFisiAsync(PersonelAvans avans)
    {
        return Task.CompletedTask;
    }

    private Task CreateBorcMuhasebeFisiAsync(PersonelBorc borc)
    {
        return Task.CompletedTask;
    }

    private Task CreateBorcOdemeMuhasebeFisiAsync(PersonelBorcOdeme odeme, PersonelBorc borc)
    {
        return Task.CompletedTask;
    }

    #endregion

    #region Raporlama

    public async Task<byte[]> ExportAvansRaporAsync(List<PersonelAvans> avanslar)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Personel Avansları");

        // Başlıklar
        worksheet.Cell(1, 1).Value = "Tarih";
        worksheet.Cell(1, 2).Value = "Personel";
        worksheet.Cell(1, 3).Value = "Firma";
        worksheet.Cell(1, 4).Value = "Tutar";
        worksheet.Cell(1, 5).Value = "Ödeme Şekli";
        worksheet.Cell(1, 6).Value = "Mahsup Edilen";
        worksheet.Cell(1, 7).Value = "Kalan";
        worksheet.Cell(1, 8).Value = "Durum";
        worksheet.Cell(1, 9).Value = "Açıklama";

        var headerRange = worksheet.Range(1, 1, 1, 9);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Veriler
        int row = 2;
        foreach (var avans in avanslar)
        {
            worksheet.Cell(row, 1).Value = avans.AvansTarihi.ToString("dd.MM.yyyy");
            worksheet.Cell(row, 2).Value = avans.Personel?.TamAd ?? "";
            worksheet.Cell(row, 3).Value = avans.Firma?.FirmaAdi ?? "-";
            worksheet.Cell(row, 4).Value = avans.Tutar;
            worksheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 5).Value = avans.OdemeSekli.ToString();
            worksheet.Cell(row, 6).Value = avans.MahsupEdilen;
            worksheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 7).Value = avans.Kalan;
            worksheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 8).Value = avans.Durum.ToString();
            worksheet.Cell(row, 9).Value = avans.Aciklama ?? "";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportBorcRaporAsync(List<PersonelBorc> borclar)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Personel Borçları");

        // Başlıklar
        worksheet.Cell(1, 1).Value = "Tarih";
        worksheet.Cell(1, 2).Value = "Personel";
        worksheet.Cell(1, 3).Value = "Firma";
        worksheet.Cell(1, 4).Value = "Borç Nedeni";
        worksheet.Cell(1, 5).Value = "Borç Tipi";
        worksheet.Cell(1, 6).Value = "Tutar";
        worksheet.Cell(1, 7).Value = "Ödenen";
        worksheet.Cell(1, 8).Value = "Kalan";
        worksheet.Cell(1, 9).Value = "Durum";
        worksheet.Cell(1, 10).Value = "Planlanan Ödeme";
        worksheet.Cell(1, 11).Value = "Açıklama";

        var headerRange = worksheet.Range(1, 1, 1, 11);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Veriler
        int row = 2;
        foreach (var borc in borclar)
        {
            worksheet.Cell(row, 1).Value = borc.BorcTarihi.ToString("dd.MM.yyyy");
            worksheet.Cell(row, 2).Value = borc.Personel?.TamAd ?? "";
            worksheet.Cell(row, 3).Value = borc.Firma?.FirmaAdi ?? "-";
            worksheet.Cell(row, 4).Value = borc.BorcNedeni;
            worksheet.Cell(row, 5).Value = borc.BorcTipi.ToString();
            worksheet.Cell(row, 6).Value = borc.Tutar;
            worksheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 7).Value = borc.OdenenTutar;
            worksheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 8).Value = borc.KalanBorc;
            worksheet.Cell(row, 8).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 9).Value = borc.OdemeDurum.ToString();
            worksheet.Cell(row, 10).Value = borc.PlanlananOdemeTarihi?.ToString("dd.MM.yyyy") ?? "-";
            worksheet.Cell(row, 11).Value = borc.Aciklama ?? "";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportPersonelOzetRaporAsync(List<PersonelFinansOzet> ozetler)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Personel Finans Özeti");

        // Başlıklar
        worksheet.Cell(1, 1).Value = "Personel Kodu";
        worksheet.Cell(1, 2).Value = "Ad Soyad";
        worksheet.Cell(1, 3).Value = "Departman";
        worksheet.Cell(1, 4).Value = "Toplam Avans";
        worksheet.Cell(1, 5).Value = "Mahsup Edilen";
        worksheet.Cell(1, 6).Value = "Kalan Avans";
        worksheet.Cell(1, 7).Value = "Toplam Borç";
        worksheet.Cell(1, 8).Value = "Ödenen Borç";
        worksheet.Cell(1, 9).Value = "Kalan Borç";
        worksheet.Cell(1, 10).Value = "Net Durum";
        worksheet.Cell(1, 11).Value = "Açıklama";

        var headerRange = worksheet.Range(1, 1, 1, 11);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Veriler
        int row = 2;
        foreach (var ozet in ozetler)
        {
            worksheet.Cell(row, 1).Value = ozet.PersonelKodu;
            worksheet.Cell(row, 2).Value = ozet.PersonelAdSoyad;
            worksheet.Cell(row, 3).Value = ozet.Departman ?? "-";
            worksheet.Cell(row, 4).Value = ozet.ToplamAvans;
            worksheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 5).Value = ozet.MahsupEdilenAvans;
            worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 6).Value = ozet.KalanAvans;
            worksheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 7).Value = ozet.ToplamBorc;
            worksheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 8).Value = ozet.OdenenBorc;
            worksheet.Cell(row, 8).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 9).Value = ozet.KalanBorc;
            worksheet.Cell(row, 9).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(row, 10).Value = ozet.NetDurum;
            worksheet.Cell(row, 10).Style.NumberFormat.Format = "#,##0.00";
            
            if (ozet.NetDurum > 0)
                worksheet.Cell(row, 10).Style.Font.FontColor = XLColor.Red;
            else if (ozet.NetDurum < 0)
                worksheet.Cell(row, 10).Style.Font.FontColor = XLColor.Green;
            
            worksheet.Cell(row, 11).Value = ozet.NetDurumAciklama;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    #endregion

    #region Toplu İşlemler

    public async Task<int> TopluAvansMahsupAsync(List<int> avansIdler, DateTime mahsupTarihi, string aciklama)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        int sayac = 0;
        foreach (var avansId in avansIdler)
        {
            var avans = await context.Set<PersonelAvans>().FindAsync(avansId);
            if (avans == null || avans.Kalan <= 0) continue;

            var mahsup = new PersonelAvansMahsup
            {
                AvansId = avansId,
                MahsupTarihi = mahsupTarihi,
                MahsupTutari = avans.Kalan,
                MahsupSekli = MahsupSekli.Diger,
                Aciklama = aciklama
            };

            await MahsupEtAvansAsync(avansId, mahsup);
            sayac++;
        }

        return sayac;
    }

    public async Task<int> TopluBorcOdemeAsync(List<int> borcIdler, DateTime odemeTarihi, BorcOdemeSekli odemeSekli, int? bankaHesapId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        int sayac = 0;
        foreach (var borcId in borcIdler)
        {
            var borc = await context.Set<PersonelBorc>().FindAsync(borcId);
            if (borc == null || borc.KalanBorc <= 0) continue;

            var odeme = new PersonelBorcOdeme
            {
                BorcId = borcId,
                OdemeTarihi = odemeTarihi,
                OdemeTutari = borc.KalanBorc,
                OdemeSekli = odemeSekli,
                BankaHesapId = bankaHesapId,
                Aciklama = "Toplu ödeme"
            };

            await OdemeYapBorcAsync(borcId, odeme);
            sayac++;
        }

        return sayac;
    }

    #endregion
}
