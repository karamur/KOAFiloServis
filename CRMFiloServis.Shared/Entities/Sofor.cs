namespace CRMFiloServis.Shared.Entities;

/// <summary>
/// Personel bilgileri (Şoför, Ofis Çalışanı, Yönetici vb.)
/// </summary>
public class Sofor : BaseEntity
{
    public string SoforKodu { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;
    public string? TcKimlikNo { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? Adres { get; set; }
    
    // Görev Bilgisi
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
    public bool Aktif { get; set; } = true;
    public string? Notlar { get; set; }
    
    // Maaş Bilgileri
    public decimal BrutMaas { get; set; }
    public decimal ResmiNetMaas { get; set; }
    public decimal DigerMaas { get; set; }
    public decimal NetMaas { get; set; }

    // ARGE ve Toplu Maaş Bilgileri
    public bool ArgePersoneli { get; set; } = false;
    public decimal TopluMaas { get; set; } // SGK'ya bildirilen + ekstra ödeme toplamı
    public decimal SgkMaasi { get; set; } // SGK'ya bildirilen maaş
    public decimal EkOdeme => TopluMaas - SgkMaasi; // Geriye kalan ödeme

    // Banka Bilgileri
    public string? BankaAdi { get; set; }
    public string? IBAN { get; set; }

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
