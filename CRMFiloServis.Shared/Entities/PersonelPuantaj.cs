using System.ComponentModel.DataAnnotations;

namespace CRMFiloServis.Shared.Entities;

/// <summary>
/// Personel puantaj kaydý
/// </summary>
public class PersonelPuantaj : BaseEntity
{
    [Required]
    public int FirmaId { get; set; }

    [Required]
    public int PersonelId { get; set; } // Sofor veya diger personel

    [Required]
    public int Yil { get; set; }

    [Required]
    [Range(1, 12)]
    public int Ay { get; set; }

    /// <summary>
    /// Çalýþýlan gün sayýsý
    /// </summary>
    public int CalisilanGun { get; set; }

    /// <summary>
    /// Fazla mesai saati
    /// </summary>
    public decimal FazlaMesaiSaat { get; set; }

    /// <summary>
    /// Ýzin günü
    /// </summary>
    public int IzinGunu { get; set; }

    /// <summary>
    /// Mazeret/Rapor günü
    /// </summary>
    public int MazeretGunu { get; set; }

    /// <summary>
    /// Brüt maaþ
    /// </summary>
    public decimal BrutMaas { get; set; }

    /// <summary>
    /// Yemek ücreti
    /// </summary>
    public decimal YemekUcreti { get; set; }

    /// <summary>
    /// Yol ücreti
    /// </summary>
    public decimal YolUcreti { get; set; }

    /// <summary>
    /// Prim
    /// </summary>
    public decimal Prim { get; set; }

    /// <summary>
    /// Diðer ödeme
    /// </summary>
    public decimal DigerOdeme { get; set; }

    /// <summary>
    /// SGK kesintisi
    /// </summary>
    public decimal SgkKesinti { get; set; }

    /// <summary>
    /// Gelir vergisi
    /// </summary>
    public decimal GelirVergisi { get; set; }

    /// <summary>
    /// Damga vergisi
    /// </summary>
    public decimal DamgaVergisi { get; set; }

    /// <summary>
    /// Diðer kesintiler
    /// </summary>
    public decimal DigerKesinti { get; set; }

    /// <summary>
    /// Net ödeme
    /// </summary>
    public decimal NetOdeme { get; set; }

    /// <summary>
    /// Ödeme tarihi
    /// </summary>
    public DateTime? OdemeTarihi { get; set; }

    /// <summary>
    /// Ödeme durumu
    /// </summary>
    public bool Odendi { get; set; }

    /// <summary>
    /// Banka hesap numarasý (IBAN)
    /// </summary>
    public string? BankaHesapNo { get; set; }

    public string? Aciklama { get; set; }

    // Navigation
    public virtual Firma? Firma { get; set; }
    public virtual Sofor? Personel { get; set; }
}

/// <summary>
/// Günlük puantaj detayý
/// </summary>
public class GunlukPuantaj : BaseEntity
{
    [Required]
    public int PersonelPuantajId { get; set; }

    [Required]
    public DateTime Tarih { get; set; }

    /// <summary>
    /// Çalýþtý mý?
    /// </summary>
    public bool Calisti { get; set; }

    /// <summary>
    /// Fazla mesai saati
    /// </summary>
    public decimal? FazlaMesaiSaat { get; set; }

    /// <summary>
    /// Ýzinli mi?
    /// </summary>
    public bool Izinli { get; set; }

    /// <summary>
    /// Mazeret/Rapor
    /// </summary>
    public bool Mazeret { get; set; }

    /// <summary>
    /// Çalýþtýðý güzergah/sefer
    /// </summary>
    public int? ServisCalismaId { get; set; }

    public string? Notlar { get; set; }

    // Navigation
    public virtual PersonelPuantaj? PersonelPuantaj { get; set; }
    public virtual ServisCalisma? ServisCalisma { get; set; }
}
