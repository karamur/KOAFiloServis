using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace CRMFiloServis.Web.Services;

public class FaturaService : IFaturaService
{
    private readonly ApplicationDbContext _context;
    private readonly IMuhasebeService _muhasebeService;

    public FaturaService(ApplicationDbContext context, IMuhasebeService muhasebeService)
    {
        _context = context;
        _muhasebeService = muhasebeService;
    }

    public async Task<List<Fatura>> GetAllAsync()
    {
        return await _context.Faturalar
            .Include(f => f.Cari)
            .OrderByDescending(f => f.FaturaTarihi)
            .ToListAsync();
    }

    public async Task<List<Fatura>> GetByCariIdAsync(int cariId)
    {
        return await _context.Faturalar
            .Include(f => f.Cari)
            .Where(f => f.CariId == cariId)
            .OrderByDescending(f => f.FaturaTarihi)
            .ToListAsync();
    }

    public async Task<List<Fatura>> GetByTipAsync(FaturaTipi tip)
    {
        return await _context.Faturalar
            .Include(f => f.Cari)
            .Where(f => f.FaturaTipi == tip)
            .OrderByDescending(f => f.FaturaTarihi)
            .ToListAsync();
    }

    public async Task<List<Fatura>> GetByDurumAsync(FaturaDurum durum)
    {
        return await _context.Faturalar
            .Include(f => f.Cari)
            .Where(f => f.Durum == durum)
            .OrderByDescending(f => f.FaturaTarihi)
            .ToListAsync();
    }

    public async Task<List<Fatura>> GetOdenmemisFaturalarAsync()
    {
        return await _context.Faturalar
            .Include(f => f.Cari)
            .Where(f => f.Durum == FaturaDurum.Beklemede || f.Durum == FaturaDurum.KismiOdendi)
            .OrderBy(f => f.VadeTarihi)
            .ToListAsync();
    }

    public async Task<List<Fatura>> GetOdenmisFaturalarAsync()
    {
        return await _context.Faturalar
            .Include(f => f.Cari)
            .Where(f => f.Durum == FaturaDurum.Odendi)
            .OrderByDescending(f => f.FaturaTarihi)
            .ToListAsync();
    }

    public async Task<List<Fatura>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Faturalar
            .Include(f => f.Cari)
            .Where(f => f.FaturaTarihi >= startDate && f.FaturaTarihi <= endDate)
            .OrderByDescending(f => f.FaturaTarihi)
            .ToListAsync();
    }

    public async Task<Fatura?> GetByIdAsync(int id)
    {
        return await _context.Faturalar
            .Include(f => f.Cari)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<Fatura?> GetByIdWithKalemlerAsync(int id)
    {
        return await _context.Faturalar
            .Include(f => f.Cari)
            .Include(f => f.FaturaKalemleri)
            .Include(f => f.OdemeEslestirmeleri)
                .ThenInclude(o => o.BankaKasaHareket)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<Fatura> CreateAsync(Fatura fatura)
    {
        // Tutarları hesapla
        CalculateTotals(fatura);

        _context.Faturalar.Add(fatura);
        await _context.SaveChangesAsync();

        // Otomatik muhasebe fişi oluştur (ayarlara göre)
        await TryCreateMuhasebeFisiAsync(fatura);

        return fatura;
    }

    public async Task<Fatura> UpdateAsync(Fatura fatura)
    {
        var existing = await _context.Faturalar.FindAsync(fatura.Id);
        if (existing == null) throw new Exception("Fatura bulunamadi");

        // Mevcut entity'yi guncelle
        existing.FaturaNo = fatura.FaturaNo;
        existing.FaturaTarihi = fatura.FaturaTarihi;
        existing.VadeTarihi = fatura.VadeTarihi;
        existing.FaturaTipi = fatura.FaturaTipi;
        existing.EFaturaTipi = fatura.EFaturaTipi;
        existing.FaturaYonu = fatura.FaturaYonu;
        existing.CariId = fatura.CariId;
        existing.FirmaId = fatura.FirmaId;
        existing.AraToplam = fatura.AraToplam;
        existing.IskontoTutar = fatura.IskontoTutar;
        existing.KdvOrani = fatura.KdvOrani;
        existing.KdvTutar = fatura.KdvTutar;
        existing.GenelToplam = fatura.GenelToplam;
        existing.Durum = fatura.Durum;
        existing.Aciklama = fatura.Aciklama;
        existing.Notlar = fatura.Notlar;
        existing.EttnNo = fatura.EttnNo;
        existing.GibKodu = fatura.GibKodu;
        existing.UpdatedAt = DateTime.UtcNow;

        // Tutarlari yeniden hesapla
        CalculateTotals(existing);

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(int id)
    {
        var fatura = await _context.Faturalar.FindAsync(id);
        if (fatura != null)
        {
            fatura.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateNextFaturaNoAsync(FaturaTipi tip)
    {
        var prefix = tip switch
        {
            FaturaTipi.SatisFaturasi => "SF",
            FaturaTipi.AlisFaturasi => "AF",
            FaturaTipi.SatisIadeFaturasi => "SIF",
            FaturaTipi.AlisIadeFaturasi => "AIF",
            _ => "FTR"
        };

        var year = DateTime.Now.Year;
        var lastFatura = await _context.Faturalar
            .IgnoreQueryFilters()
            .Where(f => f.FaturaNo.StartsWith($"{prefix}-{year}"))
            .OrderByDescending(f => f.FaturaNo)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (lastFatura != null)
        {
            var parts = lastFatura.FaturaNo.Split('-');
            if (parts.Length >= 3 && int.TryParse(parts[2], out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}-{year}-{nextNumber:D6}";
    }

    public async Task UpdateOdenenTutarAsync(int faturaId)
    {
        var fatura = await _context.Faturalar
            .Include(f => f.OdemeEslestirmeleri)
            .FirstOrDefaultAsync(f => f.Id == faturaId);

        if (fatura != null)
        {
            fatura.OdenenTutar = fatura.OdemeEslestirmeleri.Sum(o => o.EslestirilenTutar);

            // Durumu güncelle
            if (fatura.OdenenTutar >= fatura.GenelToplam)
            {
                fatura.Durum = FaturaDurum.Odendi;
            }
            else if (fatura.OdenenTutar > 0)
            {
                fatura.Durum = FaturaDurum.KismiOdendi;
            }
            else
            {
                fatura.Durum = FaturaDurum.Beklemede;
            }

            await _context.SaveChangesAsync();
        }
    }

    public async Task<DashboardFaturaStats> GetDashboardStatsAsync()
    {
        var stats = new DashboardFaturaStats();
        var today = DateTime.Today;
        var buAyBaslangic = new DateTime(today.Year, today.Month, 1);

        // Single optimized query for invoices needed for dashboard
        var relevantFaturalar = await _context.Faturalar
            .Include(f => f.Cari)
            .Where(f => f.Durum != FaturaDurum.IptalEdildi)
            .Select(f => new
            {
                f.Id,
                f.FaturaNo,
                f.FaturaTarihi,
                f.FaturaTipi,
                f.VadeTarihi,
                f.GenelToplam,
                f.OdenenTutar,
                f.Durum,
                KalanTutar = f.GenelToplam - f.OdenenTutar,
                CariUnvan = f.Cari.Unvan
            })
            .ToListAsync();

        // Count pending invoices
        stats.BekleyenFaturaSayisi = relevantFaturalar
            .Count(f => f.KalanTutar > 0);

        // Calculate this month's income/expense
        var buAyFaturalar = relevantFaturalar
            .Where(f => f.FaturaTarihi >= buAyBaslangic);
        
        stats.BuAyGelir = buAyFaturalar
            .Where(f => f.FaturaTipi == FaturaTipi.SatisFaturasi)
            .Sum(f => f.GenelToplam);
        
        stats.BuAyGider = buAyFaturalar
            .Where(f => f.FaturaTipi == FaturaTipi.AlisFaturasi)
            .Sum(f => f.GenelToplam);

        // Overdue invoices - need full entity for display
        var vadeGecmisIds = relevantFaturalar
            .Where(f => f.KalanTutar > 0 && f.VadeTarihi.HasValue && f.VadeTarihi.Value < today)
            .OrderBy(f => f.VadeTarihi)
            .Take(10)
            .Select(f => f.Id)
            .ToList();

        if (vadeGecmisIds.Count > 0)
        {
            stats.VadeGecmisFaturalar = await _context.Faturalar
                .Include(f => f.Cari)
                .Where(f => vadeGecmisIds.Contains(f.Id))
                .OrderBy(f => f.VadeTarihi)
                .ToListAsync();
        }

        // Upcoming due invoices
        var vadeYaklasanIds = relevantFaturalar
            .Where(f => f.KalanTutar > 0 && f.VadeTarihi.HasValue && 
                   f.VadeTarihi.Value >= today && f.VadeTarihi.Value <= today.AddDays(7))
            .OrderBy(f => f.VadeTarihi)
            .Take(10)
            .Select(f => f.Id)
            .ToList();

        if (vadeYaklasanIds.Count > 0)
        {
            stats.VadeYaklasanFaturalar = await _context.Faturalar
                .Include(f => f.Cari)
                .Where(f => vadeYaklasanIds.Contains(f.Id))
                .OrderBy(f => f.VadeTarihi)
                .ToListAsync();
        }

        return stats;
    }

    #region E-Fatura / E-Arsiv Metodlari

    public async Task<List<Fatura>> GetByYonAsync(FaturaYonu yon, int? firmaId = null)
    {
        var query = _context.Faturalar
            .Include(f => f.Cari)
            .Where(f => f.FaturaYonu == yon);

        if (firmaId.HasValue)
            query = query.Where(f => f.FirmaId == firmaId.Value);

        return await query.OrderByDescending(f => f.FaturaTarihi).ToListAsync();
    }

    public async Task<List<Fatura>> GetByYonAndDateRangeAsync(FaturaYonu yon, DateTime? baslangic, DateTime? bitis, int? firmaId = null)
    {
        var query = _context.Faturalar
            .Include(f => f.Cari)
            .Where(f => f.FaturaYonu == yon);

        if (firmaId.HasValue)
            query = query.Where(f => f.FirmaId == firmaId.Value);

        if (baslangic.HasValue)
            query = query.Where(f => f.FaturaTarihi >= baslangic.Value);
        
        if (bitis.HasValue)
            query = query.Where(f => f.FaturaTarihi <= bitis.Value);

        return await query.OrderByDescending(f => f.FaturaTarihi).ToListAsync();
    }

    public async Task<List<Fatura>> GetByEFaturaTipiAsync(EFaturaTipi tip)
    {
        return await _context.Faturalar
            .Include(f => f.Cari)
            .Where(f => f.EFaturaTipi == tip)
            .OrderByDescending(f => f.FaturaTarihi)
            .ToListAsync();
    }

    /// <summary>
    /// Excel Import - Ornek dosya formatina gore:
    /// A: Unvani/Adi Soyadi, B: Vkn/Tckn, C: Fatura Tipi, D: Fatura Tarihi, E: Fatura No
    /// F: Iskonto, G: Kdv Matrahi %0, H: Kdv Matrahi %1, I: Kdv Matrahi %10, J: Kdv Matrahi %20
    /// K: Kdv%1, L: Kdv%10, M: Kdv%20, N: Odenecek Tutar Turk Lirasi
    /// </summary>
    public async Task<EFaturaImportResult> ImportFromExcelAsync(byte[] fileContent, FaturaYonu yon, int? firmaId = null, EFaturaTipi? eFaturaTipi = null)
    {
        var result = new EFaturaImportResult();
        
        // Varsayilan E-Fatura tipi
        var defaultEFaturaTipi = eFaturaTipi ?? EFaturaTipi.EArsiv;
        
        try
        {
            using var stream = new MemoryStream(fileContent);
            using var package = new OfficeOpenXml.ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            
            if (worksheet == null)
            {
                result.Errors.Add("Excel dosyasinda sayfa bulunamadi.");
                return result;
            }

            var rowCount = worksheet.Dimension?.Rows ?? 0;
            
            if (rowCount < 2)
            {
                result.Errors.Add("Excel dosyasinda veri bulunamadi. (Sadece baslik satiri var)");
                return result;
            }

            int nextCariNum = await GetNextCariNumAsync();

            // Bu importta olusturulan carileri takip et
            var importCarileri = new Dictionary<string, Cari>(StringComparer.OrdinalIgnoreCase);

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var cariUnvan = worksheet.Cells[row, 1].Text?.Trim();
                    var cariVkn = worksheet.Cells[row, 2].Text?.Trim();
                    var tarihStr = worksheet.Cells[row, 4].Text?.Trim();
                    var faturaNo = NormalizeFaturaNo(worksheet.Cells[row, 5].Text);
                    
                    // Bos satiri atla
                    if (string.IsNullOrEmpty(faturaNo) && string.IsNullOrEmpty(cariUnvan)) 
                        continue;

                    // Zorunlu alan kontrolu
                    if (string.IsNullOrEmpty(faturaNo))
                    {
                        result.Errors.Add($"Satir {row}: Fatura No bos.");
                        result.ErrorCount++;
                        continue;
                    }
                    
                    if (string.IsNullOrEmpty(cariUnvan))
                    {
                        result.Errors.Add($"Satir {row}: Cari Unvan bos.");
                        result.ErrorCount++;
                        continue;
                    }

                    var existingFatura = await FindExistingFaturaAsync(faturaNo);
                    if (existingFatura != null)
                    {
                        result.SkippedCount++;
                        result.Errors.Add($"Satir {row}: '{faturaNo}' zaten mevcut.");
                        continue;
                    }

                    var iskonto = ParseDecimal(worksheet.Cells[row, 6].Text);
                    var kdvMatrah0 = ParseDecimal(worksheet.Cells[row, 7].Text);
                    var kdvMatrah1 = ParseDecimal(worksheet.Cells[row, 8].Text);
                    var kdvMatrah10 = ParseDecimal(worksheet.Cells[row, 9].Text);
                    var kdvMatrah20 = ParseDecimal(worksheet.Cells[row, 10].Text);
                    var kdv1 = ParseDecimal(worksheet.Cells[row, 11].Text);
                    var kdv10 = ParseDecimal(worksheet.Cells[row, 12].Text);
                    var kdv20 = ParseDecimal(worksheet.Cells[row, 13].Text);
                    var odenecekTutar = ParseDecimal(worksheet.Cells[row, 14].Text);

                    // Tarihi parse et
                    DateTime faturaTarihi = DateTime.UtcNow;
                    if (!string.IsNullOrEmpty(tarihStr))
                    {
                        // Turkce tarih formatlari
                        var formats = new[] { 
                            "dd.MM.yyyy", "d.MM.yyyy", "dd.M.yyyy", "d.M.yyyy",
                            "dd/MM/yyyy", "d/MM/yyyy", "dd/M/yyyy", "d/M/yyyy",
                            "yyyy-MM-dd"
                        };
                        
                        if (DateTime.TryParseExact(tarihStr, formats, 
                            new System.Globalization.CultureInfo("tr-TR"), 
                            System.Globalization.DateTimeStyles.None, out var parsedTarih))
                        {
                            faturaTarihi = DateTime.SpecifyKind(parsedTarih.Date, DateTimeKind.Utc);
                        }
                        else if (DateTime.TryParse(tarihStr, new System.Globalization.CultureInfo("tr-TR"), out parsedTarih))
                        {
                            faturaTarihi = DateTime.SpecifyKind(parsedTarih.Date, DateTimeKind.Utc);
                        }
                        else
                        {
                            // Excel sayisal tarih formati
                            if (double.TryParse(tarihStr, out var excelDate) && excelDate > 1)
                            {
                                faturaTarihi = DateTime.SpecifyKind(DateTime.FromOADate(excelDate).Date, DateTimeKind.Utc);
                            }
                        }
                    }

                    // CARİ KONTROL
                    Cari? cari = null;
                    var cariKey = !string.IsNullOrWhiteSpace(cariVkn) ? cariVkn : cariUnvan.ToLowerInvariant();
                    
                    // Once bu importta olusturulmus carilere bak
                    if (!string.IsNullOrEmpty(cariKey) && importCarileri.TryGetValue(cariKey, out var mevcutCari))
                    {
                        cari = mevcutCari;
                    }
                    
                    // Veritabaninda VKN ile ara
                    if (cari == null && !string.IsNullOrWhiteSpace(cariVkn) && cariVkn.Length >= 10)
                    {
                        cari = await _context.Cariler.FirstOrDefaultAsync(c => c.VergiNo == cariVkn);
                    }
                    
                    // Veritabaninda Unvan ile ara
                    if (cari == null && !string.IsNullOrWhiteSpace(cariUnvan))
                    {
                        cari = await _context.Cariler.FirstOrDefaultAsync(c => 
                            c.Unvan.ToLower() == cariUnvan.ToLower());
                    }
                    
                    // Hala bulunamadiysa yeni olustur
                    if (cari == null)
                    {
                        // Benzersiz CariKodu - timestamp ile garantili
                        var uniqueCode = await GetUniqueCariCodeAsync(nextCariNum);
                        
                        cari = new Cari
                        {
                            CariKodu = uniqueCode,
                            Unvan = cariUnvan,
                            VergiNo = cariVkn ?? string.Empty,
                            CariTipi = yon == FaturaYonu.Giden ? CariTipi.Musteri : CariTipi.Tedarikci,
                            Aktif = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        
                        _context.Cariler.Add(cari);
                        await _context.SaveChangesAsync();
                        
                        if (!string.IsNullOrEmpty(cariKey))
                        {
                            importCarileri[cariKey] = cari;
                        }
                        nextCariNum++;
                    }

                    // Toplam matrah ve KDV hesapla
                    var toplamMatrah = kdvMatrah0 + kdvMatrah1 + kdvMatrah10 + kdvMatrah20;
                    var toplamKdv = kdv1 + kdv10 + kdv20;
                    var genelToplam = odenecekTutar > 0 ? odenecekTutar : (toplamMatrah + toplamKdv - iskonto);

                    // Tutar kontrolu
                    if (genelToplam <= 0 && toplamMatrah <= 0)
                    {
                        result.Errors.Add($"Satir {row}: Tutar bilgisi eksik.");
                        result.ErrorCount++;
                        continue;
                    }

                    var fatura = new Fatura
                    {
                        FaturaNo = faturaNo,
                        FaturaTarihi = faturaTarihi,
                        CariId = cari.Id,
                        FirmaId = firmaId,
                        FaturaYonu = yon,
                        FaturaTipi = yon == FaturaYonu.Giden ? FaturaTipi.SatisFaturasi : FaturaTipi.AlisFaturasi,
                        EFaturaTipi = defaultEFaturaTipi,
                        AraToplam = toplamMatrah > 0 ? toplamMatrah : genelToplam,
                        IskontoTutar = iskonto,
                        KdvTutar = toplamKdv,
                        GenelToplam = genelToplam,
                        ImportKaynak = "Excel",
                        Durum = FaturaDurum.Beklemede,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Faturalar.Add(fatura);
                    
                    // Her faturayı tek tek kaydet - hata durumunda diger faturalar etkilenmesin
                    await _context.SaveChangesAsync();
                    
                    result.ImportedItems.Add(fatura);
                    result.ImportedCount++;
                }
                catch (Exception ex)
                {
                    // Detayli hata mesaji
                    var errorMessage = ex.InnerException?.Message ?? ex.Message;
                    result.Errors.Add($"Satir {row}: {errorMessage}");
                    result.ErrorCount++;
                    
                    // Context'i temizle - hatali entity'leri kaldir
                    foreach (var entry in _context.ChangeTracker.Entries().ToList())
                    {
                        if (entry.State == EntityState.Added)
                        {
                            entry.State = EntityState.Detached;
                        }
                    }
                }
            }
            
            result.Success = result.ImportedCount > 0;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Excel okuma hatasi: {ex.InnerException?.Message ?? ex.Message}");
        }

        return result;
    }

    public async Task<EFaturaImportResult> ImportFromXmlAsync(List<XmlFileContent> xmlFiles, FaturaYonu yon, int? firmaId = null, EFaturaTipi? eFaturaTipi = null)
    {
        var result = new EFaturaImportResult();
        var defaultEFaturaTipi = eFaturaTipi ?? EFaturaTipi.EFatura;
        int nextCariNum = await GetNextCariNumAsync();
        var importCarileri = new Dictionary<string, Cari>(StringComparer.OrdinalIgnoreCase);

        // Ayarları al
        var ayar = await _context.MuhasebeAyarlari.FirstOrDefaultAsync();
        var otomatikCariOlustur = ayar?.XmlImportOtomatikCariOlustur ?? true;
        var otomatikHesapKoduOlustur = ayar?.XmlImportOtomatikHesapKoduOlustur ?? true;

        foreach (var file in xmlFiles)
        {
            try
            {
                using var ms = new MemoryStream(file.Content);
                var xdoc = System.Xml.Linq.XDocument.Load(ms);
                
                // Helper to find elements safely ignoring namespaces
                string GetValue(System.Xml.Linq.XElement? parent, string localName) => 
                    parent?.Descendants().FirstOrDefault(x => x.Name.LocalName == localName)?.Value ?? string.Empty;

                decimal GetDecimalValue(System.Xml.Linq.XElement? parent, string localName) =>
                    ParseDecimal(GetValue(parent, localName));

                var invoice = xdoc.Descendants().FirstOrDefault(x => x.Name.LocalName == "Invoice");
                if (invoice == null)
                {
                    result.Errors.Add($"{file.FileName}: Geçerli bir UBL Fatura formatı değil.");
                    result.ErrorCount++;
                    continue;
                }

                var faturaNo = NormalizeFaturaNo(GetValue(invoice, "ID"));
                var issueDateStr = GetValue(invoice, "IssueDate");
                
                if (string.IsNullOrWhiteSpace(faturaNo))
                {
                    result.Errors.Add($"{file.FileName}: Fatura No (ID) bulunamadı.");
                    result.ErrorCount++;
                    continue;
                }

                var existingFatura = await FindExistingFaturaAsync(faturaNo);
                if (existingFatura != null)
                {
                    result.Errors.Add($"{file.FileName}: {faturaNo} no'lu fatura sistemde zaten var.");
                    result.SkippedCount++;
                    continue;
                }

                var ettn = GetValue(invoice, "UUID");
                var profileId = GetValue(invoice, "ProfileID");
                var invoiceTypeCode = GetValue(invoice, "InvoiceTypeCode");
                
                var fatTip = !string.IsNullOrEmpty(profileId) && profileId.ToUpperInvariant().Contains("EARSIV")
                    ? EFaturaTipi.EArsiv
                    : defaultEFaturaTipi;

                if (!DateTime.TryParse(issueDateStr, out var faturaTarihi))
                {
                    faturaTarihi = DateTime.Today;
                }
                faturaTarihi = DateTime.SpecifyKind(faturaTarihi.Date, DateTimeKind.Utc);

                string cariUnvan = string.Empty;
                string cariVkn = string.Empty;
                string cariVergiDairesi = string.Empty;
                string cariAdres = string.Empty;
                string cariTelefon = string.Empty;
                string cariEmail = string.Empty;
                string cariIlce = string.Empty;
                string cariIl = string.Empty;
                string cariUlke = string.Empty;
                string cariTcKimlikNo = string.Empty;
                
                System.Xml.Linq.XElement? supplierNode = invoice.Descendants().FirstOrDefault(x => x.Name.LocalName == "AccountingSupplierParty");
                System.Xml.Linq.XElement? customerNode = invoice.Descendants().FirstOrDefault(x => x.Name.LocalName == "AccountingCustomerParty");
                var targetNode = yon == FaturaYonu.Giden ? customerNode : supplierNode;
                var partyNode = targetNode?.Descendants().FirstOrDefault(x => x.Name.LocalName == "Party");

                if (partyNode != null)
                {
                    // Vergi No / TC Kimlik No - önce bunu al
                    var partyIdentification = partyNode.Descendants().FirstOrDefault(x => x.Name.LocalName == "PartyIdentification");
                    if (partyIdentification != null)
                    {
                        cariVkn = GetValue(partyIdentification, "ID").Trim();
                    }
                    
                    // 11 haneli ise TCKN - şahıs faturası
                    bool sahisFaturasi = !string.IsNullOrEmpty(cariVkn) && cariVkn.Length == 11;
                    
                    // Şahıs faturası ise önce Person'dan ad soyad al
                    if (sahisFaturasi)
                    {
                        var personNode = partyNode.Descendants().FirstOrDefault(x => x.Name.LocalName == "Person");
                        if (personNode != null)
                        {
                            var firstName = GetValue(personNode, "FirstName").Trim();
                            var familyName = GetValue(personNode, "FamilyName").Trim();
                            if (!string.IsNullOrWhiteSpace(firstName) || !string.IsNullOrWhiteSpace(familyName))
                            {
                                cariUnvan = $"{firstName} {familyName}".Trim();
                                cariTcKimlikNo = cariVkn; // TCKN olarak kaydet
                                cariVkn = string.Empty; // Vergi no boş
                            }
                        }
                    }
                    
                    // Şahıs değilse veya Person'dan alınamadıysa PartyName'den al
                    if (string.IsNullOrWhiteSpace(cariUnvan))
                    {
                        var partyNameNode = partyNode.Descendants().FirstOrDefault(x => x.Name.LocalName == "PartyName");
                        if (partyNameNode != null)
                        {
                            var nameValue = GetValue(partyNameNode, "Name").Trim();
                            // Ülke adı değilse kullan
                            if (!IsCountryName(nameValue))
                            {
                                cariUnvan = nameValue;
                            }
                        }
                    }
                    
                    // Hala boşsa eski yöntemle dene
                    if (string.IsNullOrWhiteSpace(cariUnvan))
                    {
                        var nameValue = GetValue(partyNode, "Name").Trim();
                        if (!IsCountryName(nameValue))
                        {
                            cariUnvan = nameValue;
                        }
                    }
                    
                    // Vergi Dairesi
                    var taxScheme = partyNode.Descendants().FirstOrDefault(x => x.Name.LocalName == "PartyTaxScheme");
                    if (taxScheme != null)
                    {
                        cariVergiDairesi = GetValue(taxScheme, "Name").Trim();
                        // Eğer Name yoksa TaxSchemeName'den al
                        if (string.IsNullOrWhiteSpace(cariVergiDairesi))
                        {
                            var taxSchemeNode = taxScheme.Descendants().FirstOrDefault(x => x.Name.LocalName == "TaxScheme");
                            if (taxSchemeNode != null)
                            {
                                cariVergiDairesi = GetValue(taxSchemeNode, "Name").Trim();
                            }
                        }
                    }
                    
                    // Adres bilgileri
                    var postalAddress = partyNode.Descendants().FirstOrDefault(x => x.Name.LocalName == "PostalAddress");
                    if (postalAddress != null)
                    {
                        var streetName = GetValue(postalAddress, "StreetName").Trim();
                        var buildingName = GetValue(postalAddress, "BuildingName").Trim();
                        var buildingNumber = GetValue(postalAddress, "BuildingNumber").Trim();
                        var room = GetValue(postalAddress, "Room").Trim();
                        var citySubdivisionName = GetValue(postalAddress, "CitySubdivisionName").Trim();
                        var cityName = GetValue(postalAddress, "CityName").Trim();
                        var postalZone = GetValue(postalAddress, "PostalZone").Trim();
                        var countryName = GetValue(postalAddress.Descendants().FirstOrDefault(x => x.Name.LocalName == "Country"), "Name").Trim();
                        
                        // Tam adres oluştur
                        var adresParcalari = new List<string>();
                        if (!string.IsNullOrWhiteSpace(streetName)) adresParcalari.Add(streetName);
                        if (!string.IsNullOrWhiteSpace(buildingName)) adresParcalari.Add(buildingName);
                        if (!string.IsNullOrWhiteSpace(buildingNumber)) adresParcalari.Add($"No:{buildingNumber}");
                        if (!string.IsNullOrWhiteSpace(room)) adresParcalari.Add($"Daire:{room}");
                        
                        cariAdres = string.Join(" ", adresParcalari);
                        cariIlce = citySubdivisionName;
                        cariIl = cityName;
                        cariUlke = countryName;
                        
                        // Tam adres formatı
                        if (!string.IsNullOrWhiteSpace(cariIlce) || !string.IsNullOrWhiteSpace(cariIl))
                        {
                            if (!string.IsNullOrWhiteSpace(cariAdres))
                                cariAdres += " ";
                            if (!string.IsNullOrWhiteSpace(cariIlce))
                                cariAdres += cariIlce + " ";
                            if (!string.IsNullOrWhiteSpace(cariIl))
                                cariAdres += cariIl;
                            if (!string.IsNullOrWhiteSpace(cariUlke) && !IsCountryName(cariUlke))
                                cariAdres += " / " + cariUlke;
                        }
                        cariAdres = cariAdres.Trim();
                    }
                    
                    // İletişim bilgileri
                    var contact = partyNode.Descendants().FirstOrDefault(x => x.Name.LocalName == "Contact");
                    if (contact != null)
                    {
                        cariTelefon = GetValue(contact, "Telephone").Trim();
                        if (string.IsNullOrWhiteSpace(cariTelefon))
                            cariTelefon = GetValue(contact, "Telefax").Trim();
                        cariEmail = GetValue(contact, "ElectronicMail").Trim();
                    }
                }

                if (string.IsNullOrWhiteSpace(cariUnvan))
                {
                    result.Errors.Add($"{file.FileName}: Cari unvan bilgisi XML içinden çıkarılamadı.");
                    result.ErrorCount++;
                    continue;
                }
                
                Cari? cari = null;
                // TCKN varsa ona göre ara, yoksa VKN'ye göre
                var aramaNo = !string.IsNullOrWhiteSpace(cariTcKimlikNo) ? cariTcKimlikNo : cariVkn;
                var cariKey = !string.IsNullOrWhiteSpace(aramaNo) ? aramaNo : cariUnvan.ToLowerInvariant();
                
                if (!string.IsNullOrEmpty(cariKey) && importCarileri.TryGetValue(cariKey, out var mevcutCari))
                {
                    cari = mevcutCari;
                }
                
                // TCKN ile ara
                if (cari == null && !string.IsNullOrWhiteSpace(cariTcKimlikNo))
                {
                    cari = await _context.Cariler.Include(c => c.MuhasebeHesap).FirstOrDefaultAsync(c => c.TcKimlikNo == cariTcKimlikNo);
                }
                
                // VKN ile ara
                if (cari == null && !string.IsNullOrWhiteSpace(cariVkn) && cariVkn.Length >= 10)
                {
                    cari = await _context.Cariler.Include(c => c.MuhasebeHesap).FirstOrDefaultAsync(c => c.VergiNo == cariVkn);
                }
                

                // Mevcut cari varsa eksik bilgileri güncelle
                if (cari != null)
                {
                    bool guncellendi = false;
                    
                    if (string.IsNullOrWhiteSpace(cari.VergiDairesi) && !string.IsNullOrWhiteSpace(cariVergiDairesi))
                    {
                        cari.VergiDairesi = cariVergiDairesi;
                        guncellendi = true;
                    }
                    if (string.IsNullOrWhiteSpace(cari.Adres) && !string.IsNullOrWhiteSpace(cariAdres))
                    {
                        cari.Adres = cariAdres;
                        guncellendi = true;
                    }
                    if (string.IsNullOrWhiteSpace(cari.Telefon) && !string.IsNullOrWhiteSpace(cariTelefon))
                    {
                        cari.Telefon = cariTelefon;
                        guncellendi = true;
                    }
                    if (string.IsNullOrWhiteSpace(cari.Email) && !string.IsNullOrWhiteSpace(cariEmail))
                    {
                        cari.Email = cariEmail;
                        guncellendi = true;
                    }
                    // TCKN eksikse ekle
                    if (string.IsNullOrWhiteSpace(cari.TcKimlikNo) && !string.IsNullOrWhiteSpace(cariTcKimlikNo))
                    {
                        cari.TcKimlikNo = cariTcKimlikNo;
                        guncellendi = true;
                    }
                    
                    if (guncellendi)
                    {
                        cari.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                }

                if (cari == null && otomatikCariOlustur)
                {
                    var uniqueCode = await GetUniqueCariCodeAsync(nextCariNum);
                    var cariTipi = yon == FaturaYonu.Giden ? CariTipi.Musteri : CariTipi.Tedarikci;
                    
                    cari = new Cari
                    {
                        CariKodu = uniqueCode,
                        Unvan = cariUnvan,
                        VergiNo = cariVkn ?? string.Empty,
                        TcKimlikNo = cariTcKimlikNo ?? string.Empty,
                        VergiDairesi = cariVergiDairesi,
                        Adres = cariAdres,
                        Telefon = cariTelefon,
                        Email = cariEmail,
                        CariTipi = cariTipi,
                        Aktif = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    _context.Cariler.Add(cari);
                    await _context.SaveChangesAsync();
                    nextCariNum++;

                    // Otomatik muhasebe hesap kodu oluştur
                    if (otomatikHesapKoduOlustur && ayar != null)
                    {
                        var prefix = cariTipi == CariTipi.Musteri ? ayar.MusteriPrefix : ayar.TedarikciPrefix;
                        var yeniHesapKodu = await GetSonrakiHesapKoduAsync(prefix);
                        
                        var yeniHesap = new MuhasebeHesap
                        {
                            HesapKodu = yeniHesapKodu,
                            HesapAdi = cariUnvan,
                            HesapTuru = cariTipi == CariTipi.Musteri ? HesapTuru.Aktif : HesapTuru.Pasif,
                            HesapGrubu = cariTipi == CariTipi.Musteri ? HesapGrubu.DonenVarliklar : HesapGrubu.KisaVadeliYabanciKaynaklar,
                            Aktif = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        
                        _context.MuhasebeHesaplari.Add(yeniHesap);
                        await _context.SaveChangesAsync();

                        cari.MuhasebeHesapId = yeniHesap.Id;
                        await _context.SaveChangesAsync();
                    }
                }

                if (cari == null)
                {
                    result.Errors.Add($"{file.FileName}: Cari bulunamadı ve otomatik oluşturma kapalı.");
                    result.ErrorCount++;
                    continue;
                }

                if (!string.IsNullOrEmpty(cariKey))
                {
                    importCarileri[cariKey] = cari;
                }

                var legalMonetaryTotal = invoice.Descendants().FirstOrDefault(x => x.Name.LocalName == "LegalMonetaryTotal");
                var dAraToplam = GetDecimalValue(legalMonetaryTotal, "TaxExclusiveAmount");
                var dGenelToplam = GetDecimalValue(legalMonetaryTotal, "PayableAmount");
                if (dGenelToplam == 0)
                    dGenelToplam = GetDecimalValue(legalMonetaryTotal, "TaxInclusiveAmount");

                var dKdvTutar = dGenelToplam - dAraToplam;

                // Tevkifat bilgilerini oku
                var tevkifatliMi = false;
                decimal tevkifatOrani = 0;
                decimal tevkifatTutar = 0;
                string? tevkifatKodu = null;

                var withholdingTaxTotal = invoice.Descendants().FirstOrDefault(x => x.Name.LocalName == "WithholdingTaxTotal");
                if (withholdingTaxTotal != null)
                {
                    tevkifatliMi = true;
                    tevkifatTutar = GetDecimalValue(withholdingTaxTotal, "TaxAmount");
                    
                    var taxSubtotal = withholdingTaxTotal.Descendants().FirstOrDefault(x => x.Name.LocalName == "TaxSubtotal");
                    if (taxSubtotal != null)
                    {
                        tevkifatOrani = GetDecimalValue(taxSubtotal, "Percent");
                        var taxCategory = taxSubtotal.Descendants().FirstOrDefault(x => x.Name.LocalName == "TaxCategory");
                        if (taxCategory != null)
                        {
                            var taxScheme = taxCategory.Descendants().FirstOrDefault(x => x.Name.LocalName == "TaxScheme");
                            tevkifatKodu = GetValue(taxScheme, "TaxTypeCode");
                        }
                    }
                }

                if (dGenelToplam <= 0 && dAraToplam <= 0)
                {
                    result.Errors.Add($"{file.FileName}: Tutar bilgisi eksik.");
                    result.ErrorCount++;
                    continue;
                }

                var fatura = new Fatura
                {
                    FaturaNo = faturaNo,
                    FaturaTarihi = faturaTarihi,
                    CariId = cari.Id,
                    FirmaId = firmaId,
                    FaturaYonu = yon,
                    FaturaTipi = tevkifatliMi ? FaturaTipi.TevkifatliFatura : (yon == FaturaYonu.Giden ? FaturaTipi.SatisFaturasi : FaturaTipi.AlisFaturasi),
                    EFaturaTipi = fatTip,
                    EttnNo = ettn,
                    AraToplam = dAraToplam > 0 ? dAraToplam : dGenelToplam,
                    IskontoTutar = 0,
                    KdvTutar = dKdvTutar,
                    GenelToplam = dGenelToplam,
                    TevkifatliMi = tevkifatliMi,
                    TevkifatOrani = tevkifatOrani,
                    TevkifatKodu = tevkifatKodu,
                    TevkifatTutar = tevkifatTutar,
                    Durum = FaturaDurum.Beklemede,
                    ImportKaynak = "XML",
                    CreatedAt = DateTime.UtcNow
                };

                // Fatura kalemlerini oku
                var invoiceLines = invoice.Descendants().Where(x => x.Name.LocalName == "InvoiceLine").ToList();
                var siraNo = 1;
                
                foreach (var line in invoiceLines)
                {
                    var urunKodu = GetValue(line, "ID");
                    var aciklama = GetValue(line.Descendants().FirstOrDefault(x => x.Name.LocalName == "Item"), "Name");
                    
                    var kalem = new FaturaKalem
                    {
                        SiraNo = siraNo++,
                        UrunKodu = urunKodu,
                        Aciklama = aciklama,
                        Miktar = GetDecimalValue(line, "InvoicedQuantity"),
                        BirimFiyat = GetDecimalValue(line.Descendants().FirstOrDefault(x => x.Name.LocalName == "Price"), "PriceAmount"),
                        KdvOrani = GetDecimalValue(line.Descendants().FirstOrDefault(x => x.Name.LocalName == "TaxTotal")?.Descendants().FirstOrDefault(x => x.Name.LocalName == "TaxSubtotal")?.Descendants().FirstOrDefault(x => x.Name.LocalName == "TaxCategory"), "Percent"),
                        KdvTutar = GetDecimalValue(line.Descendants().FirstOrDefault(x => x.Name.LocalName == "TaxTotal"), "TaxAmount"),
                        ToplamTutar = GetDecimalValue(line, "LineExtensionAmount"),
                        CreatedAt = DateTime.UtcNow
                    };

                    // Birim bilgisini al
                    var quantityElement = line.Descendants().FirstOrDefault(x => x.Name.LocalName == "InvoicedQuantity");
                    if (quantityElement != null)
                    {
                        var unitCode = quantityElement.Attribute("unitCode")?.Value;
                        kalem.Birim = unitCode ?? "Adet";
                    }

                    // İskonto bilgisini al
                    var allowanceCharge = line.Descendants().FirstOrDefault(x => x.Name.LocalName == "AllowanceCharge");
                    if (allowanceCharge != null)
                    {
                        var chargeIndicator = GetValue(allowanceCharge, "ChargeIndicator");
                        if (chargeIndicator == "false")
                        {
                            kalem.IskontoTutar = GetDecimalValue(allowanceCharge, "Amount");
                            kalem.IskontoOrani = GetDecimalValue(allowanceCharge, "MultiplierFactorNumeric") * 100;
                        }
                    }

                    // Kalem bazında tevkifat
                    var lineWithholdingTax = line.Descendants().FirstOrDefault(x => x.Name.LocalName == "WithholdingTaxTotal");
                    if (lineWithholdingTax != null)
                    {
                        kalem.TevkifatTutar = GetDecimalValue(lineWithholdingTax, "TaxAmount");
                        var lineTaxSubtotal = lineWithholdingTax.Descendants().FirstOrDefault(x => x.Name.LocalName == "TaxSubtotal");
                        if (lineTaxSubtotal != null)
                        {
                            kalem.TevkifatOrani = GetDecimalValue(lineTaxSubtotal, "Percent");
                        }
                    }

                    // Kalem tipini belirle (açıklamaya göre)
                    kalem.KalemTipi = DetermineKalemTipi(aciklama, urunKodu);
                    kalem.AltTipi = DetermineKalemAltTipi(aciklama, urunKodu, kalem.KalemTipi);

                    // Araç ilişkisi - açıklamada şase/plaka varsa
                    var aracBilgisi = ExtractAracBilgisi(aciklama);
                    if (aracBilgisi.HasValue)
                    {
                        var arac = await FindOrCreateAracAsync(aracBilgisi.Value, yon, cari.Id);
                        if (arac != null)
                        {
                            kalem.AracId = arac.Id;
                            kalem.KalemTipi = FaturaKalemTipi.Arac;
                            kalem.AltTipi = yon == FaturaYonu.Giden ? FaturaKalemAltTipi.AracSatis : FaturaKalemAltTipi.AracAlis;
                            
                            // Ana faturaya da araç ilişkisi ekle
                            fatura.AracId = arac.Id;
                            fatura.AracFaturasi = true;
                        }
                    }

                    // Varsayılan muhasebe hesabı ata (kalem tipine göre)
                    if (otomatikHesapKoduOlustur && ayar != null)
                    {
                        var hesapKodu = GetMuhasebeHesapKoduByKalemTipi(kalem.KalemTipi, yon, ayar);
                        var hesap = await _context.MuhasebeHesaplari.FirstOrDefaultAsync(h => h.HesapKodu == hesapKodu);
                        if (hesap != null)
                        {
                            kalem.MuhasebeHesapId = hesap.Id;
                        }
                    }

                    fatura.FaturaKalemleri.Add(kalem);
                }

                _context.Faturalar.Add(fatura);
                await _context.SaveChangesAsync();

                // Otomatik muhasebe fişi oluştur
                await TryCreateMuhasebeFisiAsync(fatura);
                
                result.ImportedItems.Add(fatura);
                result.ImportedCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"{file.FileName} parse hatası: {ex.InnerException?.Message ?? ex.Message}");
                result.ErrorCount++;
                
                foreach (var entry in _context.ChangeTracker.Entries().ToList())
                {
                    if (entry.State == EntityState.Added)
                    {
                        entry.State = EntityState.Detached;
                    }
                }
            }
        }
        
        result.Success = result.ImportedCount > 0;
        return result;
    }

    private async Task<string> GetSonrakiHesapKoduAsync(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            return string.Empty;

        var sonKod = await _context.MuhasebeHesaplari
            .Where(h => h.HesapKodu.StartsWith(prefix + "."))
            .OrderByDescending(h => h.HesapKodu)
            .Select(h => h.HesapKodu)
            .FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(sonKod))
            return $"{prefix}.001";

        var sonParca = sonKod.Split('.').LastOrDefault();
        if (!int.TryParse(sonParca, out var sonNumara))
            return $"{prefix}.001";

        return $"{prefix}.{sonNumara + 1:D3}";
    }

    private static bool IsCountryName(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        
        var upper = value.ToUpperInvariant().Trim();
        var countryNames = new[]
        {
            "TÜRKİYE", "TURKIYE", "TURKEY", "TR",
            "ALMANYA", "GERMANY", "FRANSA", "FRANCE",
            "İNGİLTERE", "INGILTERE", "ENGLAND", "UK",
            "ABD", "USA", "AMERIKA", "AMERICA",
            "HOLLANDA", "NETHERLANDS", "BELÇİKA", "BELGIUM"
        };
        
        return countryNames.Contains(upper);
    }

    private async Task<Fatura?> FindExistingFaturaAsync(string faturaNo)
    {
        var normalizedFaturaNo = NormalizeFaturaNo(faturaNo);
        if (string.IsNullOrWhiteSpace(normalizedFaturaNo))
        {
            return null;
        }

        return await _context.Faturalar
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(f => f.FaturaNo.Trim().ToUpper() == normalizedFaturaNo);
    }

    private static string NormalizeFaturaNo(string? value)
        => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();

    private static decimal ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0;
        value = value.Replace(".", "").Replace(",", ".").Trim();
        return decimal.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : 0;
    }

    /// <summary>
    /// Excel Sablon - Ornek dosya formatina gore:
    /// A: Unvani/Adi Soyadi, B: Vkn/Tckn, C: Fatura Tipi, D: Fatura Tarihi, E: Fatura No,
    /// F: Iskonto, G: Kdv Matrahı %0, H: Kdv Matrahı %1, I: Kdv Matrahı %10, J: Kdv Matrahı %20,
    /// K: Kdv%1, L: Kdv%10, M: Kdv%20, N: Odenecek Tutar Turk Lirasi
    /// </summary>
    public async Task<byte[]> GetExcelSablonAsync(FaturaYonu yon)
    {
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Fatura Import");

        // Basliklar - Ornek dosya formatinda
        var headers = new[] { 
            "Ünvanı/Adı Soyadi", "Vkn/Tckn", "Fatura Tipi", "Fatura Tarihi", "Fatura No.",
            "İskonto", "Kdv Matrahı %0", "Kdv Matrahı %1", "Kdv Matrahı %10", "Kdv Matrahı %20",
            "Kdv%1", "Kdv%10", "Kdv%20", "Ödenecek Tutar Türk Lirası"
        };
        
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cells[1, i + 1].Value = headers[i];
            ws.Cells[1, i + 1].Style.Font.Bold = true;
            ws.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            ws.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
        }

        // Ornek satir
        ws.Cells[2, 1].Value = "ORNEK FIRMA A.S.";
        ws.Cells[2, 2].Value = "1234567890";
        ws.Cells[2, 3].Value = "SATIS";
        ws.Cells[2, 4].Value = DateTime.Today.ToString("dd.MM.yyyy");
        ws.Cells[2, 5].Value = $"FTR{DateTime.Now:yyyyMM}000001";
        ws.Cells[2, 6].Value = "0,00";
        ws.Cells[2, 7].Value = ""; // %0 matrah
        ws.Cells[2, 8].Value = ""; // %1 matrah
        ws.Cells[2, 9].Value = ""; // %10 matrah
        ws.Cells[2, 10].Value = "1000,00"; // %20 matrah
        ws.Cells[2, 11].Value = ""; // %1 kdv
        ws.Cells[2, 12].Value = ""; // %10 kdv
        ws.Cells[2, 13].Value = "200,00"; // %20 kdv
        ws.Cells[2, 14].Value = "1200,00"; // Toplam

        // Aciklamalar
        ws.Cells[5, 1].Value = "ACIKLAMALAR:";
        ws.Cells[5, 1].Style.Font.Bold = true;
        ws.Cells[6, 1].Value = "* Ünvanı: Cari unvani (zorunlu)";
        ws.Cells[7, 1].Value = "* Vkn/Tckn: Vergi veya TC kimlik no (varsa mevcut cari bulunur, yoksa yeni olusturulur)";
        ws.Cells[8, 1].Value = "* Fatura Tipi: SATIS veya ALIS";
        ws.Cells[9, 1].Value = "* Fatura Tarihi: GG.AA.YYYY formatinda";
        ws.Cells[10, 1].Value = "* Fatura No: Benzersiz fatura numarasi (zorunlu)";
        ws.Cells[11, 1].Value = "* KDV Matrahlari: İlgili KDV oranina gore matrah tutarlari";
        ws.Cells[12, 1].Value = "* KDV Tutarlari: Her oran icin KDV tutarlari";
        ws.Cells[13, 1].Value = "* Odenecek Tutar: Toplam fatura tutari";

        ws.Cells.AutoFitColumns();
        return await Task.FromResult(package.GetAsByteArray());
    }

    public async Task<byte[]> ExportToExcelAsync(List<Fatura> faturalar)
    {
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Faturalar");

        // Basliklar
        var headers = new[] { "Fatura No", "Tarih", "Vade", "Cari", "VKN", "Matrah", "KDV", "Toplam", "Odenen", "Kalan", "Durum", "Tip", "ETTN" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cells[1, i + 1].Value = headers[i];
            ws.Cells[1, i + 1].Style.Font.Bold = true;
            ws.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            ws.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
        }

        // Veriler
        int row = 2;
        foreach (var f in faturalar)
        {
            ws.Cells[row, 1].Value = f.FaturaNo;
            ws.Cells[row, 2].Value = f.FaturaTarihi.ToString("dd.MM.yyyy");
            ws.Cells[row, 3].Value = f.VadeTarihi?.ToString("dd.MM.yyyy");
            ws.Cells[row, 4].Value = f.Cari?.Unvan;
            ws.Cells[row, 5].Value = f.Cari?.VergiNo;
            ws.Cells[row, 6].Value = f.AraToplam;
            ws.Cells[row, 7].Value = f.KdvTutar;
            ws.Cells[row, 8].Value = f.GenelToplam;
            ws.Cells[row, 9].Value = f.OdenenTutar;
            ws.Cells[row, 10].Value = f.KalanTutar;
            ws.Cells[row, 11].Value = f.Durum.ToString();
            ws.Cells[row, 12].Value = f.EFaturaTipi == EFaturaTipi.EFatura ? "E-Fatura" : "E-Arsiv";
            ws.Cells[row, 13].Value = f.EttnNo;
            row++;
        }

        // Ozet satiri
        row++;
        ws.Cells[row, 5].Value = "TOPLAM:";
        ws.Cells[row, 5].Style.Font.Bold = true;
        ws.Cells[row, 6].Value = faturalar.Sum(f => f.AraToplam);
        ws.Cells[row, 7].Value = faturalar.Sum(f => f.KdvTutar);
        ws.Cells[row, 8].Value = faturalar.Sum(f => f.GenelToplam);
        ws.Cells[row, 9].Value = faturalar.Sum(f => f.OdenenTutar);
        ws.Cells[row, 10].Value = faturalar.Sum(f => f.KalanTutar);

        // Format
        ws.Cells[2, 6, row, 10].Style.Numberformat.Format = "#,##0.00";
        ws.Cells.AutoFitColumns();

        return await Task.FromResult(package.GetAsByteArray());
    }

    #endregion

    private static void CalculateTotals(Fatura fatura)
    {
        if (fatura.FaturaKalemleri != null && fatura.FaturaKalemleri.Count != 0)
        {
            foreach (var kalem in fatura.FaturaKalemleri)
            {
                var netTutar = kalem.Miktar * kalem.BirimFiyat;
                kalem.KdvTutar = netTutar * kalem.KdvOrani / 100;
                kalem.ToplamTutar = netTutar + kalem.KdvTutar;
            }

            fatura.AraToplam = fatura.FaturaKalemleri.Sum(k => k.Miktar * k.BirimFiyat);
            fatura.KdvTutar = fatura.FaturaKalemleri.Sum(k => k.KdvTutar);
            fatura.GenelToplam = fatura.FaturaKalemleri.Sum(k => k.ToplamTutar);
        }
    }

    /// <summary>
    /// Fatura için otomatik muhasebe fişi oluşturur (ayarlara göre)
    /// </summary>
    private async Task TryCreateMuhasebeFisiAsync(Fatura fatura)
    {
        try
        {
            // Ayarları kontrol et
            var ayar = await _context.MuhasebeAyarlari.FirstOrDefaultAsync();
            if (ayar == null || !ayar.FaturaOtomatikMuhasebeFisi)
                return;

            // Faturayı tam olarak yükle
            var fullFatura = await _context.Faturalar
                .Include(f => f.Cari)
                .Include(f => f.FaturaKalemleri)
                .FirstOrDefaultAsync(f => f.Id == fatura.Id);

            if (fullFatura == null)
                return;

            // Daha önce fiş oluşturulmuş mu kontrol et
            var mevcutFis = await _context.MuhasebeFisleri
                .AnyAsync(f => f.Kaynak == FisKaynak.Fatura && f.KaynakId == fatura.Id);

            if (mevcutFis)
                return;

            // Muhasebe fişi oluştur
            await _muhasebeService.CreateFaturaFisiAsync(fullFatura);
        }
        catch
        {
            // Muhasebe fişi oluşturma hatası fatura kaydını engellemez
        }
    }

    /// <summary>
    /// Manuel olarak fatura için muhasebe fişi oluşturur
    /// </summary>
    public async Task<MuhasebeFis> CreateMuhasebeFisiAsync(int faturaId)
    {
        var fatura = await _context.Faturalar
            .Include(f => f.Cari)
            .Include(f => f.FaturaKalemleri)
            .FirstOrDefaultAsync(f => f.Id == faturaId);

        if (fatura == null)
            throw new Exception("Fatura bulunamadı");

        // Daha önce fiş oluşturulmuş mu kontrol et
        var mevcutFis = await _context.MuhasebeFisleri
            .FirstOrDefaultAsync(f => f.Kaynak == FisKaynak.Fatura && f.KaynakId == faturaId);

        if (mevcutFis != null)
            throw new Exception("Bu fatura için muhasebe fişi zaten oluşturulmuş");

        return await _muhasebeService.CreateFaturaFisiAsync(fatura);
    }

    #region Yardımcı Metodlar

    private async Task<int> GetNextCariNumAsync()
    {
        var lastCari = await _context.Cariler
            .IgnoreQueryFilters()
            .Where(c => c.CariKodu.StartsWith("C"))
            .OrderByDescending(c => c.CariKodu)
            .FirstOrDefaultAsync();

        if (lastCari != null && lastCari.CariKodu.Length > 1)
        {
            var numPart = lastCari.CariKodu.Substring(1);
            if (int.TryParse(numPart, out var num))
                return num + 1;
        }
        return 1;
    }

    private async Task<string> GetUniqueCariCodeAsync(int startNum)
    {
        var code = $"C{startNum:D5}";
        var exists = await _context.Cariler.IgnoreQueryFilters().AnyAsync(c => c.CariKodu == code);
        while (exists)
        {
            startNum++;
            code = $"C{startNum:D5}";
            exists = await _context.Cariler.IgnoreQueryFilters().AnyAsync(c => c.CariKodu == code);
        }
        return code;
    }

    /// <summary>
    /// Açıklamaya göre kalem tipini belirler
    /// </summary>
    private static FaturaKalemTipi DetermineKalemTipi(string? aciklama, string? urunKodu)
    {
        if (string.IsNullOrWhiteSpace(aciklama))
            return FaturaKalemTipi.Hizmet;

        var lower = aciklama.ToLowerInvariant();

        // Araç kontrolü
        if (lower.Contains("araç") || lower.Contains("otomobil") || lower.Contains("minibüs") || 
            lower.Contains("otobüs") || lower.Contains("midibüs") || lower.Contains("panelvan") ||
            lower.Contains("şase") || lower.Contains("plaka"))
            return FaturaKalemTipi.Arac;

        // Servis kontrolü
        if (lower.Contains("servis") || lower.Contains("bakım") || lower.Contains("onarım") ||
            lower.Contains("tamir") || lower.Contains("yağ değişimi") || lower.Contains("lastik") ||
            lower.Contains("muayene") || lower.Contains("sigorta") || lower.Contains("kasko"))
            return FaturaKalemTipi.Servis;

        // Demirbaş kontrolü
        if (lower.Contains("demirbaş") || lower.Contains("ofis") || lower.Contains("makina") ||
            lower.Contains("makine") || lower.Contains("teçhizat") || lower.Contains("ekipman"))
            return FaturaKalemTipi.Demirbas;

        // Mal kontrolü
        if (lower.Contains("mal") || lower.Contains("ürün") || lower.Contains("parça") ||
            lower.Contains("yedek") || lower.Contains("malzeme"))
            return FaturaKalemTipi.Mal;

        // Varsayılan olarak hizmet
        return FaturaKalemTipi.Hizmet;
    }

    /// <summary>
    /// Açıklamaya göre kalem alt tipini belirler
    /// </summary>
    private static FaturaKalemAltTipi? DetermineKalemAltTipi(string? aciklama, string? urunKodu, FaturaKalemTipi kalemTipi)
    {
        if (string.IsNullOrWhiteSpace(aciklama))
            return null;

        var lower = aciklama.ToLowerInvariant();

        return kalemTipi switch
        {
            FaturaKalemTipi.Hizmet => lower switch
            {
                var s when s.Contains("taşıma") || s.Contains("nakil") || s.Contains("transfer") => FaturaKalemAltTipi.TasimaHizmeti,
                var s when s.Contains("kiralama") || s.Contains("kira") => FaturaKalemAltTipi.KiralamaHizmeti,
                var s when s.Contains("danışmanlık") || s.Contains("müşavirlik") => FaturaKalemAltTipi.DanismanlikHizmeti,
                _ => null
            },
            FaturaKalemTipi.Mal => lower switch
            {
                var s when s.Contains("yedek") || s.Contains("parça") => FaturaKalemAltTipi.YedekParca,
                var s when s.Contains("sarf") || s.Contains("malzeme") => FaturaKalemAltTipi.SarfMalzeme,
                _ => FaturaKalemAltTipi.TicariMal
            },
            FaturaKalemTipi.Demirbas => lower switch
            {
                var s when s.Contains("araç") => FaturaKalemAltTipi.AracDemirbas,
                var s when s.Contains("ofis") => FaturaKalemAltTipi.OfisEkipmani,
                var s when s.Contains("makina") || s.Contains("makine") || s.Contains("teçhizat") => FaturaKalemAltTipi.MakinaTechizat,
                _ => FaturaKalemAltTipi.DigerDemirbas
            },
            FaturaKalemTipi.Servis => lower switch
            {
                var s when s.Contains("bakım") || s.Contains("onarım") || s.Contains("tamir") => FaturaKalemAltTipi.BakimOnarim,
                var s when s.Contains("kasko") => FaturaKalemAltTipi.Kasko,
                var s when s.Contains("sigorta") => FaturaKalemAltTipi.Sigorta,
                var s when s.Contains("muayene") => FaturaKalemAltTipi.Muayene,
                var s when s.Contains("lastik") => FaturaKalemAltTipi.Lastik,
                var s when s.Contains("yakıt") || s.Contains("akaryakıt") => FaturaKalemAltTipi.Yakit,
                _ => FaturaKalemAltTipi.BakimOnarim
            },
            _ => null
        };
    }

    /// <summary>
    /// Açıklamadan araç bilgisi (şase/plaka) çıkarır
    /// </summary>
    private static (string? SaseNo, string? Plaka)? ExtractAracBilgisi(string? aciklama)
    {
        if (string.IsNullOrWhiteSpace(aciklama))
            return null;

        // Şase numarası pattern (17 karakter)
        var saseMatch = System.Text.RegularExpressions.Regex.Match(aciklama, @"\b[A-HJ-NPR-Z0-9]{17}\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        // Plaka pattern (34ABC123 gibi)
        var plakaMatch = System.Text.RegularExpressions.Regex.Match(aciklama, @"\b\d{2}\s?[A-Z]{1,3}\s?\d{2,4}\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (saseMatch.Success || plakaMatch.Success)
        {
            return (saseMatch.Success ? saseMatch.Value.ToUpperInvariant() : null, 
                    plakaMatch.Success ? plakaMatch.Value.Replace(" ", "").ToUpperInvariant() : null);
        }

        return null;
    }

    /// <summary>
    /// Şase veya plakaya göre araç bulur veya yeni araç oluşturur
    /// </summary>
    private async Task<Arac?> FindOrCreateAracAsync((string? SaseNo, string? Plaka) aracBilgisi, FaturaYonu yon, int cariId)
    {
        Arac? arac = null;

        // Önce şase ile ara
        if (!string.IsNullOrWhiteSpace(aracBilgisi.SaseNo))
        {
            arac = await _context.Araclar
                .FirstOrDefaultAsync(a => a.SaseNo == aracBilgisi.SaseNo);
        }

        // Şase bulunamadıysa plaka ile ara
        if (arac == null && !string.IsNullOrWhiteSpace(aracBilgisi.Plaka))
        {
            var plakaKaydi = await _context.Set<AracPlaka>()
                .Include(p => p.Arac)
                .FirstOrDefaultAsync(p => p.Plaka == aracBilgisi.Plaka);
            
            arac = plakaKaydi?.Arac;
        }

        // Araç bulunamadıysa ve alış faturasıysa yeni araç oluştur
        if (arac == null && yon == FaturaYonu.Gelen && !string.IsNullOrWhiteSpace(aracBilgisi.SaseNo))
        {
            arac = new Arac
            {
                SaseNo = aracBilgisi.SaseNo,
                AktifPlaka = aracBilgisi.Plaka,
                SahiplikTipi = AracSahiplikTipi.Ozmal,
                Aktif = true,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Araclar.Add(arac);
            await _context.SaveChangesAsync();

            // Plaka kaydı oluştur
            if (!string.IsNullOrWhiteSpace(aracBilgisi.Plaka))
            {
                var plakaKaydi = new AracPlaka
                {
                    AracId = arac.Id,
                    Plaka = aracBilgisi.Plaka,
                    GirisTarihi = DateTime.Today,
                    IslemTipi = PlakaIslemTipi.Alis,
                    CariId = cariId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Set<AracPlaka>().Add(plakaKaydi);
                await _context.SaveChangesAsync();
            }
        }

        return arac;
    }

    /// <summary>
    /// Kalem tipine göre varsayılan muhasebe hesap kodunu döner
    /// </summary>
    private static string GetMuhasebeHesapKoduByKalemTipi(FaturaKalemTipi kalemTipi, FaturaYonu yon, MuhasebeAyar ayar)
    {
        if (yon == FaturaYonu.Giden)
        {
            // Satış faturası
            return kalemTipi switch
            {
                FaturaKalemTipi.Arac => "253", // Taşıtlar (satış için)
                FaturaKalemTipi.Demirbas => "255", // Demirbaşlar
                FaturaKalemTipi.Mal => "600.01", // Satış geliri
                FaturaKalemTipi.Hizmet => ayar.SatisGelirHesabi,
                FaturaKalemTipi.Servis => ayar.SatisGelirHesabi,
                _ => ayar.SatisGelirHesabi
            };
        }
        else
        {
            // Alış faturası
            return kalemTipi switch
            {
                FaturaKalemTipi.Arac => "253", // Taşıtlar
                FaturaKalemTipi.Demirbas => "255", // Demirbaşlar
                FaturaKalemTipi.Mal => "153", // Ticari mallar
                FaturaKalemTipi.Hizmet => ayar.AlisGiderHesabi,
                FaturaKalemTipi.Servis => "770.07", // Bakım onarım giderleri
                _ => ayar.AlisGiderHesabi
            };
        }
    }

    #endregion
}
