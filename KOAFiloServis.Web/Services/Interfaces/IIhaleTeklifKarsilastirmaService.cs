using KOAFiloServis.Web.Models;

namespace KOAFiloServis.Web.Services;

public interface IIhaleTeklifKarsilastirmaService
{
    Task<IhaleTeklifKarsilastirmaDto?> CompareAsync(int solVersiyonId, int sagVersiyonId);
}
