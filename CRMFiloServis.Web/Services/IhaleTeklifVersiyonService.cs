using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using CRMFiloServis.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class IhaleTeklifVersiyonService : IIhaleTeklifVersiyonService
{
    private readonly ApplicationDbContext _context;
    private readonly IIhaleHazirlikService _ihaleHazirlikService;
    private readonly IKullaniciService _kullaniciService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<IhaleTeklifVersiyonService> _logger;

    public IhaleTeklifVersiyonService(
        ApplicationDbContext context,
        IIhaleHazirlikService ihaleHazirlikService,
        IKullaniciService kullaniciService,
        IAuditLogService auditLogService,
        ILogger<IhaleTeklifVersiyonService> logger)
    {
        _context = context;
        _ihaleHazirlikService = ihaleHazirlikService;
        _kullaniciService = kullaniciService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<IhaleTeklifVersiyon?> GetByIdAsync(int versiyonId)
    {
        return await _context.IhaleTeklifVersiyonlari
            .Include(v => v.IhaleProje)
            .Include(v => v.HazirlayanKullanici)
            .Include(v => v.OnaylayanKullanici)
            .FirstOrDefaultAsync(v => v.Id == versiyonId);
    }

    public async Task<List<IhaleTeklifVersiyon>> GetListByIhaleProjeIdAsync(int ihaleProjeId)
    {
        return await _context.IhaleTeklifVersiyonlari
            .Include(v => v.HazirlayanKullanici)
            .Include(v => v.OnaylayanKullanici)
            .Where(v => v.IhaleProjeId == ihaleProjeId)
            .OrderByDescending(v => v.VersiyonNo)
            .ToListAsync();
    }

    public async Task<IhaleTeklifVersiyon?> GetAktifVersiyonAsync(int ihaleProjeId)
    {
        return await _context.IhaleTeklifVersiyonlari
            .Include(v => v.HazirlayanKullanici)
            .Include(v => v.OnaylayanKullanici)
            .FirstOrDefaultAsync(v => v.IhaleProjeId == ihaleProjeId && v.AktifVersiyon);
    }

    public async Task<List<IhaleTeklifKararLog>> GetKararLoglariAsync(int versiyonId)
    {
        return await _context.IhaleTeklifKararLoglari
            .Include(l => l.IslemYapanKullanici)
            .Where(l => l.IhaleTeklifVersiyonId == versiyonId)
            .OrderByDescending(l => l.IslemTarihi)
            .ToListAsync();
    }

    public async Task<IhaleTeklifVersiyon> CreateInitialAsync(int ihaleProjeId)
    {
        var kullanici = await GetCurrentUserOrThrowAsync();
        EnsureCanManageDrafts(kullanici);

        var proje = await GetProjeAsync(ihaleProjeId);
        var mevcutVersiyonVar = await _context.IhaleTeklifVersiyonlari
            .AnyAsync(v => v.IhaleProjeId == ihaleProjeId);

        if (mevcutVersiyonVar)
            throw new InvalidOperationException("Bu ihale için zaten teklif versiyonu oluşturulmuş.");

        var versiyon = new IhaleTeklifVersiyon
        {
            IhaleProjeId = proje.Id,
            VersiyonNo = 1,
            RevizyonKodu = "V1",
            Durum = IhaleTeklifVersiyonDurum.Taslak,
            HazirlayanKullaniciId = kullanici?.Id,
            HazirlamaTarihi = DateTime.UtcNow,
            AktifVersiyon = true
        };

        await SnapshotDoldurAsync(versiyon);

        _context.IhaleTeklifVersiyonlari.Add(versiyon);
        await _context.SaveChangesAsync();

        await KararLogEkleAsync(
            versiyon.Id,
            IhaleTeklifIslemTipi.Olustur,
            null,
            versiyon.Durum,
            "İlk teklif versiyonu oluşturuldu.",
            kullanici?.Id);

        await _context.SaveChangesAsync();
        await TryAuditAsync("IhaleTeklifVersiyonOlustur", versiyon.Id, $"{proje.ProjeKodu} için ilk teklif versiyonu oluşturuldu.");

        return versiyon;
    }

    public async Task<IhaleTeklifVersiyon> CreateRevisionAsync(int kaynakVersiyonId, string? revizyonNotu)
    {
        var kullanici = await GetCurrentUserOrThrowAsync();
        EnsureCanManageDrafts(kullanici);

        var kaynakVersiyon = await _context.IhaleTeklifVersiyonlari
            .AsTracking()
            .FirstOrDefaultAsync(v => v.Id == kaynakVersiyonId);

        if (kaynakVersiyon == null)
            throw new InvalidOperationException("Kaynak teklif versiyonu bulunamadı.");

        var proje = await GetProjeAsync(kaynakVersiyon.IhaleProjeId);
        var yeniVersiyonNo = await GetSonrakiVersiyonNoAsync(kaynakVersiyon.IhaleProjeId);

        await PasiflestirDigerAktifVersiyonlarAsync(kaynakVersiyon.IhaleProjeId, null);

        var yeniVersiyon = new IhaleTeklifVersiyon
        {
            IhaleProjeId = kaynakVersiyon.IhaleProjeId,
            VersiyonNo = yeniVersiyonNo,
            RevizyonKodu = $"V{yeniVersiyonNo}",
            Durum = IhaleTeklifVersiyonDurum.Taslak,
            RevizyonNotu = revizyonNotu,
            HazirlayanKullaniciId = kullanici?.Id,
            HazirlamaTarihi = DateTime.UtcNow,
            AktifVersiyon = true,
            ToplamMaliyet = kaynakVersiyon.ToplamMaliyet,
            TeklifTutari = kaynakVersiyon.TeklifTutari,
            KarMarjiTutari = kaynakVersiyon.KarMarjiTutari,
            KarMarjiOrani = kaynakVersiyon.KarMarjiOrani
        };

        await SnapshotDoldurAsync(yeniVersiyon);

        _context.IhaleTeklifVersiyonlari.Add(yeniVersiyon);
        await _context.SaveChangesAsync();

        await KararLogEkleAsync(
            yeniVersiyon.Id,
            IhaleTeklifIslemTipi.RevizyonOlustur,
            null,
            yeniVersiyon.Durum,
            revizyonNotu ?? $"{kaynakVersiyon.RevizyonKodu} versiyonundan revizyon oluşturuldu.",
            kullanici?.Id);

        await _context.SaveChangesAsync();
        await TryAuditAsync("IhaleTeklifRevizyonOlustur", yeniVersiyon.Id, $"{proje.ProjeKodu} için {yeniVersiyon.RevizyonKodu} revizyonu oluşturuldu.");

        return yeniVersiyon;
    }

    public async Task<IhaleTeklifVersiyon> SetActiveAsync(int versiyonId)
    {
        var kullanici = await GetCurrentUserOrThrowAsync();
        EnsureCanManageDrafts(kullanici);

        var versiyon = await _context.IhaleTeklifVersiyonlari
            .AsTracking()
            .FirstOrDefaultAsync(v => v.Id == versiyonId);

        if (versiyon == null)
            throw new InvalidOperationException("Teklif versiyonu bulunamadı.");

        if (!versiyon.AktifVersiyon)
        {
            await PasiflestirDigerAktifVersiyonlarAsync(versiyon.IhaleProjeId, versiyon.Id);
            versiyon.AktifVersiyon = true;
            versiyon.UpdatedAt = DateTime.UtcNow;
            await KararLogEkleAsync(
                versiyon.Id,
                IhaleTeklifIslemTipi.AktifVersiyonDegisti,
                versiyon.Durum,
                versiyon.Durum,
                $"{versiyon.RevizyonKodu} aktif versiyon olarak işaretlendi.",
                kullanici?.Id);

            await _context.SaveChangesAsync();
            await TryAuditAsync("IhaleTeklifAktifVersiyon", versiyon.Id, $"{versiyon.RevizyonKodu} aktif versiyon yapıldı.");
        }

        return versiyon;
    }

    public async Task<IhaleTeklifVersiyon> SendToReviewAsync(int versiyonId)
    {
        var kullanici = await GetCurrentUserOrThrowAsync();
        EnsureCanManageDrafts(kullanici);

        var versiyon = await _context.IhaleTeklifVersiyonlari
            .AsTracking()
            .FirstOrDefaultAsync(v => v.Id == versiyonId);

        if (versiyon == null)
            throw new InvalidOperationException("Teklif versiyonu bulunamadı.");

        if (versiyon.Durum != IhaleTeklifVersiyonDurum.Taslak)
            throw new InvalidOperationException("Sadece taslak durumundaki teklif incelemeye gönderilebilir.");

        var oncekiDurum = versiyon.Durum;

        versiyon.Durum = IhaleTeklifVersiyonDurum.Incelemede;
        versiyon.UpdatedAt = DateTime.UtcNow;

        await KararLogEkleAsync(
            versiyon.Id,
            IhaleTeklifIslemTipi.IncelemeyeGonder,
            oncekiDurum,
            versiyon.Durum,
            "Teklif incelemeye gönderildi.",
            kullanici?.Id);

        await _context.SaveChangesAsync();
        await TryAuditAsync("IhaleTeklifInceleme", versiyon.Id, $"{versiyon.RevizyonKodu} incelemeye gönderildi.");

        return versiyon;
    }

    public async Task<IhaleTeklifVersiyon> ApproveAsync(int versiyonId, string? kararNotu)
    {
        var kullanici = await GetCurrentUserOrThrowAsync();
        EnsureCanApprove(kullanici);

        var versiyon = await _context.IhaleTeklifVersiyonlari
            .AsTracking()
            .FirstOrDefaultAsync(v => v.Id == versiyonId);

        if (versiyon == null)
            throw new InvalidOperationException("Teklif versiyonu bulunamadı.");

        if (versiyon.Durum != IhaleTeklifVersiyonDurum.Incelemede)
            throw new InvalidOperationException("Sadece incelemedeki teklifler onaylanabilir.");

        var oncekiDurum = versiyon.Durum;

        versiyon.Durum = IhaleTeklifVersiyonDurum.Onaylandi;
        versiyon.KararNotu = kararNotu;
        versiyon.OnaylayanKullaniciId = kullanici?.Id;
        versiyon.OnayTarihi = DateTime.UtcNow;
        versiyon.UpdatedAt = DateTime.UtcNow;

        await KararLogEkleAsync(
            versiyon.Id,
            IhaleTeklifIslemTipi.Onayla,
            oncekiDurum,
            versiyon.Durum,
            kararNotu ?? "Teklif onaylandı.",
            kullanici?.Id);

        await _context.SaveChangesAsync();
        await TryAuditAsync("IhaleTeklifOnay", versiyon.Id, $"{versiyon.RevizyonKodu} onaylandı.");

        return versiyon;
    }

    public async Task<IhaleTeklifVersiyon> RejectAsync(int versiyonId, string kararNotu)
    {
        var kullanici = await GetCurrentUserOrThrowAsync();
        EnsureCanApprove(kullanici);

        if (string.IsNullOrWhiteSpace(kararNotu))
            throw new InvalidOperationException("Reddedilen teklifler için karar notu zorunludur.");

        var versiyon = await _context.IhaleTeklifVersiyonlari
            .AsTracking()
            .FirstOrDefaultAsync(v => v.Id == versiyonId);

        if (versiyon == null)
            throw new InvalidOperationException("Teklif versiyonu bulunamadı.");

        if (versiyon.Durum != IhaleTeklifVersiyonDurum.Incelemede)
            throw new InvalidOperationException("Sadece incelemedeki teklifler reddedilebilir.");

        var oncekiDurum = versiyon.Durum;

        versiyon.Durum = IhaleTeklifVersiyonDurum.Reddedildi;
        versiyon.KararNotu = kararNotu;
        versiyon.OnaylayanKullaniciId = kullanici?.Id;
        versiyon.OnayTarihi = DateTime.UtcNow;
        versiyon.UpdatedAt = DateTime.UtcNow;

        await KararLogEkleAsync(
            versiyon.Id,
            IhaleTeklifIslemTipi.Reddet,
            oncekiDurum,
            versiyon.Durum,
            kararNotu,
            kullanici?.Id);

        await _context.SaveChangesAsync();
        await TryAuditAsync("IhaleTeklifRed", versiyon.Id, $"{versiyon.RevizyonKodu} reddedildi.");

        return versiyon;
    }

    private async Task<IhaleProje> GetProjeAsync(int ihaleProjeId)
    {
        var proje = await _context.IhaleProjeleri
            .FirstOrDefaultAsync(p => p.Id == ihaleProjeId);

        if (proje == null)
            throw new InvalidOperationException("İhale projesi bulunamadı.");

        return proje;
    }

    private async Task<int> GetSonrakiVersiyonNoAsync(int ihaleProjeId)
    {
        var sonVersiyonNo = await _context.IhaleTeklifVersiyonlari
            .Where(v => v.IhaleProjeId == ihaleProjeId)
            .MaxAsync(v => (int?)v.VersiyonNo) ?? 0;

        return sonVersiyonNo + 1;
    }

    private async Task<Kullanici> GetCurrentUserOrThrowAsync()
    {
        var kullanici = await _kullaniciService.GetAktifKullaniciAsync();
        if (kullanici == null)
            throw new InvalidOperationException("Bu işlem için oturum açmış kullanıcı gereklidir.");

        return kullanici;
    }

    private static void EnsureCanManageDrafts(Kullanici kullanici)
    {
        if (!HasAnyRole(kullanici, "Admin", "Operasyon"))
            throw new InvalidOperationException("Bu işlem için Admin veya Operasyon rolü gereklidir.");
    }

    private static void EnsureCanApprove(Kullanici kullanici)
    {
        if (!HasAnyRole(kullanici, "Admin", "Yönetici", "Yonetici"))
            throw new InvalidOperationException("Bu işlem için Admin veya Yönetici rolü gereklidir.");
    }

    private static bool HasAnyRole(Kullanici kullanici, params string[] roller)
    {
        var rolAdi = kullanici.Rol?.RolAdi;
        if (string.IsNullOrWhiteSpace(rolAdi))
            return false;

        return roller.Any(rol => string.Equals(rolAdi, rol, StringComparison.OrdinalIgnoreCase));
    }

    private async Task SnapshotDoldurAsync(IhaleTeklifVersiyon versiyon)
    {
        var ozet = await _ihaleHazirlikService.GetProjeOzetAsync(versiyon.IhaleProjeId);
        versiyon.ToplamMaliyet = ozet.ToplamProjeMaliyeti;
        versiyon.TeklifTutari = ozet.ToplamProjeTeklif;
        versiyon.KarMarjiTutari = ozet.ToplamProjeKar;
        versiyon.KarMarjiOrani = ozet.KarMarjiOrtalama;
    }

    private async Task PasiflestirDigerAktifVersiyonlarAsync(int ihaleProjeId, int? haricVersiyonId)
    {
        var aktifVersiyonlar = await _context.IhaleTeklifVersiyonlari
            .AsTracking()
            .Where(v => v.IhaleProjeId == ihaleProjeId && v.AktifVersiyon && (!haricVersiyonId.HasValue || v.Id != haricVersiyonId.Value))
            .ToListAsync();

        foreach (var aktifVersiyon in aktifVersiyonlar)
        {
            aktifVersiyon.AktifVersiyon = false;
            aktifVersiyon.UpdatedAt = DateTime.UtcNow;
        }
    }

    private Task KararLogEkleAsync(
        int versiyonId,
        IhaleTeklifIslemTipi islemTipi,
        IhaleTeklifVersiyonDurum? oncekiDurum,
        IhaleTeklifVersiyonDurum yeniDurum,
        string? not,
        int? kullaniciId)
    {
        _context.IhaleTeklifKararLoglari.Add(new IhaleTeklifKararLog
        {
            IhaleTeklifVersiyonId = versiyonId,
            IslemTipi = islemTipi,
            OncekiDurum = oncekiDurum,
            YeniDurum = yeniDurum,
            Not = not,
            IslemYapanKullaniciId = kullaniciId,
            IslemTarihi = DateTime.UtcNow
        });

        return Task.CompletedTask;
    }

    private async Task TryAuditAsync(string islemTipi, int entityId, string aciklama)
    {
        try
        {
            await _auditLogService.LogCustomAsync(islemTipi, nameof(IhaleTeklifVersiyon), entityId, aciklama, AuditKategorileri.Sistem);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "İhale teklif versiyon audit kaydı oluşturulamadı. EntityId: {EntityId}", entityId);
        }
    }
}
