using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KOAFiloServis.Shared.Entities;

#region Komisyonculuk İş Takibi

/// <summary>
/// Komisyonculuk - Firmalardan alınan işler
/// </summary>
public class KomisyonculukIs : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string IsKodu { get; set; } = string.Empty;

    // İşi Veren Firma (Müşteri)
    public int MusteriCariId { get; set; }
    public virtual Cari MusteriCari { get; set; } = null!;

    // İş Detayları
    [Required]
    [StringLength(200)]
    public string IsAciklamasi { get; set; } = string.Empty;

    public DateTime BaslangicTarihi { get; set; } = DateTime.Today;
    public DateTime? BitisTarihi { get; set; }

    public KomisyonculukIsTipi IsTipi { get; set; } = KomisyonculukIsTipi.SoforluArac;

    // Fiyatlandırma
    public KomisyonculukFiyatlamaTipi FiyatlamaTipi { get; set; } = KomisyonculukFiyatlamaTipi.Gunluk;
    public decimal BirimFiyat { get; set; }
    public int ToplamGun { get; set; } = 1; // Günlük için
    public int ToplamSefer { get; set; } = 0; // Sefer başı için
    public decimal ToplamTutar { get; set; }

    // Durum
    public KomisyonculukIsDurum Durum { get; set; } = KomisyonculukIsDurum.Aktif;

    [StringLength(500)]
    public string? Notlar { get; set; }

    // Fatura Bilgileri
    public int? AlinanIsFaturaId { get; set; }
    public virtual Fatura? AlinanIsFatura { get; set; }
    public bool FaturaKesildi { get; set; } = false;
    public DateTime? FaturaKesimTarihi { get; set; }

    // Navigation
    public virtual ICollection<KomisyonculukIsAtama> Atamalar { get; set; } = new List<KomisyonculukIsAtama>();
}

/// <summary>
/// Komisyonculuk - İşe araç/şoför atama
/// </summary>
public class KomisyonculukIsAtama : BaseEntity
{
    public int KomisyonculukIsId { get; set; }
    public virtual KomisyonculukIs KomisyonculukIs { get; set; } = null!;

    // Atanan Araç (Özmal veya Dışarıdan Kiralık)
    public int? AracId { get; set; }
    public virtual Arac? Arac { get; set; }

    // Dışarıdan alınan araç ise tedarikçi
    public int? TedarikciCariId { get; set; }
    public virtual Cari? TedarikciCari { get; set; }

    [StringLength(15)]
    public string? DisAracPlaka { get; set; } // Dış araç plakası

    // Şoför Bilgileri
    public AracAtamaTipi AtamaTipi { get; set; } = AracAtamaTipi.OzmalSoforlu;
    
    public int? SoforId { get; set; } // Kendi şoförümüz
    public virtual Sofor? Sofor { get; set; }

    [StringLength(100)]
    public string? DisSoforAdSoyad { get; set; } // Dış şoför

    [StringLength(20)]
    public string? DisSoforTelefon { get; set; }

    // Maliyet Bilgileri
    public decimal AracKiraBedeli { get; set; } = 0;
    public decimal SoforMaliyeti { get; set; } = 0;
    public decimal YakitMaliyeti { get; set; } = 0;
    public decimal OtoyolMaliyeti { get; set; } = 0;
    public decimal DigerMasraflar { get; set; } = 0;
    public decimal ToplamMaliyet => AracKiraBedeli + SoforMaliyeti + YakitMaliyeti + OtoyolMaliyeti + DigerMasraflar;

    // Verilen İş Faturası
    public int? VerilenIsFaturaId { get; set; }
    public virtual Fatura? VerilenIsFatura { get; set; }
    public bool TedarikciOdendi { get; set; } = false;
    public DateTime? TedarikciOdemeTarihi { get; set; }

    // Tarih Aralığı
    public DateTime BaslangicTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }

    [StringLength(500)]
    public string? Notlar { get; set; }
}

#endregion

#region Araç Alım/Satım Yönetimi

/// <summary>
/// Araç Alım/Satım İşlemleri - Noter evrak takibi dahil
/// </summary>
public class AracAlimSatim : BaseEntity
{
    public int AracId { get; set; }
    public virtual Arac Arac { get; set; } = null!;

    public AracIslemTipiDetay IslemTipi { get; set; }

    // Karşı Taraf
    public int? KarsiTarafCariId { get; set; }
    public virtual Cari? KarsiTarafCari { get; set; }

    [StringLength(100)]
    public string? KarsiTarafAdSoyad { get; set; } // Cari yoksa

    [StringLength(11)]
    public string? KarsiTarafTcKimlik { get; set; }

    [StringLength(20)]
    public string? KarsiTarafTelefon { get; set; }

    // İşlem Bilgileri
    public DateTime IslemTarihi { get; set; } = DateTime.Today;
    public decimal IslemTutari { get; set; }
    public decimal KDVTutari { get; set; }
    public decimal ToplamTutar { get; set; }

    // Noter Bilgileri
    [StringLength(100)]
    public string? NoterAdi { get; set; }

    public DateTime? NoterTarihi { get; set; }

    [StringLength(50)]
    public string? NoterYevmiyeNo { get; set; }

    public bool NoterIslemTamam { get; set; } = false;

    // Fatura Bilgileri
    public int? FaturaId { get; set; }
    public virtual Fatura? Fatura { get; set; }
    public bool FaturaKesildi { get; set; } = false;
    public DateTime? FaturaKesimTarihi { get; set; }
    public FaturaUyumsuzlukDurum FaturaUyumu { get; set; } = FaturaUyumsuzlukDurum.Beklemede;

    [StringLength(500)]
    public string? FaturaUyumsuzlukAciklama { get; set; }

    // Ödeme Durumu
    public AracIslemOdemeDurum OdemeDurum { get; set; } = AracIslemOdemeDurum.Beklemede;
    public DateTime? OdemeTarihi { get; set; }
    public decimal OdenenTutar { get; set; }

    [StringLength(500)]
    public string? Notlar { get; set; }

    // Belge Takibi
    public bool RuhsatTeslimAlindi { get; set; } = false;
    public bool SigortaTeslimAlindi { get; set; } = false;
    public bool MuayeneBelgesiTeslimAlindi { get; set; } = false;
    public bool AnahtarTeslimAlindi { get; set; } = false;
    public bool YedekAnahtarTeslimAlindi { get; set; } = false;
    public bool ServisBakimDefteri { get; set; } = false;

    [StringLength(500)]
    public string? EksikBelgeler { get; set; }
}

#endregion

#region C Plaka Dönüşüm Takibi

/// <summary>
/// C Plaka (Ticari) → Normal Plaka Dönüşüm Takibi
/// </summary>
public class PlakaDonusum : BaseEntity
{
    public int AracId { get; set; }
    public virtual Arac Arac { get; set; } = null!;

    // Eski Plaka Bilgileri
    [Required]
    [StringLength(15)]
    public string EskiPlaka { get; set; } = string.Empty;

    public PlakaTipi EskiPlakaTipi { get; set; } = PlakaTipi.CPlakaLimonSari;

    // Yeni Plaka Bilgileri
    [StringLength(15)]
    public string? YeniPlaka { get; set; }

    public PlakaTipi YeniPlakaTipi { get; set; } = PlakaTipi.OzelPlaka;

    // İşlem Durumu
    public PlakaDonusumDurum Durum { get; set; } = PlakaDonusumDurum.BasvuruYapildi;
    public DateTime BasvuruTarihi { get; set; } = DateTime.Today;
    public DateTime? OnayTarihi { get; set; }
    public DateTime? TamamlanmaTarihi { get; set; }

    // Masraflar
    public decimal PlakaBedeliMasrafi { get; set; } = 0;
    public decimal EmnivetHarci { get; set; } = 0;
    public decimal NoterMasrafi { get; set; } = 0;
    public decimal DigerMasraflar { get; set; } = 0;
    public decimal ToplamMasraf => PlakaBedeliMasrafi + EmnivetHarci + NoterMasrafi + DigerMasraflar;

    // Plaka Satışı
    public bool PlakaSatilacakMi { get; set; } = false;
    public decimal? PlakaSatisBedeli { get; set; }
    public int? PlakaSatisCarisiId { get; set; }
    public virtual Cari? PlakaSatisCarisi { get; set; }
    public bool PlakaSatildi { get; set; } = false;
    public DateTime? PlakaSatisTarihi { get; set; }

    [StringLength(500)]
    public string? Notlar { get; set; }
}

#endregion

#region Filo Operasyon Özeti

/// <summary>
/// Araç Operasyon Durumu - Günlük/Aylık takip
/// </summary>
public class AracOperasyonDurum : BaseEntity
{
    public int AracId { get; set; }
    public virtual Arac Arac { get; set; } = null!;

    public int Yil { get; set; }
    public int Ay { get; set; }

    // Operasyon Tipi
    public AracOperasyonTipi OperasyonTipi { get; set; }

    // Çalışma Bilgileri
    public int ToplamCalismaGunu { get; set; }
    public int ToplamSeferSayisi { get; set; }
    public int ToplamKm { get; set; }

    // Gelir
    public decimal BrutGelir { get; set; }
    public decimal KomisyonKesintisi { get; set; } // Komisyoncu payı
    public decimal NetGelir => BrutGelir - KomisyonKesintisi;

    // Giderler
    public decimal YakitGideri { get; set; }
    public decimal SoforMaliyeti { get; set; }
    public decimal KiraBedeli { get; set; } // Kiralık araçsa
    public decimal BakimOnarimGideri { get; set; }
    public decimal SigortaGideri { get; set; }
    public decimal VergiGideri { get; set; }
    public decimal OtoyolGideri { get; set; }
    public decimal DigerGiderler { get; set; }
    public decimal ToplamGider => YakitGideri + SoforMaliyeti + KiraBedeli + BakimOnarimGideri +
                                   SigortaGideri + VergiGideri + OtoyolGideri + DigerGiderler;

    // Kar/Zarar
    public decimal NetKarZarar => NetGelir - ToplamGider;

    [StringLength(500)]
    public string? Notlar { get; set; }
}

#endregion

#region Enums

public enum KomisyonculukIsTipi
{
    /// <summary>
    /// Şoförlü araç kiralama
    /// </summary>
    SoforluArac = 1,

    /// <summary>
    /// Şoförsüz araç kiralama
    /// </summary>
    SoforsuzArac = 2,

    /// <summary>
    /// Sadece şoför temini
    /// </summary>
    SadeceArac = 3,

    /// <summary>
    /// Personel taşıma servisi
    /// </summary>
    PersonelServisi = 4,

    /// <summary>
    /// Tek seferlik transfer
    /// </summary>
    Transfer = 5
}

public enum KomisyonculukFiyatlamaTipi
{
    Gunluk = 1,
    Aylik = 2,
    SeferBasina = 3,
    SaatBasina = 4,
    TopluPaket = 5
}

public enum KomisyonculukIsDurum
{
    Aktif = 1,
    Tamamlandi = 2,
    IptalEdildi = 3,
    Beklemede = 4
}

public enum AracAtamaTipi
{
    /// <summary>
    /// Kendi aracımız + kendi şoförümüz
    /// </summary>
    OzmalSoforlu = 1,

    /// <summary>
    /// Kendi aracımız + dış şoför
    /// </summary>
    OzmalDisSofor = 2,

    /// <summary>
    /// Dışarıdan kiralık araç + kendi şoförümüz
    /// </summary>
    KiralikKendiSofor = 3,

    /// <summary>
    /// Dışarıdan kiralık araç + dış şoför (tedarikçiden tam paket)
    /// </summary>
    KiralikDisSofor = 4,

    /// <summary>
    /// Sadece araç (şoförsüz)
    /// </summary>
    SadeceArac = 5
}

public enum AracIslemTipiDetay
{
    /// <summary>
    /// Araç satın alma
    /// </summary>
    Alis = 1,

    /// <summary>
    /// Araç satışı
    /// </summary>
    Satis = 2,

    /// <summary>
    /// Takas (araç karşılığı araç)
    /// </summary>
    Takas = 3,

    /// <summary>
    /// Satış + Yerine araç alımı
    /// </summary>
    SatisYerineAlim = 4
}

public enum FaturaUyumsuzlukDurum
{
    Beklemede = 0,
    Uyumlu = 1,
    TutarFarkli = 2,
    KdvFarkli = 3,
    BelgeEksik = 4,
    DigerUyumsuzluk = 5
}

public enum AracIslemOdemeDurum
{
    Beklemede = 0,
    KismiOdendi = 1,
    TamOdendi = 2,
    Vadeli = 3,
    IptalEdildi = 4
}

public enum PlakaTipi
{
    /// <summary>
    /// C Plaka - Limon Sarısı (Ticari)
    /// </summary>
    CPlakaLimonSari = 1,

    /// <summary>
    /// C Plaka - Turkuaz (Servis)
    /// </summary>
    CPlakaTurkuaz = 2,

    /// <summary>
    /// Normal Özel Plaka (Beyaz)
    /// </summary>
    OzelPlaka = 3,

    /// <summary>
    /// M Plaka (Minibüs)
    /// </summary>
    MPlaka = 4,

    /// <summary>
    /// S Plaka (Servis)
    /// </summary>
    SPlaka = 5
}

public enum PlakaDonusumDurum
{
    BasvuruYapildi = 1,
    EvrakIncelemede = 2,
    Onaylandi = 3,
    YeniPlakaAlindi = 4,
    Tamamlandi = 5,
    Reddedildi = 6,
    IptalEdildi = 7
}

public enum AracOperasyonTipi
{
    /// <summary>
    /// Özmal araç kendi işlerimizde çalıştırılıyor
    /// </summary>
    OzmalCalistirma = 1,

    /// <summary>
    /// Özmal araç dışarıya kiralandı
    /// </summary>
    OzmalKiralama = 2,

    /// <summary>
    /// Dışarıdan kiralanan araç kendi işlerimizde çalışıyor
    /// </summary>
    KiralikCalistirma = 3,

    /// <summary>
    /// Komisyonculuk - aracılık işi
    /// </summary>
    Komisyonculuk = 4,

    /// <summary>
    /// Araç boşta/çalışmıyor
    /// </summary>
    Bosta = 5
}

#endregion
