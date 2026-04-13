namespace KOAFiloServis.Web.Models;

/// <summary>
/// Fatura haz�rl�k listesi - Kesilecek ve gelecek faturalar�n �zeti
/// </summary>
public class FaturaHazirlikListesi
{
    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }

    /// <summary>
    /// M��terilere kesilecek faturalar (Sat��)
    /// </summary>
    public List<KesilecekFaturaItem> KesilecekFaturalar { get; set; } = new();

    /// <summary>
    /// Tedarik�ilerden gelecek faturalar (Al�� - Kira, Komisyon)
    /// </summary>
    public List<GelecekFaturaItem> GelecekFaturalar { get; set; } = new();

    public decimal ToplamKesilecekTutar => KesilecekFaturalar.Sum(x => x.ToplamTutar);
    public decimal ToplamGelecekTutar => GelecekFaturalar.Sum(x => x.ToplamTutar);
    public decimal NetKar => ToplamKesilecekTutar - ToplamGelecekTutar;
}

/// <summary>
/// M��teriye kesilecek fatura kalemi
/// </summary>
public class KesilecekFaturaItem
{
    public int CariId { get; set; }
    public string CariUnvan { get; set; } = string.Empty;
    public string CariKodu { get; set; } = string.Empty;

    public List<KesilecekFaturaDetay> Detaylar { get; set; } = new();

    public int ToplamSeferSayisi => Detaylar.Sum(x => x.SeferSayisi);
    public decimal ToplamTutar => Detaylar.Sum(x => x.ToplamTutar);
}

/// <summary>
/// Kesilecek fatura detay� - G�zergah bazl�
/// </summary>
public class KesilecekFaturaDetay
{
    public int GuzergahId { get; set; }
    public string GuzergahKodu { get; set; } = string.Empty;
    public string GuzergahAdi { get; set; } = string.Empty;
    public string? BaslangicNoktasi { get; set; }
    public string? BitisNoktasi { get; set; }

    public int SeferSayisi { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal ToplamTutar { get; set; }

    // Detay bilgileri
    public List<SeferDetay> Seferler { get; set; } = new();
}

/// <summary>
/// Sefer detay bilgisi
/// </summary>
public class SeferDetay
{
    public int ServisCalismaId { get; set; }
    public DateTime Tarih { get; set; }
    public string ServisTuru { get; set; } = string.Empty;
    public string AracPlaka { get; set; } = string.Empty;
    public string SoforAdSoyad { get; set; } = string.Empty;
    public decimal Fiyat { get; set; }

    // Kiral�k ara� bilgisi
    public bool KiralikArac { get; set; }
    public string? KiralikAracSahibi { get; set; }
    public decimal? KiraBedeli { get; set; }

    // Komisyon bilgisi
    public bool KomisyonVar { get; set; }
    public string? KomisyoncuUnvan { get; set; }
    public decimal? KomisyonTutari { get; set; }
}

/// <summary>
/// Tedarik�iden gelecek fatura kalemi (Kira veya Komisyon)
/// </summary>
public class GelecekFaturaItem
{
    public int CariId { get; set; }
    public string CariUnvan { get; set; } = string.Empty;
    public string CariKodu { get; set; } = string.Empty;
    public GelecekFaturaTipi FaturaTipi { get; set; }

    public List<GelecekFaturaDetay> Detaylar { get; set; } = new();

    public int ToplamSeferSayisi => Detaylar.Sum(x => x.SeferSayisi);
    public decimal ToplamTutar => Detaylar.Sum(x => x.ToplamTutar);
}

/// <summary>
/// Gelecek fatura detay�
/// </summary>
public class GelecekFaturaDetay
{
    public string AracPlaka { get; set; } = string.Empty;
    public string? GuzergahAdi { get; set; }
    public string? MusteriUnvan { get; set; }

    public int SeferSayisi { get; set; }
    public decimal BirimTutar { get; set; }
    public decimal ToplamTutar { get; set; }

    public string Aciklama { get; set; } = string.Empty;

    // Detay seferler
    public List<SeferOzet> Seferler { get; set; } = new();
}

/// <summary>
/// Sefer �zet bilgisi
/// </summary>
public class SeferOzet
{
    public int ServisCalismaId { get; set; }
    public DateTime Tarih { get; set; }
    public string GuzergahAdi { get; set; } = string.Empty;
    public decimal SeferFiyati { get; set; }
    public decimal HesaplananTutar { get; set; } // Kira veya komisyon tutar�
}

public enum GelecekFaturaTipi
{
    AracKirasi = 1,
    Komisyon = 2
}
