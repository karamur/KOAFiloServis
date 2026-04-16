using KOAFiloServis.Shared.Entities;

namespace KOAFiloServis.Tests.Entities;

public class PuantajKayitTests
{
    [Fact]
    public void YeniPuantajKayit_VarsayilanDegerler_Dogru()
    {
        var pk = new PuantajKayit();
        Assert.Equal(PuantajYon.SabahAksam, pk.Yon);
        Assert.Equal(SoforOdemeTipi.Ozmal, pk.SoforOdemeTipi);
        Assert.Equal(20, pk.GelirKdvOrani);
        Assert.False(pk.GelirFaturaKesildi);
        Assert.False(pk.GiderFaturaAlindi);
        Assert.Equal(PuantajOdemeDurum.Odenmedi, pk.GelirOdemeDurumu);
        Assert.Equal(PuantajOdemeDurum.Odenmedi, pk.GiderOdemeDurumu);
        Assert.Equal(PuantajOnayDurum.Taslak, pk.OnayDurum);
        Assert.Equal(PuantajKaynak.Manuel, pk.Kaynak);
    }

    [Fact]
    public void SetGunDeger_VeGetGunDeger_DogruCalisir()
    {
        var pk = new PuantajKayit();
        pk.SetGunDeger(1, 2);
        pk.SetGunDeger(15, 1);
        pk.SetGunDeger(31, 2);

        Assert.Equal(2, pk.GetGunDeger(1));
        Assert.Equal(1, pk.GetGunDeger(15));
        Assert.Equal(2, pk.GetGunDeger(31));
        Assert.Equal(0, pk.GetGunDeger(2)); // atanmamış
    }

    [Fact]
    public void GetGunDeger_GecersizGun_SifirDoner()
    {
        var pk = new PuantajKayit();
        Assert.Equal(0, pk.GetGunDeger(0));
        Assert.Equal(0, pk.GetGunDeger(32));
        Assert.Equal(0, pk.GetGunDeger(-1));
    }

    [Fact]
    public void SeferGunuToplami_DogruHesaplanir()
    {
        var pk = new PuantajKayit();
        for (int i = 1; i <= 22; i++)
            pk.SetGunDeger(i, 1);

        Assert.Equal(22, pk.SeferGunuToplami);
    }

    [Fact]
    public void HesaplaGelir_BirimGelirVeGundan_Hesaplar()
    {
        var pk = new PuantajKayit
        {
            BirimGelir = 500,
            Gun = 22,
            GelirKdvOrani = 20
        };
        pk.HesaplaGelir();

        Assert.Equal(11000, pk.ToplamGelir);
        Assert.Equal(2200, pk.GelirKdvTutari);
        Assert.Equal(13200, pk.GelirToplam);
    }

    [Fact]
    public void HesaplaGelir_AyrintiliKdv_KdvOranindanHesaplamaz()
    {
        var pk = new PuantajKayit
        {
            BirimGelir = 1000,
            Gun = 10,
            GelirKdvOrani = 20,
            GelirKdv20Tutari = 1500, // manuel girilmiş
            GelirKdv10Tutari = 300
        };
        pk.HesaplaGelir();

        Assert.Equal(10000, pk.ToplamGelir);
        Assert.Equal(1800, pk.GelirKdvTutari); // 1500 + 300
        Assert.Equal(11800, pk.Alinacak); // 10000 + 1500 + 300
    }

    [Fact]
    public void HesaplaGider_BirimGiderVeGundan_Hesaplar()
    {
        var pk = new PuantajKayit
        {
            BirimGider = 400,
            Gun = 22,
            GiderKdv20Tutari = 1760
        };
        pk.HesaplaGider();

        Assert.Equal(8800, pk.ToplamGider);
        Assert.Equal(10560, pk.Odenecek); // 8800 + 1760
    }

    [Fact]
    public void HesaplaGider_KesintiVarsa_DusulerHesaplar()
    {
        var pk = new PuantajKayit
        {
            BirimGider = 1000,
            Gun = 10,
            GiderKdv20Tutari = 2000,
            GiderKesinti = 500
        };
        pk.HesaplaGider();

        Assert.Equal(10000, pk.ToplamGider);
        Assert.Equal(11500, pk.Odenecek); // 10000 + 2000 - 500
    }

    [Fact]
    public void FarkTutari_GelirEksiGider_Hesaplanir()
    {
        var pk = new PuantajKayit
        {
            Alinacak = 13200,
            Odenecek = 10560
        };

        Assert.Equal(2640, pk.FarkTutari);
    }

    [Fact]
    public void FarkTutari_Zarar_NegatifDoner()
    {
        var pk = new PuantajKayit
        {
            Alinacak = 5000,
            Odenecek = 8000
        };

        Assert.Equal(-3000, pk.FarkTutari);
    }

    [Fact]
    public void HesaplaPuantajToplam_GunlukPuantajdanHesaplar()
    {
        var pk = new PuantajKayit { BirimGider = 400 };
        for (int i = 1; i <= 20; i++)
            pk.SetGunDeger(i, 1);

        pk.HesaplaPuantajToplam();

        Assert.Equal(20, pk.Gun);
        Assert.Equal(8000, pk.ToplamGider);
        Assert.Equal(8000, pk.Odenecek);
    }

    [Fact]
    public void PuantajEnumlar_DogruDegerler()
    {
        Assert.Equal(1, (int)PuantajYon.Sabah);
        Assert.Equal(2, (int)PuantajYon.Aksam);
        Assert.Equal(3, (int)PuantajYon.SabahAksam);
        Assert.Equal(1, (int)SoforOdemeTipi.Ozmal);
        Assert.Equal(2, (int)SoforOdemeTipi.Kiralik);
        Assert.Equal(3, (int)SoforOdemeTipi.Komisyoncu);
    }
}
