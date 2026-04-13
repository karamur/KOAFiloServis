using KOAFiloServis.Shared.Entities;

namespace KOAFiloServis.Web.Services;

public interface IStokService
{
    // Stok Karti
    Task<List<StokKarti>> GetStokKartlariAsync(StokTipi? tip = null, int? kategoriId = null, bool? aktif = true);
    Task<StokKarti?> GetStokKartiByIdAsync(int id);
    Task<StokKarti?> GetStokKartiByKodAsync(string kod);
    Task<StokKarti> CreateStokKartiAsync(StokKarti stok);
    Task<StokKarti> UpdateStokKartiAsync(StokKarti stok);
    Task DeleteStokKartiAsync(int id);
    Task<string> GetNextStokKoduAsync(StokTipi tip);

    // Stok Kategori
    Task<List<StokKategori>> GetKategorilerAsync(bool? aktif = true);
    Task<StokKategori?> GetKategoriByIdAsync(int id);
    Task<StokKategori> CreateKategoriAsync(StokKategori kategori);
    Task<StokKategori> UpdateKategoriAsync(StokKategori kategori);
    Task DeleteKategoriAsync(int id);

    // Stok Hareket
    Task<List<StokHareket>> GetStokHareketleriAsync(int? stokKartiId = null, DateTime? baslangic = null, DateTime? bitis = null);
    Task<StokHareket> CreateStokHareketAsync(StokHareket hareket);
    Task<StokHareket> CreateStokOperasyonAsync(StokOperasyonModel operasyon);
    Task CreateUretimRecetesiAsync(UretimReceteModel recete);
    Task UpdateStokMiktariAsync(int stokKartiId);
    Task<decimal> GetMevcutStokAsync(int stokKartiId);

    // Arac Islem (Alis/Satis)
    Task<List<AracIslem>> GetAracIslemleriAsync(int? aracId = null, AracIslemTipi? tip = null);
    Task<AracIslem?> GetAracIslemByIdAsync(int id);
    Task<AracIslem> CreateAracIslemAsync(AracIslem islem);
    Task<AracIslem> UpdateAracIslemAsync(AracIslem islem);
    Task DeleteAracIslemAsync(int id);

    // Servis Kaydi
    Task<List<ServisKaydi>> GetServisKayitlariAsync(int? aracId = null, ServisTipi? tip = null, DateTime? baslangic = null, DateTime? bitis = null);
    Task<ServisKaydi?> GetServisKaydiByIdAsync(int id);
    Task<ServisKaydi> CreateServisKaydiAsync(ServisKaydi servis);
    Task<ServisKaydi> UpdateServisKaydiAsync(ServisKaydi servis);
    Task DeleteServisKaydiAsync(int id);

    // Dashboard
    Task<StokDashboard> GetDashboardAsync();
}

public class StokDashboard
{
    public int ToplamStokKarti { get; set; }
    public int AktifStokKarti { get; set; }
    public int DusukStoklu { get; set; }
    public decimal ToplamStokDegeri { get; set; }
    public int AylikAracAlis { get; set; }
    public int AylikAracSatis { get; set; }
    public int AylikServisKaydi { get; set; }
    public decimal AylikServisTutari { get; set; }
    public List<StokHareket> SonHareketler { get; set; } = new();
    public List<ServisKaydi> SonServisler { get; set; } = new();
}

public class StokOperasyonModel
{
    public int StokKartiId { get; set; }
    public DateTime IslemTarihi { get; set; } = DateTime.Today;
    public StokHareketTipi HareketTipi { get; set; }
    public decimal Miktar { get; set; }
    public decimal BirimFiyat { get; set; }
    public string? BelgeNo { get; set; }
    public string? Aciklama { get; set; }
    public int? CariId { get; set; }
}

public class UretimReceteModel
{
    public int MamulStokKartiId { get; set; }
    public DateTime IslemTarihi { get; set; } = DateTime.Today;
    public decimal MamulMiktari { get; set; } = 1;
    public decimal MamulBirimMaliyeti { get; set; }
    public string? BelgeNo { get; set; }
    public string? Aciklama { get; set; }
    public List<UretimReceteKalemModel> Kalemler { get; set; } = new();
}

public class UretimReceteKalemModel
{
    public int StokKartiId { get; set; }
    public decimal Miktar { get; set; }
    public decimal BirimFiyat { get; set; }
}
