using System.Security.Cryptography;

namespace KOAFiloServis.Web.Services.Security;

/// <summary>
/// AES-256-GCM tabanli dosya koruyucu.
///
/// Cikti formati (basit, ileri-uyumlu):
///   MAGIC (4) = "KOA1" | VERSION (1) = 0x01 | NONCE (12) | TAG (16) | CIPHER (n)
///
/// - Her cagri icin yeni, kriptografik rastgele nonce (12 B) uretilir.
/// - Authentication tag (16 B) otomatik olarak tutulur; bozuk/degistirilmis
///   dosyalarda <see cref="CryptographicException"/> atilir.
/// - Master key <see cref="IMasterKeyProvider"/>'dan gelir (32 B).
/// </summary>
public sealed class AesGcmFileProtector : IFileProtector
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private const int HeaderSize = 4 + 1 + NonceSize + TagSize; // MAGIC + VER + NONCE + TAG
    private static readonly byte[] Magic = "KOA1"u8.ToArray();
    private const byte Version = 0x01;

    private readonly IMasterKeyProvider _keyProvider;

    public AesGcmFileProtector(IMasterKeyProvider keyProvider)
    {
        _keyProvider = keyProvider;
    }

    public byte[] Protect(ReadOnlySpan<byte> plain)
    {
        var key = _keyProvider.GetMasterKey().Span;
        var output = new byte[HeaderSize + plain.Length];
        var span = output.AsSpan();

        // Header
        Magic.CopyTo(span[..4]);
        span[4] = Version;
        var nonce = span.Slice(5, NonceSize);
        RandomNumberGenerator.Fill(nonce);
        var tag = span.Slice(5 + NonceSize, TagSize);
        var cipher = span[HeaderSize..];

        using var aes = new AesGcm(key, TagSize);
        aes.Encrypt(nonce, plain, cipher, tag);
        return output;
    }

    public byte[] Unprotect(ReadOnlySpan<byte> cipher)
    {
        if (cipher.Length < HeaderSize)
        {
            throw new CryptographicException("Sifreli veri cok kucuk (header eksik).");
        }

        if (!cipher[..4].SequenceEqual(Magic))
        {
            throw new CryptographicException("Gecersiz dosya formati (magic number uyusmuyor).");
        }

        if (cipher[4] != Version)
        {
            throw new CryptographicException($"Desteklenmeyen surum: 0x{cipher[4]:X2}");
        }

        var nonce = cipher.Slice(5, NonceSize);
        var tag = cipher.Slice(5 + NonceSize, TagSize);
        var body = cipher[HeaderSize..];
        var plain = new byte[body.Length];

        var key = _keyProvider.GetMasterKey().Span;
        using var aes = new AesGcm(key, TagSize);
        aes.Decrypt(nonce, body, tag, plain);
        return plain;
    }

    public void ProtectFile(string plainPath, string cipherPath)
    {
        var plain = File.ReadAllBytes(plainPath);
        var cipher = Protect(plain);
        var dir = Path.GetDirectoryName(cipherPath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }
        File.WriteAllBytes(cipherPath, cipher);
    }

    public void UnprotectFile(string cipherPath, string plainPath)
    {
        var cipher = File.ReadAllBytes(cipherPath);
        var plain = Unprotect(cipher);
        var dir = Path.GetDirectoryName(plainPath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }
        File.WriteAllBytes(plainPath, plain);
    }
}
