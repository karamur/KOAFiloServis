using Quartz;
using CRMFiloServis.Web.Services;

namespace CRMFiloServis.Web.Jobs;

[DisallowConcurrentExecution]
public class CariHatirlatmaJob : IJob
{
    private readonly CariHatirlatmaBackgroundService _service;

    public CariHatirlatmaJob(CariHatirlatmaBackgroundService service)
    {
        _service = service;
    }

    public Task Execute(IJobExecutionContext context)
    {
        return _service.RunOnceAsync(context.CancellationToken);
    }
}
