namespace KOAFiloServis.Shared.Entities;

/// <summary>
/// Çoklu şirket (Multi-tenant) desteği için ana şirket entity'si.
/// Her tenant (kiracı) bir şirkettir.
/// </summary>
public class Sirket : BaseEntity
{
    /// <summary>
    /// Şirket kodu (benzersiz, kısa tanımlayıcı)
    /// Örn: "KOA", "ABC", "XYZ"
    /// </summary>
    public required string SirketKodu { get; set; }

    /// <summary>
    /// Şirket ticari unvanı
    /// </summary>
    public required string Unvan { get; set; }

    /// <summary>
    /// Kısa şirket adı (görüntüleme için)
    /// </summary>
    public string? KisaAd { get; set; }

    /// <summary>
    /// Vergi dairesi
    /// </summary>
    public string? VergiDairesi { get; set; }

    /// <summary>
    /// Vergi numarası
    /// </summary>
    public string? VergiNo { get; set; }

    /// <summary>
    /// Merkez adresi
    /// </summary>
    public string? Adres { get; set; }

    /// <summary>
    /// İl
    /// </summary>
    public string? Il { get; set; }

    /// <summary>
    /// İlçe
    /// </summary>
    public string? Ilce { get; set; }

    /// <summary>
    /// Posta kodu
    /// </summary>
    public string? PostaKodu { get; set; }

    /// <summary>
    /// Telefon numarası
    /// </summary>
    public string? Telefon { get; set; }

    /// <summary>
    /// E-posta adresi
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Web sitesi
    /// </summary>
    public string? WebSitesi { get; set; }

    /// <summary>
    /// Şirket logosu URL
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Şirket aktif mi?
    /// </summary>
    public bool Aktif { get; set; } = true;

    /// <summary>
    /// Varsayılan para birimi (TRY, USD, EUR vb.)
    /// </summary>
    public string ParaBirimi { get; set; } = "TRY";

    /// <summary>
    /// Şirket ayarları (JSON formatında)
    /// </summary>
    public string? AyarlarJson { get; set; }

    /// <summary>
    /// Lisans bitiş tarihi
    /// </summary>
    public DateTime? LisansBitisTarihi { get; set; }

    /// <summary>
    /// Maksimum kullanıcı sayısı
    /// </summary>
    public int MaxKullaniciSayisi { get; set; } = 5;

    /// <summary>
    /// Şirkete ait kullanıcılar
    /// </summary>
    public virtual ICollection<Kullanici> Kullanicilar { get; set; } = [];

    // Navigation properties - diğer tenant entity'leri buraya eklenecek
}

/// <summary>
/// Tenant bazlı entity'ler için base class.
/// SirketId foreign key'i ile multi-tenant filtreleme sağlar.
/// </summary>
public abstract class TenantEntity : BaseEntity
{
    /// <summary>
    /// Bu kaydın ait olduğu şirket ID'si
    /// </summary>
    public int SirketId { get; set; }

    /// <summary>
    /// İlişkili şirket
    /// </summary>
    public virtual Sirket? Sirket { get; set; }
}
