using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class BankaKasaHareketService : IBankaKasaHareketService
{
    private readonly ApplicationDbContext _context;

    public BankaKasaHareketService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BankaKasaHareket>> GetAllAsync()
    {
        return await _context.BankaKasaHareketleri
            .Include(h => h.BankaHesap)
            .Include(h => h.Cari)
            .OrderByDescending(h => h.IslemTarihi)
            .ToListAsync();
    }

    public async Task<List<BankaKasaHareket>> GetRecentAsync(int count = 5)
    {
        return await _context.BankaKasaHareketleri
            .Include(h => h.BankaHesap)
            .Include(h => h.Cari)
            .OrderByDescending(h => h.IslemTarihi)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<BankaKasaHareket>> GetByHesapIdAsync(int hesapId)
    {
        return await _context.BankaKasaHareketleri
            .Include(h => h.Cari)
            .Where(h => h.BankaHesapId == hesapId)
            .OrderByDescending(h => h.IslemTarihi)
            .ToListAsync();
    }

    public async Task<List<BankaKasaHareket>> GetByCariIdAsync(int cariId)
    {
        return await _context.BankaKasaHareketleri
            .Include(h => h.BankaHesap)
            .Where(h => h.CariId == cariId)
            .OrderByDescending(h => h.IslemTarihi)
            .ToListAsync();
    }

    public async Task<List<BankaKasaHareket>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.BankaKasaHareketleri
            .Include(h => h.BankaHesap)
            .Include(h => h.Cari)
            .Where(h => h.IslemTarihi >= startDate && h.IslemTarihi <= endDate)
            .OrderByDescending(h => h.IslemTarihi)
            .ToListAsync();
    }

    public async Task<List<BankaKasaHareket>> GetByTipAsync(HareketTipi tip)
    {
        return await _context.BankaKasaHareketleri
            .Include(h => h.BankaHesap)
            .Include(h => h.Cari)
            .Where(h => h.HareketTipi == tip)
            .OrderByDescending(h => h.IslemTarihi)
            .ToListAsync();
    }

    public async Task<List<BankaKasaHareket>> GetEslestirmeyeUygunHareketlerAsync(int cariId, HareketTipi tip)
    {
        // Tamamen eşleştirilmemiş hareketleri getir
        var hareketler = await _context.BankaKasaHareketleri
            .Include(h => h.BankaHesap)
            .Include(h => h.OdemeEslestirmeleri)
            .Where(h => h.CariId == cariId && h.HareketTipi == tip)
            .OrderByDescending(h => h.IslemTarihi)
            .ToListAsync();

        // Henüz tam eşleştirilmemiş olanları filtrele
        return hareketler
            .Where(h => h.Tutar > h.OdemeEslestirmeleri.Sum(e => e.EslestirilenTutar))
            .ToList();
    }

    public async Task<BankaKasaHareket?> GetByIdAsync(int id)
    {
        return await _context.BankaKasaHareketleri
            .Include(h => h.BankaHesap)
            .Include(h => h.Cari)
            .Include(h => h.OdemeEslestirmeleri)
                .ThenInclude(e => e.Fatura)
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<BankaKasaHareket> CreateAsync(BankaKasaHareket hareket)
    {
        _context.BankaKasaHareketleri.Add(hareket);
        await _context.SaveChangesAsync();
        return hareket;
    }

    public async Task<BankaKasaHareket> UpdateAsync(BankaKasaHareket hareket)
    {
        _context.BankaKasaHareketleri.Update(hareket);
        await _context.SaveChangesAsync();
        return hareket;
    }

    public async Task DeleteAsync(int id)
    {
        var hareket = await _context.BankaKasaHareketleri.FindAsync(id);
        if (hareket != null)
        {
            hareket.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateNextIslemNoAsync()
    {
        var today = DateTime.Today;
        var prefix = $"HRK-{today:yyyyMMdd}";

        var lastHareket = await _context.BankaKasaHareketleri
            .IgnoreQueryFilters()
            .Where(h => h.IslemNo.StartsWith(prefix))
            .OrderByDescending(h => h.IslemNo)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (lastHareket != null)
        {
            var parts = lastHareket.IslemNo.Split('-');
            if (parts.Length >= 3 && int.TryParse(parts[2], out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}-{nextNumber:D4}";
    }

    // BankaHesap (Kasa/Banka) işlemleri
    public async Task<List<BankaHesap>> GetHesaplarAsync()
    {
        var hesaplar = await _context.BankaHesaplari
            .Include(h => h.Hareketler)
            .OrderBy(h => h.HesapTipi)
            .ThenBy(h => h.HesapAdi)
            .ToListAsync();

        return hesaplar;
    }

    public async Task<List<BankaHesap>> GetAktifHesaplarAsync()
    {
        return await _context.BankaHesaplari
            .Include(h => h.Hareketler)
            .Where(h => h.Aktif)
            .OrderBy(h => h.HesapTipi)
            .ThenBy(h => h.HesapAdi)
            .ToListAsync();
    }

    public async Task<BankaHesap?> GetHesapByIdAsync(int id)
    {
        return await _context.BankaHesaplari
            .Include(h => h.Hareketler)
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<BankaHesap> CreateHesapAsync(BankaHesap hesap)
    {
        _context.BankaHesaplari.Add(hesap);
        await _context.SaveChangesAsync();
        return hesap;
    }

    public async Task<BankaHesap> UpdateHesapAsync(BankaHesap hesap)
    {
        _context.BankaHesaplari.Update(hesap);
        await _context.SaveChangesAsync();
        return hesap;
    }

    public async Task DeleteHesapAsync(int id)
    {
        var hesap = await _context.BankaHesaplari.FindAsync(id);
        if (hesap != null)
        {
            hesap.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<DashboardBankaStats> GetDashboardStatsAsync()
    {
        // Calculate balances using a single optimized query
        var hesapBakiyeleri = await _context.BankaHesaplari
            .Where(h => !h.IsDeleted)
            .Select(h => new
            {
                h.HesapTipi,
                h.AcilisBakiye,
                Girisler = _context.BankaKasaHareketleri
                    .Where(hr => hr.BankaHesapId == h.Id && !hr.IsDeleted && hr.HareketTipi == HareketTipi.Giris)
                    .Sum(hr => (decimal?)hr.Tutar) ?? 0,
                Cikislar = _context.BankaKasaHareketleri
                    .Where(hr => hr.BankaHesapId == h.Id && !hr.IsDeleted && hr.HareketTipi == HareketTipi.Cikis)
                    .Sum(hr => (decimal?)hr.Tutar) ?? 0
            })
            .ToListAsync();

        var stats = new DashboardBankaStats
        {
            ToplamKasa = hesapBakiyeleri
                .Where(h => h.HesapTipi == HesapTipi.Kasa)
                .Sum(h => h.AcilisBakiye + h.Girisler - h.Cikislar),
            ToplamBanka = hesapBakiyeleri
                .Where(h => h.HesapTipi != HesapTipi.Kasa)
                .Sum(h => h.AcilisBakiye + h.Girisler - h.Cikislar)
        };

        return stats;
    }

    // Mahsup İşlemleri
    public async Task<MahsupSonuc> HesaplarArasiTransferAsync(int kaynakHesapId, int hedefHesapId, decimal tutar, DateTime tarih, string aciklama)
    {
        if (kaynakHesapId == hedefHesapId)
            return new MahsupSonuc { Basarili = false, Hata = "Kaynak ve hedef hesap aynı olamaz." };

        if (tutar <= 0)
            return new MahsupSonuc { Basarili = false, Hata = "Tutar sıfırdan büyük olmalıdır." };

        var kaynakHesap = await _context.BankaHesaplari.FindAsync(kaynakHesapId);
        var hedefHesap = await _context.BankaHesaplari.FindAsync(hedefHesapId);

        if (kaynakHesap == null || hedefHesap == null)
            return new MahsupSonuc { Basarili = false, Hata = "Hesap bulunamadı." };

        var bakiye = await GetHesapBakiyeAsync(kaynakHesapId);
        if (bakiye < tutar)
            return new MahsupSonuc { Basarili = false, Hata = $"Yetersiz bakiye. Mevcut: {bakiye:N2} ₺" };

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var mahsupGrupId = Guid.NewGuid();
            var islemNo = await GenerateNextIslemNoAsync();

            // Kaynak hesaptan çıkış
            var cikisHareket = new BankaKasaHareket
            {
                IslemNo = islemNo,
                IslemTarihi = tarih,
                HareketTipi = HareketTipi.Cikis,
                Tutar = tutar,
                BankaHesapId = kaynakHesapId,
                Aciklama = $"[TRANSFER] {hedefHesap.HesapAdi}'na transfer - {aciklama}",
                IslemKaynak = IslemKaynak.Mahsup,
                MahsupGrupId = mahsupGrupId
            };
            _context.BankaKasaHareketleri.Add(cikisHareket);
            await _context.SaveChangesAsync();

            // Hedef hesaba giriş
            var girisHareket = new BankaKasaHareket
            {
                IslemNo = await GenerateNextIslemNoAsync(),
                IslemTarihi = tarih,
                HareketTipi = HareketTipi.Giris,
                Tutar = tutar,
                BankaHesapId = hedefHesapId,
                Aciklama = $"[TRANSFER] {kaynakHesap.HesapAdi}'ndan transfer - {aciklama}",
                IslemKaynak = IslemKaynak.Mahsup,
                MahsupGrupId = mahsupGrupId,
                MahsupHareketId = cikisHareket.Id
            };
            _context.BankaKasaHareketleri.Add(girisHareket);
            await _context.SaveChangesAsync();

            // Çıkış hareketine de karşı hareket ID'si ekle
            cikisHareket.MahsupHareketId = girisHareket.Id;
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return new MahsupSonuc
            {
                Basarili = true,
                MahsupGrupId = mahsupGrupId,
                KaynakHareket = cikisHareket,
                HedefHareket = girisHareket
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new MahsupSonuc { Basarili = false, Hata = ex.Message };
        }
    }

    public async Task<MahsupSonuc> CariMahsupAsync(int cariId, int hesapId, decimal tutar, DateTime tarih, string aciklama, bool caridenHesaba)
    {
        if (tutar <= 0)
            return new MahsupSonuc { Basarili = false, Hata = "Tutar sıfırdan büyük olmalıdır." };

        var cari = await _context.Cariler.FindAsync(cariId);
        var hesap = await _context.BankaHesaplari.FindAsync(hesapId);

        if (cari == null || hesap == null)
            return new MahsupSonuc { Basarili = false, Hata = "Cari veya hesap bulunamadı." };

        try
        {
            var mahsupGrupId = Guid.NewGuid();
            var islemNo = await GenerateNextIslemNoAsync();

            // caridenHesaba = true: Cari bize borçlu, biz tahsil ediyoruz (Hesaba Giriş)
            // caridenHesaba = false: Biz cariye borçluyuz, ödeme yapıyoruz (Hesaptan Çıkış)
            var hareket = new BankaKasaHareket
            {
                IslemNo = islemNo,
                IslemTarihi = tarih,
                HareketTipi = caridenHesaba ? HareketTipi.Giris : HareketTipi.Cikis,
                Tutar = tutar,
                BankaHesapId = hesapId,
                CariId = cariId,
                Aciklama = $"[CARİ MAHSUP] {cari.Unvan} - {aciklama}",
                IslemKaynak = IslemKaynak.CariMahsup,
                MahsupGrupId = mahsupGrupId
            };

            _context.BankaKasaHareketleri.Add(hareket);
            await _context.SaveChangesAsync();

            return new MahsupSonuc
            {
                Basarili = true,
                MahsupGrupId = mahsupGrupId,
                KaynakHareket = hareket
            };
        }
        catch (Exception ex)
        {
            return new MahsupSonuc { Basarili = false, Hata = ex.Message };
        }
    }

    public async Task<List<BankaKasaHareket>> GetMahsupHareketleriAsync(DateTime? baslangic = null, DateTime? bitis = null)
    {
        var query = _context.BankaKasaHareketleri
            .Include(h => h.BankaHesap)
            .Include(h => h.Cari)
            .Where(h => !h.IsDeleted && (h.IslemKaynak == IslemKaynak.Mahsup || h.IslemKaynak == IslemKaynak.CariMahsup));

        if (baslangic.HasValue)
            query = query.Where(h => h.IslemTarihi >= baslangic.Value);

        if (bitis.HasValue)
            query = query.Where(h => h.IslemTarihi <= bitis.Value);

        return await query.OrderByDescending(h => h.IslemTarihi).ToListAsync();
    }

    public async Task MahsupIptalAsync(Guid mahsupGrupId)
    {
        var hareketler = await _context.BankaKasaHareketleri
            .Where(h => h.MahsupGrupId == mahsupGrupId)
            .ToListAsync();

        foreach (var hareket in hareketler)
        {
            hareket.IsDeleted = true;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<decimal> GetHesapBakiyeAsync(int hesapId)
    {
        var hesap = await _context.BankaHesaplari.FindAsync(hesapId);
        if (hesap == null) return 0;

        var girisler = await _context.BankaKasaHareketleri
            .Where(h => h.BankaHesapId == hesapId && !h.IsDeleted && h.HareketTipi == HareketTipi.Giris)
            .SumAsync(h => (decimal?)h.Tutar) ?? 0;

        var cikislar = await _context.BankaKasaHareketleri
            .Where(h => h.BankaHesapId == hesapId && !h.IsDeleted && h.HareketTipi == HareketTipi.Cikis)
            .SumAsync(h => (decimal?)h.Tutar) ?? 0;

        return hesap.AcilisBakiye + girisler - cikislar;
    }

    public async Task<Dictionary<int, decimal>> GetTumHesapBakiyeleriAsync()
    {
        var hesaplar = await _context.BankaHesaplari
            .Where(h => !h.IsDeleted && h.Aktif)
            .Select(h => new
            {
                h.Id,
                h.AcilisBakiye,
                Girisler = _context.BankaKasaHareketleri
                    .Where(hr => hr.BankaHesapId == h.Id && !hr.IsDeleted && hr.HareketTipi == HareketTipi.Giris)
                    .Sum(hr => (decimal?)hr.Tutar) ?? 0,
                Cikislar = _context.BankaKasaHareketleri
                    .Where(hr => hr.BankaHesapId == h.Id && !hr.IsDeleted && hr.HareketTipi == HareketTipi.Cikis)
                    .Sum(hr => (decimal?)hr.Tutar) ?? 0
            })
            .ToListAsync();

        return hesaplar.ToDictionary(h => h.Id, h => h.AcilisBakiye + h.Girisler - h.Cikislar);
    }
}
