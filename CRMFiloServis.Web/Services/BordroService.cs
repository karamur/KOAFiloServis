using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using CRMFiloServis.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System.Globalization;

namespace CRMFiloServis.Web.Services;

public class BordroService : IBordroService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IMuhasebeService _muhasebeService;
    private static readonly CultureInfo TrCulture = new("tr-TR");

    public BordroService(IDbContextFactory<ApplicationDbContext> contextFactory, IMuhasebeService muhasebeService)
    {
        _contextFactory = contextFactory;
        _muhasebeService = muhasebeService;
    }

    #region Bordro Listeleme

    public async Task<List<Bordro>> GetBordrolarAsync(int? firmaId = null, int? yil = null, BordroTipi? tip = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var query = context.Bordrolar
            .Include(b => b.Firma)
            .Include(b => b.BordroDetaylar)
            .AsQueryable();

        if (firmaId.HasValue)
            query = query.Where(b => b.FirmaId == firmaId);
        
        if (yil.HasValue)
            query = query.Where(b => b.Yil == yil);
        
        if (tip.HasValue)
            query = query.Where(b => b.BordroTipi == tip);

        return await query.OrderByDescending(b => b.Yil).ThenByDescending(b => b.Ay).ToListAsync();
    }

    public async Task<Bordro?> GetBordroByIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        return await context.Bordrolar
            .Include(b => b.Firma)
            .Include(b => b.BordroDetaylar)
                .ThenInclude(d => d.Personel)
            .Include(b => b.BordroDetaylar)
                .ThenInclude(d => d.Firma)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Bordro?> GetBordroByDönemAsync(int yil, int ay, int? firmaId, BordroTipi tip)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var query = context.Bordrolar
            .Include(b => b.Firma)
            .Include(b => b.BordroDetaylar)
                .ThenInclude(d => d.Personel)
            .Where(b => b.Yil == yil && b.Ay == ay && b.BordroTipi == tip);

        if (firmaId.HasValue)
            query = query.Where(b => b.FirmaId == firmaId);
        else
            query = query.Where(b => b.FirmaId == null);

        return await query.FirstOrDefaultAsync();
    }

    #endregion

    #region Bordro Oluşturma ve Hesaplama

    public async Task<Bordro> CreateBordroAsync(int yil, int ay, int? firmaId, BordroTipi tip)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        // Aynı dönem için bordro var mı kontrol et
        var mevcutBordro = await GetBordroByDönemAsync(yil, ay, firmaId, tip);
        if (mevcutBordro != null)
            throw new InvalidOperationException($"{yil}/{ay} dönemi için {tip} bordro zaten mevcut!");

        var bordro = new Bordro
        {
            Yil = yil,
            Ay = ay,
            FirmaId = firmaId,
            BordroTipi = tip,
            HesaplamaTarihi = DateTime.Now,
            Onaylandi = false
        };

        context.Bordrolar.Add(bordro);
        await context.SaveChangesAsync();

        // Bordroyu hesapla
        await HesaplaBordroAsync(bordro.Id);

        return bordro;
    }

    public async Task<bool> HesaplaBordroAsync(int bordroId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var bordro = await context.Bordrolar
            .Include(b => b.BordroDetaylar)
            .FirstOrDefaultAsync(b => b.Id == bordroId);

        if (bordro == null)
            throw new InvalidOperationException("Bordro bulunamadı!");

        if (bordro.Onaylandi)
            throw new InvalidOperationException("Onaylı bordro yeniden hesaplanamaz!");

        // Ayarları al
        var ayarlar = await GetBordroAyarAsync(bordro.FirmaId);
        var donemBaslangic = new DateTime(bordro.Yil, bordro.Ay, 1);
        var donemBitis = new DateTime(bordro.Yil, bordro.Ay, DateTime.DaysInMonth(bordro.Yil, bordro.Ay));

        // Mevcut detayları temizle
        context.BordroDetaylar.RemoveRange(bordro.BordroDetaylar);
        await context.SaveChangesAsync();

        var personelQuery = context.Soforler.AsQueryable();

        // Not: Sofor entity'sinde FirmaId yok, tüm aktif personeller alınır
        // Firma bilgisi bordro detayında tutulur

        // Bordro tipine göre filtrele (SGKBordroDahilMi ve BordroTipiPersonel'e göre)
        personelQuery = personelQuery.Where(s => s.SGKBordroDahilMi);

        if (bordro.BordroTipi == BordroTipi.Arge)
            personelQuery = personelQuery.Where(s => s.BordroTipiPersonel == PersonelBordroTipi.Arge);
        else if (bordro.BordroTipi == BordroTipi.Normal)
            personelQuery = personelQuery.Where(s => s.BordroTipiPersonel == PersonelBordroTipi.Normal);

        var personeller = (await personelQuery.ToListAsync())
            .Where(p => PersonelBordroyaDahilEdilsinMi(p, donemBaslangic, donemBitis))
            .ToList();

        if (!personeller.Any())
            throw new InvalidOperationException("Hesaplanacak personel bulunamadı!");

        // Her personel için detay oluştur
        foreach (var personel in personeller)
        {
            var sgkMaasOrani = GetDonemMaasOrani(personel.SgkCikisTarihi, donemBaslangic, donemBitis);
            var gercekMaasOrani = GetDonemMaasOrani(personel.IstenAyrilmaTarihi, donemBaslangic, donemBitis);
            var resmiNetMaasTam = personel.ResmiNetMaas > 0 || personel.DigerMaas > 0
                ? personel.ResmiNetMaas
                : personel.NetMaas;
            var resmiNetMaas = YuvarlaTutar(resmiNetMaasTam * sgkMaasOrani);
            var digerMaas = YuvarlaTutar(personel.DigerMaas * gercekMaasOrani);
            var brutMaas = YuvarlaTutar(personel.BrutMaas * sgkMaasOrani);

            var detay = new BordroDetay
            {
                BordroId = bordro.Id,
                PersonelId = personel.Id,
                FirmaId = bordro.FirmaId, // Bordrodan al
                BrutMaas = brutMaas,
                NetMaas = resmiNetMaas,
                TopluMaas = resmiNetMaas + digerMaas,
                SgkMaasi = resmiNetMaas,
                EkOdeme = digerMaas
            };

            // Kesintileri hesapla
            detay.SgkIssizlikKesinti = detay.SgkMaasi * (ayarlar.SgkIsciPayiOrani + ayarlar.IssizlikIsciPayiOrani) / 100;
            
            // Gelir vergisi hesaplama (basit: %15 varsayım, gerçekte dilimlere göre hesaplanmalı)
            var gelirVergisiMatrahi = detay.SgkMaasi - detay.SgkIssizlikKesinti;
            detay.GelirVergisi = gelirVergisiMatrahi * 0.15M;
            
            // Damga vergisi
            detay.DamgaVergisi = detay.SgkMaasi * ayarlar.DamgaVergisiOrani / 100;

            // Ek ödeme bordroda diğer maaş olarak taşınır
            detay.TopluMaas = detay.NetMaas + detay.EkOdeme;

            context.BordroDetaylar.Add(detay);
        }

        await context.SaveChangesAsync();

        // Bordro özetini güncelle
        await UpdateBordroOzetAsync(bordro.Id);

        return true;
    }

    private static bool PersonelBordroyaDahilEdilsinMi(Sofor personel, DateTime donemBaslangic, DateTime donemBitis)
    {
        if (personel.Aktif)
            return true;

        return TarihDonemIcinde(personel.IstenAyrilmaTarihi, donemBaslangic, donemBitis)
            || TarihDonemIcinde(personel.SgkCikisTarihi, donemBaslangic, donemBitis);
    }

    private static decimal GetDonemMaasOrani(DateTime? cikisTarihi, DateTime donemBaslangic, DateTime donemBitis)
    {
        if (!cikisTarihi.HasValue)
            return 1m;

        var tarih = cikisTarihi.Value.Date;
        if (tarih < donemBaslangic.Date)
            return 0m;

        if (tarih > donemBitis.Date)
            return 1m;

        var donemGunSayisi = DateTime.DaysInMonth(donemBaslangic.Year, donemBaslangic.Month);
        return tarih.Day / (decimal)donemGunSayisi;
    }

    private static bool TarihDonemIcinde(DateTime? tarih, DateTime donemBaslangic, DateTime donemBitis)
    {
        return tarih.HasValue && tarih.Value.Date >= donemBaslangic.Date && tarih.Value.Date <= donemBitis.Date;
    }

    private static decimal YuvarlaTutar(decimal tutar)
    {
        return Math.Round(tutar, 2, MidpointRounding.AwayFromZero);
    }

    private async Task UpdateBordroOzetAsync(int bordroId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var bordro = await context.Bordrolar
            .Include(b => b.BordroDetaylar)
            .FirstOrDefaultAsync(b => b.Id == bordroId);

        if (bordro == null) return;

        bordro.ToplamPersonelSayisi = bordro.BordroDetaylar.Count;
        bordro.ToplamBrutMaas = bordro.BordroDetaylar.Sum(d => d.BrutMaas);
        bordro.ToplamNetMaas = bordro.BordroDetaylar.Sum(d => d.NetMaas);
        bordro.ToplamSgkMatrahi = bordro.BordroDetaylar.Sum(d => d.SgkMaasi);
        bordro.ToplamEkOdeme = bordro.BordroDetaylar.Sum(d => d.EkOdeme);

        await context.SaveChangesAsync();
    }

    public async Task<bool> OnaylaBordroAsync(int bordroId, string onaylayanKullanici)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var bordro = await context.Bordrolar
            .Include(b => b.BordroDetaylar)
            .FirstOrDefaultAsync(b => b.Id == bordroId);

        if (bordro == null)
            throw new InvalidOperationException("Bordro bulunamadı!");

        if (bordro.Onaylandi)
            throw new InvalidOperationException("Bordro zaten onaylı!");

        if (!bordro.BordroDetaylar.Any())
            throw new InvalidOperationException("Detaysız bordro onaylanamaz!");

        bordro.Onaylandi = true;
        bordro.OnayTarihi = DateTime.Now;
        bordro.OnaylayanKullanici = onaylayanKullanici;

        // Muhasebe fişi oluştur
        // TODO: Muhasebe fişi entegrasyonu yapılacak
        // await CreateBordroMuhasebeFisiAsync(bordro);

        await context.SaveChangesAsync();

        return true;
    }

    // TODO: Muhasebe fişi entegrasyonu yapılacak
    /*
    private async Task CreateBordroMuhasebeFisiAsync(Bordro bordro)
    {
        var ayarlar = await GetBordroAyarAsync(bordro.FirmaId);
        var tarih = new DateTime(bordro.Yil, bordro.Ay, DateTime.DaysInMonth(bordro.Yil, bordro.Ay));

        var fisAciklama = $"{bordro.DonemeAdi} {bordro.BordroTipi} Bordro";
        // Muhasebe fiş oluşturma kodu buraya gelecek
        await Task.CompletedTask;
    }
    */

    public async Task<bool> OnayIptalEtAsync(int bordroId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var bordro = await context.Bordrolar
            .Include(b => b.BordroDetaylar)
            .FirstOrDefaultAsync(b => b.Id == bordroId);

        if (bordro == null)
            throw new InvalidOperationException("Bordro bulunamadı!");

        if (!bordro.Onaylandi)
            throw new InvalidOperationException("Bordro zaten onaysız!");

        // Ödeme yapılmış mı kontrol et
        var odemeYapilmis = bordro.BordroDetaylar.Any(d => d.BankaOdemesiYapildi || d.EkOdemeYapildi);
        if (odemeYapilmis)
            throw new InvalidOperationException("Ödeme yapılmış bordronun onayı iptal edilemez!");

        bordro.Onaylandi = false;
        bordro.OnayTarihi = null;
        bordro.OnaylayanKullanici = null;

        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteBordroAsync(int bordroId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var bordro = await context.Bordrolar
            .Include(b => b.BordroDetaylar)
            .FirstOrDefaultAsync(b => b.Id == bordroId);

        if (bordro == null)
            return false;

        if (bordro.Onaylandi)
            throw new InvalidOperationException("Onaylı bordro silinemez!");

        context.Bordrolar.Remove(bordro);
        await context.SaveChangesAsync();

        return true;
    }

    #endregion

    #region Bordro Detay İşlemleri

    public async Task<List<BordroDetay>> GetBordroDetaylarAsync(int bordroId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        return await context.BordroDetaylar
            .Include(d => d.Personel)
            .Include(d => d.Firma)
            .Include(d => d.Bordro)
            .Where(d => d.BordroId == bordroId)
            .OrderBy(d => d.Personel.Ad)
            .ToListAsync();
    }

    public async Task<BordroDetay?> GetBordroDetayByIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        return await context.BordroDetaylar
            .Include(d => d.Personel)
            .Include(d => d.Firma)
            .Include(d => d.Bordro)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task UpdateBordroDetayAsync(BordroDetay detay)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var mevcutDetay = await context.BordroDetaylar
            .Include(d => d.Bordro)
            .FirstOrDefaultAsync(d => d.Id == detay.Id);

        if (mevcutDetay == null)
            throw new InvalidOperationException("Detay bulunamadı!");

        if (mevcutDetay.Bordro.Onaylandi)
            throw new InvalidOperationException("Onaylı bordro detayı güncellenemez!");

        mevcutDetay.BrutMaas = detay.BrutMaas;
        mevcutDetay.NetMaas = detay.NetMaas;
        mevcutDetay.TopluMaas = detay.TopluMaas;
        mevcutDetay.SgkMaasi = detay.SgkMaasi;
        mevcutDetay.EkOdeme = detay.EkOdeme;
        mevcutDetay.SgkIssizlikKesinti = detay.SgkIssizlikKesinti;
        mevcutDetay.GelirVergisi = detay.GelirVergisi;
        mevcutDetay.DamgaVergisi = detay.DamgaVergisi;
        mevcutDetay.YemekYardimi = detay.YemekYardimi;
        mevcutDetay.YolYardimi = detay.YolYardimi;
        mevcutDetay.PrimTutar = detay.PrimTutar;
        mevcutDetay.DigerEkOdeme = detay.DigerEkOdeme;
        mevcutDetay.Notlar = detay.Notlar;

        await context.SaveChangesAsync();

        // Bordro özetini güncelle
        await UpdateBordroOzetAsync(mevcutDetay.BordroId);
    }

    public async Task<bool> SilBordroDetayAsync(int detayId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var detay = await context.BordroDetaylar
            .Include(d => d.Bordro)
            .FirstOrDefaultAsync(d => d.Id == detayId);

        if (detay == null)
            return false;

        if (detay.Bordro.Onaylandi)
            throw new InvalidOperationException("Onaylı bordro detayı silinemez!");

        if (detay.BankaOdemesiYapildi || detay.EkOdemeYapildi)
            throw new InvalidOperationException("Ödeme yapılmış detay silinemez!");

        context.BordroDetaylar.Remove(detay);
        await context.SaveChangesAsync();

        // Bordro özetini güncelle
        await UpdateBordroOzetAsync(detay.BordroId);

        return true;
    }

    #endregion

    #region Ödeme İşlemleri

    public async Task<bool> BankaOdemesiYapAsync(List<int> detayIds, DateTime odemeTarihi, int? bankaHesapId, string? aciklama)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var detaylar = await context.BordroDetaylar
            .Include(d => d.Bordro)
            .Where(d => detayIds.Contains(d.Id))
            .ToListAsync();

        if (!detaylar.Any())
            throw new InvalidOperationException("Detay bulunamadı!");

        if (detaylar.Any(d => !d.Bordro.Onaylandi))
            throw new InvalidOperationException("Onaysız bordro için ödeme yapılamaz!");

        if (detaylar.Any(d => d.BankaOdemesiYapildi))
            throw new InvalidOperationException("Bazı kayıtlar için zaten banka ödemesi yapılmış!");

        foreach (var detay in detaylar)
        {
            var odeme = new BordroOdeme
            {
                BordroDetayId = detay.Id,
                OdemeTipi = Shared.Entities.OdemeTipi.BankaOdemesi,
                OdemeTarihi = odemeTarihi,
                OdemeTutari = detay.NetMaas + detay.ToplamEkOdeme,
                OdemeSekli = Shared.Entities.OdemeSekli.BankaTransfer,
                BankaHesapId = bankaHesapId,
                Aciklama = aciklama ?? "SGK Maaşı Banka Ödemesi"
            };

            context.BordroOdemeler.Add(odeme);

            detay.BankaOdemesiYapildi = true;
            detay.BankaOdemeTarihi = odemeTarihi;
        }

        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> EkOdemeYapAsync(List<int> detayIds, DateTime odemeTarihi, OdemeSekli odemeSekli, int? bankaHesapId, string? aciklama)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var detaylar = await context.BordroDetaylar
            .Include(d => d.Bordro)
            .Where(d => detayIds.Contains(d.Id))
            .ToListAsync();

        if (!detaylar.Any())
            throw new InvalidOperationException("Detay bulunamadı!");

        if (detaylar.Any(d => !d.Bordro.Onaylandi))
            throw new InvalidOperationException("Onaysız bordro için ödeme yapılamaz!");

        if (detaylar.Any(d => d.EkOdemeYapildi))
            throw new InvalidOperationException("Bazı kayıtlar için zaten ek ödeme yapılmış!");

        foreach (var detay in detaylar)
        {
            if (detay.EkOdeme <= 0)
                continue;

            var odeme = new BordroOdeme
            {
                BordroDetayId = detay.Id,
                OdemeTipi = Shared.Entities.OdemeTipi.EkOdeme,
                OdemeTarihi = odemeTarihi,
                OdemeTutari = detay.EkOdeme,
                OdemeSekli = odemeSekli,
                BankaHesapId = bankaHesapId,
                Aciklama = aciklama ?? "Ek Ödeme (Toplu Maaş Farkı)"
            };

            context.BordroOdemeler.Add(odeme);

            detay.EkOdemeYapildi = true;
            detay.EkOdemeTarihi = odemeTarihi;
        }

        await context.SaveChangesAsync();

        return true;
    }

    public async Task<List<BordroOdeme>> GetOdemelerAsync(int bordroDetayId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        return await context.BordroOdemeler
            .Include(o => o.BankaHesap)
            .Where(o => o.BordroDetayId == bordroDetayId)
            .OrderByDescending(o => o.OdemeTarihi)
            .ToListAsync();
    }

    #endregion

    #region Raporlama

    public async Task<byte[]> ExportBankaOdemeListesiAsync(int bordroId)
    {
        var bordro = await GetBordroByIdAsync(bordroId);
        if (bordro == null)
            throw new InvalidOperationException("Bordro bulunamadı!");

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Banka Ödeme Listesi");

        // Başlık
        worksheet.Cell("A1").Value = $"{bordro.DonemeAdi} {bordro.BordroTipi} Bordro - Banka Ödeme Listesi";
        worksheet.Range("A1:H1").Merge().Style.Font.Bold = true;
        worksheet.Range("A1:H1").Style.Font.FontSize = 14;
        worksheet.Range("A1:H1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Sütun başlıkları
        var header = worksheet.Row(3);
        header.Cell(1).Value = "Personel Kodu";
        header.Cell(2).Value = "Ad Soyad";
        header.Cell(3).Value = "TC Kimlik No";
        header.Cell(4).Value = "IBAN";
        header.Cell(5).Value = "SGK Maaşı (Net)";
        header.Cell(6).Value = "Ek Ödemeler";
        header.Cell(7).Value = "Toplam";
        header.Cell(8).Value = "Durum";
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Veriler
        int row = 4;
        decimal toplamSgk = 0, toplamEk = 0, toplamGenel = 0;

        foreach (var detay in bordro.BordroDetaylar.OrderBy(d => d.Personel.Ad))
        {
            worksheet.Cell(row, 1).Value = detay.Personel.SoforKodu;
            worksheet.Cell(row, 2).Value = detay.Personel.TamAd;
            worksheet.Cell(row, 3).Value = detay.Personel.TcKimlikNo ?? "";
            worksheet.Cell(row, 4).Value = detay.Personel.IBAN ?? "";
            worksheet.Cell(row, 5).Value = detay.NetMaas;
            worksheet.Cell(row, 6).Value = detay.ToplamEkOdeme;
            worksheet.Cell(row, 7).Value = detay.NetMaas + detay.ToplamEkOdeme;
            worksheet.Cell(row, 8).Value = detay.BankaOdemesiYapildi ? "Ödendi" : "Bekliyor";

            toplamSgk += detay.NetMaas;
            toplamEk += detay.ToplamEkOdeme;
            toplamGenel += detay.NetMaas + detay.ToplamEkOdeme;

            row++;
        }

        // Toplam
        worksheet.Cell(row, 4).Value = "TOPLAM:";
        worksheet.Cell(row, 5).Value = toplamSgk;
        worksheet.Cell(row, 6).Value = toplamEk;
        worksheet.Cell(row, 7).Value = toplamGenel;
        worksheet.Range(row, 4, row, 7).Style.Font.Bold = true;
        worksheet.Range(row, 4, row, 7).Style.Fill.BackgroundColor = XLColor.LightYellow;

        // Para formatı
        worksheet.Range(4, 5, row, 7).Style.NumberFormat.Format = "#,##0.00 ₺";

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportEkOdemeListesiAsync(int bordroId)
    {
        var bordro = await GetBordroByIdAsync(bordroId);
        if (bordro == null)
            throw new InvalidOperationException("Bordro bulunamadı!");

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Ek Ödeme Listesi");

        // Başlık
        worksheet.Cell("A1").Value = $"{bordro.DonemeAdi} {bordro.BordroTipi} Bordro - Ek Ödeme Listesi (Toplu Maaş Farkı)";
        worksheet.Range("A1:H1").Merge().Style.Font.Bold = true;
        worksheet.Range("A1:H1").Style.Font.FontSize = 14;
        worksheet.Range("A1:H1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Sütun başlıkları
        var header = worksheet.Row(3);
        header.Cell(1).Value = "Personel Kodu";
        header.Cell(2).Value = "Ad Soyad";
        header.Cell(3).Value = "Toplu Maaş";
        header.Cell(4).Value = "SGK Maaşı";
        header.Cell(5).Value = "Ek Ödeme";
        header.Cell(6).Value = "Ödeme Şekli";
        header.Cell(7).Value = "Durum";
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Veriler
        int row = 4;
        decimal toplamTopluMaas = 0, toplamSgkMaas = 0, toplamEkOdeme = 0;

        var detaylarWithEkOdeme = bordro.BordroDetaylar.Where(d => d.EkOdeme > 0).OrderBy(d => d.Personel.Ad);

        foreach (var detay in detaylarWithEkOdeme)
        {
            worksheet.Cell(row, 1).Value = detay.Personel.SoforKodu;
            worksheet.Cell(row, 2).Value = detay.Personel.TamAd;
            worksheet.Cell(row, 3).Value = detay.TopluMaas;
            worksheet.Cell(row, 4).Value = detay.SgkMaasi;
            worksheet.Cell(row, 5).Value = detay.EkOdeme;
            worksheet.Cell(row, 6).Value = ""; // Ödeme şekli sonra girilecek
            worksheet.Cell(row, 7).Value = detay.EkOdemeYapildi ? "Ödendi" : "Bekliyor";

            toplamTopluMaas += detay.TopluMaas;
            toplamSgkMaas += detay.SgkMaasi;
            toplamEkOdeme += detay.EkOdeme;

            row++;
        }

        // Toplam
        worksheet.Cell(row, 2).Value = "TOPLAM:";
        worksheet.Cell(row, 3).Value = toplamTopluMaas;
        worksheet.Cell(row, 4).Value = toplamSgkMaas;
        worksheet.Cell(row, 5).Value = toplamEkOdeme;
        worksheet.Range(row, 2, row, 5).Style.Font.Bold = true;
        worksheet.Range(row, 2, row, 5).Style.Fill.BackgroundColor = XLColor.LightYellow;

        // Para formatı
        worksheet.Range(4, 3, row, 5).Style.NumberFormat.Format = "#,##0.00 ₺";

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportTumBordroAsync(int bordroId)
    {
        var bordro = await GetBordroByIdAsync(bordroId);
        if (bordro == null)
            throw new InvalidOperationException("Bordro bulunamadı!");

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Bordro Detayı");

        // Başlık
        worksheet.Cell("A1").Value = $"{bordro.DonemeAdi} {bordro.BordroTipi} Bordro - Detaylı Liste";
        worksheet.Range("A1:O1").Merge().Style.Font.Bold = true;
        worksheet.Range("A1:O1").Style.Font.FontSize = 14;
        worksheet.Range("A1:O1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Sütun başlıkları
        var header = worksheet.Row(3);
        header.Cell(1).Value = "Personel";
        header.Cell(2).Value = "Brüt Maaş";
        header.Cell(3).Value = "SGK Maaşı";
        header.Cell(4).Value = "SGK Kesinti";
        header.Cell(5).Value = "Gelir Vergisi";
        header.Cell(6).Value = "Damga Vergisi";
        header.Cell(7).Value = "Net Maaş";
        header.Cell(8).Value = "Yemek Yardımı";
        header.Cell(9).Value = "Yol Yardımı";
        header.Cell(10).Value = "Prim";
        header.Cell(11).Value = "Diğer Ek";
        header.Cell(12).Value = "Toplu Maaş";
        header.Cell(13).Value = "Ek Ödeme";
        header.Cell(14).Value = "Toplam Ödenecek";
        header.Cell(15).Value = "IBAN";
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Veriler
        int row = 4;
        foreach (var detay in bordro.BordroDetaylar.OrderBy(d => d.Personel.Ad))
        {
            worksheet.Cell(row, 1).Value = detay.Personel.TamAd;
            worksheet.Cell(row, 2).Value = detay.BrutMaas;
            worksheet.Cell(row, 3).Value = detay.SgkMaasi;
            worksheet.Cell(row, 4).Value = detay.SgkIssizlikKesinti;
            worksheet.Cell(row, 5).Value = detay.GelirVergisi;
            worksheet.Cell(row, 6).Value = detay.DamgaVergisi;
            worksheet.Cell(row, 7).Value = detay.NetMaas;
            worksheet.Cell(row, 8).Value = detay.YemekYardimi;
            worksheet.Cell(row, 9).Value = detay.YolYardimi;
            worksheet.Cell(row, 10).Value = detay.PrimTutar;
            worksheet.Cell(row, 11).Value = detay.DigerEkOdeme;
            worksheet.Cell(row, 12).Value = detay.TopluMaas;
            worksheet.Cell(row, 13).Value = detay.EkOdeme;
            worksheet.Cell(row, 14).Value = detay.ToplamOdenecek;
            worksheet.Cell(row, 15).Value = detay.Personel.IBAN ?? "";
            row++;
        }

        // Para formatı
        worksheet.Range(4, 2, row - 1, 14).Style.NumberFormat.Format = "#,##0.00 ₺";

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<List<BordroKalanOdemeSatir>> GetKalanOdemeRaporuAsync(int bordroId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var bordro = await context.Bordrolar
            .Include(b => b.BordroDetaylar)
                .ThenInclude(d => d.Personel)
            .FirstOrDefaultAsync(b => b.Id == bordroId);

        if (bordro == null)
            throw new InvalidOperationException("Bordro bulunamadı!");

        var personelIds = bordro.BordroDetaylar.Select(d => d.PersonelId).Distinct().ToList();

        var acikBorclar = await context.PersonelBorclar
            .Where(b => personelIds.Contains(b.PersonelId)
                && b.OdemeDurum != BorcOdemeDurum.IptalEdildi
                && b.KalanBorc > 0
                && b.BorcTipi != BorcTipi.MaasAlacagi)
            .GroupBy(b => b.PersonelId)
            .Select(g => new { PersonelId = g.Key, Tutar = g.Sum(x => x.KalanBorc) })
            .ToDictionaryAsync(x => x.PersonelId, x => x.Tutar);

        var acikAvanslar = await context.PersonelAvanslar
            .Where(a => personelIds.Contains(a.PersonelId)
                && a.Durum != AvansDurum.IptalEdildi
                && a.Kalan > 0)
            .GroupBy(a => a.PersonelId)
            .Select(g => new { PersonelId = g.Key, Tutar = g.Sum(x => x.Kalan) })
            .ToDictionaryAsync(x => x.PersonelId, x => x.Tutar);

        var bordroOdemeleri = await context.BordroOdemeler
            .Where(o => personelIds.Contains(o.BordroDetay.PersonelId) && o.BordroDetay.BordroId == bordroId)
            .GroupBy(o => o.BordroDetayId)
            .Select(g => new { BordroDetayId = g.Key, Tutar = g.Sum(x => x.OdemeTutari) })
            .ToDictionaryAsync(x => x.BordroDetayId, x => x.Tutar);

        return bordro.BordroDetaylar
            .OrderBy(d => d.Personel.Ad)
            .ThenBy(d => d.Personel.Soyad)
            .Select(detay =>
            {
                var toplamNetMaas = detay.NetMaas + detay.EkOdeme;
                var bordrodaEleGecen = Math.Min(toplamNetMaas, bordroOdemeleri.GetValueOrDefault(detay.Id));
                var personelHarcamalari = acikBorclar.GetValueOrDefault(detay.PersonelId);
                var personelAvansAlacaklari = acikAvanslar.GetValueOrDefault(detay.PersonelId);
                var avansVeOdemeler = personelHarcamalari - personelAvansAlacaklari;
                var kalanMaas = Math.Max(0, toplamNetMaas - bordrodaEleGecen);

                return new BordroKalanOdemeSatir
                {
                    BordroId = bordroId,
                    BordroDetayId = detay.Id,
                    PersonelId = detay.PersonelId,
                    PersonelKodu = detay.Personel.SoforKodu,
                    PersonelAdSoyad = detay.Personel.TamAd,
                    Iban = detay.Personel.IBAN,
                    NetMaas = toplamNetMaas,
                    BordrodaEleGecen = bordrodaEleGecen,
                    KalanMaas = kalanMaas,
                    PersonelHarcamalari = personelHarcamalari,
                    PersonelAvansAlacaklari = personelAvansAlacaklari,
                    AvansVeOdemeler = avansVeOdemeler,
                    OdenecekMiktar = kalanMaas + avansVeOdemeler,
                    BankaOdemesiYapildi = detay.BankaOdemesiYapildi,
                    EkOdemeYapildi = detay.EkOdemeYapildi
                };
            })
            .ToList();
    }

    public async Task<byte[]> ExportKalanOdemeRaporuAsync(int bordroId)
    {
        var bordro = await GetBordroByIdAsync(bordroId);
        if (bordro == null)
            throw new InvalidOperationException("Bordro bulunamadı!");

        var satirlar = await GetKalanOdemeRaporuAsync(bordroId);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Kalan Ödeme Raporu");

        worksheet.Cell("A1").Value = $"{bordro.DonemeAdi} {bordro.BordroTipi} Bordro - Kalan Ödeme Raporu";
        worksheet.Range("A1:G1").Merge().Style.Font.Bold = true;
        worksheet.Range("A1:G1").Style.Font.FontSize = 14;
        worksheet.Range("A1:G1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        var header = worksheet.Row(3);
        header.Cell(1).Value = "Personel Kodu";
        header.Cell(2).Value = "Ad Soyad";
        header.Cell(3).Value = "IBAN";
        header.Cell(4).Value = "Net Maaş";
        header.Cell(5).Value = "Bordroda Ele Geçen";
        header.Cell(6).Value = "Kalan Maaş";
        header.Cell(7).Value = "Avans + Ödemeler";
        header.Cell(8).Value = "Ödenecek Miktar";
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;

        var row = 4;
        foreach (var satir in satirlar)
        {
            worksheet.Cell(row, 1).Value = satir.PersonelKodu;
            worksheet.Cell(row, 2).Value = satir.PersonelAdSoyad;
            worksheet.Cell(row, 3).Value = satir.Iban ?? string.Empty;
            worksheet.Cell(row, 4).Value = satir.NetMaas;
            worksheet.Cell(row, 5).Value = satir.BordrodaEleGecen;
            worksheet.Cell(row, 6).Value = satir.KalanMaas;
            worksheet.Cell(row, 7).Value = satir.AvansVeOdemeler;
            worksheet.Cell(row, 8).Value = satir.OdenecekMiktar;
            row++;
        }

        worksheet.Cell(row, 3).Value = "TOPLAM:";
        worksheet.Cell(row, 4).Value = satirlar.Sum(x => x.NetMaas);
        worksheet.Cell(row, 5).Value = satirlar.Sum(x => x.BordrodaEleGecen);
        worksheet.Cell(row, 6).Value = satirlar.Sum(x => x.KalanMaas);
        worksheet.Cell(row, 7).Value = satirlar.Sum(x => x.AvansVeOdemeler);
        worksheet.Cell(row, 8).Value = satirlar.Sum(x => x.OdenecekMiktar);
        worksheet.Range(row, 3, row, 8).Style.Font.Bold = true;
        worksheet.Range(row, 3, row, 8).Style.Fill.BackgroundColor = XLColor.LightYellow;

        worksheet.Range(4, 4, row, 8).Style.NumberFormat.Format = "#,##0.00 ₺";
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportBordroOzetAsync(int? firmaId, int? yil)
    {
        var bordrolar = await GetBordrolarAsync(firmaId, yil);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Bordro Özet");

        // Başlık
        worksheet.Cell("A1").Value = $"{yil ?? DateTime.Now.Year} Yılı Bordro Özet Raporu";
        worksheet.Range("A1:I1").Merge().Style.Font.Bold = true;
        worksheet.Range("A1:I1").Style.Font.FontSize = 14;
        worksheet.Range("A1:I1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Sütun başlıkları
        var header = worksheet.Row(3);
        header.Cell(1).Value = "Dönem";
        header.Cell(2).Value = "Tip";
        header.Cell(3).Value = "Personel Sayısı";
        header.Cell(4).Value = "Brüt Maaş";
        header.Cell(5).Value = "Net Maaş";
        header.Cell(6).Value = "SGK Matrahı";
        header.Cell(7).Value = "Ek Ödeme";
        header.Cell(8).Value = "Genel Toplam";
        header.Cell(9).Value = "Durum";
        header.Style.Font.Bold = true;
        header.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Veriler
        int row = 4;
        foreach (var bordro in bordrolar)
        {
            worksheet.Cell(row, 1).Value = bordro.DonemeAdi;
            worksheet.Cell(row, 2).Value = bordro.BordroTipi.ToString();
            worksheet.Cell(row, 3).Value = bordro.ToplamPersonelSayisi;
            worksheet.Cell(row, 4).Value = bordro.ToplamBrutMaas;
            worksheet.Cell(row, 5).Value = bordro.ToplamNetMaas;
            worksheet.Cell(row, 6).Value = bordro.ToplamSgkMatrahi;
            worksheet.Cell(row, 7).Value = bordro.ToplamEkOdeme;
            worksheet.Cell(row, 8).Value = bordro.GenelToplam;
            worksheet.Cell(row, 9).Value = bordro.Onaylandi ? "Onaylı" : "Bekliyor";
            row++;
        }

        // Para formatı
        worksheet.Range(4, 4, row - 1, 8).Style.NumberFormat.Format = "#,##0.00 ₺";

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    #endregion

    #region Ayarlar

    public async Task<BordroAyar> GetBordroAyarAsync(int? firmaId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var ayar = await context.BordroAyarlar
            .FirstOrDefaultAsync(a => a.FirmaId == firmaId);

        if (ayar == null)
        {
            // Varsayılan ayar oluştur
            ayar = new BordroAyar
            {
                FirmaId = firmaId,
                PersonelMaasHesapKodu = "335",
                SgkPrimHesapKodu = "361",
                GelirVergisiHesapKodu = "360",
                KasaHesapKodu = "100",
                BankaHesapKodu = "102",
                PersonelAvansHesapKodu = "195",
                SgkIsciPayiOrani = 14,
                IssizlikIsciPayiOrani = 1,
                DamgaVergisiOrani = 0.759M,
                ArgeSgkIsverenDestekVarMi = true,
                ArgeSgkIsverenDestekOrani = 100
            };

            context.BordroAyarlar.Add(ayar);
            await context.SaveChangesAsync();
        }

        return ayar;
    }

    public async Task SaveBordroAyarAsync(BordroAyar ayar)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var mevcutAyar = await context.BordroAyarlar
            .FirstOrDefaultAsync(a => a.FirmaId == ayar.FirmaId);

        if (mevcutAyar != null)
        {
            mevcutAyar.PersonelMaasHesapKodu = ayar.PersonelMaasHesapKodu;
            mevcutAyar.SgkPrimHesapKodu = ayar.SgkPrimHesapKodu;
            mevcutAyar.GelirVergisiHesapKodu = ayar.GelirVergisiHesapKodu;
            mevcutAyar.KasaHesapKodu = ayar.KasaHesapKodu;
            mevcutAyar.BankaHesapKodu = ayar.BankaHesapKodu;
            mevcutAyar.PersonelAvansHesapKodu = ayar.PersonelAvansHesapKodu;
            mevcutAyar.SgkIsciPayiOrani = ayar.SgkIsciPayiOrani;
            mevcutAyar.IssizlikIsciPayiOrani = ayar.IssizlikIsciPayiOrani;
            mevcutAyar.DamgaVergisiOrani = ayar.DamgaVergisiOrani;
            mevcutAyar.ArgeSgkIsverenDestekVarMi = ayar.ArgeSgkIsverenDestekVarMi;
            mevcutAyar.ArgeSgkIsverenDestekOrani = ayar.ArgeSgkIsverenDestekOrani;
        }
        else
        {
            context.BordroAyarlar.Add(ayar);
        }

        await context.SaveChangesAsync();
    }

    #endregion

    #region Özet Bilgiler

    public async Task<BordroOzet> GetBordroOzetAsync(int? firmaId, int? yil, int? ay)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var query = context.BordroDetaylar
            .Include(d => d.Bordro)
            .Include(d => d.Personel)
            .AsQueryable();

        if (firmaId.HasValue)
            query = query.Where(d => d.FirmaId == firmaId);

        if (yil.HasValue)
            query = query.Where(d => d.Bordro.Yil == yil);

        if (ay.HasValue)
            query = query.Where(d => d.Bordro.Ay == ay);

        var detaylar = await query.ToListAsync();
        var bordrolar = await GetBordrolarAsync(firmaId, yil);

        return new BordroOzet
        {
            ToplamPersonel = detaylar.Select(d => d.PersonelId).Distinct().Count(),
            NormalPersonel = detaylar.Where(d => !d.Personel.ArgePersoneli).Select(d => d.PersonelId).Distinct().Count(),
            ArgePersonel = detaylar.Where(d => d.Personel.ArgePersoneli).Select(d => d.PersonelId).Distinct().Count(),
            ToplamBrutMaas = detaylar.Sum(d => d.BrutMaas),
            ToplamNetMaas = detaylar.Sum(d => d.NetMaas),
            ToplamSgkMaasi = detaylar.Sum(d => d.SgkMaasi),
            ToplamEkOdeme = detaylar.Sum(d => d.EkOdeme),
            OnayliDönemSayisi = bordrolar.Count(b => b.Onaylandi),
            BekleyenDönemSayisi = bordrolar.Count(b => !b.Onaylandi)
        };
    }

    #endregion
}
