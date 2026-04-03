using Microsoft.Extensions.Options;

namespace CRMFiloServis.Web.Services;

public sealed class PortalProjectCatalogService : IPortalProjectCatalogService
{
    private readonly PortalProjectCatalogOptions _options;

    public PortalProjectCatalogService(IConfiguration configuration)
    {
        _options = configuration.GetSection("PortalProjects").Get<PortalProjectCatalogOptions>() ?? new PortalProjectCatalogOptions();

        if (_options.Projects.Count == 0)
        {
            _options.Projects.Add(new PortalProjectDefinition
            {
                Slug = "koa-filo-servis",
                Name = "Koa Filo Servis",
                BrandHighlight = "Koa Filo Servis",
                Category = "Filo Yönetimi",
                Subtitle = "Filo operasyonu, muhasebe ve servis süreçleri tek panelde.",
                Description = "Araç, sürücü, cari ve muhasebe operasyonlarını tek portal üzerinden yönetin.",
                Icon = "bi bi-truck",
                ThemeColor = "#1e3c72",
                LoginEnabled = true,
                Highlights =
                {
                    "Operasyon ve araç yönetimi",
                    "Muhasebe ve raporlama modülleri",
                    "Kurumsal kullanıcı giriş akışı"
                }
            });
        }
    }

    public PortalProjectCatalogOptions GetCatalog() => _options;

    public IReadOnlyList<PortalProjectDefinition> GetProjects() => _options.Projects;

    public PortalProjectDefinition? GetProjectBySlug(string? slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return null;
        }

        return _options.Projects.FirstOrDefault(p => string.Equals(p.Slug, slug, StringComparison.OrdinalIgnoreCase));
    }

    public PortalProjectDefinition GetDefaultProject()
    {
        return _options.Projects.FirstOrDefault(p => p.LoginEnabled)
            ?? _options.Projects.First();
    }
}

public sealed class PortalProjectCatalogOptions
{
    public string HeroTitle { get; set; } = "İş süreçlerinizi tek merkezden yönetin";
    public string HeroSubtitle { get; set; } = "Kurumsal portal üzerinden projenizi seçin ve güvenli girişe devam edin.";
    public List<PortalProjectDefinition> Projects { get; set; } = new();
}

public sealed class PortalProjectDefinition
{
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string BrandHighlight { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi bi-grid-1x2-fill";
    public string ThemeColor { get; set; } = "#1e3c72";
    public bool LoginEnabled { get; set; } = true;
    public List<string> Highlights { get; set; } = new();
}
