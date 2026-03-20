namespace CRMFiloServis.Shared.Entities;

/// <summary>
/// Aylýk Checklist - Þoför, Araç, Güzergah için aylýk kontrol listesi
/// </summary>
public class AylikChecklist : BaseEntity
{
    public int Yil { get; set; }
    public int Ay { get; set; }
    public ChecklistTipi ChecklistTipi { get; set; }

    // Ýlgili kayýt (Þoför, Araç veya Güzergah)
    public int? SoforId { get; set; }
    public int? AracId { get; set; }
    public int? GuzergahId { get; set; }

    public DateTime? KontrolTarihi { get; set; }
    public string? KontrolEden { get; set; }
    public ChecklistDurum GenelDurum { get; set; } = ChecklistDurum.Bekliyor;
    public string? Notlar { get; set; }

    // Navigation Properties
    public virtual Sofor? Sofor { get; set; }
    public virtual Arac? Arac { get; set; }
    public virtual Guzergah? Guzergah { get; set; }
    public virtual ICollection<ChecklistKalem> Kalemler { get; set; } = new List<ChecklistKalem>();
}

/// <summary>
/// Checklist Kalemi - Her bir kontrol maddesi
/// </summary>
public class ChecklistKalem : BaseEntity
{
    public int AylikChecklistId { get; set; }
    public string KalemAdi { get; set; } = string.Empty;
    public ChecklistDurum Durum { get; set; } = ChecklistDurum.Bekliyor;
    public DateTime? SonGecerlilikTarihi { get; set; }
    public DateTime? KontrolTarihi { get; set; }
    public string? Aciklama { get; set; }
    public int SiraNo { get; set; }

    // Navigation
    public virtual AylikChecklist AylikChecklist { get; set; } = null!;
}

public enum ChecklistTipi
{
    Sofor = 1,
    Arac = 2,
    Guzergah = 3
}

public enum ChecklistDurum
{
    Bekliyor = 0,
    Tamam = 1,
    Uyari = 2,
    Kritik = 3
}
