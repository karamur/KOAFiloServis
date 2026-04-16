using KOAFiloServis.Shared.Entities;

namespace KOAFiloServis.Tests.Entities;

public class ProformaFaturaTests
{
    [Fact]
    public void YeniProforma_VarsayilanDegerler_Dogru()
    {
        var pf = new ProformaFatura();
        Assert.Equal(ProformaDurum.Taslak, pf.Durum);
        Assert.Equal(20, pf.KdvOrani);
        Assert.Equal(0, pf.IskontoTutar);
        Assert.Equal(0, pf.IskontoOrani);
        Assert.False(pf.FaturayaDonusturuldu);
        Assert.Empty(pf.ProformaNo);
        Assert.NotNull(pf.Kalemler);
        Assert.Empty(pf.Kalemler);
    }

    [Fact]
    public void ProformaFaturaKalem_VarsayilanDegerler_Dogru()
    {
        var kalem = new ProformaFaturaKalem();
        Assert.Equal(1, kalem.Miktar);
        Assert.Equal("Adet", kalem.Birim);
        Assert.Equal(20, kalem.KdvOrani);
        Assert.Equal(0, kalem.IskontoOrani);
        Assert.Equal(0, kalem.IskontoTutar);
        Assert.Empty(kalem.UrunAdi);
    }

    [Fact]
    public void ProformaDurum_TumDegerler_Mevcut()
    {
        Assert.Equal(1, (int)ProformaDurum.Taslak);
        Assert.Equal(2, (int)ProformaDurum.Gonderildi);
        Assert.Equal(3, (int)ProformaDurum.Onaylandi);
        Assert.Equal(4, (int)ProformaDurum.Reddedildi);
        Assert.Equal(5, (int)ProformaDurum.FaturayaDonusturuldu);
        Assert.Equal(6, (int)ProformaDurum.SuresiDoldu);
    }

    [Fact]
    public void ProformaFatura_KalemEkleme_CollectionaEklenir()
    {
        var pf = new ProformaFatura { ProformaNo = "PF-2025-001" };
        var kalem = new ProformaFaturaKalem
        {
            SiraNo = 1,
            UrunAdi = "Servis Hizmeti",
            Miktar = 22,
            Birim = "Gün",
            BirimFiyat = 500,
            AraToplam = 11000,
            KdvOrani = 20,
            KdvTutar = 2200,
            NetTutar = 11000,
            ToplamTutar = 13200
        };
        pf.Kalemler.Add(kalem);

        Assert.Single(pf.Kalemler);
        Assert.Equal(13200, pf.Kalemler.First().ToplamTutar);
    }

    [Fact]
    public void ProformaFatura_FaturaDonusum_Alanlari_Dogru()
    {
        var pf = new ProformaFatura
        {
            FaturayaDonusturuldu = true,
            FaturaId = 42,
            FaturaDonusumTarihi = new DateTime(2025, 6, 15)
        };

        Assert.True(pf.FaturayaDonusturuldu);
        Assert.Equal(42, pf.FaturaId);
        Assert.Equal(new DateTime(2025, 6, 15), pf.FaturaDonusumTarihi);
    }

    [Fact]
    public void ProformaFatura_OdemeKosullari_Atanabilir()
    {
        var pf = new ProformaFatura
        {
            OdemeKosulu = "30 gün vadeli",
            TeslimKosulu = "Kapıda teslim",
            VadeGun = 30
        };

        Assert.Equal("30 gün vadeli", pf.OdemeKosulu);
        Assert.Equal("Kapıda teslim", pf.TeslimKosulu);
        Assert.Equal(30, pf.VadeGun);
    }
}
