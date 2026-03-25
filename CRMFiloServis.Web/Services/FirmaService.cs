using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class FirmaService : IFirmaService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private static AktifFirmaBilgisi _aktifFirma = new();
    private static readonly object _lock = new();

    public FirmaService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    #region CRUD

    public async Task<List<Firma>> GetAllAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Firmalar
            .OrderBy(f => f.SiraNo)
            .ThenBy(f => f.FirmaAdi)
            .ToListAsync();
    }

    public async Task<List<Firma>> GetAktifFirmalarAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Firmalar
            .Where(f => f.Aktif)
            .OrderBy(f => f.SiraNo)
            .ThenBy(f => f.FirmaAdi)
            .ToListAsync();
    }

    public async Task<Firma?> GetByIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Firmalar.FindAsync(id);
    }

    public async Task<Firma?> GetVarsayilanFirmaAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Firmalar
            .FirstOrDefaultAsync(f => f.VarsayilanFirma && f.Aktif);
    }

    public async Task<Firma> CreateAsync(Firma firma)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        firma.CreatedAt = DateTime.UtcNow;
        context.Firmalar.Add(firma);
        await context.SaveChangesAsync();
        return firma;
    }

    public async Task<Firma> UpdateAsync(Firma firma)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        // Entity'yi attach et ve modified olarak isaretle
        context.Firmalar.Attach(firma);
        context.Entry(firma).State = EntityState.Modified;
        
        firma.UpdatedAt = DateTime.UtcNow;
        
        await context.SaveChangesAsync();
        return firma;
    }

    public async Task DeleteAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var firma = await context.Firmalar.FindAsync(id);
        if (firma == null) return;

        firma.IsDeleted = true;
        firma.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }

    public async Task SetVarsayilanAsync(int firmaId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        // Onceki varsayilani kaldir
        var eskiVarsayilan = await context.Firmalar.Where(f => f.VarsayilanFirma).ToListAsync();
        foreach (var f in eskiVarsayilan)
        {
            f.VarsayilanFirma = false;
        }

        // Yeni varsayilan
        var firma = await context.Firmalar.FindAsync(firmaId);
        if (firma != null)
        {
            firma.VarsayilanFirma = true;
        }

        await context.SaveChangesAsync();
    }

    #endregion

    #region Aktif Firma Yonetimi

    public AktifFirmaBilgisi GetAktifFirma()
    {
        lock (_lock)
        {
            // Eger aktif firma secilmemisse, varsayilan firmayi getir
            if (_aktifFirma.FirmaId == 0)
            {
                using var context = _contextFactory.CreateDbContext();
                var varsayilan = context.Firmalar.FirstOrDefault(f => f.VarsayilanFirma && f.Aktif);
                if (varsayilan != null)
                {
                    _aktifFirma = new AktifFirmaBilgisi
                    {
                        FirmaId = varsayilan.Id,
                        FirmaKodu = varsayilan.FirmaKodu,
                        FirmaAdi = varsayilan.FirmaAdi,
                        AktifDonemYil = varsayilan.AktifDonemYil,
                        AktifDonemAy = varsayilan.AktifDonemAy,
                        TumFirmalar = false
                    };
                }
                else
                {
                    // Hic firma yoksa default degerler
                    _aktifFirma = new AktifFirmaBilgisi
                    {
                        FirmaId = 0,
                        FirmaKodu = "VARSAYILAN",
                        FirmaAdi = "Varsayilan Firma",
                        AktifDonemYil = DateTime.Today.Year,
                        AktifDonemAy = DateTime.Today.Month,
                        TumFirmalar = true
                    };
                }
            }
            return _aktifFirma;
        }
    }

    public void SetAktifFirma(int firmaId)
    {
        lock (_lock)
        {
            using var context = _contextFactory.CreateDbContext();
            var firma = context.Firmalar.Find(firmaId);
            if (firma != null)
            {
                _aktifFirma = new AktifFirmaBilgisi
                {
                    FirmaId = firma.Id,
                    FirmaKodu = firma.FirmaKodu,
                    FirmaAdi = firma.FirmaAdi,
                    AktifDonemYil = firma.AktifDonemYil,
                    AktifDonemAy = firma.AktifDonemAy,
                    TumFirmalar = false
                };
            }
        }
    }

    public void SetAktifFirma(AktifFirmaBilgisi firma)
    {
        lock (_lock)
        {
            _aktifFirma = firma;
        }
    }

    public void SetTumFirmalar(bool tumFirmalar)
    {
        lock (_lock)
        {
            _aktifFirma.TumFirmalar = tumFirmalar;
        }
    }

    public void SetAktifDonem(int yil, int ay)
    {
        lock (_lock)
        {
            _aktifFirma.AktifDonemYil = yil;
            _aktifFirma.AktifDonemAy = ay;

            // Firmada da guncelle
            if (_aktifFirma.FirmaId > 0)
            {
                using var context = _contextFactory.CreateDbContext();
                var firma = context.Firmalar.Find(_aktifFirma.FirmaId);
                if (firma != null)
                {
                    firma.AktifDonemYil = yil;
                    firma.AktifDonemAy = ay;
                    context.SaveChanges();
                }
            }
        }
    }

    #endregion

    #region Seed

    public async Task SeedVarsayilanFirmaAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        if (await context.Firmalar.AnyAsync()) return;

        var firma = new Firma
        {
            FirmaKodu = "ANA",
            FirmaAdi = "Ana Firma",
            UnvanTam = "Ana Firma Ltd. Sti.",
            Aktif = true,
            VarsayilanFirma = true,
            AktifDonemYil = DateTime.Today.Year,
            AktifDonemAy = DateTime.Today.Month,
            CreatedAt = DateTime.UtcNow
        };

        context.Firmalar.Add(firma);
        await context.SaveChangesAsync();

        // Aktif firma olarak ayarla
        SetAktifFirma(firma.Id);
    }

    #endregion
}
