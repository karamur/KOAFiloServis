using KOAFiloServis.Shared.Entities;

namespace KOAFiloServis.Web.Services;

public interface IBelgeUyariService
{
    Task<BelgeUyariOzet> GetBelgeUyarilarAsync(int yaklasanGunSayisi = 30);
}

public class BelgeUyariOzet
{
    public int ToplamKritikUyari { get; set; }
    public int ToplamUyari { get; set; }

    // Personel Belgeleri
    public List<BelgeUyari> EhliyetUyarilari { get; set; } = new();
    public List<BelgeUyari> SrcUyarilari { get; set; } = new();
    public List<BelgeUyari> PsikoteknikUyarilari { get; set; } = new();
    public List<BelgeUyari> SaglikRaporuUyarilari { get; set; } = new();
    public List<BelgeUyari> DigerPersonelEvrakUyarilari { get; set; } = new();

    /// <summary>Diger kategorisindeki TUM ozluk evraklari (eksik, gecerli, suresi gecmis)</summary>
    public List<PersonelBelgeDetay> DigerTumPersonelBelgeler { get; set; } = new();

    // Arac Belgeleri
    public List<BelgeUyari> MuayeneUyarilari { get; set; } = new();
    public List<BelgeUyari> KaskoUyarilari { get; set; } = new();
    public List<BelgeUyari> TrafikSigortasiUyarilari { get; set; } = new();
    public List<BelgeUyari> DigerAracEvrakUyarilari { get; set; } = new();

    public List<BelgeUyari> TumUyarilar =>
        EhliyetUyarilari
        .Concat(SrcUyarilari)
        .Concat(PsikoteknikUyarilari)
        .Concat(SaglikRaporuUyarilari)
        .Concat(DigerPersonelEvrakUyarilari)
        .Concat(MuayeneUyarilari)
        .Concat(KaskoUyarilari)
        .Concat(TrafikSigortasiUyarilari)
        .Concat(DigerAracEvrakUyarilari)
        .OrderBy(u => u.KalanGun)
        .ToList();
}

public class BelgeUyari
{
    public int Id { get; set; }
    public string Kaynak { get; set; } = string.Empty;
    public string Baslik { get; set; } = string.Empty;
    public string BelgeTuru { get; set; } = string.Empty;
    public DateTime BitisTarihi { get; set; }
    public string DetayUrl { get; set; } = string.Empty;
    public int KalanGun => (BitisTarihi - DateTime.Today).Days;
    public BelgeUyariSeviye Seviye => KalanGun switch
    {
        < 0 => BelgeUyariSeviye.Kritik,
        <= 7 => BelgeUyariSeviye.Acil,
        <= 30 => BelgeUyariSeviye.Uyari,
        _ => BelgeUyariSeviye.Bilgi
    };
    public string SeviyeClass => Seviye switch
    {
        BelgeUyariSeviye.Kritik => "bg-danger",
        BelgeUyariSeviye.Acil => "bg-warning text-dark",
        BelgeUyariSeviye.Uyari => "bg-info",
        _ => "bg-secondary"
    };
    public string Icon => Seviye switch
    {
        BelgeUyariSeviye.Kritik => "bi-exclamation-triangle-fill",
        BelgeUyariSeviye.Acil => "bi-exclamation-circle-fill",
        BelgeUyariSeviye.Uyari => "bi-info-circle-fill",
        _ => "bi-info-circle"
    };
}

public enum BelgeUyariSeviye
{
    Bilgi = 0,
    Uyari = 1,
    Acil = 2,
    Kritik = 3
}

/// <summary>
/// "Diger Onemli Belgeler" bolumu icin tam liste modeli
/// </summary>
public class PersonelBelgeDetay
{
    public int EvrakId { get; set; }
    public int SoforId { get; set; }
    public string PersonelAdi { get; set; } = string.Empty;
    public string PersonelKodu { get; set; } = string.Empty;
    public string EvrakAdi { get; set; } = string.Empty;
    public OzlukEvrakKategori Kategori { get; set; }
    public bool Tamamlandi { get; set; }
    public DateTime? TamamlanmaTarihi { get; set; }
    public DateTime? GecerlilikBitisTarihi { get; set; }
    public bool Zorunlu { get; set; }
    public string? DosyaYolu { get; set; }
    public string DetayUrl { get; set; } = string.Empty;

    public int? KalanGun => GecerlilikBitisTarihi.HasValue
        ? (GecerlilikBitisTarihi.Value - DateTime.Today).Days
        : null;

    public string DurumClass
    {
        get
        {
            if (!Tamamlandi) return "bg-secondary";
            if (GecerlilikBitisTarihi == null) return "bg-success";
            return KalanGun switch
            {
                < 0 => "bg-danger",
                <= 7 => "bg-warning text-dark",
                <= 30 => "bg-info",
                _ => "bg-success"
            };
        }
    }

    public string DurumMetin
    {
        get
        {
            if (!Tamamlandi) return "Eksik";
            if (GecerlilikBitisTarihi == null) return "Mevcut";
            return KalanGun switch
            {
                < 0 => $"{Math.Abs(KalanGun!.Value)} gun gecti",
                <= 30 => $"{KalanGun} gun kaldi",
                _ => "Gecerli"
            };
        }
    }
}
