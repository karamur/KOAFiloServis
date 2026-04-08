using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using CRMFiloServis.Web.Models;
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
        var cariler = await _context.Cariler
            .Include(c => c.MuhasebeHesap)
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Unvan)
            .ToListAsync();

        foreach (var cari in cariler)
        {
            await FillMuhasebeBilgisiAsync(cari);
        }

        return cariler;
    }

    public async Task<List<Cari>> GetAllWithBakiyeAsync()
    {
        var cariler = await _context.Cariler
            .Include(c => c.MuhasebeHesap)
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Unvan)
            .ToListAsync();

        foreach (var cari in cariler)
        {
            await FillMuhasebeBilgisiAsync(cari);

            // Her cari icin borc/alacak hesapla
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

    public async Task<PagedResult<Cari>> GetPagedAsync(CariFilterParams filter)
    {
        var query = _context.Cariler
            .Include(c => c.MuhasebeHesap)
            .Where(c => !c.IsDeleted)
            .AsQueryable();

        // Arama filtresi
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchLower = filter.SearchTerm.ToLower();
            query = query.Where(c =>
                (c.CariKodu != null && c.CariKodu.ToLower().Contains(searchLower)) ||
                (c.Unvan != null && c.Unvan.ToLower().Contains(searchLower)) ||
                (c.YetkiliKisi != null && c.YetkiliKisi.ToLower().Contains(searchLower)) ||
                (c.Telefon != null && c.Telefon.Contains(searchLower)));
        }

        // Tip filtresi
        if (filter.CariTipi.HasValue)
        {
            query = query.Where(c => c.CariTipi == filter.CariTipi.Value);
        }

        // Aktif/Pasif filtresi
        if (filter.Aktif.HasValue)
        {
            query = query.Where(c => c.Aktif == filter.Aktif.Value);
        }

        // Toplam kayıt sayısı (filtrelenmiş)
        var totalCount = await query.CountAsync();

        // Sayfalama uygula
        var items = await query
            .OrderBy(c => c.Unvan)
            .Skip(filter.Skip)
            .Take(filter.PageSize)
            .ToListAsync();

        // Bakiye hesaplamaları
        foreach (var cari in items)
        {
            await FillMuhasebeBilgisiAsync(cari);
            await CalculateBakiyeAsync(cari);
        }

        // Durum filtresi (bakiye hesaplaması sonrası)
        if (!string.IsNullOrEmpty(filter.DurumFiltre))
        {
            items = filter.DurumFiltre switch
            {
                "borclu" => items.Where(c => c.Borc > c.Alacak).ToList(),
                "alacakli" => items.Where(c => c.Alacak > c.Borc).ToList(),
                "sifir" => items.Where(c => c.Borc == c.Alacak).ToList(),
                "islemsiz" => items.Where(c => c.Borc == 0 && c.Alacak == 0).ToList(),
                _ => items
            };
            // Not: Durum filtresinde toplam sayı yeniden hesaplanmaz (performans için)
        }

        return new PagedResult<Cari>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    private async Task CalculateBakiyeAsync(Cari cari)
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

        if (cari.CariTipi == CariTipi.Musteri)
        {
            cari.Alacak = gidenFaturalar;
            cari.Borc = tahsilatlar;
        }
        else if (cari.CariTipi == CariTipi.Tedarikci)
        {
            cari.Borc = gelenFaturalar;
            cari.Alacak = odemeler;
        }
        else if (cari.CariTipi == CariTipi.Personel)
        {
            cari.Borc = gelenFaturalar;
            cari.Alacak = odemeler;
        }
        else // MusteriTedarikci
        {
            cari.Alacak = gidenFaturalar + odemeler;
            cari.Borc = gelenFaturalar + tahsilatlar;
        }
    }

    public async Task<Cari?> GetByIdAsync(int id)
    {
        var cari = await _context.Cariler
            .Include(c => c.Guzergahlar)
            .Include(c => c.MuhasebeHesap)
            .Where(c => !c.IsDeleted)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cari != null)
        {
            await FillMuhasebeBilgisiAsync(cari);
        }

        return cari;
    }

    public async Task<Cari?> GetByKodAsync(string cariKodu)
    {
        var cari = await _context.Cariler
            .Include(c => c.MuhasebeHesap)
            .Where(c => !c.IsDeleted)
            .FirstOrDefaultAsync(c => c.CariKodu == cariKodu);

        if (cari != null)
        {
            await FillMuhasebeBilgisiAsync(cari);
        }

        return cari;
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
        var girilenCariKodu = cari.CariKodu?.Trim();

        if (!cari.MuhasebeHesapId.HasValue && IsMuhasebeHesapKodu(girilenCariKodu))
        {
            var existingHesap = await FindMuhasebeHesapByKodAsync(girilenCariKodu);
            if (existingHesap != null)
            {
                cari.MuhasebeHesapId = existingHesap.Id;
            }
            else
            {
                var ozelHesap = await CreateMuhasebeHesapAsync(cari, girilenCariKodu);
                if (ozelHesap != null)
                {
                    cari.MuhasebeHesapId = ozelHesap.Id;
                }
            }
        }

        if (!cari.MuhasebeHesapId.HasValue)
        {
            var muhasebeHesap = await CreateMuhasebeHesapAsync(cari);
            if (muhasebeHesap != null)
            {
                cari.MuhasebeHesapId = muhasebeHesap.Id;
            }
        }

        if (!string.IsNullOrWhiteSpace(girilenCariKodu))
        {
            cari.CariKodu = girilenCariKodu;
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

        var girilenCariKodu = cari.CariKodu?.Trim();
        var kodDegisti = !string.Equals(girilenCariKodu, existing.CariKodu, StringComparison.OrdinalIgnoreCase);

        if (!cari.MuhasebeHesapId.HasValue)
        {
            var eslesecekKod = IsMuhasebeHesapKodu(girilenCariKodu)
                ? girilenCariKodu
                : (IsMuhasebeHesapKodu(existing.CariKodu) ? existing.CariKodu : null);

            if (!string.IsNullOrWhiteSpace(eslesecekKod))
            {
                var existingHesap = await FindMuhasebeHesapByKodAsync(eslesecekKod);
                if (existingHesap != null)
                {
                    cari.MuhasebeHesapId = existingHesap.Id;
                }
                else if (IsMuhasebeHesapKodu(girilenCariKodu) && kodDegisti)
                {
                    var ozelHesap = await CreateMuhasebeHesapAsync(cari, girilenCariKodu);
                    if (ozelHesap != null)
                    {
                        cari.MuhasebeHesapId = ozelHesap.Id;
                    }
                }
            }
        }

        if (!cari.MuhasebeHesapId.HasValue && !existing.MuhasebeHesapId.HasValue)
        {
            var muhasebeHesap = await CreateMuhasebeHesapAsync(cari);
            if (muhasebeHesap != null)
            {
                cari.MuhasebeHesapId = muhasebeHesap.Id;
            }
        }
        else if ((existing.MuhasebeHesapId ?? cari.MuhasebeHesapId).HasValue && existing.Unvan != cari.Unvan)
        {
            var hesapId = cari.MuhasebeHesapId ?? existing.MuhasebeHesapId;
            var mHesap = await _context.MuhasebeHesaplari.FindAsync(hesapId!.Value);
            if (mHesap != null)
            {
                mHesap.HesapAdi = cari.Unvan;
                mHesap.UpdatedAt = DateTime.UtcNow;
                _context.MuhasebeHesaplari.Update(mHesap);
            }
        }

        if (!string.IsNullOrWhiteSpace(girilenCariKodu))
        {
            cari.CariKodu = girilenCariKodu;
        }

        existing.CariKodu = cari.CariKodu ?? string.Empty;
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

    public async Task<Cari> MatchMuhasebeHesapByKodAsync(int cariId, string hesapKodu)
    {
        var cari = await _context.Cariler
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == cariId && !c.IsDeleted);

        if (cari == null)
        {
            throw new Exception("Cari bulunamadi");
        }

        if (string.IsNullOrWhiteSpace(hesapKodu))
        {
            throw new Exception("Hesap kodu bos olamaz");
        }

        var muhasebeHesap = await FindMuhasebeHesapByKodAsync(hesapKodu);
        if (muhasebeHesap == null)
        {
            throw new Exception("Girilen hesap kodu hesap planinda bulunamadi");
        }

        cari.MuhasebeHesapId = muhasebeHesap.Id;
        cari.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return cari;
    }

    public async Task<Cari> EnsureMuhasebeHesapAsync(int cariId)
    {
        var cari = await _context.Cariler
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == cariId && !c.IsDeleted);

        if (cari == null)
        {
            throw new Exception("Cari bulunamadi");
        }

        if (cari.MuhasebeHesapId.HasValue)
        {
            var bagliHesap = await _context.MuhasebeHesaplari.FindAsync(cari.MuhasebeHesapId.Value);
            if (bagliHesap != null)
            {
                return cari;
            }
        }

        if (IsMuhasebeHesapKodu(cari.CariKodu))
        {
            var existingHesap = await FindMuhasebeHesapByKodAsync(cari.CariKodu);
            if (existingHesap != null)
            {
                cari.MuhasebeHesapId = existingHesap.Id;
                cari.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return cari;
            }
        }

        var olusanHesap = await CreateMuhasebeHesapAsync(cari);
        if (olusanHesap == null)
        {
            throw new Exception("Muhasebe hesap kodu olusturulamadi");
        }

        cari.MuhasebeHesapId = olusanHesap.Id;
        cari.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return cari;
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
    private async Task<MuhasebeHesap?> CreateMuhasebeHesapAsync(Cari cari, string? ozelHesapKodu = null)
    {
        try
        {
            // Ayarları al (Eğer DB'de yoksa default ayarları kullan)
            var ayar = await _context.MuhasebeAyarlari.FirstOrDefaultAsync() ?? new MuhasebeAyar();

            if (!ayar.OtomatikHesapDuzenlensin)
            {
                return null; // Otomatik hesap açılması kapalıysa işlem yapma
            }

            string anaHesapKodu;
            string anaHesapAdi;
            HesapGrubu hesapGrubu;

            if (cari.CariTipi == CariTipi.Personel)
            {
                // Personel icin ayardaki prefix'i kullan
                // Ornegin ayar "335.01" ise onu kullan, "335" ise "335.FirmaId" de yapilabilir,
                // Ama prefix genelde mutlaktir. Formatini ayardan al.
                anaHesapKodu = ayar.PersonelPrefix;
                anaHesapAdi = "Personel Borclari";
                hesapGrubu = HesapGrubu.KisaVadeliYabanciKaynaklar;
            }
            else if (cari.CariTipi == CariTipi.Musteri)
            {
                anaHesapKodu = ayar.MusteriPrefix;
                anaHesapAdi = "Alicilar";
                hesapGrubu = HesapGrubu.DonenVarliklar;
            }
            else if (cari.CariTipi == CariTipi.Tedarikci)
            {
                anaHesapKodu = ayar.TedarikciPrefix;
                anaHesapAdi = "Saticilar";
                hesapGrubu = HesapGrubu.KisaVadeliYabanciKaynaklar;
            }
            else // MusteriTedarikci
            {
                anaHesapKodu = ayar.MusteriPrefix;
                anaHesapAdi = "Alicilar";
                hesapGrubu = HesapGrubu.DonenVarliklar;
            }

            // 335 ana hesabini kontrol et (personel icin) - opsiyonel, sadece 335 ise ana kontrol gerekir
            var prefixBasKisim = anaHesapKodu.Split('.')[0];
            if (cari.CariTipi == CariTipi.Personel)
            {
                var anaPersonelHesap = await _context.MuhasebeHesaplari
                    .FirstOrDefaultAsync(h => h.HesapKodu == prefixBasKisim);
                
                if (anaPersonelHesap == null)
                {
                    anaPersonelHesap = new MuhasebeHesap
                    {
                        HesapKodu = prefixBasKisim,
                        HesapAdi = "Personel Borclari Ana Hesap",
                        HesapTuru = HesapTuru.Pasif,
                        HesapGrubu = HesapGrubu.KisaVadeliYabanciKaynaklar,
                        AltHesapVar = true,
                        Aktif = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.MuhasebeHesaplari.Add(anaPersonelHesap);
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

            // Ayni unvana ve ana hesaba sahip mevcut bir kayit var mi kontrol et
            var ayniUnvanliHesap = await _context.MuhasebeHesaplari
                .FirstOrDefaultAsync(h => h.HesapKodu.StartsWith(anaHesapKodu + ".") && h.HesapAdi == cari.Unvan);

            if (ayniUnvanliHesap != null && string.IsNullOrEmpty(ozelHesapKodu))
            {
                // Unvan tutuyorsa ve ozel kod zorlamiyorsa onu dondur
                return ayniUnvanliHesap;
            }

            // Alt hesap numarasini bul
            string yeniHesapKodu;
            
            if (!string.IsNullOrWhiteSpace(ozelHesapKodu))
            {
                // Kullanici spesifik bir kod istedi e.g "120.01.077", direkt onu kullan (zaten yukarida varligi test edildi)
                yeniHesapKodu = ozelHesapKodu;
            }
            else
            {
                // Tum cariler icin sayisal format (Prefix.001) otomatik uretim
                var sonAltHesap = await _context.MuhasebeHesaplari
                    .Where(h => h.HesapKodu.StartsWith(anaHesapKodu + "."))
                    .OrderByDescending(h => h.HesapKodu)
                    .FirstOrDefaultAsync();

                int nextNum = 1;
                if (sonAltHesap != null)
                {
                    // Son '.' dan sonrasini al
                    var parts = sonAltHesap.HesapKodu.Split('.');
                    var sonKisim = parts[parts.Length - 1]; // "001", "PRS00001" vs.

                    // Eger icinde PRS vb. harf varsa temizle, sadece sayilari al
                    var sadeceSayi = new string(sonKisim.Where(char.IsDigit).ToArray());

                    if (int.TryParse(sadeceSayi, out var lastNum))
                    {
                        nextNum = lastNum + 1;
                    }
                }

                // Hesabi olustururken Kac haneli olacak? Personelse ornegin 5 haneli yapabiliriz, standartsa 3
                if (cari.CariTipi == CariTipi.Personel)
                {
                    yeniHesapKodu = $"{anaHesapKodu}.{nextNum:D3}"; // Artik PRS ismini kaldirip sadece sayisal yapiyoruz (3 haneli)
                }
                else
                {
                    yeniHesapKodu = $"{anaHesapKodu}.{nextNum:D3}";
                }
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

    private async Task<MuhasebeHesap?> FindMuhasebeHesapByKodAsync(string? hesapKodu)
    {
        if (string.IsNullOrWhiteSpace(hesapKodu))
        {
            return null;
        }

        var normalizedKod = hesapKodu.Trim();
        return await _context.MuhasebeHesaplari
            .FirstOrDefaultAsync(h => h.HesapKodu == normalizedKod);
    }

    private async Task FillMuhasebeBilgisiAsync(Cari cari)
    {
        var muhasebeHesap = cari.MuhasebeHesap;

        if (muhasebeHesap == null && cari.MuhasebeHesapId.HasValue)
        {
            muhasebeHesap = await _context.MuhasebeHesaplari.FindAsync(cari.MuhasebeHesapId.Value);
        }

        if (muhasebeHesap == null && IsMuhasebeHesapKodu(cari.CariKodu))
        {
            muhasebeHesap = await FindMuhasebeHesapByKodAsync(cari.CariKodu);
        }

        if (muhasebeHesap == null && !string.IsNullOrWhiteSpace(cari.Unvan))
        {
            var unvan = cari.Unvan.Trim();

            muhasebeHesap = await _context.MuhasebeHesaplari
                .FirstOrDefaultAsync(h => h.HesapAdi == unvan)
                ?? await _context.MuhasebeHesaplari
                    .FirstOrDefaultAsync(h => !string.IsNullOrWhiteSpace(h.HesapAdi) &&
                                              (h.HesapAdi.Contains(unvan) || unvan.Contains(h.HesapAdi)));
        }

        if (muhasebeHesap != null)
        {
            cari.MuhasebeHesapId = muhasebeHesap.Id;
            cari.MuhasebeHesap = muhasebeHesap;
        }
    }

    private static bool IsMuhasebeHesapKodu(string? kod)
    {
        if (string.IsNullOrWhiteSpace(kod))
        {
            return false;
        }

        return kod.Contains('.') &&
               (kod.StartsWith("120.", StringComparison.OrdinalIgnoreCase) ||
                kod.StartsWith("320.", StringComparison.OrdinalIgnoreCase) ||
                kod.StartsWith("335.", StringComparison.OrdinalIgnoreCase));
    }

    // ===== İletişim Geçmişi =====

    public async Task<List<CariIletisimNot>> GetIletisimNotlariAsync(int cariId, int? adet = null)
    {
        var query = _context.CariIletisimNotlar
            .Where(n => n.CariId == cariId && !n.IsDeleted)
            .OrderByDescending(n => n.IletisimTarihi)
            .AsQueryable();

        if (adet.HasValue)
            query = query.Take(adet.Value);

        return await query.ToListAsync();
    }

    public async Task<CariIletisimNot> AddIletisimNotuAsync(CariIletisimNot not)
    {
        _context.CariIletisimNotlar.Add(not);
        await _context.SaveChangesAsync();
        return not;
    }

    public async Task<CariIletisimNot> UpdateIletisimNotuAsync(CariIletisimNot not)
    {
        var mevcut = await _context.CariIletisimNotlar.FindAsync(not.Id);
        if (mevcut == null) throw new Exception("İletişim notu bulunamadı.");

        mevcut.Konu = not.Konu;
        mevcut.Notlar = not.Notlar;
        mevcut.IletisimTipi = not.IletisimTipi;
        mevcut.IletisimTarihi = not.IletisimTarihi;
        mevcut.SonrakiAksiyon = not.SonrakiAksiyon;
        mevcut.SonrakiAksiyonTarihi = not.SonrakiAksiyonTarihi;
        mevcut.AksiyonTamamlandi = not.AksiyonTamamlandi;
        mevcut.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return mevcut;
    }

    public async Task<bool> DeleteIletisimNotuAsync(int notId)
    {
        var not = await _context.CariIletisimNotlar.FindAsync(notId);
        if (not == null) return false;

        not.IsDeleted = true;
        not.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    // ===== Hatırlatıcılar =====

    public async Task<List<Hatirlatici>> GetCariHatirlaticilariAsync(int cariId)
    {
        return await _context.Hatirlaticilar
            .Where(h => h.CariId == cariId && !h.IsDeleted)
            .OrderByDescending(h => h.BaslangicTarihi)
            .ToListAsync();
    }

    public async Task<Hatirlatici> AddCariHatirlaticiAsync(Hatirlatici hatirlatici)
    {
        _context.Hatirlaticilar.Add(hatirlatici);
        await _context.SaveChangesAsync();
        return hatirlatici;
    }

    // ===== Vade Uyarıları =====

    public async Task<List<CariVadeUyari>> GetVadeUyarilariAsync(int? cariId = null, int yaklasmaSuresiGun = 7)
    {
        var bugun = DateTime.Today;
        var uyarilar = new List<CariVadeUyari>();

        var query = _context.Faturalar
            .Include(f => f.Cari)
            .Where(f => !f.IsDeleted && f.VadeTarihi.HasValue && f.Durum != FaturaDurum.Odendi && f.Durum != FaturaDurum.IptalEdildi);

        if (cariId.HasValue)
            query = query.Where(f => f.CariId == cariId.Value);

        var faturalar = await query.ToListAsync();

        foreach (var f in faturalar)
        {
            var kalanTutar = f.GenelToplam - f.OdenenTutar;
            if (kalanTutar <= 0) continue;

            var vade = f.VadeTarihi!.Value;
            var kalanGun = (vade - bugun).Days;

            // Kritik: 30+ gün gecikmiş, Gecikmiş: vadesi geçmiş, Bugün: bugün vadeli, Yaklaşan: yaklasmaSuresiGun içinde
            VadeUyariSeviye seviye;
            if (kalanGun < -30)
                seviye = VadeUyariSeviye.VadesiGecmisKritik;
            else if (kalanGun < 0)
                seviye = VadeUyariSeviye.VadesiGecmis;
            else if (kalanGun == 0)
                seviye = VadeUyariSeviye.BugunVadeli;
            else if (kalanGun <= yaklasmaSuresiGun)
                seviye = VadeUyariSeviye.YaklasanVade;
            else
                continue;

            uyarilar.Add(new CariVadeUyari
            {
                CariId = f.CariId,
                CariUnvan = f.Cari?.Unvan ?? "",
                CariKodu = f.Cari?.CariKodu ?? "",
                FaturaId = f.Id,
                FaturaNo = f.FaturaNo,
                FaturaTarihi = f.FaturaTarihi,
                VadeTarihi = vade,
                KalanGun = kalanGun,
                KalanTutar = kalanTutar,
                Seviye = seviye
            });
        }

        return uyarilar.OrderBy(u => u.KalanGun).ToList();
    }
}
