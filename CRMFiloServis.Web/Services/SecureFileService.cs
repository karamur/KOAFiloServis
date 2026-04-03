using CRMFiloServis.Web.Helpers;
using Microsoft.AspNetCore.DataProtection;

namespace CRMFiloServis.Web.Services;

public sealed class SecureFileService : ISecureFileService
{
    private readonly IDataProtector _protector;
    private readonly string _storageRoot;

    public SecureFileService(IDataProtectionProvider dataProtectionProvider, IWebHostEnvironment environment)
    {
        _protector = dataProtectionProvider.CreateProtector("CRMFiloServis.SecureFileStorage.v1");
        _storageRoot = AppStoragePaths.GetUploadsRoot(environment.ContentRootPath);
        Directory.CreateDirectory(_storageRoot);
    }

    public async Task<string> SaveEncryptedAsync(string relativeDirectory, string originalFileName, byte[] content, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(originalFileName);
        var safeName = string.Concat(Path.GetFileNameWithoutExtension(originalFileName)
            .Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch));

        var fileName = $"{safeName}_{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{extension}.enc";
        var relativePath = Path.Combine(relativeDirectory, fileName);
        var fullPath = ResolveFullPath(relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var encrypted = _protector.Protect(content);
        await File.WriteAllBytesAsync(fullPath, encrypted, cancellationToken);

        return relativePath.Replace('\\', '/');
    }

    public async Task<byte[]?> ReadDecryptedAsync(string? relativePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return null;

        var fullPath = ResolveFullPath(relativePath);
        if (!File.Exists(fullPath))
            return null;

        var encrypted = await File.ReadAllBytesAsync(fullPath, cancellationToken);
        return _protector.Unprotect(encrypted);
    }

    public Task DeleteAsync(string? relativePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return Task.CompletedTask;

        var fullPath = ResolveFullPath(relativePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string ResolveFullPath(string relativePath)
    {
        var normalized = relativePath
            .Replace('/', Path.DirectorySeparatorChar)
            .TrimStart(Path.DirectorySeparatorChar);

        var fullPath = Path.GetFullPath(Path.Combine(_storageRoot, normalized));
        var rootPath = Path.GetFullPath(_storageRoot);

        if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Gecersiz dosya yolu.");

        return fullPath;
    }
}
