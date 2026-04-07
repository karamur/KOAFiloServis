namespace CRMFiloServis.Web.Helpers;

public static class AppStoragePaths
{
    public const string DefaultInstallRoot = @"C:\KOAFiloServis";
    public const string DefaultStorageRoot = @"C:\KOAFiloServis_yedekleme";

    public static string GetStorageRoot(string contentRootPath)
    {
        var configured = Environment.GetEnvironmentVariable("CRMFILO_STORAGE_ROOT");
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return Path.GetFullPath(configured);
        }

        return DefaultStorageRoot;
    }

    public static string GetUploadsRoot(string contentRootPath)
        => Path.Combine(GetStorageRoot(contentRootPath), "uploads");

    public static string GetDatabaseBackupRoot(string contentRootPath)
        => Path.Combine(GetStorageRoot(contentRootPath), "database");

    public static string GetDataProtectionKeysRoot(string contentRootPath)
        => Path.Combine(GetStorageRoot(contentRootPath), "keys");
}
