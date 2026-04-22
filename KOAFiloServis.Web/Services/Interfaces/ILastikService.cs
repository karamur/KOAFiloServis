using KOAFiloServis.Shared.Entities;

namespace KOAFiloServis.Web.Services.Interfaces;

public interface ILastikService
{
    // --- Depo ---
    Task<List<LastikDepo>> GetDepoListAsync();
    Task<LastikDepo?> GetDepoByIdAsync(int id);
    Task<LastikDepo> CreateDepoAsync(LastikDepo depo);
    Task<LastikDepo> UpdateDepoAsync(LastikDepo depo);
    Task DeleteDepoAsync(int id);

    // --- Stok ---
    /// <summary>Bireysel lastikleri listeler. aktif=true → aktif, false → pasif, null → tümü</summary>
    Task<List<LastikStok>> GetStokListAsync(int? depoId = null, bool? aktif = true);
    Task<LastikStok?> GetStokByIdAsync(int id);
    Task<LastikStok> CreateStokAsync(LastikStok stok);
    Task<LastikStok> UpdateStokAsync(LastikStok stok);
    Task DeleteStokAsync(int id);
    /// <summary>Lastiği pasife alır (hurda / atıldı)</summary>
    Task PasifAlAsync(int id);

    // --- Değişim ---
    Task<List<LastikDegisim>> GetDegisimListAsync(int? aracId = null, DateTime? baslangic = null, DateTime? bitis = null);
    Task<LastikDegisim?> GetDegisimByIdAsync(int id);
    Task<LastikDegisim> CreateDegisimAsync(LastikDegisim degisim);
    Task<LastikDegisim> UpdateDegisimAsync(LastikDegisim degisim);
    Task DeleteDegisimAsync(int id);

    // --- Rapor / Araç Detay ---
    Task<List<LastikAracDonemOzet>> GetAracDonemOzetListAsync(DateTime? baslangic = null, DateTime? bitis = null);
    Task<LastikAracDetay?> GetAracDetayAsync(int aracId, DateTime? baslangic = null, DateTime? bitis = null);
}

public sealed class LastikAracDonemOzet
{
    public int AracId { get; set; }
    public string Plaka { get; set; } = string.Empty;
    public string AracBilgisi { get; set; } = string.Empty;
    public int DonemDegisimSayisi { get; set; }
    public bool DonemdeDegisti { get; set; }
    public int TakiliLastikSayisi { get; set; }
    public bool DortLastikAyniMi { get; set; }
    public string TakiliLastikOzeti { get; set; } = string.Empty;
}

public sealed class LastikAracHareketSatiri
{
    public int DegisimId { get; set; }
    public DateTime Tarih { get; set; }
    public LastikDegisimTipi DegisimTipi { get; set; }
    public int? KmDurumu { get; set; }
    public string TakilanAciklama { get; set; } = string.Empty;
    public string SokulenAciklama { get; set; } = string.Empty;
    public string YapilanYer { get; set; } = string.Empty;
    public decimal? Ucret { get; set; }
}

public sealed class LastikAracDetay
{
    public int AracId { get; set; }
    public string Plaka { get; set; } = string.Empty;
    public string AracBilgisi { get; set; } = string.Empty;
    public List<LastikAracPlakaSatiri> PlakaGecmisi { get; set; } = new();
    public List<LastikStok> TakiliLastikler { get; set; } = new();
    public List<LastikAracHareketSatiri> Hareketler { get; set; } = new();
}

public sealed class LastikAracPlakaSatiri
{
    public string Plaka { get; set; } = string.Empty;
    public DateTime GirisTarihi { get; set; }
    public DateTime? CikisTarihi { get; set; }
    public bool Aktif { get; set; }
}
