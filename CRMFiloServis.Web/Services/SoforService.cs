using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class SoforService : ISoforService
{
    private readonly ApplicationDbContext _context;

    public SoforService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Sofor>> GetAllAsync()
    {
        var personeller = await _context.Soforler
            .AsNoTracking()
            .OrderBy(s => s.Ad)
            .ThenBy(s => s.Soyad)
            .ToListAsync();

        personeller.ForEach(NormalizeMaasBilgileri);
        return personeller;
    }

    public async Task<List<Sofor>> GetActiveAsync()
    {
        var personeller = await _context.Soforler
            .AsNoTracking()
            .Where(s => s.Aktif)
            .OrderBy(s => s.Ad)
            .ThenBy(s => s.Soyad)
            .ToListAsync();

        personeller.ForEach(NormalizeMaasBilgileri);
        return personeller;
    }

    public async Task<int> GetActiveCountAsync()
    {
        return await _context.Soforler
            .Where(s => s.Aktif)
            .CountAsync();
    }

    public async Task<Sofor?> GetByIdAsync(int id)
    {
        var sofor = await _context.Soforler
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sofor != null)
            NormalizeMaasBilgileri(sofor);

        return sofor;
    }

    public async Task<Sofor> CreateAsync(Sofor sofor)
    {
        sofor.NetMaas = sofor.ResmiNetMaas + sofor.DigerMaas;
        _context.Soforler.Add(sofor);
        await _context.SaveChangesAsync();
        return sofor;
    }

    public async Task<Sofor> UpdateAsync(Sofor sofor)
    {
        var existing = await _context.Soforler.FindAsync(sofor.Id);
        if (existing == null)
            throw new InvalidOperationException($"Şoför bulunamadı. Id: {sofor.Id}");

        existing.SoforKodu = sofor.SoforKodu;
        existing.Ad = sofor.Ad;
        existing.Soyad = sofor.Soyad;
        existing.TcKimlikNo = sofor.TcKimlikNo;
        existing.Telefon = sofor.Telefon;
        existing.Email = sofor.Email;
        existing.Adres = sofor.Adres;
        existing.Gorev = sofor.Gorev;
        existing.Departman = sofor.Departman;
        existing.Pozisyon = sofor.Pozisyon;
        existing.EhliyetNo = sofor.EhliyetNo;
        existing.EhliyetGecerlilikTarihi = sofor.EhliyetGecerlilikTarihi;
        existing.SrcBelgesiGecerlilikTarihi = sofor.SrcBelgesiGecerlilikTarihi;
        existing.PsikoteknikGecerlilikTarihi = sofor.PsikoteknikGecerlilikTarihi;
        existing.SaglikRaporuGecerlilikTarihi = sofor.SaglikRaporuGecerlilikTarihi;
        existing.IseBaslamaTarihi = sofor.IseBaslamaTarihi;
        existing.IstenAyrilmaTarihi = sofor.IstenAyrilmaTarihi;
        existing.BrutMaas = sofor.BrutMaas;
        existing.ResmiNetMaas = sofor.ResmiNetMaas;
        existing.DigerMaas = sofor.DigerMaas;
        existing.NetMaas = sofor.ResmiNetMaas + sofor.DigerMaas;
        existing.BankaAdi = sofor.BankaAdi;
        existing.IBAN = sofor.IBAN;
        existing.Notlar = sofor.Notlar;
        existing.Aktif = sofor.Aktif;
        existing.IsDeleted = sofor.IsDeleted;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(int id)
    {
        var sofor = await _context.Soforler.FindAsync(id);
        if (sofor != null)
        {
            sofor.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateNextKodAsync()
    {
        var lastSofor = await _context.Soforler
            .IgnoreQueryFilters()
            .OrderByDescending(s => s.Id)
            .FirstOrDefaultAsync();

        var nextNumber = (lastSofor?.Id ?? 0) + 1;
        return $"SFR-{nextNumber:D4}";
    }

    // Görev bazlı filtreleme metodları
    public async Task<List<Sofor>> GetByGorevAsync(PersonelGorev gorev)
    {
        var personeller = await _context.Soforler
            .AsNoTracking()
            .Where(s => s.Gorev == gorev)
            .OrderBy(s => s.Ad)
            .ThenBy(s => s.Soyad)
            .ToListAsync();

        personeller.ForEach(NormalizeMaasBilgileri);
        return personeller;
    }

    public async Task<List<Sofor>> GetActiveSoforlerAsync()
    {
        var personeller = await _context.Soforler
            .AsNoTracking()
            .Where(s => s.Aktif && s.Gorev == PersonelGorev.Sofor)
            .OrderBy(s => s.Ad)
            .ThenBy(s => s.Soyad)
            .ToListAsync();

        personeller.ForEach(NormalizeMaasBilgileri);
        return personeller;
    }

    public async Task<List<Sofor>> GetActiveByGorevAsync(PersonelGorev gorev)
    {
        var personeller = await _context.Soforler
            .AsNoTracking()
            .Where(s => s.Aktif && s.Gorev == gorev)
            .OrderBy(s => s.Ad)
            .ThenBy(s => s.Soyad)
            .ToListAsync();

        personeller.ForEach(NormalizeMaasBilgileri);
        return personeller;
    }

    private static void NormalizeMaasBilgileri(Sofor sofor)
    {
        if (sofor.ResmiNetMaas == 0 && sofor.DigerMaas == 0 && sofor.NetMaas > 0)
        {
            sofor.ResmiNetMaas = sofor.NetMaas;
        }

        sofor.NetMaas = sofor.ResmiNetMaas + sofor.DigerMaas;
    }
}
