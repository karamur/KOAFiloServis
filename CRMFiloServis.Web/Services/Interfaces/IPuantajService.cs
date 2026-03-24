using CRMFiloServis.Shared.Entities;

namespace CRMFiloServis.Web.Services;

public interface IPuantajService
{
    // Puantaj Ýþlemleri
    Task<List<PersonelPuantaj>> GetAylikPuantajAsync(int firmaId, int yil, int ay);
    Task<PersonelPuantaj?> GetPuantajByIdAsync(int id);
    Task<PersonelPuantaj?> GetPersonelAylikPuantajAsync(int personelId, int yil, int ay);
    Task<PersonelPuantaj> CreateOrUpdatePuantajAsync(PersonelPuantaj puantaj);
    Task DeletePuantajAsync(int id);

    // Günlük Puantaj
    Task<List<GunlukPuantaj>> GetGunlukPuantajlarAsync(int puantajId);
    Task<GunlukPuantaj> SaveGunlukPuantajAsync(GunlukPuantaj gunluk);
    Task OtomatikGunlukPuantajOlusturAsync(int puantajId, int yil, int ay);

    // Hesaplamalar
    Task<PersonelPuantaj> HesaplaAsync(int puantajId);
    Task<decimal> ToplamBrutMaasHesaplaAsync(int firmaId, int yil, int ay);
    Task<decimal> ToplamNetOdemeHesaplaAsync(int firmaId, int yil, int ay);

    // Excel Export
    Task<byte[]> ExportPuantajListesiAsync(int firmaId, int yil, int ay);
    Task<byte[]> ExportVakifbankOdemeListesiAsync(int firmaId, int yil, int ay);

    // Ýstatistikler
    Task<int> GetToplamPersonelSayisiAsync(int firmaId);
    Task<Dictionary<int, decimal>> GetAylikMaasGrafigiAsync(int firmaId, int yil);
}
