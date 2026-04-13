namespace KOAFiloServis.Shared.Entities;

/// <summary>
/// Araç masraf girişleri
/// </summary>
public class AracMasraf : BaseEntity
{
    public DateTime MasrafTarihi { get; set; }
    public decimal Tutar { get; set; }
    public string? Aciklama { get; set; }
    public string? BelgeNo { get; set; } // Fatura/Fiş numarası
    public bool ArizaKaynaklimi { get; set; } = false; // Arıza nedeniyle mi?

    // Foreign Keys
    public int AracId { get; set; }
    public int MasrafKalemiId { get; set; }
    public int? GuzergahId { get; set; } // Arıza kaynaklı personel ulaşım masrafları için
    public int? ServisCalismaId { get; set; } // İlgili servis çalışması
    public int? SoforId { get; set; }
    public int? CariId { get; set; }
    public int? MuhasebeFisId { get; set; }

    // Navigation Properties
    public virtual Arac Arac { get; set; } = null!;
    public virtual MasrafKalemi MasrafKalemi { get; set; } = null!;
    public virtual Guzergah? Guzergah { get; set; }
    public virtual ServisCalisma? ServisCalisma { get; set; }
    public virtual Sofor? Sofor { get; set; }
    public virtual Cari? Cari { get; set; }
    public virtual MuhasebeFis? MuhasebeFis { get; set; }
}
