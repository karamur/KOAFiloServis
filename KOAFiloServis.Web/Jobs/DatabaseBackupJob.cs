using Quartz;
using KOAFiloServis.Web.Services;

namespace KOAFiloServis.Web.Jobs;

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
