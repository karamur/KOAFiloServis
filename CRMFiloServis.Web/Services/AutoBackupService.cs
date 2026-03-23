namespace CRMFiloServis.Web.Services;

public class AutoBackupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AutoBackupService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(30);

    public AutoBackupService(IServiceProvider serviceProvider, ILogger<AutoBackupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Otomatik yedekleme servisi baþlatýldý.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndBackupAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Otomatik yedekleme kontrolü hatasý");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckAndBackupAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();

        var settings = backupService.GetSettings();

        if (!settings.AutoBackupEnabled)
            return;

        var shouldBackup = false;

        if (!settings.LastBackupTime.HasValue)
        {
            shouldBackup = true;
        }
        else
        {
            var elapsed = DateTime.Now - settings.LastBackupTime.Value;
            shouldBackup = elapsed.TotalHours >= settings.AutoBackupIntervalHours;
        }

        if (shouldBackup)
        {
            _logger.LogInformation("Otomatik yedekleme baþlatýlýyor...");
            var result = await backupService.CreateBackupAsync();

            if (result.Success)
            {
                _logger.LogInformation("Otomatik yedekleme tamamlandý: {FileName}", result.FileName);
            }
            else
            {
                _logger.LogError("Otomatik yedekleme baþarýsýz: {Error}", result.ErrorMessage);
            }
        }
    }
}
