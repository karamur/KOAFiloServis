using KOAFiloServis.Shared.Entities;

namespace KOAFiloServis.Tests.Entities;

public class BordroTests
{
    [Fact]
    public void GenelToplam_NetMaasVeEkOdemeToplami()
    {
        var bordro = new Bordro
        {
            ToplamNetMaas = 50000m,
            ToplamEkOdeme = 10000m
        };
        Assert.Equal(60000m, bordro.GenelToplam);
    }

    [Fact]
    public void DonemeAdi_DogruFormat()
    {
        var bordro = new Bordro { Ay = 3, Yil = 2025 };
        Assert.Equal("3/2025", bordro.DonemeAdi);
    }
}

public class BordroDetayTests
{
    [Fact]
    public void ToplamKesinti_TumKesintilerinToplami()
    {
        var detay = new BordroDetay
        {
            SgkIssizlikKesinti = 1000m,
            GelirVergisi = 500m,
            DamgaVergisi = 100m
        };
        Assert.Equal(1600m, detay.ToplamKesinti);
    }

    [Fact]
    public void ToplamKesinti_OzelKesintilerDahil()
    {
        var detay = new BordroDetay
        {
            SgkIssizlikKesinti = 1000m,
            GelirVergisi = 500m,
            DamgaVergisi = 100m,
            IcraKesintisi = 200m,
            BESKesintisi = 150m,
            SendikaKesintisi = 50m
        };
        // 1000 + 500 + 100 + (200+150+50+0+0+0) = 2000
        Assert.Equal(2000m, detay.ToplamKesinti);
    }

    [Fact]
    public void OzelKesintilerToplam_HepsiBirlesiyor()
    {
        var detay = new BordroDetay
        {
            IcraKesintisi = 300m,
            BESKesintisi = 200m,
            SendikaKesintisi = 100m,
            HayatSigortasi = 150m,
            BireyselEmeklilik = 250m,
            DigerOzelKesinti = 50m
        };
        Assert.Equal(1050m, detay.OzelKesintilerToplam);
    }

    [Fact]
    public void IsverenMaliyeti_SgkVeIssizlik()
    {
        var detay = new BordroDetay
        {
            SgkIsverenPrim = 3000m,
            IssizlikIsverenPrim = 400m
        };
        Assert.Equal(3400m, detay.ToplamIsverenMaliyet);
    }

    [Fact]
    public void ToplamIsverenMaliyetDahilMaas_BrutArtIsverenMaliyet()
    {
        var detay = new BordroDetay
        {
            BrutMaas = 20000m,
            SgkIsverenPrim = 3000m,
            IssizlikIsverenPrim = 400m
        };
        Assert.Equal(23400m, detay.ToplamIsverenMaliyetDahilMaas);
    }

    [Fact]
    public void ToplamEkOdeme_SosyalYardimlarDahil()
    {
        var detay = new BordroDetay
        {
            YemekYardimi = 200m,
            YolYardimi = 150m,
            PrimTutar = 500m,
            AileYardimi = 300m,
            Ikramiye = 1000m,
            DigerEkOdeme = 100m
        };
        Assert.Equal(2250m, detay.ToplamEkOdeme);
    }

    [Fact]
    public void ToplamOdenecek_NetMaasArtEkOdemeArtEkodeme()
    {
        var detay = new BordroDetay
        {
            NetMaas = 10000m,
            EkOdeme = 5000m,
            YemekYardimi = 200m,
            YolYardimi = 100m,
            PrimTutar = 0m,
            DigerEkOdeme = 0m
        };
        // ToplamOdenecek = NetMaas + ToplamEkOdeme + EkOdeme
        // = 10000 + (200+100+0+0) + 5000 = 15300
        Assert.Equal(15300m, detay.ToplamOdenecek);
    }

    [Fact]
    public void ToplamKesinti_SifirDegerlerle_SifirDoner()
    {
        var detay = new BordroDetay();
        Assert.Equal(0m, detay.ToplamKesinti);
    }
}

public class NettenBruteHesapSonucuTests
{
    [Fact]
    public void SgkIsciToplam_DogruHesaplanir()
    {
        var sonuc = new NettenBruteHesapSonucu
        {
            SgkIsciPrim = 1400m,
            IssizlikIsciPrim = 100m
        };
        Assert.Equal(1500m, sonuc.SgkIsciToplam);
    }

    [Fact]
    public void ToplamKesinti_DogruHesaplanir()
    {
        var sonuc = new NettenBruteHesapSonucu
        {
            SgkIsciPrim = 1400m,
            IssizlikIsciPrim = 100m,
            GelirVergisi = 800m,
            DamgaVergisi = 76m
        };
        Assert.Equal(2376m, sonuc.ToplamKesinti);
    }

    [Fact]
    public void HesaplananNetMaas_BrutEksiKesintiler()
    {
        var sonuc = new NettenBruteHesapSonucu
        {
            HesaplananBrutMaas = 10000m,
            SgkIsciPrim = 1400m,
            IssizlikIsciPrim = 100m,
            GelirVergisi = 1275m,
            DamgaVergisi = 75.90m
        };
        Assert.Equal(10000m - 1400m - 100m - 1275m - 75.90m, sonuc.HesaplananNetMaas);
    }

    [Fact]
    public void ToplamIsverenMaliyet_DogruHesaplanir()
    {
        var sonuc = new NettenBruteHesapSonucu
        {
            SgkIsverenPrim = 1550m,
            IssizlikIsverenPrim = 200m
        };
        Assert.Equal(1750m, sonuc.ToplamIsverenMaliyet);
    }

    [Fact]
    public void ToplamMaliyet_BrutVeIsverenToplami()
    {
        var sonuc = new NettenBruteHesapSonucu
        {
            HesaplananBrutMaas = 10000m,
            SgkIsverenPrim = 1550m,
            IssizlikIsverenPrim = 200m
        };
        Assert.Equal(11750m, sonuc.ToplamMaliyet);
    }
}
