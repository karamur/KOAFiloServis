using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMFiloServis.Shared.Entities;

/// <summary>
/// Puantaj/Hakedis Kayıtları - Excel'den içe aktarılan ve manuel düzenlenebilen kayıtlar
/// </summary>
public class PuantajKayit : BaseEntity
{
    // Dönem Bilgisi
    public int Yil { get; set; }
    public int Ay { get; set; }
    
    // Kurum/Firma (Müşteri)
    public int? KurumCariId { get; set; }
    public virtual Cari? KurumCari { get; set; }
    public string? KurumAdi { get; set; } // Excel'den gelen ham değer
    
    // Güzergah
    public int? GuzergahId { get; set; }
    public virtual Guzergah? Guzergah { get; set; }
    public string? GuzergahAdi { get; set; } // Excel'den gelen ham değer
    
    // Yön (Sabah, Akşam, Sabah-Akşam, Diğer)
    public PuantajYon Yon { get; set; } = PuantajYon.SabahAksam;
    
    // Araç (Plaka)
    public int? AracId { get; set; }
    public virtual Arac? Arac { get; set; }
    public string? Plaka { get; set; } // Excel'den gelen ham değer
    
    // Şoför
    public int? SoforId { get; set; }
    public virtual Sofor? Sofor { get; set; }
    public string? SoforAdi { get; set; } // Excel'den gelen ham değer
    public string? SoforTelefon { get; set; }
    
    // Şoför Tipi (Özel, Kiralık, Komisyoncu)
    public SoforOdemeTipi SoforOdemeTipi { get; set; } = SoforOdemeTipi.Ozmal;
    
    // Ödeme Yapılacak Firma/Cari (Kiralık/Komisyoncu için)
    public int? OdemeYapilacakCariId { get; set; }
    public virtual Cari? OdemeYapilacakCari { get; set; }
    
    // Fatura Kesen Cari (Müşteriye fatura kesen firma)
    public int? FaturaKesiciCariId { get; set; }
    public virtual Cari? FaturaKesiciCari { get; set; }
    public string? FaturaKesiciAdi { get; set; } // Excel'den gelen ham değer
    public string? FaturaKesiciTelefon { get; set; }
    
    // Gün/Sefer Bilgisi
    public decimal Gun { get; set; } // Çarpan (örn: 1, 0.5, 22 gün vb.)
    public int SeferSayisi { get; set; } = 1;
    
    // GELİR (Müşteriden Alınacak)
    [Column(TypeName = "decimal(18,2)")]
    public decimal BirimGelir { get; set; } // Sefer/gün başına

    [Column(TypeName = "decimal(18,2)")]
    public decimal ToplamGelir { get; set; } // BirimGelir * Gun

    public int GelirKdvOrani { get; set; } = 20; // %20 veya %10

    [Column(TypeName = "decimal(18,2)")]
    public decimal GelirKdvTutari { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal GelirToplam { get; set; } // KDV dahil toplam gelir

    // Gelir KDV Detayları (Gider tarafı gibi ayrıntılı)
    public int GelirKdvOrani20 { get; set; } = 0; // %20 KDV'li kısım tutarı
    [Column(TypeName = "decimal(18,2)")]
    public decimal GelirKdv20Tutari { get; set; }

    public int GelirKdvOrani10 { get; set; } = 0; // %10 KDV'li kısım tutarı
    [Column(TypeName = "decimal(18,2)")]
    public decimal GelirKdv10Tutari { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal GelirKesinti { get; set; } // Kesintiler

    [Column(TypeName = "decimal(18,2)")]
    public decimal Alinacak { get; set; } // Net alınacak tutar (ToplamGelir + KDVler - Kesintiler)
    
    // GİDER (Şoför/Tedarikçiye Ödenecek)
    [Column(TypeName = "decimal(18,2)")]
    public decimal BirimGider { get; set; } // Sefer/gün başına
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal ToplamGider { get; set; } // BirimGider * Gun
    
    // Gider KDV Oranları (Stopaj/Kesinti dahil)
    public int GiderKdvOrani20 { get; set; } = 0; // %20 KDV'li kısım tutarı (yüzde değil tutar)
    [Column(TypeName = "decimal(18,2)")]
    public decimal GiderKdv20Tutari { get; set; }
    
    public int GiderKdvOrani10 { get; set; } = 0; // %10 KDV'li kısım tutarı
    [Column(TypeName = "decimal(18,2)")]
    public decimal GiderKdv10Tutari { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal GiderKesinti { get; set; } // Stopaj veya diğer kesintiler
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Odenecek { get; set; } // Net ödenecek tutar (ToplamGider + KDVler - Kesintiler)

    // FARK (Gelir - Gider)
    [NotMapped]
    public decimal FarkTutari => Alinacak - Odenecek; // Pozitif: Kar, Negatif: Zarar
    
    // Fatura Durumu - GELİR (Müşteriye Kesilen Fatura)
    public bool GelirFaturaKesildi { get; set; } = false;
    public string? GelirFaturaNo { get; set; }
    public DateTime? GelirFaturaTarihi { get; set; }
    public int? GelirFaturaId { get; set; }
    
    // Fatura Durumu - GİDER (Tedarikçiden Alınan Fatura)
    public bool GiderFaturaAlindi { get; set; } = false;
    public string? GiderFaturaNo { get; set; }
    public DateTime? GiderFaturaTarihi { get; set; }
    public int? GiderFaturaId { get; set; }
    
    // Ödeme Durumu - GELİR (Müşteriden Tahsilat)
    public PuantajOdemeDurum GelirOdemeDurumu { get; set; } = PuantajOdemeDurum.Odenmedi;
    public DateTime? GelirOdemeTarihi { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal GelirOdenenTutar { get; set; }

    // Ödeme Durumu - GİDER (Tedarikçiye Ödeme)
    public PuantajOdemeDurum GiderOdemeDurumu { get; set; } = PuantajOdemeDurum.Odenmedi;
    public DateTime? GiderOdemeTarihi { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal GiderOdenenTutar { get; set; }
    
    // Onay Durumu
    public PuantajOnayDurum OnayDurum { get; set; } = PuantajOnayDurum.Taslak;
    public string? OnaylayanKullanici { get; set; }
    public DateTime? OnayTarihi { get; set; }
    
    // Kaynak Bilgisi
    public PuantajKaynak Kaynak { get; set; } = PuantajKaynak.Manuel;
    public int? ExcelImportId { get; set; } // Hangi import batch'inden geldi
    public int? ExcelSatirNo { get; set; } // Excel'deki satır numarası
    
    // Notlar ve Açıklamalar
    public string? Notlar { get; set; }
    
    // Hesaplama metodları
    public void HesaplaGelir()
    {
        ToplamGelir = BirimGelir * Gun;
        GelirKdvTutari = GelirKdv20Tutari + GelirKdv10Tutari;
        if (GelirKdvTutari == 0 && GelirKdvOrani > 0)
        {
            // Eğer ayrıntılı KDV girilmemişse genel orandan hesapla
            GelirKdvTutari = ToplamGelir * GelirKdvOrani / 100;
        }
        GelirToplam = ToplamGelir + GelirKdvTutari;
        Alinacak = ToplamGelir + GelirKdv20Tutari + GelirKdv10Tutari - GelirKesinti;
    }
    
    public void HesaplaGider()
    {
        ToplamGider = BirimGider * Gun;
        // KDV tutarları zaten Excel'den geliyor veya manuel giriliyor
        Odenecek = ToplamGider + GiderKdv20Tutari + GiderKdv10Tutari - GiderKesinti;
    }
}

/// <summary>
/// Excel Import Batch Kaydı
/// </summary>
public class PuantajExcelImport : BaseEntity
{
    public string DosyaAdi { get; set; } = string.Empty;
    public DateTime ImportTarihi { get; set; } = DateTime.UtcNow;
    public string? ImportEdenKullanici { get; set; }
    
    // İstatistikler
    public int ToplamSatir { get; set; }
    public int BasariliSatir { get; set; }
    public int HataliSatir { get; set; }
    public int OtoOlusturulanFirma { get; set; }
    public int OtoOlusturulanGuzergah { get; set; }
    public int OtoOlusturulanSofor { get; set; }
    
    // Dönem
    public int Yil { get; set; }
    public int Ay { get; set; }
    
    // Durum
    public ImportDurum Durum { get; set; } = ImportDurum.Bekliyor;
    public string? HataMesaji { get; set; }
    
    // İlişkili kayıtlar
    public virtual ICollection<PuantajKayit> Kayitlar { get; set; } = new List<PuantajKayit>();
}

/// <summary>
/// Import sırasında oluşturulan eşleştirme önerileri
/// </summary>
public class PuantajEslestirmeOneri : BaseEntity
{
    public int ExcelImportId { get; set; }
    public virtual PuantajExcelImport ExcelImport { get; set; } = null!;
    
    public EslestirmeTipi Tip { get; set; }
    public string ExcelDeger { get; set; } = string.Empty; // Excel'deki değer
    
    // Önerilen eşleştirmeler
    public int? OnerilenId { get; set; } // CariId, GuzergahId, SoforId, AracId
    public string? OnerilenAd { get; set; }
    public int BenzerlikPuani { get; set; } // 0-100
    
    public bool Onaylandi { get; set; } = false;
    public bool YeniOlusturulacak { get; set; } = false;
}

#region Enums

/// <summary>
/// Puantaj yön türleri
/// </summary>
public enum PuantajYon
{
    Sabah = 1,
    Aksam = 2,
    SabahAksam = 3,
    Diger = 4
}

/// <summary>
/// Şoför ödeme tipi
/// </summary>
public enum SoforOdemeTipi
{
    Ozmal = 1,        // Kendi şoförümüz, doğrudan ödeme
    Kiralik = 2,      // Kiralık araç ile gelen şoför, firmaya ödeme
    Komisyoncu = 3    // Komisyoncu üzerinden, komisyoncuya ödeme
}

/// <summary>
/// Puantaj ödeme durumu
/// </summary>
public enum PuantajOdemeDurum
{
    Odenmedi = 0,
    KismiOdendi = 1,
    Odendi = 2,
    Iptal = 3
}

/// <summary>
/// Puantaj onay durumu
/// </summary>
public enum PuantajOnayDurum
{
    Taslak = 0,
    OnayBekliyor = 1,
    Onaylandi = 2,
    Reddedildi = 3
}

/// <summary>
/// Puantaj kaydı kaynağı
/// </summary>
public enum PuantajKaynak
{
    Manuel = 0,
    ExcelImport = 1,
    ServisCalismaOtomatik = 2
}

/// <summary>
/// Excel import durumu
/// </summary>
public enum ImportDurum
{
    Bekliyor = 0,
    Eslestiriliyor = 1,
    OnayBekliyor = 2,
    Isleniyor = 3,
    Tamamlandi = 4,
    Hata = 5
}

/// <summary>
/// Eşleştirme tipi
/// </summary>
public enum EslestirmeTipi
{
    Kurum = 1,
    Guzergah = 2,
    Sofor = 3,
    Arac = 4,
    FaturaKesici = 5
}

#endregion
