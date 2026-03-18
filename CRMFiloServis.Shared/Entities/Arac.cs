namespace CRMFiloServis.Shared.Entities;

/// <summary>
/// Araç bilgileri
/// </summary>
public class Arac : BaseEntity
{
    public string Plaka { get; set; } = string.Empty;
    public string? Marka { get; set; }
    public string? Model { get; set; }
    public int? ModelYili { get; set; }
    public string? SaseNo { get; set; }
    public string? MotorNo { get; set; }
    public string? Renk { get; set; }
    public int KoltukSayisi { get; set; }
    public AracTipi AracTipi { get; set; }
    public DateTime? TrafikSigortaBitisTarihi { get; set; }
    public DateTime? KaskoBitisTarihi { get; set; }
    public DateTime? MuayeneBitisTarihi { get; set; }
    public int? KmDurumu { get; set; }
    public bool Aktif { get; set; } = true;
    public string? Notlar { get; set; }

    // Navigation Properties
    public virtual ICollection<AracMasraf> Masraflar { get; set; } = new List<AracMasraf>();
    public virtual ICollection<ServisCalisma> ServisCalismalari { get; set; } = new List<ServisCalisma>();
}

public enum AracTipi
{
    Minibus = 1,
    Midibus = 2,
    Otobus = 3,
    Otomobil = 4,
    Panelvan = 5
}
