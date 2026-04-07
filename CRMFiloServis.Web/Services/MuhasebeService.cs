using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using CRMFiloServis.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class MuhasebeService : IMuhasebeService
{
    private readonly ApplicationDbContext _context;
    private static readonly string[] AyAdlari = { "", "Ocak", "Subat", "Mart", "Nisan", "Mayis", "Haziran", 
                                                   "Temmuz", "Agustos", "Eylul", "Ekim", "Kasim", "Aralik" };

    public MuhasebeService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Hesap Plani

    public async Task<List<MuhasebeHesap>> GetHesapPlaniAsync()
    {
        return await _context.MuhasebeHesaplari
            .OrderBy(h => h.HesapKodu)
            .ToListAsync();
    }

    public async Task<List<MuhasebeHesap>> GetHesaplarByGrupAsync(HesapGrubu grup)
    {
        return await _context.MuhasebeHesaplari
            .Where(h => h.HesapGrubu == grup && h.Aktif)
            .OrderBy(h => h.HesapKodu)
            .ToListAsync();
    }

    public async Task<MuhasebeHesap?> GetHesapByKodAsync(string hesapKodu)
    {
        return await _context.MuhasebeHesaplari
            .FirstOrDefaultAsync(h => h.HesapKodu == hesapKodu);
    }

    public async Task<MuhasebeHesap?> GetHesapByIdAsync(int id)
    {
        return await _context.MuhasebeHesaplari.FindAsync(id);
    }

    public async Task<MuhasebeHesap> CreateHesapAsync(MuhasebeHesap hesap)
    {
        hesap.CreatedAt = DateTime.UtcNow;
        _context.MuhasebeHesaplari.Add(hesap);
        await _context.SaveChangesAsync();
        return hesap;
    }

    public async Task<MuhasebeHesap> UpdateHesapAsync(MuhasebeHesap hesap)
    {
        var existing = await _context.MuhasebeHesaplari.FindAsync(hesap.Id);
        if (existing == null) throw new Exception("Hesap bulunamadi");

        existing.HesapAdi = hesap.HesapAdi;
        existing.Aciklama = hesap.Aciklama;
        existing.Aktif = hesap.Aktif;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteHesapAsync(int id)
    {
        var hesap = await _context.MuhasebeHesaplari.FindAsync(id);
        if (hesap == null) return;
        if (hesap.SistemHesabi) throw new Exception("Sistem hesabi silinemez");

        // Hesapta hareket var mi kontrol et
        var hareketVar = await _context.MuhasebeFisKalemleri.AnyAsync(k => k.HesapId == id);
        if (hareketVar) throw new Exception("Hesapta hareket var, silinemez");

        hesap.IsDeleted = true;
        hesap.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task SeedVarsayilanHesapPlaniAsync()
    {
        var hesaplar = new List<MuhasebeHesap>
        {
            // 1 - DONEN VARLIKLAR
            new() { HesapKodu = "100", HesapAdi = "KASA", HesapTuru = HesapTuru.Aktif, HesapGrubu = HesapGrubu.DonenVarliklar, SistemHesabi = true, AltHesapVar = true },
            new() { HesapKodu = "100.01", HesapAdi = "Merkez Kasa", HesapTuru = HesapTuru.Aktif, HesapGrubu = HesapGrubu.DonenVarliklar, SistemHesabi = true },
            new() { HesapKodu = "102", HesapAdi = "BANKALAR", HesapTuru = HesapTuru.Aktif, HesapGrubu = HesapGrubu.DonenVarliklar, SistemHesabi = true, AltHesapVar = true },
            new() { HesapKodu = "102.01", HesapAdi = "Vadesiz Mevduat", HesapTuru = HesapTuru.Aktif, HesapGrubu = HesapGrubu.DonenVarliklar, SistemHesabi = true },
            new() { HesapKodu = "120", HesapAdi = "ALICILAR", HesapTuru = HesapTuru.Aktif, HesapGrubu = HesapGrubu.DonenVarliklar, SistemHesabi = true, AltHesapVar = true },
            new() { HesapKodu = "121", HesapAdi = "ALACAK SENETLERI", HesapTuru = HesapTuru.Aktif, HesapGrubu = HesapGrubu.DonenVarliklar, SistemHesabi = true },
            new() { HesapKodu = "126", HesapAdi = "VERILEN DEPOZITO VE TEMINATLAR", HesapTuru = HesapTuru.Aktif, HesapGrubu = HesapGrubu.DonenVarliklar },
            new() { HesapKodu = "153", HesapAdi = "TICARI MALLAR", HesapTuru = HesapTuru.Aktif, HesapGrubu = HesapGrubu.DonenVarliklar },
            new() { HesapKodu = "180", HesapAdi = "GELECEK AYLARA AIT GIDERLER", HesapTuru = HesapTuru.Aktif, HesapGrubu = HesapGrubu.DonenVarliklar },
            new() { HesapKodu = "190", HesapAdi = "DEVREDEN KDV", HesapTuru = HesapTuru.Aktif, HesapGrubu = HesapGrubu.DonenVarliklar },
            new() { HesapKodu = "191", HesapAdi = "INDIRILECEK KDV", HesapTuru = HesapTuru.Aktif, HesapGrubu = HesapGrubu.DonenVarliklar },

            // 2 - DURAN VARLIKLAR
            new() { HesapKodu = "253", HesapAdi = "TESISLER, MAKINE VE CIHAZLAR", HesapTuru = HesapTuru.Aktif, HesapGrubu = HesapGrubu.DuranVarliklar },
            new() { HesapKodu = "254", HesapAdi = "TASITLAR", HesapTuru = HesapTuru.Aktif, HesapGrubu = HesapGrubu.DuranVarliklar },
            new() { HesapKodu = "255", HesapAdi = "DEMIRBASLAR", HesapTuru = HesapTuru.Aktif, HesapGrubu = HesapGrubu.DuranVarliklar },
            new() { HesapKodu = "257", HesapAdi = "BIRIKNIS AMORTISMANLAR (-)", HesapTuru = HesapTuru.Aktif, HesapGrubu = HesapGrubu.DuranVarliklar },

            // 3 - KISA VADELI YABANCI KAYNAKLAR
            new() { HesapKodu = "300", HesapAdi = "BANKA KREDILERI", HesapTuru = HesapTuru.Pasif, HesapGrubu = HesapGrubu.KisaVadeliYabanciKaynaklar },
            new() { HesapKodu = "320", HesapAdi = "SATICILAR", HesapTuru = HesapTuru.Pasif, HesapGrubu = HesapGrubu.KisaVadeliYabanciKaynaklar, SistemHesabi = true, AltHesapVar = true },
            new() { HesapKodu = "321", HesapAdi = "BORC SENETLERI", HesapTuru = HesapTuru.Pasif, HesapGrubu = HesapGrubu.KisaVadeliYabanciKaynaklar },
            new() { HesapKodu = "335", HesapAdi = "PERSONELE BORCLAR", HesapTuru = HesapTuru.Pasif, HesapGrubu = HesapGrubu.KisaVadeliYabanciKaynaklar },
            new() { HesapKodu = "340", HesapAdi = "ALINAN SIPARIS AVANSLARI", HesapTuru = HesapTuru.Pasif, HesapGrubu = HesapGrubu.KisaVadeliYabanciKaynaklar },
            new() { HesapKodu = "360", HesapAdi = "ODENECEK VERGILER VE FONLAR", HesapTuru = HesapTuru.Pasif, HesapGrubu = HesapGrubu.KisaVadeliYabanciKaynaklar },
            new() { HesapKodu = "361", HesapAdi = "ODENECEK SOSYAL GUVENLIK KESINTILERI", HesapTuru = HesapTuru.Pasif, HesapGrubu = HesapGrubu.KisaVadeliYabanciKaynaklar },
            new() { HesapKodu = "391", HesapAdi = "HESAPLANAN KDV", HesapTuru = HesapTuru.Pasif, HesapGrubu = HesapGrubu.KisaVadeliYabanciKaynaklar },

            // 4 - UZUN VADELI YABANCI KAYNAKLAR
            new() { HesapKodu = "400", HesapAdi = "BANKA KREDILERI", HesapTuru = HesapTuru.Pasif, HesapGrubu = HesapGrubu.UzunVadeliYabanciKaynaklar },

            // 5 - OZKAYNAKLAR
            new() { HesapKodu = "500", HesapAdi = "SERMAYE", HesapTuru = HesapTuru.Pasif, HesapGrubu = HesapGrubu.Ozkaynaklar, SistemHesabi = true },
            new() { HesapKodu = "570", HesapAdi = "GECMIS YILLAR KARLARI", HesapTuru = HesapTuru.Pasif, HesapGrubu = HesapGrubu.Ozkaynaklar },
            new() { HesapKodu = "580", HesapAdi = "GECMIS YILLAR ZARARLARI (-)", HesapTuru = HesapTuru.Pasif, HesapGrubu = HesapGrubu.Ozkaynaklar },
            new() { HesapKodu = "590", HesapAdi = "DONEM NET KARI", HesapTuru = HesapTuru.Pasif, HesapGrubu = HesapGrubu.Ozkaynaklar },
            new() { HesapKodu = "591", HesapAdi = "DONEM NET ZARARI (-)", HesapTuru = HesapTuru.Pasif, HesapGrubu = HesapGrubu.Ozkaynaklar },

            // 6 - GELIR TABLOSU HESAPLARI
            new() { HesapKodu = "600", HesapAdi = "YURTICI SATISLAR", HesapTuru = HesapTuru.Gelir, HesapGrubu = HesapGrubu.GelirTablosu, SistemHesabi = true },
            new() { HesapKodu = "602", HesapAdi = "DIGER GELIRLER", HesapTuru = HesapTuru.Gelir, HesapGrubu = HesapGrubu.GelirTablosu },
            new() { HesapKodu = "610", HesapAdi = "SATISTAN IADELER (-)", HesapTuru = HesapTuru.Gelir, HesapGrubu = HesapGrubu.GelirTablosu },
            new() { HesapKodu = "611", HesapAdi = "SATIS ISKONTALARI (-)", HesapTuru = HesapTuru.Gelir, HesapGrubu = HesapGrubu.GelirTablosu },
            new() { HesapKodu = "620", HesapAdi = "SATILAN MAMULLER MALIYETI", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.GelirTablosu },
            new() { HesapKodu = "621", HesapAdi = "SATILAN TICARI MALLAR MALIYETI", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.GelirTablosu },
            new() { HesapKodu = "622", HesapAdi = "SATILAN HIZMET MALIYETI", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.GelirTablosu },
            new() { HesapKodu = "642", HesapAdi = "FAIZ GELIRLERI", HesapTuru = HesapTuru.Gelir, HesapGrubu = HesapGrubu.GelirTablosu },
            new() { HesapKodu = "649", HesapAdi = "DIGER FAALIYETLERDEN GELIRLER", HesapTuru = HesapTuru.Gelir, HesapGrubu = HesapGrubu.GelirTablosu },
            new() { HesapKodu = "653", HesapAdi = "KOMISYON GIDERLERI", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.GelirTablosu },
            new() { HesapKodu = "660", HesapAdi = "KISA VADELI BORÇLANMA GIDERLERI", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.GelirTablosu },

            // 7 - MALIYET/GIDER HESAPLARI
            new() { HesapKodu = "710", HesapAdi = "DIREKT ILKMADDE MALZEME GIDERLERI", HesapTuru = HesapTuru.Maliyet, HesapGrubu = HesapGrubu.MaliyetHesaplari },
            new() { HesapKodu = "720", HesapAdi = "DIREKT ISCILIK GIDERLERI", HesapTuru = HesapTuru.Maliyet, HesapGrubu = HesapGrubu.MaliyetHesaplari },
            new() { HesapKodu = "730", HesapAdi = "GENEL URETIM GIDERLERI", HesapTuru = HesapTuru.Maliyet, HesapGrubu = HesapGrubu.MaliyetHesaplari },
            new() { HesapKodu = "740", HesapAdi = "HIZMET URETIM MALIYETI", HesapTuru = HesapTuru.Maliyet, HesapGrubu = HesapGrubu.MaliyetHesaplari },
            new() { HesapKodu = "750", HesapAdi = "ARASTIRMA GELISTIRME GIDERLERI", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari },
            new() { HesapKodu = "760", HesapAdi = "PAZARLAMA SATIS DAGITIM GIDERLERI", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari },
            new() { HesapKodu = "770", HesapAdi = "GENEL YONETIM GIDERLERI", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari, SistemHesabi = true },
            new() { HesapKodu = "770.01", HesapAdi = "Kira Giderleri", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari },
            new() { HesapKodu = "770.02", HesapAdi = "Elektrik Giderleri", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari },
            new() { HesapKodu = "770.03", HesapAdi = "Su Giderleri", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari },
            new() { HesapKodu = "770.04", HesapAdi = "Dogalgaz Giderleri", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari },
            new() { HesapKodu = "770.05", HesapAdi = "Telefon/Internet Giderleri", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari },
            new() { HesapKodu = "770.06", HesapAdi = "Yakit Giderleri", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari },
            new() { HesapKodu = "770.07", HesapAdi = "Bakim Onarim Giderleri", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari },
            new() { HesapKodu = "770.08", HesapAdi = "Sigorta Giderleri", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari },
            new() { HesapKodu = "770.09", HesapAdi = "Personel Giderleri", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari },
            new() { HesapKodu = "780", HesapAdi = "FINANSMAN GIDERLERI", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari }
        };

        var mevcutKodlar = await _context.MuhasebeHesaplari
            .Select(h => h.HesapKodu)
            .ToListAsync();

        var mevcutKodSet = new HashSet<string>(mevcutKodlar, StringComparer.OrdinalIgnoreCase);
        var eklenecekHesaplar = hesaplar
            .Where(h => !mevcutKodSet.Contains(h.HesapKodu))
            .Select(h =>
            {
                h.CreatedAt = DateTime.UtcNow;
                return h;
            })
            .ToList();

        if (eklenecekHesaplar.Count == 0)
            return;

        _context.MuhasebeHesaplari.AddRange(eklenecekHesaplar);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HesapIslemGormusMuAsync(int hesapId)
    {
        return await _context.MuhasebeFisKalemleri.AnyAsync(k => k.HesapId == hesapId);
    }

    public async Task<HesapPlaniImportResult> ImportHesapPlaniFromExcelAsync(byte[] fileContent)
    {
        var result = new HesapPlaniImportResult();
        
        try
        {
            using var stream = new MemoryStream(fileContent);
            using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
            var ws = workbook.Worksheets.First();
            
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            var mevcutHesaplar = await _context.MuhasebeHesaplari.ToListAsync();
            var mevcutKodlar = mevcutHesaplar.ToDictionary(h => h.HesapKodu.Trim().ToUpper(), h => h);
            
            for (int row = 2; row <= lastRow; row++) // 1. satır başlık
            {
                try
                {
                    var hesapKodu = ws.Cell(row, 1).GetString()?.Trim();
                    var hesapAdi = ws.Cell(row, 2).GetString()?.Trim();
                    
                    if (string.IsNullOrWhiteSpace(hesapKodu) || string.IsNullOrWhiteSpace(hesapAdi))
                        continue;
                    
                    // Hesap kodunu normalize et (123 -> 123, 123.01 -> 123.01)
                    hesapKodu = NormalizeHesapKodu(hesapKodu);
                    
                    // Hesap grubu ve türü otomatik belirle
                    var (grup, tur) = DetermineHesapGrupVeTur(hesapKodu);
                    
                    if (mevcutKodlar.TryGetValue(hesapKodu.ToUpper(), out var mevcutHesap))
                    {
                        // Mevcut hesabı güncelle (sadece ismi)
                        if (mevcutHesap.HesapAdi != hesapAdi)
                        {
                            mevcutHesap.HesapAdi = hesapAdi;
                            mevcutHesap.UpdatedAt = DateTime.UtcNow;
                            result.UpdatedCount++;
                        }
                    }
                    else
                    {
                        // Yeni hesap ekle
                        var yeniHesap = new MuhasebeHesap
                        {
                            HesapKodu = hesapKodu,
                            HesapAdi = hesapAdi,
                            HesapGrubu = grup,
                            HesapTuru = tur,
                            Aktif = true,
                            AltHesapVar = !hesapKodu.Contains('.'),
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.MuhasebeHesaplari.Add(yeniHesap);
                        mevcutKodlar[hesapKodu.ToUpper()] = yeniHesap;
                        result.ImportedCount++;
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Satir {row}: {ex.Message}");
                    result.ErrorCount++;
                }
            }
            
            await _context.SaveChangesAsync();
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Excel okuma hatasi: {ex.Message}");
            result.Success = false;
        }
        
        return result;
    }

    public async Task<byte[]> GetHesapPlaniSablonAsync()
    {
        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var ws = workbook.Worksheets.Add("Hesap Plani");
        
        // Başlıklar
        ws.Cell(1, 1).Value = "Hesap Kodu";
        ws.Cell(1, 2).Value = "Hesap Adi";
        ws.Row(1).Style.Font.Bold = true;
        ws.Row(1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;
        
        // Örnek veriler
        ws.Cell(2, 1).Value = "100";
        ws.Cell(2, 2).Value = "KASA";
        ws.Cell(3, 1).Value = "100.01";
        ws.Cell(3, 2).Value = "Merkez Kasa";
        ws.Cell(4, 1).Value = "102";
        ws.Cell(4, 2).Value = "BANKALAR";
        ws.Cell(5, 1).Value = "120";
        ws.Cell(5, 2).Value = "ALICILAR";
        ws.Cell(6, 1).Value = "320";
        ws.Cell(6, 2).Value = "SATICILAR";
        ws.Cell(7, 1).Value = "600";
        ws.Cell(7, 2).Value = "YURTICI SATISLAR";
        ws.Cell(8, 1).Value = "770";
        ws.Cell(8, 2).Value = "GENEL YONETIM GIDERLERI";
        
        ws.Columns().AdjustToContents();
        
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private string NormalizeHesapKodu(string kod)
    {
        // Boşlukları temizle
        kod = kod.Trim().Replace(" ", "");
        
        // Virgülleri noktaya çevir
        kod = kod.Replace(",", ".");
        
        return kod;
    }

    private (HesapGrubu grup, HesapTuru tur) DetermineHesapGrupVeTur(string hesapKodu)
    {
        if (string.IsNullOrEmpty(hesapKodu)) 
            return (HesapGrubu.DonenVarliklar, HesapTuru.Aktif);
        
        var ilkKarakter = hesapKodu[0];
        
        return ilkKarakter switch
        {
            '1' => (HesapGrubu.DonenVarliklar, HesapTuru.Aktif),
            '2' => (HesapGrubu.DuranVarliklar, HesapTuru.Aktif),
            '3' => (HesapGrubu.KisaVadeliYabanciKaynaklar, HesapTuru.Pasif),
            '4' => (HesapGrubu.UzunVadeliYabanciKaynaklar, HesapTuru.Pasif),
            '5' => (HesapGrubu.Ozkaynaklar, HesapTuru.Pasif),
            '6' => (HesapGrubu.GelirTablosu, HesapTuru.Gelir),
            '7' => (HesapGrubu.MaliyetHesaplari, HesapTuru.Gider),
            '8' => (HesapGrubu.MaliyetHesaplari, HesapTuru.Maliyet),
            '9' => (HesapGrubu.MaliyetHesaplari, HesapTuru.Maliyet),
            _ => (HesapGrubu.DonenVarliklar, HesapTuru.Aktif)
        };
    }

    #endregion

    #region Muhasebe Fisleri

    public async Task<List<MuhasebeFis>> GetFislerAsync(int yil, int? ay = null)
    {
        var query = _context.MuhasebeFisleri
            .Include(f => f.Kalemler)
            .ThenInclude(k => k.Hesap)
            .Where(f => f.FisTarihi.Year == yil);

        if (ay.HasValue)
            query = query.Where(f => f.FisTarihi.Month == ay.Value);

        return await query.OrderByDescending(f => f.FisTarihi).ThenBy(f => f.FisNo).ToListAsync();
    }

    public async Task<List<MuhasebeFis>> GetFislerByTipAsync(FisTipi tip, int yil, int? ay = null)
    {
        var query = _context.MuhasebeFisleri
            .Include(f => f.Kalemler)
            .Where(f => f.FisTipi == tip && f.FisTarihi.Year == yil);

        if (ay.HasValue)
            query = query.Where(f => f.FisTarihi.Month == ay.Value);

        return await query.OrderByDescending(f => f.FisTarihi).ToListAsync();
    }

    public async Task<MuhasebeFis?> GetFisByIdAsync(int id)
    {
        return await _context.MuhasebeFisleri
            .Include(f => f.Kalemler)
            .ThenInclude(k => k.Hesap)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<MuhasebeFis> CreateFisAsync(MuhasebeFis fis)
    {
        // Borc/Alacak toplamlarini hesapla
        fis.ToplamBorc = fis.Kalemler.Sum(k => k.Borc);
        fis.ToplamAlacak = fis.Kalemler.Sum(k => k.Alacak);

        // Borc = Alacak kontrolu
        if (Math.Abs(fis.ToplamBorc - fis.ToplamAlacak) > 0.01m)
            throw new Exception("Borc ve Alacak toplamlari esit olmali!");

        fis.FisTarihi = DateTime.SpecifyKind(fis.FisTarihi, DateTimeKind.Utc);
        fis.CreatedAt = DateTime.UtcNow;

        _context.MuhasebeFisleri.Add(fis);
        await _context.SaveChangesAsync();
        return fis;
    }

    public async Task<MuhasebeFis> UpdateFisAsync(MuhasebeFis fis)
    {
        var existing = await _context.MuhasebeFisleri
            .Include(f => f.Kalemler)
            .FirstOrDefaultAsync(f => f.Id == fis.Id);

        if (existing == null) throw new Exception("Fis bulunamadi");
        if (existing.Durum == FisDurum.Onaylandi) throw new Exception("Onaylanmis fis duzenlenemez");

        existing.FisTarihi = DateTime.SpecifyKind(fis.FisTarihi, DateTimeKind.Utc);
        existing.Aciklama = fis.Aciklama;
        existing.UpdatedAt = DateTime.UtcNow;

        // Mevcut kalemleri sil
        _context.MuhasebeFisKalemleri.RemoveRange(existing.Kalemler);

        // Yeni kalemleri ekle
        foreach (var kalem in fis.Kalemler)
        {
            kalem.FisId = existing.Id;
            _context.MuhasebeFisKalemleri.Add(kalem);
        }

        existing.ToplamBorc = fis.Kalemler.Sum(k => k.Borc);
        existing.ToplamAlacak = fis.Kalemler.Sum(k => k.Alacak);

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteFisAsync(int id)
    {
        var fis = await _context.MuhasebeFisleri.FindAsync(id);
        if (fis == null) return;
        if (fis.Durum == FisDurum.Onaylandi) throw new Exception("Onaylanmis fis silinemez");

        fis.IsDeleted = true;
        fis.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<string> GenerateNextFisNoAsync(FisTipi tip)
    {
        var prefix = tip switch
        {
            FisTipi.Mahsup => "MH",
            FisTipi.Tahsilat => "TH",
            FisTipi.Tediye => "TD",
            FisTipi.Acilis => "AC",
            FisTipi.Kapanis => "KP",
            FisTipi.Devir => "DV",
            _ => "MH"
        };

        var yil = DateTime.Now.Year;
        var ay = DateTime.Now.Month;

        var lastFis = await _context.MuhasebeFisleri
            .Where(f => f.FisNo.StartsWith($"{prefix}-{yil}{ay:D2}"))
            .OrderByDescending(f => f.FisNo)
            .FirstOrDefaultAsync();

        var siraNo = 1;
        if (lastFis != null)
        {
            var parts = lastFis.FisNo.Split('-');
            if (parts.Length >= 2 && int.TryParse(parts.Last(), out var lastNo))
                siraNo = lastNo + 1;
        }

        return $"{prefix}-{yil}{ay:D2}-{siraNo:D4}";
    }

    public async Task OnayliFisAsync(int fisId)
    {
        var fis = await _context.MuhasebeFisleri.FindAsync(fisId);
        if (fis == null) throw new Exception("Fis bulunamadi");

        fis.Durum = FisDurum.Onaylandi;
        fis.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Otomatik Fis Olusturma

    /// <summary>
    /// Fatura için muhasebe fişi oluşturur
    /// Giden Fatura: 120 Alıcılar BORÇ, 600 Satışlar + 391 Hesaplanan KDV ALACAK
    /// Gelen Fatura: 320 Satıcılar ALACAK, 770 Giderler + 191 İndirilecek KDV BORÇ
    /// Tevkifatlı: + 360 Sorumlu Sıfatıyla Ödenen KDV
    /// </summary>
    public async Task<MuhasebeFis> CreateFaturaFisiAsync(Fatura fatura)
    {
        // Ayarları al
        var ayar = await _context.MuhasebeAyarlari.FirstOrDefaultAsync();

        var fisNo = await GenerateNextFisNoAsync(FisTipi.Mahsup);
        var fis = new MuhasebeFis
        {
            FisNo = fisNo,
            FisTarihi = DateTime.SpecifyKind(fatura.FaturaTarihi, DateTimeKind.Utc),
            FisTipi = FisTipi.Mahsup,
            Aciklama = $"Fatura: {fatura.FaturaNo}" + (fatura.TevkifatliMi ? " (Tevkifatlı)" : ""),
            Kaynak = FisKaynak.Fatura,
            KaynakId = fatura.Id,
            KaynakTip = "Fatura",
            Durum = FisDurum.Onaylandi,
            CreatedAt = DateTime.UtcNow
        };

        var kalemler = new List<MuhasebeFisKalem>();
        var siraNo = 1;

        if (fatura.FaturaYonu == FaturaYonu.Giden)
        {
            // GIDEN FATURA - SATIŞ
            // Tevkifatlı faturada alıcıdan alınacak tutar = GenelToplam - TevkifatTutar
            var alicidanAlinacak = fatura.TevkifatliMi 
                ? fatura.GenelToplam - fatura.TevkifatTutar 
                : fatura.GenelToplam;

            // 120 Alıcılar BORÇ
            var alicilarHesap = await GetOrCreateCariHesapAsync("120", fatura.CariId);
            kalemler.Add(new MuhasebeFisKalem
            {
                HesapId = alicilarHesap.Id,
                Borc = alicidanAlinacak,
                Alacak = 0,
                Aciklama = $"Fatura: {fatura.FaturaNo}",
                CariId = fatura.CariId,
                SiraNo = siraNo++
            });

            // Tevkifat varsa - 136 Diğer Çeşitli Alacaklar BORÇ (Tevkifattan alacak)
            if (fatura.TevkifatliMi && fatura.TevkifatTutar > 0)
            {
                var tevkifatHesapKodu = ayar?.TevkifatAlacakHesabi ?? "136.01";
                var tevkifatAlacakHesap = await GetHesapByKodAsync(tevkifatHesapKodu) ?? await GetHesapByKodAsync("136");
                if (tevkifatAlacakHesap != null)
                {
                    kalemler.Add(new MuhasebeFisKalem
                    {
                        HesapId = tevkifatAlacakHesap.Id,
                        Borc = fatura.TevkifatTutar,
                        Alacak = 0,
                        Aciklama = $"Tevkifat Alacağı ({fatura.TevkifatKodu})",
                        SiraNo = siraNo++
                    });
                }
            }

            // 600 Satışlar ALACAK (AraToplam) - Fatura kalemlerine göre
            var satisGelirHesapKodu = ayar?.SatisGelirHesabi ?? "600.01";
            var satislarHesap = await GetHesapByKodAsync(satisGelirHesapKodu) ?? await GetHesapByKodAsync("600");
            if (satislarHesap != null)
            {
                kalemler.Add(new MuhasebeFisKalem
                {
                    HesapId = satislarHesap.Id,
                    Borc = 0,
                    Alacak = fatura.AraToplam,
                    Aciklama = "Satış Geliri",
                    SiraNo = siraNo++
                });
            }

            // 391 Hesaplanan KDV ALACAK
            if (fatura.KdvTutar > 0)
            {
                var kdvHesapKodu = ayar?.HesaplananKdvHesabi ?? "391.01";
                var kdvHesap = await GetHesapByKodAsync(kdvHesapKodu) ?? await GetHesapByKodAsync("391");
                if (kdvHesap != null)
                {
                    kalemler.Add(new MuhasebeFisKalem
                    {
                        HesapId = kdvHesap.Id,
                        Borc = 0,
                        Alacak = fatura.KdvTutar,
                        Aciklama = "Hesaplanan KDV",
                        SiraNo = siraNo++
                    });
                }
            }
        }
        else
        {
            // GELEN FATURA - ALIŞ
            // Tevkifatlı faturada satıcıya ödenecek = GenelToplam - TevkifatTutar
            var saticiyaOdenecek = fatura.TevkifatliMi 
                ? fatura.GenelToplam - fatura.TevkifatTutar 
                : fatura.GenelToplam;

            // 320 Satıcılar ALACAK
            var saticilarHesap = await GetOrCreateCariHesapAsync("320", fatura.CariId);
            kalemler.Add(new MuhasebeFisKalem
            {
                HesapId = saticilarHesap.Id,
                Borc = 0,
                Alacak = saticiyaOdenecek,
                Aciklama = $"Fatura: {fatura.FaturaNo}",
                CariId = fatura.CariId,
                SiraNo = siraNo++
            });

            // Tevkifat varsa - 360 Sorumlu Sıfatıyla Ödenen KDV ALACAK
            if (fatura.TevkifatliMi && fatura.TevkifatTutar > 0)
            {
                var tevkifatKdvHesapKodu = ayar?.TevkifatKdvHesabi ?? "360.01";
                var tevkifatKdvHesap = await GetHesapByKodAsync(tevkifatKdvHesapKodu) ?? await GetHesapByKodAsync("360");
                if (tevkifatKdvHesap != null)
                {
                    kalemler.Add(new MuhasebeFisKalem
                    {
                        HesapId = tevkifatKdvHesap.Id,
                        Borc = 0,
                        Alacak = fatura.TevkifatTutar,
                        Aciklama = $"Sorumlu Sıfatıyla Ödenen KDV ({fatura.TevkifatKodu})",
                        SiraNo = siraNo++
                    });
                }
            }

            // 770 Gider veya 153 Ticari Mal BORÇ - Fatura kalemlerine göre
            var giderHesapKodu = ayar?.AlisGiderHesabi ?? "770.01";
            var giderHesap = await GetHesapByKodAsync(giderHesapKodu) ?? await GetHesapByKodAsync("770");
            if (giderHesap != null)
            {
                kalemler.Add(new MuhasebeFisKalem
                {
                    HesapId = giderHesap.Id,
                    Borc = fatura.AraToplam,
                    Alacak = 0,
                    Aciklama = "Gider",
                    SiraNo = siraNo++
                });
            }

            // 191 İndirilecek KDV BORÇ (Tevkifatsız kısım)
            var indirilecekKdv = fatura.TevkifatliMi 
                ? fatura.KdvTutar - fatura.TevkifatTutar 
                : fatura.KdvTutar;

            if (indirilecekKdv > 0)
            {
                var kdvHesapKodu = ayar?.IndirilecekKdvHesabi ?? "191.01";
                var kdvHesap = await GetHesapByKodAsync(kdvHesapKodu) ?? await GetHesapByKodAsync("191");
                if (kdvHesap != null)
                {
                    kalemler.Add(new MuhasebeFisKalem
                    {
                        HesapId = kdvHesap.Id,
                        Borc = indirilecekKdv,
                        Alacak = 0,
                        Aciklama = "İndirilecek KDV",
                        SiraNo = siraNo++
                    });
                }
            }

            // Tevkifat KDV'si de indirilecek KDV olarak kaydedilir
            if (fatura.TevkifatliMi && fatura.TevkifatTutar > 0)
            {
                var kdvHesapKodu = ayar?.IndirilecekKdvHesabi ?? "191.01";
                var kdvHesap = await GetHesapByKodAsync(kdvHesapKodu) ?? await GetHesapByKodAsync("191");
                if (kdvHesap != null)
                {
                    kalemler.Add(new MuhasebeFisKalem
                    {
                        HesapId = kdvHesap.Id,
                        Borc = fatura.TevkifatTutar,
                        Alacak = 0,
                        Aciklama = "Tevkifat KDV (İndirilecek)",
                        SiraNo = siraNo++
                    });
                }
            }
        }

        fis.Kalemler = kalemler;
        fis.ToplamBorc = kalemler.Sum(k => k.Borc);
        fis.ToplamAlacak = kalemler.Sum(k => k.Alacak);

        _context.MuhasebeFisleri.Add(fis);
        await _context.SaveChangesAsync();

        // Faturaya fiş ID'sini kaydet
        var faturaEntity = await _context.Faturalar.FindAsync(fatura.Id);
        if (faturaEntity != null)
        {
            faturaEntity.MuhasebeFisiOlusturuldu = true;
            faturaEntity.MuhasebeFisId = fis.Id;
            await _context.SaveChangesAsync();
        }

        return fis;
    }

    /// <summary>
    /// Tahsilat fisi: 100/102 Kasa/Banka BORC, 120 Alicilar ALACAK
    /// </summary>
    public async Task<MuhasebeFis> CreateTahsilatFisiAsync(BankaKasaHareket hareket, int faturaId)
    {
        var fatura = await _context.Faturalar.FindAsync(faturaId);
        if (fatura == null)
            throw new Exception("Fatura bulunamadi");

        var fisNo = await GenerateNextFisNoAsync(FisTipi.Tahsilat);

        var bankaHesap = await _context.BankaHesaplari.FindAsync(hareket.BankaHesapId);
        if (bankaHesap == null)
            throw new Exception("Banka/Kasa hesabi bulunamadi");

        var kasaBankaHesap = bankaHesap.HesapTipi == HesapTipi.Kasa
            ? await GetHesapByKodAsync("100")
            : await GetHesapByKodAsync("102");

        if (kasaBankaHesap == null)
            throw new Exception("Kasa/Banka muhasebe hesabi bulunamadi");

        var alicilarHesap = await GetOrCreateCariHesapAsync("120", fatura.CariId);

        var fis = new MuhasebeFis
        {
            FisNo = fisNo,
            FisTarihi = DateTime.SpecifyKind(hareket.IslemTarihi, DateTimeKind.Utc),
            FisTipi = FisTipi.Tahsilat,
            Aciklama = $"Tahsilat: {hareket.Aciklama}",
            Kaynak = FisKaynak.BankaHareket,
            KaynakId = hareket.Id,
            KaynakTip = "BankaKasaHareket",
            Durum = FisDurum.Onaylandi,
            CreatedAt = DateTime.UtcNow,
            Kalemler = new List<MuhasebeFisKalem>
            {
                new() { HesapId = kasaBankaHesap.Id, Borc = hareket.Tutar, Alacak = 0, SiraNo = 1 },
                new() { HesapId = alicilarHesap.Id, Borc = 0, Alacak = hareket.Tutar, CariId = fatura.CariId, SiraNo = 2 }
            }
        };

        fis.ToplamBorc = hareket.Tutar;
        fis.ToplamAlacak = hareket.Tutar;

        _context.MuhasebeFisleri.Add(fis);
        await _context.SaveChangesAsync();
        return fis;
    }

    /// <summary>
    /// Tediye (Odeme) fisi: 320 Saticilar BORC, 100/102 Kasa/Banka ALACAK
    /// </summary>
    public async Task<MuhasebeFis> CreateTediyeFisiAsync(BankaKasaHareket hareket, int? faturaId = null)
    {
        var fatura = faturaId.HasValue ? await _context.Faturalar.FindAsync(faturaId.Value) : null;
        var fisNo = await GenerateNextFisNoAsync(FisTipi.Tediye);

        var bankaHesap = await _context.BankaHesaplari.FindAsync(hareket.BankaHesapId);
        if (bankaHesap == null)
            throw new Exception("Banka/Kasa hesabi bulunamadi");

        var kasaBankaHesap = bankaHesap?.HesapTipi == HesapTipi.Kasa
            ? await GetHesapByKodAsync("100")
            : await GetHesapByKodAsync("102");

        if (kasaBankaHesap == null)
            throw new Exception("Kasa/Banka muhasebe hesabi bulunamadi");

        var fis = new MuhasebeFis
        {
            FisNo = fisNo,
            FisTarihi = DateTime.SpecifyKind(hareket.IslemTarihi, DateTimeKind.Utc),
            FisTipi = FisTipi.Tediye,
            Aciklama = $"Odeme: {hareket.Aciklama}",
            Kaynak = FisKaynak.BankaHareket,
            KaynakId = hareket.Id,
            KaynakTip = "BankaKasaHareket",
            Durum = FisDurum.Onaylandi,
            CreatedAt = DateTime.UtcNow,
            Kalemler = new List<MuhasebeFisKalem>()
        };

        if (fatura != null)
        {
            // Faturali odeme: 320 Saticilar BORC
            var saticilarHesap = await GetOrCreateCariHesapAsync("320", fatura.CariId);
            fis.Kalemler.Add(new MuhasebeFisKalem { HesapId = saticilarHesap.Id, Borc = hareket.Tutar, Alacak = 0, CariId = fatura.CariId, SiraNo = 1 });
        }
        else
        {
            // Genel odeme: 770 Gider BORC
            var giderHesap = await GetHesapByKodAsync("770");
            if (giderHesap == null)
                throw new Exception("Gider muhasebe hesabi bulunamadi");

            fis.Kalemler.Add(new MuhasebeFisKalem { HesapId = giderHesap.Id, Borc = hareket.Tutar, Alacak = 0, SiraNo = 1 });
        }

        // Kasa/Banka ALACAK
        fis.Kalemler.Add(new MuhasebeFisKalem { HesapId = kasaBankaHesap.Id, Borc = 0, Alacak = hareket.Tutar, SiraNo = 2 });

        fis.ToplamBorc = hareket.Tutar;
        fis.ToplamAlacak = hareket.Tutar;

        _context.MuhasebeFisleri.Add(fis);
        await _context.SaveChangesAsync();
        return fis;
    }

    private async Task<MuhasebeHesap> GetOrCreateCariHesapAsync(string ustHesapKodu, int? cariId)
    {
        var ustHesap = await GetHesapByKodAsync(ustHesapKodu);
        if (ustHesap == null)
            throw new Exception($"Ust hesap {ustHesapKodu} bulunamadi");

        if (!cariId.HasValue)
            return ustHesap;

        var cari = await _context.Cariler.FindAsync(cariId.Value);
        if (cari == null)
            return ustHesap;

        // Cari alt hesabi var mi?
        var cariAltKod = BuildCariAltKod(cari);
        var cariHesapKodu = $"{ustHesapKodu}.{cariAltKod}";
        var cariHesap = await GetHesapByKodAsync(cariHesapKodu);

        if (cariHesap == null)
        {
            // Olustur
            cariHesap = new MuhasebeHesap
            {
                HesapKodu = cariHesapKodu,
                HesapAdi = cari.Unvan,
                HesapTuru = ustHesap.HesapTuru,
                HesapGrubu = ustHesap.HesapGrubu,
                UstHesapId = ustHesap.Id,
                SistemHesabi = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.MuhasebeHesaplari.Add(cariHesap);

            // Ust hesabin AltHesapVar flag'ini guncelle
            ustHesap.AltHesapVar = true;

            await _context.SaveChangesAsync();
        }

        return cariHesap;
    }

    private string BuildCariAltKod(Cari cari)
    {
        var kaynakKod = string.IsNullOrWhiteSpace(cari.CariKodu)
            ? cari.Id.ToString()
            : cari.CariKodu;

        var temizKod = NormalizeHesapKodu(kaynakKod);
        temizKod = new string(temizKod.Where(ch => char.IsLetterOrDigit(ch) || ch == '.').ToArray());

        return string.IsNullOrWhiteSpace(temizKod)
            ? cari.Id.ToString()
            : temizKod;
    }

    #endregion

    #region Donemler

    public async Task<List<MuhasebeDonem>> GetDonemlerAsync(int yil)
    {
        return await _context.MuhasebeDonemleri
            .Where(d => d.Yil == yil)
            .OrderBy(d => d.Ay)
            .ToListAsync();
    }

    public async Task<MuhasebeDonem?> GetAktifDonemAsync()
    {
        return await _context.MuhasebeDonemleri
            .Where(d => d.Durum == DonemDurum.Acik)
            .OrderByDescending(d => d.Yil)
            .ThenByDescending(d => d.Ay)
            .FirstOrDefaultAsync();
    }

    public async Task DonemKapatAsync(int donemId)
    {
        var donem = await _context.MuhasebeDonemleri.FindAsync(donemId);
        if (donem == null) throw new Exception("Donem bulunamadi");

        donem.Durum = DonemDurum.Kapali;
        donem.KapanisTarihi = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Raporlar

    public async Task<MuavinRapor> GetMuavinRaporuAsync(string hesapKodu, DateTime baslangic, DateTime bitis)
    {
        var hesap = await GetHesapByKodAsync(hesapKodu);
        if (hesap == null)
            throw new Exception("Hesap bulunamadi");

        var baslangicUtc = DateTime.SpecifyKind(baslangic.Date, DateTimeKind.Utc);
        var bitisUtc = DateTime.SpecifyKind(bitis.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

        // Alt hesaplari da dahil et
        var hesapKodlari = new List<string> { hesapKodu };
        var altHesaplar = await _context.MuhasebeHesaplari
            .Where(h => h.HesapKodu.StartsWith(hesapKodu + "."))
            .Select(h => h.HesapKodu)
            .ToListAsync();
        hesapKodlari.AddRange(altHesaplar);

        var hesapIds = await _context.MuhasebeHesaplari
            .Where(h => hesapKodlari.Contains(h.HesapKodu))
            .Select(h => h.Id)
            .ToListAsync();

        // Devir (onceki donem toplamlar)
        var devirKalemler = await _context.MuhasebeFisKalemleri
            .Include(k => k.Fis)
            .Where(k => hesapIds.Contains(k.HesapId) && k.Fis.FisTarihi < baslangicUtc && k.Fis.Durum == FisDurum.Onaylandi)
            .ToListAsync();

        var devirBorc = devirKalemler.Sum(k => k.Borc);
        var devirAlacak = devirKalemler.Sum(k => k.Alacak);

        // Donem hareketleri
        var kalemler = await _context.MuhasebeFisKalemleri
            .Include(k => k.Fis)
            .Include(k => k.Hesap)
            .Where(k => hesapIds.Contains(k.HesapId) && 
                       k.Fis.FisTarihi >= baslangicUtc && 
                       k.Fis.FisTarihi <= bitisUtc &&
                       k.Fis.Durum == FisDurum.Onaylandi)
            .OrderBy(k => k.Fis.FisTarihi)
            .ThenBy(k => k.Fis.FisNo)
            .ToListAsync();

        var rapor = new MuavinRapor
        {
            HesapKodu = hesapKodu,
            HesapAdi = hesap.HesapAdi,
            BaslangicTarihi = baslangic,
            BitisTarihi = bitis,
            DevirBorc = devirBorc,
            DevirAlacak = devirAlacak,
            ToplamBorc = kalemler.Sum(k => k.Borc),
            ToplamAlacak = kalemler.Sum(k => k.Alacak)
        };

        decimal bakiye = devirBorc - devirAlacak;
        foreach (var kalem in kalemler)
        {
            bakiye += kalem.Borc - kalem.Alacak;
            rapor.Satirlar.Add(new MuavinSatir
            {
                Tarih = kalem.Fis.FisTarihi,
                FisNo = kalem.Fis.FisNo,
                Aciklama = kalem.Aciklama ?? kalem.Fis.Aciklama ?? "",
                Borc = kalem.Borc,
                Alacak = kalem.Alacak,
                Bakiye = bakiye
            });
        }

        rapor.Bakiye = bakiye;
        return rapor;
    }

    public async Task<YevmiyeRapor> GetYevmiyeRaporuAsync(DateTime baslangic, DateTime bitis)
    {
        var baslangicUtc = DateTime.SpecifyKind(baslangic.Date, DateTimeKind.Utc);
        var bitisUtc = DateTime.SpecifyKind(bitis.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

        var kalemler = await _context.MuhasebeFisKalemleri
            .Include(k => k.Fis)
            .Include(k => k.Hesap)
            .Where(k => k.Fis.FisTarihi >= baslangicUtc && 
                       k.Fis.FisTarihi <= bitisUtc &&
                       k.Fis.Durum == FisDurum.Onaylandi)
            .OrderBy(k => k.Fis.FisTarihi)
            .ThenBy(k => k.Fis.FisNo)
            .ThenBy(k => k.SiraNo)
            .ToListAsync();

        var rapor = new YevmiyeRapor
        {
            BaslangicTarihi = baslangic,
            BitisTarihi = bitis,
            ToplamBorc = kalemler.Sum(k => k.Borc),
            ToplamAlacak = kalemler.Sum(k => k.Alacak)
        };

        int siraNo = 1;
        foreach (var kalem in kalemler)
        {
            rapor.Satirlar.Add(new YevmiyeSatir
            {
                SiraNo = siraNo++,
                Tarih = kalem.Fis.FisTarihi,
                FisNo = kalem.Fis.FisNo,
                HesapKodu = kalem.Hesap.HesapKodu,
                HesapAdi = kalem.Hesap.HesapAdi,
                Aciklama = kalem.Aciklama ?? "",
                Borc = kalem.Borc,
                Alacak = kalem.Alacak
            });
        }

        return rapor;
    }

    public async Task<GelirGiderRapor> GetGelirGiderRaporuAsync(int yil, int? ay = null)
    {
        var gelirHesaplar = await _context.MuhasebeHesaplari
            .Where(h => h.HesapGrubu == HesapGrubu.GelirTablosu && h.HesapTuru == HesapTuru.Gelir)
            .ToListAsync();

        var giderHesaplar = await _context.MuhasebeHesaplari
            .Where(h => (h.HesapGrubu == HesapGrubu.GelirTablosu || h.HesapGrubu == HesapGrubu.MaliyetHesaplari) && 
                       (h.HesapTuru == HesapTuru.Gider || h.HesapTuru == HesapTuru.Maliyet))
            .ToListAsync();

        var gelirIds = gelirHesaplar.Select(h => h.Id).ToList();
        var giderIds = giderHesaplar.Select(h => h.Id).ToList();

        var query = _context.MuhasebeFisKalemleri
            .Include(k => k.Fis)
            .Include(k => k.Hesap)
            .Where(k => k.Fis.FisTarihi.Year == yil && k.Fis.Durum == FisDurum.Onaylandi);

        if (ay.HasValue)
            query = query.Where(k => k.Fis.FisTarihi.Month == ay.Value);

        var kalemler = await query.ToListAsync();

        var rapor = new GelirGiderRapor { Yil = yil, Ay = ay };

        // Gelirler
        var gelirKalemler = kalemler.Where(k => gelirIds.Contains(k.HesapId)).ToList();
        rapor.ToplamGelir = gelirKalemler.Sum(k => k.Alacak - k.Borc);
        rapor.Gelirler = gelirKalemler
            .GroupBy(k => new { k.Hesap.HesapKodu, k.Hesap.HesapAdi })
            .Select(g => new GelirGiderKalem
            {
                HesapKodu = g.Key.HesapKodu,
                HesapAdi = g.Key.HesapAdi,
                Tutar = g.Sum(k => k.Alacak - k.Borc)
            })
            .OrderByDescending(g => g.Tutar)
            .ToList();

        // Giderler
        var giderKalemler = kalemler.Where(k => giderIds.Contains(k.HesapId)).ToList();
        rapor.ToplamGider = giderKalemler.Sum(k => k.Borc - k.Alacak);
        rapor.Giderler = giderKalemler
            .GroupBy(k => new { k.Hesap.HesapKodu, k.Hesap.HesapAdi })
            .Select(g => new GelirGiderKalem
            {
                HesapKodu = g.Key.HesapKodu,
                HesapAdi = g.Key.HesapAdi,
                Tutar = g.Sum(k => k.Borc - k.Alacak)
            })
            .OrderByDescending(g => g.Tutar)
            .ToList();

        rapor.NetKar = rapor.ToplamGelir - rapor.ToplamGider;

        // Yuzde hesapla
        foreach (var gelir in rapor.Gelirler)
            gelir.Yuzde = rapor.ToplamGelir > 0 ? Math.Round(gelir.Tutar / rapor.ToplamGelir * 100, 1) : 0;
        foreach (var gider in rapor.Giderler)
            gider.Yuzde = rapor.ToplamGider > 0 ? Math.Round(gider.Tutar / rapor.ToplamGider * 100, 1) : 0;

        // Aylik detay
        if (!ay.HasValue)
        {
            for (int m = 1; m <= 12; m++)
            {
                var aylikKalemler = kalemler.Where(k => k.Fis.FisTarihi.Month == m).ToList();
                var ayGelir = aylikKalemler.Where(k => gelirIds.Contains(k.HesapId)).Sum(k => k.Alacak - k.Borc);
                var ayGider = aylikKalemler.Where(k => giderIds.Contains(k.HesapId)).Sum(k => k.Borc - k.Alacak);

                rapor.AylikDetay.Add(new AylikGelirGider
                {
                    Ay = m,
                    AyAdi = AyAdlari[m],
                    Gelir = ayGelir,
                    Gider = ayGider,
                    Net = ayGelir - ayGider
                });
            }
        }

        return rapor;
    }

    public async Task<BilancoRapor> GetBilancoRaporuAsync(DateTime tarih)
    {
        var tarihUtc = DateTime.SpecifyKind(tarih.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

        var hesaplar = await _context.MuhasebeHesaplari.ToListAsync();

        var kalemler = await _context.MuhasebeFisKalemleri
            .Include(k => k.Fis)
            .Where(k => k.Fis.FisTarihi <= tarihUtc && k.Fis.Durum == FisDurum.Onaylandi)
            .ToListAsync();

        var rapor = new BilancoRapor { Tarih = tarih };

        // Hesap bakiyeleri hesapla
        var bakiyeler = kalemler
            .GroupBy(k => k.HesapId)
            .Select(g => new
            {
                HesapId = g.Key,
                Borc = g.Sum(k => k.Borc),
                Alacak = g.Sum(k => k.Alacak),
                Bakiye = g.Sum(k => k.Borc) - g.Sum(k => k.Alacak)
            })
            .ToDictionary(b => b.HesapId, b => b.Bakiye);

        // Aktif kalemler
        rapor.DonenVarliklar = GetBilancoKalemler(hesaplar, bakiyeler, HesapGrubu.DonenVarliklar);
        rapor.DuranVarliklar = GetBilancoKalemler(hesaplar, bakiyeler, HesapGrubu.DuranVarliklar);

        // Pasif kalemler (isareti ters)
        rapor.KisaVadeliYabanciKaynaklar = GetBilancoKalemler(hesaplar, bakiyeler, HesapGrubu.KisaVadeliYabanciKaynaklar, true);
        rapor.UzunVadeliYabanciKaynaklar = GetBilancoKalemler(hesaplar, bakiyeler, HesapGrubu.UzunVadeliYabanciKaynaklar, true);
        rapor.Ozkaynaklar = GetBilancoKalemler(hesaplar, bakiyeler, HesapGrubu.Ozkaynaklar, true);

        rapor.ToplamAktif = rapor.DonenVarliklar.Sum(k => k.Tutar) + rapor.DuranVarliklar.Sum(k => k.Tutar);
        rapor.ToplamPasif = rapor.KisaVadeliYabanciKaynaklar.Sum(k => k.Tutar) + 
                           rapor.UzunVadeliYabanciKaynaklar.Sum(k => k.Tutar) + 
                           rapor.Ozkaynaklar.Sum(k => k.Tutar);

        return rapor;
    }

    private List<BilancoKalem> GetBilancoKalemler(List<MuhasebeHesap> tumHesaplar, Dictionary<int, decimal> bakiyeler, HesapGrubu grup, bool tersIsaret = false)
    {
        var grupHesaplar = tumHesaplar.Where(h => h.HesapGrubu == grup && !h.AltHesapVar).ToList();

        return grupHesaplar
            .Where(h => bakiyeler.ContainsKey(h.Id) && bakiyeler[h.Id] != 0)
            .Select(h => new BilancoKalem
            {
                HesapKodu = h.HesapKodu,
                HesapAdi = h.HesapAdi,
                Tutar = tersIsaret ? -bakiyeler[h.Id] : bakiyeler[h.Id]
            })
            .OrderBy(k => k.HesapKodu)
            .ToList();
    }

    public async Task<MizanRapor> GetMizanRaporuAsync(DateTime baslangic, DateTime bitis)
    {
        var baslangicUtc = DateTime.SpecifyKind(baslangic.Date, DateTimeKind.Utc);
        var bitisUtc = DateTime.SpecifyKind(bitis.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

        var hesaplar = await _context.MuhasebeHesaplari.ToListAsync();

        var kalemler = await _context.MuhasebeFisKalemleri
            .Include(k => k.Fis)
            .Where(k => k.Fis.FisTarihi >= baslangicUtc && 
                       k.Fis.FisTarihi <= bitisUtc &&
                       k.Fis.Durum == FisDurum.Onaylandi)
            .ToListAsync();

        var bakiyeler = kalemler
            .GroupBy(k => k.HesapId)
            .Select(g => new
            {
                HesapId = g.Key,
                Borc = g.Sum(k => k.Borc),
                Alacak = g.Sum(k => k.Alacak)
            })
            .ToList();

        var rapor = new MizanRapor
        {
            BaslangicTarihi = baslangic,
            BitisTarihi = bitis
        };

        foreach (var b in bakiyeler)
        {
            var hesap = hesaplar.FirstOrDefault(h => h.Id == b.HesapId);
            if (hesap == null) continue;

            var bakiye = b.Borc - b.Alacak;
            rapor.Satirlar.Add(new MizanSatir
            {
                HesapKodu = hesap.HesapKodu,
                HesapAdi = hesap.HesapAdi,
                Borc = b.Borc,
                Alacak = b.Alacak,
                BorcBakiye = bakiye > 0 ? bakiye : 0,
                AlacakBakiye = bakiye < 0 ? -bakiye : 0
            });
        }

        rapor.Satirlar = rapor.Satirlar.OrderBy(s => s.HesapKodu).ToList();
        rapor.ToplamBorc = rapor.Satirlar.Sum(s => s.Borc);
        rapor.ToplamAlacak = rapor.Satirlar.Sum(s => s.Alacak);
        rapor.ToplamBorcBakiye = rapor.Satirlar.Sum(s => s.BorcBakiye);
        rapor.ToplamAlacakBakiye = rapor.Satirlar.Sum(s => s.AlacakBakiye);

        return rapor;
    }

    public async Task<decimal> GetHesapBakiyeAsync(string hesapKodu, DateTime? tarih = null)
    {
        var hesap = await GetHesapByKodAsync(hesapKodu);
        if (hesap == null) return 0;

        var query = _context.MuhasebeFisKalemleri
            .Include(k => k.Fis)
            .Where(k => k.HesapId == hesap.Id && k.Fis.Durum == FisDurum.Onaylandi);

        if (tarih.HasValue)
        {
            var tarihUtc = DateTime.SpecifyKind(tarih.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            query = query.Where(k => k.Fis.FisTarihi <= tarihUtc);
        }

        var kalemler = await query.ToListAsync();
        return kalemler.Sum(k => k.Borc) - kalemler.Sum(k => k.Alacak);
    }

    public async Task<List<HesapBakiye>> GetHesapBakiyeleriAsync(HesapGrubu grup, DateTime? tarih = null)
    {
        var hesaplar = await _context.MuhasebeHesaplari
            .Where(h => h.HesapGrubu == grup && h.Aktif)
            .ToListAsync();

        var query = _context.MuhasebeFisKalemleri
            .Include(k => k.Fis)
            .Where(k => hesaplar.Select(h => h.Id).Contains(k.HesapId) && k.Fis.Durum == FisDurum.Onaylandi);

        if (tarih.HasValue)
        {
            var tarihUtc = DateTime.SpecifyKind(tarih.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            query = query.Where(k => k.Fis.FisTarihi <= tarihUtc);
        }

        var kalemler = await query.ToListAsync();

        return hesaplar.Select(h =>
        {
            var hesapKalemler = kalemler.Where(k => k.HesapId == h.Id).ToList();
            return new HesapBakiye
            {
                HesapKodu = h.HesapKodu,
                HesapAdi = h.HesapAdi,
                Borc = hesapKalemler.Sum(k => k.Borc),
                Alacak = hesapKalemler.Sum(k => k.Alacak),
                Bakiye = hesapKalemler.Sum(k => k.Borc) - hesapKalemler.Sum(k => k.Alacak)
            };
        })
        .Where(b => b.Bakiye != 0)
        .OrderBy(b => b.HesapKodu)
        .ToList();
    }

    #endregion

    #region KDV Beyanname Raporu

    /// <summary>
    /// KDV Beyanname raporu oluşturur
    /// </summary>
    public async Task<KdvBeyanRapor> GetKdvBeyanRaporuAsync(int yil, int ay)
    {
        var baslangic = new DateTime(yil, ay, 1, 0, 0, 0, DateTimeKind.Utc);
        var bitis = baslangic.AddMonths(1).AddTicks(-1);

        var rapor = new KdvBeyanRapor
        {
            Yil = yil,
            Ay = ay,
            AyAdi = AyAdlari[ay]
        };

        // Faturalardan KDV bilgileri
        var faturalar = await _context.Faturalar
            .Where(f => f.FaturaTarihi >= baslangic && f.FaturaTarihi <= bitis && !f.IsDeleted)
            .ToListAsync();

        // HESAPLANAN KDV (Satış faturaları)
        var satisFaturalar = faturalar.Where(f => f.FaturaYonu == FaturaYonu.Giden).ToList();
        rapor.HesaplananKdv = satisFaturalar.Sum(f => f.KdvTutar);
        rapor.SatisTutari = satisFaturalar.Sum(f => f.AraToplam);

        // KDV oranlarına göre grupla
        rapor.HesaplananKdvDetay = satisFaturalar
            .GroupBy(f => f.KdvOrani)
            .Select(g => new KdvOranDetay
            {
                KdvOrani = g.Key,
                Matrah = g.Sum(f => f.AraToplam),
                KdvTutar = g.Sum(f => f.KdvTutar)
            })
            .OrderBy(k => k.KdvOrani)
            .ToList();

        // İNDİRİLECEK KDV (Alış faturaları)
        var alisFaturalar = faturalar.Where(f => f.FaturaYonu == FaturaYonu.Gelen).ToList();
        rapor.IndirilecekKdv = alisFaturalar.Sum(f => f.KdvTutar);
        rapor.AlisTutari = alisFaturalar.Sum(f => f.AraToplam);

        rapor.IndirilecekKdvDetay = alisFaturalar
            .GroupBy(f => f.KdvOrani)
            .Select(g => new KdvOranDetay
            {
                KdvOrani = g.Key,
                Matrah = g.Sum(f => f.AraToplam),
                KdvTutar = g.Sum(f => f.KdvTutar)
            })
            .OrderBy(k => k.KdvOrani)
            .ToList();

        // TEVKİFAT KDV
        var tevkifatliSatis = satisFaturalar.Where(f => f.TevkifatliMi).ToList();
        var tevkifatliAlis = alisFaturalar.Where(f => f.TevkifatliMi).ToList();

        rapor.TevkifatKdv = tevkifatliAlis.Sum(f => f.TevkifatTutar);
        rapor.TevkifatliSatisKdv = tevkifatliSatis.Sum(f => f.TevkifatTutar);

        // DEVREDİLEN KDV (önceki aydan)
        var oncekiAyBitis = baslangic.AddTicks(-1);
        var oncekiKdv = await GetKdvBakiyeAsync(oncekiAyBitis);
        rapor.DevredenKdv = oncekiKdv > 0 ? oncekiKdv : 0;

        // ÖDENECEK/DEVREDEN HESAPLAMA
        rapor.ToplamIndirimler = rapor.IndirilecekKdv + rapor.DevredenKdv + rapor.TevkifatKdv;
        rapor.FarkKdv = rapor.HesaplananKdv - rapor.ToplamIndirimler;

        if (rapor.FarkKdv > 0)
        {
            rapor.OdenecekKdv = rapor.FarkKdv;
            rapor.SonrakiAyaDevredenKdv = 0;
        }
        else
        {
            rapor.OdenecekKdv = 0;
            rapor.SonrakiAyaDevredenKdv = Math.Abs(rapor.FarkKdv);
        }

        return rapor;
    }

    /// <summary>
    /// Belirli bir tarihe kadar olan KDV bakiyesini hesaplar
    /// </summary>
    private async Task<decimal> GetKdvBakiyeAsync(DateTime tarih)
    {
        var tarihUtc = DateTime.SpecifyKind(tarih, DateTimeKind.Utc);

        // 191 - İndirilecek KDV bakiyesi
        var indirilecekKdvHesap = await GetHesapByKodAsync("191");
        // 391 - Hesaplanan KDV bakiyesi
        var hesaplananKdvHesap = await GetHesapByKodAsync("391");
        // 190 - Devreden KDV
        var devredenKdvHesap = await GetHesapByKodAsync("190");

        decimal indirilecek = 0, hesaplanan = 0, devreden = 0;

        if (indirilecekKdvHesap != null)
        {
            var kalemler = await _context.MuhasebeFisKalemleri
                .Include(k => k.Fis)
                .Where(k => k.HesapId == indirilecekKdvHesap.Id && 
                           k.Fis.FisTarihi <= tarihUtc && 
                           k.Fis.Durum == FisDurum.Onaylandi)
                .ToListAsync();
            indirilecek = kalemler.Sum(k => k.Borc) - kalemler.Sum(k => k.Alacak);
        }

        if (hesaplananKdvHesap != null)
        {
            var kalemler = await _context.MuhasebeFisKalemleri
                .Include(k => k.Fis)
                .Where(k => k.HesapId == hesaplananKdvHesap.Id && 
                           k.Fis.FisTarihi <= tarihUtc && 
                           k.Fis.Durum == FisDurum.Onaylandi)
                .ToListAsync();
            hesaplanan = kalemler.Sum(k => k.Alacak) - kalemler.Sum(k => k.Borc);
        }

        if (devredenKdvHesap != null)
        {
            var kalemler = await _context.MuhasebeFisKalemleri
                .Include(k => k.Fis)
                .Where(k => k.HesapId == devredenKdvHesap.Id && 
                           k.Fis.FisTarihi <= tarihUtc && 
                           k.Fis.Durum == FisDurum.Onaylandi)
                .ToListAsync();
            devreden = kalemler.Sum(k => k.Borc) - kalemler.Sum(k => k.Alacak);
        }

        // Devreden KDV = İndirilecek - Hesaplanan (pozitif ise devreden var)
        return indirilecek + devreden - hesaplanan;
    }

    /// <summary>
    /// Yıllık KDV özet raporu
    /// </summary>
    public async Task<List<KdvAylikOzet>> GetYillikKdvOzetiAsync(int yil)
    {
        var ozet = new List<KdvAylikOzet>();

        for (int ay = 1; ay <= 12; ay++)
        {
            var rapor = await GetKdvBeyanRaporuAsync(yil, ay);
            ozet.Add(new KdvAylikOzet
            {
                Ay = ay,
                AyAdi = AyAdlari[ay],
                HesaplananKdv = rapor.HesaplananKdv,
                IndirilecekKdv = rapor.IndirilecekKdv,
                TevkifatKdv = rapor.TevkifatKdv,
                DevredenKdv = rapor.DevredenKdv,
                OdenecekKdv = rapor.OdenecekKdv,
                SonrakiAyaDevreden = rapor.SonrakiAyaDevredenKdv
            });
        }

        return ozet;
    }

    #endregion

    #region Mahsup Fişi Oluşturma

    /// <summary>
    /// Hesaplar arası transfer için muhasebe fişi oluşturur.
    /// Kaynak hesap ALACAK, Hedef hesap BORÇ kaydedilir.
    /// Örnek: Kasadan Bankaya transfer -> 102 Banka BORÇ, 100 Kasa ALACAK
    /// </summary>
    public async Task<MuhasebeFis?> CreateHesapTransferFisiAsync(
        BankaKasaHareket cikisHareket, 
        BankaKasaHareket girisHareket,
        BankaHesap kaynakHesap, 
        BankaHesap hedefHesap)
    {
        // Hesap tipine göre muhasebe hesap kodlarını belirle
        var kaynakMuhasebeKodu = kaynakHesap.VarsayilanMuhasebeKodu ?? GetDefaultMuhasebeKodu(kaynakHesap.HesapTipi);
        var hedefMuhasebeKodu = hedefHesap.VarsayilanMuhasebeKodu ?? GetDefaultMuhasebeKodu(hedefHesap.HesapTipi);

        var kaynakMuhasebeHesap = await GetHesapByKodAsync(kaynakMuhasebeKodu);
        var hedefMuhasebeHesap = await GetHesapByKodAsync(hedefMuhasebeKodu);

        // Muhasebe hesapları yoksa null dön (muhasebe entegrasyonu aktif değil)
        if (kaynakMuhasebeHesap == null || hedefMuhasebeHesap == null)
            return null;

        var fisNo = await GenerateNextFisNoAsync(FisTipi.Mahsup);

        var fis = new MuhasebeFis
        {
            FisNo = fisNo,
            FisTarihi = DateTime.SpecifyKind(cikisHareket.IslemTarihi, DateTimeKind.Utc),
            FisTipi = FisTipi.Mahsup,
            Aciklama = $"Hesaplar Arası Transfer: {kaynakHesap.HesapAdi} → {hedefHesap.HesapAdi}",
            Kaynak = FisKaynak.BankaHareket,
            KaynakId = cikisHareket.Id,
            KaynakTip = "HesapTransfer",
            Durum = FisDurum.Onaylandi,
            CreatedAt = DateTime.UtcNow,
            Kalemler = new List<MuhasebeFisKalem>
            {
                // Hedef hesap BORÇ (para giriyor)
                new()
                {
                    HesapId = hedefMuhasebeHesap.Id,
                    Borc = girisHareket.Tutar,
                    Alacak = 0,
                    Aciklama = $"Transfer girişi: {kaynakHesap.HesapAdi}'ndan",
                    SiraNo = 1
                },
                // Kaynak hesap ALACAK (para çıkıyor)
                new()
                {
                    HesapId = kaynakMuhasebeHesap.Id,
                    Borc = 0,
                    Alacak = cikisHareket.Tutar,
                    Aciklama = $"Transfer çıkışı: {hedefHesap.HesapAdi}'na",
                    SiraNo = 2
                }
            }
        };

        fis.ToplamBorc = fis.Kalemler.Sum(k => k.Borc);
        fis.ToplamAlacak = fis.Kalemler.Sum(k => k.Alacak);

        _context.MuhasebeFisleri.Add(fis);
        await _context.SaveChangesAsync();

        return fis;
    }

    /// <summary>
    /// Cari mahsup için muhasebe fişi oluşturur.
    /// Tahsilat: Kasa/Banka BORÇ, 120 Alıcılar ALACAK
    /// Ödeme: 320 Satıcılar BORÇ, Kasa/Banka ALACAK
    /// </summary>
    public async Task<MuhasebeFis?> CreateCariMahsupFisiAsync(
        BankaKasaHareket hareket, 
        Cari cari, 
        BankaHesap hesap,
        bool tahsilatMi)
    {
        var hesapMuhasebeKodu = hesap.VarsayilanMuhasebeKodu ?? GetDefaultMuhasebeKodu(hesap.HesapTipi);
        var kasaBankaHesap = await GetHesapByKodAsync(hesapMuhasebeKodu);

        if (kasaBankaHesap == null)
            return null;

        // Cari hesap kodunu belirle (Müşteri: 120, Tedarikçi: 320)
        var cariUstKod = tahsilatMi ? "120" : "320";
        var cariHesap = await GetOrCreateCariHesapAsync(cariUstKod, cari.Id);

        var fisNo = await GenerateNextFisNoAsync(tahsilatMi ? FisTipi.Tahsilat : FisTipi.Tediye);

        var fis = new MuhasebeFis
        {
            FisNo = fisNo,
            FisTarihi = DateTime.SpecifyKind(hareket.IslemTarihi, DateTimeKind.Utc),
            FisTipi = tahsilatMi ? FisTipi.Tahsilat : FisTipi.Tediye,
            Aciklama = $"Cari Mahsup: {cari.Unvan} - {(tahsilatMi ? "Tahsilat" : "Ödeme")}",
            Kaynak = FisKaynak.BankaHareket,
            KaynakId = hareket.Id,
            KaynakTip = "CariMahsup",
            Durum = FisDurum.Onaylandi,
            CreatedAt = DateTime.UtcNow,
            Kalemler = new List<MuhasebeFisKalem>()
        };

        if (tahsilatMi)
        {
            // Tahsilat: Kasa/Banka BORÇ, Alıcılar ALACAK
            fis.Kalemler.Add(new MuhasebeFisKalem
            {
                HesapId = kasaBankaHesap.Id,
                Borc = hareket.Tutar,
                Alacak = 0,
                Aciklama = $"Tahsilat: {cari.Unvan}",
                SiraNo = 1
            });
            fis.Kalemler.Add(new MuhasebeFisKalem
            {
                HesapId = cariHesap.Id,
                Borc = 0,
                Alacak = hareket.Tutar,
                CariId = cari.Id,
                Aciklama = $"Tahsilat: {hesap.HesapAdi}",
                SiraNo = 2
            });
        }
        else
        {
            // Ödeme: Satıcılar BORÇ, Kasa/Banka ALACAK
            fis.Kalemler.Add(new MuhasebeFisKalem
            {
                HesapId = cariHesap.Id,
                Borc = hareket.Tutar,
                Alacak = 0,
                CariId = cari.Id,
                Aciklama = $"Ödeme: {hesap.HesapAdi}",
                SiraNo = 1
            });
            fis.Kalemler.Add(new MuhasebeFisKalem
            {
                HesapId = kasaBankaHesap.Id,
                Borc = 0,
                Alacak = hareket.Tutar,
                Aciklama = $"Ödeme: {cari.Unvan}",
                SiraNo = 2
            });
        }

        fis.ToplamBorc = fis.Kalemler.Sum(k => k.Borc);
        fis.ToplamAlacak = fis.Kalemler.Sum(k => k.Alacak);

        _context.MuhasebeFisleri.Add(fis);
        await _context.SaveChangesAsync();

        return fis;
    }

    /// <summary>
    /// Mahsup iptal edildiğinde ters kayıt (storno) fişi oluşturur.
    /// </summary>
    public async Task IptalFisiOlusturAsync(Guid mahsupGrupId)
    {
        // İlişkili muhasebe fişini bul
        var mevcutFisler = await _context.MuhasebeFisleri
            .Include(f => f.Kalemler)
                .ThenInclude(k => k.Hesap)
            .Where(f => f.KaynakTip == "HesapTransfer" || f.KaynakTip == "CariMahsup")
            .ToListAsync();

        // MahsupGrupId ile eşleşen hareketlerin KaynakId'lerini bul
        var iliskiliHareketler = await _context.BankaKasaHareketleri
            .Where(h => h.MahsupGrupId == mahsupGrupId)
            .Select(h => h.Id)
            .ToListAsync();

        var iptalEdilecekFisler = mevcutFisler
            .Where(f => f.KaynakId.HasValue && iliskiliHareketler.Contains(f.KaynakId.Value))
            .ToList();

        foreach (var eskiFis in iptalEdilecekFisler)
        {
            if (eskiFis.Durum == FisDurum.IptalEdildi)
                continue;

            // Eski fişi iptal et
            eskiFis.Durum = FisDurum.IptalEdildi;
            eskiFis.UpdatedAt = DateTime.UtcNow;

            // Ters kayıt fişi oluştur
            var fisNo = await GenerateNextFisNoAsync(eskiFis.FisTipi);
            var tersFis = new MuhasebeFis
            {
                FisNo = fisNo,
                FisTarihi = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc),
                FisTipi = eskiFis.FisTipi,
                Aciklama = $"[İPTAL] {eskiFis.Aciklama} - Orijinal Fiş: {eskiFis.FisNo}",
                Kaynak = FisKaynak.Otomatik,
                KaynakId = eskiFis.Id,
                KaynakTip = "IptalKaydi",
                Durum = FisDurum.Onaylandi,
                CreatedAt = DateTime.UtcNow,
                Kalemler = new List<MuhasebeFisKalem>()
            };

            // Borç ve alacakları ters çevir
            int siraNo = 1;
            foreach (var kalem in eskiFis.Kalemler)
            {
                tersFis.Kalemler.Add(new MuhasebeFisKalem
                {
                    HesapId = kalem.HesapId,
                    Borc = kalem.Alacak,  // Alacağı borç yap
                    Alacak = kalem.Borc,  // Borcu alacak yap
                    CariId = kalem.CariId,
                    Aciklama = $"[İPTAL] {kalem.Aciklama}",
                    SiraNo = siraNo++
                });
            }

            tersFis.ToplamBorc = tersFis.Kalemler.Sum(k => k.Borc);
            tersFis.ToplamAlacak = tersFis.Kalemler.Sum(k => k.Alacak);

            _context.MuhasebeFisleri.Add(tersFis);
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Hesap tipine göre varsayılan muhasebe kodunu döndürür.
    /// </summary>
    private string GetDefaultMuhasebeKodu(HesapTipi tip) => tip switch
    {
        HesapTipi.Kasa => "100",
        HesapTipi.VadesizHesap => "102",
        HesapTipi.VadeliHesap => "102",
        HesapTipi.KrediHesabi => "300",
        HesapTipi.KrediKarti => "103",  // Veya 300 Kredi
        _ => "102"
    };

    #endregion

    #region Nakit Akış Raporu

    /// <summary>
    /// Nakit akış raporu oluşturur
    /// </summary>
    public async Task<NakitAkisRapor> GetNakitAkisRaporuAsync(int yil, int? ay = null)
    {
        var rapor = new NakitAkisRapor { Yil = yil, Ay = ay };

        // Kasa ve Banka hesapları
        var kasaHesaplar = await _context.MuhasebeHesaplari
            .Where(h => h.HesapKodu.StartsWith("100"))
            .Select(h => h.Id)
            .ToListAsync();

        var bankaHesaplar = await _context.MuhasebeHesaplari
            .Where(h => h.HesapKodu.StartsWith("102"))
            .Select(h => h.Id)
            .ToListAsync();

        var nakitHesaplar = kasaHesaplar.Concat(bankaHesaplar).ToList();

        // Dönem başı bakiye
        var donemBasiTarih = ay.HasValue 
            ? new DateTime(yil, ay.Value, 1, 0, 0, 0, DateTimeKind.Utc)
            : new DateTime(yil, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var oncekiKalemler = await _context.MuhasebeFisKalemleri
            .Include(k => k.Fis)
            .Where(k => nakitHesaplar.Contains(k.HesapId) && 
                       k.Fis.FisTarihi < donemBasiTarih &&
                       k.Fis.Durum == FisDurum.Onaylandi)
            .ToListAsync();

        rapor.DonemBasiBakiye = oncekiKalemler.Sum(k => k.Borc) - oncekiKalemler.Sum(k => k.Alacak);

        // Dönem içi hareketler
        var donemBitisTarih = ay.HasValue
            ? donemBasiTarih.AddMonths(1).AddTicks(-1)
            : new DateTime(yil, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        var donemKalemler = await _context.MuhasebeFisKalemleri
            .Include(k => k.Fis)
            .Include(k => k.Hesap)
            .Where(k => nakitHesaplar.Contains(k.HesapId) && 
                       k.Fis.FisTarihi >= donemBasiTarih &&
                       k.Fis.FisTarihi <= donemBitisTarih &&
                       k.Fis.Durum == FisDurum.Onaylandi)
            .ToListAsync();

        rapor.ToplamGiris = donemKalemler.Sum(k => k.Borc);
        rapor.ToplamCikis = donemKalemler.Sum(k => k.Alacak);
        rapor.DonemSonuBakiye = rapor.DonemBasiBakiye + rapor.ToplamGiris - rapor.ToplamCikis;

        // Hareket detayları - Fiş tiplerine göre grupla
        var fisTipleriGiris = donemKalemler
            .Where(k => k.Borc > 0)
            .GroupBy(k => k.Fis.FisTipi)
            .Select(g => new NakitHareketDetay
            {
                Tur = g.Key.ToString(),
                Tutar = g.Sum(k => k.Borc)
            })
            .ToList();

        var fisTipleriCikis = donemKalemler
            .Where(k => k.Alacak > 0)
            .GroupBy(k => k.Fis.FisTipi)
            .Select(g => new NakitHareketDetay
            {
                Tur = g.Key.ToString(),
                Tutar = g.Sum(k => k.Alacak)
            })
            .ToList();

        rapor.GirisDetay = fisTipleriGiris;
        rapor.CikisDetay = fisTipleriCikis;

        // Aylık detay (yıllık raporda)
        if (!ay.HasValue)
        {
            decimal baslangicBakiye = rapor.DonemBasiBakiye;
            for (int m = 1; m <= 12; m++)
            {
                var ayBaslangic = new DateTime(yil, m, 1, 0, 0, 0, DateTimeKind.Utc);
                var ayBitis = ayBaslangic.AddMonths(1).AddTicks(-1);

                var ayKalemler = donemKalemler
                    .Where(k => k.Fis.FisTarihi >= ayBaslangic && k.Fis.FisTarihi <= ayBitis)
                    .ToList();

                var giris = ayKalemler.Sum(k => k.Borc);
                var cikis = ayKalemler.Sum(k => k.Alacak);
                var sonBakiye = baslangicBakiye + giris - cikis;

                rapor.AylikDetay.Add(new NakitAylikOzet
                {
                    Ay = m,
                    AyAdi = AyAdlari[m],
                    BaslangicBakiye = baslangicBakiye,
                    Giris = giris,
                    Cikis = cikis,
                    SonBakiye = sonBakiye
                });

                baslangicBakiye = sonBakiye;
            }
        }

        return rapor;
    }

    #endregion

        #region Yevmiye Excel Export

        /// <summary>
        /// Yevmiye kayıtlarını Excel formatında export eder
        /// </summary>
        public async Task<byte[]> ExportYevmiyeToExcelAsync(DateTime baslangic, DateTime bitis)
        {
            var rapor = await GetYevmiyeRaporuAsync(baslangic, bitis);

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("Yevmiye Kayitlari");

            // Başlık bilgileri
            ws.Cell(1, 1).Value = "YEVMİYE KAYITLARI";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Range(1, 1, 1, 7).Merge();

            ws.Cell(2, 1).Value = $"Tarih Aralığı: {baslangic:dd.MM.yyyy} - {bitis:dd.MM.yyyy}";
            ws.Range(2, 1, 2, 7).Merge();

            // Tablo başlıkları
            var headers = new[] { "Sıra", "Tarih", "Fiş No", "Hesap Kodu", "Hesap Adı", "Açıklama", "Borç", "Alacak" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(4, i + 1).Value = headers[i];
                ws.Cell(4, i + 1).Style.Font.Bold = true;
                ws.Cell(4, i + 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;
                ws.Cell(4, i + 1).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            }

            // Veri satırları
            int row = 5;
            foreach (var satir in rapor.Satirlar)
            {
                ws.Cell(row, 1).Value = satir.SiraNo;
                ws.Cell(row, 2).Value = satir.Tarih.ToString("dd.MM.yyyy");
                ws.Cell(row, 3).Value = satir.FisNo;
                ws.Cell(row, 4).Value = satir.HesapKodu;
                ws.Cell(row, 5).Value = satir.HesapAdi;
                ws.Cell(row, 6).Value = satir.Aciklama;
                ws.Cell(row, 7).Value = satir.Borc;
                ws.Cell(row, 8).Value = satir.Alacak;

                // Borç ve Alacak formatı
                ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 8).Style.NumberFormat.Format = "#,##0.00";

                row++;
            }

            // Toplam satırı
            ws.Cell(row, 1).Value = "TOPLAM";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Range(row, 1, row, 6).Merge();
            ws.Cell(row, 7).Value = rapor.ToplamBorc;
            ws.Cell(row, 8).Value = rapor.ToplamAlacak;
            ws.Cell(row, 7).Style.Font.Bold = true;
            ws.Cell(row, 8).Style.Font.Bold = true;
            ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 8).Style.NumberFormat.Format = "#,##0.00";
            ws.Row(row).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;

            // Sütun genişlikleri
            ws.Column(1).Width = 8;
            ws.Column(2).Width = 12;
            ws.Column(3).Width = 18;
            ws.Column(4).Width = 12;
            ws.Column(5).Width = 30;
            ws.Column(6).Width = 40;
            ws.Column(7).Width = 15;
            ws.Column(8).Width = 15;

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        /// <summary>
        /// Yevmiye kayıtlarını yazdırma için JSON formatında döndürür
        /// </summary>
        public async Task<byte[]> GetYevmiyeYazdirDataAsync(DateTime baslangic, DateTime bitis)
        {
            var rapor = await GetYevmiyeRaporuAsync(baslangic, bitis);
            var json = System.Text.Json.JsonSerializer.Serialize(rapor);
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        #endregion

        #region Zirve Muhasebe Programı Export

        /// <summary>
        /// Yevmiye kayıtlarını Zirve Muhasebe Programı formatında Excel'e export eder
        /// Zirve formatı: Fiş No, Fiş Tarihi, Hesap Kodu, Hesap Adı, Borç, Alacak, Açıklama
        /// </summary>
        public async Task<byte[]> ExportZirveFormatAsync(DateTime baslangic, DateTime bitis)
        {
            var rapor = await GetYevmiyeRaporuAsync(baslangic, bitis);

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("Zirve Import");

            // Zirve formatı başlıkları (Zirve programının beklediği sütun sırası)
            var headers = new[] { "FIS_NO", "FIS_TARIHI", "HESAP_KODU", "HESAP_ADI", "BORC", "ALACAK", "ACIKLAMA", "EVRAK_NO", "VADE_TARIHI" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
                ws.Cell(1, i + 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.DarkBlue;
                ws.Cell(1, i + 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                ws.Cell(1, i + 1).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            }

            // Fiş bazlı gruplama (Zirve her fişi bir yevmiye maddesi olarak bekler)
            var fisGruplari = rapor.Satirlar
                .GroupBy(s => new { s.FisNo, s.Tarih })
                .OrderBy(g => g.Key.Tarih)
                .ThenBy(g => g.Key.FisNo);

            int row = 2;
            int yevmiyeNo = 1;
            foreach (var fisGrup in fisGruplari)
            {
                foreach (var satir in fisGrup.OrderBy(s => s.HesapKodu))
                {
                    // FIS_NO - Zirve için yevmiye madde numarası
                    ws.Cell(row, 1).Value = yevmiyeNo;

                    // FIS_TARIHI - dd.MM.yyyy formatında
                    ws.Cell(row, 2).Value = satir.Tarih.ToString("dd.MM.yyyy");

                    // HESAP_KODU - Nokta yerine boşluk olabilir bazı Zirve versiyonlarında
                    ws.Cell(row, 3).Value = satir.HesapKodu;

                    // HESAP_ADI
                    ws.Cell(row, 4).Value = satir.HesapAdi;

                    // BORC - Sayısal format
                    ws.Cell(row, 5).Value = satir.Borc;
                    ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";

                    // ALACAK - Sayısal format
                    ws.Cell(row, 6).Value = satir.Alacak;
                    ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";

                    // ACIKLAMA
                    ws.Cell(row, 7).Value = !string.IsNullOrEmpty(satir.Aciklama) ? satir.Aciklama : satir.FisNo;

                    // EVRAK_NO - Orijinal fiş numarası
                    ws.Cell(row, 8).Value = satir.FisNo;

                    // VADE_TARIHI - Varsayılan olarak fiş tarihi
                    ws.Cell(row, 9).Value = satir.Tarih.ToString("dd.MM.yyyy");

                    row++;
                }
                yevmiyeNo++;
            }

            // Sütun genişlikleri (Zirve uyumlu)
            ws.Column(1).Width = 10;   // FIS_NO
            ws.Column(2).Width = 12;   // FIS_TARIHI
            ws.Column(3).Width = 15;   // HESAP_KODU
            ws.Column(4).Width = 35;   // HESAP_ADI
            ws.Column(5).Width = 15;   // BORC
            ws.Column(6).Width = 15;   // ALACAK
            ws.Column(7).Width = 50;   // ACIKLAMA
            ws.Column(8).Width = 18;   // EVRAK_NO
            ws.Column(9).Width = 12;   // VADE_TARIHI

            // Kontrol bilgisi sayfası
            var wsKontrol = workbook.Worksheets.Add("Kontrol Bilgisi");
            wsKontrol.Cell(1, 1).Value = "ZIRVE MUHASEBE IMPORT KONTROL";
            wsKontrol.Cell(1, 1).Style.Font.Bold = true;
            wsKontrol.Cell(1, 1).Style.Font.FontSize = 14;

            wsKontrol.Cell(3, 1).Value = "Tarih Aralığı:";
            wsKontrol.Cell(3, 2).Value = $"{baslangic:dd.MM.yyyy} - {bitis:dd.MM.yyyy}";

            wsKontrol.Cell(4, 1).Value = "Toplam Kayıt Sayısı:";
            wsKontrol.Cell(4, 2).Value = rapor.Satirlar.Count;

            wsKontrol.Cell(5, 1).Value = "Toplam Yevmiye Maddesi:";
            wsKontrol.Cell(5, 2).Value = yevmiyeNo - 1;

            wsKontrol.Cell(6, 1).Value = "Toplam Borç:";
            wsKontrol.Cell(6, 2).Value = rapor.ToplamBorc;
            wsKontrol.Cell(6, 2).Style.NumberFormat.Format = "#,##0.00 TL";

            wsKontrol.Cell(7, 1).Value = "Toplam Alacak:";
            wsKontrol.Cell(7, 2).Value = rapor.ToplamAlacak;
            wsKontrol.Cell(7, 2).Style.NumberFormat.Format = "#,##0.00 TL";

            wsKontrol.Cell(8, 1).Value = "Borç-Alacak Farkı:";
            wsKontrol.Cell(8, 2).Value = rapor.ToplamBorc - rapor.ToplamAlacak;
            wsKontrol.Cell(8, 2).Style.NumberFormat.Format = "#,##0.00 TL";
            wsKontrol.Cell(8, 2).Style.Font.Bold = true;
            wsKontrol.Cell(8, 2).Style.Font.FontColor = Math.Abs(rapor.ToplamBorc - rapor.ToplamAlacak) < 0.01m 
                ? ClosedXML.Excel.XLColor.Green 
                : ClosedXML.Excel.XLColor.Red;

            wsKontrol.Cell(10, 1).Value = "⚠️ UYARI:";
            wsKontrol.Cell(10, 1).Style.Font.Bold = true;
            wsKontrol.Cell(10, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.OrangeRed;

            wsKontrol.Cell(11, 1).Value = "1. Zirve'ye aktarmadan önce hesap kodlarının eşleştiğinden emin olun.";
            wsKontrol.Cell(12, 1).Value = "2. Borç-Alacak farkı 0 (sıfır) olmalıdır.";
            wsKontrol.Cell(13, 1).Value = "3. Tarih formatı: GG.AA.YYYY";
            wsKontrol.Cell(14, 1).Value = "4. 'Zirve Import' sayfasını CSV olarak da kaydedebilirsiniz.";

            wsKontrol.Column(1).Width = 25;
            wsKontrol.Column(2).Width = 30;

            // Oluşturma tarihi
            wsKontrol.Cell(16, 1).Value = "Oluşturma Tarihi:";
            wsKontrol.Cell(16, 2).Value = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        /// <summary>
        /// Muhasebe fişlerini detaylı kontrol listesi formatında export eder
        /// Zirve'ye aktarmadan önce kontrol amaçlı kullanılır
        /// </summary>
        public async Task<byte[]> ExportMuhasebeKontrolListesiAsync(DateTime baslangic, DateTime bitis)
        {
            var baslangicUtc = DateTime.SpecifyKind(baslangic.Date, DateTimeKind.Utc);
            var bitisUtc = DateTime.SpecifyKind(bitis.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

            var fisler = await _context.MuhasebeFisleri
                .Include(f => f.Kalemler)
                    .ThenInclude(k => k.Hesap)
                .Where(f => f.FisTarihi >= baslangicUtc && f.FisTarihi <= bitisUtc)
                .OrderBy(f => f.FisTarihi)
                .ThenBy(f => f.FisNo)
                .ToListAsync();

            using var workbook = new ClosedXML.Excel.XLWorkbook();

            // Özet Sayfa
            var wsOzet = workbook.Worksheets.Add("Özet");
            wsOzet.Cell(1, 1).Value = "MUHASEBE FİŞLERİ KONTROL LİSTESİ";
            wsOzet.Cell(1, 1).Style.Font.Bold = true;
            wsOzet.Cell(1, 1).Style.Font.FontSize = 16;
            wsOzet.Range(1, 1, 1, 4).Merge();

            wsOzet.Cell(3, 1).Value = "Tarih Aralığı:";
            wsOzet.Cell(3, 2).Value = $"{baslangic:dd.MM.yyyy} - {bitis:dd.MM.yyyy}";

            // Fiş tiplerine göre özet
            var fisTipOzet = fisler.GroupBy(f => f.FisTipi)
                .Select(g => new { Tip = g.Key, Adet = g.Count(), Borc = g.Sum(f => f.ToplamBorc), Alacak = g.Sum(f => f.ToplamAlacak) })
                .ToList();

            wsOzet.Cell(5, 1).Value = "Fiş Tipi";
            wsOzet.Cell(5, 2).Value = "Adet";
            wsOzet.Cell(5, 3).Value = "Borç";
            wsOzet.Cell(5, 4).Value = "Alacak";
            wsOzet.Range(5, 1, 5, 4).Style.Font.Bold = true;
            wsOzet.Range(5, 1, 5, 4).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;

            int ozetRow = 6;
            foreach (var ozet in fisTipOzet)
            {
                wsOzet.Cell(ozetRow, 1).Value = ozet.Tip.ToString();
                wsOzet.Cell(ozetRow, 2).Value = ozet.Adet;
                wsOzet.Cell(ozetRow, 3).Value = ozet.Borc;
                wsOzet.Cell(ozetRow, 4).Value = ozet.Alacak;
                wsOzet.Cell(ozetRow, 3).Style.NumberFormat.Format = "#,##0.00";
                wsOzet.Cell(ozetRow, 4).Style.NumberFormat.Format = "#,##0.00";
                ozetRow++;
            }

            wsOzet.Cell(ozetRow, 1).Value = "TOPLAM";
            wsOzet.Cell(ozetRow, 1).Style.Font.Bold = true;
            wsOzet.Cell(ozetRow, 2).Value = fisler.Count;
            wsOzet.Cell(ozetRow, 3).Value = fisler.Sum(f => f.ToplamBorc);
            wsOzet.Cell(ozetRow, 4).Value = fisler.Sum(f => f.ToplamAlacak);
            wsOzet.Range(ozetRow, 1, ozetRow, 4).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
            wsOzet.Range(ozetRow, 1, ozetRow, 4).Style.Font.Bold = true;

            // Durum bazlı özet
            ozetRow += 2;
            wsOzet.Cell(ozetRow, 1).Value = "Durum Dağılımı:";
            wsOzet.Cell(ozetRow, 1).Style.Font.Bold = true;
            ozetRow++;

            var durumOzet = fisler.GroupBy(f => f.Durum).ToList();
            foreach (var d in durumOzet)
            {
                wsOzet.Cell(ozetRow, 1).Value = d.Key.ToString();
                wsOzet.Cell(ozetRow, 2).Value = d.Count();
                ozetRow++;
            }

            wsOzet.Columns().AdjustToContents();

            // Detay Sayfa - Fiş bazlı
            var wsDetay = workbook.Worksheets.Add("Detay");
            var detayHeaders = new[] { "Fiş No", "Tarih", "Tip", "Durum", "Kaynak", "Hesap Kodu", "Hesap Adı", "Borç", "Alacak", "Açıklama" };
            for (int i = 0; i < detayHeaders.Length; i++)
            {
                wsDetay.Cell(1, i + 1).Value = detayHeaders[i];
                wsDetay.Cell(1, i + 1).Style.Font.Bold = true;
                wsDetay.Cell(1, i + 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.DarkBlue;
                wsDetay.Cell(1, i + 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            }

            int detayRow = 2;
            foreach (var fis in fisler)
            {
                foreach (var kalem in fis.Kalemler.OrderBy(k => k.SiraNo))
                {
                    wsDetay.Cell(detayRow, 1).Value = fis.FisNo;
                    wsDetay.Cell(detayRow, 2).Value = fis.FisTarihi.ToString("dd.MM.yyyy");
                    wsDetay.Cell(detayRow, 3).Value = fis.FisTipi.ToString();
                    wsDetay.Cell(detayRow, 4).Value = fis.Durum.ToString();
                    wsDetay.Cell(detayRow, 5).Value = fis.Kaynak.ToString();
                    wsDetay.Cell(detayRow, 6).Value = kalem.Hesap?.HesapKodu ?? "";
                    wsDetay.Cell(detayRow, 7).Value = kalem.Hesap?.HesapAdi ?? "";
                    wsDetay.Cell(detayRow, 8).Value = kalem.Borc;
                    wsDetay.Cell(detayRow, 9).Value = kalem.Alacak;
                    wsDetay.Cell(detayRow, 10).Value = kalem.Aciklama ?? fis.Aciklama ?? "";

                    wsDetay.Cell(detayRow, 8).Style.NumberFormat.Format = "#,##0.00";
                    wsDetay.Cell(detayRow, 9).Style.NumberFormat.Format = "#,##0.00";

                    // Onaylanmamış fişleri sarı ile işaretle
                    if (fis.Durum != FisDurum.Onaylandi)
                    {
                        wsDetay.Range(detayRow, 1, detayRow, 10).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightYellow;
                    }

                    detayRow++;
                }
            }

            wsDetay.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        #endregion

    #region Toplu Muhasebeleştirme

    public async Task<MuhasbelestirmeDurum> GetMuhasbelestirmeDurumuAsync()
    {
        var faturalar = await _context.Faturalar
            .Where(f => !f.IsDeleted)
            .ToListAsync();

        var masraflar = await _context.AracMasraflari
            .Where(m => !m.IsDeleted)
            .ToListAsync();

        var bekleyenFaturalar = faturalar.Where(f => !f.MuhasebeFisiOlusturuldu).ToList();
        var bekleyenMasraflar = masraflar.Where(m => m.MuhasebeFisId == null).ToList();

        return new MuhasbelestirmeDurum
        {
            ToplamFatura = faturalar.Count,
            MuhasbelestirilmisFatura = faturalar.Count(f => f.MuhasebeFisiOlusturuldu),
            BekleyenFatura = bekleyenFaturalar.Count,
            BekleyenFaturaTutar = bekleyenFaturalar.Sum(f => f.GenelToplam),
            ToplamMasraf = masraflar.Count,
            MuhasbelestirilmisMasraf = masraflar.Count(m => m.MuhasebeFisId != null),
            BekleyenMasraf = bekleyenMasraflar.Count,
            BekleyenMasrafTutar = bekleyenMasraflar.Sum(m => m.Tutar)
        };
    }

    public async Task<List<MuhasebeFaturaOzet>> GetMuhasbelestirilmemisFaturalarAsync(
        DateTime? baslangic = null, DateTime? bitis = null, FaturaYonu? faturaYonu = null)
    {
        var query = _context.Faturalar
            .Include(f => f.Cari)
            .Where(f => !f.IsDeleted && !f.MuhasebeFisiOlusturuldu)
            .AsQueryable();

        if (baslangic.HasValue)
            query = query.Where(f => f.FaturaTarihi >= baslangic.Value);
        if (bitis.HasValue)
            query = query.Where(f => f.FaturaTarihi <= bitis.Value);
        if (faturaYonu.HasValue)
            query = query.Where(f => f.FaturaYonu == faturaYonu.Value);

        return await query
            .OrderBy(f => f.FaturaTarihi)
            .Select(f => new MuhasebeFaturaOzet
            {
                FaturaId = f.Id,
                FaturaNo = f.FaturaNo,
                FaturaTarihi = f.FaturaTarihi,
                CariUnvan = f.Cari != null ? f.Cari.Unvan : "",
                FaturaYonu = f.FaturaYonu == FaturaYonu.Giden ? "Giden" : "Gelen",
                FaturaTipi = f.FaturaTipi.ToString(),
                AraToplam = f.AraToplam,
                KdvTutar = f.KdvTutar,
                GenelToplam = f.GenelToplam,
                TevkifatliMi = f.TevkifatliMi,
                TevkifatTutar = f.TevkifatTutar
            })
            .ToListAsync();
    }

    public async Task<List<MuhasebeMasrafOzet>> GetMuhasbelestirilmemisMasraflarAsync(
        DateTime? baslangic = null, DateTime? bitis = null)
    {
        var query = _context.AracMasraflari
            .Include(m => m.Arac)
            .Include(m => m.MasrafKalemi)
            .Include(m => m.Cari)
            .Include(m => m.Sofor)
            .Where(m => !m.IsDeleted && m.MuhasebeFisId == null)
            .AsQueryable();

        if (baslangic.HasValue)
            query = query.Where(m => m.MasrafTarihi >= baslangic.Value);
        if (bitis.HasValue)
            query = query.Where(m => m.MasrafTarihi <= bitis.Value);

        return await query
            .OrderBy(m => m.MasrafTarihi)
            .Select(m => new MuhasebeMasrafOzet
            {
                MasrafId = m.Id,
                MasrafTarihi = m.MasrafTarihi,
                AracPlaka = m.Arac != null ? m.Arac.AktifPlaka : "",
                MasrafKalemi = m.MasrafKalemi != null ? m.MasrafKalemi.MasrafAdi : "",
                MasrafKategori = m.MasrafKalemi != null ? m.MasrafKalemi.Kategori.ToString() : "",
                Tutar = m.Tutar,
                BelgeNo = m.BelgeNo,
                CariUnvan = m.Cari != null ? m.Cari.Unvan : null,
                SoforAd = m.Sofor != null ? (m.Sofor.Ad + " " + m.Sofor.Soyad) : null,
                Aciklama = m.Aciklama
            })
            .ToListAsync();
    }

    public async Task<MuhasbelestirmeSonuc> TopluFaturaMuhasbelestirAsync(List<int> faturaIdleri)
    {
        var sonuc = new MuhasbelestirmeSonuc();

        foreach (var faturaId in faturaIdleri)
        {
            try
            {
                var fatura = await _context.Faturalar
                    .Include(f => f.Cari)
                    .Include(f => f.FaturaKalemleri)
                    .FirstOrDefaultAsync(f => f.Id == faturaId && !f.IsDeleted);

                if (fatura == null)
                {
                    sonuc.HataliSayisi++;
                    sonuc.Hatalar.Add($"Fatura #{faturaId} bulunamadı.");
                    continue;
                }

                if (fatura.MuhasebeFisiOlusturuldu)
                {
                    sonuc.Hatalar.Add($"Fatura {fatura.FaturaNo} zaten muhasebeleştirilmiş.");
                    continue;
                }

                var fis = await CreateFaturaFisiAsync(fatura);
                sonuc.BasariliSayisi++;
                sonuc.OlusturulanFisIdleri.Add(fis.Id);
            }
            catch (Exception ex)
            {
                sonuc.HataliSayisi++;
                sonuc.Hatalar.Add($"Fatura #{faturaId}: {ex.Message}");
            }
        }

        return sonuc;
    }

    public async Task<MuhasbelestirmeSonuc> TopluMasrafMuhasbelestirAsync(List<int> masrafIdleri)
    {
        var sonuc = new MuhasbelestirmeSonuc();

        foreach (var masrafId in masrafIdleri)
        {
            try
            {
                var masraf = await _context.AracMasraflari
                    .Include(m => m.Arac)
                    .Include(m => m.MasrafKalemi)
                    .Include(m => m.Sofor)
                    .Include(m => m.Cari)
                    .FirstOrDefaultAsync(m => m.Id == masrafId && !m.IsDeleted);

                if (masraf == null)
                {
                    sonuc.HataliSayisi++;
                    sonuc.Hatalar.Add($"Masraf #{masrafId} bulunamadı.");
                    continue;
                }

                if (masraf.MuhasebeFisId != null)
                {
                    sonuc.Hatalar.Add($"Masraf #{masrafId} zaten muhasebeleştirilmiş.");
                    continue;
                }

                // Masraf muhasebe fişi oluştur
                var fis = await CreateMasrafMuhasebeFisiAsync(masraf);
                sonuc.BasariliSayisi++;
                sonuc.OlusturulanFisIdleri.Add(fis.Id);
            }
            catch (Exception ex)
            {
                sonuc.HataliSayisi++;
                sonuc.Hatalar.Add($"Masraf #{masrafId}: {ex.Message}");
            }
        }

        return sonuc;
    }

    private async Task<MuhasebeFis> CreateMasrafMuhasebeFisiAsync(AracMasraf masraf)
    {
        var ayar = await _context.MuhasebeAyarlari.FirstOrDefaultAsync();

        // Masraf kategorisine göre gider hesabı
        var giderHesapKodu = masraf.MasrafKalemi?.Kategori switch
        {
            MasrafKategori.Yakit => "770.06",
            MasrafKategori.Bakim or MasrafKategori.Tamir or MasrafKategori.Lastik or MasrafKategori.YedekParca => "770.07",
            MasrafKategori.Sigorta => "770.08",
            MasrafKategori.Personel => "770.09",
            _ => "770"
        };

        var giderHesap = await GetHesapByKodAsync(giderHesapKodu) ?? await GetHesapByKodAsync("770");
        if (giderHesap == null)
            throw new InvalidOperationException("Masraf gider hesabı bulunamadı.");

        // Karşı hesap belirleme
        MuhasebeHesap karsiHesap;
        string karsiAciklama;

        if (masraf.CariId.HasValue)
        {
            karsiHesap = await GetOrCreateCariHesapAsync("320", masraf.CariId.Value);
            karsiAciklama = $"Cari: {masraf.Cari?.Unvan}";
        }
        else if (masraf.SoforId.HasValue)
        {
            // Personel hesabı (335)
            var personelHesap = await GetHesapByKodAsync("335");
            karsiHesap = personelHesap ?? await GetHesapByKodAsync("100") ?? throw new InvalidOperationException("Karşı hesap bulunamadı.");
            karsiAciklama = $"Personel: {masraf.Sofor?.TamAd}";
        }
        else
        {
            karsiHesap = await GetHesapByKodAsync("100") ?? throw new InvalidOperationException("Kasa hesabı bulunamadı.");
            karsiAciklama = "Kasa karşılığı";
        }

        var fisNo = await GenerateNextFisNoAsync(FisTipi.Mahsup);
        var aciklama = $"Araç Masrafı: {masraf.Arac?.AktifPlaka} - {masraf.MasrafKalemi?.MasrafAdi}";
        if (!string.IsNullOrWhiteSpace(masraf.BelgeNo))
            aciklama += $" / Belge: {masraf.BelgeNo}";

        var fis = new MuhasebeFis
        {
            FisNo = fisNo,
            FisTarihi = DateTime.SpecifyKind(masraf.MasrafTarihi, DateTimeKind.Utc),
            FisTipi = FisTipi.Mahsup,
            Aciklama = aciklama,
            Kaynak = FisKaynak.Otomatik,
            KaynakId = masraf.Id,
            KaynakTip = "AracMasraf",
            Durum = FisDurum.Onaylandi,
            CreatedAt = DateTime.UtcNow,
            Kalemler = new List<MuhasebeFisKalem>
            {
                new()
                {
                    HesapId = giderHesap.Id,
                    Borc = masraf.Tutar,
                    Alacak = 0,
                    SiraNo = 1,
                    Aciklama = aciklama,
                    CariId = masraf.CariId
                },
                new()
                {
                    HesapId = karsiHesap.Id,
                    Borc = 0,
                    Alacak = masraf.Tutar,
                    SiraNo = 2,
                    Aciklama = karsiAciklama,
                    CariId = masraf.CariId
                }
            }
        };

        fis.ToplamBorc = fis.Kalemler.Sum(k => k.Borc);
        fis.ToplamAlacak = fis.Kalemler.Sum(k => k.Alacak);

        _context.MuhasebeFisleri.Add(fis);
        await _context.SaveChangesAsync();

        // Masrafa fiş ID kaydet
        masraf.MuhasebeFisId = fis.Id;
        await _context.SaveChangesAsync();

        return fis;
    }

    #endregion
    }
