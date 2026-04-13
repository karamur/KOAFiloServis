using Quartz;
using CRMFiloServis.Web.Services;

namespace CRMFiloServis.Web.Jobs;

[DisallowConcurrentExecution]
public class DatabaseBackupJob : IJob
{
    private readonly DatabaseBackupService _service;

    public DatabaseBackupJob(DatabaseBackupService service)
    {
        _service = service;
    }

    public Task Execute(IJobExecutionContext context)
    {
        return _service.ExecuteScheduledBackupAsync();
    }
}
