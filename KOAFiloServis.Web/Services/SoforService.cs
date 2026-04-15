using KOAFiloServis.Shared.Entities;
using KOAFiloServis.Web.Data;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace KOAFiloServis.Web.Services;

public class SoforService : ISoforService
{
    private readonly ApplicationDbContext _context;
    private readonly IMuhasebeService _muhasebeService;

    public SoforService(ApplicationDbContext context, IMuhasebeService muhasebeService)
    {
        _context = context;
        _muhasebeService = muhasebeService;
    }

    public async Task<List<Sofor>> GetAllAsync()
    {
        var personeller = await QuerySoforler()
            .OrderBy(s => s.Ad)
            .ThenBy(s => s.Soyad)
            .ToListAsync();

        personeller.ForEach(NormalizeMaasBilgileri);
        return personeller;
    }

    public async Task<List<Sofor>> GetActiveAsync()
    {
        var personeller = await QuerySoforler()
            .Where(s => s.Aktif)
            .OrderBy(s => s.Ad)
            .ThenBy(s => s.Soyad)
            .ToListAsync();

        personeller.ForEach(NormalizeMaasBilgileri);
        return personeller;
    }

    public async Task<int> GetActiveCountAsync()
    {
        return await QuerySoforler()
            .Where(s => s.Aktif)
            .CountAsync();
    }

    public async Task<Sofor?> GetByIdAsync(int id)
    {
        var sofor = await QuerySoforler()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sofor != null)
            NormalizeMaasBilgileri(sofor);

        return sofor;
    }

    public async Task<Sofor> CreateAsync(Sofor sofor)
    {
        NormalizeSofor(sofor);
        ApplyMaasHesaplama(sofor);
        SyncBordroFlags(sofor);
        await ValidateSoforAsync(sofor);

        // Muhasebe hesabı oluştur veya eşleştir
        await OtomatikMuhasebeHesabiOlusturAsync(sofor);

        _context.Soforler.Add(sofor);
        await _context.SaveChangesAsync();
        return sofor;
    }

    public async Task<Sofor> UpdateAsync(Sofor sofor)
    {
        NormalizeSofor(sofor);
        ApplyMaasHesaplama(sofor);
        SyncBordroFlags(sofor);
        await ValidateSoforAsync(sofor);

        // Global NoTracking ayarı nedeniyle explicit tracking kullan
        var existing = await QuerySoforler(asNoTracking: false)
            .FirstOrDefaultAsync(s => s.Id == sofor.Id);

        if (existing == null)
            throw new InvalidOperationException($"Şoför bulunamadı. Id: {sofor.Id}");

        // Tüm alanları güncelle
        existing.SiralamaNo = sofor.SiralamaNo;
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
        existing.SgkCikisTarihi = sofor.SgkCikisTarihi;
        existing.BrutMaasHesaplamaTipi = sofor.BrutMaasHesaplamaTipi;
        existing.CalismaMiktari = sofor.CalismaMiktari;
        existing.BirimUcret = sofor.BirimUcret;
        existing.BrutMaas = sofor.BrutMaas;
        existing.ResmiNetMaas = sofor.ResmiNetMaas;
        existing.DigerMaas = sofor.DigerMaas;
        existing.SGKBordroDahilMi = sofor.SGKBordroDahilMi;
        existing.BordroTipiPersonel = sofor.BordroTipiPersonel;
        existing.BankaAdi = sofor.BankaAdi;
        existing.IBAN = sofor.IBAN;
        existing.MuhasebeHesapId = sofor.MuhasebeHesapId;
        existing.Notlar = sofor.Notlar;
        existing.Aktif = sofor.Aktif;
        existing.IsDeleted = sofor.IsDeleted;
        existing.UpdatedAt = DateTime.UtcNow;

        // Entity'yi explicit olarak güncelle (global NoTracking olduğu için)
        _context.Soforler.Update(existing);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(int id)
    {
        var sofor = await QuerySoforler(asNoTracking: false).FirstOrDefaultAsync(s => s.Id == id);
        if (sofor != null)
        {
            sofor.Aktif = false;
            sofor.IsDeleted = true;
            sofor.UpdatedAt = DateTime.UtcNow;
            _context.Soforler.Update(sofor);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateNextKodAsync()
    {
        return await GenerateNextKodAsync(PersonelGorev.Sofor);
    }

    public async Task<string> GenerateNextKodAsync(PersonelGorev gorev)
    {
        var prefix = GetKodPrefix(gorev);
        var lastKod = await _context.Soforler
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(s => s.SoforKodu.StartsWith(prefix + "-"))
            .OrderByDescending(s => s.SoforKodu)
            .Select(s => s.SoforKodu)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (lastKod != null)
        {
            var parts = lastKod.Split('-');
            if (parts.Length == 2 && int.TryParse(parts[1], out var num))
                nextNumber = num + 1;
        }

        return $"{prefix}-{nextNumber:D4}";
    }

    public static string GetKodPrefix(PersonelGorev gorev)
    {
        return gorev switch
        {
            PersonelGorev.Sofor => "SFR",
            PersonelGorev.Muhasebe => "MUH",
            _ => "PRS"
        };
    }

    // Görev bazlı filtreleme metodları
    public async Task<List<Sofor>> GetByGorevAsync(PersonelGorev gorev)
    {
        var personeller = await QuerySoforler()
            .Where(s => s.Gorev == gorev)
            .OrderBy(s => s.Ad)
            .ThenBy(s => s.Soyad)
            .ToListAsync();

        personeller.ForEach(NormalizeMaasBilgileri);
        return personeller;
    }

    public async Task<List<Sofor>> GetActiveSoforlerAsync()
    {
        var personeller = await QuerySoforler()
            .Where(s => s.Aktif && s.Gorev == PersonelGorev.Sofor)
            .OrderBy(s => s.Ad)
            .ThenBy(s => s.Soyad)
            .ToListAsync();

        personeller.ForEach(NormalizeMaasBilgileri);
        return personeller;
    }

    public async Task<List<Sofor>> GetActiveByGorevAsync(PersonelGorev gorev)
    {
        var personeller = await QuerySoforler()
            .Where(s => s.Aktif && s.Gorev == gorev)
            .OrderBy(s => s.Ad)
            .ThenBy(s => s.Soyad)
            .ToListAsync();

        personeller.ForEach(NormalizeMaasBilgileri);
        return personeller;
    }

    private IQueryable<Sofor> QuerySoforler(bool asNoTracking = true)
    {
        var query = _context.Soforler.Where(s => !s.IsDeleted);
        return asNoTracking ? query.AsNoTracking() : query;
    }

    private async Task ValidateSoforAsync(Sofor sofor)
    {
        var currentId = sofor.Id;

        if (string.IsNullOrWhiteSpace(sofor.SoforKodu))
            throw new InvalidOperationException("Personel kodu zorunludur.");

        if (string.IsNullOrWhiteSpace(sofor.Ad) || string.IsNullOrWhiteSpace(sofor.Soyad))
            throw new InvalidOperationException("Ad ve Soyad zorunludur.");

        if (sofor.IseBaslamaTarihi.HasValue && sofor.IstenAyrilmaTarihi.HasValue && sofor.IstenAyrilmaTarihi < sofor.IseBaslamaTarihi)
            throw new InvalidOperationException("İşten çıkış tarihi işe başlama tarihinden önce olamaz.");

        if (sofor.SGKBordroDahilMi && sofor.BordroTipiPersonel == PersonelBordroTipi.Yok)
            throw new InvalidOperationException("SGK bordroya dahil personel için bordro tipi seçilmelidir.");

        if (!string.IsNullOrWhiteSpace(sofor.TcKimlikNo))
        {
            if (sofor.TcKimlikNo.Length != 11 || !sofor.TcKimlikNo.All(char.IsDigit))
                throw new InvalidOperationException("TC Kimlik No 11 haneli olmalıdır.");

            var tcKimlikKullanimda = await QuerySoforler()
                .AnyAsync(s => s.Id != currentId && s.TcKimlikNo == sofor.TcKimlikNo);

            if (tcKimlikKullanimda)
                throw new InvalidOperationException($"'{sofor.TcKimlikNo}' TC Kimlik No zaten kullanımda.");
        }

        var kodKullanimda = await QuerySoforler()
            .AnyAsync(s => s.Id != currentId && s.SoforKodu == sofor.SoforKodu);

        if (kodKullanimda)
            throw new InvalidOperationException($"'{sofor.SoforKodu}' personel kodu zaten kullanımda.");
    }

    private static void NormalizeSofor(Sofor sofor)
    {
        sofor.SoforKodu = string.IsNullOrWhiteSpace(sofor.SoforKodu) ? string.Empty : sofor.SoforKodu.Trim().ToUpperInvariant();
        sofor.Ad = string.IsNullOrWhiteSpace(sofor.Ad) ? string.Empty : sofor.Ad.Trim();
        sofor.Soyad = string.IsNullOrWhiteSpace(sofor.Soyad) ? string.Empty : sofor.Soyad.Trim();
        sofor.TcKimlikNo = NormalizeNullableText(sofor.TcKimlikNo)?.Replace(" ", string.Empty);
        sofor.Telefon = NormalizeNullableText(sofor.Telefon);
        sofor.Email = NormalizeNullableText(sofor.Email);
        sofor.Adres = NormalizeNullableText(sofor.Adres);
        sofor.Departman = NormalizeNullableText(sofor.Departman);
        sofor.Pozisyon = NormalizeNullableText(sofor.Pozisyon);
        sofor.EhliyetNo = NormalizeNullableText(sofor.EhliyetNo);
        sofor.BankaAdi = NormalizeNullableText(sofor.BankaAdi);
        sofor.IBAN = NormalizeNullableText(sofor.IBAN)?.Replace(" ", string.Empty).ToUpperInvariant();
        sofor.Notlar = NormalizeNullableText(sofor.Notlar);
    }

    private static string? NormalizeNullableText(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void NormalizeMaasBilgileri(Sofor sofor)
    {
        ApplyMaasHesaplama(sofor);

        if (sofor.ResmiNetMaas == 0 && sofor.DigerMaas == 0 && sofor.NetMaas > 0)
        {
            sofor.ResmiNetMaas = sofor.NetMaas;
        }

        sofor.NetMaas = RoundCurrency(sofor.ResmiNetMaas + sofor.DigerMaas);
    }

    private static void ApplyMaasHesaplama(Sofor sofor)
    {
        sofor.CalismaMiktari = RoundCurrency(sofor.CalismaMiktari);
        sofor.BirimUcret = RoundCurrency(sofor.BirimUcret);

        if (sofor.BrutMaasHesaplamaTipi != BrutMaasHesaplamaTipi.Manuel)
        {
            sofor.BrutMaas = RoundCurrency(sofor.CalismaMiktari * sofor.BirimUcret);
        }
        else
        {
            sofor.BrutMaas = RoundCurrency(sofor.BrutMaas);
        }

        sofor.ResmiNetMaas = RoundCurrency(sofor.ResmiNetMaas);
        sofor.DigerMaas = RoundCurrency(sofor.DigerMaas);
        sofor.NetMaas = RoundCurrency(sofor.ResmiNetMaas + sofor.DigerMaas);
    }

    private static decimal RoundCurrency(decimal value)
        => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static void SyncBordroFlags(Sofor sofor)
    {
        // ArgePersoneli geriye dönük uyumluluk
        sofor.ArgePersoneli = sofor.SGKBordroDahilMi && sofor.BordroTipiPersonel == PersonelBordroTipi.Arge;
    }

    #region Excel Import/Export

    /// <summary>
    /// Personel import şablonu oluşturur (Excel)
    /// </summary>
    public Task<byte[]> GetImportSablonAsync()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Personel Import");

        // Başlık satırı
        var headers = new[]
        {
            "Ad*", "Soyad*", "TC Kimlik No", "Telefon", "Email", "Adres",
            "Görev (Sofor/OfisCalisani/Muhasebe/Yonetici/Teknik/Diger)", "Departman", "Pozisyon",
            "İşe Başlama (GG.AA.YYYY)", "Brüt Maaş", "Net Maaş",
            "SGK Bordrolu (Evet/Hayır)", "Bordro Tipi (Normal/Arge)",
            "Banka Adı", "IBAN", "Notlar"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // Zorunlu alanları vurgula
            if (headers[i].EndsWith("*"))
            {
                cell.Style.Fill.BackgroundColor = XLColor.LightCoral;
            }
        }

        // Örnek veri satırı
        ws.Cell(2, 1).Value = "Ahmet";
        ws.Cell(2, 2).Value = "Yılmaz";
        ws.Cell(2, 3).Value = "12345678901";
        ws.Cell(2, 4).Value = "0532 123 4567";
        ws.Cell(2, 5).Value = "ahmet@firma.com";
        ws.Cell(2, 6).Value = "İstanbul";
        ws.Cell(2, 7).Value = "Sofor";
        ws.Cell(2, 8).Value = "Operasyon";
        ws.Cell(2, 9).Value = "Şoför";
        ws.Cell(2, 10).Value = DateTime.Now.ToString("dd.MM.yyyy");
        ws.Cell(2, 11).Value = 50000;
        ws.Cell(2, 12).Value = 35000;
        ws.Cell(2, 13).Value = "Evet";
        ws.Cell(2, 14).Value = "Normal";
        ws.Cell(2, 15).Value = "Ziraat Bankası";
        ws.Cell(2, 16).Value = "TR00 0000 0000 0000 0000 0000 00";
        ws.Cell(2, 17).Value = "Örnek personel";

        // Sütun genişlikleri
        ws.Columns().AdjustToContents();

        // Açıklama sayfası
        var helpWs = workbook.Worksheets.Add("Açıklamalar");
        helpWs.Cell(1, 1).Value = "PERSONEL IMPORT ŞABLONU AÇIKLAMALARI";
        helpWs.Cell(1, 1).Style.Font.Bold = true;
        helpWs.Cell(1, 1).Style.Font.FontSize = 14;

        helpWs.Cell(3, 1).Value = "Zorunlu Alanlar:";
        helpWs.Cell(3, 1).Style.Font.Bold = true;
        helpWs.Cell(4, 1).Value = "• Ad ve Soyad zorunludur (kırmızı arka plan)";

        helpWs.Cell(6, 1).Value = "Görev Değerleri:";
        helpWs.Cell(6, 1).Style.Font.Bold = true;
        helpWs.Cell(7, 1).Value = "• Sofor, OfisCalisani, Muhasebe, Yonetici, Teknik, Diger";

        helpWs.Cell(9, 1).Value = "Tarih Formatı:";
        helpWs.Cell(9, 1).Style.Font.Bold = true;
        helpWs.Cell(10, 1).Value = "• GG.AA.YYYY (örn: 15.03.2024)";

        helpWs.Cell(12, 1).Value = "SGK/Bordro:";
        helpWs.Cell(12, 1).Style.Font.Bold = true;
        helpWs.Cell(13, 1).Value = "• SGK Bordrolu: Evet/Hayır veya 1/0";
        helpWs.Cell(14, 1).Value = "• Bordro Tipi: Normal veya Arge";

        helpWs.Cell(16, 1).Value = "Önemli Not:";
        helpWs.Cell(16, 1).Style.Font.Bold = true;
        helpWs.Cell(17, 1).Value = "• TC Kimlik No ile mevcut personel kontrolü yapılır";
        helpWs.Cell(18, 1).Value = "• Aynı TC'li personel varsa güncelleme seçeneğine göre işlem yapılır";

        helpWs.Column(1).Width = 60;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }

    /// <summary>
    /// Excel dosyasından personel import eder
    /// </summary>
    public async Task<PersonelImportSonuc> ImportFromExcelAsync(byte[] excelData, bool mevcutGuncelle = false)
    {
        var sonuc = new PersonelImportSonuc();

        using var stream = new MemoryStream(excelData);
        using var workbook = new XLWorkbook(stream);
        var ws = workbook.Worksheet(1);

        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
        sonuc.ToplamSatir = Math.Max(0, lastRow - 1); // Başlık hariç

        // Mevcut personelleri TC'ye göre indexle
        var mevcutPersoneller = await _context.Soforler
            .AsNoTracking()
            .ToDictionaryAsync(p => p.TcKimlikNo ?? $"ID_{p.Id}", p => p);

        for (int row = 2; row <= lastRow; row++)
        {
            try
            {
                var ad = ws.Cell(row, 1).GetString().Trim();
                var soyad = ws.Cell(row, 2).GetString().Trim();

                // Zorunlu alan kontrolü
                if (string.IsNullOrWhiteSpace(ad) || string.IsNullOrWhiteSpace(soyad))
                {
                    sonuc.Hatalar.Add(new PersonelImportHata
                    {
                        SatirNo = row,
                        Kolon = "Ad/Soyad",
                        Mesaj = "Ad ve Soyad zorunludur",
                        Kritik = false
                    });
                    sonuc.Atlanan++;
                    continue;
                }

                var tcKimlik = ws.Cell(row, 3).GetString().Trim();

                // Mevcut personel kontrolü (TC ile)
                if (!string.IsNullOrWhiteSpace(tcKimlik) && mevcutPersoneller.TryGetValue(tcKimlik, out var mevcut))
                {
                    if (!mevcutGuncelle)
                    {
                        sonuc.Hatalar.Add(new PersonelImportHata
                        {
                            SatirNo = row,
                            Kolon = "TC Kimlik",
                            Mesaj = $"{tcKimlik} TC'li personel zaten mevcut: {mevcut.TamAd}",
                            Kritik = false
                        });
                        sonuc.Atlanan++;
                        continue;
                    }

                    // Mevcut personeli güncelle
                    await GuncellePersonelFromRow(mevcut.Id, ws, row);
                    sonuc.BasariliGuncellenen++;
                    continue;
                }

                // Yeni personel oluştur
                var personel = await OlusturPersonelFromRow(ws, row);
                sonuc.BasariliEklenen++;
            }
            catch (Exception ex)
            {
                sonuc.Hatalar.Add(new PersonelImportHata
                {
                    SatirNo = row,
                    Kolon = "Genel",
                    Mesaj = ex.Message,
                    Kritik = false
                });
                sonuc.Atlanan++;
            }
        }

        return sonuc;
    }

    private async Task<Sofor> OlusturPersonelFromRow(IXLWorksheet ws, int row)
    {
        var gorevStr = ws.Cell(row, 7).GetString().Trim();
        var gorev = ParseGorev(gorevStr);

        var personel = new Sofor
        {
            SoforKodu = await GenerateNextKodAsync(gorev),
            Ad = ws.Cell(row, 1).GetString().Trim(),
            Soyad = ws.Cell(row, 2).GetString().Trim(),
            TcKimlikNo = ws.Cell(row, 3).GetString().Trim().NullIfEmpty(),
            Telefon = ws.Cell(row, 4).GetString().Trim().NullIfEmpty(),
            Email = ws.Cell(row, 5).GetString().Trim().NullIfEmpty(),
            Adres = ws.Cell(row, 6).GetString().Trim().NullIfEmpty(),
            Gorev = gorev,
            Departman = ws.Cell(row, 8).GetString().Trim().NullIfEmpty(),
            Pozisyon = ws.Cell(row, 9).GetString().Trim().NullIfEmpty(),
            IseBaslamaTarihi = ParseTarih(ws.Cell(row, 10)),
            BrutMaas = ParseDecimal(ws.Cell(row, 11)),
            NetMaas = ParseDecimal(ws.Cell(row, 12)),
            SGKBordroDahilMi = ParseBool(ws.Cell(row, 13)),
            BordroTipiPersonel = ParseBordroTipi(ws.Cell(row, 14)),
            BankaAdi = ws.Cell(row, 15).GetString().Trim().NullIfEmpty(),
            IBAN = ws.Cell(row, 16).GetString().Trim().NullIfEmpty(),
            Notlar = ws.Cell(row, 17).GetString().Trim().NullIfEmpty(),
            Aktif = true
        };

        // SGK'lı ise bordro tipine göre AR-GE flag'i set et
        if (personel.SGKBordroDahilMi)
        {
            personel.ArgePersoneli = personel.BordroTipiPersonel == PersonelBordroTipi.Arge;
        }

        return await CreateAsync(personel);
    }

    private async Task GuncellePersonelFromRow(int personelId, IXLWorksheet ws, int row)
    {
        var existing = await _context.Soforler.FirstOrDefaultAsync(p => p.Id == personelId);
        if (existing == null) return;

        existing.Ad = ws.Cell(row, 1).GetString().Trim();
        existing.Soyad = ws.Cell(row, 2).GetString().Trim();
        existing.Telefon = ws.Cell(row, 4).GetString().Trim().NullIfEmpty() ?? existing.Telefon;
        existing.Email = ws.Cell(row, 5).GetString().Trim().NullIfEmpty() ?? existing.Email;
        existing.Adres = ws.Cell(row, 6).GetString().Trim().NullIfEmpty() ?? existing.Adres;
        existing.Departman = ws.Cell(row, 8).GetString().Trim().NullIfEmpty() ?? existing.Departman;
        existing.Pozisyon = ws.Cell(row, 9).GetString().Trim().NullIfEmpty() ?? existing.Pozisyon;

        var yeniIseBaslama = ParseTarih(ws.Cell(row, 10));
        if (yeniIseBaslama.HasValue) existing.IseBaslamaTarihi = yeniIseBaslama;

        var brutMaas = ParseDecimal(ws.Cell(row, 11));
        if (brutMaas > 0) existing.BrutMaas = brutMaas;

        var netMaas = ParseDecimal(ws.Cell(row, 12));
        if (netMaas > 0) existing.NetMaas = netMaas;

        existing.BankaAdi = ws.Cell(row, 15).GetString().Trim().NullIfEmpty() ?? existing.BankaAdi;
        existing.IBAN = ws.Cell(row, 16).GetString().Trim().NullIfEmpty() ?? existing.IBAN;
        existing.Notlar = ws.Cell(row, 17).GetString().Trim().NullIfEmpty() ?? existing.Notlar;

        existing.UpdatedAt = DateTime.UtcNow;
        _context.Soforler.Update(existing);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Mevcut personelleri Excel'e export eder
    /// </summary>
    public async Task<byte[]> ExportToExcelAsync()
    {
        var personeller = await GetAllAsync();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Personel Listesi");

        // Başlık
        ws.Cell(1, 1).Value = "PERSONEL LİSTESİ";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Range(1, 1, 1, 15).Merge();

        ws.Cell(2, 1).Value = $"Oluşturma Tarihi: {DateTime.Now:dd.MM.yyyy HH:mm}";
        ws.Range(2, 1, 2, 15).Merge();

        // Tablo başlıkları
        var headers = new[]
        {
            "Personel Kodu", "Ad", "Soyad", "TC Kimlik", "Telefon", "Email",
            "Görev", "Departman", "Pozisyon", "İşe Başlama", "Durum",
            "Brüt Maaş", "Net Maaş", "SGK Bordrolu", "Bordro Tipi"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(4, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        // Veri satırları
        int row = 5;
        foreach (var p in personeller.OrderBy(x => x.SiralamaNo == 0 ? int.MaxValue : x.SiralamaNo).ThenBy(x => x.Ad))
        {
            ws.Cell(row, 1).Value = p.SoforKodu;
            ws.Cell(row, 2).Value = p.Ad;
            ws.Cell(row, 3).Value = p.Soyad;
            ws.Cell(row, 4).Value = p.TcKimlikNo;
            ws.Cell(row, 5).Value = p.Telefon;
            ws.Cell(row, 6).Value = p.Email;
            ws.Cell(row, 7).Value = GetGorevAdi(p.Gorev);
            ws.Cell(row, 8).Value = p.Departman;
            ws.Cell(row, 9).Value = p.Pozisyon;
            ws.Cell(row, 10).Value = p.IseBaslamaTarihi?.ToString("dd.MM.yyyy");
            ws.Cell(row, 11).Value = p.Aktif ? "Aktif" : "Pasif";
            ws.Cell(row, 12).Value = p.BrutMaas;
            ws.Cell(row, 13).Value = p.NetMaas;
            ws.Cell(row, 14).Value = p.SGKBordroDahilMi ? "Evet" : "Hayır";
            ws.Cell(row, 15).Value = GetBordroTipiAdi(p.BordroTipiPersonel);

            ws.Cell(row, 12).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 13).Style.NumberFormat.Format = "#,##0.00";

            row++;
        }

        // Özet
        row++;
        ws.Cell(row, 1).Value = "ÖZET";
        ws.Cell(row, 1).Style.Font.Bold = true;
        row++;
        ws.Cell(row, 1).Value = $"Toplam Personel: {personeller.Count}";
        row++;
        ws.Cell(row, 1).Value = $"Aktif Personel: {personeller.Count(p => p.Aktif)}";
        row++;
        ws.Cell(row, 1).Value = $"Toplam Net Maaş: {personeller.Where(p => p.Aktif).Sum(p => p.NetMaas):C0}";

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    // Helper methods
    private static PersonelGorev ParseGorev(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return PersonelGorev.Sofor;

        return value.ToLowerInvariant() switch
        {
            "sofor" or "şoför" => PersonelGorev.Sofor,
            "ofiscalisani" or "ofis çalışanı" or "ofis" => PersonelGorev.OfisCalisani,
            "muhasebe" => PersonelGorev.Muhasebe,
            "yonetici" or "yönetici" => PersonelGorev.Yonetici,
            "teknik" => PersonelGorev.Teknik,
            _ => PersonelGorev.Diger
        };
    }

    private static PersonelBordroTipi ParseBordroTipi(IXLCell cell)
    {
        var value = cell.GetString().Trim().ToLowerInvariant();
        return value switch
        {
            "arge" or "ar-ge" => PersonelBordroTipi.Arge,
            "normal" => PersonelBordroTipi.Normal,
            _ => PersonelBordroTipi.Yok
        };
    }

    private static DateTime? ParseTarih(IXLCell cell)
    {
        if (cell.TryGetValue<DateTime>(out var dt))
            return dt;

        var str = cell.GetString().Trim();
        if (DateTime.TryParseExact(str, new[] { "dd.MM.yyyy", "dd/MM/yyyy", "yyyy-MM-dd" },
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var parsed))
            return parsed;

        return null;
    }

    private static decimal ParseDecimal(IXLCell cell)
    {
        if (cell.TryGetValue<decimal>(out var d))
            return d;

        var str = cell.GetString().Trim().Replace(".", "").Replace(",", ".");
        if (decimal.TryParse(str, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var parsed))
            return parsed;

        return 0;
    }

    private static bool ParseBool(IXLCell cell)
    {
        var value = cell.GetString().Trim().ToLowerInvariant();
        return value is "evet" or "yes" or "1" or "true" or "e";
    }

    private static string GetGorevAdi(PersonelGorev gorev) => gorev switch
    {
        PersonelGorev.Sofor => "Şoför",
        PersonelGorev.OfisCalisani => "Ofis Çalışanı",
        PersonelGorev.Muhasebe => "Muhasebe",
        PersonelGorev.Yonetici => "Yönetici",
        PersonelGorev.Teknik => "Teknik",
        _ => "Diğer"
    };

    private static string GetBordroTipiAdi(PersonelBordroTipi tip) => tip switch
    {
        PersonelBordroTipi.Normal => "Normal",
        PersonelBordroTipi.Arge => "AR-GE",
        _ => "Yok"
    };

    #endregion

    #region Muhasebe Hesap Otomasyonu

    /// <summary>
    /// Personel için otomatik muhasebe hesabı oluşturur veya mevcut hesabı eşleştirir.
    /// Hesap kodu formatı: 335.XXX (Personele Borçlar alt hesabı)
    /// </summary>
    private async Task OtomatikMuhasebeHesabiOlusturAsync(Sofor sofor)
    {
        // Kullanıcı zaten bir hesap seçtiyse, oluşturma
        if (sofor.MuhasebeHesapId.HasValue)
            return;

        // Personel kodu ile eşleşen hesap var mı kontrol et
        var hesapKodu = $"335.{sofor.SoforKodu}";
        var mevcutHesap = await _muhasebeService.GetHesapByKodAsync(hesapKodu);

        if (mevcutHesap != null)
        {
            // Mevcut hesabı eşleştir
            sofor.MuhasebeHesapId = mevcutHesap.Id;
            return;
        }

        // Ana hesap 335 var mı kontrol et
        var anaHesap = await _muhasebeService.GetHesapByKodAsync("335");
        int? ustHesapId = anaHesap?.Id;

        // Yeni hesap oluştur
        var yeniHesap = new MuhasebeHesap
        {
            HesapKodu = hesapKodu,
            HesapAdi = $"{sofor.TamAd} - Personele Borçlar",
            HesapTuru = HesapTuru.Pasif,
            HesapGrubu = HesapGrubu.KisaVadeliYabanciKaynaklar,
            UstHesapId = ustHesapId,
            AltHesapVar = false,
            Aktif = true,
            SistemHesabi = false,
            Aciklama = $"Personel: {sofor.SoforKodu} - {sofor.TamAd} için otomatik oluşturuldu"
        };

        var olusturulanHesap = await _muhasebeService.CreateHesapAsync(yeniHesap);
        sofor.MuhasebeHesapId = olusturulanHesap.Id;

        // Ana hesabın AltHesapVar flag'ini güncelle
        if (anaHesap != null && !anaHesap.AltHesapVar)
        {
            anaHesap.AltHesapVar = true;
            await _muhasebeService.UpdateHesapAsync(anaHesap);
        }
    }

    /// <summary>
    /// Muhasebe hesabı olmayan tüm mevcut personellere toplu hesap oluşturur.
    /// </summary>
    public async Task<int> TopluMuhasebeHesabiOlusturAsync()
    {
        var hesapsizPersoneller = await _context.Soforler
            .Where(s => !s.IsDeleted && s.MuhasebeHesapId == null)
            .ToListAsync();

        var olusturulanSayisi = 0;
        foreach (var personel in hesapsizPersoneller)
        {
            await OtomatikMuhasebeHesabiOlusturAsync(personel);
            if (personel.MuhasebeHesapId.HasValue)
            {
                _context.Entry(personel).State = EntityState.Modified;
                olusturulanSayisi++;
            }
        }

        if (olusturulanSayisi > 0)
            await _context.SaveChangesAsync();

        return olusturulanSayisi;
    }

    /// <summary>
    /// Personelin muhasebe hesaplarını listeler (335 altındaki hesaplar)
    /// </summary>
    public async Task<List<MuhasebeHesap>> GetPersonelMuhasebeHesaplariAsync()
    {
        return await _context.MuhasebeHesaplari
            .Where(h => h.HesapKodu.StartsWith("335.") && !h.IsDeleted && h.Aktif)
            .OrderBy(h => h.HesapKodu)
            .ToListAsync();
    }

    #endregion
}

internal static class StringExtensions
{
    public static string? NullIfEmpty(this string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value;
}
