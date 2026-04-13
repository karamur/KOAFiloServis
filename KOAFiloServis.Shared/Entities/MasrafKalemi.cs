namespace KOAFiloServis.Shared.Entities;

/// <summary>
/// Masraf kalemleri tan�mlar�
/// </summary>
public class MasrafKalemi : BaseEntity
{
    public string MasrafKodu { get; set; } = string.Empty;
    public string MasrafAdi { get; set; } = string.Empty;
    public MasrafKategori Kategori { get; set; }
    public bool Aktif { get; set; } = true;
    public string? Notlar { get; set; }

    // Navigation Properties
    public virtual ICollection<AracMasraf> AracMasraflari { get; set; } = new List<AracMasraf>();
}

public enum MasrafKategori
{
    Yakit = 1,
    Bakim = 2,
    Tamir = 3,
    Sigorta = 4,
    Vergi = 5,
    Personel = 6,        // Taksi, ula��m fi�leri vb.
    Lastik = 7,
    YedekParca = 8,
    Diger = 99
}
