namespace KOAFiloServis.Shared.Entities;

/// <summary>
/// Şirketler arası veri transfer işlemlerinin log kaydı
/// </summary>
public class SirketTransferLog : BaseEntity
{
    /// <summary>
    /// Transfer edilen entity türü (Cari, Arac, Sofor, vb.)
    /// </summary>
    public required string EntityTuru { get; set; }

    /// <summary>
    /// Transfer edilen entity ID'si
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// Entity açıklaması (Cari adı, Araç plakası vb.)
    /// </summary>
    public string? EntityAciklama { get; set; }

    /// <summary>
    /// Kaynak şirket ID'si
    /// </summary>
    public int KaynakSirketId { get; set; }

    /// <summary>
    /// Kaynak şirket
    /// </summary>
    public virtual Sirket? KaynakSirket { get; set; }

    /// <summary>
    /// Hedef şirket ID'si
    /// </summary>
    public int HedefSirketId { get; set; }

    /// <summary>
    /// Hedef şirket
    /// </summary>
    public virtual Sirket? HedefSirket { get; set; }

    /// <summary>
    /// Transfer işlemini yapan kullanıcı ID'si
    /// </summary>
    public int KullaniciId { get; set; }

    /// <summary>
    /// Transfer işlemini yapan kullanıcı
    /// </summary>
    public virtual Kullanici? Kullanici { get; set; }

    /// <summary>
    /// Transfer tarihi
    /// </summary>
    public DateTime TransferTarihi { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Transfer durumu
    /// </summary>
    public TransferDurum Durum { get; set; } = TransferDurum.Basarili;

    /// <summary>
    /// Hata mesajı (başarısız transferlerde)
    /// </summary>
    public string? HataMesaji { get; set; }

    /// <summary>
    /// İlişkili veriler de transfer edildi mi?
    /// </summary>
    public bool IliskiliVerilerTransferEdildi { get; set; }

    /// <summary>
    /// Transfer edilen ilişkili entity sayısı
    /// </summary>
    public int IliskiliEntitySayisi { get; set; }

    /// <summary>
    /// Ek notlar
    /// </summary>
    public string? Notlar { get; set; }
}

/// <summary>
/// Transfer işlem durumu
/// </summary>
public enum TransferDurum
{
    Basarili = 1,
    Basarisiz = 2,
    KismiBasarili = 3
}

/// <summary>
/// Transfer edilebilir entity türleri
/// </summary>
public static class TransferEntityTurleri
{
    public const string Cari = "Cari";
    public const string Arac = "Arac";
    public const string Sofor = "Sofor";
    public const string Guzergah = "Guzergah";
    public const string Fatura = "Fatura";
    public const string BankaHesap = "BankaHesap";
    public const string BankaKasaHareket = "BankaKasaHareket";

    public static readonly string[] Tumu = [Cari, Arac, Sofor, Guzergah, Fatura, BankaHesap, BankaKasaHareket];

    public static string GetDisplayName(string tur) => tur switch
    {
        Cari => "Cari",
        Arac => "Araç",
        Sofor => "Şoför",
        Guzergah => "Güzergah",
        Fatura => "Fatura",
        BankaHesap => "Banka Hesabı",
        BankaKasaHareket => "Banka/Kasa Hareketi",
        _ => tur
    };
}
