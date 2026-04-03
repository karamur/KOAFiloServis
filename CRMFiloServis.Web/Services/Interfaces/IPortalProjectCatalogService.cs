namespace CRMFiloServis.Web.Services;

public interface IPortalProjectCatalogService
{
    PortalProjectCatalogOptions GetCatalog();
    IReadOnlyList<PortalProjectDefinition> GetProjects();
    PortalProjectDefinition? GetProjectBySlug(string? slug);
    PortalProjectDefinition GetDefaultProject();
}
