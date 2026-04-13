using KOAFiloServis.Shared.Entities;
using KOAFiloServis.Web.Models;

namespace KOAFiloServis.Web.Services;

public interface IFaturaHazirlikService
{
    Task<FaturaHazirlikListesi> GetFaturaHazirlikListesiAsync(DateTime baslangicTarihi, DateTime bitisTarihi);
    Task<FaturaHazirlikListesi> GetFaturaHazirlikListesiAsync(DateTime baslangicTarihi, DateTime bitisTarihi, int? cariId);
}
