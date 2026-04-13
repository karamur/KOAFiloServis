using CRMFiloServis.Web.Helpers;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;

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
        var relativePath = NormalizeRelativePath(Path.Combine(relativeDirectory, fileName));
        var fullPath = ResolveFullPath(relativePath);
        var encrypted = _protector.Protect(content);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllBytesAsync(fullPath, encrypted, cancellationToken);

        return relativePath;
    }

    public async Task<byte[]?> ReadDecryptedAsync(string? relativePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return null;

        var rawContent = await ReadRawAsync(relativePath, cancellationToken);
        if (rawContent == null)
            return null;

        try
        {
            return _protector.Unprotect(rawContent);
        }
        catch (CryptographicException)
        {
            return rawContent;
        }
    }

    public async Task DeleteAsync(string? relativePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return;

        var normalized = NormalizeRelativePath(relativePath);

        var fullPath = ResolveFullPath(normalized);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    private async Task<byte[]?> ReadRawAsync(string relativePath, CancellationToken cancellationToken)
    {
        var normalized = NormalizeRelativePath(relativePath);

        var fullPath = ResolveFullPath(normalized);
        if (!File.Exists(fullPath))
            return null;

        return await File.ReadAllBytesAsync(fullPath, cancellationToken);
    }

    private string ResolveFullPath(string relativePath)
    {
        var normalized = NormalizeRelativePath(relativePath)
            .Replace('/', Path.DirectorySeparatorChar);

        var fullPath = Path.GetFullPath(Path.Combine(_storageRoot, normalized));
        var rootPath = Path.GetFullPath(_storageRoot);

        if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Gecersiz dosya yolu.");

        return fullPath;
    }

    private static string NormalizeRelativePath(string relativePath)
    {
        var normalized = relativePath.Replace('\\', '/').TrimStart('/');
        if (normalized.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized.Substring("uploads/".Length);
        }

        return normalized;
    }
}
