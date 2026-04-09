using CRMFiloServis.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CRMFiloServis.Web.Components.Shared;

public class AppErrorBoundary : ErrorBoundary
{
    [Inject] public NavigationManager Navigation { get; set; } = null!;
    [Inject] public AppIssueStateService AppIssueState { get; set; } = null!;

    protected override Task OnErrorAsync(Exception exception)
    {
        AppIssueState.Report(exception);

        if (!Navigation.Uri.Contains("/ters-giden-bir-sey", StringComparison.OrdinalIgnoreCase) &&
            !Navigation.Uri.Contains("/error", StringComparison.OrdinalIgnoreCase))
        {
            Navigation.NavigateTo("/ters-giden-bir-sey", forceLoad: false);
        }

        return Task.CompletedTask;
    }
}
