namespace CRMFiloServis.Web.Services;

public interface IBackupService
{
    string GetCurrentDatabaseProvider();
    Task<BackupResult> CreateBackupAsync(string? customBackupFolder = null);
    Task<List<BackupInfo>> GetBackupListAsync();
    Task<bool> RestoreBackupAsync(string backupFileName);
    Task<bool> DeleteBackupAsync(string backupFileName);
    Task CleanupOldBackupsAsync(int keepCount = 10);
    BackupSettings GetSettings();
    Task SaveSettingsAsync(BackupSettings settings);
}

public class BackupResult
{
    public bool Success { get; set; }
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
    public long FileSizeBytes { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public class BackupInfo
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }

    public string FileSizeFormatted => FileSizeBytes switch
    {
        < 1024 => $"{FileSizeBytes} B",
        < 1024 * 1024 => $"{FileSizeBytes / 1024.0:F1} KB",
        _ => $"{FileSizeBytes / (1024.0 * 1024.0):F2} MB"
    };
}

public class BackupSettings
{
    public bool AutoBackupEnabled { get; set; } = true;
    public int AutoBackupIntervalHours { get; set; } = 24;
    public int KeepBackupCount { get; set; } = 10;
    public string BackupFolder { get; set; } = "Backups";
    public DateTime? LastBackupTime { get; set; }
}
