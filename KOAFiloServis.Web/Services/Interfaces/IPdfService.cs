using KOAFiloServis.Shared.Entities;

namespace KOAFiloServis.Web.Services;

public interface IPdfService
{
    byte[] GenerateFaturaPdf(Fatura fatura);
    byte[] GenerateServisCalismaRaporuPdf(List<ServisCalisma> calismalar, DateTime baslangic, DateTime bitis);
    byte[] GenerateBelgeUyariRaporuPdf(List<BelgeUyari> uyarilar);
    byte[] GenerateCariEkstresPdf(Cari cari, List<Fatura> faturalar, List<BankaKasaHareket> hareketler);
}
