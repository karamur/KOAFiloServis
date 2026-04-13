using KOAFiloServis.Shared.Entities;
using KOAFiloServis.Web.Data;
using KOAFiloServis.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace KOAFiloServis.Web.Services;

public class BankaKasaHareketService : IBankaKasaHareketService
{
    private const string IslemNoPrefix = "HRK-";
    private readonly ApplicationDbContext _context;
    private readonly IMuhasebeService _muhasebeService;
    private readonly IBankaHesapService _bankaHesapService;

    public BankaKasaHareketService(ApplicationDbContext context, IMuhasebeService muhasebeService, IBankaHesapService bankaHesapService)
    {
        _context = context;
        _muhasebeService = muhasebeService;
        _bankaHesapService = bankaHesapService;
    }

    public async Task<List<BankaKasaHareket>> GetAllAsync()
    {
        return await QueryHareketler()
            .Include(h => h.BankaHesap)
            .Include(h => h.Cari)
            .OrderByDescending(h => h.IslemTarihi)
            .ToListAsync();
    }

    public async Task<PagedResult<BankaKasaHareket>> GetPagedAsync(BankaHareketFilterParams filter)
    {
        var query = QueryHareketler()
            .Include(h => h.BankaHesap)
            .Include(h => h.Cari)
            .AsQueryable();

        // Arama filtresi
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchLower = filter.SearchTerm.ToLower();
            query = query.Where(h =>
                (h.IslemNo != null && h.IslemNo.ToLower().Contains(searchLower)) ||
                (h.Aciklama != null && h.Aciklama.ToLower().Contains(searchLower)) ||
                (h.BelgeNo != null && h.BelgeNo.ToLower().Contains(searchLower)) ||
                (h.BankaHesap != null && h.BankaHesap.HesapAdi.ToLower().Contains(searchLower)) ||
                (h.Cari != null && h.Cari.Unvan.ToLower().Contains(searchLower)));
        }

        // Hesap filtresi
        if (filter.HesapId.HasValue && filter.HesapId.Value > 0)
        {
            query = query.Where(h => h.BankaHesapId == filter.HesapId.Value);
        }

        // Cari filtresi
        if (filter.CariId.HasValue && filter.CariId.Value > 0)
        {
            query = query.Where(h => h.CariId == filter.CariId.Value);
        }

        // Hareket tipi filtresi
        if (filter.HareketTipi.HasValue)
        {
            query = query.Where(h => h.HareketTipi == filter.HareketTipi.Value);
        }

        // Tarih aralığı filtresi
        if (filter.BaslangicTarihi.HasValue)
        {
            query = query.Where(h => h.IslemTarihi >= filter.BaslangicTarihi.Value);
        }

        if (filter.BitisTarihi.HasValue)
        {
            query = query.Where(h => h.IslemTarihi <= filter.BitisTarihi.Value);
        }

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderByDescending(h => h.IslemTarihi)
            .ThenByDescending(h => h.Id)
            .Skip(filter.Skip)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<BankaKasaHareket>(items, totalItems, filter.PageNumber, filter.PageSize);
    }

    public async Task<List<BankaKasaHareket>> GetRecentAsync(int count = 5)
    {
        return await QueryHareketler()
            .Include(h => h.BankaHesap)
            .Include(h => h.Cari)
            .OrderByDescending(h => h.IslemTarihi)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<BankaKasaHareket>> GetByHesapIdAsync(int hesapId)
    {
        return await QueryHareketler()
            .Include(h => h.Cari)
            .Where(h => h.BankaHesapId == hesapId)
            .OrderByDescending(h => h.IslemTarihi)
            .ToListAsync();
    }

    public async Task<List<BankaKasaHareket>> GetByCariIdAsync(int cariId)
    {
        return await QueryHareketler()
            .Include(h => h.BankaHesap)
            .Where(h => h.CariId == cariId)
            .OrderByDescending(h => h.IslemTarihi)
            .ToListAsync();
    }

    public async Task<List<BankaKasaHareket>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await QueryHareketler()
            .Include(h => h.BankaHesap)
            .Include(h => h.Cari)
            .Where(h => h.IslemTarihi >= startDate && h.IslemTarihi <= endDate)
            .OrderByDescending(h => h.IslemTarihi)
            .ToListAsync();
    }

    public async Task<List<BankaKasaHareket>> GetByTipAsync(HareketTipi tip)
    {
        return await QueryHareketler()
            .Include(h => h.BankaHesap)
            .Include(h => h.Cari)
            .Where(h => h.HareketTipi == tip)
            .OrderByDescending(h => h.IslemTarihi)
            .ToListAsync();
    }

    public async Task<List<BankaKasaHareket>> GetEslestirmeyeUygunHareketlerAsync(int cariId, HareketTipi tip)
    {
        // Tamamen eşleştirilmemiş hareketleri getir
        var hareketler = await QueryHareketler()
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
        return await QueryHareketler()
            .Include(h => h.BankaHesap)
            .Include(h => h.Cari)
            .Include(h => h.OdemeEslestirmeleri)
                .ThenInclude(e => e.Fatura)
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<BankaKasaHareket> CreateAsync(BankaKasaHareket hareket)
    {
        NormalizeHareket(hareket);
        await ValidateHareketAsync(hareket);
        await ApplyMuhasebeDefaultsAsync(hareket);
        hareket.BankaHesap = null!;
        hareket.Cari = null;
        _context.BankaKasaHareketleri.Add(hareket);
        await _context.SaveChangesAsync();
        return hareket;
    }

    public async Task<BankaKasaHareket> UpdateAsync(BankaKasaHareket hareket)
    {
        NormalizeHareket(hareket);
        await ValidateHareketAsync(hareket);
        await ApplyMuhasebeDefaultsAsync(hareket);

        var existing = await QueryHareketler(asNoTracking: false)
            .FirstOrDefaultAsync(h => h.Id == hareket.Id);
        if (existing == null)
            throw new InvalidOperationException($"Banka/Kasa hareketi bulunamadı. Id: {hareket.Id}");

        existing.IslemNo = hareket.IslemNo;
        existing.IslemTarihi = hareket.IslemTarihi;
        existing.HareketTipi = hareket.HareketTipi;
        existing.Tutar = hareket.Tutar;
        existing.Aciklama = hareket.Aciklama;
        existing.BelgeNo = hareket.BelgeNo;
        existing.IslemKaynak = hareket.IslemKaynak;
        existing.MahsupHareketId = hareket.MahsupHareketId;
        existing.MahsupGrupId = hareket.MahsupGrupId;
        existing.MuhasebeHesapKodu = hareket.MuhasebeHesapKodu;
        existing.MuhasebeAltHesapKodu = hareket.MuhasebeAltHesapKodu;
        existing.KostMerkeziKodu = hareket.KostMerkeziKodu;
        existing.ProjeKodu = hareket.ProjeKodu;
        existing.MuhasebeAciklama = hareket.MuhasebeAciklama;
        existing.BankaHesapId = hareket.BankaHesapId;
        existing.CariId = hareket.CariId;
        existing.IsDeleted = hareket.IsDeleted;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    private async Task ApplyMuhasebeDefaultsAsync(BankaKasaHareket hareket)
    {
        if (string.IsNullOrWhiteSpace(hareket.MuhasebeAciklama) && !string.IsNullOrWhiteSpace(hareket.Aciklama))
        {
            hareket.MuhasebeAciklama = hareket.Aciklama;
        }

        if (hareket.BankaHesapId == 0)
            return;

        var hesap = await _bankaHesapService.GetByIdAsync(hareket.BankaHesapId);

        if (hesap == null)
            return;

        if (string.IsNullOrWhiteSpace(hareket.MuhasebeHesapKodu))
            hareket.MuhasebeHesapKodu = hesap.VarsayilanMuhasebeKodu;

        if (string.IsNullOrWhiteSpace(hareket.KostMerkeziKodu))
            hareket.KostMerkeziKodu = hesap.VarsayilanKostMerkezi;
    }

    private IQueryable<BankaKasaHareket> QueryHareketler(bool asNoTracking = true)
    {
        var query = _context.BankaKasaHareketleri
            .Where(h => !h.IsDeleted);

        return asNoTracking ? query.AsNoTracking() : query;
    }

    private IQueryable<Cari> QueryCariler(bool asNoTracking = true)
    {
        var query = _context.Cariler
            .Where(c => !c.IsDeleted);

        return asNoTracking ? query.AsNoTracking() : query;
    }

    private async Task ValidateHareketAsync(BankaKasaHareket hareket)
    {
        if (string.IsNullOrWhiteSpace(hareket.IslemNo))
            throw new InvalidOperationException("İşlem no zorunludur.");

        if (hareket.IslemTarihi == default)
            throw new InvalidOperationException("İşlem tarihi zorunludur.");

        if (hareket.BankaHesapId <= 0)
            throw new InvalidOperationException("Geçerli bir hesap seçiniz.");

        if (hareket.Tutar <= 0)
            throw new InvalidOperationException("Tutar sıfırdan büyük olmalıdır.");

        var islemNoKullanimda = await QueryHareketler()
            .AnyAsync(h => h.Id != hareket.Id && h.IslemNo == hareket.IslemNo);

        if (islemNoKullanimda)
            throw new InvalidOperationException($"'{hareket.IslemNo}' işlem numarası zaten kullanımda.");

        var hesapVar = await _bankaHesapService.GetByIdAsync(hareket.BankaHesapId);
        if (hesapVar == null)
            throw new InvalidOperationException("Seçilen hesap bulunamadı.");

        if (hareket.CariId.HasValue)
        {
            var cariVar = await QueryCariler()
                .AnyAsync(c => c.Id == hareket.CariId.Value);

            if (!cariVar)
                throw new InvalidOperationException("Seçilen cari bulunamadı.");
        }
    }

    private static void NormalizeHareket(BankaKasaHareket hareket)
    {
        hareket.IslemNo = string.IsNullOrWhiteSpace(hareket.IslemNo)
            ? string.Empty
            : hareket.IslemNo.Trim().ToUpperInvariant();
        hareket.Aciklama = NormalizeNullableText(hareket.Aciklama);
        hareket.BelgeNo = NormalizeNullableText(hareket.BelgeNo);
        hareket.MuhasebeHesapKodu = NormalizeNullableText(hareket.MuhasebeHesapKodu);
        hareket.MuhasebeAltHesapKodu = NormalizeNullableText(hareket.MuhasebeAltHesapKodu);
        hareket.KostMerkeziKodu = NormalizeNullableText(hareket.KostMerkeziKodu);
        hareket.ProjeKodu = NormalizeNullableText(hareket.ProjeKodu);
        hareket.MuhasebeAciklama = NormalizeNullableText(hareket.MuhasebeAciklama);
        hareket.CariId = hareket.CariId <= 0 ? null : hareket.CariId;
        hareket.MahsupHareketId = hareket.MahsupHareketId <= 0 ? null : hareket.MahsupHareketId;
    }

    private static string? NormalizeNullableText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public async Task DeleteAsync(int id)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var hareket = await QueryHareketler(asNoTracking: false)
                .Include(h => h.OdemeEslestirmeleri)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hareket == null)
            {
                await transaction.CommitAsync();
                return;
            }

            // İlişkili bütçe ödemesini bul ve durumunu geri al
            var iliskiliOdeme = await _context.BudgetOdemeler
                .FirstOrDefaultAsync(o => o.BankaKasaHareketId == id);

            if (iliskiliOdeme != null)
            {
                // Ödeme durumunu geri al
                iliskiliOdeme.Durum = OdemeDurum.Bekliyor;
                iliskiliOdeme.GercekOdemeTarihi = null;
                iliskiliOdeme.OdenenTutar = null;
                iliskiliOdeme.BankaKasaHareketId = null;
                iliskiliOdeme.OdemeYapildigiHesapId = null;
                iliskiliOdeme.OdemeNotu = null;
                iliskiliOdeme.MasrafKesintisi = 0;
                iliskiliOdeme.CezaKesintisi = 0;
                iliskiliOdeme.DigerKesinti = 0;
                iliskiliOdeme.KesintiAciklamasi = null;
                iliskiliOdeme.UpdatedAt = DateTime.UtcNow;
            }

            if (hareket.OdemeEslestirmeleri.Any())
            {
                _context.OdemeEslestirmeleri.RemoveRange(hareket.OdemeEslestirmeleri);
            }

            _context.BankaKasaHareketleri.Remove(hareket);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        });
    }

    public async Task<string> GenerateNextIslemNoAsync()
    {
        var today = DateTime.Today;
        var prefix = $"{IslemNoPrefix}{today:yyyyMMdd}";

        var islemNolari = await _context.BankaKasaHareketleri
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(h => h.IslemNo.StartsWith(prefix))
            .Select(h => h.IslemNo)
            .ToListAsync();

        var nextNumber = islemNolari
            .Select(TryParseIslemNoSequence)
            .Where(number => number.HasValue)
            .Select(number => number!.Value)
            .DefaultIfEmpty(0)
            .Max() + 1;

        return $"{prefix}-{nextNumber:D4}";
    }

    // BankaHesap (Kasa/Banka) işlemleri
    public async Task<List<BankaHesap>> GetHesaplarAsync()
    {
        return await _bankaHesapService.GetAllAsync();
    }

    public async Task<List<BankaHesap>> GetAktifHesaplarAsync()
    {
        return await _bankaHesapService.GetActiveAsync();
    }

    public async Task<BankaHesap?> GetHesapByIdAsync(int id)
    {
        return await _bankaHesapService.GetByIdAsync(id);
    }

    public async Task<BankaHesap> CreateHesapAsync(BankaHesap hesap)
    {
        return await _bankaHesapService.CreateAsync(hesap);
    }

    public async Task<BankaHesap> UpdateHesapAsync(BankaHesap hesap)
    {
        return await _bankaHesapService.UpdateAsync(hesap);
    }

    public async Task DeleteHesapAsync(int id)
    {
        await _bankaHesapService.DeleteAsync(id);
    }

    public async Task<DashboardBankaStats> GetDashboardStatsAsync()
    {
        // Calculate balances using a single optimized query
        var hesapBakiyeleri = await _context.BankaHesaplari
            .AsNoTracking()
            .Where(h => !h.IsDeleted)
            .Select(h => new
            {
                h.HesapTipi,
                h.AcilisBakiye,
                Girisler = QueryHareketler()
                    .Where(hr => hr.BankaHesapId == h.Id && hr.HareketTipi == HareketTipi.Giris)
                    .Sum(hr => (decimal?)hr.Tutar) ?? 0,
                Cikislar = QueryHareketler()
                    .Where(hr => hr.BankaHesapId == h.Id && hr.HareketTipi == HareketTipi.Cikis)
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
    public async Task<MahsupSonuc> HesaplarArasiTransferAsync(int kaynakHesapId, int hedefHesapId, decimal tutar, DateTime tarih, string aciklama, string? belgeNo = null, string? muhasebeHesapKodu = null, string? kostMerkeziKodu = null, string? projeKodu = null)
    {
        if (kaynakHesapId == hedefHesapId)
            return new MahsupSonuc { Basarili = false, Hata = "Kaynak ve hedef hesap aynı olamaz." };

        if (tutar <= 0)
            return new MahsupSonuc { Basarili = false, Hata = "Tutar sıfırdan büyük olmalıdır." };

        var kaynakHesap = await _bankaHesapService.GetByIdAsync(kaynakHesapId);
        var hedefHesap = await _bankaHesapService.GetByIdAsync(hedefHesapId);

        if (kaynakHesap == null || hedefHesap == null)
            return new MahsupSonuc { Basarili = false, Hata = "Hesap bulunamadı." };

        if (!kaynakHesap.Aktif || !hedefHesap.Aktif)
            return new MahsupSonuc { Basarili = false, Hata = "Transfer için hesapların aktif olması gerekir." };

        var bakiye = await GetHesapBakiyeAsync(kaynakHesapId);
        if (bakiye < tutar)
            return new MahsupSonuc { Basarili = false, Hata = $"Yetersiz bakiye. Mevcut: {bakiye:N2} ₺" };

        // ExecutionStrategy ile transaction sarmalama (NpgsqlRetryingExecutionStrategy uyumluluğu)
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
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
                BelgeNo = string.IsNullOrWhiteSpace(belgeNo) ? null : belgeNo.Trim(),
                Aciklama = $"[TRANSFER] {hedefHesap.HesapAdi}'na transfer - {aciklama}",
                MuhasebeHesapKodu = string.IsNullOrWhiteSpace(muhasebeHesapKodu) ? kaynakHesap.VarsayilanMuhasebeKodu : muhasebeHesapKodu.Trim(),
                KostMerkeziKodu = string.IsNullOrWhiteSpace(kostMerkeziKodu) ? kaynakHesap.VarsayilanKostMerkezi : kostMerkeziKodu.Trim(),
                ProjeKodu = string.IsNullOrWhiteSpace(projeKodu) ? null : projeKodu.Trim(),
                MuhasebeAciklama = string.IsNullOrWhiteSpace(aciklama) ? null : aciklama.Trim(),
                IslemKaynak = IslemKaynak.Mahsup,
                MahsupGrupId = mahsupGrupId
            };
            NormalizeHareket(cikisHareket);
            await ValidateHareketAsync(cikisHareket);
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
                BelgeNo = string.IsNullOrWhiteSpace(belgeNo) ? null : belgeNo.Trim(),
                Aciklama = $"[TRANSFER] {kaynakHesap.HesapAdi}'ndan transfer - {aciklama}",
                MuhasebeHesapKodu = string.IsNullOrWhiteSpace(muhasebeHesapKodu) ? hedefHesap.VarsayilanMuhasebeKodu : muhasebeHesapKodu.Trim(),
                KostMerkeziKodu = string.IsNullOrWhiteSpace(kostMerkeziKodu) ? hedefHesap.VarsayilanKostMerkezi : kostMerkeziKodu.Trim(),
                ProjeKodu = string.IsNullOrWhiteSpace(projeKodu) ? null : projeKodu.Trim(),
                MuhasebeAciklama = string.IsNullOrWhiteSpace(aciklama) ? null : aciklama.Trim(),
                IslemKaynak = IslemKaynak.Mahsup,
                MahsupGrupId = mahsupGrupId,
                MahsupHareketId = cikisHareket.Id
            };
            NormalizeHareket(girisHareket);
            await ValidateHareketAsync(girisHareket);
            _context.BankaKasaHareketleri.Add(girisHareket);
            await _context.SaveChangesAsync();

            // Çıkış hareketine de karşı hareket ID'si ekle
            cikisHareket.MahsupHareketId = girisHareket.Id;
            await _context.SaveChangesAsync();

            // Muhasebe fişi oluştur
            try
            {
                await _muhasebeService.CreateHesapTransferFisiAsync(cikisHareket, girisHareket, kaynakHesap, hedefHesap);
            }
            catch
            {
                // Muhasebe entegrasyonu başarısız olsa bile işlem devam eder
                // Loglama eklenebilir
            }

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
        }); // ExecutionStrategy lambda sonu
    }

    public async Task<MahsupSonuc> CariMahsupAsync(int cariId, int hesapId, decimal tutar, DateTime tarih, string aciklama, bool caridenHesaba, string? belgeNo = null, string? muhasebeHesapKodu = null, string? kostMerkeziKodu = null, string? projeKodu = null)
    {
        if (tutar <= 0)
            return new MahsupSonuc { Basarili = false, Hata = "Tutar sıfırdan büyük olmalıdır." };

        var cari = await QueryCariler()
            .FirstOrDefaultAsync(c => c.Id == cariId);
        var hesap = await _bankaHesapService.GetByIdAsync(hesapId);

        if (cari == null || hesap == null)
            return new MahsupSonuc { Basarili = false, Hata = "Cari veya hesap bulunamadı." };

        if (!hesap.Aktif)
            return new MahsupSonuc { Basarili = false, Hata = "Cari mahsup için hesabın aktif olması gerekir." };

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

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
                    BelgeNo = string.IsNullOrWhiteSpace(belgeNo) ? null : belgeNo.Trim(),
                    Aciklama = $"[CARİ MAHSUP] {cari.Unvan} - {aciklama}",
                    MuhasebeHesapKodu = string.IsNullOrWhiteSpace(muhasebeHesapKodu) ? hesap.VarsayilanMuhasebeKodu : muhasebeHesapKodu.Trim(),
                    KostMerkeziKodu = string.IsNullOrWhiteSpace(kostMerkeziKodu) ? hesap.VarsayilanKostMerkezi : kostMerkeziKodu.Trim(),
                    ProjeKodu = string.IsNullOrWhiteSpace(projeKodu) ? null : projeKodu.Trim(),
                    MuhasebeAciklama = string.IsNullOrWhiteSpace(aciklama) ? null : aciklama.Trim(),
                    IslemKaynak = IslemKaynak.CariMahsup,
                    MahsupGrupId = mahsupGrupId
                };

                NormalizeHareket(hareket);
                await ValidateHareketAsync(hareket);
                _context.BankaKasaHareketleri.Add(hareket);
                await _context.SaveChangesAsync();

                try
                {
                    await _muhasebeService.CreateCariMahsupFisiAsync(hareket, cari, hesap, caridenHesaba);
                }
                catch
                {
                }

                await transaction.CommitAsync();

                return new MahsupSonuc
                {
                    Basarili = true,
                    MahsupGrupId = mahsupGrupId,
                    KaynakHareket = hareket
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new MahsupSonuc { Basarili = false, Hata = ex.Message };
            }
        });
    }

    public async Task<List<BankaKasaHareket>> GetMahsupHareketleriAsync(DateTime? baslangic = null, DateTime? bitis = null)
    {
        var query = QueryHareketler()
            .Include(h => h.BankaHesap)
            .Include(h => h.Cari)
            .Where(h => h.IslemKaynak == IslemKaynak.Mahsup || h.IslemKaynak == IslemKaynak.CariMahsup);

        if (baslangic.HasValue)
            query = query.Where(h => h.IslemTarihi >= baslangic.Value);

        if (bitis.HasValue)
            query = query.Where(h => h.IslemTarihi <= bitis.Value);

        var hareketler = await query
            .OrderByDescending(h => h.IslemTarihi)
            .ToListAsync();

        if (!hareketler.Any())
            return hareketler;

        var hareketIds = hareketler.Select(h => h.Id).ToHashSet();
        var mahsupGrupIds = hareketler.Where(h => h.MahsupGrupId.HasValue).Select(h => h.MahsupGrupId!.Value).ToHashSet();

        var fisler = await _context.MuhasebeFisleri
            .AsNoTracking()
            .Where(f => (f.KaynakTip == "HesapTransfer" || f.KaynakTip == "CariMahsup") && f.KaynakId.HasValue && hareketIds.Contains(f.KaynakId.Value))
            .Select(f => new { f.Id, f.FisNo, f.Durum, f.KaynakId, f.KaynakTip })
            .ToListAsync();

        var iptalFisleri = await _context.MuhasebeFisleri
            .AsNoTracking()
            .Where(f => f.KaynakTip == "IptalKaydi" && f.KaynakId.HasValue)
            .Select(f => new { f.FisNo, f.KaynakId })
            .ToListAsync();

        var fisByKaynakId = fisler
            .Where(f => f.KaynakId.HasValue)
            .ToDictionary(f => f.KaynakId!.Value, f => f);

        var iptalFisNoByFisId = iptalFisleri
            .Where(f => f.KaynakId.HasValue)
            .GroupBy(f => f.KaynakId!.Value)
            .ToDictionary(g => g.Key, g => g.Select(x => x.FisNo).FirstOrDefault());

        foreach (var hareket in hareketler)
        {
            var kaynakHareketId = hareket.Id;

            if (!fisByKaynakId.TryGetValue(kaynakHareketId, out var fis)
                && hareket.MahsupGrupId.HasValue
                && mahsupGrupIds.Contains(hareket.MahsupGrupId.Value))
            {
                var grupKaynakHareket = hareketler.FirstOrDefault(h => h.MahsupGrupId == hareket.MahsupGrupId && fisByKaynakId.ContainsKey(h.Id));
                if (grupKaynakHareket != null)
                {
                    fis = fisByKaynakId[grupKaynakHareket.Id];
                }
            }

            if (fis == null)
                continue;

            hareket.MuhasebeFisNo = fis.FisNo;
            hareket.MuhasebeFisDurumu = fis.Durum switch
            {
                FisDurum.Onaylandi => "Onaylandı",
                FisDurum.IptalEdildi => "İptal Edildi",
                _ => "Taslak"
            };

            if (iptalFisNoByFisId.TryGetValue(fis.Id, out var iptalFisNo) && !string.IsNullOrWhiteSpace(iptalFisNo))
            {
                hareket.IptalFisNo = iptalFisNo;
            }
        }

        return hareketler;
    }

    public async Task MahsupIptalAsync(Guid mahsupGrupId)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var hareketler = await QueryHareketler(asNoTracking: false)
                .Include(h => h.OdemeEslestirmeleri)
                .Where(h => h.MahsupGrupId == mahsupGrupId)
                .ToListAsync();

            try
            {
                await _muhasebeService.IptalFisiOlusturAsync(mahsupGrupId);
            }
            catch
            {
            }

            var eslestirmeler = hareketler.SelectMany(h => h.OdemeEslestirmeleri).ToList();
            if (eslestirmeler.Any())
            {
                _context.OdemeEslestirmeleri.RemoveRange(eslestirmeler);
            }

            _context.BankaKasaHareketleri.RemoveRange(hareketler);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        });
    }

    public async Task<decimal> GetHesapBakiyeAsync(int hesapId)
    {
        var hesap = await _bankaHesapService.GetByIdAsync(hesapId);
        if (hesap == null) return 0;

        var girisler = await QueryHareketler()
            .Where(h => h.BankaHesapId == hesapId && h.HareketTipi == HareketTipi.Giris)
            .SumAsync(h => (decimal?)h.Tutar) ?? 0;

        var cikislar = await QueryHareketler()
            .Where(h => h.BankaHesapId == hesapId && h.HareketTipi == HareketTipi.Cikis)
            .SumAsync(h => (decimal?)h.Tutar) ?? 0;

        return hesap.AcilisBakiye + girisler - cikislar;
    }

    public async Task<Dictionary<int, decimal>> GetTumHesapBakiyeleriAsync()
    {
        var hesaplar = await _context.BankaHesaplari
            .AsNoTracking()
            .Where(h => !h.IsDeleted && h.Aktif)
            .Select(h => new
            {
                h.Id,
                h.AcilisBakiye,
                Girisler = QueryHareketler()
                    .Where(hr => hr.BankaHesapId == h.Id && hr.HareketTipi == HareketTipi.Giris)
                    .Sum(hr => (decimal?)hr.Tutar) ?? 0,
                Cikislar = QueryHareketler()
                    .Where(hr => hr.BankaHesapId == h.Id && hr.HareketTipi == HareketTipi.Cikis)
                    .Sum(hr => (decimal?)hr.Tutar) ?? 0
            })
            .ToListAsync();

        return hesaplar.ToDictionary(h => h.Id, h => h.AcilisBakiye + h.Girisler - h.Cikislar);
    }

    private static int? TryParseIslemNoSequence(string? islemNo)
    {
        if (string.IsNullOrWhiteSpace(islemNo))
            return null;

        var normalized = islemNo.Trim().ToUpperInvariant();
        var parts = normalized.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3 || parts[0] != IslemNoPrefix.TrimEnd('-'))
            return null;

        return int.TryParse(parts[2], out var sequence) ? sequence : null;
    }
}
