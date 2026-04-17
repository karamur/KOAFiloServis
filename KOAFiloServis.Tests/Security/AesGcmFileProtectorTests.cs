using System.Security.Cryptography;
using KOAFiloServis.Web.Services.Security;

namespace KOAFiloServis.Tests.Security;

public class AesGcmFileProtectorTests
{
    private sealed class FixedMasterKeyProvider : IMasterKeyProvider
    {
        private readonly byte[] _key;
        public FixedMasterKeyProvider(byte[] key) => _key = key;
        public ReadOnlyMemory<byte> GetMasterKey() => _key;
    }

    private static AesGcmFileProtector CreateSut(byte[]? key = null)
    {
        key ??= RandomNumberGenerator.GetBytes(32);
        return new AesGcmFileProtector(new FixedMasterKeyProvider(key));
    }

    [Fact]
    public void Protect_Unprotect_RoundTrip_EmptyInput()
    {
        var sut = CreateSut();
        var cipher = sut.Protect(ReadOnlySpan<byte>.Empty);
        var plain = sut.Unprotect(cipher);
        Assert.Empty(plain);
    }

    [Fact]
    public void Protect_Unprotect_RoundTrip_SmallPayload()
    {
        var sut = CreateSut();
        var data = "Merhaba KOAFiloServis - gizli PDF icerigi"u8.ToArray();

        var cipher = sut.Protect(data);
        var plain = sut.Unprotect(cipher);

        Assert.Equal(data, plain);
    }

    [Fact]
    public void Protect_Unprotect_RoundTrip_LargePayload()
    {
        var sut = CreateSut();
        var data = RandomNumberGenerator.GetBytes(1024 * 256); // 256 KB

        var cipher = sut.Protect(data);
        var plain = sut.Unprotect(cipher);

        Assert.Equal(data, plain);
    }

    [Fact]
    public void Protect_ProducesDifferentCipher_ForSameInput()
    {
        var sut = CreateSut();
        var data = "ayni girdi"u8.ToArray();

        var c1 = sut.Protect(data);
        var c2 = sut.Protect(data);

        Assert.NotEqual(c1, c2); // farkli nonce -> farkli cipher
    }

    [Fact]
    public void Unprotect_TamperedCipher_Throws()
    {
        var sut = CreateSut();
        var data = "dokunulmamasi gereken veri"u8.ToArray();
        var cipher = sut.Protect(data);

        // Son byte'i cevir (ciphertext kismi) -> tag dogrulamasi patlamali
        cipher[^1] ^= 0x01;

        Assert.ThrowsAny<CryptographicException>(() => sut.Unprotect(cipher));
    }

    [Fact]
    public void Unprotect_WithWrongKey_Throws()
    {
        var data = "gizli"u8.ToArray();
        var sut1 = CreateSut(RandomNumberGenerator.GetBytes(32));
        var cipher = sut1.Protect(data);

        var sut2 = CreateSut(RandomNumberGenerator.GetBytes(32));
        Assert.ThrowsAny<CryptographicException>(() => sut2.Unprotect(cipher));
    }

    [Fact]
    public void Unprotect_BadMagic_Throws()
    {
        var sut = CreateSut();
        var bogus = new byte[128];
        Assert.ThrowsAny<CryptographicException>(() => sut.Unprotect(bogus));
    }

    [Fact]
    public void ProtectFile_UnprotectFile_RoundTrip()
    {
        var sut = CreateSut();
        var tmp = Path.Combine(Path.GetTempPath(), $"koa-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tmp);
        try
        {
            var srcPath    = Path.Combine(tmp, "src.bin");
            var cipherPath = Path.Combine(tmp, "out.enc");
            var outPath    = Path.Combine(tmp, "decoded.bin");
            var data = RandomNumberGenerator.GetBytes(4096);
            File.WriteAllBytes(srcPath, data);

            sut.ProtectFile(srcPath, cipherPath);
            sut.UnprotectFile(cipherPath, outPath);

            Assert.Equal(data, File.ReadAllBytes(outPath));
        }
        finally
        {
            try { Directory.Delete(tmp, recursive: true); } catch { }
        }
    }
}
