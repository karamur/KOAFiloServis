namespace CRMFiloServis.Mobile.Services;

/// <summary>
/// API iletişim servisi interface'i
/// </summary>
public interface IApiService
{
    /// <summary>
    /// Kullanıcı girişi
    /// </summary>
    Task<GirisYanit?> GirisYapAsync(string kullaniciAdi, string sifre);
    
    /// <summary>
    /// Oturum kapatma
    /// </summary>
    Task CikisYapAsync();
    
    /// <summary>
    /// Mevcut kullanıcı bilgisi
    /// </summary>
    Task<KullaniciBilgisi?> KullaniciBilgisiGetirAsync();
    
    /// <summary>
    /// Şoföre atanmış araçları getir
    /// </summary>
    Task<List<AracOzet>> SoforAraclariniGetirAsync();
    
    /// <summary>
    /// Aktif seferleri getir
    /// </summary>
    Task<List<SeferOzet>> AktifSeferleriGetirAsync();
    
    /// <summary>
    /// Sefer başlat
    /// </summary>
    Task<SeferDetay?> SeferBaslatAsync(SeferBaslatRequest request);
    
    /// <summary>
    /// Sefer bitir
    /// </summary>
    Task<SeferDetay?> SeferBitirAsync(int seferId, SeferBitirRequest request);
    
    /// <summary>
    /// Konum gönder
    /// </summary>
    Task<bool> KonumGonderAsync(KonumGonderRequest request);
    
    /// <summary>
    /// Arıza bildirimi gönder
    /// </summary>
    Task<bool> ArizaBildirAsync(ArizaBildirimRequest request);
    
    /// <summary>
    /// Masraf girişi yap
    /// </summary>
    Task<bool> MasrafKaydetAsync(MasrafKayitRequest request);
    
    /// <summary>
    /// Masraf kalemleri listesi
    /// </summary>
    Task<List<MasrafKalemiOzet>> MasrafKalemleriGetirAsync();

    /// <summary>
    /// Güzergah listesi
    /// </summary>
    Task<List<GuzergahOzet>> GuzergahlariGetirAsync();

    /// <summary>
    /// Sefer geçmişini getir
    /// </summary>
    Task<List<SeferOzet>> SeferGecmisiniGetirAsync();

    /// <summary>
    /// Belirli bir seferi getir
    /// </summary>
    Task<SeferOzet?> SeferGetirAsync(int seferId);

    /// <summary>
    /// Sefer bitir (basitleştirilmiş model)
    /// </summary>
    Task<bool> SeferBitirAsync(object model);

    /// <summary>
    /// Bağlantıyı test et
    /// </summary>
    Task<bool> BaglantiyiTestEtAsync();

    /// <summary>
    /// Kaydedilmiş sunucu adresini getir
    /// </summary>
    Task<string> GetSunucuAdresiAsync();

    /// <summary>
    /// Sunucu adresini kaydet ve kullan
    /// </summary>
    Task SetSunucuAdresiAsync(string sunucuAdresi);

    /// <summary>
    /// Token'ı başlangıçta yükle
    /// </summary>
    Task TokenYukleAsync();

    /// <summary>
    /// JWT token geçerli mi kontrol et
    /// </summary>
    bool TokenGecerliMi { get; }
}

#region DTO Sınıfları

public class GirisYanit
{
    public bool Basarili { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? TokenSonlanma { get; set; }
    public string? Mesaj { get; set; }
    public KullaniciBilgisi? Kullanici { get; set; }
}

public class KullaniciBilgisi
{
    public int Id { get; set; }
    public string KullaniciAdi { get; set; } = string.Empty;
    public string AdSoyad { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefon { get; set; }
    public string? Rol { get; set; }
    public int? SoforId { get; set; }
    public string? ProfilResmi { get; set; }
}

public class AracOzet
{
    public int Id { get; set; }
    public string Plaka { get; set; } = string.Empty;
    public string? Marka { get; set; }
    public string? Model { get; set; }
    public int? KmDurumu { get; set; }
    public decimal? YakitSeviyesi { get; set; }
    public bool Aktif { get; set; }
}

public class SeferOzet
{
    public int Id { get; set; }
    public int AracId { get; set; }
    public string AracPlaka { get; set; } = string.Empty;
    public int GuzergahId { get; set; }
    public string GuzergahAdi { get; set; } = string.Empty;
    public DateTime BaslangicZamani { get; set; }
    public DateTime? BitisZamani { get; set; }
    public string Durum { get; set; } = string.Empty; // DevamEdiyor, Tamamlandi, IptalEdildi
    public int? BaslangicKm { get; set; }
    public int? BitisKm { get; set; }
    public bool Tamamlandi => Durum == "Tamamlandi" || BitisZamani.HasValue;
    public decimal ToplamKm => (BitisKm ?? 0) - (BaslangicKm ?? 0);
}

public class GuzergahOzet
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? BaslangicNoktasi { get; set; }
    public string? BitisNoktasi { get; set; }
    public decimal? Mesafe { get; set; }
}

public class SeferDetay : SeferOzet
{
    public string? Notlar { get; set; }
    public decimal? YakitTuketimi { get; set; }
    public List<SeferKonumKaydi> KonumGecmisi { get; set; } = new();
}

public class SeferKonumKaydi
{
    public double Enlem { get; set; }
    public double Boylam { get; set; }
    public DateTime Zaman { get; set; }
    public double? Hiz { get; set; }
}

public class SeferBaslatRequest
{
    public int AracId { get; set; }
    public int GuzergahId { get; set; }
    public int BaslangicKm { get; set; }
    public double? BaslangicEnlem { get; set; }
    public double? BaslangicBoylam { get; set; }
    public string? Notlar { get; set; }
}

public class SeferBitirRequest
{
    public int BitisKm { get; set; }
    public double? BitisEnlem { get; set; }
    public double? BitisBoylam { get; set; }
    public decimal? YakitTuketimi { get; set; }
    public string? Notlar { get; set; }
}

public class KonumGonderRequest
{
    public int? SeferId { get; set; }
    public int? AracId { get; set; }
    public double Enlem { get; set; }
    public double Boylam { get; set; }
    public double? Hiz { get; set; }
    public double? Yon { get; set; }
    public bool? KontakDurumu { get; set; }
    public bool? MotorDurumu { get; set; }
    public decimal? YakitSeviyesi { get; set; }
}

public class ArizaBildirimRequest
{
    public int AracId { get; set; }
    public int? SeferId { get; set; }
    public string ArizaTipi { get; set; } = string.Empty; // Motor, Lastik, Fren, Elektrik, Diger
    public string Aciklama { get; set; } = string.Empty;
    public string? OncelikSeviyesi { get; set; } // Dusuk, Orta, Yuksek, Acil
    public double? Enlem { get; set; }
    public double? Boylam { get; set; }
    public List<string>? FotografBase64 { get; set; }
}

public class MasrafKayitRequest
{
    public int AracId { get; set; }
    public int MasrafKalemiId { get; set; }
    public decimal Tutar { get; set; }
    public DateTime Tarih { get; set; }
    public string? Aciklama { get; set; }
    public int? KmDurumu { get; set; }
    public double? Enlem { get; set; }
    public double? Boylam { get; set; }
    public string? FisGorseliBase64 { get; set; }
}

public class MasrafKalemiOzet
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? Kategori { get; set; }
    public string? Ikon { get; set; }
}

#endregion
