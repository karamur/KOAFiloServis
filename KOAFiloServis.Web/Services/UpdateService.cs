using System.IO.Compression;
using System.Text.Json;

namespace KOAFiloServis.Web.Services;

/// <summary>
/// Güncelleme bilgisi modeli
/// </summary>
public class UpdateInfo
{
    public string Version { get; set; } = "";
    public string BuildDate { get; set; } = "";
    public string BuildNumber { get; set; } = "";
    public string Framework { get; set; } = "";
    public string Description { get; set; } = "";
}

/// <summary>
/// Güncelleme manifest modeli
/// </summary>
public class UpdateManifest
{
    public string AppName { get; set; } = "";
    public string CurrentVersion { get; set; } = "";
    public string ReleaseDate { get; set; } = "";
    public string UpdateUrl { get; set; } = "";
    public List<string> ChangeLog { get; set; } = new();
}

/// <summary>
/// Program içinden güncelleme yapılmasını sağlayan servis
/// </summary>
public class UpdateService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<UpdateService> _logger;
    private readonly string _artifactsPath;
    private readonly string _currentVersion;

    public UpdateService(IWebHostEnvironment environment, ILogger<UpdateService> logger)
    {
        _environment = environment;
        _logger = logger;
        _artifactsPath = Path.Combine(Directory.GetParent(_environment.ContentRootPath)?.FullName ?? "", "artifacts");
        _currentVersion = GetCurrentVersion();
    }

    /// <summary>
    /// Mevcut uygulama versiyonunu döner
    /// </summary>
    public string GetCurrentVersion()
    {
        try
        {
            var versionFile = Path.Combine(_artifactsPath, "version.json");
            if (File.Exists(versionFile))
            {
                var json = File.ReadAllText(versionFile);
                var info = JsonSerializer.Deserialize<UpdateInfo>(json);
                return info?.Version ?? "1.0.0";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Versiyon dosyası okunamadı");
        }
        return "1.0.0";
    }

    /// <summary>
    /// Güncelleme bilgilerini döner
    /// </summary>
    public async Task<UpdateInfo?> GetUpdateInfoAsync()
    {
        try
        {
            var versionFile = Path.Combine(_artifactsPath, "version.json");
            if (File.Exists(versionFile))
            {
                var json = await File.ReadAllTextAsync(versionFile);
                return JsonSerializer.Deserialize<UpdateInfo>(json);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Güncelleme bilgisi alınamadı");
        }
        return null;
    }

    /// <summary>
    /// Güncelleme manifest dosyasını döner
    /// </summary>
    public async Task<UpdateManifest?> GetUpdateManifestAsync()
    {
        try
        {
            var manifestFile = Path.Combine(_artifactsPath, "update-manifest.json");
            if (File.Exists(manifestFile))
            {
                var json = await File.ReadAllTextAsync(manifestFile);
                return JsonSerializer.Deserialize<UpdateManifest>(json);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Manifest dosyası alınamadı");
        }
        return null;
    }

    /// <summary>
    /// Mevcut güncelleme paketlerini listeler
    /// </summary>
    public List<string> GetAvailableUpdatePackages()
    {
        var packages = new List<string>();
        try
        {
            if (Directory.Exists(_artifactsPath))
            {
                packages = Directory.GetFiles(_artifactsPath, "KOAFiloServis_Update_*.zip")
                    .Select(Path.GetFileName)
                    .Where(f => f != null)
                    .Cast<string>()
                    .OrderByDescending(f => f)
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Güncelleme paketleri listelenemedi");
        }
        return packages;
    }

    /// <summary>
    /// Güncelleme paketini yükler ve uygular
    /// </summary>
    public async Task<(bool Success, string Message)> ApplyUpdateAsync(string packageName)
    {
        try
        {
            var packagePath = Path.Combine(_artifactsPath, packageName);
            if (!File.Exists(packagePath))
            {
                return (false, "Güncelleme paketi bulunamadı");
            }

            var backupPath = Path.Combine(_artifactsPath, "backup_before_update");
            var publishPath = Path.Combine(_artifactsPath, "publish");

            // Mevcut dosyaları yedekle
            if (Directory.Exists(publishPath))
            {
                if (Directory.Exists(backupPath))
                    Directory.Delete(backupPath, true);

                Directory.CreateDirectory(backupPath);
                foreach (var file in Directory.GetFiles(publishPath))
                {
                    var destFile = Path.Combine(backupPath, Path.GetFileName(file));
                    File.Copy(file, destFile, true);
                }
                _logger.LogInformation("Yedekleme tamamlandı: {BackupPath}", backupPath);
            }

            // Güncelleme paketini aç
            var tempExtractPath = Path.Combine(_artifactsPath, "temp_update");
            if (Directory.Exists(tempExtractPath))
                Directory.Delete(tempExtractPath, true);

            ZipFile.ExtractToDirectory(packagePath, tempExtractPath);
            _logger.LogInformation("Paket açıldı: {PackagePath}", packagePath);

            // Dosyaları kopyala
            foreach (var file in Directory.GetFiles(tempExtractPath, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(tempExtractPath, file);
                var destPath = Path.Combine(publishPath, relativePath);
                var destDir = Path.GetDirectoryName(destPath);
                
                if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                    Directory.CreateDirectory(destDir);

                File.Copy(file, destPath, true);
            }

            // Temp klasörünü temizle
            Directory.Delete(tempExtractPath, true);

            _logger.LogInformation("Güncelleme başarıyla uygulandı: {Package}", packageName);
            return (true, "Güncelleme başarıyla uygulandı. Uygulamayı yeniden başlatmanız gerekiyor.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Güncelleme uygulanırken hata oluştu");
            return (false, $"Güncelleme hatası: {ex.Message}");
        }
    }

    /// <summary>
    /// Harici güncelleme dosyası yükler
    /// </summary>
    public async Task<(bool Success, string Message)> UploadUpdatePackageAsync(Stream fileStream, string fileName)
    {
        try
        {
            if (!fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                return (false, "Sadece ZIP dosyaları kabul edilmektedir");
            }

            var destPath = Path.Combine(_artifactsPath, fileName);
            
            using var fs = new FileStream(destPath, FileMode.Create);
            await fileStream.CopyToAsync(fs);

            _logger.LogInformation("Güncelleme paketi yüklendi: {FileName}", fileName);
            return (true, $"Güncelleme paketi başarıyla yüklendi: {fileName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Güncelleme paketi yüklenemedi");
            return (false, $"Yükleme hatası: {ex.Message}");
        }
    }

    /// <summary>
    /// Artifacts dizin yolunu döner
    /// </summary>
    public string GetArtifactsPath() => _artifactsPath;

    /// <summary>
    /// Publish dizin yolunu döner
    /// </summary>
    public string GetPublishPath() => Path.Combine(_artifactsPath, "publish");
}
