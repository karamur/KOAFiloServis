using System.ComponentModel.DataAnnotations;

namespace KOAFiloServis.Shared.Entities;

/// <summary>
/// Sistem aktivite logu - t�m �nemli i�lemlerin kayd�
/// </summary>
public class AktiviteLog : BaseEntity
{
    [Required]
    public DateTime IslemZamani { get; set; } = DateTime.Now;

    [Required]
    public string IslemTipi { get; set; } = string.Empty; // Ekleme, G�ncelleme, Silme, Giri�, ��k��

    [Required]
    public string Modul { get; set; } = string.Empty; // Cari, Ara�, Fatura, vb.

    public string? EntityTipi { get; set; } // Entity s�n�f ad�

    public int? EntityId { get; set; }

    public string? EntityAdi { get; set; } // Cari ad�, Plaka, vb.

    public string? Aciklama { get; set; }

    public string? EskiDeger { get; set; } // JSON format�nda

    public string? YeniDeger { get; set; } // JSON format�nda

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
