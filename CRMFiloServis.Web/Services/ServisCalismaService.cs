using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class ServisCalismaService : IServisCalismaService
{
    private readonly ApplicationDbContext _context;

    public ServisCalismaService(ApplicationDbContext context)
    {
        _context = context;
    }

    private IQueryable<ServisCalisma> CreateReadQuery(bool includeArizaMasraflari = false)
    {
        IQueryable<ServisCalisma> query = _context.ServisCalismalari
            .AsNoTracking()
            .Where(s => !s.IsDeleted);

        query = query.Include(s => s.Arac);
        query = query.Include(s => s.Sofor);
        query = query.Include(s => s.Guzergah)
            .ThenInclude(g => g.Cari);

        if (includeArizaMasraflari)
        {
            query = query.Include(s => s.ArizaMasraflari);
        }

        return query;
    }

    public async Task<List<ServisCalisma>> GetAllAsync()
    {
        return await CreateReadQuery()
            .OrderByDescending(s => s.CalismaTarihi)
            .ToListAsync();
    }

    public async Task<List<ServisCalisma>> GetRecentAsync(int count = 5)
    {
        return await CreateReadQuery()
            .OrderByDescending(s => s.CalismaTarihi)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<ServisCalisma>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await CreateReadQuery()
            .Where(s => s.CalismaTarihi >= startDate && s.CalismaTarihi <= endDate)
            .OrderByDescending(s => s.CalismaTarihi)
            .ToListAsync();
    }

    public async Task<List<ServisCalisma>> GetByAracIdAsync(int aracId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = CreateReadQuery()
            .Where(s => s.AracId == aracId);

        if (startDate.HasValue)
            query = query.Where(s => s.CalismaTarihi >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(s => s.CalismaTarihi <= endDate.Value);

        return await query.OrderByDescending(s => s.CalismaTarihi).ToListAsync();
    }

    public async Task<List<ServisCalisma>> GetBySoforIdAsync(int soforId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = CreateReadQuery()
            .Where(s => s.SoforId == soforId);

        if (startDate.HasValue)
            query = query.Where(s => s.CalismaTarihi >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(s => s.CalismaTarihi <= endDate.Value);

        return await query.OrderByDescending(s => s.CalismaTarihi).ToListAsync();
    }

    public async Task<List<ServisCalisma>> GetByGuzergahIdAsync(int guzergahId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = CreateReadQuery()
            .Where(s => s.GuzergahId == guzergahId);

        if (startDate.HasValue)
            query = query.Where(s => s.CalismaTarihi >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(s => s.CalismaTarihi <= endDate.Value);

        return await query.OrderByDescending(s => s.CalismaTarihi).ToListAsync();
    }

    public async Task<List<ServisCalisma>> GetByCariIdAsync(int cariId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = CreateReadQuery()
            .Where(s => s.Guzergah.CariId == cariId);

        if (startDate.HasValue)
            query = query.Where(s => s.CalismaTarihi >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(s => s.CalismaTarihi <= endDate.Value);

        return await query.OrderByDescending(s => s.CalismaTarihi).ToListAsync();
    }

    public async Task<ServisCalisma?> GetByIdAsync(int id)
    {
        return await CreateReadQuery(includeArizaMasraflari: true)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<ServisCalisma> CreateAsync(ServisCalisma servisCalisma)
    {
        // Güzergah fiyatını al
        if (!servisCalisma.Fiyat.HasValue)
        {
            var guzergah = await _context.Guzergahlar.FindAsync(servisCalisma.GuzergahId);
            if (guzergah != null)
            {
                servisCalisma.Fiyat = guzergah.BirimFiyat;
            }
        }

        _context.ServisCalismalari.Add(servisCalisma);
        await _context.SaveChangesAsync();
        return servisCalisma;
    }

    public async Task<ServisCalisma> UpdateAsync(ServisCalisma servisCalisma)
    {
        _context.ServisCalismalari.Update(servisCalisma);
        await _context.SaveChangesAsync();
        return servisCalisma;
    }

    public async Task DeleteAsync(int id)
    {
        var servisCalisma = await _context.ServisCalismalari.FindAsync(id);
        if (servisCalisma != null)
        {
            servisCalisma.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<ServisCalisma>> FilterAsync(
        DateTime startDate,
        DateTime endDate,
        int? aracId = null,
        int? soforId = null,
        int? guzergahId = null,
        int? cariId = null)
    {
        var query = CreateReadQuery()
            .Where(s => s.CalismaTarihi >= startDate && s.CalismaTarihi <= endDate);

        if (aracId.HasValue)
            query = query.Where(s => s.AracId == aracId.Value);

        if (soforId.HasValue)
            query = query.Where(s => s.SoforId == soforId.Value);

        if (guzergahId.HasValue)
            query = query.Where(s => s.GuzergahId == guzergahId.Value);

        if (cariId.HasValue)
            query = query.Where(s => s.Guzergah.CariId == cariId.Value);

        return await query.OrderByDescending(s => s.CalismaTarihi).ToListAsync();
    }
}

