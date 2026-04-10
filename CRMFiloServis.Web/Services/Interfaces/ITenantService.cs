using CRMFiloServis.Shared.Entities;

namespace CRMFiloServis.Web.Services.Interfaces;

/// <summary>
/// Multi-tenant (çoklu şirket) yönetimi için servis interface'i
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Mevcut oturumun şirket ID'sini döner
    /// Null dönerse sistem admin (tüm şirketlere erişim)
    /// </summary>
    int? CurrentSirketId { get; }

    /// <summary>
    /// Mevcut oturumun şirket bilgisini döner
    /// </summary>
    Sirket? CurrentSirket { get; }

    /// <summary>
    /// Kullanıcının belirtilen şirkete erişim yetkisi var mı?
    /// </summary>
    bool HasAccessToSirket(int sirketId);

    /// <summary>
    /// Kullanıcı sistem admin mi? (tüm şirketlere erişebilir)
    /// </summary>
    bool IsSuperAdmin { get; }

    /// <summary>
    /// Şirket context'ini değiştirir (sadece super admin için)
    /// </summary>
    Task SetCurrentSirketAsync(int? sirketId);

    /// <summary>
    /// Tüm şirketleri listeler (sadece super admin için)
    /// </summary>
    Task<List<Sirket>> GetAllSirketlerAsync();

    /// <summary>
    /// Şirket ID'sine göre şirket bilgisi döner
    /// </summary>
    Task<Sirket?> GetSirketByIdAsync(int id);

    /// <summary>
    /// Yeni şirket oluşturur
    /// </summary>
    Task<Sirket> CreateSirketAsync(SirketOlusturModel model);

    /// <summary>
    /// Şirket bilgilerini günceller
    /// </summary>
    Task<Sirket> UpdateSirketAsync(SirketGuncelleModel model);

    /// <summary>
    /// Şirketi siler (soft delete)
    /// </summary>
    Task DeleteSirketAsync(int id);

    /// <summary>
    /// Şirket kodunun benzersiz olup olmadığını kontrol eder
    /// </summary>
    Task<bool> IsSirketKoduUniqueAsync(string sirketKodu, int? excludeId = null);

    #region Şirketler Arası Transfer

    /// <summary>
    /// Entity'leri başka şirkete transfer eder
    /// </summary>
    Task<SirketTransferResult> TransferAsync(SirketTransferRequest request);

    /// <summary>
    /// Transfer öncesi etkilenecek entity'lerin özetini döner
    /// </summary>
    Task<List<TransferEntityOzet>> GetTransferOnizlemeAsync(string entityTuru, List<int> entityIdler);

    /// <summary>
    /// Transfer loglarını listeler
    /// </summary>
    Task<List<SirketTransferLog>> GetTransferLoglariAsync(int? sirketId = null, DateTime? baslangic = null, DateTime? bitis = null);

    /// <summary>
    /// Transfer log detayını döner
    /// </summary>
    Task<SirketTransferLog?> GetTransferLogByIdAsync(int id);

    #endregion
}

/// <summary>
/// Yeni şirket oluşturma modeli
/// </summary>
public class SirketOlusturModel
{
    public required string SirketKodu { get; set; }
    public required string Unvan { get; set; }
    public string? KisaAd { get; set; }
    public string? VergiDairesi { get; set; }
    public string? VergiNo { get; set; }
    public string? Adres { get; set; }
    public string? Il { get; set; }
    public string? Ilce { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? WebSitesi { get; set; }
    public string ParaBirimi { get; set; } = "TRY";
    public int MaxKullaniciSayisi { get; set; } = 5;
}

/// <summary>
/// Şirket güncelleme modeli
/// </summary>
public class SirketGuncelleModel
{
    public int Id { get; set; }
    public required string Unvan { get; set; }
    public string? KisaAd { get; set; }
    public string? VergiDairesi { get; set; }
    public string? VergiNo { get; set; }
    public string? Adres { get; set; }
    public string? Il { get; set; }
    public string? Ilce { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? WebSitesi { get; set; }
    public string? LogoUrl { get; set; }
    public string ParaBirimi { get; set; } = "TRY";
    public bool Aktif { get; set; } = true;
    public int MaxKullaniciSayisi { get; set; } = 5;
}

/// <summary>
/// Şirketler arası transfer isteği
/// </summary>
public class SirketTransferRequest
{
    /// <summary>
    /// Transfer edilecek entity türü
    /// </summary>
    public required string EntityTuru { get; set; }

    /// <summary>
    /// Transfer edilecek entity ID'leri
    /// </summary>
    public required List<int> EntityIdler { get; set; }

    /// <summary>
    /// Hedef şirket ID'si
    /// </summary>
    public int HedefSirketId { get; set; }

    /// <summary>
    /// İlişkili veriler de transfer edilsin mi?
    /// (Örn: Cari transferinde faturalar da transfer edilsin mi?)
    /// </summary>
    public bool IliskiliVerileriTransferEt { get; set; }

    /// <summary>
    /// Transfer notu
    /// </summary>
    public string? Notlar { get; set; }
}

/// <summary>
/// Şirketler arası transfer sonucu
/// </summary>
public class SirketTransferResult
{
    public bool Basarili { get; set; }
    public int TransferEdilenSayisi { get; set; }
    public int BasarisizSayisi { get; set; }
    public int IliskiliEntitySayisi { get; set; }
    public List<string> Hatalar { get; set; } = [];
    public List<string> Uyarilar { get; set; } = [];
    public List<int> TransferLogIdler { get; set; } = [];
}

/// <summary>
/// Transfer edilebilir entity özeti
/// </summary>
public class TransferEntityOzet
{
    public string EntityTuru { get; set; } = "";
    public int Id { get; set; }
    public string Aciklama { get; set; } = "";
    public int? MevcutSirketId { get; set; }
    public string? MevcutSirketAdi { get; set; }
    public int IliskiliEntitySayisi { get; set; }
}
