namespace CRMFiloServis.Shared.Entities;

/// <summary>
/// Araç bilgileri - Þase numarasýna göre tekil
/// </summary>
public class Arac : BaseEntity
{
    // Þase numarasý - Tekil (Unique)
    public string SaseNo { get; set; } = string.Empty;
    
    // Aktif plaka - Otomatik hesaplanýr
    public string? AktifPlaka { get; set; }
    
    public string? Marka { get; set; }
    public string? Model { get; set; }
    public int? ModelYili { get; set; }
    public string? MotorNo { get; set; }
    public string? Renk { get; set; }
    public int KoltukSayisi { get; set; }
    public AracTipi AracTipi { get; set; }
    public AracSahiplikTipi SahiplikTipi { get; set; } = AracSahiplikTipi.Ozmal;
    
    // Kiralýk araç bilgileri
    public int? KiralikCariId { get; set; } // Araç sahibi (kiralýk ise)
    public decimal? GunlukKiraBedeli { get; set; }
    public decimal? AylikKiraBedeli { get; set; }
    public decimal? SeferBasinaKiraBedeli { get; set; }
    public KiraHesaplamaTipi? KiraHesaplamaTipi { get; set; }
    
    // Komisyon bilgileri
    public bool KomisyonVar { get; set; } = false;
    public int? KomisyoncuCariId { get; set; } // Komisyoncu
    public decimal? KomisyonOrani { get; set; } // Yüzde
    public decimal? SabitKomisyonTutari { get; set; } // Sefer baþýna sabit tutar
    public KomisyonHesaplamaTipi? KomisyonHesaplamaTipi { get; set; }
    
    // Belge tarihleri
    public DateTime? TrafikSigortaBitisTarihi { get; set; }
    public DateTime? KaskoBitisTarihi { get; set; }
    public DateTime? MuayeneBitisTarihi { get; set; }
    public int? KmDurumu { get; set; }
    public bool Aktif { get; set; } = true;
    public string? Notlar { get; set; }
    
    // Satýþ durumu
    public bool SatisaAcik { get; set; } = false;
    public decimal? SatisFiyati { get; set; }
    public DateTime? SatisaAcilmaTarihi { get; set; }
    public string? SatisAciklamasi { get; set; }

    // Navigation Properties
    public virtual Cari? KiralikCari { get; set; }
    public virtual Cari? KomisyoncuCari { get; set; }
    public virtual ICollection<AracPlaka> PlakaGecmisi { get; set; } = new List<AracPlaka>();
    public virtual ICollection<AracMasraf> Masraflar { get; set; } = new List<AracMasraf>();
    public virtual ICollection<ServisCalisma> ServisCalismalari { get; set; } = new List<ServisCalisma>();
    
    // Hesaplanan Özellik - Aktif plakayý döner (CikisTarihi null veya bugünden sonra)
    public AracPlaka? AktifPlakaKaydi => PlakaGecmisi?
        .Where(p => !p.IsDeleted && (p.CikisTarihi == null || p.CikisTarihi > DateTime.Today))
        .OrderByDescending(p => p.GirisTarihi)
        .FirstOrDefault();
}

/// <summary>
/// Araç plaka geçmiþi - Her þase için birden fazla plaka olabilir
/// </summary>
public class AracPlaka : BaseEntity
{
    public int AracId { get; set; }
    public virtual Arac Arac { get; set; } = null!;
    
    public string Plaka { get; set; } = string.Empty;
    
    // Plaka dönemi
    public DateTime GirisTarihi { get; set; }
    public DateTime? CikisTarihi { get; set; }
    
    // Ýþlem tipi
    public PlakaIslemTipi IslemTipi { get; set; }
    
    // Ek bilgiler
    public string? Aciklama { get; set; }
    public decimal? IslemTutari { get; set; } // Alýþ/Satýþ fiyatý
    
    // Ýliþkili kayýtlar
    public int? CariId { get; set; } // Kimden alýndý / Kime satýldý
    public virtual Cari? Cari { get; set; }
    
    // Aktif mi? (CikisTarihi null veya gelecek tarihli ise aktif)
    public bool Aktif => CikisTarihi == null || CikisTarihi > DateTime.Today;
}

public enum PlakaIslemTipi
{
    Alis = 1,           // Araç alýþý
    Satis = 2,          // Araç satýþý
    PlakaDevir = 3,     // Plaka devri (ayný þase, farklý plaka)
    Servis = 4,         // Servis giriþi
    Kiralama = 5,       // Kiralamaya verildi
    KiralamaBitis = 6,  // Kiralamadan döndü
    TramerKaydi = 7,    // Tramer kaydý
    Diger = 99
}

public enum AracTipi
{
    Minibus = 1,
    Midibus = 2,
    Otobus = 3,
    Otomobil = 4,
    Panelvan = 5
}

public enum AracSahiplikTipi
{
    Ozmal = 1,
    Kiralik = 2
}

public enum KiraHesaplamaTipi
{
    Gunluk = 1,
    Aylik = 2,
    SeferBasina = 3
}

public enum KomisyonHesaplamaTipi
{
    YuzdeOrani = 1,
    SabitTutar = 2
}
