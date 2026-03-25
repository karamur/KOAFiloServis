using System.Text.Json;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

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

    public string GetCurrentDatabaseProvider()
    {
        return _configuration.GetValue<string>("DatabaseProvider") ?? "SQLite";
    }

    public async Task<BackupResult> CreateBackupAsync(string? customBackupFolder = null)
    {
        var result = new BackupResult();
        var dbProvider = GetCurrentDatabaseProvider();

        try
        {
            var settings = GetSettings();
            var backupFolder = customBackupFolder ?? GetBackupFolderPath(settings);

            if (!Directory.Exists(backupFolder))
            {
                Directory.CreateDirectory(backupFolder);
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupFileName;
            string backupFilePath;

            switch (dbProvider.ToUpper())
            {
                case "POSTGRESQL":
                    backupFileName = $"CRMFiloServis_PostgreSQL_{timestamp}.sql";
                    backupFilePath = Path.Combine(backupFolder, backupFileName);
                    result = await CreatePostgreSqlBackupAsync(backupFilePath);
                    break;

                case "MSSQL":
                case "SQLSERVER":
                    backupFileName = $"CRMFiloServis_MSSQL_{timestamp}.bak";
                    backupFilePath = Path.Combine(backupFolder, backupFileName);
                    result = await CreateMsSqlBackupAsync(backupFilePath);
                    break;

                case "SQLITE":
                default:
                    backupFileName = $"CRMFiloServis_SQLite_{timestamp}.db";
                    backupFilePath = Path.Combine(backupFolder, backupFileName);
                    result = await CreateSqliteBackupAsync(backupFilePath);
                    break;
            }

            if (result.Success)
            {
                settings.LastBackupTime = DateTime.Now;
                await SaveSettingsAsync(settings);
                await CleanupOldBackupsAsync(settings.KeepBackupCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yedekleme hatasi");
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    private async Task<BackupResult> CreateSqliteBackupAsync(string backupFilePath)
    {
        var result = new BackupResult();

        try
        {
            var connectionString = _configuration.GetConnectionString("SQLite");
            if (string.IsNullOrEmpty(connectionString))
            {
                result.ErrorMessage = "SQLite connection string bulunamadi.";
                return result;
            }

            // Data Source path'ini parse et
            var sourcePath = connectionString.Replace("Data Source=", "").Trim();
            if (!Path.IsPathRooted(sourcePath))
            {
                sourcePath = Path.Combine(_environment.ContentRootPath, sourcePath);
            }

            if (!File.Exists(sourcePath))
            {
                result.ErrorMessage = $"SQLite veritabani dosyasi bulunamadi: {sourcePath}";
                return result;
            }

            // Veritabani dosyasini kopyala
            File.Copy(sourcePath, backupFilePath, overwrite: true);

            var fileInfo = new FileInfo(backupFilePath);
            result.Success = true;
            result.FileName = Path.GetFileName(backupFilePath);
            result.FilePath = backupFilePath;
            result.FileSizeBytes = fileInfo.Length;
            result.CreatedAt = DateTime.Now;

            _logger.LogInformation("SQLite yedekleme basarili: {FileName}", result.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQLite yedekleme hatasi");
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    private async Task<BackupResult> CreatePostgreSqlBackupAsync(string backupFilePath)
    {
        var result = new BackupResult();

        try
        {
            var connectionString = _configuration.GetConnectionString("PostgreSQL");
            if (string.IsNullOrEmpty(connectionString))
            {
                result.ErrorMessage = "PostgreSQL connection string bulunamadi.";
                return result;
            }

            var connParts = ParseConnectionString(connectionString);
            var pgDumpPath = FindPgDump();

            if (string.IsNullOrEmpty(pgDumpPath))
            {
                // pg_dump bulunamadiysa JSON export yap
                var jsonPath = backupFilePath.Replace(".sql", ".json");
                await CreateJsonBackupAsync(jsonPath);

                var jsonFileInfo = new FileInfo(jsonPath);
                result.Success = true;
                result.FileName = Path.GetFileName(jsonPath);
                result.FilePath = jsonPath;
                result.FileSizeBytes = jsonFileInfo.Length;
                result.CreatedAt = DateTime.Now;

                _logger.LogInformation("PostgreSQL JSON yedekleme basarili: {FileName}", result.FileName);
                return result;
            }

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
                    _logger.LogWarning("pg_dump hatasi: {Error}", error);

                    // Fallback to JSON
                    var jsonPath = backupFilePath.Replace(".sql", ".json");
                    await CreateJsonBackupAsync(jsonPath);
                    backupFilePath = jsonPath;
                }
            }

            if (File.Exists(backupFilePath))
            {
                var fileInfo = new FileInfo(backupFilePath);
                result.Success = true;
                result.FileName = Path.GetFileName(backupFilePath);
                result.FilePath = backupFilePath;
                result.FileSizeBytes = fileInfo.Length;
                result.CreatedAt = DateTime.Now;

                _logger.LogInformation("PostgreSQL yedekleme basarili: {FileName}", result.FileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PostgreSQL yedekleme hatasi");
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    private async Task<BackupResult> CreateMsSqlBackupAsync(string backupFilePath)
    {
        var result = new BackupResult();

        try
        {
            var connectionString = _configuration.GetConnectionString("MSSQL") 
                ?? _configuration.GetConnectionString("SqlServer");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                // JSON fallback
                var jsonPath = backupFilePath.Replace(".bak", ".json");
                await CreateJsonBackupAsync(jsonPath);

                var jsonFileInfo = new FileInfo(jsonPath);
                result.Success = true;
                result.FileName = Path.GetFileName(jsonPath);
                result.FilePath = jsonPath;
                result.FileSizeBytes = jsonFileInfo.Length;
                result.CreatedAt = DateTime.Now;
                return result;
            }

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var dbName = context.Database.GetDbConnection().Database;
            var backupSql = $"BACKUP DATABASE [{dbName}] TO DISK = N'{backupFilePath}' WITH FORMAT, INIT, NAME = N'CRMFiloServis Backup'";

            await context.Database.ExecuteSqlRawAsync(backupSql);

            if (File.Exists(backupFilePath))
            {
                var fileInfo = new FileInfo(backupFilePath);
                result.Success = true;
                result.FileName = Path.GetFileName(backupFilePath);
                result.FilePath = backupFilePath;
                result.FileSizeBytes = fileInfo.Length;
                result.CreatedAt = DateTime.Now;

                _logger.LogInformation("MSSQL yedekleme basarili: {FileName}", result.FileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MSSQL yedekleme hatasi");
            
            // Fallback to JSON
            try
            {
                var jsonPath = backupFilePath.Replace(".bak", ".json");
                await CreateJsonBackupAsync(jsonPath);

                var jsonFileInfo = new FileInfo(jsonPath);
                result.Success = true;
                result.FileName = Path.GetFileName(jsonPath);
                result.FilePath = jsonPath;
                result.FileSizeBytes = jsonFileInfo.Length;
                result.CreatedAt = DateTime.Now;
            }
            catch
            {
                result.ErrorMessage = ex.Message;
            }
        }

        return result;
    }

    private async Task CreateJsonBackupAsync(string filePath)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

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
        var firmalar = await context.Firmalar.IgnoreQueryFilters().AsNoTracking().ToListAsync();

        var backup = new
        {
            ExportDate = DateTime.UtcNow,
            DatabaseProvider = GetCurrentDatabaseProvider(),
            Firmalar = firmalar,
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

        if (!parts.ContainsKey("Port")) parts["Port"] = "5432";
        if (!parts.ContainsKey("Host")) parts["Host"] = "localhost";

        return parts;
    }

    private string? FindPgDump()
    {
        var possiblePaths = new[]
        {
            @"C:\Program Files\PostgreSQL\17\bin\pg_dump.exe",
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
            var files = Directory.GetFiles(backupFolder, "CRMFiloServis_*.*")
                .Where(f => f.EndsWith(".sql") || f.EndsWith(".json") || f.EndsWith(".db") || f.EndsWith(".bak"))
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

            var dbProvider = GetCurrentDatabaseProvider();

            // SQLite restore
            if (backupFilePath.EndsWith(".db"))
            {
                var connectionString = _configuration.GetConnectionString("SQLite");
                var targetPath = connectionString?.Replace("Data Source=", "").Trim();
                if (!string.IsNullOrEmpty(targetPath))
                {
                    if (!Path.IsPathRooted(targetPath))
                    {
                        targetPath = Path.Combine(_environment.ContentRootPath, targetPath);
                    }
                    File.Copy(backupFilePath, targetPath, overwrite: true);
                    _logger.LogInformation("SQLite restore basarili");
                    return true;
                }
            }

            // JSON restore desteklenmiyor (sadece bilgi amacli)
            if (backupFilePath.EndsWith(".json"))
            {
                _logger.LogWarning("JSON yedekten geri yukleme henuz desteklenmiyor.");
                return false;
            }

            // PostgreSQL restore
            if (backupFilePath.EndsWith(".sql") && dbProvider == "PostgreSQL")
            {
                var connectionString = _configuration.GetConnectionString("PostgreSQL");
                var connParts = ParseConnectionString(connectionString!);
                var psqlPath = FindPgDump()?.Replace("pg_dump", "psql");

                if (!string.IsNullOrEmpty(psqlPath) && File.Exists(psqlPath))
                {
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
                            _logger.LogInformation("PostgreSQL restore basarili");
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Restore hatasi");
            return false;
        }
    }

    public async Task<bool> DeleteBackupAsync(string backupFileName)
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
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yedek silme hatasi");
            return false;
        }
    }

    public async Task CleanupOldBackupsAsync(int keepCount = 10)
    {
        try
        {
            var settings = GetSettings();
            var backupFolder = GetBackupFolderPath(settings);

            if (!Directory.Exists(backupFolder)) return;

            var files = Directory.GetFiles(backupFolder, "CRMFiloServis_*.*")
                .Where(f => f.EndsWith(".sql") || f.EndsWith(".json") || f.EndsWith(".db") || f.EndsWith(".bak"))
                .OrderByDescending(f => new FileInfo(f).CreationTime)
                .Skip(keepCount)
                .ToList();

            foreach (var file in files)
            {
                File.Delete(file);
                _logger.LogInformation("Eski yedek silindi: {FileName}", Path.GetFileName(file));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eski yedekleri temizleme hatasi");
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
        catch { }

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
            _logger.LogError(ex, "Ayar kaydetme hatasi");
        }
    }

    private string GetBackupFolderPath(BackupSettings settings)
    {
        var folder = settings.BackupFolder;

        if (string.IsNullOrEmpty(folder))
        {
            folder = "Backups";
        }

        if (!Path.IsPathRooted(folder))
        {
            folder = Path.Combine(_environment.ContentRootPath, folder);
        }

        return folder;
    }
}
