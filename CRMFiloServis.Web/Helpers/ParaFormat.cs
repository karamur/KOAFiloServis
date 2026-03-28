namespace CRMFiloServis.Web.Helpers;

/// <summary>
/// Para birimi ve sayı formatlama yardımcı sınıfı
/// </summary>
public static class ParaFormat
{
    private const string ParaBirimi = "₺";
    private const string ParaBirimiKodu = "TL";

    /// <summary>
    /// Decimal değeri para formatında döner (1.234,56 ?)
    /// </summary>
    public static string Format(decimal tutar, bool birimGoster = true)
    {
        var formatli = tutar.ToString("N2", System.Globalization.CultureInfo.GetCultureInfo("tr-TR"));
        return birimGoster ? $"{formatli} {ParaBirimi}" : formatli;
    }

    /// <summary>
    /// Decimal değeri kısa para formatında döner (1.234 ?)
    /// </summary>
    public static string FormatKisa(decimal tutar, bool birimGoster = true)
    {
        var formatli = tutar.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("tr-TR"));
        return birimGoster ? $"{formatli} {ParaBirimi}" : formatli;
    }

    /// <summary>
    /// Decimal değeri TL koduyla döner (1.234,56 TL)
    /// </summary>
    public static string FormatTL(decimal tutar)
    {
        var formatli = tutar.ToString("N2", System.Globalization.CultureInfo.GetCultureInfo("tr-TR"));
        return $"{formatli} {ParaBirimiKodu}";
    }

    /// <summary>
    /// Decimal değeri kısa TL koduyla döner (1.234 TL)
    /// </summary>
    public static string FormatTLKisa(decimal tutar)
    {
        var formatli = tutar.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("tr-TR"));
        return $"{formatli} {ParaBirimiKodu}";
    }

    /// <summary>
    /// Nullable decimal değeri formatlar
    /// </summary>
    public static string Format(decimal? tutar, bool birimGoster = true, string bostaDeger = "-")
    {
        return tutar.HasValue ? Format(tutar.Value, birimGoster) : bostaDeger;
    }

    /// <summary>
    /// Nullable decimal değeri kısa formatlar
    /// </summary>
    public static string FormatKisa(decimal? tutar, bool birimGoster = true, string bostaDeger = "-")
    {
        return tutar.HasValue ? FormatKisa(tutar.Value, birimGoster) : bostaDeger;
    }

    /// <summary>
    /// Yüzde formatı (% 15,5)
    /// </summary>
    public static string FormatYuzde(decimal oran)
    {
        return $"% {oran.ToString("N1", System.Globalization.CultureInfo.GetCultureInfo("tr-TR"))}";
    }

    /// <summary>
    /// Sadece para birimi sembolü
    /// </summary>
    public static string Birim => ParaBirimi;

    /// <summary>
    /// Sadece para birimi kodu
    /// </summary>
    public static string BirimKodu => ParaBirimiKodu;
}
