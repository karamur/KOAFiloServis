using System.Text.Json;
using CRMFiloServis.Shared.Entities;
using CRMFiloServis.Web.Data;
using Microsoft.Data.Sqlite;
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

    public string GetCurrentDatabaseProvider()
    {
        var dbSettings = ReadDatabaseSettings();
        if (dbSettings != null)
        {
            return dbSettings.Provider switch
            {
                DatabaseProvider.PostgreSQL => "PostgreSQL",
                DatabaseProvider.SQLServer => "MSSQL",
                DatabaseProvider.MySQL => "MySQL",
                DatabaseProvider.SQLite => "SQLite",
                _ => "SQLite"
            };
        }

        var configuredProvider = _configuration.GetValue<string>("DatabaseProvider");
        if (!string.IsNullOrWhiteSpace(configuredProvider))
            return configuredProvider;

        var defaultConnection = _configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(defaultConnection))
            return GetProviderFromConnectionString(defaultConnection) switch
            {
                "POSTGRESQL" => "PostgreSQL",
                "SQLSERVER" => "MSSQL",
                "MYSQL" => "MySQL",
                "SQLITE" => "SQLite",
                _ => "SQLite"
            };

        return "SQLite";
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
                Directory.CreateDirectory(backupFolder);

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupFileName;
            string backupFilePath;

            switch (dbProvider.ToUpperInvariant())
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

                case "MYSQL":
                    backupFileName = $"CRMFiloServis_MySQL_{timestamp}.sql";
                    backupFilePath = Path.Combine(backupFolder, backupFileName);
                    result = await CreateMySqlBackupAsync(backupFilePath);
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
            var connectionString = ResolveConnectionString("SQLite");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                result.ErrorMessage = "SQLite connection string bulunamadi.";
                return result;
            }

            var sourcePath = connectionString.Replace("Data Source=", string.Empty, StringComparison.OrdinalIgnoreCase).Trim().TrimEnd(';');
            if (!Path.IsPathRooted(sourcePath))
                sourcePath = Path.Combine(_environment.ContentRootPath, sourcePath);

            if (!File.Exists(sourcePath))
            {
                result.ErrorMessage = $"SQLite veritabani dosyasi bulunamadi: {sourcePath}";
                return result;
            }

            File.Copy(sourcePath, backupFilePath, overwrite: true);

            var fileInfo = new FileInfo(backupFilePath);
            result.Success = true;
            result.FileName = Path.GetFileName(backupFilePath);
            result.FilePath = backupFilePath;
            result.FileSizeBytes = fileInfo.Length;
            result.CreatedAt = DateTime.Now;
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
            var connectionString = ResolveConnectionString("PostgreSQL");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                result.ErrorMessage = "PostgreSQL connection string bulunamadi.";
                return result;
            }

            var connParts = ParseConnectionString(connectionString);
            var pgDumpPath = FindPgDump();

            if (string.IsNullOrWhiteSpace(pgDumpPath))
            {
                var jsonPath = backupFilePath.Replace(".sql", ".json", StringComparison.OrdinalIgnoreCase);
                await CreateJsonBackupAsync(jsonPath);
                return CreateSuccessResult(jsonPath);
            }

            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = pgDumpPath,
                Arguments = $"-h {connParts.GetValueOrDefault("Host", "localhost")} -p {connParts.GetValueOrDefault("Port", "5432")} -U {connParts.GetValueOrDefault("Username", string.Empty)} -d {connParts.GetValueOrDefault("Database", string.Empty)} -f \"{backupFilePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            processInfo.Environment["PGPASSWORD"] = connParts.GetValueOrDefault("Password", string.Empty);

            using var process = System.Diagnostics.Process.Start(processInfo);
            if (process != null)
            {
                await process.WaitForExitAsync();
                if (process.ExitCode != 0)
                {
                    var error = await process.StandardError.ReadToEndAsync();
                    _logger.LogWarning("pg_dump hatasi: {Error}", error);

                    var jsonPath = backupFilePath.Replace(".sql", ".json", StringComparison.OrdinalIgnoreCase);
                    await CreateJsonBackupAsync(jsonPath);
                    backupFilePath = jsonPath;
                }
            }

            if (File.Exists(backupFilePath))
                return CreateSuccessResult(backupFilePath);
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
            var connectionString = ResolveConnectionString("MSSQL");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                var jsonPath = backupFilePath.Replace(".bak", ".json", StringComparison.OrdinalIgnoreCase);
                await CreateJsonBackupAsync(jsonPath);
                return CreateSuccessResult(jsonPath);
            }

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var dbName = context.Database.GetDbConnection().Database;
            var backupSql = $"BACKUP DATABASE [{dbName}] TO DISK = N'{backupFilePath}' WITH FORMAT, INIT, NAME = N'CRMFiloServis Backup'";

            await context.Database.ExecuteSqlRawAsync(backupSql);

            if (File.Exists(backupFilePath))
                return CreateSuccessResult(backupFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MSSQL yedekleme hatasi");

            try
            {
                var jsonPath = backupFilePath.Replace(".bak", ".json", StringComparison.OrdinalIgnoreCase);
                await CreateJsonBackupAsync(jsonPath);
                return CreateSuccessResult(jsonPath);
            }
            catch
            {
                result.ErrorMessage = ex.Message;
            }
        }

        return result;
    }

    private async Task<BackupResult> CreateMySqlBackupAsync(string backupFilePath)
    {
        var result = new BackupResult();

        try
        {
            var connectionString = ResolveConnectionString("MySQL");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                result.ErrorMessage = "MySQL connection string bulunamadi.";
                return result;
            }

            var connParts = ParseConnectionString(connectionString);
            var mySqlDumpPath = FindMySqlDump();

            if (string.IsNullOrWhiteSpace(mySqlDumpPath))
            {
                var jsonPath = backupFilePath.Replace(".sql", ".json", StringComparison.OrdinalIgnoreCase);
                await CreateJsonBackupAsync(jsonPath);
                return CreateSuccessResult(jsonPath);
            }

            var host = connParts.GetValueOrDefault("Server") ?? connParts.GetValueOrDefault("Host") ?? "localhost";
            var port = connParts.GetValueOrDefault("Port") ?? "3306";
            var username = connParts.GetValueOrDefault("User") ?? connParts.GetValueOrDefault("User Id") ?? connParts.GetValueOrDefault("Username") ?? string.Empty;
            var password = connParts.GetValueOrDefault("Password") ?? string.Empty;
            var database = connParts.GetValueOrDefault("Database") ?? string.Empty;

            var processInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = mySqlDumpPath,
                Arguments = $"--host={host} --port={port} --user={username} --result-file=\"{backupFilePath}\" {database}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            processInfo.Environment["MYSQL_PWD"] = password;

            using var process = System.Diagnostics.Process.Start(processInfo);
            if (process != null)
            {
                await process.WaitForExitAsync();
                if (process.ExitCode != 0)
                {
                    var error = await process.StandardError.ReadToEndAsync();
                    _logger.LogWarning("mysqldump hatasi: {Error}", error);

                    var jsonPath = backupFilePath.Replace(".sql", ".json", StringComparison.OrdinalIgnoreCase);
                    await CreateJsonBackupAsync(jsonPath);
                    backupFilePath = jsonPath;
                }
            }

            if (File.Exists(backupFilePath))
                return CreateSuccessResult(backupFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MySQL yedekleme hatasi");
            result.ErrorMessage = ex.Message;
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

        foreach (var part in connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var keyValue = part.Split('=', 2);
            if (keyValue.Length == 2)
                parts[keyValue[0].Trim()] = keyValue[1].Trim();
        }

        if (!parts.ContainsKey("Port"))
            parts["Port"] = "5432";
        if (!parts.ContainsKey("Host") && parts.ContainsKey("Server"))
            parts["Host"] = parts["Server"];
        if (!parts.ContainsKey("Host"))
            parts["Host"] = "localhost";

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

        return possiblePaths.FirstOrDefault(File.Exists);
    }

    private string? FindMySqlDump()
    {
        var possiblePaths = new[]
        {
            @"C:\Program Files\MySQL\MySQL Server 8.4\bin\mysqldump.exe",
            @"C:\Program Files\MySQL\MySQL Server 8.3\bin\mysqldump.exe",
            @"C:\Program Files\MySQL\MySQL Server 8.0\bin\mysqldump.exe",
            @"C:\xampp\mysql\bin\mysqldump.exe",
            "/usr/bin/mysqldump",
            "/usr/local/bin/mysqldump"
        };

        return possiblePaths.FirstOrDefault(File.Exists);
    }

    private string? ResolveConnectionString(string provider)
    {
        var dbSettings = ReadDatabaseSettings();
        if (dbSettings != null)
        {
            var settingsProvider = dbSettings.Provider switch
            {
                DatabaseProvider.PostgreSQL => "POSTGRESQL",
                DatabaseProvider.SQLServer => "SQLSERVER",
                DatabaseProvider.MySQL => "MYSQL",
                DatabaseProvider.SQLite => "SQLITE",
                _ => string.Empty
            };

            if (string.Equals(settingsProvider, provider, StringComparison.OrdinalIgnoreCase) ||
                (provider.Equals("MSSQL", StringComparison.OrdinalIgnoreCase) && settingsProvider == "SQLSERVER"))
            {
                return dbSettings.GetConnectionString();
            }
        }

        var directConnection = provider.ToUpperInvariant() switch
        {
            "POSTGRESQL" => _configuration.GetConnectionString("PostgreSQL"),
            "MSSQL" or "SQLSERVER" => _configuration.GetConnectionString("MSSQL") ?? _configuration.GetConnectionString("SqlServer"),
            "SQLITE" => _configuration.GetConnectionString("SQLite"),
            "MYSQL" => _configuration.GetConnectionString("MySQL"),
            _ => null
        };

        if (!string.IsNullOrWhiteSpace(directConnection))
            return directConnection;

        var defaultConnection = _configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(defaultConnection))
        {
            var inferredProvider = GetProviderFromConnectionString(defaultConnection);
            if (string.Equals(inferredProvider, provider, StringComparison.OrdinalIgnoreCase) ||
                (provider.Equals("MSSQL", StringComparison.OrdinalIgnoreCase) && inferredProvider == "SQLSERVER"))
            {
                return defaultConnection;
            }
        }

        return null;
    }

    private string GetProviderFromConnectionString(string connectionString)
    {
        if (connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase) ||
            connectionString.Contains("Username=", StringComparison.OrdinalIgnoreCase))
            return "POSTGRESQL";

        if (connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase))
            return "SQLITE";

        if (connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase) &&
            connectionString.Contains("Database=", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString.Contains("User", StringComparison.OrdinalIgnoreCase)
                ? "MYSQL"
                : "SQLSERVER";
        }

        return string.Empty;
    }

    private DatabaseSettings? ReadDatabaseSettings()
    {
        try
        {
            var dbSettingsPath = Path.Combine(_environment.ContentRootPath, "dbsettings.json");
            if (!File.Exists(dbSettingsPath))
                return null;

            var json = File.ReadAllText(dbSettingsPath);
            return JsonSerializer.Deserialize<DatabaseSettings>(json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "dbsettings.json okunamadi");
            return null;
        }
    }

    private static BackupResult CreateSuccessResult(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        return new BackupResult
        {
            Success = true,
            FileName = Path.GetFileName(filePath),
            FilePath = filePath,
            FileSizeBytes = fileInfo.Length,
            CreatedAt = DateTime.Now
        };
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

            if (backupFilePath.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
            {
                var connectionString = ResolveConnectionString("SQLite");
                var targetPath = connectionString?.Replace("Data Source=", string.Empty, StringComparison.OrdinalIgnoreCase).Trim().TrimEnd(';');
                if (!string.IsNullOrWhiteSpace(targetPath))
                {
                    if (!Path.IsPathRooted(targetPath))
                        targetPath = Path.Combine(_environment.ContentRootPath, targetPath);

                    File.Copy(backupFilePath, targetPath, overwrite: true);
                    _logger.LogInformation("SQLite restore basarili");
                    return true;
                }
            }

            if (backupFilePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("JSON yedekten geri yukleme henuz desteklenmiyor.");
                return false;
            }

            if (backupFilePath.EndsWith(".sql", StringComparison.OrdinalIgnoreCase) && dbProvider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
            {
                var connectionString = ResolveConnectionString("PostgreSQL");
                if (string.IsNullOrWhiteSpace(connectionString))
                    return false;

                var connParts = ParseConnectionString(connectionString);
                var psqlPath = FindPgDump()?.Replace("pg_dump", "psql", StringComparison.OrdinalIgnoreCase);

                if (!string.IsNullOrWhiteSpace(psqlPath) && File.Exists(psqlPath))
                {
                    var processInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = psqlPath,
                        Arguments = $"-h {connParts.GetValueOrDefault("Host", "localhost")} -p {connParts.GetValueOrDefault("Port", "5432")} -U {connParts.GetValueOrDefault("Username", string.Empty)} -d {connParts.GetValueOrDefault("Database", string.Empty)} -f \"{backupFilePath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    processInfo.Environment["PGPASSWORD"] = connParts.GetValueOrDefault("Password", string.Empty);

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

            if (!Directory.Exists(backupFolder))
                return;

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
        catch
        {
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
            _logger.LogError(ex, "Ayar kaydetme hatasi");
        }
    }

    private string GetBackupFolderPath(BackupSettings settings)
    {
        var folder = settings.BackupFolder;

        if (string.IsNullOrWhiteSpace(folder))
            folder = "Backups";

        if (!Path.IsPathRooted(folder))
            folder = Path.Combine(_environment.ContentRootPath, folder);

        return folder;
    }
}
