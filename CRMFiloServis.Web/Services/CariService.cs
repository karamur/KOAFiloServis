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
        // Kullanıcı ozel bir hesap kodu girdiyse (CRI- baslamayan) once hesap var mi bakalim
        if (!string.IsNullOrWhiteSpace(cari.CariKodu) && !cari.CariKodu.StartsWith("CRI-"))
        {
            var existingHesap = await _context.MuhasebeHesaplari.FirstOrDefaultAsync(h => h.HesapKodu == cari.CariKodu);
            if (existingHesap != null)
            {
                cari.MuhasebeHesapId = existingHesap.Id;
            }
            else
            {
                // Yoksa bu ozel kodla acalim
                var ozelHesap = await CreateMuhasebeHesapAsync(cari, cari.CariKodu);
                if (ozelHesap != null)
                {
                    cari.MuhasebeHesapId = ozelHesap.Id;
                }
            }
        }

        // Yukaridaki isleme ragmen Muhasebe hesabi otomatik olustur (eger kod girmeden secilmediyse) Veya var olani ustlen
        if (!cari.MuhasebeHesapId.HasValue)
        {
            var muhasebeHesap = await CreateMuhasebeHesapAsync(cari);
            if (muhasebeHesap != null)
            {
                cari.MuhasebeHesapId = muhasebeHesap.Id;
                cari.CariKodu = muhasebeHesap.HesapKodu;
            }
        }
        else if (cari.CariKodu.StartsWith("CRI-"))
        {
            // Kullanici sectiyse onun kodunu bas
            var bHesap = await _context.MuhasebeHesaplari.FindAsync(cari.MuhasebeHesapId.Value);
            if (bHesap != null)
            {
                cari.CariKodu = bHesap.HesapKodu;
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

        // UI uzerinden kullanici cari kodunu manuel 120.xxx vs girip kaydettiyse:
        if (!string.IsNullOrWhiteSpace(cari.CariKodu) && !cari.CariKodu.StartsWith("CRI-") && cari.CariKodu != existing.CariKodu)
        {
            // Bu hesabi hesapplaninda ara
            var existingHesap = await _context.MuhasebeHesaplari.FirstOrDefaultAsync(h => h.HesapKodu == cari.CariKodu);
            if (existingHesap != null)
            {
                cari.MuhasebeHesapId = existingHesap.Id;
            }
            else
            {
                // Yoksa bu ozel kodla acmayi dene
                var ozelHesap = await CreateMuhasebeHesapAsync(cari, cari.CariKodu);
                if (ozelHesap != null)
                {
                    cari.MuhasebeHesapId = ozelHesap.Id;
                }
            }
        }

        // Cari muhasebe hesabı yoksa ve ayarlarda otomatik hesap aç seçiliyse açsın
        if (!cari.MuhasebeHesapId.HasValue && !existing.MuhasebeHesapId.HasValue)
        {
            var muhasebeHesap = await CreateMuhasebeHesapAsync(cari);
            if (muhasebeHesap != null)
            {
                cari.MuhasebeHesapId = muhasebeHesap.Id;
                cari.CariKodu = muhasebeHesap.HesapKodu;
            }
        }
        else if (cari.MuhasebeHesapId.HasValue && cari.MuhasebeHesapId != existing.MuhasebeHesapId)
        {
            // Kullanıcı dropdown listesinden muhasebe hesabını manuel değiştirdiyse hesap kodunu da güncelle
            var bHesap = await _context.MuhasebeHesaplari.FindAsync(cari.MuhasebeHesapId.Value);
            if (bHesap != null)
            {
                cari.CariKodu = bHesap.HesapKodu;
            }
        }
        else if (existing.MuhasebeHesapId.HasValue && existing.Unvan != cari.Unvan) 
        {
            // Eğer cari adı değiştiyse bağli muhasebe hesap adını da güncelleyelim.
            var mHesap = await _context.MuhasebeHesaplari.FindAsync(existing.MuhasebeHesapId.Value);
            if (mHesap != null)
            {
                mHesap.HesapAdi = cari.Unvan;
                mHesap.UpdatedAt = DateTime.UtcNow;
                _context.MuhasebeHesaplari.Update(mHesap);
            }
        }

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
}
