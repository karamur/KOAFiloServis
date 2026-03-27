using System.ComponentModel.DataAnnotations;

namespace CRMFiloServis.Shared.Entities;

#region Bildirim/Uyarý Sistemi

/// <summary>
/// Kullanýcý bildirimleri - uyarý sistemi
/// </summary>
public class Bildirim : BaseEntity
{
    public int KullaniciId { get; set; }
    public virtual Kullanici Kullanici { get; set; } = null!;

    [Required]
    [StringLength(200)]
    public string Baslik { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Icerik { get; set; }

    public BildirimTipi Tip { get; set; } = BildirimTipi.Bilgi;
    public BildirimOncelik Oncelik { get; set; } = BildirimOncelik.Normal;

    public bool Okundu { get; set; } = false;
    public DateTime? OkunmaTarihi { get; set; }

    // Ýliþkili kayýt bilgisi
    public string? IliskiliTablo { get; set; } // "Cari", "Fatura", "Arac" vs.
    public int? IliskiliKayitId { get; set; }
    public string? Link { get; set; } // Yönlendirilecek sayfa

    // Zamanlama
    public DateTime? SonGosterimTarihi { get; set; }
    public bool Tekrarli { get; set; } = false;
}

public enum BildirimTipi
{
    Bilgi = 0,
    Uyari = 1,
    Hata = 2,
    Basari = 3,
    BelgeSuresi = 4,
    OdemeBildirimi = 5,
    Hatirlatici = 6,
    Mesaj = 7
}

public enum BildirimOncelik
{
    Dusuk = 0,
    Normal = 1,
    Yuksek = 2,
    Kritik = 3
}

#endregion

#region Mesajlaþma Sistemi

/// <summary>
/// Dahili mesajlaþma sistemi
/// </summary>
public class Mesaj : BaseEntity
{
    public int GonderenId { get; set; }
    public virtual Kullanici Gonderen { get; set; } = null!;

    public int? AliciId { get; set; } // null ise tüm kullanýcýlara
    public virtual Kullanici? Alici { get; set; }

    [Required]
    [StringLength(200)]
    public string Konu { get; set; } = string.Empty;

    [Required]
    public string Icerik { get; set; } = string.Empty;

    public MesajTipi Tip { get; set; } = MesajTipi.Dahili;
    public MesajDurum Durum { get; set; } = MesajDurum.Gonderildi;

    public bool Okundu { get; set; } = false;
    public DateTime? OkunmaTarihi { get; set; }

    // Dýþ sistemler için
    public string? DisAlici { get; set; } // Telefon veya email
    public string? DisGonderimId { get; set; } // WhatsApp/SMS ID

    // Yanýt zinciri
    public int? UstMesajId { get; set; }
    public virtual Mesaj? UstMesaj { get; set; }
    public virtual ICollection<Mesaj> Yanitlar { get; set; } = new List<Mesaj>();
}

public enum MesajTipi
{
    Dahili = 0,
    Email = 1,
    SMS = 2,
    WhatsApp = 3
}

public enum MesajDurum
{
    Taslak = 0,
    Gonderildi = 1,
    Iletildi = 2,
    Okundu = 3,
    Hata = 4
}

/// <summary>
/// Email ayarlarý
/// </summary>
public class EmailAyar : BaseEntity
{
    public int? KullaniciId { get; set; } // null ise firma geneli
    public virtual Kullanici? Kullanici { get; set; }

    [Required]
    [StringLength(100)]
    public string SmtpSunucu { get; set; } = string.Empty;

    public int SmtpPort { get; set; } = 587;
    public bool SslKullan { get; set; } = true;

    [StringLength(100)]
    public string? ImapSunucu { get; set; }

    public int ImapPort { get; set; } = 993;
    public bool ImapSslKullan { get; set; } = true;
    public bool GelenKutusuAktif { get; set; } = false;

    [StringLength(100)]
    public string GelenKlasoru { get; set; } = "INBOX";

    [Required]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Sifre { get; set; }

    [StringLength(100)]
    public string? GonderenAdi { get; set; }

    public bool Aktif { get; set; } = true;
}

/// <summary>
/// WhatsApp ayarlarý
/// </summary>
public class WhatsAppAyar : BaseEntity
{
    public int? KullaniciId { get; set; }
    public virtual Kullanici? Kullanici { get; set; }

    [StringLength(20)]
    public string? Telefon { get; set; }

    [StringLength(500)]
    public string? ApiKey { get; set; }

    [StringLength(200)]
    public string? WebhookUrl { get; set; }

    public string? HizliSablonlarJson { get; set; }

    public bool Aktif { get; set; } = false;
}

#endregion

#region Hatýrlatýcý/Randevu Sistemi

/// <summary>
/// Kullanýcý hatýrlatýcýlarý ve randevularý
/// </summary>
public class Hatirlatici : BaseEntity
{
    public int KullaniciId { get; set; }
    public virtual Kullanici Kullanici { get; set; } = null!;

    [Required]
    [StringLength(200)]
    public string Baslik { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Aciklama { get; set; }

    public HatirlaticiTip Tip { get; set; } = HatirlaticiTip.Hatirlatici;

    public DateTime BaslangicTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }
    public bool TumGun { get; set; } = false;

    // Tekrar ayarlarý
    public TekrarTipi TekrarTipi { get; set; } = TekrarTipi.Yok;
    public int TekrarAraligi { get; set; } = 1; // Her X gün/hafta/ay
    public DateTime? TekrarBitisTarihi { get; set; }

    // Bildirim
    public int BildirimDakikaOnce { get; set; } = 15;
    public bool EmailBildirim { get; set; } = false;
    public bool PushBildirim { get; set; } = true;

    // Ýliþkili kayýt
    public string? IliskiliTablo { get; set; }
    public int? IliskiliKayitId { get; set; }

    // Durum
    public HatirlaticiDurum Durum { get; set; } = HatirlaticiDurum.Bekliyor;
    public string? Renk { get; set; } = "#0d6efd";

    // Cari/Kiþi baðlantýsý
    public int? CariId { get; set; }
    public virtual Cari? Cari { get; set; }
}

public enum HatirlaticiTip
{
    Hatirlatici = 0,
    Randevu = 1,
    Toplanti = 2,
    Gorev = 3,
    Arama = 4,
    Ziyaret = 5
}

public enum TekrarTipi
{
    Yok = 0,
    Gunluk = 1,
    Haftalik = 2,
    Aylik = 3,
    Yillik = 4
}

public enum HatirlaticiDurum
{
    Bekliyor = 0,
    Tamamlandi = 1,
    Iptal = 2,
    Ertelendi = 3
}

#endregion

#region Kullanýcý-Cari Eþleþtirme

/// <summary>
/// Kullanýcýya baðlý cariler
/// </summary>
public class KullaniciCari : BaseEntity
{
    public int KullaniciId { get; set; }
    public virtual Kullanici Kullanici { get; set; } = null!;

    public int CariId { get; set; }
    public virtual Cari Cari { get; set; } = null!;

    // Ýzinler
    public bool EkstreGorebilir { get; set; } = true;
    public bool FaturaGorebilir { get; set; } = true;
    public bool OdemeYapabilir { get; set; } = false;
    public bool DuzenlemeYapabilir { get; set; } = false;

    // Cari tipi
    public KullaniciCariTip Tip { get; set; } = KullaniciCariTip.Musteri;

    public string? Not { get; set; }
}

public enum KullaniciCariTip
{
    Musteri = 0,      // Müþteri carisi
    Tedarikci = 1,    // Tedarikçi/Satýcý
    Personel = 2,     // Personel carisi
    Ozel = 3          // Özel tanýmlý
}

#endregion

#region Dashboard Widget

/// <summary>
/// Kullanýcý dashboard widget ayarlarý
/// </summary>
public class DashboardWidget : BaseEntity
{
    public int KullaniciId { get; set; }
    public virtual Kullanici Kullanici { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string WidgetKodu { get; set; } = string.Empty; // "bildirimler", "mesajlar", "randevular" vs.

    public int Sira { get; set; } = 0;
    public int Kolon { get; set; } = 0; // 0-11 (12 sütunlu grid)
    public int Genislik { get; set; } = 4; // col-md-X

    public bool Gorunur { get; set; } = true;
    public bool Kucultulmus { get; set; } = false;

    public string? Ayarlar { get; set; } // JSON formatýnda widget ayarlarý
}

#endregion
