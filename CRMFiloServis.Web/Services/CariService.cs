using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class CariService : ICariService
{
    private readonly ApplicationDbContext _context;

    public CariService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Cari>> GetAllAsync()
    {
        return await _context.Cariler
            .Where(c => !c.IsDeleted) // Soft delete filtresi
            .OrderBy(c => c.Unvan)
            .ToListAsync();
    }

    public async Task<List<Cari>> GetAllWithBakiyeAsync()
    {
        var cariler = await _context.Cariler
            .Where(c => !c.IsDeleted) // Soft delete filtresi
            .OrderBy(c => c.Unvan)
            .ToListAsync();

        // Her cari icin borc/alacak hesapla
        foreach (var cari in cariler)
        {
            // Gelen faturalar (Alis) = Borcumuz
            var gelenFaturalar = await _context.Faturalar
                .Where(f => f.CariId == cari.Id && f.FaturaYonu == FaturaYonu.Gelen)
                .SumAsync(f => (decimal?)f.GenelToplam) ?? 0;

            // Giden faturalar (Satis) = Alacagimiz
            var gidenFaturalar = await _context.Faturalar
                .Where(f => f.CariId == cari.Id && f.FaturaYonu == FaturaYonu.Giden)
                .SumAsync(f => (decimal?)f.GenelToplam) ?? 0;

            // Banka hareketlerinden odeme/tahsilat
            var odemeler = await _context.BankaKasaHareketleri
                .Where(h => h.CariId == cari.Id && h.HareketTipi == HareketTipi.Cikis)
                .SumAsync(h => (decimal?)h.Tutar) ?? 0;

            var tahsilatlar = await _context.BankaKasaHareketleri
                .Where(h => h.CariId == cari.Id && h.HareketTipi == HareketTipi.Giris)
                .SumAsync(h => (decimal?)h.Tutar) ?? 0;

            // Musteri icin: Giden fatura = Alacak, Tahsilat = Borc azaltir
            // Tedarikci icin: Gelen fatura = Borc, Odeme = Borc azaltir
            // Personel icin: Maas = Borc, Odeme = Borc azaltir
            if (cari.CariTipi == CariTipi.Musteri)
            {
                cari.Alacak = gidenFaturalar; // Musteriye kesilen fatura (alacak)
                cari.Borc = tahsilatlar; // Musteriden alinan odeme (borc azaltir)
            }
            else if (cari.CariTipi == CariTipi.Tedarikci)
            {
                cari.Borc = gelenFaturalar; // Tedarikci faturasi (borc)
                cari.Alacak = odemeler; // Tedarikci odemesi (alacak)
            }
            else if (cari.CariTipi == CariTipi.Personel)
            {
                // Personel icin: Maas bordrosu = Borc (personele borcumuz)
                // Odeme yapildiginda = Alacak (borcumuz azalir)
                cari.Borc = gelenFaturalar; // Personel maas bordrosu gibi
                cari.Alacak = odemeler; // Personele yapilan odeme
            }
            else // MusteriTedarikci
            {
                cari.Alacak = gidenFaturalar + odemeler;
                cari.Borc = gelenFaturalar + tahsilatlar;
            }
        }

        return cariler;
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.Cariler
            .Where(c => !c.IsDeleted)
            .CountAsync();
    }

    public async Task<Cari?> GetByIdAsync(int id)
    {
        return await _context.Cariler
            .Include(c => c.Guzergahlar)
            .Where(c => !c.IsDeleted)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Cari?> GetByKodAsync(string cariKodu)
    {
        return await _context.Cariler
            .Where(c => !c.IsDeleted)
            .FirstOrDefaultAsync(c => c.CariKodu == cariKodu);
    }

    public async Task<List<Cari>> GetByTipAsync(CariTipi tip)
    {
        return await _context.Cariler
            .Where(c => !c.IsDeleted)
            .Where(c => c.CariTipi == tip || c.CariTipi == CariTipi.MusteriTedarikci)
            .OrderBy(c => c.Unvan)
            .ToListAsync();
    }

    public async Task<Cari> CreateAsync(Cari cari)
    {
        // Muhasebe hesabi otomatik olustur (eger secilmediyse)
        if (!cari.MuhasebeHesapId.HasValue)
        {
            var muhasebeHesap = await CreateMuhasebeHesapAsync(cari);
            if (muhasebeHesap != null)
            {
                cari.MuhasebeHesapId = muhasebeHesap.Id;
            }
        }
        
        cari.IsDeleted = false;
        cari.CreatedAt = DateTime.UtcNow;
        _context.Cariler.Add(cari);
        await _context.SaveChangesAsync();
        return cari;
    }

    public async Task<Cari> UpdateAsync(Cari cari)
    {
        var existing = await _context.Cariler
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == cari.Id && !c.IsDeleted);
            
        if (existing == null) throw new Exception("Cari bulunamadi");

        existing.CariKodu = cari.CariKodu;
        existing.Unvan = cari.Unvan;
        existing.CariTipi = cari.CariTipi;
        existing.VergiDairesi = cari.VergiDairesi;
        existing.VergiNo = cari.VergiNo;
        existing.TcKimlikNo = cari.TcKimlikNo;
        existing.Adres = cari.Adres;
        existing.Telefon = cari.Telefon;
        existing.Email = cari.Email;
        existing.YetkiliKisi = cari.YetkiliKisi;
        existing.Notlar = cari.Notlar;
        existing.Aktif = cari.Aktif;
        existing.MuhasebeHesapId = cari.MuhasebeHesapId;
        existing.FirmaId = cari.FirmaId;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        // IgnoreQueryFilters ile bul
        var cari = await _context.Cariler
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id);
            
        if (cari != null && !cari.IsDeleted)
        {
            cari.IsDeleted = true;
            cari.Aktif = false;
            cari.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<string> GenerateNextKodAsync()
    {
        var lastCari = await _context.Cariler
            .IgnoreQueryFilters()
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync();

        var nextNumber = (lastCari?.Id ?? 0) + 1;
        return $"CRI-{nextNumber:D5}";
    }

    /// <summary>
    /// Cari icin muhasebe hesabi olusturur
    /// Musteri: 120.01.xxx (Alicilar)
    /// Tedarikci: 320.01.xxx (Saticilar)
    /// Personel: 335.XX.PRSXXXXX (Personel Borclari - XX = FirmaId)
    /// </summary>
    private async Task<MuhasebeHesap?> CreateMuhasebeHesapAsync(Cari cari)
    {
        try
        {
            string anaHesapKodu;
            string anaHesapAdi;
            HesapGrubu hesapGrubu;

            if (cari.CariTipi == CariTipi.Personel)
            {
                // Personel icin 335.XX.PRSXXXXX formatinda hesap
                var firmaId = cari.FirmaId ?? 1;
                anaHesapKodu = $"335.{firmaId:D2}";
                anaHesapAdi = "Personel Borclari";
                hesapGrubu = HesapGrubu.KisaVadeliYabanciKaynaklar;
            }
            else if (cari.CariTipi == CariTipi.Musteri)
            {
                anaHesapKodu = "120.01";
                anaHesapAdi = "Alicilar";
                hesapGrubu = HesapGrubu.DonenVarliklar;
            }
            else if (cari.CariTipi == CariTipi.Tedarikci)
            {
                anaHesapKodu = "320.01";
                anaHesapAdi = "Saticilar";
                hesapGrubu = HesapGrubu.KisaVadeliYabanciKaynaklar;
            }
            else // MusteriTedarikci
            {
                anaHesapKodu = "120.01";
                anaHesapAdi = "Alicilar";
                hesapGrubu = HesapGrubu.DonenVarliklar;
            }

            // 335 ana hesabini kontrol et (personel icin)
            if (cari.CariTipi == CariTipi.Personel)
            {
                var ana335 = await _context.MuhasebeHesaplari
                    .FirstOrDefaultAsync(h => h.HesapKodu == "335");
                
                if (ana335 == null)
                {
                    // 335 - Personele Borclar ana hesabini olustur
                    ana335 = new MuhasebeHesap
                    {
                        HesapKodu = "335",
                        HesapAdi = "Personele Borclar",
                        HesapTuru = HesapTuru.Pasif,
                        HesapGrubu = HesapGrubu.KisaVadeliYabanciKaynaklar,
                        AltHesapVar = true,
                        Aktif = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.MuhasebeHesaplari.Add(ana335);
                    await _context.SaveChangesAsync();
                }
            }

            // Ana hesabi bul veya olustur
            var anaHesap = await _context.MuhasebeHesaplari
                .FirstOrDefaultAsync(h => h.HesapKodu == anaHesapKodu);

            if (anaHesap == null)
            {
                // Ust hesabi bul
                var ustHesapKodu = anaHesapKodu.Split('.')[0];
                var ustHesap = await _context.MuhasebeHesaplari
                    .FirstOrDefaultAsync(h => h.HesapKodu == ustHesapKodu);

                anaHesap = new MuhasebeHesap
                {
                    HesapKodu = anaHesapKodu,
                    HesapAdi = anaHesapAdi,
                    HesapTuru = cari.CariTipi == CariTipi.Personel ? HesapTuru.Pasif : HesapTuru.Aktif,
                    HesapGrubu = hesapGrubu,
                    UstHesapId = ustHesap?.Id,
                    AltHesapVar = true,
                    Aktif = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.MuhasebeHesaplari.Add(anaHesap);
                await _context.SaveChangesAsync();
            }

            // Alt hesap numarasini bul
            string yeniHesapKodu;
            
            if (cari.CariTipi == CariTipi.Personel)
            {
                // Personel icin PRS00001 formatinda
                var sonPersonelHesap = await _context.MuhasebeHesaplari
                    .Where(h => h.HesapKodu.StartsWith(anaHesapKodu + ".PRS"))
                    .OrderByDescending(h => h.HesapKodu)
                    .FirstOrDefaultAsync();

                int nextNum = 1;
                if (sonPersonelHesap != null)
                {
                    var parts = sonPersonelHesap.HesapKodu.Split('.');
                    if (parts.Length >= 3)
                    {
                        var prsKod = parts[2].Replace("PRS", "");
                        if (int.TryParse(prsKod, out var lastNum))
                        {
                            nextNum = lastNum + 1;
                        }
                    }
                }
                yeniHesapKodu = $"{anaHesapKodu}.PRS{nextNum:D5}";
            }
            else
            {
                // Diger cariler icin sayisal format
                var sonAltHesap = await _context.MuhasebeHesaplari
                    .Where(h => h.HesapKodu.StartsWith(anaHesapKodu + "."))
                    .OrderByDescending(h => h.HesapKodu)
                    .FirstOrDefaultAsync();

                int nextNum = 1;
                if (sonAltHesap != null)
                {
                    var parts = sonAltHesap.HesapKodu.Split('.');
                    if (parts.Length >= 3 && int.TryParse(parts[2], out var lastNum))
                    {
                        nextNum = lastNum + 1;
                    }
                }
                yeniHesapKodu = $"{anaHesapKodu}.{nextNum:D3}";
            }

            // Cari icin alt hesap olustur
            var cariHesap = new MuhasebeHesap
            {
                HesapKodu = yeniHesapKodu,
                HesapAdi = cari.Unvan,
                HesapTuru = cari.CariTipi == CariTipi.Personel ? HesapTuru.Pasif : HesapTuru.Aktif,
                HesapGrubu = hesapGrubu,
                UstHesapId = anaHesap.Id,
                AltHesapVar = false,
                Aktif = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.MuhasebeHesaplari.Add(cariHesap);
            await _context.SaveChangesAsync();

            return cariHesap;
        }
        catch
        {
            return null;
        }
    }
}
