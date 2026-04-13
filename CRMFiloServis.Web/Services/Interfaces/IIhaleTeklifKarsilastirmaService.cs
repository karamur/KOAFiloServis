using CRMFiloServis.Web.Models;

namespace CRMFiloServis.Web.Services;

public interface IIhaleTeklifKarsilastirmaService
{
    Task<IhaleTeklifKarsilastirmaDto?> CompareAsync(int solVersiyonId, int sagVersiyonId);
}
