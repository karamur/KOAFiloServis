using System.Security.Cryptography;

namespace KOAFiloServis.Web.Services.Security;

/// <summary>
/// Master key'i <c>{storageRoot}\keys\master.key</c> icinde DPAPI (LocalMachine) ile sifreli tutar.
/// Anahtar yoksa rastgele 32 byte uretip saklar. Thread-safe, singleton kullanima uygundur.
///
/// Platform: Windows. Linux/Mac uretimde calismaz (DPAPI yok) - o senaryoda
/// alternatif bir IMasterKeyProvider yazilip DI'da swap edilmelidir.
/// </summary>
public sealed class DpapiMasterKeyProvider : IMasterKeyProvider
{
    private const int KeyLength = 32; // AES-256
    private static readonly byte[] Entropy = "KOAFiloServis.MasterKey.v1"u8.ToArray();

    private readonly string _keyFilePath;
    private readonly ILogger<DpapiMasterKeyProvider> _logger;
    private readonly Lock _lock = new();
    private byte[]? _cachedKey;

    public DpapiMasterKeyProvider(string keyFilePath, ILogger<DpapiMasterKeyProvider> logger)
    {
        _keyFilePath = keyFilePath ?? throw new ArgumentNullException(nameof(keyFilePath));
        _logger = logger;
    }

    public ReadOnlyMemory<byte> GetMasterKey()
    {
        if (_cachedKey is not null)
        {
            return _cachedKey;
        }

        lock (_lock)
        {
            if (_cachedKey is not null)
            {
                return _cachedKey;
            }

            _cachedKey = LoadOrCreate();
            return _cachedKey;
        }
    }

    private byte[] LoadOrCreate()
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException(
                "DpapiMasterKeyProvider sadece Windows'ta calisir. Linux/Mac icin alternatif bir IMasterKeyProvider kullanin.");
        }

        var dir = Path.GetDirectoryName(_keyFilePath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        if (File.Exists(_keyFilePath))
        {
            try
            {
                var protectedBytes = File.ReadAllBytes(_keyFilePath);
                var plain = System.Security.Cryptography.ProtectedData.Unprotect(
                    protectedBytes, Entropy, DataProtectionScope.LocalMachine);

                if (plain.Length == KeyLength)
                {
                    _logger.LogInformation("Master key yuklendi: {Path}", _keyFilePath);
                    return plain;
                }

                _logger.LogWarning("Master key beklenmeyen uzunlukta ({Len} byte), yenisi uretiliyor.", plain.Length);
            }
            catch (CryptographicException ex)
            {
                _logger.LogError(ex,
                    "Master key cozulemedi (baska bir kullanici/makine ile sifrelenmis olabilir): {Path}. Yenisi uretiliyor.",
                    _keyFilePath);
                // Bozuk dosyayi yedekle
                var backup = _keyFilePath + ".corrupt." + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                try { File.Move(_keyFilePath, backup); } catch { /* yoksay */ }
            }
        }

        // Yeni anahtar uret
        var fresh = RandomNumberGenerator.GetBytes(KeyLength);
        var protectedFresh = System.Security.Cryptography.ProtectedData.Protect(
            fresh, Entropy, DataProtectionScope.LocalMachine);
        File.WriteAllBytes(_keyFilePath, protectedFresh);
        _logger.LogInformation("Yeni master key olusturuldu: {Path}", _keyFilePath);
        return fresh;
    }
}
