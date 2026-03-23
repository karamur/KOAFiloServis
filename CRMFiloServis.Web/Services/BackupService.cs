using System.Text.Json;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Services;

public class BackupService : IBackupService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BackupService> _logger;
    private readonly string _settingsFile;

    public BackupService(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        IServiceProvider serviceProvider,
        ILogger<BackupService> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _settingsFile = Path.Combine(_environment.ContentRootPath, "backup_settings.json");
    }

    public async Task<BackupResult> CreateBackupAsync()
    {
        var result = new BackupResult();

        try
        {
            var settings = GetSettings();
            var backupFolder = GetBackupFolderPath(settings);

            // Klasör yoksa oluþtur
            if (!Directory.Exists(backupFolder))
            {
                Directory.CreateDirectory(backupFolder);
            }

            // SQLite veritabaný yolunu al
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var dbPath = connectionString?.Replace("Data Source=", "").Trim() ?? "crmfiloservis.db";

            if (!Path.IsPathRooted(dbPath))
            {
                dbPath = Path.Combine(_environment.ContentRootPath, dbPath);
            }

            if (!File.Exists(dbPath))
            {
                result.ErrorMessage = "Veritabaný dosyasý bulunamadý.";
                return result;
            }

            // Yedek dosya adý
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"CRMFiloServis_Backup_{timestamp}.db";
            var backupFilePath = Path.Combine(backupFolder, backupFileName);

            // Veritabanýný kopyala
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Checkpoint yaparak WAL modundaki deðiþiklikleri ana dosyaya yaz
                await context.Database.ExecuteSqlRawAsync("PRAGMA wal_checkpoint(TRUNCATE);");
            }

            // Dosyayý kopyala
            File.Copy(dbPath, backupFilePath, overwrite: true);

            var fileInfo = new FileInfo(backupFilePath);

            result.Success = true;
            result.FileName = backupFileName;
            result.FilePath = backupFilePath;
            result.FileSizeBytes = fileInfo.Length;
            result.CreatedAt = DateTime.Now;

            // Son yedekleme zamanýný güncelle
            settings.LastBackupTime = DateTime.Now;
            await SaveSettingsAsync(settings);

            // Eski yedekleri temizle
            await CleanupOldBackupsAsync(settings.KeepBackupCount);

            _logger.LogInformation("Yedekleme baþarýlý: {FileName}, Boyut: {Size}", backupFileName, fileInfo.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yedekleme hatasý");
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    public Task<List<BackupInfo>> GetBackupListAsync()
    {
        var settings = GetSettings();
        var backupFolder = GetBackupFolderPath(settings);
        var backups = new List<BackupInfo>();

        if (Directory.Exists(backupFolder))
        {
            var files = Directory.GetFiles(backupFolder, "CRMFiloServis_Backup_*.db")
                .OrderByDescending(f => f);

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                backups.Add(new BackupInfo
                {
                    FileName = fileInfo.Name,
                    FilePath = file,
                    FileSizeBytes = fileInfo.Length,
                    CreatedAt = fileInfo.CreationTime
                });
            }
        }

        return Task.FromResult(backups);
    }

    public async Task<bool> RestoreBackupAsync(string backupFileName)
    {
        try
        {
            var settings = GetSettings();
            var backupFolder = GetBackupFolderPath(settings);
            var backupFilePath = Path.Combine(backupFolder, backupFileName);

            if (!File.Exists(backupFilePath))
            {
                _logger.LogError("Yedek dosyasý bulunamadý: {FileName}", backupFileName);
                return false;
            }

            // SQLite veritabaný yolunu al
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var dbPath = connectionString?.Replace("Data Source=", "").Trim() ?? "crmfiloservis.db";

            if (!Path.IsPathRooted(dbPath))
            {
                dbPath = Path.Combine(_environment.ContentRootPath, dbPath);
            }

            // Mevcut veritabanýnýn yedeðini al (geri alma için)
            var currentBackup = dbPath + ".restore_backup";
            if (File.Exists(dbPath))
            {
                File.Copy(dbPath, currentBackup, overwrite: true);
            }

            try
            {
                // Yedekten geri yükle
                File.Copy(backupFilePath, dbPath, overwrite: true);

                // WAL ve SHM dosyalarýný temizle
                var walPath = dbPath + "-wal";
                var shmPath = dbPath + "-shm";
                if (File.Exists(walPath)) File.Delete(walPath);
                if (File.Exists(shmPath)) File.Delete(shmPath);

                _logger.LogInformation("Veritabaný geri yüklendi: {FileName}", backupFileName);

                // Geçici yedeði sil
                if (File.Exists(currentBackup)) File.Delete(currentBackup);

                return true;
            }
            catch
            {
                // Hata durumunda eski veritabanýný geri yükle
                if (File.Exists(currentBackup))
                {
                    File.Copy(currentBackup, dbPath, overwrite: true);
                    File.Delete(currentBackup);
                }
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Geri yükleme hatasý: {FileName}", backupFileName);
            return false;
        }
    }

    public Task<bool> DeleteBackupAsync(string backupFileName)
    {
        try
        {
            var settings = GetSettings();
            var backupFolder = GetBackupFolderPath(settings);
            var backupFilePath = Path.Combine(backupFolder, backupFileName);

            if (File.Exists(backupFilePath))
            {
                File.Delete(backupFilePath);
                _logger.LogInformation("Yedek silindi: {FileName}", backupFileName);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yedek silme hatasý: {FileName}", backupFileName);
            return Task.FromResult(false);
        }
    }

    public async Task CleanupOldBackupsAsync(int keepCount = 10)
    {
        try
        {
            var backups = await GetBackupListAsync();
            var toDelete = backups.Skip(keepCount).ToList();

            foreach (var backup in toDelete)
            {
                await DeleteBackupAsync(backup.FileName);
            }

            if (toDelete.Any())
            {
                _logger.LogInformation("{Count} eski yedek temizlendi", toDelete.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eski yedek temizleme hatasý");
        }
    }

    public BackupSettings GetSettings()
    {
        try
        {
            if (File.Exists(_settingsFile))
            {
                var json = File.ReadAllText(_settingsFile);
                return JsonSerializer.Deserialize<BackupSettings>(json) ?? new BackupSettings();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ayarlar okunamadý");
        }

        return new BackupSettings();
    }

    public async Task SaveSettingsAsync(BackupSettings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_settingsFile, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ayarlar kaydedilemedi");
        }
    }

    private string GetBackupFolderPath(BackupSettings settings)
    {
        var folder = settings.BackupFolder;
        if (!Path.IsPathRooted(folder))
        {
            folder = Path.Combine(_environment.ContentRootPath, folder);
        }
        return folder;
    }
}
