using KOAFiloServis.Web.Services.Security;
using Microsoft.Extensions.Logging.Abstractions;

namespace KOAFiloServis.Tests.Security;

public class DpapiMasterKeyProviderTests
{
    [Fact]
    public void GetMasterKey_CreatesAndPersists_32ByteKey()
    {
        if (!OperatingSystem.IsWindows())
        {
            return; // DPAPI yok
        }

        var tmp = Path.Combine(Path.GetTempPath(), $"koa-keytest-{Guid.NewGuid():N}");
        var keyPath = Path.Combine(tmp, "master.key");
        try
        {
            var sut1 = new DpapiMasterKeyProvider(keyPath, NullLogger<DpapiMasterKeyProvider>.Instance);
            var key1 = sut1.GetMasterKey().ToArray();

            Assert.Equal(32, key1.Length);
            Assert.True(File.Exists(keyPath));

            // Dosyadaki byte'lar ham anahtar OLMAMALI (DPAPI ile sifreli)
            var onDisk = File.ReadAllBytes(keyPath);
            Assert.NotEqual(key1, onDisk);

            // Ikinci provider ayni dosyadan ayni anahtari yuklemeli
            var sut2 = new DpapiMasterKeyProvider(keyPath, NullLogger<DpapiMasterKeyProvider>.Instance);
            var key2 = sut2.GetMasterKey().ToArray();
            Assert.Equal(key1, key2);
        }
        finally
        {
            try { Directory.Delete(tmp, recursive: true); } catch { }
        }
    }

    [Fact]
    public void GetMasterKey_CachesAfterFirstCall()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var tmp = Path.Combine(Path.GetTempPath(), $"koa-keytest-{Guid.NewGuid():N}");
        var keyPath = Path.Combine(tmp, "master.key");
        try
        {
            var sut = new DpapiMasterKeyProvider(keyPath, NullLogger<DpapiMasterKeyProvider>.Instance);
            var k1 = sut.GetMasterKey();
            var k2 = sut.GetMasterKey();
            Assert.True(k1.Span.SequenceEqual(k2.Span));
        }
        finally
        {
            try { Directory.Delete(tmp, recursive: true); } catch { }
        }
    }
}
