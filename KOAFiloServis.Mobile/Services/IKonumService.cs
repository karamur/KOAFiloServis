namespace KOAFiloServis.Mobile.Services;

/// <summary>
/// GPS konum servisi interface'i
/// </summary>
public interface IKonumService
{
    /// <summary>
    /// Konum takibini başlat
    /// </summary>
    Task<bool> TakipBaslatAsync();
    
    /// <summary>
    /// Konum takibini durdur
    /// </summary>
    Task DurdurAsync();
    
    /// <summary>
    /// Mevcut konumu al
    /// </summary>
    Task<KonumBilgisi?> MevcutKonumuAlAsync();
    
    /// <summary>
    /// Konum izni var mı?
    /// </summary>
    Task<bool> KonumIzniVarMiAsync();
    
    /// <summary>
    /// Konum izni iste
    /// </summary>
    Task<bool> KonumIzniIsteAsync();
    
    /// <summary>
    /// Konum değişti event'i
    /// </summary>
    event EventHandler<KonumBilgisi>? KonumDegisti;
    
    /// <summary>
    /// Takip aktif mi?
    /// </summary>
    bool TakipAktif { get; }
}

/// <summary>
/// Konum bilgisi DTO
/// </summary>
public class KonumBilgisi
{
    public double Enlem { get; set; }
    public double Boylam { get; set; }
    public double? Hiz { get; set; } // km/saat
    public double? Yon { get; set; } // derece (0-360)
    public double? Yukseklik { get; set; } // metre
    public double? Hassasiyet { get; set; } // metre
    public DateTime Zaman { get; set; }
}
