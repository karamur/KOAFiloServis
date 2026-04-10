namespace CRMFiloServis.Shared.Entities;

/// <summary>
/// Personel bilgileri (Şoför, Ofis Çalışanı, Yönetici vb.)
/// </summary>
public class Sofor : BaseEntity
{
    /// <summary>
    /// Multi-tenant: Şirket ID (null = sistem geneli)
    /// </summary>
    public int? SirketId { get; set; }
    public virtual Sirket? Sirket { get; set; }

    public string SoforKodu { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;
    public string? TcKimlikNo { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? Adres { get; set; }

    // Sıralama ve Görev Bilgisi
    public int SiralamaNo { get; set; } = 0;
    public PersonelGorev Gorev { get; set; } = PersonelGorev.Sofor;
    public string? Departman { get; set; }
    public string? Pozisyon { get; set; }
    
    // Şoför Belgeler (Sadece şoförler için)
    public string? EhliyetNo { get; set; }
    public DateTime? EhliyetGecerlilikTarihi { get; set; }
    public DateTime? SrcBelgesiGecerlilikTarihi { get; set; }
    public DateTime? PsikoteknikGecerlilikTarihi { get; set; }
    public DateTime? SaglikRaporuGecerlilikTarihi { get; set; }
    
    // Genel Bilgiler
    public DateTime? IseBaslamaTarihi { get; set; }
    public DateTime? IstenAyrilmaTarihi { get; set; }
    public DateTime? SgkCikisTarihi { get; set; }
    public bool Aktif { get; set; } = true;
    public string? Notlar { get; set; }
    
    // Maaş Bilgileri
    public BrutMaasHesaplamaTipi BrutMaasHesaplamaTipi { get; set; } = BrutMaasHesaplamaTipi.Manuel;
    public decimal CalismaMiktari { get; set; }
    public decimal BirimUcret { get; set; }
    public decimal BrutMaas { get; set; }
    public decimal ResmiNetMaas { get; set; }
    public decimal DigerMaas { get; set; }
    public decimal NetMaas { get; set; }

    // SGK Bordro Ayarları
    public bool SGKBordroDahilMi { get; set; } = false;
    public PersonelBordroTipi BordroTipiPersonel { get; set; } = PersonelBordroTipi.Yok;

    // ARGE ve Toplu Maaş Bilgileri
    public bool ArgePersoneli { get; set; } = false; // Geriye dönük uyumluluk
    public decimal TopluMaas { get; set; } // SGK'ya bildirilen + ekstra ödeme toplamı
    public decimal SgkMaasi { get; set; } // SGK'ya bildirilen maaş
    public decimal EkOdeme => TopluMaas - SgkMaasi; // Geriye kalan ödeme

    // Banka Bilgileri
    public string? BankaAdi { get; set; }
    public string? IBAN { get; set; }

    // Muhasebe Hesap Entegrasyonu
    public int? MuhasebeHesapId { get; set; }
    public virtual MuhasebeHesap? MuhasebeHesap { get; set; }

    public string TamAd => $"{Ad} {Soyad}";
    
    // Şoför mü kontrolü
    public bool IsSofor => Gorev == PersonelGorev.Sofor;

    // Navigation Properties
    public virtual ICollection<ServisCalisma> ServisCalismalari { get; set; } = new List<ServisCalisma>();
    public virtual ICollection<PersonelMaas> Maaslar { get; set; } = new List<PersonelMaas>();
    public virtual ICollection<PersonelIzin> Izinler { get; set; } = new List<PersonelIzin>();
    public virtual ICollection<PersonelIzinHakki> IzinHaklari { get; set; } = new List<PersonelIzinHakki>();
}

/// <summary>
/// Personel görev türleri
/// </summary>
public enum PersonelGorev
{
    Sofor = 1,
    OfisCalisani = 2,
    Muhasebe = 3,
    Yonetici = 4,
    Teknik = 5,
    Diger = 99
}

/// <summary>
/// Personel bordro tipi (SGK bordrosuna dahil mi ve hangi tip)
/// </summary>
public enum PersonelBordroTipi
{
    Yok = 0,
    Normal = 1,
    Arge = 2
}

public enum BrutMaasHesaplamaTipi
{
    Manuel = 0,
    Saatlik = 1,
    Aylik = 2,
    Gunluk = 3
}
