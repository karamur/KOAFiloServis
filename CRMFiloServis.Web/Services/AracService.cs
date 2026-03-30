using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class AracService : IAracService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public AracService(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    #region Araç CRUD İşlemleri

    public async Task<List<Arac>> GetAllAsync()
    {
        var araclar = await _context.Araclar
            .Include(a => a.PlakaGecmisi.Where(p => !p.IsDeleted))
            .Where(a => !a.IsDeleted)
            .ToListAsync();
            
        // Aktif plakaları güncelle (CikisTarihi null veya gelecek tarihli olanlar)
        foreach (var arac in araclar)
        {
            var aktifPlaka = arac.PlakaGecmisi
                .Where(p => p.CikisTarihi == null || p.CikisTarihi > DateTime.Today)
                .OrderByDescending(p => p.GirisTarihi)
                .FirstOrDefault();
            
            if (aktifPlaka != null && arac.AktifPlaka != aktifPlaka.Plaka)
            {
                arac.AktifPlaka = aktifPlaka.Plaka;
            }
        }
        
        return araclar.OrderBy(a => a.AktifPlaka ?? a.SaseNo).ToList();
    }

    public async Task<List<Arac>> GetActiveAsync()
    {
        var araclar = await _context.Araclar
            .Include(a => a.PlakaGecmisi.Where(p => !p.IsDeleted))
            .Where(a => a.Aktif && !a.IsDeleted)
            .ToListAsync();
            
        // Aktif plakaları güncelle
        foreach (var arac in araclar)
        {
            var aktifPlaka = arac.PlakaGecmisi
                .Where(p => p.CikisTarihi == null || p.CikisTarihi > DateTime.Today)
                .OrderByDescending(p => p.GirisTarihi)
                .FirstOrDefault();
            
            if (aktifPlaka != null && arac.AktifPlaka != aktifPlaka.Plaka)
            {
                arac.AktifPlaka = aktifPlaka.Plaka;
            }
        }
        
        return araclar.OrderBy(a => a.AktifPlaka ?? a.SaseNo).ToList();
    }

    public async Task<int> GetActiveCountAsync()
    {
        return await _context.Araclar
            .Where(a => a.Aktif && !a.IsDeleted)
            .CountAsync();
    }

    public async Task<Arac?> GetByIdAsync(int id)
    {
        var arac = await _context.Araclar
            .Include(a => a.PlakaGecmisi.Where(p => !p.IsDeleted).OrderByDescending(p => p.GirisTarihi))
            .Include(a => a.KiralikCari)
            .Include(a => a.KomisyoncuCari)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
            
        if (arac != null)
        {
            // Aktif plakayı güncelle
            var aktifPlaka = arac.PlakaGecmisi
                .Where(p => p.CikisTarihi == null || p.CikisTarihi > DateTime.Today)
                .OrderByDescending(p => p.GirisTarihi)
                .FirstOrDefault();
            
            if (aktifPlaka != null)
            {
                arac.AktifPlaka = aktifPlaka.Plaka;
            }
        }
        
        return arac;
    }

    public async Task<Arac?> GetByPlakaAsync(string plaka)
    {
        // Aktif plakaya göre bul (CikisTarihi null veya gelecek tarihli)
        var aracPlaka = await _context.AracPlakalar
            .Include(ap => ap.Arac)
            .FirstOrDefaultAsync(ap => ap.Plaka == plaka && 
                                       !ap.IsDeleted &&
                                       (ap.CikisTarihi == null || ap.CikisTarihi > DateTime.Today));
            
        return aracPlaka?.Arac;
    }
    
    public async Task<Arac?> GetBySaseNoAsync(string saseNo)
    {
        return await _context.Araclar
            .Include(a => a.PlakaGecmisi.Where(p => !p.IsDeleted))
            .FirstOrDefaultAsync(a => a.SaseNo == saseNo && !a.IsDeleted);
    }
    
    public async Task<bool> SaseNoMevcutMu(string saseNo, int? haricAracId = null)
    {
        return await _context.Araclar
            .AnyAsync(a => a.SaseNo == saseNo && 
                          !a.IsDeleted &&
                          (!haricAracId.HasValue || a.Id != haricAracId.Value));
    }
    
    public async Task<bool> PlakaMevcutMu(string plaka, int? haricAracPlakaId = null)
    {
        // Aktif plaka kontrolü (CikisTarihi null veya gelecek tarihli)
        return await _context.AracPlakalar
            .AnyAsync(ap => ap.Plaka == plaka && 
                           !ap.IsDeleted &&
                           (ap.CikisTarihi == null || ap.CikisTarihi > DateTime.Today) && 
                           (!haricAracPlakaId.HasValue || ap.Id != haricAracPlakaId.Value));
    }

    public async Task<Arac> CreateAsync(Arac arac, string plaka, PlakaIslemTipi islemTipi = PlakaIslemTipi.Alis, 
        decimal? islemTutari = null, int? cariId = null, string? aciklama = null)
    {
        // Şase no kontrolü
        if (await SaseNoMevcutMu(arac.SaseNo))
            throw new InvalidOperationException($"Bu şase numarası ({arac.SaseNo}) sistemde zaten kayıtlı.");
            
        // Plaka kontrolü
        if (await PlakaMevcutMu(plaka))
            throw new InvalidOperationException($"Bu plaka ({plaka}) başka bir araçta aktif olarak kullanılıyor.");
        
        // Araç oluştur
        arac.AktifPlaka = plaka;
        arac.CreatedAt = DateTime.UtcNow;
        _context.Araclar.Add(arac);
        await _context.SaveChangesAsync();
        
        // İlk plaka kaydını oluştur
        var aracPlaka = new AracPlaka
        {
            AracId = arac.Id,
            Plaka = plaka,
            GirisTarihi = DateTime.UtcNow,
            IslemTipi = islemTipi,
            IslemTutari = islemTutari,
            CariId = cariId,
            Aciklama = aciklama ?? $"Araç ilk kayıt - {islemTipi}",
            CreatedAt = DateTime.UtcNow
        };
        _context.AracPlakalar.Add(aracPlaka);
        await _context.SaveChangesAsync();
        
        return arac;
    }

    public async Task<Arac> UpdateAsync(Arac arac)
    {
        // Şase no kontrolü (kendi hariç)
        if (await SaseNoMevcutMu(arac.SaseNo, arac.Id))
            throw new InvalidOperationException($"Bu şase numarası ({arac.SaseNo}) sistemde zaten kayıtlı.");
            
        arac.UpdatedAt = DateTime.UtcNow;
        _context.Araclar.Update(arac);
        await _context.SaveChangesAsync();
        
        // Aktif plakayı güncelle
        await GuncelleAktifPlaka(arac.Id);
        
        return arac;
    }

    public async Task DeleteAsync(int id)
    {
        var arac = await _context.Araclar.FindAsync(id);
        if (arac != null)
        {
            arac.IsDeleted = true;
            arac.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
    
    #endregion
    
    #region Plaka İşlemleri
    
    public async Task<List<AracPlaka>> GetPlakaGecmisiAsync(int aracId)
    {
        return await _context.AracPlakalar
            .Include(ap => ap.Cari)
            .Where(ap => ap.AracId == aracId)
            .OrderByDescending(ap => ap.GirisTarihi)
            .ToListAsync();
    }
    
    public async Task<AracPlaka> PlakaEkle(int aracId, string yeniPlaka, PlakaIslemTipi islemTipi, 
        decimal? islemTutari = null, int? cariId = null, string? aciklama = null)
    {
        // Plaka kontrolü
        if (await PlakaMevcutMu(yeniPlaka))
            throw new InvalidOperationException($"Bu plaka ({yeniPlaka}) başka bir araçta aktif olarak kullanılıyor.");
        
        // Mevcut aktif plakayı kapat
        var mevcutAktif = await _context.AracPlakalar
            .FirstOrDefaultAsync(ap => ap.AracId == aracId && ap.CikisTarihi == null);
            
        if (mevcutAktif != null)
        {
            mevcutAktif.CikisTarihi = DateTime.UtcNow;
            mevcutAktif.UpdatedAt = DateTime.UtcNow;
        }
        
        // Yeni plaka ekle
        var yeniPlakaKaydi = new AracPlaka
        {
            AracId = aracId,
            Plaka = yeniPlaka,
            GirisTarihi = DateTime.UtcNow,
            IslemTipi = islemTipi,
            IslemTutari = islemTutari,
            CariId = cariId,
            Aciklama = aciklama,
            CreatedAt = DateTime.UtcNow
        };
        _context.AracPlakalar.Add(yeniPlakaKaydi);
        
        // Araçtaki aktif plakayı güncelle
        var arac = await _context.Araclar.FindAsync(aracId);
        if (arac != null)
        {
            arac.AktifPlaka = yeniPlaka;
            arac.UpdatedAt = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync();
        return yeniPlakaKaydi;
    }
    
    public async Task PlakaCikis(int aracPlakaId, PlakaIslemTipi cikisIslemTipi, 
        decimal? islemTutari = null, int? cariId = null, string? aciklama = null)
    {
        var plakaKaydi = await _context.AracPlakalar
            .Include(ap => ap.Arac)
            .FirstOrDefaultAsync(ap => ap.Id == aracPlakaId);
            
        if (plakaKaydi == null)
            throw new InvalidOperationException("Plaka kaydı bulunamadı.");
            
        if (plakaKaydi.CikisTarihi.HasValue)
            throw new InvalidOperationException("Bu plaka zaten kapatılmış.");
        
        plakaKaydi.CikisTarihi = DateTime.UtcNow;
        plakaKaydi.IslemTipi = cikisIslemTipi;
        if (islemTutari.HasValue) plakaKaydi.IslemTutari = islemTutari;
        if (cariId.HasValue) plakaKaydi.CariId = cariId;
        if (!string.IsNullOrEmpty(aciklama)) plakaKaydi.Aciklama = aciklama;
        plakaKaydi.UpdatedAt = DateTime.UtcNow;
        
        // Araçtaki aktif plakayı temizle
        if (plakaKaydi.Arac != null)
        {
            plakaKaydi.Arac.AktifPlaka = null;
            plakaKaydi.Arac.UpdatedAt = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync();
    }
    
    private async Task GuncelleAktifPlaka(int aracId)
    {
        var arac = await _context.Araclar.FindAsync(aracId);
        if (arac == null) return;
        
        // CikisTarihi null olan veya CikisTarihi bugünden sonra olan plakalardan en son eklenen
        var aktifPlaka = await _context.AracPlakalar
            .Where(ap => ap.AracId == aracId && 
                        !ap.IsDeleted &&
                        (ap.CikisTarihi == null || ap.CikisTarihi > DateTime.Today))
            .OrderByDescending(ap => ap.GirisTarihi)
            .FirstOrDefaultAsync();
            
        arac.AktifPlaka = aktifPlaka?.Plaka;
        await _context.SaveChangesAsync();
    }
    
    #endregion
    
    #region Satışa Açık Araçlar
    
    public async Task<List<Arac>> GetSatisaAcikAraclarAsync()
    {
        return await _context.Araclar
            .Include(a => a.PlakaGecmisi.Where(p => !p.IsDeleted))
            .Where(a => a.SatisaAcik && a.Aktif)
            .OrderBy(a => a.SatisaAcilmaTarihi)
            .ToListAsync();
    }
    
    public async Task AracSatisaAc(int aracId, decimal satisFiyati, string? aciklama = null)
    {
        var arac = await _context.Araclar.FindAsync(aracId);
        if (arac == null)
            throw new InvalidOperationException("Araç bulunamadı.");
            
        arac.SatisaAcik = true;
        arac.SatisFiyati = satisFiyati;
        arac.SatisaAcilmaTarihi = DateTime.UtcNow;
        arac.SatisAciklamasi = aciklama;
        arac.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
    }
    
    public async Task AracSatisKapat(int aracId)
    {
        var arac = await _context.Araclar.FindAsync(aracId);
        if (arac == null)
            throw new InvalidOperationException("Araç bulunamadı.");
            
        arac.SatisaAcik = false;
        arac.SatisFiyati = null;
        arac.SatisaAcilmaTarihi = null;
        arac.SatisAciklamasi = null;
        arac.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
    }
    
    #endregion

    #region Arac Evrak Islemleri

    public async Task<List<AracEvrak>> GetAracEvraklariAsync(int aracId)
    {
        return await _context.AracEvraklari
            .Include(e => e.Dosyalar)
            .Where(e => e.AracId == aracId)
            .OrderBy(e => e.EvrakKategorisi)
            .ThenByDescending(e => e.BitisTarihi)
            .ToListAsync();
    }

    public async Task<AracEvrak?> GetAracEvrakByIdAsync(int evrakId)
    {
        return await _context.AracEvraklari
            .Include(e => e.Dosyalar)
            .FirstOrDefaultAsync(e => e.Id == evrakId);
    }

    public async Task<AracEvrak> CreateAracEvrakAsync(AracEvrak evrak)
    {
        if (evrak.BaslangicTarihi.HasValue)
            evrak.BaslangicTarihi = DateTime.SpecifyKind(evrak.BaslangicTarihi.Value, DateTimeKind.Utc);
        if (evrak.BitisTarihi.HasValue)
            evrak.BitisTarihi = DateTime.SpecifyKind(evrak.BitisTarihi.Value, DateTimeKind.Utc);
        
        evrak.CreatedAt = DateTime.UtcNow;
        _context.AracEvraklari.Add(evrak);
        await _context.SaveChangesAsync();
        return evrak;
    }

    public async Task<AracEvrak> UpdateAracEvrakAsync(AracEvrak evrak)
    {
        if (evrak.BaslangicTarihi.HasValue)
            evrak.BaslangicTarihi = DateTime.SpecifyKind(evrak.BaslangicTarihi.Value, DateTimeKind.Utc);
        if (evrak.BitisTarihi.HasValue)
            evrak.BitisTarihi = DateTime.SpecifyKind(evrak.BitisTarihi.Value, DateTimeKind.Utc);
        
        evrak.UpdatedAt = DateTime.UtcNow;
        _context.AracEvraklari.Update(evrak);
        await _context.SaveChangesAsync();
        return evrak;
    }

    public async Task DeleteAracEvrakAsync(int evrakId)
    {
        var evrak = await _context.AracEvraklari
            .Include(e => e.Dosyalar)
            .FirstOrDefaultAsync(e => e.Id == evrakId);
            
        if (evrak != null)
        {
            // Dosyaları sil
            foreach (var dosya in evrak.Dosyalar)
            {
                var dosyaYolu = Path.Combine(_env.ContentRootPath, "wwwroot", dosya.DosyaYolu);
                if (File.Exists(dosyaYolu))
                    File.Delete(dosyaYolu);
            }
            
            evrak.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<AracEvrakDosya> UploadEvrakDosyaAsync(int evrakId, IBrowserFile file)
    {
        var evrak = await _context.AracEvraklari.FindAsync(evrakId);
        if (evrak == null)
            throw new Exception("Evrak bulunamadi");

        var klasorYolu = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads", "evraklar", evrakId.ToString());
        if (!Directory.Exists(klasorYolu))
            Directory.CreateDirectory(klasorYolu);

        var dosyaAdi = $"{Guid.NewGuid()}{Path.GetExtension(file.Name)}";
        var dosyaYolu = Path.Combine(klasorYolu, dosyaAdi);

        await using var stream = new FileStream(dosyaYolu, FileMode.Create);
        await file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024).CopyToAsync(stream);

        var evrakDosya = new AracEvrakDosya
        {
            AracEvrakId = evrakId,
            DosyaAdi = file.Name,
            DosyaYolu = $"uploads/evraklar/{evrakId}/{dosyaAdi}",
            DosyaTipi = Path.GetExtension(file.Name).TrimStart('.').ToLower(),
            DosyaBoyutu = file.Size,
            CreatedAt = DateTime.UtcNow
        };

        _context.AracEvrakDosyalari.Add(evrakDosya);
        await _context.SaveChangesAsync();
        return evrakDosya;
    }

    public async Task<byte[]> GetEvrakDosyaAsync(int dosyaId)
    {
        var dosya = await _context.AracEvrakDosyalari.FindAsync(dosyaId);
        if (dosya == null)
            throw new Exception("Dosya bulunamadi");

        var dosyaYolu = Path.Combine(_env.ContentRootPath, "wwwroot", dosya.DosyaYolu);
        if (!File.Exists(dosyaYolu))
            throw new Exception("Dosya diskte bulunamadi");

        return await File.ReadAllBytesAsync(dosyaYolu);
    }

    public async Task DeleteEvrakDosyaAsync(int dosyaId)
    {
        var dosya = await _context.AracEvrakDosyalari.FindAsync(dosyaId);
        if (dosya != null)
        {
            var dosyaYolu = Path.Combine(_env.ContentRootPath, "wwwroot", dosya.DosyaYolu);
            if (File.Exists(dosyaYolu))
                File.Delete(dosyaYolu);

            _context.AracEvrakDosyalari.Remove(dosya);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Evrak Uyarilari

    public async Task<List<AracEvrak>> GetSuresiDolacakEvraklarAsync(int gunSayisi = 30)
    {
        var bugun = DateTime.UtcNow.Date;
        var bitisTarihi = bugun.AddDays(gunSayisi);

        return await _context.AracEvraklari
            .Include(e => e.Arac)
            .Where(e => e.Durum == EvrakDurum.Aktif && 
                        e.BitisTarihi.HasValue && 
                        e.BitisTarihi.Value <= bitisTarihi)
            .OrderBy(e => e.BitisTarihi)
            .ToListAsync();
    }

    #endregion

    #region Excel Import/Export

    public async Task<byte[]> GetExcelSablonAsync()
    {
        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var ws = workbook.Worksheets.Add("Araclar");
        
        // Başlıklar
        var headers = new[] { "Şase No *", "Plaka", "Marka", "Model", "Model Yılı", "Motor No", "Renk", "Koltuk Sayısı", "Araç Tipi", "KM" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGreen;
        }
        
        // Örnek veriler
        ws.Cell(2, 1).Value = "WVWZZZ3CZWE123456";
        ws.Cell(2, 2).Value = "34ABC123";
        ws.Cell(2, 3).Value = "VOLKSWAGEN";
        ws.Cell(2, 4).Value = "CARAVELLE";
        ws.Cell(2, 5).Value = 2023;
        ws.Cell(2, 6).Value = "DFG123456";
        ws.Cell(2, 7).Value = "BEYAZ";
        ws.Cell(2, 8).Value = 9;
        ws.Cell(2, 9).Value = "Minibüs";
        ws.Cell(2, 10).Value = 15000;
        
        // Açıklamalar
        ws.Cell(5, 1).Value = "AÇIKLAMALAR:";
        ws.Cell(5, 1).Style.Font.Bold = true;
        ws.Cell(6, 1).Value = "* Şase No: Zorunlu, benzersiz olmalı (17 karakter)";
        ws.Cell(7, 1).Value = "* Araç Tipi: Minibüs, Midibüs, Otobüs, Otomobil, Panelvan";
        ws.Cell(8, 1).Value = "* Model Yılı: 4 haneli (örn: 2023)";
        ws.Cell(9, 1).Value = "* Plaka: Opsiyonel, varsa araç bu plaka ile kaydedilir";
        
        ws.Columns().AdjustToContents();
        
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<AracImportResult> ImportFromExcelAsync(byte[] fileContent)
    {
        var result = new AracImportResult();
        
        try
        {
            using var stream = new MemoryStream(fileContent);
            using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
            var ws = workbook.Worksheets.First();
            
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            var mevcutSaseNolar = await _context.Araclar.Where(a => !a.IsDeleted).Select(a => a.SaseNo.ToUpper()).ToListAsync();
            
            for (int row = 2; row <= lastRow; row++)
            {
                try
                {
                    var saseNo = ws.Cell(row, 1).GetString()?.Trim().ToUpper();
                    
                    if (string.IsNullOrWhiteSpace(saseNo))
                        continue;
                    
                    var plaka = ws.Cell(row, 2).GetString()?.Trim().ToUpper();
                    var marka = ws.Cell(row, 3).GetString()?.Trim();
                    var model = ws.Cell(row, 4).GetString()?.Trim();
                    var modelYiliStr = ws.Cell(row, 5).GetString()?.Trim();
                    var motorNo = ws.Cell(row, 6).GetString()?.Trim();
                    var renk = ws.Cell(row, 7).GetString()?.Trim();
                    var koltukSayisiStr = ws.Cell(row, 8).GetString()?.Trim();
                    var aracTipiStr = ws.Cell(row, 9).GetString()?.Trim();
                    var kmStr = ws.Cell(row, 10).GetString()?.Trim();
                    
                    int? modelYili = null;
                    if (int.TryParse(modelYiliStr, out var y)) modelYili = y;
                    
                    int koltukSayisi = 0;
                    if (int.TryParse(koltukSayisiStr, out var k)) koltukSayisi = k;
                    
                    int? km = null;
                    if (int.TryParse(kmStr?.Replace(".", "").Replace(",", ""), out var kmVal)) km = kmVal;
                    
                    var aracTipi = ParseAracTipi(aracTipiStr);
                    
                    // Mevcut araç var mı?
                    if (mevcutSaseNolar.Contains(saseNo))
                    {
                        // Güncelle
                        var mevcutArac = await _context.Araclar.FirstOrDefaultAsync(a => a.SaseNo.ToUpper() == saseNo && !a.IsDeleted);
                        if (mevcutArac != null)
                        {
                            if (!string.IsNullOrWhiteSpace(marka)) mevcutArac.Marka = marka;
                            if (!string.IsNullOrWhiteSpace(model)) mevcutArac.Model = model;
                            if (modelYili.HasValue) mevcutArac.ModelYili = modelYili;
                            if (!string.IsNullOrWhiteSpace(motorNo)) mevcutArac.MotorNo = motorNo;
                            if (!string.IsNullOrWhiteSpace(renk)) mevcutArac.Renk = renk;
                            if (koltukSayisi > 0) mevcutArac.KoltukSayisi = koltukSayisi;
                            if (km.HasValue) mevcutArac.KmDurumu = km;
                            mevcutArac.AracTipi = aracTipi;
                            mevcutArac.UpdatedAt = DateTime.UtcNow;
                            result.UpdatedCount++;
                        }
                    }
                    else
                    {
                        // Yeni ekle
                        var yeniArac = new Arac
                        {
                            SaseNo = saseNo,
                            Marka = marka,
                            Model = model,
                            ModelYili = modelYili,
                            MotorNo = motorNo,
                            Renk = renk,
                            KoltukSayisi = koltukSayisi,
                            AracTipi = aracTipi,
                            KmDurumu = km,
                            Aktif = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        
                        // Plaka varsa ekle
                        if (!string.IsNullOrWhiteSpace(plaka))
                        {
                            yeniArac.AktifPlaka = plaka;
                            yeniArac.PlakaGecmisi.Add(new AracPlaka
                            {
                                Plaka = plaka,
                                GirisTarihi = DateTime.UtcNow,
                                IslemTipi = PlakaIslemTipi.Alis,
                                Aciklama = "Excel'den aktarıldı",
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                        
                        _context.Araclar.Add(yeniArac);
                        mevcutSaseNolar.Add(saseNo);
                        result.ImportedCount++;
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Satır {row}: {ex.Message}");
                    result.ErrorCount++;
                }
            }
            
            await _context.SaveChangesAsync();
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Excel okuma hatası: {ex.Message}");
            result.Success = false;
        }
        
        return result;
    }

    private AracTipi ParseAracTipi(string? tip)
    {
        if (string.IsNullOrWhiteSpace(tip)) return AracTipi.Minibus;
        
        var tipUpper = tip.ToUpperInvariant().Replace("İ", "I").Replace("Ü", "U").Replace("Ö", "O");
        
        return tipUpper switch
        {
            "MINIBUS" or "MİNİBÜS" => AracTipi.Minibus,
            "MIDIBUS" or "MİDİBÜS" => AracTipi.Midibus,
            "OTOBUS" or "OTOBÜS" => AracTipi.Otobus,
            "OTOMOBIL" or "OTOMOBİL" => AracTipi.Otomobil,
            "PANELVAN" => AracTipi.Panelvan,
            _ => AracTipi.Minibus
        };
    }

    #endregion
}
