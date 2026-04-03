namespace CRMFiloServis.Web.Services;

public interface ISecureFileService
{
    Task<string> SaveEncryptedAsync(string relativeDirectory, string originalFileName, byte[] content, CancellationToken cancellationToken = default);
    Task<byte[]?> ReadDecryptedAsync(string? relativePath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string? relativePath, CancellationToken cancellationToken = default);
}
