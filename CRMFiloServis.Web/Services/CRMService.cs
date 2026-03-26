using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public interface ICRMService
{
    // Bildirimler
    Task<List<Bildirim>> GetBildirimlerAsync(int kullaniciId, bool sadeceokunmamis = false);
    Task<int> GetOkunmamisBildirimSayisiAsync(int kullaniciId);
    Task<Bildirim> CreateBildirimAsync(Bildirim bildirim);
    Task BildirimOkunduIsaretle(int bildirimId);
    Task TumBildirimleriOkunduIsaretle(int kullaniciId);
    Task DeleteBildirimAsync(int bildirimId);

    // Mesajlar
    Task<List<Mesaj>> GetGelenMesajlarAsync(int kullaniciId);
    Task<List<Mesaj>> GetGonderilenMesajlarAsync(int kullaniciId);
    Task<int> GetOkunmamisMesajSayisiAsync(int kullaniciId);
    Task<Mesaj> SendMesajAsync(Mesaj mesaj);
    Task MesajOkunduIsaretle(int mesajId);
    Task DeleteMesajAsync(int mesajId);

    // Email
    Task<EmailAyar?> GetEmailAyarAsync(int? kullaniciId = null);
    Task<EmailAyar> SaveEmailAyarAsync(EmailAyar ayar);
    Task<bool> SendEmailAsync(int gonderenId, string aliciEmail, string konu, string icerik);

    // WhatsApp
    Task<WhatsAppAyar?> GetWhatsAppAyarAsync(int? kullaniciId = null);
    Task<WhatsAppAyar> SaveWhatsAppAyarAsync(WhatsAppAyar ayar);
    Task<bool> SendWhatsAppAsync(int gonderenId, string telefon, string mesaj);

    // Hatýrlatýcýlar
    Task<List<Hatirlatici>> GetHatirlaticilarAsync(int kullaniciId, DateTime? baslangic = null, DateTime? bitis = null);
    Task<List<Hatirlatici>> GetBugunkuHatirlaticilarAsync(int kullaniciId);
    Task<Hatirlatici> CreateHatirlaticiAsync(Hatirlatici hatirlatici);
    Task<Hatirlatici> UpdateHatirlaticiAsync(Hatirlatici hatirlatici);
    Task DeleteHatirlaticiAsync(int hatirlaticiId);
    Task HatirlaticiTamamlaAsync(int hatirlaticiId);

    // Kullanýcý-Cari Eþleþtirme
    Task<List<KullaniciCari>> GetKullaniciBagliCarilerAsync(int kullaniciId);
    Task<KullaniciCari> AddKullaniciCariAsync(KullaniciCari kullaniciCari);
    Task<KullaniciCari> UpdateKullaniciCariAsync(KullaniciCari kullaniciCari);
    Task DeleteKullaniciCariAsync(int id);
    Task<bool> KullaniciBuCariyeErisebilirMi(int kullaniciId, int cariId);

    // Dashboard Widget
    Task<List<DashboardWidget>> GetDashboardWidgetlarAsync(int kullaniciId);
    Task SaveDashboardWidgetlarAsync(int kullaniciId, List<DashboardWidget> widgets);
}

public class CRMService : ICRMService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CRMService> _logger;

    public CRMService(ApplicationDbContext context, ILogger<CRMService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Bildirimler

    public async Task<List<Bildirim>> GetBildirimlerAsync(int kullaniciId, bool sadeceokunmamis = false)
    {
        var query = _context.Bildirimler
            .Where(b => b.KullaniciId == kullaniciId);

        if (sadeceokunmamis)
            query = query.Where(b => !b.Okundu);

        return await query
            .OrderByDescending(b => b.CreatedAt)
            .Take(100)
            .ToListAsync();
    }

    public async Task<int> GetOkunmamisBildirimSayisiAsync(int kullaniciId)
    {
        return await _context.Bildirimler
            .CountAsync(b => b.KullaniciId == kullaniciId && !b.Okundu);
    }

    public async Task<Bildirim> CreateBildirimAsync(Bildirim bildirim)
    {
        _context.Bildirimler.Add(bildirim);
        await _context.SaveChangesAsync();
        return bildirim;
    }

    public async Task BildirimOkunduIsaretle(int bildirimId)
    {
        var bildirim = await _context.Bildirimler.FindAsync(bildirimId);
        if (bildirim != null)
        {
            bildirim.Okundu = true;
            bildirim.OkunmaTarihi = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task TumBildirimleriOkunduIsaretle(int kullaniciId)
    {
        await _context.Bildirimler
            .Where(b => b.KullaniciId == kullaniciId && !b.Okundu)
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.Okundu, true)
                .SetProperty(b => b.OkunmaTarihi, DateTime.UtcNow));
    }

    public async Task DeleteBildirimAsync(int bildirimId)
    {
        var bildirim = await _context.Bildirimler.FindAsync(bildirimId);
        if (bildirim != null)
        {
            bildirim.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Mesajlar

    public async Task<List<Mesaj>> GetGelenMesajlarAsync(int kullaniciId)
    {
        return await _context.Mesajlar
            .Include(m => m.Gonderen)
            .Where(m => m.AliciId == kullaniciId || m.AliciId == null)
            .OrderByDescending(m => m.CreatedAt)
            .Take(100)
            .ToListAsync();
    }

    public async Task<List<Mesaj>> GetGonderilenMesajlarAsync(int kullaniciId)
    {
        return await _context.Mesajlar
            .Include(m => m.Alici)
            .Where(m => m.GonderenId == kullaniciId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(100)
            .ToListAsync();
    }

    public async Task<int> GetOkunmamisMesajSayisiAsync(int kullaniciId)
    {
        return await _context.Mesajlar
            .CountAsync(m => (m.AliciId == kullaniciId || m.AliciId == null) && !m.Okundu);
    }

    public async Task<Mesaj> SendMesajAsync(Mesaj mesaj)
    {
        mesaj.Durum = MesajDurum.Gonderildi;
        _context.Mesajlar.Add(mesaj);
        await _context.SaveChangesAsync();

        // Alýcýya bildirim oluþtur
        if (mesaj.AliciId.HasValue)
        {
            var gonderen = await _context.Kullanicilar.FindAsync(mesaj.GonderenId);
            await CreateBildirimAsync(new Bildirim
            {
                KullaniciId = mesaj.AliciId.Value,
                Baslik = $"Yeni mesaj: {mesaj.Konu}",
                Icerik = $"{gonderen?.AdSoyad ?? "Bilinmeyen"} size bir mesaj gönderdi.",
                Tip = BildirimTipi.Mesaj,
                Link = "/crm/mesajlar"
            });
        }

        return mesaj;
    }

    public async Task MesajOkunduIsaretle(int mesajId)
    {
        var mesaj = await _context.Mesajlar.FindAsync(mesajId);
        if (mesaj != null)
        {
            mesaj.Okundu = true;
            mesaj.OkunmaTarihi = DateTime.UtcNow;
            mesaj.Durum = MesajDurum.Okundu;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteMesajAsync(int mesajId)
    {
        var mesaj = await _context.Mesajlar.FindAsync(mesajId);
        if (mesaj != null)
        {
            mesaj.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Email

    public async Task<EmailAyar?> GetEmailAyarAsync(int? kullaniciId = null)
    {
        if (kullaniciId.HasValue)
        {
            return await _context.EmailAyarlari
                .FirstOrDefaultAsync(e => e.KullaniciId == kullaniciId && e.Aktif);
        }

        return await _context.EmailAyarlari
            .FirstOrDefaultAsync(e => e.KullaniciId == null && e.Aktif);
    }

    public async Task<EmailAyar> SaveEmailAyarAsync(EmailAyar ayar)
    {
        if (ayar.Id == 0)
            _context.EmailAyarlari.Add(ayar);
        else
            _context.EmailAyarlari.Update(ayar);

        await _context.SaveChangesAsync();
        return ayar;
    }

    public async Task<bool> SendEmailAsync(int gonderenId, string aliciEmail, string konu, string icerik)
    {
        try
        {
            var ayar = await GetEmailAyarAsync(gonderenId) ?? await GetEmailAyarAsync();
            if (ayar == null)
            {
                _logger.LogWarning("Email ayarlarý bulunamadý");
                return false;
            }

            // TODO: Email gönderme implementasyonu
            // MailKit veya System.Net.Mail ile email gönder

            // Mesaj kaydý oluþtur
            await SendMesajAsync(new Mesaj
            {
                GonderenId = gonderenId,
                Konu = konu,
                Icerik = icerik,
                Tip = MesajTipi.Email,
                DisAlici = aliciEmail,
                Durum = MesajDurum.Gonderildi
            });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email gönderilirken hata");
            return false;
        }
    }

    #endregion

    #region WhatsApp

    public async Task<WhatsAppAyar?> GetWhatsAppAyarAsync(int? kullaniciId = null)
    {
        if (kullaniciId.HasValue)
        {
            return await _context.WhatsAppAyarlari
                .FirstOrDefaultAsync(e => e.KullaniciId == kullaniciId && e.Aktif);
        }

        return await _context.WhatsAppAyarlari
            .FirstOrDefaultAsync(e => e.KullaniciId == null && e.Aktif);
    }

    public async Task<WhatsAppAyar> SaveWhatsAppAyarAsync(WhatsAppAyar ayar)
    {
        if (ayar.Id == 0)
            _context.WhatsAppAyarlari.Add(ayar);
        else
            _context.WhatsAppAyarlari.Update(ayar);

        await _context.SaveChangesAsync();
        return ayar;
    }

    public async Task<bool> SendWhatsAppAsync(int gonderenId, string telefon, string mesaj)
    {
        try
        {
            var ayar = await GetWhatsAppAyarAsync(gonderenId) ?? await GetWhatsAppAyarAsync();
            if (ayar == null || string.IsNullOrEmpty(ayar.ApiKey))
            {
                _logger.LogWarning("WhatsApp ayarlarý bulunamadý");
                return false;
            }

            // TODO: WhatsApp API implementasyonu
            // Twilio veya WhatsApp Business API ile mesaj gönder

            // Mesaj kaydý oluþtur
            await SendMesajAsync(new Mesaj
            {
                GonderenId = gonderenId,
                Konu = "WhatsApp Mesajý",
                Icerik = mesaj,
                Tip = MesajTipi.WhatsApp,
                DisAlici = telefon,
                Durum = MesajDurum.Gonderildi
            });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WhatsApp mesajý gönderilirken hata");
            return false;
        }
    }

    #endregion

    #region Hatýrlatýcýlar

    public async Task<List<Hatirlatici>> GetHatirlaticilarAsync(int kullaniciId, DateTime? baslangic = null, DateTime? bitis = null)
    {
        var query = _context.Hatirlaticilar
            .Include(h => h.Cari)
            .Where(h => h.KullaniciId == kullaniciId);

        if (baslangic.HasValue)
            query = query.Where(h => h.BaslangicTarihi >= baslangic.Value);

        if (bitis.HasValue)
            query = query.Where(h => h.BaslangicTarihi <= bitis.Value);

        return await query
            .OrderBy(h => h.BaslangicTarihi)
            .ToListAsync();
    }

    public async Task<List<Hatirlatici>> GetBugunkuHatirlaticilarAsync(int kullaniciId)
    {
        var bugun = DateTime.UtcNow.Date;
        var yarin = bugun.AddDays(1);

        return await _context.Hatirlaticilar
            .Include(h => h.Cari)
            .Where(h => h.KullaniciId == kullaniciId 
                && h.BaslangicTarihi >= bugun 
                && h.BaslangicTarihi < yarin
                && h.Durum == HatirlaticiDurum.Bekliyor)
            .OrderBy(h => h.BaslangicTarihi)
            .ToListAsync();
    }

    public async Task<Hatirlatici> CreateHatirlaticiAsync(Hatirlatici hatirlatici)
    {
        _context.Hatirlaticilar.Add(hatirlatici);
        await _context.SaveChangesAsync();
        return hatirlatici;
    }

    public async Task<Hatirlatici> UpdateHatirlaticiAsync(Hatirlatici hatirlatici)
    {
        _context.Hatirlaticilar.Update(hatirlatici);
        await _context.SaveChangesAsync();
        return hatirlatici;
    }

    public async Task DeleteHatirlaticiAsync(int hatirlaticiId)
    {
        var hatirlatici = await _context.Hatirlaticilar.FindAsync(hatirlaticiId);
        if (hatirlatici != null)
        {
            hatirlatici.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task HatirlaticiTamamlaAsync(int hatirlaticiId)
    {
        var hatirlatici = await _context.Hatirlaticilar.FindAsync(hatirlaticiId);
        if (hatirlatici != null)
        {
            hatirlatici.Durum = HatirlaticiDurum.Tamamlandi;
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Kullanýcý-Cari Eþleþtirme

    public async Task<List<KullaniciCari>> GetKullaniciBagliCarilerAsync(int kullaniciId)
    {
        return await _context.KullaniciCariler
            .Include(kc => kc.Cari)
            .Where(kc => kc.KullaniciId == kullaniciId)
            .OrderBy(kc => kc.Cari.Unvan)
            .ToListAsync();
    }

    public async Task<KullaniciCari> AddKullaniciCariAsync(KullaniciCari kullaniciCari)
    {
        // Coka-cok iliski oldugu icin duplicate kontrolu KALDIRILDI
        // Ayni kullanici-cari cifti birden fazla kez eklenebilir (farkli izinlerle)
        _context.KullaniciCariler.Add(kullaniciCari);
        await _context.SaveChangesAsync();
        return kullaniciCari;
    }

    public async Task<KullaniciCari> UpdateKullaniciCariAsync(KullaniciCari kullaniciCari)
    {
        _context.KullaniciCariler.Update(kullaniciCari);
        await _context.SaveChangesAsync();
        return kullaniciCari;
    }

    public async Task DeleteKullaniciCariAsync(int id)
    {
        var kullaniciCari = await _context.KullaniciCariler.FindAsync(id);
        if (kullaniciCari != null)
        {
            kullaniciCari.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> KullaniciBuCariyeErisebilirMi(int kullaniciId, int cariId)
    {
        // Admin her cariye eriþebilir
        var kullanici = await _context.Kullanicilar
            .Include(k => k.Rol)
            .FirstOrDefaultAsync(k => k.Id == kullaniciId);

        if (kullanici?.Rol?.RolAdi == "Admin")
            return true;

        // Kullanýcýnýn baðlý carileri kontrol et
        return await _context.KullaniciCariler
            .AnyAsync(kc => kc.KullaniciId == kullaniciId && kc.CariId == cariId);
    }

    #endregion

    #region Dashboard Widget

    public async Task<List<DashboardWidget>> GetDashboardWidgetlarAsync(int kullaniciId)
    {
        var widgets = await _context.DashboardWidgetlar
            .Where(w => w.KullaniciId == kullaniciId)
            .OrderBy(w => w.Sira)
            .ToListAsync();

        // Varsayýlan widget'larý yoksa oluþtur
        if (!widgets.Any())
        {
            widgets = GetVarsayilanWidgetlar(kullaniciId);
            _context.DashboardWidgetlar.AddRange(widgets);
            await _context.SaveChangesAsync();
        }

        return widgets;
    }

    public async Task SaveDashboardWidgetlarAsync(int kullaniciId, List<DashboardWidget> widgets)
    {
        var mevcutlar = await _context.DashboardWidgetlar
            .Where(w => w.KullaniciId == kullaniciId)
            .ToListAsync();

        _context.DashboardWidgetlar.RemoveRange(mevcutlar);

        foreach (var widget in widgets)
        {
            widget.KullaniciId = kullaniciId;
            _context.DashboardWidgetlar.Add(widget);
        }

        await _context.SaveChangesAsync();
    }

    private List<DashboardWidget> GetVarsayilanWidgetlar(int kullaniciId)
    {
        return new List<DashboardWidget>
        {
            new() { KullaniciId = kullaniciId, WidgetKodu = "bildirimler", Sira = 0, Kolon = 0, Genislik = 4, Gorunur = true },
            new() { KullaniciId = kullaniciId, WidgetKodu = "mesajlar", Sira = 1, Kolon = 4, Genislik = 4, Gorunur = true },
            new() { KullaniciId = kullaniciId, WidgetKodu = "randevular", Sira = 2, Kolon = 8, Genislik = 4, Gorunur = true },
            new() { KullaniciId = kullaniciId, WidgetKodu = "belgeler", Sira = 3, Kolon = 0, Genislik = 6, Gorunur = true },
            new() { KullaniciId = kullaniciId, WidgetKodu = "odemeler", Sira = 4, Kolon = 6, Genislik = 6, Gorunur = true },
        };
    }

    #endregion
}
