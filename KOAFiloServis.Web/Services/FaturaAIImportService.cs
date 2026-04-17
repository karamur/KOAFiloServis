using System.Text.RegularExpressions;
using System.Xml.Linq;
using KOAFiloServis.Shared.Entities;
using KOAFiloServis.Web.Data;
using KOAFiloServis.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace KOAFiloServis.Web.Services;

public class FaturaAIImportService : IFaturaAIImportService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IOllamaService _ollamaService;
    private readonly ICariService _cariService;
    private readonly ILogger<FaturaAIImportService> _logger;

    public FaturaAIImportService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        IOllamaService ollamaService,
        ICariService cariService,
        ILogger<FaturaAIImportService> logger)
    {
        _contextFactory = contextFactory;
        _ollamaService = ollamaService;
        _cariService = cariService;
        _logger = logger;
    }

    #region XML Parse

    public async Task<FaturaAIAnalizSonuc> AnalizEtXmlAsync(string xmlIcerik, string dosyaAdi)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var sonuc = new FaturaAIAnalizSonuc
        {
            DosyaAdi = dosyaAdi,
            DosyaTipi = "xml"
        };

        try
        {
            var doc = XDocument.Parse(xmlIcerik);
            var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

            // UBL e-fatura namespace'leri
            var cbc = doc.Root?.GetNamespaceOfPrefix("cbc") ?? XNamespace.None;
            var cac = doc.Root?.GetNamespaceOfPrefix("cac") ?? XNamespace.None;

            // Namespace bulunamazsa fallback dene
            if (cbc == XNamespace.None)
            {
                cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
                cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
            }

            // Fatura No
            sonuc.FaturaNo = doc.Root?.Element(cbc + "ID")?.Value?.Trim();

            // ETTN
            sonuc.EttnNo = doc.Root?.Element(cbc + "UUID")?.Value?.Trim();

            // Fatura Tarihi
            var tarihStr = doc.Root?.Element(cbc + "IssueDate")?.Value;
            if (DateTime.TryParse(tarihStr, out var faturaTarihi))
                sonuc.FaturaTarihi = faturaTarihi;

            // Fatura Tipi
            var profilId = doc.Root?.Element(cbc + "ProfileID")?.Value ?? "";
            var faturaKodu = doc.Root?.Element(cbc + "InvoiceTypeCode")?.Value ?? "";
            sonuc.EFaturaTipi = profilId.Contains("TICARIFATURA") ? EFaturaTipi.EFatura : EFaturaTipi.EArsiv;

            // Satıcı Bilgileri
            var saticiParty = doc.Root?.Element(cac + "AccountingSupplierParty")?.Element(cac + "Party");
            if (saticiParty != null)
                sonuc.SaticiBilgi = ParsePartyBilgi(saticiParty, cbc, cac);

            // Alıcı Bilgileri
            var aliciParty = doc.Root?.Element(cac + "AccountingCustomerParty")?.Element(cac + "Party");
            if (aliciParty != null)
                sonuc.AliciBilgi = ParsePartyBilgi(aliciParty, cbc, cac);

            // Fatura Kalemleri
            var invoiceLines = doc.Root?.Elements(cac + "InvoiceLine");
            if (invoiceLines != null)
            {
                int sira = 1;
                foreach (var line in invoiceLines)
                {
                    var kalem = ParseFaturaKalem(line, cbc, cac, sira++);
                    sonuc.Kalemler.Add(kalem);
                }
            }

            // Toplam Tutarlar
            var monetaryTotal = doc.Root?.Element(cac + "LegalMonetaryTotal");
            if (monetaryTotal != null)
            {
                sonuc.AraToplam = ParseDecimal(monetaryTotal.Element(cbc + "LineExtensionAmount")?.Value);
                sonuc.GenelToplam = ParseDecimal(monetaryTotal.Element(cbc + "PayablAmount")?.Value);
            }

            // KDV
            var taxTotal = doc.Root?.Element(cac + "TaxTotal");
            if (taxTotal != null)
            {
                sonuc.KdvTutar = ParseDecimal(taxTotal.Element(cbc + "TaxAmount")?.Value);
            }

            // Tevkifat kontrolü
            var withholdingTax = doc.Root?.Element(cac + "WithholdingTaxTotal");
            if (withholdingTax != null)
            {
                sonuc.TevkifatliMi = true;
                var tevkifatSubtotal = withholdingTax.Element(cac + "TaxSubtotal");
                if (tevkifatSubtotal != null)
                {
                    var percent = ParseDecimal(tevkifatSubtotal.Element(cbc + "Percent")?.Value);
                    sonuc.TevkifatOrani = percent;
                    sonuc.TevkifatKodu = tevkifatSubtotal
                        .Element(cac + "TaxCategory")?
                        .Element(cac + "TaxScheme")?
                        .Element(cbc + "TaxTypeCode")?.Value;
                }
            }

            // Vade Tarihi
            var paymentTerms = doc.Root?.Element(cac + "PaymentTerms");
            var vadeTarihStr = paymentTerms?.Element(cbc + "PaymentDueDate")?.Value;
            if (string.IsNullOrEmpty(vadeTarihStr))
            {
                // Note içinde vade arama
                var noteElements = doc.Root?.Elements(cbc + "Note");
                if (noteElements != null)
                {
                    foreach (var note in noteElements)
                    {
                        var match = Regex.Match(note.Value ?? "", @"[Vv]ade.*?(\d{2}[./]\d{2}[./]\d{4})");
                        if (match.Success && DateTime.TryParse(match.Groups[1].Value.Replace("/", "."), out var vadeParsed))
                        {
                            sonuc.VadeTarihi = vadeParsed;
                            break;
                        }
                    }
                }
            }
            else if (DateTime.TryParse(vadeTarihStr, out var vadeTarihi))
            {
                sonuc.VadeTarihi = vadeTarihi;
            }

            // Cari eşleştirme yap (satıcı = gelen fatura carisi)
            sonuc.FaturaYonu = FaturaYonu.Gelen;
            sonuc.CariEslesme = await CariEslestirAsync(sonuc.SaticiBilgi);

            // Kalemleri AI ile sınıflandır
            if (sonuc.Kalemler.Any())
            {
                var cariId = sonuc.CariEslesme.MevcutCariId;
                var cariUnvan = sonuc.CariEslesme.MevcutCariUnvan ?? sonuc.SaticiBilgi.Unvan;
                sonuc.Kalemler = await KalemleriSiniflandirAsync(sonuc.Kalemler, cariId, cariUnvan);
            }

            sonuc.AnalizBasarili = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "XML fatura analizi hatası: {DosyaAdi}", dosyaAdi);
            sonuc.AnalizBasarili = false;
            sonuc.HataMesaji = $"XML parse hatası: {ex.Message}";
        }

        return sonuc;
    }

    private FaturaAICariBilgi ParsePartyBilgi(XElement party, XNamespace cbc, XNamespace cac)
    {
        var bilgi = new FaturaAICariBilgi();

        // Unvan
        bilgi.Unvan = party.Element(cac + "PartyName")?.Element(cbc + "Name")?.Value?.Trim();

        // Vergi bilgileri
        var taxScheme = party.Element(cac + "PartyTaxScheme");
        if (taxScheme != null)
        {
            bilgi.VergiDairesi = taxScheme.Element(cac + "TaxScheme")?.Element(cbc + "Name")?.Value?.Trim();
        }

        var identification = party.Element(cac + "PartyIdentification");
        var schemeId = identification?.Element(cbc + "ID")?.Attribute("schemeID")?.Value;
        var idValue = identification?.Element(cbc + "ID")?.Value?.Trim();

        // Çoklu PartyIdentification kontrolü
        var identifications = party.Elements(cac + "PartyIdentification");
        foreach (var id in identifications)
        {
            var scheme = id.Element(cbc + "ID")?.Attribute("schemeID")?.Value;
            var val = id.Element(cbc + "ID")?.Value?.Trim();
            if (scheme == "VKN" || scheme == "TICARETSICILNO")
                bilgi.VergiNo = val;
            else if (scheme == "TCKN")
                bilgi.TcKimlikNo = val;
        }

        // Fallback
        if (string.IsNullOrEmpty(bilgi.VergiNo) && string.IsNullOrEmpty(bilgi.TcKimlikNo))
        {
            if (idValue?.Length == 11)
                bilgi.TcKimlikNo = idValue;
            else if (idValue?.Length == 10)
                bilgi.VergiNo = idValue;
        }

        // Adres
        var postalAddr = party.Element(cac + "PostalAddress");
        if (postalAddr != null)
        {
            var streetName = postalAddr.Element(cbc + "StreetName")?.Value ?? "";
            var buildingName = postalAddr.Element(cbc + "BuildingName")?.Value ?? "";
            bilgi.Adres = $"{streetName} {buildingName}".Trim();
            bilgi.Il = postalAddr.Element(cbc + "CityName")?.Value?.Trim();
            bilgi.Ilce = postalAddr.Element(cbc + "CitySubdivisionName")?.Value?.Trim();
        }

        // İletişim
        var contact = party.Element(cac + "Contact");
        if (contact != null)
        {
            bilgi.Telefon = contact.Element(cbc + "Telephone")?.Value?.Trim();
            bilgi.Email = contact.Element(cbc + "ElectronicMail")?.Value?.Trim();
        }

        return bilgi;
    }

    private FaturaAIKalem ParseFaturaKalem(XElement line, XNamespace cbc, XNamespace cac, int siraNo)
    {
        var kalem = new FaturaAIKalem
        {
            SiraNo = siraNo
        };

        // Miktar
        kalem.Miktar = ParseDecimal(line.Element(cbc + "InvoicedQuantity")?.Value);
        kalem.Birim = line.Element(cbc + "InvoicedQuantity")?.Attribute("unitCode")?.Value ?? "C62";
        kalem.Birim = NormalizeBirim(kalem.Birim);

        // Tutar
        kalem.ToplamTutar = ParseDecimal(line.Element(cbc + "LineExtensionAmount")?.Value);

        // Açıklama
        var item = line.Element(cac + "Item");
        kalem.Aciklama = item?.Element(cbc + "Name")?.Value?.Trim() ?? "";
        kalem.UrunKodu = item?.Element(cac + "SellersItemIdentification")?.Element(cbc + "ID")?.Value?.Trim();

        // Birim Fiyat
        var price = line.Element(cac + "Price");
        kalem.BirimFiyat = ParseDecimal(price?.Element(cbc + "PriceAmount")?.Value);

        // İskonto
        var allowance = line.Element(cac + "AllowanceCharge");
        if (allowance != null)
        {
            kalem.IskontoOrani = ParseDecimal(allowance.Element(cbc + "MultiplierFactorNumeric")?.Value) * 100;
            kalem.IskontoTutar = ParseDecimal(allowance.Element(cbc + "Amount")?.Value);
        }

        // KDV
        var taxTotal = line.Element(cac + "TaxTotal");
        if (taxTotal != null)
        {
            kalem.KdvTutar = ParseDecimal(taxTotal.Element(cbc + "TaxAmount")?.Value);
            var taxSubtotal = taxTotal.Element(cac + "TaxSubtotal");
            kalem.KdvOrani = ParseDecimal(taxSubtotal?.Element(cbc + "Percent")?.Value);
        }

        // Tevkifat (kalem bazında)
        var withholdingTax = line.Element(cac + "WithholdingTaxTotal");
        if (withholdingTax != null)
        {
            kalem.TevkifatTutar = ParseDecimal(withholdingTax.Element(cbc + "TaxAmount")?.Value);
            var whSubtotal = withholdingTax.Element(cac + "TaxSubtotal");
            kalem.TevkifatOrani = ParseDecimal(whSubtotal?.Element(cbc + "Percent")?.Value);
        }

        // Varsayılan olarak kullanıcı tipini AI tipiyle aynı yap
        kalem.KullaniciKalemTipi = kalem.AIKalemTipi;
        kalem.KullaniciAltTipi = kalem.AIAltTipi;

        return kalem;
    }

    #endregion

    #region PDF Parse

    public async Task<FaturaAIAnalizSonuc> AnalizEtPdfAsync(byte[] pdfIcerik, string dosyaAdi)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // PDF analiz için sadece AI kullan - XML verisi yoksa
        var sonuc = new FaturaAIAnalizSonuc
        {
            DosyaAdi = dosyaAdi,
            DosyaTipi = "pdf",
            AnalizBasarili = false,
            HataMesaji = "PDF analizi henüz desteklenmiyor. Lütfen e-fatura XML dosyası yükleyin."
        };

        return sonuc;
    }

    #endregion

    #region Cari Eşleştirme

    public async Task<CariEslesmeSonuc> CariEslestirAsync(FaturaAICariBilgi cariBilgi)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var sonuc = new CariEslesmeSonuc();

        // 1. Vergi No ile ara
        if (!string.IsNullOrEmpty(cariBilgi.VergiNo))
        {
            var cari = await context.Cariler
                .AsNoTracking()
                .FirstOrDefaultAsync(c => !c.IsDeleted && c.VergiNo == cariBilgi.VergiNo);

            if (cari != null)
            {
                sonuc.CariMevcut = true;
                sonuc.MevcutCariId = cari.Id;
                sonuc.MevcutCariUnvan = cari.Unvan;
                sonuc.MevcutCariKodu = cari.CariKodu;
                sonuc.EslesmeYontemi = "VergiNo";
                return sonuc;
            }
        }

        // 2. TC Kimlik No ile ara
        if (!string.IsNullOrEmpty(cariBilgi.TcKimlikNo))
        {
            var cari = await context.Cariler
                .AsNoTracking()
                .FirstOrDefaultAsync(c => !c.IsDeleted && c.TcKimlikNo == cariBilgi.TcKimlikNo);

            if (cari != null)
            {
                sonuc.CariMevcut = true;
                sonuc.MevcutCariId = cari.Id;
                sonuc.MevcutCariUnvan = cari.Unvan;
                sonuc.MevcutCariKodu = cari.CariKodu;
                sonuc.EslesmeYontemi = "TcKimlikNo";
                return sonuc;
            }
        }

        // 3. Unvan ile ara (benzer eşleşme)
        if (!string.IsNullOrEmpty(cariBilgi.Unvan))
        {
            var unvanLower = cariBilgi.Unvan.ToLower().Trim();
            var cariler = await context.Cariler
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .ToListAsync();

            // Tam eşleşme
            var tamEslesen = cariler.FirstOrDefault(c =>
                c.Unvan.Equals(cariBilgi.Unvan, StringComparison.OrdinalIgnoreCase));

            if (tamEslesen != null)
            {
                sonuc.CariMevcut = true;
                sonuc.MevcutCariId = tamEslesen.Id;
                sonuc.MevcutCariUnvan = tamEslesen.Unvan;
                sonuc.MevcutCariKodu = tamEslesen.CariKodu;
                sonuc.EslesmeYontemi = "Unvan (Tam)";
                return sonuc;
            }

            // Kısmi eşleşme (contains)
            var kismiEslesen = cariler.FirstOrDefault(c =>
                c.Unvan.ToLower().Contains(unvanLower) || unvanLower.Contains(c.Unvan.ToLower()));

            if (kismiEslesen != null)
            {
                sonuc.CariMevcut = true;
                sonuc.MevcutCariId = kismiEslesen.Id;
                sonuc.MevcutCariUnvan = kismiEslesen.Unvan;
                sonuc.MevcutCariKodu = kismiEslesen.CariKodu;
                sonuc.EslesmeYontemi = "Unvan (Kısmi)";
                return sonuc;
            }
        }

        // 4. Cari bulunamadı - yeni oluşturma önerisi
        sonuc.CariMevcut = false;
        sonuc.EslesmeYontemi = "Yok";
        sonuc.YeniCariOlusturulacak = true;

        var yeniKod = await _cariService.GenerateNextKodAsync();
        sonuc.YeniCariOnerisi = new Cari
        {
            CariKodu = yeniKod,
            Unvan = cariBilgi.Unvan ?? "Bilinmeyen",
            CariTipi = CariTipi.Tedarikci,
            VergiDairesi = cariBilgi.VergiDairesi,
            VergiNo = cariBilgi.VergiNo,
            TcKimlikNo = cariBilgi.TcKimlikNo,
            Adres = cariBilgi.Adres,
            Il = cariBilgi.Il,
            Ilce = cariBilgi.Ilce,
            Telefon = cariBilgi.Telefon,
            Email = cariBilgi.Email,
            Aktif = true
        };

        return sonuc;
    }

    #endregion

    #region AI Sınıflandırma

    public async Task<List<FaturaAIKalem>> KalemleriSiniflandirAsync(
        List<FaturaAIKalem> kalemler, int? cariId, string? cariUnvan)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // Mevcut güzergahları al
        var guzergahlar = await context.Guzergahlar
            .AsNoTracking()
            .Where(g => !g.IsDeleted && g.Aktif)
            .Include(g => g.Cari)
            .ToListAsync();

        // Mevcut stok kartlarını al
        var stokKartlari = await context.StokKartlari
            .AsNoTracking()
            .Where(s => !s.IsDeleted && s.Aktif)
            .ToListAsync();

        // AI ile sınıflandırma yap
        var kalemBilgileri = string.Join("\n", kalemler.Select((k, i) =>
            $"{i + 1}. Açıklama: \"{k.Aciklama}\", Ürün Kodu: \"{k.UrunKodu ?? "-"}\", Miktar: {k.Miktar}, Birim: {k.Birim}, Birim Fiyat: {k.BirimFiyat:N2} TL"));

        var guzergahBilgileri = guzergahlar.Any()
            ? string.Join("\n", guzergahlar.Select(g =>
                $"- {g.GuzergahKodu}: {g.GuzergahAdi} (Cari: {g.Cari?.Unvan ?? "-"}, Fiyat: {g.BirimFiyat:N2})"))
            : "Kayıtlı güzergah yok";

        var stokBilgileri = stokKartlari.Any()
            ? string.Join("\n", stokKartlari.Take(50).Select(s =>
                $"- {s.StokKodu}: {s.StokAdi} (Tip: {s.StokTipi}, Fiyat: {s.AlisFiyati:N2})"))
            : "Kayıtlı stok kartı yok";

        var sistemPrompt = @"Sen bir fatura analiz uzmanısın. Fatura kalemlerini sınıflandır ve eşleştir.

HER KALEM İÇİN şu bilgileri JSON formatında döndür:
{
  ""kalemler"": [
    {
      ""siraNo"": 1,
      ""kalemTipi"": ""Hizmet|Mal|Demirbas|Arac|Servis|Diger"",
      ""altTipi"": ""TasimaHizmeti|KiralamaHizmeti|DanismanlikHizmeti|TicariMal|YedekParca|SarfMalzeme|BakimOnarim|Kasko|Sigorta|Muayene|Lastik|Yakit|Diger"",
      ""guvenSkoru"": 85,
      ""guzergahAdi"": ""eşleşen güzergah adı veya önerilen yeni güzergah adı"",
      ""guzergahKodu"": ""eşleşen güzergah kodu veya null"",
      ""stokKodu"": ""eşleşen stok kodu veya null"",
      ""aciklama"": ""kısa açıklama""
    }
  ]
}

SINIFLANDIRMA KURALLARI:
- Taşıma/servis/güzergah/hat/sefer içeren açıklamalar = Hizmet > TasimaHizmeti  
- Kiralama/kira/aylık kira içeren = Hizmet > KiralamaHizmeti
- Danışmanlık/müşavirlik = Hizmet > DanismanlikHizmeti
- Yedek parça/filtre/yağ/akü/fren = Mal > YedekParca
- Kırtasiye/temizlik/ofis malzemesi = Mal > SarfMalzeme
- Ticari ürün satış/alış = Mal > TicariMal
- Bakım/onarım/tamir = Servis > BakimOnarim
- Sigorta/kasko/trafik = Servis > Kasko veya Sigorta
- Lastik = Servis > Lastik
- Yakıt/akaryakıt/mazot = Servis > Yakit

GÜZERGAH TESPİTİ:
- Taşıma hizmetlerinde güzergah adı tespit et (iki nokta arası, hat adı)
- Mevcut güzergahlarla eşleştirmeye çalış
- Eşleşme yoksa yeni güzergah adı öner

STOK TESPİTİ:
- Mal tipindeki kalemler için stok kartı eşleştir
- Ürün kodu veya benzer açıklama ile ara

Sadece JSON döndür, açıklama ekleme.";

        var prompt = $@"Cari: {cariUnvan ?? "Bilinmiyor"}

FATURA KALEMLERİ:
{kalemBilgileri}

MEVCUT GÜZERGAHLAR:
{guzergahBilgileri}

MEVCUT STOK KARTLARI:
{stokBilgileri}

Her kalemi sınıflandır ve eşleştir.";

        try
        {
            var aiYanit = await _ollamaService.AnalizYapAsync(prompt, sistemPrompt);
            var parsedKalemler = ParseAISiniflandirma(aiYanit, kalemler);

            // AI sonuçlarıyla güzergah ve stok eşleştirmelerini detaylandır
            foreach (var kalem in parsedKalemler)
            {
                // Güzergah eşleştirme detayı
                if (kalem.AIKalemTipi == FaturaKalemTipi.Hizmet &&
                    (kalem.AIAltTipi == FaturaKalemAltTipi.TasimaHizmeti || kalem.AIAltTipi == FaturaKalemAltTipi.KiralamaHizmeti))
                {
                    kalem.GuzergahEslesme = await GuzergahEslestirAsync(
                        kalem.Aciklama, cariId, kalem.BirimFiyat, kalem.Miktar);
                }

                // Stok eşleştirme detayı
                if (kalem.AIKalemTipi == FaturaKalemTipi.Mal || kalem.AIKalemTipi == FaturaKalemTipi.Servis)
                {
                    kalem.StokEslesme = await StokEslestirAsync(kalem.Aciklama, kalem.UrunKodu);
                }

                // Kullanıcı tipini AI tipiyle senkronize et
                kalem.KullaniciKalemTipi = kalem.AIKalemTipi;
                kalem.KullaniciAltTipi = kalem.AIAltTipi;
            }

            return parsedKalemler;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI kalem sınıflandırma hatası");
            // AI başarısız olursa varsayılan olarak döndür
            return kalemler;
        }
    }

    private List<FaturaAIKalem> ParseAISiniflandirma(string aiYanit, List<FaturaAIKalem> kalemler)
    {
        try
        {
            // JSON bloğunu çıkar
            var jsonMatch = Regex.Match(aiYanit, @"\{[\s\S]*\}", RegexOptions.Multiline);
            if (!jsonMatch.Success) return kalemler;

            var jsonStr = jsonMatch.Value;
            using var doc = System.Text.Json.JsonDocument.Parse(jsonStr);
            var root = doc.RootElement;

            if (!root.TryGetProperty("kalemler", out var kalemlerArr)) return kalemler;

            foreach (var aiKalem in kalemlerArr.EnumerateArray())
            {
                var siraNo = aiKalem.TryGetProperty("siraNo", out var s) ? s.GetInt32() : 0;
                var hedefKalem = kalemler.FirstOrDefault(k => k.SiraNo == siraNo);
                if (hedefKalem == null) continue;

                // Kalem Tipi
                if (aiKalem.TryGetProperty("kalemTipi", out var tipProp))
                {
                    hedefKalem.AIKalemTipi = tipProp.GetString() switch
                    {
                        "Hizmet" => FaturaKalemTipi.Hizmet,
                        "Mal" => FaturaKalemTipi.Mal,
                        "Demirbas" => FaturaKalemTipi.Demirbas,
                        "Arac" => FaturaKalemTipi.Arac,
                        "Servis" => FaturaKalemTipi.Servis,
                        _ => FaturaKalemTipi.Diger
                    };
                }

                // Alt Tipi
                if (aiKalem.TryGetProperty("altTipi", out var altTipProp))
                {
                    hedefKalem.AIAltTipi = altTipProp.GetString() switch
                    {
                        "TasimaHizmeti" => FaturaKalemAltTipi.TasimaHizmeti,
                        "KiralamaHizmeti" => FaturaKalemAltTipi.KiralamaHizmeti,
                        "DanismanlikHizmeti" => FaturaKalemAltTipi.DanismanlikHizmeti,
                        "TicariMal" => FaturaKalemAltTipi.TicariMal,
                        "YedekParca" => FaturaKalemAltTipi.YedekParca,
                        "SarfMalzeme" => FaturaKalemAltTipi.SarfMalzeme,
                        "BakimOnarim" => FaturaKalemAltTipi.BakimOnarim,
                        "Kasko" => FaturaKalemAltTipi.Kasko,
                        "Sigorta" => FaturaKalemAltTipi.Sigorta,
                        "Muayene" => FaturaKalemAltTipi.Muayene,
                        "Lastik" => FaturaKalemAltTipi.Lastik,
                        "Yakit" => FaturaKalemAltTipi.Yakit,
                        _ => FaturaKalemAltTipi.Diger
                    };
                }

                // Güven Skoru
                if (aiKalem.TryGetProperty("guvenSkoru", out var guvenProp))
                    hedefKalem.AIGuvenSkoru = guvenProp.GetInt32();

                // AI Açıklama
                if (aiKalem.TryGetProperty("aciklama", out var aciklamaProp))
                    hedefKalem.AIAciklama = aciklamaProp.GetString();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI sınıflandırma JSON parse hatası");
        }

        return kalemler;
    }

    #endregion

    #region Güzergah Eşleştirme

    public async Task<GuzergahEslesmeSonuc> GuzergahEslestirAsync(
        string kalemAciklama, int? cariId, decimal birimFiyat, decimal miktar)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var sonuc = new GuzergahEslesmeSonuc();
        var aciklamaLower = kalemAciklama.ToLower().Trim();

        // 1. Cari'ye ait güzergahlarla eşleştir
        var guzergahlar = await context.Guzergahlar
            .AsNoTracking()
            .Where(g => !g.IsDeleted && g.Aktif)
            .Include(g => g.Cari)
            .ToListAsync();

        // Benzerlik hesapla
        var benzerler = new List<BenzerGuzergah>();
        foreach (var g in guzergahlar)
        {
            var skor = HesaplaBenzerlik(aciklamaLower, g.GuzergahAdi.ToLower());

            // Aynı cari'ye aitse bonus
            if (cariId.HasValue && g.CariId == cariId.Value)
                skor = Math.Min(100, skor + 20);

            if (skor > 30)
            {
                benzerler.Add(new BenzerGuzergah
                {
                    GuzergahId = g.Id,
                    GuzergahAdi = g.GuzergahAdi,
                    GuzergahKodu = g.GuzergahKodu,
                    BirimFiyat = g.BirimFiyat,
                    CariUnvan = g.Cari?.Unvan,
                    BenzerlikSkoru = skor
                });
            }
        }

        benzerler = benzerler.OrderByDescending(b => b.BenzerlikSkoru).Take(5).ToList();
        sonuc.BenzerGuzergahlar = benzerler;

        // En iyi eşleşme %70 üzerindeyse otomatik eşle
        var enIyi = benzerler.FirstOrDefault();
        if (enIyi != null && enIyi.BenzerlikSkoru >= 70)
        {
            sonuc.GuzergahMevcut = true;
            sonuc.MevcutGuzergahId = enIyi.GuzergahId;
            sonuc.MevcutGuzergahAdi = enIyi.GuzergahAdi;
            sonuc.MevcutGuzergahKodu = enIyi.GuzergahKodu;
            sonuc.MevcutBirimFiyat = enIyi.BirimFiyat;
        }
        else
        {
            sonuc.GuzergahMevcut = false;
            sonuc.YeniGuzergahOlusturulacak = true;
            sonuc.OnerilenGuzergahAdi = kalemAciklama.Length > 100 ? kalemAciklama[..100] : kalemAciklama;

            // Başlangıç-bitiş noktası tespit
            var noktalar = TespitEtGuzergahNoktalari(kalemAciklama);
            sonuc.OnerilenBaslangic = noktalar.baslangic;
            sonuc.OnerilenBitis = noktalar.bitis;
        }

        return sonuc;
    }

    private (string? baslangic, string? bitis) TespitEtGuzergahNoktalari(string aciklama)
    {
        // "X - Y", "X → Y", "X > Y" formatlarını dene
        var patterns = new[]
        {
            @"(.+?)\s*[-–—→>]\s*(.+)",
            @"(.+?)\s+[Gg]üzergah[ıi]?\s*[-–—:]\s*(.+)",
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(aciklama, pattern);
            if (match.Success)
            {
                return (match.Groups[1].Value.Trim(), match.Groups[2].Value.Trim());
            }
        }

        return (null, null);
    }

    #endregion

    #region Stok Eşleştirme

    public async Task<StokEslesmeSonuc> StokEslestirAsync(string kalemAciklama, string? urunKodu)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var sonuc = new StokEslesmeSonuc();

        var stokKartlari = await context.StokKartlari
            .AsNoTracking()
            .Where(s => !s.IsDeleted && s.Aktif)
            .ToListAsync();

        // 1. Ürün kodu ile tam eşleşme
        if (!string.IsNullOrEmpty(urunKodu))
        {
            var tamEslesen = stokKartlari.FirstOrDefault(s =>
                s.StokKodu.Equals(urunKodu, StringComparison.OrdinalIgnoreCase));

            if (tamEslesen != null)
            {
                sonuc.StokMevcut = true;
                sonuc.MevcutStokId = tamEslesen.Id;
                sonuc.MevcutStokKodu = tamEslesen.StokKodu;
                sonuc.MevcutStokAdi = tamEslesen.StokAdi;
                sonuc.MevcutFiyat = tamEslesen.AlisFiyati;
                return sonuc;
            }
        }

        // 2. Açıklama benzerliği
        var aciklamaLower = kalemAciklama.ToLower().Trim();
        var benzerler = new List<BenzerStok>();

        foreach (var s in stokKartlari)
        {
            var skor = HesaplaBenzerlik(aciklamaLower, s.StokAdi.ToLower());
            if (skor > 30)
            {
                benzerler.Add(new BenzerStok
                {
                    StokId = s.Id,
                    StokKodu = s.StokKodu,
                    StokAdi = s.StokAdi,
                    Fiyat = s.AlisFiyati,
                    Birim = s.Birim,
                    BenzerlikSkoru = skor
                });
            }
        }

        benzerler = benzerler.OrderByDescending(b => b.BenzerlikSkoru).Take(5).ToList();
        sonuc.BenzerStoklar = benzerler;

        var enIyi = benzerler.FirstOrDefault();
        if (enIyi != null && enIyi.BenzerlikSkoru >= 70)
        {
            sonuc.StokMevcut = true;
            sonuc.MevcutStokId = enIyi.StokId;
            sonuc.MevcutStokKodu = enIyi.StokKodu;
            sonuc.MevcutStokAdi = enIyi.StokAdi;
            sonuc.MevcutFiyat = enIyi.Fiyat;
        }

        return sonuc;
    }

    #endregion

    #region Kaydet

    public async Task<FaturaAIKaydetSonuc> KaydetAsync(FaturaAIAnalizSonuc sonuc, int? firmaId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var kaydetSonuc = new FaturaAIKaydetSonuc();

        // ExecutionStrategy ile transaction sarmalama (NpgsqlRetryingExecutionStrategy uyumluluğu)
        var strategy = context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                // 1. Cari oluştur (gerekiyorsa)
                int cariId;
            if (sonuc.CariEslesme.CariMevcut && sonuc.CariEslesme.MevcutCariId.HasValue)
            {
                cariId = sonuc.CariEslesme.MevcutCariId.Value;
            }
            else if (sonuc.CariEslesme.YeniCariOlusturulacak && sonuc.CariEslesme.YeniCariOnerisi != null)
            {
                var yeniCari = sonuc.CariEslesme.YeniCariOnerisi;
                yeniCari.FirmaId = firmaId;
                var olusan = await _cariService.CreateAsync(yeniCari);
                cariId = olusan.Id;
                kaydetSonuc.CariId = cariId;
                kaydetSonuc.Uyarilar.Add($"Yeni cari oluşturuldu: {olusan.Unvan} ({olusan.CariKodu})");
            }
            else
            {
                kaydetSonuc.Basarili = false;
                kaydetSonuc.Mesaj = "Cari bilgisi belirlenemedi.";
                return kaydetSonuc;
            }

            // 2. Güzergahları oluştur (gerekiyorsa)
            foreach (var kalem in sonuc.Kalemler)
            {
                if (kalem.GuzergahEslesme != null && kalem.GuzergahEslesme.YeniGuzergahOlusturulacak && !kalem.GuzergahEslesme.GuzergahMevcut)
                {
                    var guzergah = new Guzergah
                    {
                        GuzergahKodu = await GenerateGuzergahKoduAsync(context),
                        GuzergahAdi = kalem.GuzergahEslesme.OnerilenGuzergahAdi ?? kalem.Aciklama,
                        BaslangicNoktasi = kalem.GuzergahEslesme.OnerilenBaslangic,
                        BitisNoktasi = kalem.GuzergahEslesme.OnerilenBitis,
                        BirimFiyat = kalem.BirimFiyat,
                        CariId = cariId,
                        FirmaId = firmaId,
                        Aktif = true,
                        SeferTipi = SeferTipi.SabahAksam
                    };
                    context.Guzergahlar.Add(guzergah);
                    await context.SaveChangesAsync();

                    kalem.GuzergahEslesme.MevcutGuzergahId = guzergah.Id;
                    kalem.GuzergahEslesme.GuzergahMevcut = true;
                    kaydetSonuc.OlusturulanGuzergahIdler.Add(guzergah.Id);
                    kaydetSonuc.Uyarilar.Add($"Yeni güzergah oluşturuldu: {guzergah.GuzergahAdi}");
                }
            }

            // 3. Faturayı oluştur
            var fatura = new Fatura
            {
                FaturaNo = sonuc.FaturaNo ?? "",
                FaturaTarihi = sonuc.FaturaTarihi ?? DateTime.Today,
                VadeTarihi = sonuc.VadeTarihi,
                FaturaTipi = FaturaTipi.AlisFaturasi,
                FaturaYonu = sonuc.FaturaYonu,
                EFaturaTipi = sonuc.EFaturaTipi,
                EttnNo = sonuc.EttnNo,
                ImportKaynak = $"AI-{sonuc.DosyaTipi?.ToUpper()}",
                CariId = cariId,
                FirmaId = firmaId,
                AraToplam = sonuc.AraToplam,
                KdvTutar = sonuc.KdvTutar,
                GenelToplam = sonuc.GenelToplam,
                IskontoTutar = sonuc.IskontoTutar,
                TevkifatliMi = sonuc.TevkifatliMi,
                TevkifatOrani = sonuc.TevkifatOrani,
                TevkifatKodu = sonuc.TevkifatKodu,
                Durum = FaturaDurum.Beklemede
            };

            // Fatura kalemlerini oluştur
            foreach (var aiKalem in sonuc.Kalemler)
            {
                var faturaKalem = new FaturaKalem
                {
                    SiraNo = aiKalem.SiraNo,
                    UrunKodu = aiKalem.UrunKodu,
                    Aciklama = aiKalem.Aciklama,
                    Miktar = aiKalem.Miktar,
                    Birim = aiKalem.Birim,
                    BirimFiyat = aiKalem.BirimFiyat,
                    IskontoOrani = aiKalem.IskontoOrani,
                    IskontoTutar = aiKalem.IskontoTutar,
                    KdvOrani = aiKalem.KdvOrani,
                    KdvTutar = aiKalem.KdvTutar,
                    ToplamTutar = aiKalem.ToplamTutar,
                    KalemTipi = aiKalem.KullaniciKalemTipi,
                    AltTipi = aiKalem.KullaniciAltTipi,
                    TevkifatOrani = aiKalem.TevkifatOrani,
                    TevkifatTutar = aiKalem.TevkifatTutar
                };

                fatura.FaturaKalemleri.Add(faturaKalem);
            }

            context.Faturalar.Add(fatura);
            await context.SaveChangesAsync();

            // 4. Güzergahların FaturaKalemId'sini güncelle
            var faturaKalemler = fatura.FaturaKalemleri.OrderBy(k => k.SiraNo).ToList();
            foreach (var aiKalem in sonuc.Kalemler)
            {
                if (aiKalem.GuzergahEslesme?.MevcutGuzergahId != null)
                {
                    var guzergah = await context.Guzergahlar.FindAsync(aiKalem.GuzergahEslesme.MevcutGuzergahId);
                    if (guzergah != null)
                    {
                        var fk = faturaKalemler.FirstOrDefault(k => k.SiraNo == aiKalem.SiraNo);
                        if (fk != null)
                        {
                            guzergah.FaturaKalemId = fk.Id;
                            // Birim fiyat güncelle (faturadaki fiyat güncel)
                            if (aiKalem.BirimFiyat > 0)
                                guzergah.BirimFiyat = aiKalem.BirimFiyat;
                            context.Guzergahlar.Update(guzergah);
                        }
                    }
                }
            }
            await context.SaveChangesAsync();

            await transaction.CommitAsync();

            kaydetSonuc.Basarili = true;
            kaydetSonuc.FaturaId = fatura.Id;
            kaydetSonuc.CariId = cariId;
            kaydetSonuc.Mesaj = $"Fatura başarıyla kaydedildi. (No: {fatura.FaturaNo})";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Fatura AI kaydetme hatası");
            kaydetSonuc.Basarili = false;
            kaydetSonuc.Mesaj = $"Kaydetme hatası: {ex.Message}";
        }

        return kaydetSonuc;
        }); // ExecutionStrategy lambda sonu
    }

    #endregion

    #region Yardımcı Metotlar

    private static decimal ParseDecimal(string? value)
    {
        if (string.IsNullOrEmpty(value)) return 0;
        value = value.Replace(",", ".").Trim();
        return decimal.TryParse(value, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : 0;
    }

    private static string NormalizeBirim(string birimKodu)
    {
        return birimKodu.ToUpper() switch
        {
            "C62" or "NIU" => "Adet",
            "KGM" => "Kg",
            "LTR" => "Lt",
            "MTR" => "Mt",
            "MTK" => "m²",
            "MTQ" => "m³",
            "TNE" => "Ton",
            "HUR" => "Saat",
            "DAY" => "Gün",
            "MON" => "Ay",
            "ANN" => "Yıl",
            "KMT" => "Km",
            "SET" => "Takım",
            "PR" => "Çift",
            "BX" => "Kutu",
            "PK" => "Paket",
            _ => birimKodu
        };
    }

    private static int HesaplaBenzerlik(string str1, string str2)
    {
        if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2)) return 0;

        // Kelime bazlı benzerlik
        var words1 = str1.Split([' ', '-', '/', '\\', '.', ','], StringSplitOptions.RemoveEmptyEntries);
        var words2 = str2.Split([' ', '-', '/', '\\', '.', ','], StringSplitOptions.RemoveEmptyEntries);

        if (words1.Length == 0 || words2.Length == 0) return 0;

        int ortakKelime = 0;
        foreach (var w1 in words1)
        {
            if (w1.Length < 2) continue;
            foreach (var w2 in words2)
            {
                if (w2.Length < 2) continue;
                if (w1.Equals(w2, StringComparison.OrdinalIgnoreCase) ||
                    w1.Contains(w2, StringComparison.OrdinalIgnoreCase) ||
                    w2.Contains(w1, StringComparison.OrdinalIgnoreCase))
                {
                    ortakKelime++;
                    break;
                }
            }
        }

        var maxKelime = Math.Max(words1.Length, words2.Length);
        return maxKelime > 0 ? (int)((double)ortakKelime / maxKelime * 100) : 0;
    }

    private async Task<string> GenerateGuzergahKoduAsync(ApplicationDbContext context)
    {
        var sonKod = await context.Guzergahlar
            .AsNoTracking()
            .Where(g => g.GuzergahKodu.StartsWith("GZR"))
            .OrderByDescending(g => g.GuzergahKodu)
            .Select(g => g.GuzergahKodu)
            .FirstOrDefaultAsync();

        if (sonKod != null && int.TryParse(sonKod.Replace("GZR", ""), out var no))
            return $"GZR{(no + 1):D4}";

        return "GZR0001";
    }

    #endregion
}
