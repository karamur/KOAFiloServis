using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using CRMFiloServis.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class WhatsAppService : IWhatsAppService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<WhatsAppService> _logger;
    private readonly ICRMService _crmService;

    public WhatsAppService(ApplicationDbContext context, ILogger<WhatsAppService> logger, ICRMService crmService)
    {
        _context = context;
        _logger = logger;
        _crmService = crmService;
    }

    #region Kiþiler

    public async Task<List<WhatsAppKisi>> GetKisilerAsync()
    {
        return await _context.WhatsAppKisiler
            .Include(k => k.Cari)
            .OrderBy(k => k.AdSoyad)
            .ToListAsync();
    }

    public async Task<WhatsAppKisi?> GetKisiByIdAsync(int id)
    {
        return await _context.WhatsAppKisiler
            .Include(k => k.Gruplari)
            .ThenInclude(gu => gu.Grup)
            .FirstOrDefaultAsync(k => k.Id == id);
    }

    public async Task<WhatsAppKisi> CreateKisiAsync(WhatsAppKisi kisi)
    {
        var mevcut = await _context.WhatsAppKisiler.FirstOrDefaultAsync(k => k.Telefon == kisi.Telefon);
        if (mevcut != null)
            throw new Exception("Bu telefon numarasý ile kayýtlý bir kiþi zaten var.");

        _context.WhatsAppKisiler.Add(kisi);
        await _context.SaveChangesAsync();
        return kisi;
    }

    public async Task<WhatsAppKisi> UpdateKisiAsync(WhatsAppKisi kisi)
    {
        _context.WhatsAppKisiler.Update(kisi);
        await _context.SaveChangesAsync();
        return kisi;
    }

    public async Task DeleteKisiAsync(int id)
    {
        var kisi = await _context.WhatsAppKisiler.FindAsync(id);
        if (kisi != null)
        {
            kisi.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task SeciliCarilerdenKisiOlustur(List<int> cariIds)
    {
        var cariler = await _context.Cariler.Where(c => cariIds.Contains(c.Id)).ToListAsync();
        foreach (var cari in cariler)
        {
            if (string.IsNullOrWhiteSpace(cari.Telefon)) continue;

            var mevcut = await _context.WhatsAppKisiler.FirstOrDefaultAsync(k => k.Telefon == cari.Telefon);
            if (mevcut == null)
            {
                _context.WhatsAppKisiler.Add(new WhatsAppKisi
                {
                    AdSoyad = cari.Unvan,
                    Telefon = cari.Telefon,
                    CariId = cari.Id
                });
            }
        }
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Gruplar

    public async Task<List<WhatsAppGrup>> GetGruplarAsync()
    {
        return await _context.WhatsAppGruplar
            .Include(g => g.Uyeler)
            .OrderBy(g => g.GrupAdi)
            .ToListAsync();
    }

    public async Task<WhatsAppGrup?> GetGrupByIdAsync(int id)
    {
        return await _context.WhatsAppGruplar
            .Include(g => g.Uyeler)
            .ThenInclude(gu => gu.Kisi)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<WhatsAppGrup> CreateGrupAsync(WhatsAppGrup grup)
    {
        _context.WhatsAppGruplar.Add(grup);
        await _context.SaveChangesAsync();
        return grup;
    }

    public async Task<WhatsAppGrup> UpdateGrupAsync(WhatsAppGrup grup)
    {
        _context.WhatsAppGruplar.Update(grup);
        await _context.SaveChangesAsync();
        return grup;
    }

    public async Task DeleteGrupAsync(int id)
    {
        var grup = await _context.WhatsAppGruplar.FindAsync(id);
        if (grup != null)
        {
            grup.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task GrubaKisiEkleAsync(int grupId, int kisiId)
    {
        var mevcut = await _context.WhatsAppGrupUyeler.AnyAsync(gu => gu.GrupId == grupId && gu.KisiId == kisiId);
        if (!mevcut)
        {
            _context.WhatsAppGrupUyeler.Add(new WhatsAppGrupUye { GrupId = grupId, KisiId = kisiId });
            await _context.SaveChangesAsync();
        }
    }

    public async Task GruptanKisiCikarAsync(int grupId, int kisiId)
    {
        var uye = await _context.WhatsAppGrupUyeler.FirstOrDefaultAsync(gu => gu.GrupId == grupId && gu.KisiId == kisiId);
        if (uye != null)
        {
            _context.WhatsAppGrupUyeler.Remove(uye);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Sablonlar

    public async Task<List<WhatsAppSablon>> GetSablonlarAsync()
    {
        return await _context.WhatsAppSablonlar.OrderBy(s => s.Baslik).ToListAsync();
    }

    public async Task<WhatsAppSablon?> GetSablonByIdAsync(int id)
    {
        return await _context.WhatsAppSablonlar.FindAsync(id);
    }

    public async Task<WhatsAppSablon> CreateSablonAsync(WhatsAppSablon sablon)
    {
        _context.WhatsAppSablonlar.Add(sablon);
        await _context.SaveChangesAsync();
        return sablon;
    }

    public async Task<WhatsAppSablon> UpdateSablonAsync(WhatsAppSablon sablon)
    {
        _context.WhatsAppSablonlar.Update(sablon);
        await _context.SaveChangesAsync();
        return sablon;
    }

    public async Task DeleteSablonAsync(int id)
    {
        var sablon = await _context.WhatsAppSablonlar.FindAsync(id);
        if (sablon != null)
        {
            sablon.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Mesajlar

    public async Task<List<WhatsAppMesaj>> GetMesajlarByKisiAsync(int kisiId)
    {
        return await _context.WhatsAppMesajlar
            .Include(m => m.Gonderen)
            .Where(m => m.KisiId == kisiId)
            .OrderBy(m => m.MesajTarihi)
            .ToListAsync();
    }

    public async Task<List<WhatsAppMesaj>> GetMesajlarByGrupAsync(int grupId)
    {
        return await _context.WhatsAppMesajlar
            .Include(m => m.Gonderen)
            .Where(m => m.GrupId == grupId)
            .OrderBy(m => m.MesajTarihi)
            .ToListAsync();
    }

    public async Task<WhatsAppMesaj> SendMesajToKisiAsync(int kisiId, string icerik, int? gonderenId = null)
    {
        var kisi = await _context.WhatsAppKisiler.FindAsync(kisiId);
        if (kisi == null) throw new Exception("Kiþi bulunamadý.");

        var mesaj = new WhatsAppMesaj
        {
            KisiId = kisiId,
            Icerik = icerik,
            Tipi = WhatsAppMesaj.Yon.Giden,
            GonderenId = gonderenId,
            MesajTarihi = DateTime.UtcNow,
            Durum = MesajDurum.Gonderildi,
            Okundu = true
        };

        _context.WhatsAppMesajlar.Add(mesaj);
        await _context.SaveChangesAsync();

        // Arka planda gerçek gönderim yapýlabilir
        await _crmService.SendWhatsAppAsync(gonderenId ?? 0, kisi.Telefon, icerik);

        return mesaj;
    }

    public async Task<WhatsAppMesaj> SendMesajToGrupAsync(int grupId, string icerik, int? gonderenId = null)
    {
        var grup = await _context.WhatsAppGruplar
            .Include(g => g.Uyeler).ThenInclude(gu => gu.Kisi)
            .FirstOrDefaultAsync(g => g.Id == grupId);

        if (grup == null) throw new Exception("Grup bulunamadý.");

        var mesaj = new WhatsAppMesaj
        {
            GrupId = grupId,
            Icerik = icerik,
            Tipi = WhatsAppMesaj.Yon.Giden,
            GonderenId = gonderenId,
            MesajTarihi = DateTime.UtcNow,
            Durum = MesajDurum.Gonderildi,
            Okundu = true
        };

        _context.WhatsAppMesajlar.Add(mesaj);
        await _context.SaveChangesAsync();

        // Her üyeye mesajý gönder
        foreach (var uye in grup.Uyeler)
        {
            await _crmService.SendWhatsAppAsync(gonderenId ?? 0, uye.Kisi.Telefon, icerik);
        }

        return mesaj;
    }

    public async Task<int> GetOkunmamisMesajSayisiAsync()
    {
        return await _context.WhatsAppMesajlar
            .CountAsync(m => m.Tipi == WhatsAppMesaj.Yon.Gelen && !m.Okundu);
    }

    public async Task MesajlariOkunduIsaretleAsync(int? kisiId = null, int? grupId = null)
    {
        var query = _context.WhatsAppMesajlar.Where(m => m.Tipi == WhatsAppMesaj.Yon.Gelen && !m.Okundu);

        if (kisiId.HasValue)
            query = query.Where(m => m.KisiId == kisiId.Value);

        if (grupId.HasValue)
            query = query.Where(m => m.GrupId == grupId.Value);

        await query.ExecuteUpdateAsync(s => s.SetProperty(m => m.Okundu, true));
    }

    #endregion
}