using KOAFiloServis.Shared.Entities;

namespace KOAFiloServis.Web.Services;

public interface ILisansService
{
    // Lisans Yonetimi
    Task<Lisans?> GetAktifLisansAsync();
    Task<bool> LisansGecerliMiAsync();
    Task<Lisans> AktiveLisansAsync(string lisansAnahtari);
    Task<int> KalanGunAsync();
    Task<string> GetMakineKoduAsync();

    // Trial
    Task<Lisans> OlusturTrialLisansAsync();

    // Kontroller
    Task<bool> KullanicLimitiKontrolAsync();
    Task<bool> ModulIzniVarMiAsync(string modulAdi);
}
