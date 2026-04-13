using Quartz;
using CRMFiloServis.Web.Services;

namespace CRMFiloServis.Web.Jobs;

[DisallowConcurrentExecution]
public class AutoBackupJob : IJob
{
    private readonly AutoBackupService _autoBackupService;

    public AutoBackupJob(AutoBackupService autoBackupService)
    {
        _autoBackupService = autoBackupService;
    }

    public Task Execute(IJobExecutionContext context)
    {
        return _autoBackupService.RunOnceAsync(context.CancellationToken);
    }
}
