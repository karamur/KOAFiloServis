using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class StokService : IStokService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StokService> _logger;

    public StokService(ApplicationDbContext context, ILogger<StokService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Stok Karti

    public async Task<List<StokKarti>> GetStokKartlariAsync(StokTipi? tip = null, int? kategoriId = null, bool? aktif = true)
    {
        var query = _context.StokKartlari
            .Include(s => s.Kategori)
            .Include(s => s.VarsayilanTedarikci)
            .AsQueryable();

        if (tip.HasValue)
            query = query.Where(s => s.StokTipi == tip.Value);

        if (kategoriId.HasValue)
            query = query.Where(s => s.KategoriId == kategoriId.Value);

        if (aktif.HasValue)
            query = query.Where(s => s.Aktif == aktif.Value);

        return await query.OrderBy(s => s.StokKodu).ToListAsync();
    }

    public async Task<StokKarti?> GetStokKartiByIdAsync(int id)
    {
        return await _context.StokKartlari
            .Include(s => s.Kategori)
            .Include(s => s.VarsayilanTedarikci)
            .Include(s => s.MuhasebeHesap)
            .Include(s => s.Hareketler.OrderByDescending(h => h.IslemTarihi).Take(10))
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<StokKarti?> GetStokKartiByKodAsync(string kod)
    {
        return await _context.StokKartlari
            .Include(s => s.Kategori)
            .FirstOrDefaultAsync(s => s.StokKodu == kod);
    }

    public async Task<StokKarti> CreateStokKartiAsync(StokKarti stok)
    {
        if (string.IsNullOrEmpty(stok.StokKodu))
        {
            stok.StokKodu = await GetNextStokKoduAsync(stok.StokTipi);
        }

        stok.CreatedAt = DateTime.UtcNow;
        _context.StokKartlari.Add(stok);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Stok karti olusturuldu: {StokKodu} - {StokAdi}", stok.StokKodu, stok.StokAdi);
        return stok;
    }

    public async Task<StokKarti> UpdateStokKartiAsync(StokKarti stok)
    {
        var existing = await _context.StokKartlari.FindAsync(stok.Id);
        if (existing == null)
            throw new Exception("Stok karti bulunamadi");

        existing.StokAdi = stok.StokAdi;
        existing.Aciklama = stok.Aciklama;
        existing.Barkod = stok.Barkod;
        existing.StokTipi = stok.StokTipi;
        existing.AltTipi = stok.AltTipi;
        existing.KategoriId = stok.KategoriId;
        existing.Birim = stok.Birim;
        existing.AlisFiyati = stok.AlisFiyati;
        existing.SatisFiyati = stok.SatisFiyati;
        existing.KdvOrani = stok.KdvOrani;
        existing.StokTakibiYapilsin = stok.StokTakibiYapilsin;
        existing.MinStokMiktari = stok.MinStokMiktari;
        existing.MaksStokMiktari = stok.MaksStokMiktari;
        existing.VarsayilanTedarikciId = stok.VarsayilanTedarikciId;
        existing.MuhasebeHesapId = stok.MuhasebeHesapId;
        existing.Aktif = stok.Aktif;
        existing.ResimUrl = stok.ResimUrl;
        existing.Notlar = stok.Notlar;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteStokKartiAsync(int id)
    {
        var stok = await _context.StokKartlari.FindAsync(id);
        if (stok != null)
        {
            stok.IsDeleted = true;
            stok.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GetNextStokKoduAsync(StokTipi tip)
    {
        var prefix = tip switch
        {
            StokTipi.Mal => "MAL",
            StokTipi.Hizmet => "HZM",
            StokTipi.Arac => "ARC",
            StokTipi.YedekParca => "YDP",
            StokTipi.SarfMalzeme => "SRF",
            StokTipi.Demirbas => "DMR",
            _ => "STK"
        };

        var lastStok = await _context.StokKartlari
            .IgnoreQueryFilters()
            .Where(s => s.StokKodu.StartsWith(prefix))
            .OrderByDescending(s => s.StokKodu)
            .FirstOrDefaultAsync();

        int nextNum = 1;
        if (lastStok != null)
        {
            var numPart = lastStok.StokKodu.Replace(prefix, "");
            if (int.TryParse(numPart, out var num))
                nextNum = num + 1;
        }

        return $"{prefix}{nextNum:D5}";
    }

    #endregion

    #region Stok Kategori

    public async Task<List<StokKategori>> GetKategorilerAsync(bool? aktif = true)
    {
        var query = _context.StokKategoriler
            .Include(k => k.UstKategori)
            .Include(k => k.AltKategoriler)
            .AsQueryable();

        if (aktif.HasValue)
            query = query.Where(k => k.Aktif == aktif.Value);

        return await query.OrderBy(k => k.Sira).ThenBy(k => k.KategoriAdi).ToListAsync();
    }

    public async Task<StokKategori?> GetKategoriByIdAsync(int id)
    {
        return await _context.StokKategoriler
            .Include(k => k.UstKategori)
            .Include(k => k.AltKategoriler)
            .Include(k => k.StokKartlari)
            .FirstOrDefaultAsync(k => k.Id == id);
    }

    public async Task<StokKategori> CreateKategoriAsync(StokKategori kategori)
    {
        kategori.CreatedAt = DateTime.UtcNow;
        _context.StokKategoriler.Add(kategori);
        await _context.SaveChangesAsync();
        return kategori;
    }

    public async Task<StokKategori> UpdateKategoriAsync(StokKategori kategori)
    {
        var existing = await _context.StokKategoriler.FindAsync(kategori.Id);
        if (existing == null)
            throw new Exception("Kategori bulunamadi");

        existing.KategoriAdi = kategori.KategoriAdi;
        existing.Aciklama = kategori.Aciklama;
        existing.UstKategoriId = kategori.UstKategoriId;
        existing.Renk = kategori.Renk;
        existing.Icon = kategori.Icon;
        existing.Sira = kategori.Sira;
        existing.Aktif = kategori.Aktif;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteKategoriAsync(int id)
    {
        var kategori = await _context.StokKategoriler.FindAsync(id);
        if (kategori != null)
        {
            kategori.IsDeleted = true;
            kategori.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Stok Hareket

    public async Task<List<StokHareket>> GetStokHareketleriAsync(int? stokKartiId = null, DateTime? baslangic = null, DateTime? bitis = null)
    {
        var query = _context.StokHareketler
            .Include(h => h.StokKarti)
            .Include(h => h.Cari)
            .Include(h => h.Arac)
            .AsQueryable();

        if (stokKartiId.HasValue)
            query = query.Where(h => h.StokKartiId == stokKartiId.Value);

        if (baslangic.HasValue)
            query = query.Where(h => h.IslemTarihi >= baslangic.Value);

        if (bitis.HasValue)
            query = query.Where(h => h.IslemTarihi <= bitis.Value);

        return await query.OrderByDescending(h => h.IslemTarihi).ToListAsync();
    }

    public async Task<StokHareket> CreateStokHareketAsync(StokHareket hareket)
    {
        hareket.CreatedAt = DateTime.UtcNow;
        _context.StokHareketler.Add(hareket);
        await _context.SaveChangesAsync();

        // Stok miktarini guncelle
        await UpdateStokMiktariAsync(hareket.StokKartiId);

        return hareket;
    }

    public async Task UpdateStokMiktariAsync(int stokKartiId)
    {
        var stok = await _context.StokKartlari.FindAsync(stokKartiId);
        if (stok == null) return;

        var toplamMiktar = await _context.StokHareketler
            .Where(h => h.StokKartiId == stokKartiId)
            .SumAsync(h => h.Miktar);

        stok.MevcutStok = toplamMiktar;
        stok.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<decimal> GetMevcutStokAsync(int stokKartiId)
    {
        return await _context.StokHareketler
            .Where(h => h.StokKartiId == stokKartiId)
            .SumAsync(h => h.Miktar);
    }

    #endregion

    #region Arac Islem (Alis/Satis)

    public async Task<List<AracIslem>> GetAracIslemleriAsync(int? aracId = null, AracIslemTipi? tip = null)
    {
        var query = _context.AracIslemler
            .Include(i => i.Arac)
            .Include(i => i.Cari)
            .Include(i => i.Fatura)
            .AsQueryable();

        if (aracId.HasValue)
            query = query.Where(i => i.AracId == aracId.Value);

        if (tip.HasValue)
            query = query.Where(i => i.IslemTipi == tip.Value);

        return await query.OrderByDescending(i => i.IslemTarihi).ToListAsync();
    }

    public async Task<AracIslem?> GetAracIslemByIdAsync(int id)
    {
        return await _context.AracIslemler
            .Include(i => i.Arac)
            .Include(i => i.Cari)
            .Include(i => i.Fatura)
            .Include(i => i.StokHareket)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<AracIslem> CreateAracIslemAsync(AracIslem islem)
    {
        // KDV hesapla
        islem.KdvTutar = islem.Tutar * islem.KdvOrani / 100;
        islem.ToplamTutar = islem.Tutar + islem.KdvTutar;
        islem.CreatedAt = DateTime.UtcNow;

        _context.AracIslemler.Add(islem);
        await _context.SaveChangesAsync();

        // Arac durumunu guncelle
        var arac = await _context.Araclar.FindAsync(islem.AracId);
        if (arac != null)
        {
            if (islem.IslemTipi == AracIslemTipi.Satis)
            {
                arac.Aktif = false;
                arac.SatisaAcik = false;
            }
            else if (islem.IslemTipi == AracIslemTipi.Alis)
            {
                arac.Aktif = true;
            }

            if (islem.Kilometre.HasValue)
            {
                arac.KmDurumu = islem.Kilometre;
            }

            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("Arac islemi olusturuldu: {IslemTipi} - Arac ID: {AracId}", islem.IslemTipi, islem.AracId);
        return islem;
    }

    public async Task<AracIslem> UpdateAracIslemAsync(AracIslem islem)
    {
        var existing = await _context.AracIslemler.FindAsync(islem.Id);
        if (existing == null)
            throw new Exception("Arac islemi bulunamadi");

        existing.IslemTipi = islem.IslemTipi;
        existing.IslemTarihi = islem.IslemTarihi;
        existing.CariId = islem.CariId;
        existing.Tutar = islem.Tutar;
        existing.KdvOrani = islem.KdvOrani;
        existing.KdvTutar = islem.Tutar * islem.KdvOrani / 100;
        existing.ToplamTutar = islem.Tutar + existing.KdvTutar;
        existing.FaturaId = islem.FaturaId;
        existing.Aciklama = islem.Aciklama;
        existing.Notlar = islem.Notlar;
        existing.Kilometre = islem.Kilometre;
        existing.NoterId = islem.NoterId;
        existing.NoterTarihi = islem.NoterTarihi;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAracIslemAsync(int id)
    {
        var islem = await _context.AracIslemler.FindAsync(id);
        if (islem != null)
        {
            islem.IsDeleted = true;
            islem.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Servis Kaydi

    public async Task<List<ServisKaydi>> GetServisKayitlariAsync(int? aracId = null, ServisTipi? tip = null, DateTime? baslangic = null, DateTime? bitis = null)
    {
        var query = _context.ServisKayitlari
            .Include(s => s.Arac)
            .Include(s => s.ServisciCari)
            .Include(s => s.Parcalar)
            .AsQueryable();

        if (aracId.HasValue)
            query = query.Where(s => s.AracId == aracId.Value);

        if (tip.HasValue)
            query = query.Where(s => s.ServisTipi == tip.Value);

        if (baslangic.HasValue)
            query = query.Where(s => s.ServisTarihi >= baslangic.Value);

        if (bitis.HasValue)
            query = query.Where(s => s.ServisTarihi <= bitis.Value);

        return await query.OrderByDescending(s => s.ServisTarihi).ToListAsync();
    }

    public async Task<ServisKaydi?> GetServisKaydiByIdAsync(int id)
    {
        return await _context.ServisKayitlari
            .Include(s => s.Arac)
            .Include(s => s.ServisciCari)
            .Include(s => s.Parcalar)
                .ThenInclude(p => p.StokKarti)
            .Include(s => s.Fatura)
            .Include(s => s.AracMasraf)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<ServisKaydi> CreateServisKaydiAsync(ServisKaydi servis)
    {
        // Toplam tutar hesapla
        servis.ParcaTutari = servis.Parcalar?.Sum(p => p.Miktar * p.BirimFiyat) ?? 0;
        var toplamNet = servis.IscilikTutari + servis.ParcaTutari;
        servis.KdvTutar = toplamNet * servis.KdvOrani / 100;
        servis.ToplamTutar = toplamNet + servis.KdvTutar;
        servis.CreatedAt = DateTime.UtcNow;

        _context.ServisKayitlari.Add(servis);
        await _context.SaveChangesAsync();

        // Arac km guncelle
        if (servis.Kilometre.HasValue)
        {
            var arac = await _context.Araclar.FindAsync(servis.AracId);
            if (arac != null)
            {
                arac.KmDurumu = servis.Kilometre;
                await _context.SaveChangesAsync();
            }
        }

        _logger.LogInformation("Servis kaydi olusturuldu: {ServisAdi} - Arac ID: {AracId}", servis.ServisAdi, servis.AracId);
        return servis;
    }

    public async Task<ServisKaydi> UpdateServisKaydiAsync(ServisKaydi servis)
    {
        var existing = await _context.ServisKayitlari
            .Include(s => s.Parcalar)
            .FirstOrDefaultAsync(s => s.Id == servis.Id);

        if (existing == null)
            throw new Exception("Servis kaydi bulunamadi");

        existing.ServisTarihi = servis.ServisTarihi;
        existing.ServisciCariId = servis.ServisciCariId;
        existing.ServisTipi = servis.ServisTipi;
        existing.ServisAdi = servis.ServisAdi;
        existing.Aciklama = servis.Aciklama;
        existing.IscilikTutari = servis.IscilikTutari;
        existing.KdvOrani = servis.KdvOrani;
        existing.Kilometre = servis.Kilometre;
        existing.Durum = servis.Durum;
        existing.GarantiKapsaminda = servis.GarantiKapsaminda;
        existing.GarantiBitisTarihi = servis.GarantiBitisTarihi;
        existing.Notlar = servis.Notlar;

        // Parcalari guncelle
        _context.ServisParcalar.RemoveRange(existing.Parcalar);
        if (servis.Parcalar != null)
        {
            foreach (var parca in servis.Parcalar)
            {
                parca.ServisKaydiId = existing.Id;
                _context.ServisParcalar.Add(parca);
            }
        }

        // Toplam tutar hesapla
        existing.ParcaTutari = servis.Parcalar?.Sum(p => p.Miktar * p.BirimFiyat) ?? 0;
        var toplamNet = existing.IscilikTutari + existing.ParcaTutari;
        existing.KdvTutar = toplamNet * existing.KdvOrani / 100;
        existing.ToplamTutar = toplamNet + existing.KdvTutar;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteServisKaydiAsync(int id)
    {
        var servis = await _context.ServisKayitlari.FindAsync(id);
        if (servis != null)
        {
            servis.IsDeleted = true;
            servis.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Dashboard

    public async Task<StokDashboard> GetDashboardAsync()
    {
        var dashboard = new StokDashboard();

        var buAyBaslangic = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var buAyBitis = buAyBaslangic.AddMonths(1).AddDays(-1);

        dashboard.ToplamStokKarti = await _context.StokKartlari.CountAsync();
        dashboard.AktifStokKarti = await _context.StokKartlari.CountAsync(s => s.Aktif);

        dashboard.DusukStoklu = await _context.StokKartlari
            .Where(s => s.StokTakibiYapilsin && s.MevcutStok <= s.MinStokMiktari)
            .CountAsync();

        dashboard.ToplamStokDegeri = await _context.StokKartlari
            .Where(s => s.StokTakibiYapilsin)
            .SumAsync(s => s.MevcutStok * s.AlisFiyati);

        dashboard.AylikAracAlis = await _context.AracIslemler
            .Where(i => i.IslemTipi == AracIslemTipi.Alis && 
                       i.IslemTarihi >= buAyBaslangic && i.IslemTarihi <= buAyBitis)
            .CountAsync();

        dashboard.AylikAracSatis = await _context.AracIslemler
            .Where(i => i.IslemTipi == AracIslemTipi.Satis && 
                       i.IslemTarihi >= buAyBaslangic && i.IslemTarihi <= buAyBitis)
            .CountAsync();

        dashboard.AylikServisKaydi = await _context.ServisKayitlari
            .Where(s => s.ServisTarihi >= buAyBaslangic && s.ServisTarihi <= buAyBitis)
            .CountAsync();

        dashboard.AylikServisTutari = await _context.ServisKayitlari
            .Where(s => s.ServisTarihi >= buAyBaslangic && s.ServisTarihi <= buAyBitis)
            .SumAsync(s => s.ToplamTutar);

        dashboard.SonHareketler = await _context.StokHareketler
            .Include(h => h.StokKarti)
            .OrderByDescending(h => h.IslemTarihi)
            .Take(10)
            .ToListAsync();

        dashboard.SonServisler = await _context.ServisKayitlari
            .Include(s => s.Arac)
            .OrderByDescending(s => s.ServisTarihi)
            .Take(10)
            .ToListAsync();

        return dashboard;
    }

    #endregion
}
