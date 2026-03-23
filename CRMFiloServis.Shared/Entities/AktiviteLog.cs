using System.ComponentModel.DataAnnotations;

namespace CRMFiloServis.Shared.Entities;

/// <summary>
/// Sistem aktivite logu - tüm önemli iþlemlerin kaydý
/// </summary>
public class AktiviteLog : BaseEntity
{
    [Required]
    public DateTime IslemZamani { get; set; } = DateTime.Now;

    [Required]
    public string IslemTipi { get; set; } = string.Empty; // Ekleme, Güncelleme, Silme, Giriþ, Çýkýþ

    [Required]
    public string Modul { get; set; } = string.Empty; // Cari, Araç, Fatura, vb.

    public string? EntityTipi { get; set; } // Entity sýnýf adý

    public int? EntityId { get; set; }

    public string? EntityAdi { get; set; } // Cari adý, Plaka, vb.

    public string? Aciklama { get; set; }

    public string? EskiDeger { get; set; } // JSON formatýnda

    public string? YeniDeger { get; set; } // JSON formatýnda

    public string? KullaniciAdi { get; set; }

    public string? IpAdresi { get; set; }

    public string? Tarayici { get; set; }

    public AktiviteSeviye Seviye { get; set; } = AktiviteSeviye.Bilgi;
}

public enum AktiviteSeviye
{
    Bilgi = 1,
    Uyari = 2,
    Hata = 3,
    Kritik = 4
}
