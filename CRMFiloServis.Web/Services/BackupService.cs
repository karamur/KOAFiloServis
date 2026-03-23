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

            // Klasor yoksa olustur
            if (!Directory.Exists(backupFolder))
            {
                Directory.CreateDirectory(backupFolder);
            }

            // Connection string'den veritabani bilgilerini al
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                result.ErrorMessage = "Veritabani baglanti bilgisi bulunamadi.";
                return result;
            }

            // PostgreSQL icin pg_dump kullan
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"CRMFiloServis_Backup_{timestamp}.sql";
            var backupFilePath = Path.Combine(backupFolder, backupFileName);

            // PostgreSQL baglanti bilgilerini parse et
            var connParts = ParseConnectionString(connectionString);
            
            // pg_dump komutu olustur
            var pgDumpPath = FindPgDump();
            
            if (string.IsNullOrEmpty(pgDumpPath))
            {
                // pg_dump bulunamadiysa JSON export yap
                await CreateJsonBackupAsync(backupFilePath.Replace(".sql", ".json"));
                backupFileName = backupFileName.Replace(".sql", ".json");
                backupFilePath = backupFilePath.Replace(".sql", ".json");
            }
            else
            {
                // pg_dump ile yedek al
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = pgDumpPath,
                    Arguments = $"-h {connParts["Host"]} -p {connParts["Port"]} -U {connParts["Username"]} -d {connParts["Database"]} -f \"{backupFilePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Environment = { ["PGPASSWORD"] = connParts["Password"] }
                };

                using var process = System.Diagnostics.Process.Start(processInfo);
                if (process != null)
                {
                    await process.WaitForExitAsync();
                    if (process.ExitCode != 0)
                    {
                        var error = await process.StandardError.ReadToEndAsync();
                        _logger.LogWarning("pg_dump hatasi: {Error}, JSON backup yapiliyor...", error);
                        
                        // pg_dump basarisiz olursa JSON export yap
                        await CreateJsonBackupAsync(backupFilePath.Replace(".sql", ".json"));
                        backupFileName = backupFileName.Replace(".sql", ".json");
                        backupFilePath = backupFilePath.Replace(".sql", ".json");
                    }
                }
            }

            if (File.Exists(backupFilePath))
            {
                var fileInfo = new FileInfo(backupFilePath);
                
                result.Success = true;
                result.FileName = backupFileName;
                result.FilePath = backupFilePath;
                result.FileSizeBytes = fileInfo.Length;
                result.CreatedAt = DateTime.Now;

                // Son yedekleme zamanini guncelle
                settings.LastBackupTime = DateTime.Now;
                await SaveSettingsAsync(settings);

                // Eski yedekleri temizle
                await CleanupOldBackupsAsync(settings.KeepBackupCount);

                _logger.LogInformation("Yedekleme basarili: {FileName}, Boyut: {Size}", backupFileName, fileInfo.Length);
            }
            else
            {
                result.ErrorMessage = "Yedek dosyasi olusturulamadi.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yedekleme hatasi");
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    private async Task CreateJsonBackupAsync(string filePath)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Sirayla sorgula (concurrent DbContext kullanimi hatasi onlemek icin)
        var cariler = await context.Cariler.IgnoreQueryFilters().AsNoTracking().ToListAsync();
        var araclar = await context.Araclar.IgnoreQueryFilters().AsNoTracking().ToListAsync();
        var soforler = await context.Soforler.IgnoreQueryFilters().AsNoTracking().ToListAsync();
        var guzergahlar = await context.Guzergahlar.IgnoreQueryFilters().AsNoTracking().ToListAsync();
        var faturalar = await context.Faturalar.IgnoreQueryFilters().AsNoTracking().ToListAsync();
        var faturaKalemleri = await context.FaturaKalemleri.IgnoreQueryFilters().AsNoTracking().ToListAsync();
        var bankaHesaplari = await context.BankaHesaplari.IgnoreQueryFilters().AsNoTracking().ToListAsync();
        var bankaKasaHareketleri = await context.BankaKasaHareketleri.IgnoreQueryFilters().AsNoTracking().ToListAsync();
        var budgetOdemeler = await context.BudgetOdemeler.IgnoreQueryFilters().AsNoTracking().ToListAsync();
        var budgetMasrafKalemleri = await context.BudgetMasrafKalemleri.IgnoreQueryFilters().AsNoTracking().ToListAsync();
        var servisCalismalari = await context.ServisCalismalari.IgnoreQueryFilters().AsNoTracking().ToListAsync();
        var aracMasraflari = await context.AracMasraflari.IgnoreQueryFilters().AsNoTracking().ToListAsync();
        var masrafKalemleri = await context.MasrafKalemleri.IgnoreQueryFilters().AsNoTracking().ToListAsync();

        var backup = new
        {
            ExportDate = DateTime.UtcNow,
            Cariler = cariler,
            Araclar = araclar,
            Soforler = soforler,
            Guzergahlar = guzergahlar,
            Faturalar = faturalar,
            FaturaKalemleri = faturaKalemleri,
            BankaHesaplari = bankaHesaplari,
            BankaKasaHareketleri = bankaKasaHareketleri,
            BudgetOdemeler = budgetOdemeler,
            BudgetMasrafKalemleri = budgetMasrafKalemleri,
            ServisCalismalari = servisCalismalari,
            AracMasraflari = aracMasraflari,
            MasrafKalemleri = masrafKalemleri
        };

        var json = JsonSerializer.Serialize(backup, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
        
        await File.WriteAllTextAsync(filePath, json);
    }

    private Dictionary<string, string> ParseConnectionString(string connectionString)
    {
        var parts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var part in connectionString.Split(';'))
        {
            var keyValue = part.Split('=', 2);
            if (keyValue.Length == 2)
            {
                parts[keyValue[0].Trim()] = keyValue[1].Trim();
            }
        }

        // Varsayilan degerler
        if (!parts.ContainsKey("Port")) parts["Port"] = "5432";
        if (!parts.ContainsKey("Host")) parts["Host"] = "localhost";
        
        return parts;
    }

    private string? FindPgDump()
    {
        var possiblePaths = new[]
        {
            @"C:\Program Files\PostgreSQL\16\bin\pg_dump.exe",
            @"C:\Program Files\PostgreSQL\15\bin\pg_dump.exe",
            @"C:\Program Files\PostgreSQL\14\bin\pg_dump.exe",
            @"C:\Program Files\PostgreSQL\13\bin\pg_dump.exe",
            "/usr/bin/pg_dump",
            "/usr/local/bin/pg_dump"
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }

    public Task<List<BackupInfo>> GetBackupListAsync()
    {
        var settings = GetSettings();
        var backupFolder = GetBackupFolderPath(settings);
        var backups = new List<BackupInfo>();

        if (Directory.Exists(backupFolder))
        {
            var files = Directory.GetFiles(backupFolder, "CRMFiloServis_Backup_*.*")
                .Where(f => f.EndsWith(".sql") || f.EndsWith(".json") || f.EndsWith(".db"))
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
                _logger.LogError("Yedek dosyasi bulunamadi: {FileName}", backupFileName);
                return false;
            }

            // JSON yedek ise
            if (backupFilePath.EndsWith(".json"))
            {
                _logger.LogWarning("JSON yedekten geri yukleme henuz desteklenmiyor.");
                return false;
            }

            // SQL yedek ise psql ile geri yukle
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var connParts = ParseConnectionString(connectionString!);
            
            var psqlPath = FindPgDump()?.Replace("pg_dump", "psql");
            
            if (string.IsNullOrEmpty(psqlPath) || !File.Exists(psqlPath))
            {
                _logger.LogError("psql bulunamadi");
                return false;
            }

            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = psqlPath,
                Arguments = $"-h {connParts["Host"]} -p {connParts["Port"]} -U {connParts["Username"]} -d {connParts["Database"]} -f \"{backupFilePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Environment = { ["PGPASSWORD"] = connParts["Password"] }
            };

            using var process = System.Diagnostics.Process.Start(processInfo);
            if (process != null)
            {
                await process.WaitForExitAsync();
                if (process.ExitCode == 0)
                {
                    _logger.LogInformation("Veritabani geri yuklendi: {FileName}", backupFileName);
                    return true;
                }
                else
                {
                    var error = await process.StandardError.ReadToEndAsync();
                    _logger.LogError("Geri yukleme hatasi: {Error}", error);
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Geri yukleme hatasi: {FileName}", backupFileName);
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
            _logger.LogError(ex, "Yedek silme hatasi: {FileName}", backupFileName);
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
            _logger.LogError(ex, "Eski yedek temizleme hatasi");
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
            _logger.LogError(ex, "Ayarlar okunamadi");
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
