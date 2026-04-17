using KOAFiloServis.Shared.Entities;
using KOAFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace KOAFiloServis.Web.Services;

public class BankaHesapService : IBankaHesapService
{
    private const string HesapKodPrefix = "HSP-";
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public BankaHesapService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<BankaHesap>> GetAllAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await QueryBankaHesaplari(context)
            .OrderBy(b => b.HesapAdi)
            .ToListAsync();
    }

    public async Task<List<BankaHesap>> GetActiveAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await QueryBankaHesaplari(context)
            .Where(b => b.Aktif)
            .OrderBy(b => b.HesapAdi)
            .ToListAsync();
    }

    public async Task<List<BankaHesap>> GetByTipAsync(HesapTipi tip)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await QueryBankaHesaplari(context)
            .Where(b => b.HesapTipi == tip && b.Aktif)
            .OrderBy(b => b.HesapAdi)
            .ToListAsync();
    }

    public async Task<BankaHesap?> GetByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await QueryBankaHesaplari(context)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<BankaHesap> CreateAsync(BankaHesap bankaHesap)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        NormalizeBankaHesap(bankaHesap);
        await ValidateBankaHesapAsync(context, bankaHesap);

        context.BankaHesaplari.Add(bankaHesap);
        await context.SaveChangesAsync();
        return bankaHesap;
    }

    public async Task<BankaHesap> UpdateAsync(BankaHesap bankaHesap)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await QueryBankaHesaplari(context, asNoTracking: false)
            .FirstOrDefaultAsync(b => b.Id == bankaHesap.Id);

        if (existing == null)
            throw new InvalidOperationException($"Banka hesabı bulunamadı. Id: {bankaHesap.Id}");

        NormalizeBankaHesap(bankaHesap);
        await ValidateBankaHesapAsync(context, bankaHesap);

        existing.HesapKodu = bankaHesap.HesapKodu;
        existing.HesapAdi = bankaHesap.HesapAdi;
        existing.HesapTipi = bankaHesap.HesapTipi;
        existing.BankaAdi = bankaHesap.BankaAdi;
        existing.SubeAdi = bankaHesap.SubeAdi;
        existing.SubeKodu = bankaHesap.SubeKodu;
        existing.HesapNo = bankaHesap.HesapNo;
        existing.Iban = bankaHesap.Iban;
        existing.ParaBirimi = bankaHesap.ParaBirimi;
        existing.AcilisBakiye = bankaHesap.AcilisBakiye;
        existing.Aktif = bankaHesap.Aktif;
        existing.Notlar = bankaHesap.Notlar;
        existing.KrediTaksitGrupId = bankaHesap.KrediTaksitGrupId;
        existing.VarsayilanMuhasebeKodu = bankaHesap.VarsayilanMuhasebeKodu;
        existing.VarsayilanKostMerkezi = bankaHesap.VarsayilanKostMerkezi;
        existing.IsDeleted = bankaHesap.IsDeleted;
        existing.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var bankaHesap = await QueryBankaHesaplari(context, asNoTracking: false)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bankaHesap != null)
        {
            bankaHesap.IsDeleted = true;
            bankaHesap.Aktif = false;
            bankaHesap.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateNextKodAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var hesapKodlari = await context.BankaHesaplari
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Select(b => b.HesapKodu)
            .ToListAsync();

        var nextNumber = hesapKodlari
            .Select(TryParseGeneratedKodNumber)
            .Where(number => number.HasValue)
            .Select(number => number!.Value)
            .DefaultIfEmpty(0)
            .Max() + 1;

        return $"{HesapKodPrefix}{nextNumber:D4}";
    }

    public async Task<decimal> GetBakiyeAsync(int hesapId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var hesap = await QueryBankaHesaplari(context)
            .Where(h => h.Id == hesapId)
            .Select(h => new { h.AcilisBakiye })
            .FirstOrDefaultAsync();

        if (hesap == null) return 0;

        var girisler = await QueryBankaKasaHareketleri(context)
            .Where(h => h.BankaHesapId == hesapId && h.HareketTipi == HareketTipi.Giris)
            .SumAsync(h => h.Tutar);

        var cikislar = await QueryBankaKasaHareketleri(context)
            .Where(h => h.BankaHesapId == hesapId && h.HareketTipi == HareketTipi.Cikis)
            .SumAsync(h => h.Tutar);

        return hesap.AcilisBakiye + girisler - cikislar;
    }

    public async Task<Dictionary<int, decimal>> GetTumHesapBakiyeleriAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var hesaplar = await QueryBankaHesaplari(context)
            .Where(h => h.Aktif)
            .Select(h => new
            {
                h.Id,
                h.AcilisBakiye,
                Girisler = QueryBankaKasaHareketleri(context)
                    .Where(hr => hr.BankaHesapId == h.Id && hr.HareketTipi == HareketTipi.Giris)
                    .Sum(hr => (decimal?)hr.Tutar) ?? 0,
                Cikislar = QueryBankaKasaHareketleri(context)
                    .Where(hr => hr.BankaHesapId == h.Id && hr.HareketTipi == HareketTipi.Cikis)
                    .Sum(hr => (decimal?)hr.Tutar) ?? 0
            })
            .ToListAsync();

        return hesaplar.ToDictionary(h => h.Id, h => h.AcilisBakiye + h.Girisler - h.Cikislar);
    }

    private IQueryable<BankaHesap> QueryBankaHesaplari(ApplicationDbContext context, bool asNoTracking = true)
    {
        var query = context.BankaHesaplari
            .Where(b => !b.IsDeleted);

        return asNoTracking ? query.AsNoTracking() : query;
    }

    private IQueryable<BankaKasaHareket> QueryBankaKasaHareketleri(ApplicationDbContext context)
    {
        return context.BankaKasaHareketleri
            .Where(h => !h.IsDeleted);
    }

    private async Task ValidateBankaHesapAsync(ApplicationDbContext context, BankaHesap bankaHesap)
    {
        if (string.IsNullOrWhiteSpace(bankaHesap.HesapKodu))
            throw new InvalidOperationException("Hesap kodu zorunludur.");

        if (string.IsNullOrWhiteSpace(bankaHesap.HesapAdi))
            throw new InvalidOperationException("Hesap adı zorunludur.");

        var hesapKoduVar = await QueryBankaHesaplari(context)
            .AnyAsync(b => b.Id != bankaHesap.Id && b.HesapKodu == bankaHesap.HesapKodu);

        if (hesapKoduVar)
            throw new InvalidOperationException($"'{bankaHesap.HesapKodu}' hesap kodu zaten kullanımda.");

        if (string.IsNullOrWhiteSpace(bankaHesap.Iban))
            return;

        if (!IsValidIban(bankaHesap.Iban))
            throw new InvalidOperationException("Geçerli bir IBAN giriniz.");

        var ibanVar = await QueryBankaHesaplari(context)
            .AnyAsync(b => b.Id != bankaHesap.Id && b.Iban == bankaHesap.Iban);

        if (ibanVar)
            throw new InvalidOperationException($"'{bankaHesap.Iban}' IBAN bilgisi başka bir hesapta tanımlı.");
    }

    private static void NormalizeBankaHesap(BankaHesap bankaHesap)
    {
        bankaHesap.HesapKodu = bankaHesap.HesapKodu.Trim().ToUpperInvariant();
        bankaHesap.HesapAdi = bankaHesap.HesapAdi.Trim();
        bankaHesap.BankaAdi = NormalizeNullableText(bankaHesap.BankaAdi);
        bankaHesap.SubeAdi = NormalizeNullableText(bankaHesap.SubeAdi);
        bankaHesap.SubeKodu = NormalizeNullableText(bankaHesap.SubeKodu);
        bankaHesap.HesapNo = NormalizeNullableText(bankaHesap.HesapNo);
        bankaHesap.Iban = NormalizeIban(bankaHesap.Iban);
        bankaHesap.ParaBirimi = string.IsNullOrWhiteSpace(bankaHesap.ParaBirimi)
            ? "TRY"
            : bankaHesap.ParaBirimi.Trim().ToUpperInvariant();
        bankaHesap.Notlar = NormalizeNullableText(bankaHesap.Notlar);
        bankaHesap.VarsayilanMuhasebeKodu = NormalizeNullableText(bankaHesap.VarsayilanMuhasebeKodu);
        bankaHesap.VarsayilanKostMerkezi = NormalizeNullableText(bankaHesap.VarsayilanKostMerkezi);
    }

    private static string? NormalizeNullableText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? NormalizeIban(string? iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
            return null;

        return new string(iban.Where(ch => !char.IsWhiteSpace(ch)).ToArray()).ToUpperInvariant();
    }

    private static bool IsValidIban(string iban)
    {
        var normalizedIban = NormalizeIban(iban);
        if (string.IsNullOrWhiteSpace(normalizedIban))
            return false;

        if (normalizedIban.Length is < 15 or > 34)
            return false;

        if (!char.IsLetter(normalizedIban[0]) || !char.IsLetter(normalizedIban[1]) ||
            !char.IsDigit(normalizedIban[2]) || !char.IsDigit(normalizedIban[3]) ||
            normalizedIban.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            return false;
        }

        var rearranged = string.Concat(normalizedIban.AsSpan(4), normalizedIban.AsSpan(0, 4));
        var remainder = 0;

        foreach (var ch in rearranged)
        {
            if (char.IsDigit(ch))
            {
                remainder = (remainder * 10 + (ch - '0')) % 97;
                continue;
            }

            var numericValue = ch - 'A' + 10;
            foreach (var digit in numericValue.ToString())
            {
                remainder = (remainder * 10 + (digit - '0')) % 97;
            }
        }

        return remainder == 1;
    }

    private static int? TryParseGeneratedKodNumber(string? hesapKodu)
    {
        if (string.IsNullOrWhiteSpace(hesapKodu))
            return null;

        var normalizedKod = hesapKodu.Trim().ToUpperInvariant();
        if (!normalizedKod.StartsWith(HesapKodPrefix, StringComparison.Ordinal))
            return null;

        var numberPart = normalizedKod[HesapKodPrefix.Length..];
        return int.TryParse(numberPart, out var number) ? number : null;
    }
}
