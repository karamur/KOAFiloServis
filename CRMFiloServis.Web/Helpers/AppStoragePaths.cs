namespace CRMFiloServis.Web.Helpers;

public static class AppStoragePaths
{
    public static string GetStorageRoot(string contentRootPath)
    {
        var configured = Environment.GetEnvironmentVariable("CRMFILO_STORAGE_ROOT");
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return Path.GetFullPath(configured);
        }

        var siblingRoot = Path.GetFullPath(Path.Combine(contentRootPath, "..", "..", "yedekleme"));
        if (Directory.Exists(siblingRoot))
        {
            return siblingRoot;
        }

        return Path.Combine(AppContext.BaseDirectory, "yedekleme");
    }

    public static string GetUploadsRoot(string contentRootPath)
        => Path.Combine(GetStorageRoot(contentRootPath), "uploads");

    public static string GetDatabaseBackupRoot(string contentRootPath)
        => Path.Combine(GetStorageRoot(contentRootPath), "database");

    public static string GetDataProtectionKeysRoot(string contentRootPath)
        => Path.Combine(GetStorageRoot(contentRootPath), "keys");
}
