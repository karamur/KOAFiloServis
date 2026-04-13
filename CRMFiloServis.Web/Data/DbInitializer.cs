using CRMFiloServis.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;
using System.Data.Common;

namespace CRMFiloServis.Web.Data;

public static class DbInitializer
{
    private const string AracSasePlakaMigrationId = "20260326224724_AracSasePlakaYapisi";

    public static async Task InitializeAsync(ApplicationDbContext context, IConfiguration configuration)
    {
        var dbProvider = context.Database.IsNpgsql()
            ? "PostgreSQL"
            : context.Database.IsSqlite()
                ? "SQLite"
                : configuration.GetValue<string>("DatabaseProvider") ?? "SQLite";
        var migrationRecovered = false;
        
        try
        {
            // Veritabani baglantisini kontrol et
            if (!await context.Database.CanConnectAsync())
            {
                throw new Exception("Veritabani baglantisi kurulamadi!");
            }

            if (context.Database.IsNpgsql())
            {
                await NormalizePostgreSqlAuditTimestampColumnsAsync(context, configuration);
            }

            // Bekleyen migration'lari uygula (yeni tablolar icin)
            var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
            if (context.Database.IsSqlite() && pendingMigrations.Any())
            {
                await EnsureSqliteMigrationHistoryAsync(context, pendingMigrations);
                pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
            }

            if (pendingMigrations.Any())
            {
                Console.WriteLine($"Bekleyen migration sayisi: {pendingMigrations.Count()}");
                await context.Database.MigrateAsync();
                Console.WriteLine("Migration'lar basariyla uygulandi.");
            }
        }
        catch (Exception ex)
        {
            if (context.Database.IsSqlite())
            {
                try
                {
                    var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
                    if (pendingMigrations.Any())
                    {
                        await EnsureSqliteMigrationHistoryAsync(context, pendingMigrations);
                        pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
                        if (!pendingMigrations.Any())
                        {
                            migrationRecovered = true;
                            Console.WriteLine("SQLite migration gecmisi mevcut tablo yapisina gore duzeltildi.");
                        }
                    }
                }
                catch (Exception sqliteFixEx)
                {
                    Console.WriteLine($"SQLite migration duzeltme hatasi: {sqliteFixEx.Message}");
                }
            }

            if (migrationRecovered)
            {
                Console.WriteLine("Migration kurtarma islemi tamamlandi, startup yardimcilari calistiriliyor.");
            }
            else
            {
                throw new InvalidOperationException(
                    "Veritabani migration islemi tamamlanamadi. Uygulama tutarsiz sema ile devam ettirilmedi.",
                    ex);
            }
        }

        // PostgreSQL için eksik kolonları ekle
        if (dbProvider == "PostgreSQL")
        {
            await EnsurePostgreSqlMissingColumnsAsync(context, configuration);
        }

        // PiyasaKaynaklar tablosunu oluştur
        await EnsurePiyasaKaynaklarTableAsync(context, dbProvider, configuration);

        // TekrarlayanOdemeler tablosunu oluştur
        await EnsureTekrarlayanOdemelerTableAsync(context, dbProvider, configuration);

        // Roller tablosuna Renk kolonu ekle (yoksa)
        await EnsureRollerRenkColumnAsync(context, dbProvider, configuration);

        // PuantajKayitlar tablosuna yeni kolonları ekle
        await EnsurePuantajKayitlarColumnsAsync(context, dbProvider, configuration);

        // Destek modulu tablolarini kontrol et / olustur
        await EnsureDestekModuluTablesAsync(context, dbProvider, configuration);

        // Destek modulu eksik kolonlarini tamamla
        await EnsureDestekModuluColumnsAsync(context, dbProvider, configuration);

        // Budget masraf kalemleri her zaman kontrol et
        await SeedBudgetMasrafKalemleriAsync(context);
        await SeedAracMasrafKalemleriAsync(context);

        // Destek Talebi seed verilerini ekle
        await SeedDestekTalebiVerileriAsync(context);

        // EBYS örnek veri ve test senaryoları
        await SeedEbysOrnekVerileriAsync(context);
    }

    private static string GetDefaultConnectionString(ApplicationDbContext context, IConfiguration configuration)
    {
        return context.Database.GetConnectionString()
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection bulunamadi.");
    }

    private static async Task NormalizePostgreSqlAuditTimestampColumnsAsync(ApplicationDbContext context, IConfiguration configuration)
    {
        var connectionString = GetDefaultConnectionString(context, configuration);
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        const string findColumnsSql = @"
SELECT c.table_name, c.column_name, c.column_default
FROM information_schema.columns c
INNER JOIN information_schema.tables t
    ON t.table_schema = c.table_schema
   AND t.table_name = c.table_name
WHERE c.table_schema = current_schema()
  AND t.table_type = 'BASE TABLE'
  AND c.data_type = 'timestamp with time zone'
ORDER BY CASE WHEN c.table_name IN ('Soforler', 'Personeller') THEN 0 ELSE 1 END,
         c.table_name,
         c.ordinal_position;";

        var targets = new List<(string TableName, string ColumnName, string? ColumnDefault)>();
        await using (var findCmd = new NpgsqlCommand(findColumnsSql, conn))
        await using (var reader = await findCmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                targets.Add((
                    reader.GetString(0),
                    reader.GetString(1),
                    reader.IsDBNull(2) ? null : reader.GetString(2)));
            }
        }

        foreach (var (tableName, columnName, columnDefault) in targets)
        {
            var escapedTableName = tableName.Replace("\"", "\"\"");
            var escapedColumnName = columnName.Replace("\"", "\"\"");

            if (!string.IsNullOrWhiteSpace(columnDefault))
            {
                var dropDefaultSql = $@"ALTER TABLE ""{escapedTableName}""
ALTER COLUMN ""{escapedColumnName}"" DROP DEFAULT;";

                await using var dropDefaultCmd = new NpgsqlCommand(dropDefaultSql, conn);
                await dropDefaultCmd.ExecuteNonQueryAsync();
            }

            var alterSql = $@"ALTER TABLE ""{escapedTableName}""
ALTER COLUMN ""{escapedColumnName}"" TYPE timestamp without time zone
USING ""{escapedColumnName}"" AT TIME ZONE 'UTC';";

            await using (var alterCmd = new NpgsqlCommand(alterSql, conn))
            {
                await alterCmd.ExecuteNonQueryAsync();
            }

            var normalizedDefaultSql = NormalizeTimestampDefaultExpression(columnDefault);
            if (!string.IsNullOrWhiteSpace(normalizedDefaultSql))
            {
                var setDefaultSql = $@"ALTER TABLE ""{escapedTableName}""
ALTER COLUMN ""{escapedColumnName}"" SET DEFAULT {normalizedDefaultSql};";

                await using var setDefaultCmd = new NpgsqlCommand(setDefaultSql, conn);
                await setDefaultCmd.ExecuteNonQueryAsync();
            }
        }

        if (targets.Count > 0)
        {
            Console.WriteLine($"PostgreSQL timestamp kolonlari normalize edildi: {targets.Count}");
        }
    }

    private static string? NormalizeTimestampDefaultExpression(string? columnDefault)
    {
        if (string.IsNullOrWhiteSpace(columnDefault))
        {
            return null;
        }

        if (columnDefault.Contains("now()", StringComparison.OrdinalIgnoreCase)
            || columnDefault.Contains("current_timestamp", StringComparison.OrdinalIgnoreCase)
            || columnDefault.Contains("statement_timestamp()", StringComparison.OrdinalIgnoreCase)
            || columnDefault.Contains("transaction_timestamp()", StringComparison.OrdinalIgnoreCase))
        {
            return "LOCALTIMESTAMP";
        }

        return null;
    }

    private static async Task EnsurePiyasaKaynaklarTableAsync(ApplicationDbContext context, string dbProvider, IConfiguration configuration)
    {
        try
        {
            if (dbProvider == "PostgreSQL")
            {
                var connectionString = GetDefaultConnectionString(context, configuration);
                using var conn = new NpgsqlConnection(connectionString);
                await conn.OpenAsync();
                
                // Tablo var mı kontrol et
                using var checkCmd = new NpgsqlCommand(
                    "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'PiyasaKaynaklar')", conn);
                var exists = (bool)(await checkCmd.ExecuteScalarAsync() ?? false);
                
                if (!exists)
                {
                    using var createCmd = new NpgsqlCommand(@"
                        CREATE TABLE ""PiyasaKaynaklar"" (
                            ""Id"" SERIAL PRIMARY KEY,
                            ""Ad"" VARCHAR(100) NOT NULL,
                            ""Kod"" VARCHAR(50) NOT NULL,
                            ""BaseUrl"" VARCHAR(500) NOT NULL,
                            ""AramaUrl"" VARCHAR(500),
                            ""AramaParametreleri"" VARCHAR(1000),
                            ""Selectors"" VARCHAR(2000),
                            ""DesteklenenMarkalar"" VARCHAR(500),
                            ""KaynakTipi"" VARCHAR(50) DEFAULT 'Genel',
                            ""Sira"" INTEGER DEFAULT 99,
                            ""Aktif"" BOOLEAN DEFAULT TRUE,
                            ""OlusturmaTarihi"" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                            ""GuncellemeTarihi"" TIMESTAMP,
                            ""IsDeleted"" BOOLEAN DEFAULT FALSE
                        )", conn);
                    await createCmd.ExecuteNonQueryAsync();
                    Console.WriteLine("PiyasaKaynaklar tablosu PostgreSQL'de oluşturuldu.");
                }
                else
                {
                    Console.WriteLine("PiyasaKaynaklar tablosu zaten mevcut.");
                }
            }
            else
            {
                // SQLite için
                await context.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS ""PiyasaKaynaklar"" (
                        ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                        ""Ad"" TEXT NOT NULL,
                        ""Kod"" TEXT NOT NULL,
                        ""BaseUrl"" TEXT NOT NULL,
                        ""AramaUrl"" TEXT,
                        ""AramaParametreleri"" TEXT,
                        ""Selectors"" TEXT,
                        ""DesteklenenMarkalar"" TEXT,
                        ""KaynakTipi"" TEXT DEFAULT 'Genel',
                        ""Sira"" INTEGER DEFAULT 99,
                        ""Aktif"" INTEGER DEFAULT 1,
                        ""OlusturmaTarihi"" TEXT DEFAULT CURRENT_TIMESTAMP,
                        ""GuncellemeTarihi"" TEXT,
                        ""IsDeleted"" INTEGER DEFAULT 0
                    )");
                Console.WriteLine("PiyasaKaynaklar tablosu SQLite'da kontrol edildi.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PiyasaKaynaklar tablo oluşturma hatası: {ex.Message}");
        }
    }

    private static async Task EnsureTekrarlayanOdemelerTableAsync(ApplicationDbContext context, string dbProvider, IConfiguration configuration)
    {
        try
        {
            if (dbProvider == "PostgreSQL")
            {
                var connectionString = GetDefaultConnectionString(context, configuration);
                using var conn = new NpgsqlConnection(connectionString);
                await conn.OpenAsync();

                using var checkCmd = new NpgsqlCommand(
                    "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'TekrarlayanOdemeler')", conn);
                var exists = (bool)(await checkCmd.ExecuteScalarAsync() ?? false);

                if (!exists)
                {
                    using var createCmd = new NpgsqlCommand(@"
                        CREATE TABLE ""TekrarlayanOdemeler"" (
                            ""Id"" SERIAL PRIMARY KEY,
                            ""OdemeAdi"" VARCHAR(200) NOT NULL,
                            ""MasrafKalemi"" VARCHAR(200) NOT NULL,
                            ""Aciklama"" VARCHAR(500),
                            ""Tutar"" NUMERIC(18,2) NOT NULL,
                            ""Periyod"" INTEGER NOT NULL DEFAULT 1,
                            ""OdemeGunu"" INTEGER NOT NULL DEFAULT 1,
                            ""BaslangicTarihi"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                            ""BitisTarihi"" TIMESTAMP,
                            ""HatirlatmaGunSayisi"" INTEGER NOT NULL DEFAULT 3,
                            ""FirmaId"" INTEGER REFERENCES ""Firmalar""(""Id"") ON DELETE SET NULL,
                            ""Aktif"" BOOLEAN NOT NULL DEFAULT TRUE,
                            ""Renk"" VARCHAR(20) DEFAULT '#dc3545',
                            ""Icon"" VARCHAR(50) DEFAULT 'bi-arrow-repeat',
                            ""Notlar"" VARCHAR(1000),
                            ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                            ""UpdatedAt"" TIMESTAMP,
                            ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                        );
                        CREATE INDEX ""IX_TekrarlayanOdemeler_FirmaId"" ON ""TekrarlayanOdemeler"" (""FirmaId"");
                        CREATE INDEX ""IX_TekrarlayanOdemeler_MasrafKalemi"" ON ""TekrarlayanOdemeler"" (""MasrafKalemi"");
                    ", conn);
                    await createCmd.ExecuteNonQueryAsync();
                    Console.WriteLine("TekrarlayanOdemeler tablosu PostgreSQL'de oluşturuldu.");
                }
                else
                {
                    Console.WriteLine("TekrarlayanOdemeler tablosu zaten mevcut.");
                }
            }
            else
            {
                // SQLite icin
                await context.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS ""TekrarlayanOdemeler"" (
                        ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                        ""OdemeAdi"" TEXT NOT NULL,
                        ""MasrafKalemi"" TEXT NOT NULL,
                        ""Aciklama"" TEXT,
                        ""Tutar"" REAL NOT NULL,
                        ""Periyod"" INTEGER NOT NULL DEFAULT 1,
                        ""OdemeGunu"" INTEGER NOT NULL DEFAULT 1,
                        ""BaslangicTarihi"" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""BitisTarihi"" TEXT,
                        ""HatirlatmaGunSayisi"" INTEGER NOT NULL DEFAULT 3,
                        ""FirmaId"" INTEGER,
                        ""Aktif"" INTEGER NOT NULL DEFAULT 1,
                        ""Renk"" TEXT DEFAULT '#dc3545',
                        ""Icon"" TEXT DEFAULT 'bi-arrow-repeat',
                        ""Notlar"" TEXT,
                        ""CreatedAt"" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TEXT,
                        ""IsDeleted"" INTEGER NOT NULL DEFAULT 0
                    )");
                Console.WriteLine("TekrarlayanOdemeler tablosu SQLite'da kontrol edildi.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"TekrarlayanOdemeler tablo oluşturma hatası: {ex.Message}");
        }
    }

    private static async Task EnsureRollerRenkColumnAsync(ApplicationDbContext context, string dbProvider, IConfiguration configuration)
    {
        try
        {
            if (dbProvider == "PostgreSQL")
            {
                var connectionString = GetDefaultConnectionString(context, configuration);
                using var conn = new NpgsqlConnection(connectionString);
                await conn.OpenAsync();

                // Önce tablo var mı kontrol et
                using var tableCheckCmd = new NpgsqlCommand(
                    "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Roller')", conn);
                var tableExists = (bool)(await tableCheckCmd.ExecuteScalarAsync() ?? false);

                if (!tableExists)
                {
                    Console.WriteLine("Roller tablosu henüz oluşturulmamış, kolon ekleme atlanıyor.");
                    return;
                }

                // Kolon var mı kontrol et
                using var checkCmd = new NpgsqlCommand(
                    "SELECT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Roller' AND column_name = 'Renk')", conn);
                var exists = (bool)(await checkCmd.ExecuteScalarAsync() ?? false);

                if (!exists)
                {
                    using var addColumnCmd = new NpgsqlCommand(
                        "ALTER TABLE \"Roller\" ADD COLUMN \"Renk\" VARCHAR(20) DEFAULT '#dc3545'", conn);
                    await addColumnCmd.ExecuteNonQueryAsync();
                    Console.WriteLine("Roller tablosuna Renk kolonu eklendi.");
                }
                else
                {
                    Console.WriteLine("Roller tablosunda Renk kolonu zaten mevcut.");
                }
            }
            else
            {
                // SQLite için
                var connection = context.Database.GetDbConnection();
                var shouldClose = connection.State != System.Data.ConnectionState.Open;

                if (shouldClose)
                {
                    await connection.OpenAsync();
                }

                try
                {
                    await using var checkCommand = connection.CreateCommand();
                    checkCommand.CommandText = "SELECT 1 FROM pragma_table_info('Roller') WHERE name = $columnName LIMIT 1";
                    var parameter = checkCommand.CreateParameter();
                    parameter.ParameterName = "$columnName";
                    parameter.Value = "Renk";
                    checkCommand.Parameters.Add(parameter);

                    var exists = await checkCommand.ExecuteScalarAsync() is not null;
                    if (!exists)
                    {
                        await using var alterCommand = connection.CreateCommand();
                        alterCommand.CommandText = "ALTER TABLE \"Roller\" ADD COLUMN \"Renk\" TEXT DEFAULT '#dc3545'";
                        await alterCommand.ExecuteNonQueryAsync();
                        Console.WriteLine("Roller tablosuna Renk kolonu eklendi.");
                    }
                    else
                    {
                        Console.WriteLine("Roller tablosunda Renk kolonu zaten mevcut.");
                    }
                }
                finally
                {
                    if (shouldClose)
                    {
                        await connection.CloseAsync();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Roller Renk kolonu ekleme hatası: {ex.Message}");
        }
    }

    private static async Task EnsurePuantajKayitlarColumnsAsync(ApplicationDbContext context, string dbProvider, IConfiguration configuration)
    {
        try
        {
            if (dbProvider != "PostgreSQL") return;

            var connectionString = GetDefaultConnectionString(context, configuration);
            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            // Önce tablo var mı kontrol et
            using var tableCheckCmd = new NpgsqlCommand(
                "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'PuantajKayitlar')", conn);
            var tableExists = (bool)(await tableCheckCmd.ExecuteScalarAsync() ?? false);

            if (!tableExists)
            {
                Console.WriteLine("PuantajKayitlar tablosu henüz oluşturulmamış, kolon ekleme atlanıyor.");
                return;
            }

            // Eklenecek kolonlar: Bolge, SiraNo, AitFirmaAdi, Gun01-Gun31
            var kolonlar = new List<(string kolon, string tip, string varsayilan)>
            {
                ("Bolge", "VARCHAR(100)", "NULL"),
                ("SiraNo", "INTEGER", "0"),
                ("AitFirmaAdi", "VARCHAR(200)", "NULL"),
            };

            // Gun01 - Gun31
            for (int g = 1; g <= 31; g++)
                kolonlar.Add(($"Gun{g:D2}", "INTEGER", "0"));

            foreach (var (kolon, tip, varsayilan) in kolonlar)
            {
                using var checkCmd = new NpgsqlCommand(
                    $"SELECT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'PuantajKayitlar' AND column_name = '{kolon}')", conn);
                var exists = (bool)(await checkCmd.ExecuteScalarAsync() ?? false);

                if (!exists)
                {
                    var defaultClause = varsayilan == "NULL" ? "" : $" DEFAULT {varsayilan}";
                    var nullClause = varsayilan == "NULL" ? "" : " NOT NULL";
                    using var addCmd = new NpgsqlCommand(
                        $"ALTER TABLE \"PuantajKayitlar\" ADD COLUMN \"{kolon}\" {tip}{nullClause}{defaultClause}", conn);
                    await addCmd.ExecuteNonQueryAsync();
                    Console.WriteLine($"PuantajKayitlar tablosuna {kolon} kolonu eklendi.");
                }
            }

            Console.WriteLine("PuantajKayitlar tablo kolon kontrolü tamamlandı.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PuantajKayitlar kolon ekleme hatası: {ex.Message}");
        }
    }

    private static async Task EnsureSqliteMigrationHistoryAsync(ApplicationDbContext context, List<string> pendingMigrations)
    {
        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;

        if (shouldClose)
        {
            await connection.OpenAsync();
        }

        try
        {
            await using var tableCheck = connection.CreateCommand();
            tableCheck.CommandText = "SELECT 1 FROM sqlite_master WHERE type = 'table' AND name <> '__EFMigrationsHistory' AND name NOT LIKE 'sqlite_%' LIMIT 1";
            var hasUserTables = await tableCheck.ExecuteScalarAsync() is not null;
            if (!hasUserTables)
            {
                return;
            }

            await using var historyCreate = connection.CreateCommand();
            historyCreate.CommandText = @"
                CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
                    ""MigrationId"" TEXT NOT NULL CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY,
                    ""ProductVersion"" TEXT NOT NULL
                );";
            await historyCreate.ExecuteNonQueryAsync();

            var existingMigrationIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using (var historyReader = connection.CreateCommand())
            {
                historyReader.CommandText = "SELECT \"MigrationId\" FROM \"__EFMigrationsHistory\"";
                await using var reader = await historyReader.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    existingMigrationIds.Add(reader.GetString(0));
                }
            }

            var allMigrations = context.Database.GetMigrations().ToList();
            var migrationsToBaseline = allMigrations
                .Where(pendingMigrations.Contains)
                .Where(migrationId => !existingMigrationIds.Contains(migrationId))
                .ToList();

            if (migrationsToBaseline.Count == 0)
            {
                return;
            }

            await using var insertHistory = connection.CreateCommand();
            insertHistory.CommandText = "INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ($id, $version)";

            var idParameter = insertHistory.CreateParameter();
            idParameter.ParameterName = "$id";
            idParameter.Value = string.Empty;
            insertHistory.Parameters.Add(idParameter);

            var versionParameter = insertHistory.CreateParameter();
            versionParameter.ParameterName = "$version";
            versionParameter.Value = "10.0.5";
            insertHistory.Parameters.Add(versionParameter);

            foreach (var migrationId in migrationsToBaseline)
            {
                idParameter.Value = migrationId;
                await insertHistory.ExecuteNonQueryAsync();
            }

            Console.WriteLine($"SQLite migration history baselined: {migrationsToBaseline.Count} migration");
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }

    // Eski metod - geriye donuk uyumluluk icin
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        try
        {
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Veritabani migration islemi tamamlanamadi. Uygulama tutarsiz sema ile devam ettirilmedi.",
                ex);
        }

        var dbProvider = context.Database.IsNpgsql() ? "PostgreSQL" : context.Database.IsSqlite() ? "SQLite" : "SQLite";
        await EnsureDestekModuluTablesAsync(context, dbProvider, null);
        await EnsureDestekModuluColumnsAsync(context, dbProvider, null);

        await SeedBudgetMasrafKalemleriAsync(context);
        await SeedAracMasrafKalemleriAsync(context);

        // Destek Talebi seed verilerini ekle
        await SeedDestekTalebiVerileriAsync(context);

        // EBYS örnek veri ve test senaryoları
        await SeedEbysOrnekVerileriAsync(context);
    }

    private static async Task EnsureDestekModuluTablesAsync(ApplicationDbContext context, string dbProvider, IConfiguration? configuration)
    {
        try
        {
            if (dbProvider == "PostgreSQL")
            {
                var connectionString = GetDefaultConnectionString(context, configuration ?? new ConfigurationBuilder().Build());
                using var conn = new NpgsqlConnection(connectionString);
                await conn.OpenAsync();

                using var createCmd = new NpgsqlCommand(@"
                    CREATE TABLE IF NOT EXISTS ""DestekDepartmanlari"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""Ad"" VARCHAR(200) NOT NULL,
                        ""Aciklama"" TEXT,
                        ""Email"" VARCHAR(200),
                        ""OtomatikAtama"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""VarsayilanSlaSuresi"" INTEGER,
                        ""SiraNo"" INTEGER NOT NULL DEFAULT 0,
                        ""Aktif"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ""UstDepartmanId"" INTEGER NULL REFERENCES ""DestekDepartmanlari""(""Id"") ON DELETE SET NULL,
                        ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP NULL,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    );

                    CREATE TABLE IF NOT EXISTS ""DestekKategorileri"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""Ad"" VARCHAR(200) NOT NULL,
                        ""Aciklama"" TEXT,
                        ""Renk"" VARCHAR(50),
                        ""Simge"" VARCHAR(100),
                        ""SiraNo"" INTEGER NOT NULL DEFAULT 0,
                        ""Aktif"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ""DepartmanId"" INTEGER NULL REFERENCES ""DestekDepartmanlari""(""Id"") ON DELETE SET NULL,
                        ""UstKategoriId"" INTEGER NULL REFERENCES ""DestekKategorileri""(""Id"") ON DELETE SET NULL,
                        ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP NULL,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    );

                    CREATE TABLE IF NOT EXISTS ""DestekSlaListesi"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""Ad"" VARCHAR(200) NOT NULL,
                        ""Aciklama"" TEXT,
                        ""IlkYanitSuresi"" INTEGER NOT NULL,
                        ""CozumSuresi"" INTEGER NOT NULL,
                        ""Oncelik"" INTEGER NOT NULL,
                        ""Aktif"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ""SadeceMesaiSaatleri"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ""SadeceHaftaIci"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP NULL,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    );

                    CREATE TABLE IF NOT EXISTS ""DestekHazirYanitlari"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""Ad"" VARCHAR(200) NOT NULL,
                        ""Icerik"" TEXT NOT NULL,
                        ""KonuSablonu"" VARCHAR(500),
                        ""Aciklama"" TEXT,
                        ""Aktif"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ""SiraNo"" INTEGER NOT NULL DEFAULT 0,
                        ""DepartmanId"" INTEGER NULL REFERENCES ""DestekDepartmanlari""(""Id"") ON DELETE SET NULL,
                        ""KategoriId"" INTEGER NULL REFERENCES ""DestekKategorileri""(""Id"") ON DELETE SET NULL,
                        ""KullanimSayisi"" INTEGER NOT NULL DEFAULT 0,
                        ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP NULL,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    );

                    CREATE TABLE IF NOT EXISTS ""DestekAyarlari"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""Anahtar"" VARCHAR(200) NOT NULL,
                        ""Deger"" TEXT NOT NULL,
                        ""Aciklama"" TEXT,
                        ""Grup"" VARCHAR(100),
                        ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP NULL,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    );

                    CREATE TABLE IF NOT EXISTS ""DestekTalepleri"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""TalepNo"" VARCHAR(50) NOT NULL,
                        ""Konu"" VARCHAR(500) NOT NULL,
                        ""Aciklama"" TEXT NOT NULL,
                        ""Durum"" INTEGER NOT NULL DEFAULT 1,
                        ""Oncelik"" INTEGER NOT NULL DEFAULT 2,
                        ""Kaynak"" INTEGER NOT NULL DEFAULT 1,
                        ""SlaSuresi"" INTEGER NULL,
                        ""SlaBitisTarihi"" TIMESTAMP NULL,
                        ""SlaAsildi"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""SonAktiviteTarihi"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""KapatilmaTarihi"" TIMESTAMP NULL,
                        ""CozumSuresiDakika"" INTEGER NULL,
                        ""IlkYanitSuresiDakika"" INTEGER NULL,
                        ""MemnuniyetPuani"" INTEGER NULL,
                        ""MemnuniyetYorumu"" TEXT,
                        ""DahiliNotlar"" TEXT,
                        ""Etiketler"" TEXT,
                        ""DepartmanId"" INTEGER NOT NULL REFERENCES ""DestekDepartmanlari""(""Id"") ON DELETE RESTRICT,
                        ""KategoriId"" INTEGER NULL REFERENCES ""DestekKategorileri""(""Id"") ON DELETE SET NULL,
                        ""AtananKullaniciId"" INTEGER NULL REFERENCES ""Kullanicilar""(""Id"") ON DELETE SET NULL,
                        ""OlusturanKullaniciId"" INTEGER NULL REFERENCES ""Kullanicilar""(""Id"") ON DELETE SET NULL,
                        ""CariId"" INTEGER NULL REFERENCES ""Cariler""(""Id"") ON DELETE SET NULL,
                        ""MusteriAdi"" VARCHAR(200) NOT NULL DEFAULT '',
                        ""MusteriEmail"" VARCHAR(200) NOT NULL DEFAULT '',
                        ""MusteriTelefon"" VARCHAR(50),
                        ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP NULL,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    );

                    CREATE TABLE IF NOT EXISTS ""DestekDepartmanUyeleri"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""DepartmanId"" INTEGER NOT NULL REFERENCES ""DestekDepartmanlari""(""Id"") ON DELETE CASCADE,
                        ""KullaniciId"" INTEGER NOT NULL REFERENCES ""Kullanicilar""(""Id"") ON DELETE CASCADE,
                        ""Yonetici"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""OtomatikAtamaUygun"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP NULL,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    );

                    CREATE TABLE IF NOT EXISTS ""DestekTalebiIliskileri"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""AnaTalepId"" INTEGER NOT NULL REFERENCES ""DestekTalepleri""(""Id"") ON DELETE CASCADE,
                        ""IliskiliTalepId"" INTEGER NOT NULL REFERENCES ""DestekTalepleri""(""Id"") ON DELETE CASCADE,
                        ""IliskiTuru"" INTEGER NOT NULL,
                        ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP NULL,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    );

                    CREATE TABLE IF NOT EXISTS ""DestekTalebiYanitlari"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""DestekTalebiId"" INTEGER NOT NULL REFERENCES ""DestekTalepleri""(""Id"") ON DELETE CASCADE,
                        ""Icerik"" TEXT NOT NULL,
                        ""DahiliNot"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""YanitTuru"" INTEGER NOT NULL DEFAULT 1,
                        ""KullaniciId"" INTEGER NULL REFERENCES ""Kullanicilar""(""Id"") ON DELETE SET NULL,
                        ""MusteriYaniti"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""MusteriAdi"" VARCHAR(200),
                        ""HazirYanitId"" INTEGER NULL REFERENCES ""DestekHazirYanitlari""(""Id"") ON DELETE SET NULL,
                        ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP NULL,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    );

                    CREATE TABLE IF NOT EXISTS ""DestekTalebiEkleri"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""DestekTalebiId"" INTEGER NULL REFERENCES ""DestekTalepleri""(""Id"") ON DELETE CASCADE,
                        ""YanitId"" INTEGER NULL REFERENCES ""DestekTalebiYanitlari""(""Id"") ON DELETE CASCADE,
                        ""DosyaAdi"" VARCHAR(500) NOT NULL,
                        ""OrijinalDosyaAdi"" VARCHAR(500) NOT NULL,
                        ""DosyaYolu"" TEXT NOT NULL,
                        ""DosyaBoyutu"" BIGINT NOT NULL DEFAULT 0,
                        ""MimeTipi"" VARCHAR(200) NOT NULL DEFAULT '',
                        ""YukleyenKullaniciId"" INTEGER NULL REFERENCES ""Kullanicilar""(""Id"") ON DELETE SET NULL,
                        ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP NULL,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    );

                    CREATE TABLE IF NOT EXISTS ""DestekTalebiAktiviteleri"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""DestekTalebiId"" INTEGER NOT NULL REFERENCES ""DestekTalepleri""(""Id"") ON DELETE CASCADE,
                        ""AktiviteTuru"" INTEGER NOT NULL,
                        ""Aciklama"" TEXT NOT NULL,
                        ""EskiDeger"" TEXT,
                        ""YeniDeger"" TEXT,
                        ""KullaniciId"" INTEGER NULL REFERENCES ""Kullanicilar""(""Id"") ON DELETE SET NULL,
                        ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP NULL,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    );

                    CREATE TABLE IF NOT EXISTS ""DestekBilgiBankasiMakaleleri"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""Baslik"" VARCHAR(500) NOT NULL,
                        ""Icerik"" TEXT NOT NULL,
                        ""Ozet"" TEXT,
                        ""Etiketler"" TEXT,
                        ""SeoBaslik"" VARCHAR(500),
                        ""SeoAciklama"" TEXT,
                        ""Slug"" VARCHAR(500),
                        ""GoruntulemeSayisi"" INTEGER NOT NULL DEFAULT 0,
                        ""YararliBulmaSayisi"" INTEGER NOT NULL DEFAULT 0,
                        ""YararsizBulmaSayisi"" INTEGER NOT NULL DEFAULT 0,
                        ""Durum"" INTEGER NOT NULL DEFAULT 1,
                        ""YayinlanmaTarihi"" TIMESTAMP NULL,
                        ""KategoriId"" INTEGER NULL REFERENCES ""DestekKategorileri""(""Id"") ON DELETE SET NULL,
                        ""YazarKullaniciId"" INTEGER NULL REFERENCES ""Kullanicilar""(""Id"") ON DELETE SET NULL,
                        ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP NULL,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    );

                    CREATE UNIQUE INDEX IF NOT EXISTS ""IX_DestekTalepleri_TalepNo"" ON ""DestekTalepleri"" (""TalepNo"");
                    CREATE UNIQUE INDEX IF NOT EXISTS ""IX_DestekAyarlari_Anahtar"" ON ""DestekAyarlari"" (""Anahtar"");
                ", conn);

                await createCmd.ExecuteNonQueryAsync();
                Console.WriteLine("Destek modulu tablolari PostgreSQL'de kontrol edildi.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Destek modulu tablo kontrol hatasi: {ex.Message}");
        }
    }

    private static async Task EnsureDestekModuluColumnsAsync(ApplicationDbContext context, string dbProvider, IConfiguration? configuration)
    {
        try
        {
            if (dbProvider != "PostgreSQL")
                return;

            var connectionString = GetDefaultConnectionString(context, configuration ?? new ConfigurationBuilder().Build());
            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            // Önce hangi tabloların var olduğunu kontrol et
            var existingTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var checkTablesCmd = new NpgsqlCommand(
                "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'", conn))
            using (var reader = await checkTablesCmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    existingTables.Add(reader.GetString(0));
                }
            }

            // Her tablo için ayrı ayrı kolon ekle (tablo varsa)
            var tableColumnUpdates = new Dictionary<string, string>
            {
                ["DestekDepartmanlari"] = @"
                    ALTER TABLE ""DestekDepartmanlari""
                        ADD COLUMN IF NOT EXISTS ""Email"" VARCHAR(200),
                        ADD COLUMN IF NOT EXISTS ""OtomatikAtama"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ADD COLUMN IF NOT EXISTS ""VarsayilanSlaSuresi"" INTEGER,
                        ADD COLUMN IF NOT EXISTS ""SiraNo"" INTEGER NOT NULL DEFAULT 0,
                        ADD COLUMN IF NOT EXISTS ""Aktif"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ADD COLUMN IF NOT EXISTS ""UstDepartmanId"" INTEGER NULL,
                        ADD COLUMN IF NOT EXISTS ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS ""UpdatedAt"" TIMESTAMP NULL,
                        ADD COLUMN IF NOT EXISTS ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE",

                ["DestekKategorileri"] = @"
                    ALTER TABLE ""DestekKategorileri""
                        ADD COLUMN IF NOT EXISTS ""Aciklama"" TEXT,
                        ADD COLUMN IF NOT EXISTS ""Renk"" VARCHAR(50),
                        ADD COLUMN IF NOT EXISTS ""Simge"" VARCHAR(100),
                        ADD COLUMN IF NOT EXISTS ""SiraNo"" INTEGER NOT NULL DEFAULT 0,
                        ADD COLUMN IF NOT EXISTS ""Aktif"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ADD COLUMN IF NOT EXISTS ""DepartmanId"" INTEGER NULL,
                        ADD COLUMN IF NOT EXISTS ""UstKategoriId"" INTEGER NULL,
                        ADD COLUMN IF NOT EXISTS ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS ""UpdatedAt"" TIMESTAMP NULL,
                        ADD COLUMN IF NOT EXISTS ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE",

                ["DestekSlaListesi"] = @"
                    ALTER TABLE ""DestekSlaListesi""
                        ADD COLUMN IF NOT EXISTS ""Aciklama"" TEXT,
                        ADD COLUMN IF NOT EXISTS ""Aktif"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ADD COLUMN IF NOT EXISTS ""SadeceMesaiSaatleri"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ADD COLUMN IF NOT EXISTS ""SadeceHaftaIci"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ADD COLUMN IF NOT EXISTS ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS ""UpdatedAt"" TIMESTAMP NULL,
                        ADD COLUMN IF NOT EXISTS ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE",

                ["DestekHazirYanitlari"] = @"
                    ALTER TABLE ""DestekHazirYanitlari""
                        ADD COLUMN IF NOT EXISTS ""KonuSablonu"" VARCHAR(500),
                        ADD COLUMN IF NOT EXISTS ""Aciklama"" TEXT,
                        ADD COLUMN IF NOT EXISTS ""Aktif"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ADD COLUMN IF NOT EXISTS ""SiraNo"" INTEGER NOT NULL DEFAULT 0,
                        ADD COLUMN IF NOT EXISTS ""DepartmanId"" INTEGER NULL,
                        ADD COLUMN IF NOT EXISTS ""KategoriId"" INTEGER NULL,
                        ADD COLUMN IF NOT EXISTS ""KullanimSayisi"" INTEGER NOT NULL DEFAULT 0,
                        ADD COLUMN IF NOT EXISTS ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS ""UpdatedAt"" TIMESTAMP NULL,
                        ADD COLUMN IF NOT EXISTS ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE",

                ["DestekAyarlari"] = @"
                    ALTER TABLE ""DestekAyarlari""
                        ADD COLUMN IF NOT EXISTS ""Aciklama"" TEXT,
                        ADD COLUMN IF NOT EXISTS ""Grup"" VARCHAR(100),
                        ADD COLUMN IF NOT EXISTS ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS ""UpdatedAt"" TIMESTAMP NULL,
                        ADD COLUMN IF NOT EXISTS ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE",

                ["DestekTalepleri"] = @"
                    ALTER TABLE ""DestekTalepleri""
                        ADD COLUMN IF NOT EXISTS ""SlaSuresi"" INTEGER NULL,
                        ADD COLUMN IF NOT EXISTS ""SlaBitisTarihi"" TIMESTAMP NULL,
                        ADD COLUMN IF NOT EXISTS ""SlaAsildi"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ADD COLUMN IF NOT EXISTS ""SonAktiviteTarihi"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS ""KapatilmaTarihi"" TIMESTAMP NULL,
                        ADD COLUMN IF NOT EXISTS ""CozumSuresiDakika"" INTEGER NULL,
                        ADD COLUMN IF NOT EXISTS ""IlkYanitSuresiDakika"" INTEGER NULL,
                        ADD COLUMN IF NOT EXISTS ""MemnuniyetPuani"" INTEGER NULL,
                        ADD COLUMN IF NOT EXISTS ""MemnuniyetYorumu"" TEXT,
                        ADD COLUMN IF NOT EXISTS ""DahiliNotlar"" TEXT,
                        ADD COLUMN IF NOT EXISTS ""Etiketler"" TEXT,
                        ADD COLUMN IF NOT EXISTS ""KategoriId"" INTEGER NULL,
                        ADD COLUMN IF NOT EXISTS ""AtananKullaniciId"" INTEGER NULL,
                        ADD COLUMN IF NOT EXISTS ""OlusturanKullaniciId"" INTEGER NULL,
                        ADD COLUMN IF NOT EXISTS ""CariId"" INTEGER NULL,
                        ADD COLUMN IF NOT EXISTS ""MusteriAdi"" VARCHAR(200) NOT NULL DEFAULT '',
                        ADD COLUMN IF NOT EXISTS ""MusteriEmail"" VARCHAR(200) NOT NULL DEFAULT '',
                        ADD COLUMN IF NOT EXISTS ""MusteriTelefon"" VARCHAR(50),
                        ADD COLUMN IF NOT EXISTS ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS ""UpdatedAt"" TIMESTAMP NULL,
                        ADD COLUMN IF NOT EXISTS ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE",

                ["DestekDepartmanUyeleri"] = @"
                    ALTER TABLE ""DestekDepartmanUyeleri""
                        ADD COLUMN IF NOT EXISTS ""Yonetici"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ADD COLUMN IF NOT EXISTS ""OtomatikAtamaUygun"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ADD COLUMN IF NOT EXISTS ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS ""UpdatedAt"" TIMESTAMP NULL,
                        ADD COLUMN IF NOT EXISTS ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE",

                ["DestekTalebiIliskileri"] = @"
                    ALTER TABLE ""DestekTalebiIliskileri""
                        ADD COLUMN IF NOT EXISTS ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS ""UpdatedAt"" TIMESTAMP NULL,
                        ADD COLUMN IF NOT EXISTS ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE",

                ["DestekTalebiYanitlari"] = @"
                    ALTER TABLE ""DestekTalebiYanitlari""
                        ADD COLUMN IF NOT EXISTS ""DahiliNot"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ADD COLUMN IF NOT EXISTS ""YanitTuru"" INTEGER NOT NULL DEFAULT 1,
                        ADD COLUMN IF NOT EXISTS ""KullaniciId"" INTEGER NULL,
                        ADD COLUMN IF NOT EXISTS ""MusteriYaniti"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ADD COLUMN IF NOT EXISTS ""MusteriAdi"" VARCHAR(200),
                        ADD COLUMN IF NOT EXISTS ""HazirYanitId"" INTEGER NULL,
                        ADD COLUMN IF NOT EXISTS ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS ""UpdatedAt"" TIMESTAMP NULL,
                        ADD COLUMN IF NOT EXISTS ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE",

                ["DestekTalebiEkleri"] = @"
                    ALTER TABLE ""DestekTalebiEkleri""
                        ADD COLUMN IF NOT EXISTS ""DosyaBoyutu"" BIGINT NOT NULL DEFAULT 0,
                        ADD COLUMN IF NOT EXISTS ""MimeTipi"" VARCHAR(200) NOT NULL DEFAULT '',
                        ADD COLUMN IF NOT EXISTS ""YukleyenKullaniciId"" INTEGER NULL,
                        ADD COLUMN IF NOT EXISTS ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS ""UpdatedAt"" TIMESTAMP NULL,
                        ADD COLUMN IF NOT EXISTS ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE",

                ["DestekTalebiAktiviteleri"] = @"
                    ALTER TABLE ""DestekTalebiAktiviteleri""
                        ADD COLUMN IF NOT EXISTS ""EskiDeger"" TEXT,
                        ADD COLUMN IF NOT EXISTS ""YeniDeger"" TEXT,
                        ADD COLUMN IF NOT EXISTS ""KullaniciId"" INTEGER NULL,
                        ADD COLUMN IF NOT EXISTS ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS ""UpdatedAt"" TIMESTAMP NULL,
                        ADD COLUMN IF NOT EXISTS ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE",

                ["DestekBilgiBankasiMakaleleri"] = @"
                    ALTER TABLE ""DestekBilgiBankasiMakaleleri""
                        ADD COLUMN IF NOT EXISTS ""Ozet"" TEXT,
                        ADD COLUMN IF NOT EXISTS ""Etiketler"" TEXT,
                        ADD COLUMN IF NOT EXISTS ""SeoBaslik"" VARCHAR(500),
                        ADD COLUMN IF NOT EXISTS ""SeoAciklama"" TEXT,
                        ADD COLUMN IF NOT EXISTS ""Slug"" VARCHAR(500),
                        ADD COLUMN IF NOT EXISTS ""GoruntulemeSayisi"" INTEGER NOT NULL DEFAULT 0,
                        ADD COLUMN IF NOT EXISTS ""YararliBulmaSayisi"" INTEGER NOT NULL DEFAULT 0,
                        ADD COLUMN IF NOT EXISTS ""YararsizBulmaSayisi"" INTEGER NOT NULL DEFAULT 0,
                        ADD COLUMN IF NOT EXISTS ""Durum"" INTEGER NOT NULL DEFAULT 1,
                        ADD COLUMN IF NOT EXISTS ""YayinlanmaTarihi"" TIMESTAMP NULL,
                        ADD COLUMN IF NOT EXISTS ""KategoriId"" INTEGER NULL,
                        ADD COLUMN IF NOT EXISTS ""YazarKullaniciId"" INTEGER NULL,
                        ADD COLUMN IF NOT EXISTS ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ADD COLUMN IF NOT EXISTS ""UpdatedAt"" TIMESTAMP NULL,
                        ADD COLUMN IF NOT EXISTS ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE"
            };

            foreach (var (tableName, sql) in tableColumnUpdates)
            {
                if (existingTables.Contains(tableName))
                {
                    try
                    {
                        using var cmd = new NpgsqlCommand(sql, conn);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{tableName} kolon güncelleme hatası: {ex.Message}");
                    }
                }
            }

            // İndeksleri oluştur (tablolar varsa)
            if (existingTables.Contains("DestekTalepleri"))
            {
                try
                {
                    using var indexCmd = new NpgsqlCommand(
                        @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_DestekTalepleri_TalepNo"" ON ""DestekTalepleri"" (""TalepNo"")", conn);
                    await indexCmd.ExecuteNonQueryAsync();
                }
                catch { /* İndeks zaten var */ }
            }

            if (existingTables.Contains("DestekAyarlari"))
            {
                try
                {
                    using var indexCmd = new NpgsqlCommand(
                        @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_DestekAyarlari_Anahtar"" ON ""DestekAyarlari"" (""Anahtar"")", conn);
                    await indexCmd.ExecuteNonQueryAsync();
                }
                catch { /* İndeks zaten var */ }
            }

            Console.WriteLine("Destek modulu kolonlari PostgreSQL'de kontrol edildi.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Destek modulu kolon kontrol hatasi: {ex.Message}");
        }
    }

                    private static async Task SeedDestekTalebiVerileriAsync(ApplicationDbContext context)
    {
        try
        {
            // Varsayılan Departmanlar
            if (!await context.DestekDepartmanlari.AnyAsync())
            {
                var departmanlar = new List<DestekDepartman>
                {
                    new() { Ad = "Teknik Destek", Aciklama = "Teknik sorunlar ve sistem hataları", Email = "teknik@firma.com", SiraNo = 1, Aktif = true, VarsayilanSlaSuresi = 24, CreatedAt = DateTime.UtcNow },
                    new() { Ad = "Satış", Aciklama = "Satış ve fiyatlandırma soruları", Email = "satis@firma.com", SiraNo = 2, Aktif = true, VarsayilanSlaSuresi = 8, CreatedAt = DateTime.UtcNow },
                    new() { Ad = "Muhasebe", Aciklama = "Fatura ve ödeme sorunları", Email = "muhasebe@firma.com", SiraNo = 3, Aktif = true, VarsayilanSlaSuresi = 48, CreatedAt = DateTime.UtcNow },
                    new() { Ad = "Genel", Aciklama = "Genel sorular ve öneriler", Email = "destek@firma.com", SiraNo = 4, Aktif = true, VarsayilanSlaSuresi = 72, CreatedAt = DateTime.UtcNow }
                };
                context.DestekDepartmanlari.AddRange(departmanlar);
                await context.SaveChangesAsync();
                Console.WriteLine("Destek departmanları eklendi.");
            }

            // Varsayılan Kategoriler
            if (!await context.DestekKategorileri.AnyAsync())
            {
                var teknikDept = await context.DestekDepartmanlari.FirstOrDefaultAsync(d => d.Ad == "Teknik Destek");
                var satisDept = await context.DestekDepartmanlari.FirstOrDefaultAsync(d => d.Ad == "Satış");
                var muhasebeDept = await context.DestekDepartmanlari.FirstOrDefaultAsync(d => d.Ad == "Muhasebe");

                var kategoriler = new List<DestekKategori>
                {
                    // Teknik Destek kategorileri
                    new() { Ad = "Sistem Hatası", DepartmanId = teknikDept?.Id, Renk = "#dc3545", Simge = "bi-bug", SiraNo = 1, Aktif = true, CreatedAt = DateTime.UtcNow },
                    new() { Ad = "Performans Sorunu", DepartmanId = teknikDept?.Id, Renk = "#ffc107", Simge = "bi-speedometer", SiraNo = 2, Aktif = true, CreatedAt = DateTime.UtcNow },
                    new() { Ad = "Özellik İsteği", DepartmanId = teknikDept?.Id, Renk = "#17a2b8", Simge = "bi-lightbulb", SiraNo = 3, Aktif = true, CreatedAt = DateTime.UtcNow },
                    new() { Ad = "Kullanım Yardımı", DepartmanId = teknikDept?.Id, Renk = "#28a745", Simge = "bi-question-circle", SiraNo = 4, Aktif = true, CreatedAt = DateTime.UtcNow },

                    // Satış kategorileri
                    new() { Ad = "Fiyat Teklifi", DepartmanId = satisDept?.Id, Renk = "#007bff", Simge = "bi-currency-dollar", SiraNo = 1, Aktif = true, CreatedAt = DateTime.UtcNow },
                    new() { Ad = "Lisans/Abonelik", DepartmanId = satisDept?.Id, Renk = "#6f42c1", Simge = "bi-key", SiraNo = 2, Aktif = true, CreatedAt = DateTime.UtcNow },

                    // Muhasebe kategorileri
                    new() { Ad = "Fatura Sorunu", DepartmanId = muhasebeDept?.Id, Renk = "#fd7e14", Simge = "bi-receipt", SiraNo = 1, Aktif = true, CreatedAt = DateTime.UtcNow },
                    new() { Ad = "Ödeme Sorunu", DepartmanId = muhasebeDept?.Id, Renk = "#e83e8c", Simge = "bi-credit-card", SiraNo = 2, Aktif = true, CreatedAt = DateTime.UtcNow }
                };
                context.DestekKategorileri.AddRange(kategoriler);
                await context.SaveChangesAsync();
                Console.WriteLine("Destek kategorileri eklendi.");
            }

            // Varsayılan SLA Tanımları
            if (!await context.DestekSlaListesi.AnyAsync())
            {
                var slaListesi = new List<DestekSla>
                {
                    new() { Ad = "Kritik SLA", Oncelik = DestekOncelik.Kritik, IlkYanitSuresi = 1, CozumSuresi = 4, Aktif = true, SadeceMesaiSaatleri = false, SadeceHaftaIci = false, CreatedAt = DateTime.UtcNow },
                    new() { Ad = "Acil SLA", Oncelik = DestekOncelik.Acil, IlkYanitSuresi = 2, CozumSuresi = 8, Aktif = true, SadeceMesaiSaatleri = false, SadeceHaftaIci = false, CreatedAt = DateTime.UtcNow },
                    new() { Ad = "Yüksek SLA", Oncelik = DestekOncelik.Yuksek, IlkYanitSuresi = 4, CozumSuresi = 24, Aktif = true, SadeceMesaiSaatleri = true, SadeceHaftaIci = true, CreatedAt = DateTime.UtcNow },
                    new() { Ad = "Normal SLA", Oncelik = DestekOncelik.Normal, IlkYanitSuresi = 8, CozumSuresi = 48, Aktif = true, SadeceMesaiSaatleri = true, SadeceHaftaIci = true, CreatedAt = DateTime.UtcNow },
                    new() { Ad = "Düşük SLA", Oncelik = DestekOncelik.Dusuk, IlkYanitSuresi = 24, CozumSuresi = 72, Aktif = true, SadeceMesaiSaatleri = true, SadeceHaftaIci = true, CreatedAt = DateTime.UtcNow }
                };
                context.DestekSlaListesi.AddRange(slaListesi);
                await context.SaveChangesAsync();
                Console.WriteLine("SLA tanımları eklendi.");
            }

            // Varsayılan Hazır Yanıtlar
            if (!await context.DestekHazirYanitlari.AnyAsync())
            {
                var hazirYanitlar = new List<DestekHazirYanit>
                {
                    new() { Ad = "Hoş Geldiniz", KonuSablonu = "Talebiniz Alındı", Icerik = "<p>Merhaba,</p><p>Destek talebiniz başarıyla alınmıştır. En kısa sürede size geri dönüş yapacağız.</p><p>Saygılarımızla,<br/>Destek Ekibi</p>", SiraNo = 1, Aktif = true, CreatedAt = DateTime.UtcNow },
                    new() { Ad = "Ek Bilgi İsteği", Icerik = "<p>Merhaba,</p><p>Talebinizi daha iyi değerlendirebilmemiz için aşağıdaki bilgileri paylaşmanızı rica ederiz:</p><ul><li>Sorunun detaylı açıklaması</li><li>Hangi adımlarda sorun yaşıyorsunuz?</li><li>Varsa ekran görüntüsü</li></ul><p>Teşekkürler.</p>", SiraNo = 2, Aktif = true, CreatedAt = DateTime.UtcNow },
                    new() { Ad = "Çözüm Bildirimi", KonuSablonu = "Talebiniz Çözüldü", Icerik = "<p>Merhaba,</p><p>Talebiniz incelenmiş ve gerekli işlemler yapılmıştır. Sorununuzun çözüldüğünü düşünüyoruz.</p><p>Başka bir sorunuz olursa bizimle iletişime geçmekten çekinmeyin.</p><p>İyi çalışmalar dileriz.</p>", SiraNo = 3, Aktif = true, CreatedAt = DateTime.UtcNow },
                    new() { Ad = "Teşekkür", Icerik = "<p>Merhaba,</p><p>Geri bildiriminiz için teşekkür ederiz. Önerileriniz bizim için değerlidir.</p><p>Saygılarımızla.</p>", SiraNo = 4, Aktif = true, CreatedAt = DateTime.UtcNow }
                };
                context.DestekHazirYanitlari.AddRange(hazirYanitlar);
                await context.SaveChangesAsync();
                Console.WriteLine("Hazır yanıtlar eklendi.");
            }

            // Varsayılan Sistem Ayarları
            if (!await context.DestekAyarlari.AnyAsync())
            {
                var ayarlar = new List<DestekAyar>
                {
                    new() { Anahtar = "SirketAdi", Deger = "Koa Filo Servis", Grup = "Genel", Aciklama = "Şirket adı", CreatedAt = DateTime.UtcNow },
                    new() { Anahtar = "DestekEmail", Deger = "destek@koafiloservis.com", Grup = "Genel", Aciklama = "Destek e-posta adresi", CreatedAt = DateTime.UtcNow },
                    new() { Anahtar = "OtomatikAtama", Deger = "false", Grup = "Atama", Aciklama = "Yeni talepleri otomatik ata", CreatedAt = DateTime.UtcNow },
                    new() { Anahtar = "MusteriPortaliAktif", Deger = "false", Grup = "Portal", Aciklama = "Müşteri self-servis portalı aktif mi", CreatedAt = DateTime.UtcNow },
                    new() { Anahtar = "MemnuniyetAnketiAktif", Deger = "true", Grup = "Anket", Aciklama = "Talep kapatıldığında memnuniyet anketi gönder", CreatedAt = DateTime.UtcNow },
                    new() { Anahtar = "MaksimumDosyaBoyutuMB", Deger = "10", Grup = "Dosya", Aciklama = "Maksimum dosya yükleme boyutu (MB)", CreatedAt = DateTime.UtcNow },
                    new() { Anahtar = "IzinliDosyaTipleri", Deger = ".jpg,.jpeg,.png,.gif,.pdf,.doc,.docx,.xls,.xlsx,.txt,.zip", Grup = "Dosya", Aciklama = "İzin verilen dosya uzantıları", CreatedAt = DateTime.UtcNow }
                };
                context.DestekAyarlari.AddRange(ayarlar);
                await context.SaveChangesAsync();
                Console.WriteLine("Destek ayarları eklendi.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Destek talebi seed hatası: {ex.Message}");
        }
    }

    private static async Task SeedBudgetMasrafKalemleriAsync(ApplicationDbContext context)
    {
        try
        {
            // Kritik masraf kalemleri - Her zaman kontrol et
            var gerekliKalemler = new[] { "Yakıt", "Araç Bakım/Onarım", "Şoför Maaşları", "Sigorta" };

            foreach (var kalemAdi in gerekliKalemler)
            {
                if (!await context.BudgetMasrafKalemleri.AnyAsync(k => k.KalemAdi == kalemAdi))
                {
                    context.BudgetMasrafKalemleri.Add(new BudgetMasrafKalemi
                    {
                        KalemAdi = kalemAdi,
                        Aktif = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Seed hatasi: {ex.Message}");
        }
    }

    private static async Task SeedAracMasrafKalemleriAsync(ApplicationDbContext context)
    {
        try
        {
            var mevcutKalem = await context.MasrafKalemleri
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(k => k.MasrafAdi == "AdBlue" || k.MasrafKodu == "MSR-ADBLUE");

            if (mevcutKalem == null)
            {
                context.MasrafKalemleri.Add(new MasrafKalemi
                {
                    MasrafKodu = "MSR-ADBLUE",
                    MasrafAdi = "AdBlue",
                    Kategori = MasrafKategori.Yakit,
                    Aktif = true,
                    CreatedAt = DateTime.UtcNow
                });

                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Araç masraf kalemi seed hatası: {ex.Message}");
        }
    }

    /// <summary>
    /// EBYS modülü için örnek veriler ve test senaryoları
    /// Evrak kategorileri, gelen/giden evraklar, özlük evrak tanımları
    /// </summary>
    private static async Task SeedEbysOrnekVerileriAsync(ApplicationDbContext context)
    {
        try
        {
            if (!await ContextTableExistsAsync(context, "EbysEvrakKategoriler") || !await ContextTableExistsAsync(context, "OzlukEvrakTanimlari"))
            {
                Console.WriteLine("EBYS seed atlandi: gerekli tablolar henuz olusmamis.");
                return;
            }

            // ========== 1. EBYS Evrak Kategorileri ==========
            if (!await context.EbysEvrakKategoriler.AnyAsync())
            {
                var kategoriler = new List<EbysEvrakKategori>
                {
                    new() { KategoriAdi = "Resmi Yazışmalar", Aciklama = "Kurumlar arası resmi yazışmalar", SiraNo = 1, Aktif = true, Renk = "#0d6efd", Ikon = "bi-envelope-paper", CreatedAt = DateTime.UtcNow },
                    new() { KategoriAdi = "Sözleşmeler", Aciklama = "İş sözleşmeleri, hizmet sözleşmeleri", SiraNo = 2, Aktif = true, Renk = "#198754", Ikon = "bi-file-earmark-text", CreatedAt = DateTime.UtcNow },
                    new() { KategoriAdi = "Faturalar / Mali Belgeler", Aciklama = "Faturalar, dekontlar, mali belgeler", SiraNo = 3, Aktif = true, Renk = "#ffc107", Ikon = "bi-receipt", CreatedAt = DateTime.UtcNow },
                    new() { KategoriAdi = "Raporlar", Aciklama = "Faaliyet raporları, denetim raporları", SiraNo = 4, Aktif = true, Renk = "#6f42c1", Ikon = "bi-file-earmark-bar-graph", CreatedAt = DateTime.UtcNow },
                    new() { KategoriAdi = "Dilekçeler / Başvurular", Aciklama = "Dilekçeler ve başvuru evrakları", SiraNo = 5, Aktif = true, Renk = "#17a2b8", Ikon = "bi-file-earmark-person", CreatedAt = DateTime.UtcNow },
                    new() { KategoriAdi = "İhale Belgeleri", Aciklama = "İhale dosyaları, şartnameler, teklifler", SiraNo = 6, Aktif = true, Renk = "#dc3545", Ikon = "bi-briefcase", CreatedAt = DateTime.UtcNow },
                    new() { KategoriAdi = "SGK / Vergi Belgeleri", Aciklama = "SGK bildirgeleri, vergi beyannameleri", SiraNo = 7, Aktif = true, Renk = "#fd7e14", Ikon = "bi-building", CreatedAt = DateTime.UtcNow },
                    new() { KategoriAdi = "Araç Belgeleri", Aciklama = "Ruhsat, sigorta, muayene belgeleri", SiraNo = 8, Aktif = true, Renk = "#20c997", Ikon = "bi-truck", CreatedAt = DateTime.UtcNow },
                    new() { KategoriAdi = "Personel Belgeleri", Aciklama = "Personel özlük dosyası belgeleri", SiraNo = 9, Aktif = true, Renk = "#6610f2", Ikon = "bi-person-badge", CreatedAt = DateTime.UtcNow },
                    new() { KategoriAdi = "Diğer", Aciklama = "Sınıflandırılmamış belgeler", SiraNo = 99, Aktif = true, Renk = "#6c757d", Ikon = "bi-folder", CreatedAt = DateTime.UtcNow }
                };
                context.EbysEvrakKategoriler.AddRange(kategoriler);
                await context.SaveChangesAsync();
                Console.WriteLine("EBYS evrak kategorileri eklendi.");
            }

            // ========== 2. Özlük Evrak Tanımları ==========
            if (!await context.OzlukEvrakTanimlari.AnyAsync())
            {
                var evrakTanimlari = new List<OzlukEvrakTanim>
                {
                    // Kimlik Belgeleri
                    new() { EvrakAdi = "Nüfus Cüzdanı Fotokopisi", Aciklama = "Kimlik kartı veya nüfus cüzdanı fotokopisi", Kategori = OzlukEvrakKategori.KimlikBelgeleri, Zorunlu = true, SiraNo = 1, Aktif = true, CreatedAt = DateTime.UtcNow },
                    new() { EvrakAdi = "İkametgah Belgesi", Aciklama = "Yerleşim yeri belgesi (e-Devlet)", Kategori = OzlukEvrakKategori.KimlikBelgeleri, Zorunlu = true, SiraNo = 2, Aktif = true, CreatedAt = DateTime.UtcNow },
                    new() { EvrakAdi = "Vesikalık Fotoğraf", Aciklama = "2 adet vesikalık fotoğraf", Kategori = OzlukEvrakKategori.KimlikBelgeleri, Zorunlu = true, SiraNo = 3, Aktif = true, CreatedAt = DateTime.UtcNow },
                    new() { EvrakAdi = "Sabıka Kaydı", Aciklama = "Adli sicil kaydı (e-Devlet)", Kategori = OzlukEvrakKategori.KimlikBelgeleri, Zorunlu = true, SiraNo = 4, Aktif = true, CreatedAt = DateTime.UtcNow },

                    // Eğitim Belgeleri
                    new() { EvrakAdi = "Diploma Fotokopisi", Aciklama = "Son mezuniyet belgesi", Kategori = OzlukEvrakKategori.EgitimBelgeleri, Zorunlu = false, SiraNo = 1, Aktif = true, CreatedAt = DateTime.UtcNow },
                    new() { EvrakAdi = "Sertifika / Kurs Belgeleri", Aciklama = "Mesleki sertifikalar ve kurs belgeleri", Kategori = OzlukEvrakKategori.EgitimBelgeleri, Zorunlu = false, SiraNo = 2, Aktif = true, CreatedAt = DateTime.UtcNow },

                    // Sağlık Belgeleri
                    new() { EvrakAdi = "Sağlık Raporu", Aciklama = "İşe giriş sağlık raporu", Kategori = OzlukEvrakKategori.SaglikBelgeleri, Zorunlu = true, SiraNo = 1, Aktif = true, CreatedAt = DateTime.UtcNow },
                    new() { EvrakAdi = "Kan Grubu Belgesi", Aciklama = "Kan grubu kartı veya belgesi", Kategori = OzlukEvrakKategori.SaglikBelgeleri, Zorunlu = false, SiraNo = 2, Aktif = true, CreatedAt = DateTime.UtcNow },

                    // Şoför Belgeleri
                    new() { EvrakAdi = "Ehliyet Fotokopisi", Aciklama = "Sürücü belgesi fotokopisi", Kategori = OzlukEvrakKategori.SoforBelgeleri, Zorunlu = true, SiraNo = 1, Aktif = true, GecerliGorevler = "1", CreatedAt = DateTime.UtcNow },
                    new() { EvrakAdi = "SRC Belgesi", Aciklama = "Mesleki yeterlilik belgesi", Kategori = OzlukEvrakKategori.SoforBelgeleri, Zorunlu = true, SiraNo = 2, Aktif = true, GecerliGorevler = "1", CreatedAt = DateTime.UtcNow },
                    new() { EvrakAdi = "Psikoteknik Belgesi", Aciklama = "Psikoteknik değerlendirme raporu", Kategori = OzlukEvrakKategori.SoforBelgeleri, Zorunlu = true, SiraNo = 3, Aktif = true, GecerliGorevler = "1", CreatedAt = DateTime.UtcNow },

                    // SGK Belgeleri
                    new() { EvrakAdi = "SGK İşe Giriş Bildirgesi", Aciklama = "SGK'ya işe giriş bildirgesi", Kategori = OzlukEvrakKategori.SGKBelgeleri, Zorunlu = true, SiraNo = 1, Aktif = true, CreatedAt = DateTime.UtcNow },
                    new() { EvrakAdi = "SGK Hizmet Dökümü", Aciklama = "SGK hizmet dökümü (e-Devlet)", Kategori = OzlukEvrakKategori.SGKBelgeleri, Zorunlu = false, SiraNo = 2, Aktif = true, CreatedAt = DateTime.UtcNow },

                    // İşe Giriş Belgeleri
                    new() { EvrakAdi = "İş Başvuru Formu", Aciklama = "Doldurulmuş iş başvuru formu", Kategori = OzlukEvrakKategori.IseGirisBelgeleri, Zorunlu = true, SiraNo = 1, Aktif = true, CreatedAt = DateTime.UtcNow },
                    new() { EvrakAdi = "İş Sözleşmesi", Aciklama = "İmzalı iş sözleşmesi", Kategori = OzlukEvrakKategori.IseGirisBelgeleri, Zorunlu = true, SiraNo = 2, Aktif = true, CreatedAt = DateTime.UtcNow },
                    new() { EvrakAdi = "İşe Giriş Bildirgesi", Aciklama = "İşe giriş bildirgesi fotokopisi", Kategori = OzlukEvrakKategori.IseGirisBelgeleri, Zorunlu = true, SiraNo = 3, Aktif = true, CreatedAt = DateTime.UtcNow },
                    new() { EvrakAdi = "IBAN Bilgi Formu", Aciklama = "Maaş ödemesi için banka hesap bilgileri", Kategori = OzlukEvrakKategori.IseGirisBelgeleri, Zorunlu = true, SiraNo = 4, Aktif = true, CreatedAt = DateTime.UtcNow },
                    new() { EvrakAdi = "Acil Durum İletişim Formu", Aciklama = "Acil durum iletişim bilgileri formu", Kategori = OzlukEvrakKategori.IseGirisBelgeleri, Zorunlu = false, SiraNo = 5, Aktif = true, CreatedAt = DateTime.UtcNow }
                };
                context.OzlukEvrakTanimlari.AddRange(evrakTanimlari);
                await context.SaveChangesAsync();
                Console.WriteLine("Özlük evrak tanımları eklendi.");
            }

            // ========== 3. Örnek EBYS Gelen Evraklar ==========
            if (!await context.EbysEvraklar.AnyAsync())
            {
                // Kategori ID'lerini al
                var resmiYazismaKat = await context.EbysEvrakKategoriler.FirstOrDefaultAsync(k => k.KategoriAdi == "Resmi Yazışmalar");
                var sozlesmeKat = await context.EbysEvrakKategoriler.FirstOrDefaultAsync(k => k.KategoriAdi == "Sözleşmeler");
                var faturaKat = await context.EbysEvrakKategoriler.FirstOrDefaultAsync(k => k.KategoriAdi == "Faturalar / Mali Belgeler");
                var sgkKat = await context.EbysEvrakKategoriler.FirstOrDefaultAsync(k => k.KategoriAdi == "SGK / Vergi Belgeleri");
                var ihaleKat = await context.EbysEvrakKategoriler.FirstOrDefaultAsync(k => k.KategoriAdi == "İhale Belgeleri");

                var ornekEvraklar = new List<EbysEvrak>
                {
                    // Gelen Evraklar
                    new()
                    {
                        EvrakNo = "GE-2025-00001",
                        Yon = EvrakYonu.Gelen,
                        EvrakTarihi = DateTime.Today.AddDays(-30),
                        KayitTarihi = DateTime.Today.AddDays(-29),
                        Konu = "2025 Yılı Personel Servis Hizmeti İhale Daveti",
                        Ozet = "Belediye tarafından gönderilen personel servis hizmeti ihale daveti",
                        GonderenKurum = "Belediye Başkanlığı",
                        GelisNo = "BEL-2025-1234",
                        GelisTarihi = DateTime.Today.AddDays(-30),
                        KategoriId = ihaleKat?.Id,
                        Oncelik = EvrakOncelik.Yuksek,
                        Gizlilik = EvrakGizlilik.Normal,
                        Durum = EbysEvrakDurum.Isleniyor,
                        CevapGerekli = true,
                        CevapSuresi = DateTime.Today.AddDays(15),
                        Aciklama = "İhale şartnamesine göre teklif hazırlanacak",
                        CreatedAt = DateTime.UtcNow
                    },
                    new()
                    {
                        EvrakNo = "GE-2025-00002",
                        Yon = EvrakYonu.Gelen,
                        EvrakTarihi = DateTime.Today.AddDays(-25),
                        KayitTarihi = DateTime.Today.AddDays(-24),
                        Konu = "SGK Denetim Bildirimi",
                        Ozet = "SGK müfettişi tarafından yapılacak denetim hakkında bildirim",
                        GonderenKurum = "Sosyal Güvenlik Kurumu",
                        GelisNo = "SGK-2025-56789",
                        GelisTarihi = DateTime.Today.AddDays(-25),
                        KategoriId = sgkKat?.Id,
                        Oncelik = EvrakOncelik.Acil,
                        Gizlilik = EvrakGizlilik.Gizli,
                        Durum = EbysEvrakDurum.Tamamlandi,
                        CevapGerekli = false,
                        Aciklama = "Denetim 15 gün sonra yapılacak, belgeler hazırlandı",
                        CreatedAt = DateTime.UtcNow
                    },
                    new()
                    {
                        EvrakNo = "GE-2025-00003",
                        Yon = EvrakYonu.Gelen,
                        EvrakTarihi = DateTime.Today.AddDays(-20),
                        KayitTarihi = DateTime.Today.AddDays(-19),
                        Konu = "Sözleşme Yenileme Talebi",
                        Ozet = "Mevcut servis sözleşmesinin yenilenmesi talebi",
                        GonderenKurum = "ABC Sanayi A.Ş.",
                        GelisNo = "ABC-2025-0123",
                        GelisTarihi = DateTime.Today.AddDays(-20),
                        KategoriId = sozlesmeKat?.Id,
                        Oncelik = EvrakOncelik.Normal,
                        Gizlilik = EvrakGizlilik.Normal,
                        Durum = EbysEvrakDurum.CevapBekliyor,
                        CevapGerekli = true,
                        CevapSuresi = DateTime.Today.AddDays(10),
                        Aciklama = "Yeni fiyat teklifi hazırlanıyor",
                        CreatedAt = DateTime.UtcNow
                    },
                    new()
                    {
                        EvrakNo = "GE-2025-00004",
                        Yon = EvrakYonu.Gelen,
                        EvrakTarihi = DateTime.Today.AddDays(-15),
                        KayitTarihi = DateTime.Today.AddDays(-14),
                        Konu = "Araç Muayene Hatırlatması",
                        Ozet = "Araç muayene süresi dolmak üzere hatırlatma",
                        GonderenKurum = "TÜVTÜRK",
                        GelisNo = "TUV-2025-9876",
                        GelisTarihi = DateTime.Today.AddDays(-15),
                        KategoriId = resmiYazismaKat?.Id,
                        Oncelik = EvrakOncelik.Normal,
                        Gizlilik = EvrakGizlilik.Normal,
                        Durum = EbysEvrakDurum.Beklemede,
                        CevapGerekli = false,
                        Aciklama = "5 aracın muayenesi yapılacak",
                        CreatedAt = DateTime.UtcNow
                    },

                    // Giden Evraklar
                    new()
                    {
                        EvrakNo = "GI-2025-00001",
                        Yon = EvrakYonu.Giden,
                        EvrakTarihi = DateTime.Today.AddDays(-28),
                        KayitTarihi = DateTime.Today.AddDays(-28),
                        Konu = "2025 Yılı Personel Servis Hizmeti Teklif",
                        Ozet = "Belediye ihalesine verilen fiyat teklifi",
                        AliciKurum = "Belediye Başkanlığı",
                        GidisNo = "TEK-2025-0001",
                        GonderimTarihi = DateTime.Today.AddDays(-28),
                        GonderimYontemi = GonderimYontemi.KEP,
                        KategoriId = ihaleKat?.Id,
                        Oncelik = EvrakOncelik.Yuksek,
                        Gizlilik = EvrakGizlilik.Normal,
                        Durum = EbysEvrakDurum.Tamamlandi,
                        Aciklama = "Teklif KEP ile gönderildi",
                        CreatedAt = DateTime.UtcNow
                    },
                    new()
                    {
                        EvrakNo = "GI-2025-00002",
                        Yon = EvrakYonu.Giden,
                        EvrakTarihi = DateTime.Today.AddDays(-10),
                        KayitTarihi = DateTime.Today.AddDays(-10),
                        Konu = "Aylık Fatura Gönderimi - Ocak 2025",
                        Ozet = "Ocak 2025 dönemi servis hizmeti faturası",
                        AliciKurum = "XYZ Holding A.Ş.",
                        GidisNo = "FAT-2025-0015",
                        GonderimTarihi = DateTime.Today.AddDays(-10),
                        GonderimYontemi = GonderimYontemi.Email,
                        KategoriId = faturaKat?.Id,
                        Oncelik = EvrakOncelik.Normal,
                        Gizlilik = EvrakGizlilik.Normal,
                        Durum = EbysEvrakDurum.Tamamlandi,
                        Aciklama = "E-fatura olarak gönderildi",
                        CreatedAt = DateTime.UtcNow
                    },
                    new()
                    {
                        EvrakNo = "GI-2025-00003",
                        Yon = EvrakYonu.Giden,
                        EvrakTarihi = DateTime.Today.AddDays(-5),
                        KayitTarihi = DateTime.Today.AddDays(-5),
                        Konu = "Sözleşme Yenileme Cevabı",
                        Ozet = "ABC Sanayi sözleşme yenileme talebine cevap",
                        AliciKurum = "ABC Sanayi A.Ş.",
                        GidisNo = "CVP-2025-0003",
                        GonderimTarihi = DateTime.Today.AddDays(-5),
                        GonderimYontemi = GonderimYontemi.Elden,
                        KategoriId = sozlesmeKat?.Id,
                        Oncelik = EvrakOncelik.Normal,
                        Gizlilik = EvrakGizlilik.Normal,
                        Durum = EbysEvrakDurum.Tamamlandi,
                        Aciklama = "Yeni sözleşme şartları ile teklif iletildi",
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.EbysEvraklar.AddRange(ornekEvraklar);
                await context.SaveChangesAsync();
                Console.WriteLine("EBYS örnek evraklar eklendi.");
            }

            Console.WriteLine("EBYS örnek veri kontrolü tamamlandı.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EBYS örnek veri ekleme hatası: {ex.Message}");
        }
    }

    /// <summary>
    /// PostgreSQL için eksik tabloları ve kolonları otomatik ekler
    /// </summary>
    private static async Task EnsurePostgreSqlMissingColumnsAsync(ApplicationDbContext context, IConfiguration configuration)
    {
        var connectionString = GetDefaultConnectionString(context, configuration);

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // Önce hangi tabloların var olduğunu kontrol et
        var existingTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var checkTablesSql = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'";
        await using (var checkTablesCmd = new NpgsqlCommand(checkTablesSql, connection))
        await using (var reader = await checkTablesCmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                existingTables.Add(reader.GetString(0));
            }
        }

        // ========== ADIM 1: Eksik tabloları oluştur ==========
        await EnsureCriticalTablesAsync(connection, existingTables);

        // ========== ADIM 1.5: Araç şase-plaka yapısını eski şemadan taşı ==========
        await EnsureAracSasePlakaMigrationAsync(connection, existingTables);

        // ========== ADIM 2: Eksik kolonları ekle ==========
        // Eklenecek kolonlar listesi: (Tablo, Kolon, Tip, Default)
        var missingColumns = new List<(string Table, string Column, string Type, string? Default)>
        {
            // Fatura tablosu - EslesenFatura ve Mahsup alanları
            ("Faturalar", "EslesenFaturaId", "INTEGER", null),
            ("Faturalar", "MahsupKapatildi", "BOOLEAN", "FALSE"),
            ("Faturalar", "MahsupTarihi", "TIMESTAMP WITHOUT TIME ZONE", null),

            // Fatura tablosu - FirmalarArasi alanları
            ("Faturalar", "FirmalarArasiFatura", "BOOLEAN", "FALSE"),
            ("Faturalar", "KarsiFirmaId", "INTEGER", null),

            // BudgetOdemeler tablosu - Ödeme bilgileri
            ("BudgetOdemeler", "GercekOdemeTarihi", "TIMESTAMP WITHOUT TIME ZONE", null),
            ("BudgetOdemeler", "OdemeYapildigiHesapId", "INTEGER", null),
            ("BudgetOdemeler", "OdenenTutar", "NUMERIC(18,2)", null),
            ("BudgetOdemeler", "OdemeNotu", "VARCHAR(500)", null),
            ("BudgetOdemeler", "BankaKasaHareketId", "INTEGER", null),

            // BudgetOdemeler tablosu - Kesinti bilgileri
            ("BudgetOdemeler", "MasrafKesintisi", "NUMERIC(18,2)", "0"),
            ("BudgetOdemeler", "CezaKesintisi", "NUMERIC(18,2)", "0"),
            ("BudgetOdemeler", "DigerKesinti", "NUMERIC(18,2)", "0"),
            ("BudgetOdemeler", "KesintiAciklamasi", "VARCHAR(500)", null),

            // BudgetOdemeler tablosu - Fatura eşleştirme
            ("BudgetOdemeler", "FaturaId", "INTEGER", null),
            ("BudgetOdemeler", "FaturaIleKapatildi", "BOOLEAN", "FALSE"),

            // Personeller tablosu - Muhasebe entegrasyonu
            ("Personeller", "MuhasebeHesapId", "INTEGER", null),

            // Personeller tablosu - Sıralama ve diğer alanlar
            ("Personeller", "SiralamaNo", "INTEGER", "0"),
            ("Personeller", "Gorev", "INTEGER", "1"),
            ("Personeller", "Departman", "VARCHAR(100)", null),
            ("Personeller", "Pozisyon", "VARCHAR(100)", null),
            ("Personeller", "BrutMaasHesaplamaTipi", "INTEGER", "0"),
            ("Personeller", "CalismaMiktari", "NUMERIC(18,2)", "0"),
            ("Personeller", "BirimUcret", "NUMERIC(18,2)", "0"),
            ("Personeller", "BrutMaas", "NUMERIC(18,2)", "0"),
            ("Personeller", "ResmiNetMaas", "NUMERIC(18,2)", "0"),
            ("Personeller", "DigerMaas", "NUMERIC(18,2)", "0"),
            ("Personeller", "NetMaas", "NUMERIC(18,2)", "0"),
            ("Personeller", "SGKBordroDahilMi", "BOOLEAN", "FALSE"),
            ("Personeller", "BordroTipiPersonel", "INTEGER", "0"),
            ("Personeller", "ArgePersoneli", "BOOLEAN", "FALSE"),
            ("Personeller", "TopluMaas", "NUMERIC(18,2)", "0"),
            ("Personeller", "SgkMaasi", "NUMERIC(18,2)", "0"),
            ("Personeller", "BankaAdi", "VARCHAR(100)", null),
            ("Personeller", "IBAN", "VARCHAR(50)", null),

            // BankaKasaHareketleri tablosu - Mahsup alanları
            ("BankaKasaHareketleri", "MahsupHareketId", "INTEGER", null),
            ("BankaKasaHareketleri", "MahsupGrupId", "UUID", null),

            // BankaKasaHareketleri tablosu - Muhasebe alanları
            ("BankaKasaHareketleri", "MuhasebeHesapKodu", "TEXT", null),
            ("BankaKasaHareketleri", "MuhasebeAltHesapKodu", "TEXT", null),
            ("BankaKasaHareketleri", "KostMerkeziKodu", "TEXT", null),
            ("BankaKasaHareketleri", "ProjeKodu", "TEXT", null),
            ("BankaKasaHareketleri", "MuhasebeAciklama", "TEXT", null),

             // MuhasebeFisleri tablosu - Bordro bağlantısı
             ("MuhasebeFisleri", "BordroId", "INTEGER", null),

            // ========== Mega-migration (EbysEvrakKategoriler_Fix) eksik kolonları ==========

            // SirketId kolonları - 8 tablo (multi-tenant)
            ("Faturalar", "SirketId", "INTEGER", null),
            ("Cariler", "SirketId", "INTEGER", null),
            ("Araclar", "SirketId", "INTEGER", null),
            ("Guzergahlar", "SirketId", "INTEGER", null),
            ("BankaHesaplari", "SirketId", "INTEGER", null),
            ("BankaKasaHareketleri", "SirketId", "INTEGER", null),
            ("Personeller", "SirketId", "INTEGER", null),
            ("Kullanicilar", "SirketId", "INTEGER", null),

            // PersonelOzlukEvraklar tablosu - Versiyon/dosya alanları
            ("PersonelOzlukEvraklar", "DosyaAdi", "TEXT", null),
            ("PersonelOzlukEvraklar", "DosyaBoyutu", "BIGINT", null),
            ("PersonelOzlukEvraklar", "DosyaTipi", "TEXT", null),
            ("PersonelOzlukEvraklar", "SonDegisiklikNotu", "TEXT", null),
            ("PersonelOzlukEvraklar", "VersiyonNo", "INTEGER", "0"),

            // EbysEvrakDosyalar tablosu - Versiyon alanları
            ("EbysEvrakDosyalar", "SonDegisiklikNotu", "TEXT", null),
            ("EbysEvrakDosyalar", "VersiyonNo", "INTEGER", "0"),

            // AracEvrakDosyalari tablosu - Versiyon alanları
            ("AracEvrakDosyalari", "SonDegisiklikNotu", "TEXT", null),
            ("AracEvrakDosyalari", "VersiyonNo", "INTEGER", "0"),

            // PersonelPuantajlar tablosu - Onay alanları
            ("PersonelPuantajlar", "OnayDurumu", "INTEGER", "0"),
            ("PersonelPuantajlar", "OnayNotu", "TEXT", null),
            ("PersonelPuantajlar", "OnayTarihi", "TIMESTAMP WITHOUT TIME ZONE", null),
            ("PersonelPuantajlar", "OnaylayanKullanici", "TEXT", null),
        };

        foreach (var (table, column, type, defaultValue) in missingColumns)
        {
            try
            {
                // Tablo var mı kontrol et - yoksa atla
                if (!existingTables.Contains(table))
                {
                    continue;
                }

                // Kolon var mı kontrol et
                var checkSql = $@"
                    SELECT COUNT(*) FROM information_schema.columns 
                    WHERE table_name = '{table}' AND column_name = '{column}'";

                await using var checkCmd = new NpgsqlCommand(checkSql, connection);
                var exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0;

                if (!exists)
                {
                    // Kolonu ekle
                    var alterSql = defaultValue != null
                        ? $@"ALTER TABLE ""{table}"" ADD COLUMN ""{column}"" {type} NOT NULL DEFAULT {defaultValue}"
                        : $@"ALTER TABLE ""{table}"" ADD COLUMN ""{column}"" {type} NULL";

                    await using var alterCmd = new NpgsqlCommand(alterSql, connection);
                    await alterCmd.ExecuteNonQueryAsync();
                    Console.WriteLine($"{table}.{column} kolonu eklendi.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{table}.{column} eklenirken hata: {ex.Message}");
            }
        }

        // MuhasebeFisleri.BordroId için indeks ve foreign key kontrolü
        if (existingTables.Contains("MuhasebeFisleri") && existingTables.Contains("Bordrolar"))
        {
            try
            {
                await using var indexCmd = new NpgsqlCommand(
                    @"CREATE INDEX IF NOT EXISTS ""IX_MuhasebeFisleri_BordroId"" ON ""MuhasebeFisleri"" (""BordroId"")",
                    connection);
                await indexCmd.ExecuteNonQueryAsync();

                const string fkName = "FK_MuhasebeFisleri_Bordrolar_BordroId";
                await using var fkCheckCmd = new NpgsqlCommand(
                    @"SELECT COUNT(*) FROM pg_constraint WHERE conname = @constraintName",
                    connection);
                fkCheckCmd.Parameters.AddWithValue("constraintName", fkName);

                var fkExists = Convert.ToInt32(await fkCheckCmd.ExecuteScalarAsync()) > 0;
                if (!fkExists)
                {
                    await using var fkCmd = new NpgsqlCommand(
                        @"ALTER TABLE ""MuhasebeFisleri""
                          ADD CONSTRAINT ""FK_MuhasebeFisleri_Bordrolar_BordroId""
                          FOREIGN KEY (""BordroId"") REFERENCES ""Bordrolar"" (""Id"") ON DELETE SET NULL",
                        connection);
                    await fkCmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MuhasebeFisleri.BordroId ilişki kontrolü hatası: {ex.Message}");
            }
        }

        await connection.CloseAsync();
    }

    private static async Task EnsureAracSasePlakaMigrationAsync(NpgsqlConnection connection, HashSet<string> existingTables)
    {
        if (!existingTables.Contains("Araclar"))
        {
            return;
        }

        var legacyPlakaExists = await ColumnExistsAsync(connection, "Araclar", "Plaka");
        var aktifPlakaExists = await ColumnExistsAsync(connection, "Araclar", "AktifPlaka");
        var aracPlakalarExists = existingTables.Contains("AracPlakalar") || await TableExistsAsync(connection, "AracPlakalar");

        if (!legacyPlakaExists && aktifPlakaExists && aracPlakalarExists)
        {
            await EnsureMigrationHistoryEntryAsync(connection, AracSasePlakaMigrationId);
            return;
        }

        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            await ExecuteNonQueryAsync(connection, transaction, @"
                CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
                    ""MigrationId"" character varying(150) NOT NULL,
                    ""ProductVersion"" character varying(32) NOT NULL,
                    CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY (""MigrationId"")
                );");

            await ExecuteNonQueryAsync(connection, transaction, @"
                ALTER TABLE ""Araclar"" ADD COLUMN IF NOT EXISTS ""AktifPlaka"" character varying(15);
                ALTER TABLE ""Araclar"" ADD COLUMN IF NOT EXISTS ""SatisaAcik"" boolean NOT NULL DEFAULT false;
                ALTER TABLE ""Araclar"" ADD COLUMN IF NOT EXISTS ""SatisFiyati"" numeric(18,2);
                ALTER TABLE ""Araclar"" ADD COLUMN IF NOT EXISTS ""SatisaAcilmaTarihi"" timestamp without time zone;
                ALTER TABLE ""Araclar"" ADD COLUMN IF NOT EXISTS ""SatisAciklamasi"" text;");

            if (legacyPlakaExists)
            {
                await ExecuteNonQueryAsync(connection, transaction, @"
                    UPDATE ""Araclar""
                    SET ""SaseNo"" = ""Plaka""
                    WHERE (""SaseNo"" IS NULL OR BTRIM(""SaseNo"") = '')
                      AND ""Plaka"" IS NOT NULL
                      AND BTRIM(""Plaka"") <> '';

                    UPDATE ""Araclar""
                    SET ""AktifPlaka"" = ""Plaka""
                    WHERE (""AktifPlaka"" IS NULL OR BTRIM(""AktifPlaka"") = '')
                      AND ""Plaka"" IS NOT NULL
                      AND BTRIM(""Plaka"") <> '';");
            }

            await ExecuteNonQueryAsync(connection, transaction, @"
                CREATE TABLE IF NOT EXISTS ""AracPlakalar"" (
                    ""Id"" integer GENERATED BY DEFAULT AS IDENTITY,
                    ""AracId"" integer NOT NULL,
                    ""Plaka"" character varying(15) NOT NULL,
                    ""GirisTarihi"" timestamp without time zone NOT NULL,
                    ""CikisTarihi"" timestamp without time zone,
                    ""IslemTipi"" integer NOT NULL,
                    ""Aciklama"" character varying(500),
                    ""IslemTutari"" numeric(18,2),
                    ""CariId"" integer,
                    ""CreatedAt"" timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    ""UpdatedAt"" timestamp without time zone,
                    ""IsDeleted"" boolean NOT NULL DEFAULT false,
                    CONSTRAINT ""PK_AracPlakalar"" PRIMARY KEY (""Id""),
                    CONSTRAINT ""FK_AracPlakalar_Araclar_AracId"" FOREIGN KEY (""AracId"") REFERENCES ""Araclar"" (""Id"") ON DELETE CASCADE,
                    CONSTRAINT ""FK_AracPlakalar_Cariler_CariId"" FOREIGN KEY (""CariId"") REFERENCES ""Cariler"" (""Id"") ON DELETE SET NULL
                );

                CREATE INDEX IF NOT EXISTS ""IX_AracPlakalar_AracId"" ON ""AracPlakalar"" (""AracId"");
                CREATE INDEX IF NOT EXISTS ""IX_AracPlakalar_CariId"" ON ""AracPlakalar"" (""CariId"");
                CREATE INDEX IF NOT EXISTS ""IX_AracPlakalar_Plaka_CikisTarihi""
                    ON ""AracPlakalar"" (""Plaka"", ""CikisTarihi"")
                    WHERE ""CikisTarihi"" IS NULL AND ""IsDeleted"" = false;");

            existingTables.Add("AracPlakalar");

            if (legacyPlakaExists)
            {
                await ExecuteNonQueryAsync(connection, transaction, @"
                    INSERT INTO ""AracPlakalar"" (""AracId"", ""Plaka"", ""GirisTarihi"", ""IslemTipi"", ""Aciklama"", ""CreatedAt"", ""IsDeleted"")
                    SELECT a.""Id"", a.""Plaka"", COALESCE(a.""CreatedAt"", CURRENT_TIMESTAMP), 1, 'Mevcut kayıttan aktarıldı', CURRENT_TIMESTAMP, false
                    FROM ""Araclar"" a
                    WHERE a.""Plaka"" IS NOT NULL
                      AND BTRIM(a.""Plaka"") <> ''
                      AND NOT EXISTS (
                          SELECT 1
                          FROM ""AracPlakalar"" ap
                          WHERE ap.""AracId"" = a.""Id""
                            AND ap.""Plaka"" = a.""Plaka""
                            AND ap.""IsDeleted"" = false
                      );

                    DROP INDEX IF EXISTS ""IX_Araclar_Plaka"";
                    ALTER TABLE ""Araclar"" DROP COLUMN IF EXISTS ""Plaka"";");
            }

            await ExecuteNonQueryAsync(connection, transaction, @"
                INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
                VALUES ('20260326224724_AracSasePlakaYapisi', '10.0.5')
                ON CONFLICT (""MigrationId"") DO NOTHING;");

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static async Task<bool> ContextTableExistsAsync(ApplicationDbContext context, string tableName)
    {
        var connection = context.Database.GetDbConnection();
        var closeAfter = connection.State != ConnectionState.Open;

        if (closeAfter)
            await connection.OpenAsync();

        try
        {
            await using var command = connection.CreateCommand();

            if (context.Database.IsNpgsql())
            {
                command.CommandText = $"SELECT to_regclass('\"{tableName}\"') IS NOT NULL";
            }
            else if (context.Database.IsSqlite())
            {
                command.CommandText = $"SELECT EXISTS(SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = '{tableName}')";
            }
            else
            {
                return true;
            }

            var result = await command.ExecuteScalarAsync();
            return result != null && Convert.ToBoolean(result);
        }
        finally
        {
            if (closeAfter)
                await connection.CloseAsync();
        }
    }

    private static async Task<bool> TableExistsAsync(NpgsqlConnection connection, string tableName)
    {
        await using var command = new NpgsqlCommand(@"
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.tables
                WHERE table_schema = 'public' AND table_name = @tableName
            );", connection);
        command.Parameters.AddWithValue("tableName", tableName);
        return (bool)(await command.ExecuteScalarAsync() ?? false);
    }

    private static async Task<bool> ColumnExistsAsync(NpgsqlConnection connection, string tableName, string columnName)
    {
        await using var command = new NpgsqlCommand(@"
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.columns
                WHERE table_schema = 'public'
                  AND table_name = @tableName
                  AND column_name = @columnName
            );", connection);
        command.Parameters.AddWithValue("tableName", tableName);
        command.Parameters.AddWithValue("columnName", columnName);
        return (bool)(await command.ExecuteScalarAsync() ?? false);
    }

    private static async Task EnsureMigrationHistoryEntryAsync(NpgsqlConnection connection, string migrationId)
    {
        await ExecuteNonQueryAsync(connection, null, @"
            CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
                ""MigrationId"" character varying(150) NOT NULL,
                ""ProductVersion"" character varying(32) NOT NULL,
                CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY (""MigrationId"")
            );");

        await using var insertCommand = new NpgsqlCommand(@"
            INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
            VALUES (@migrationId, @productVersion)
            ON CONFLICT (""MigrationId"") DO NOTHING;", connection);
        insertCommand.Parameters.AddWithValue("migrationId", migrationId);
        insertCommand.Parameters.AddWithValue("productVersion", "10.0.5");
        await insertCommand.ExecuteNonQueryAsync();
    }

    private static async Task ExecuteNonQueryAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, string sql)
    {
        await using var command = new NpgsqlCommand(sql, connection, transaction);
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Kritik tabloları oluşturur (yoksa)
    /// </summary>
    private static async Task EnsureCriticalTablesAsync(NpgsqlConnection connection, HashSet<string> existingTables)
    {
        // Personeller tablosu
        if (!existingTables.Contains("Personeller"))
        {
            try
            {
                await using var createCmd = new NpgsqlCommand(@"
                    CREATE TABLE ""Personeller"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""SoforKodu"" VARCHAR(50) NOT NULL DEFAULT '',
                        ""Ad"" VARCHAR(100) NOT NULL DEFAULT '',
                        ""Soyad"" VARCHAR(100) NOT NULL DEFAULT '',
                        ""TcKimlikNo"" VARCHAR(20),
                        ""Telefon"" VARCHAR(20),
                        ""Email"" VARCHAR(200),
                        ""Adres"" TEXT,
                        ""SiralamaNo"" INTEGER NOT NULL DEFAULT 0,
                        ""Gorev"" INTEGER NOT NULL DEFAULT 1,
                        ""Departman"" VARCHAR(100),
                        ""Pozisyon"" VARCHAR(100),
                        ""EhliyetNo"" VARCHAR(50),
                        ""EhliyetGecerlilikTarihi"" TIMESTAMP WITHOUT TIME ZONE,
                        ""SrcBelgesiGecerlilikTarihi"" TIMESTAMP WITHOUT TIME ZONE,
                        ""PsikoteknikGecerlilikTarihi"" TIMESTAMP WITHOUT TIME ZONE,
                        ""SaglikRaporuGecerlilikTarihi"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IseBaslamaTarihi"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IstenAyrilmaTarihi"" TIMESTAMP WITHOUT TIME ZONE,
                        ""SgkCikisTarihi"" TIMESTAMP WITHOUT TIME ZONE,
                        ""Aktif"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ""Notlar"" TEXT,
                        ""BrutMaasHesaplamaTipi"" INTEGER NOT NULL DEFAULT 0,
                        ""CalismaMiktari"" NUMERIC(18,2) NOT NULL DEFAULT 0,
                        ""BirimUcret"" NUMERIC(18,2) NOT NULL DEFAULT 0,
                        ""BrutMaas"" NUMERIC(18,2) NOT NULL DEFAULT 0,
                        ""ResmiNetMaas"" NUMERIC(18,2) NOT NULL DEFAULT 0,
                        ""DigerMaas"" NUMERIC(18,2) NOT NULL DEFAULT 0,
                        ""NetMaas"" NUMERIC(18,2) NOT NULL DEFAULT 0,
                        ""SGKBordroDahilMi"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""BordroTipiPersonel"" INTEGER NOT NULL DEFAULT 0,
                        ""ArgePersoneli"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""TopluMaas"" NUMERIC(18,2) NOT NULL DEFAULT 0,
                        ""SgkMaasi"" NUMERIC(18,2) NOT NULL DEFAULT 0,
                        ""BankaAdi"" VARCHAR(100),
                        ""IBAN"" VARCHAR(50),
                        ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    )", connection);
                await createCmd.ExecuteNonQueryAsync();
                existingTables.Add("Personeller");
                Console.WriteLine("Personeller tablosu oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Personeller tablosu oluşturulurken hata: {ex.Message}");
            }
        }

        // Faturalar tablosu
        if (!existingTables.Contains("Faturalar"))
        {
            try
            {
                await using var createCmd = new NpgsqlCommand(@"
                    CREATE TABLE ""Faturalar"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""FaturaNo"" VARCHAR(50) NOT NULL DEFAULT '',
                        ""FaturaTarihi"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""VadeTarihi"" TIMESTAMP WITHOUT TIME ZONE,
                        ""FaturaTipi"" INTEGER NOT NULL DEFAULT 1,
                        ""Durum"" INTEGER NOT NULL DEFAULT 1,
                        ""EFaturaTipi"" INTEGER NOT NULL DEFAULT 2,
                        ""FaturaYonu"" INTEGER NOT NULL DEFAULT 1,
                        ""EttnNo"" VARCHAR(100),
                        ""GibKodu"" VARCHAR(50),
                        ""GibOnayTarihi"" TIMESTAMP WITHOUT TIME ZONE,
                        ""ImportKaynak"" VARCHAR(50),
                        ""XmlDosyaYolu"" TEXT,
                        ""PdfDosyaYolu"" TEXT,
                        ""FirmaId"" INTEGER,
                        ""FirmalarArasiFatura"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""KarsiFirmaId"" INTEGER,
                        ""EslesenFaturaId"" INTEGER,
                        ""MahsupKapatildi"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""MahsupTarihi"" TIMESTAMP WITHOUT TIME ZONE,
                        ""AraToplam"" NUMERIC(18,2) NOT NULL DEFAULT 0,
                        ""IskontoTutar"" NUMERIC(18,2) NOT NULL DEFAULT 0,
                        ""KdvOrani"" NUMERIC(5,2) NOT NULL DEFAULT 20,
                        ""KdvTutar"" NUMERIC(18,2) NOT NULL DEFAULT 0,
                        ""GenelToplam"" NUMERIC(18,2) NOT NULL DEFAULT 0,
                        ""OdenenTutar"" NUMERIC(18,2) NOT NULL DEFAULT 0,
                        ""TevkifatliMi"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""TevkifatOrani"" NUMERIC(5,2) NOT NULL DEFAULT 0,
                        ""TevkifatKodu"" VARCHAR(20),
                        ""TevkifatTutar"" NUMERIC(18,2) NOT NULL DEFAULT 0,
                        ""Aciklama"" TEXT,
                        ""Notlar"" TEXT,
                        ""MuhasebeFisiOlusturuldu"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""MuhasebeFisId"" INTEGER,
                        ""AracId"" INTEGER,
                        ""AracFaturasi"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""CariId"" INTEGER NOT NULL,
                        ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    )", connection);
                await createCmd.ExecuteNonQueryAsync();
                existingTables.Add("Faturalar");
                Console.WriteLine("Faturalar tablosu oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Faturalar tablosu oluşturulurken hata: {ex.Message}");
            }
        }

        // BudgetOdemeler tablosu
        if (!existingTables.Contains("BudgetOdemeler"))
        {
            try
            {
                await using var createCmd = new NpgsqlCommand(@"
                    CREATE TABLE ""BudgetOdemeler"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""OdemeTarihi"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""OdemeAy"" INTEGER NOT NULL DEFAULT 1,
                        ""OdemeYil"" INTEGER NOT NULL DEFAULT 2024,
                        ""MasrafKalemi"" VARCHAR(200) NOT NULL DEFAULT '',
                        ""Aciklama"" TEXT,
                        ""Miktar"" NUMERIC(18,2) NOT NULL DEFAULT 0,
                        ""FirmaId"" INTEGER,
                        ""TaksitliMi"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""ToplamTaksitSayisi"" INTEGER NOT NULL DEFAULT 1,
                        ""KacinciTaksit"" INTEGER NOT NULL DEFAULT 1,
                        ""TaksitGrupId"" UUID,
                        ""TaksitBaslangicAy"" TIMESTAMP WITHOUT TIME ZONE,
                        ""TaksitBitisAy"" TIMESTAMP WITHOUT TIME ZONE,
                        ""Durum"" INTEGER NOT NULL DEFAULT 1,
                        ""Notlar"" TEXT,
                        ""GercekOdemeTarihi"" TIMESTAMP WITHOUT TIME ZONE,
                        ""OdemeYapildigiHesapId"" INTEGER,
                        ""OdenenTutar"" NUMERIC(18,2),
                        ""OdemeNotu"" VARCHAR(500),
                        ""BankaKasaHareketId"" INTEGER,
                        ""MasrafKesintisi"" NUMERIC(18,2) NOT NULL DEFAULT 0,
                        ""CezaKesintisi"" NUMERIC(18,2) NOT NULL DEFAULT 0,
                        ""DigerKesinti"" NUMERIC(18,2) NOT NULL DEFAULT 0,
                        ""KesintiAciklamasi"" VARCHAR(500),
                        ""FaturaId"" INTEGER,
                        ""FaturaIleKapatildi"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    )", connection);
                await createCmd.ExecuteNonQueryAsync();
                existingTables.Add("BudgetOdemeler");
                Console.WriteLine("BudgetOdemeler tablosu oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BudgetOdemeler tablosu oluşturulurken hata: {ex.Message}");
            }
        }

        // BankaKasaHareketleri tablosu
        if (!existingTables.Contains("BankaKasaHareketleri"))
        {
            try
            {
                await using var createCmd = new NpgsqlCommand(@"
                    CREATE TABLE ""BankaKasaHareketleri"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""IslemNo"" VARCHAR(50) NOT NULL DEFAULT '',
                        ""IslemTarihi"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""HareketTipi"" INTEGER NOT NULL DEFAULT 1,
                        ""Tutar"" NUMERIC(18,2) NOT NULL DEFAULT 0,
                        ""Aciklama"" TEXT,
                        ""BelgeNo"" VARCHAR(50),
                        ""IslemKaynak"" INTEGER NOT NULL DEFAULT 1,
                        ""MahsupHareketId"" INTEGER,
                        ""MahsupGrupId"" UUID,
                        ""MuhasebeHesapKodu"" TEXT,
                        ""MuhasebeAltHesapKodu"" TEXT,
                        ""KostMerkeziKodu"" TEXT,
                        ""ProjeKodu"" TEXT,
                        ""MuhasebeAciklama"" TEXT,
                        ""BankaHesapId"" INTEGER NOT NULL,
                        ""CariId"" INTEGER,
                        ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    )", connection);
                await createCmd.ExecuteNonQueryAsync();
                existingTables.Add("BankaKasaHareketleri");
                Console.WriteLine("BankaKasaHareketleri tablosu oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BankaKasaHareketleri tablosu oluşturulurken hata: {ex.Message}");
            }
        }

        // BudgetMasrafKalemleri tablosu
        if (!existingTables.Contains("BudgetMasrafKalemleri"))
        {
            try
            {
                await using var createCmd = new NpgsqlCommand(@"
                    CREATE TABLE ""BudgetMasrafKalemleri"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""KalemAdi"" VARCHAR(200) NOT NULL DEFAULT '',
                        ""Aktif"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    )", connection);
                await createCmd.ExecuteNonQueryAsync();
                existingTables.Add("BudgetMasrafKalemleri");
                Console.WriteLine("BudgetMasrafKalemleri tablosu oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BudgetMasrafKalemleri tablosu oluşturulurken hata: {ex.Message}");
            }
        }

        // BankaHesaplari tablosu
        if (!existingTables.Contains("BankaHesaplari"))
        {
            try
            {
                await using var createCmd = new NpgsqlCommand(@"
                    CREATE TABLE ""BankaHesaplari"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""HesapAdi"" VARCHAR(200) NOT NULL DEFAULT '',
                        ""BankaAdi"" VARCHAR(100),
                        ""SubeAdi"" VARCHAR(100),
                        ""SubeKodu"" VARCHAR(20),
                        ""HesapNo"" VARCHAR(50),
                        ""IBAN"" VARCHAR(50),
                        ""ParaBirimi"" VARCHAR(10) NOT NULL DEFAULT 'TRY',
                        ""Bakiye"" NUMERIC(18,2) NOT NULL DEFAULT 0,
                        ""HesapTipi"" INTEGER NOT NULL DEFAULT 1,
                        ""Aktif"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ""FirmaId"" INTEGER,
                        ""Notlar"" TEXT,
                        ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    )", connection);
                await createCmd.ExecuteNonQueryAsync();
                existingTables.Add("BankaHesaplari");
                Console.WriteLine("BankaHesaplari tablosu oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BankaHesaplari tablosu oluşturulurken hata: {ex.Message}");
            }
        }

        // Cariler tablosu
        if (!existingTables.Contains("Cariler"))
        {
            try
            {
                await using var createCmd = new NpgsqlCommand(@"
                    CREATE TABLE ""Cariler"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""CariKodu"" VARCHAR(50) NOT NULL DEFAULT '',
                        ""Unvan"" VARCHAR(300) NOT NULL DEFAULT '',
                        ""CariTipi"" INTEGER NOT NULL DEFAULT 1,
                        ""VergiDairesi"" VARCHAR(100),
                        ""VergiNo"" VARCHAR(20),
                        ""TcKimlikNo"" VARCHAR(20),
                        ""Adres"" TEXT,
                        ""Telefon"" VARCHAR(20),
                        ""Email"" VARCHAR(200),
                        ""WebSite"" VARCHAR(200),
                        ""YetkiliAdi"" VARCHAR(100),
                        ""YetkiliTelefon"" VARCHAR(20),
                        ""YetkiliEmail"" VARCHAR(200),
                        ""Bakiye"" NUMERIC(18,2) NOT NULL DEFAULT 0,
                        ""RiskLimiti"" NUMERIC(18,2) NOT NULL DEFAULT 0,
                        ""VadeGunu"" INTEGER NOT NULL DEFAULT 0,
                        ""IskontoOrani"" NUMERIC(5,2) NOT NULL DEFAULT 0,
                        ""Aktif"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ""FirmaId"" INTEGER,
                        ""Notlar"" TEXT,
                        ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    )", connection);
                await createCmd.ExecuteNonQueryAsync();
                existingTables.Add("Cariler");
                Console.WriteLine("Cariler tablosu oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cariler tablosu oluşturulurken hata: {ex.Message}");
            }
        }

        // Firmalar tablosu
        if (!existingTables.Contains("Firmalar"))
        {
            try
            {
                await using var createCmd = new NpgsqlCommand(@"
                    CREATE TABLE ""Firmalar"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""FirmaKodu"" VARCHAR(50) NOT NULL DEFAULT '',
                        ""FirmaAdi"" VARCHAR(300) NOT NULL DEFAULT '',
                        ""VergiDairesi"" VARCHAR(100),
                        ""VergiNo"" VARCHAR(20),
                        ""TicariSicilNo"" VARCHAR(50),
                        ""MersisNo"" VARCHAR(50),
                        ""Adres"" TEXT,
                        ""Telefon"" VARCHAR(20),
                        ""Email"" VARCHAR(200),
                        ""WebSite"" VARCHAR(200),
                        ""YetkiliAdi"" VARCHAR(100),
                        ""YetkiliTelefon"" VARCHAR(20),
                        ""YetkiliEmail"" VARCHAR(200),
                        ""LogoYolu"" TEXT,
                        ""Aktif"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ""Notlar"" TEXT,
                        ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    )", connection);
                await createCmd.ExecuteNonQueryAsync();
                existingTables.Add("Firmalar");
                Console.WriteLine("Firmalar tablosu oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Firmalar tablosu oluşturulurken hata: {ex.Message}");
            }
        }

        // Kullanicilar tablosu
        if (!existingTables.Contains("Kullanicilar"))
        {
            try
            {
                await using var createCmd = new NpgsqlCommand(@"
                    CREATE TABLE ""Kullanicilar"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""KullaniciAdi"" VARCHAR(100) NOT NULL DEFAULT '',
                        ""Email"" VARCHAR(200) NOT NULL DEFAULT '',
                        ""SifreHash"" TEXT NOT NULL DEFAULT '',
                        ""Ad"" VARCHAR(100) NOT NULL DEFAULT '',
                        ""Soyad"" VARCHAR(100) NOT NULL DEFAULT '',
                        ""Telefon"" VARCHAR(20),
                        ""ProfilFotoUrl"" TEXT,
                        ""RolId"" INTEGER,
                        ""Aktif"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ""SonGirisTarihi"" TIMESTAMP WITHOUT TIME ZONE,
                        ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    )", connection);
                await createCmd.ExecuteNonQueryAsync();
                existingTables.Add("Kullanicilar");
                Console.WriteLine("Kullanicilar tablosu oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kullanicilar tablosu oluşturulurken hata: {ex.Message}");
            }
        }

        // Roller tablosu
        if (!existingTables.Contains("Roller"))
        {
            try
            {
                await using var createCmd = new NpgsqlCommand(@"
                    CREATE TABLE ""Roller"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""RolAdi"" VARCHAR(100) NOT NULL DEFAULT '',
                        ""Aciklama"" TEXT,
                        ""Yetkiler"" TEXT,
                        ""Renk"" VARCHAR(20) DEFAULT '#dc3545',
                        ""Aktif"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    )", connection);
                await createCmd.ExecuteNonQueryAsync();
                existingTables.Add("Roller");
                Console.WriteLine("Roller tablosu oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Roller tablosu oluşturulurken hata: {ex.Message}");
            }
        }

        // PuantajKayitlar tablosu
        if (!existingTables.Contains("PuantajKayitlar"))
        {
            try
            {
                var gunKolonlari = string.Join(", ", Enumerable.Range(1, 31).Select(g => $@"""Gun{g:D2}"" INTEGER NOT NULL DEFAULT 0"));
                await using var createCmd = new NpgsqlCommand($@"
                    CREATE TABLE ""PuantajKayitlar"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""Yil"" INTEGER NOT NULL,
                        ""Ay"" INTEGER NOT NULL,
                        ""SoforId"" INTEGER NOT NULL,
                        ""Bolge"" VARCHAR(100),
                        ""SiraNo"" INTEGER NOT NULL DEFAULT 0,
                        ""AitFirmaAdi"" VARCHAR(200),
                        {gunKolonlari},
                        ""ToplamCalismaGunu"" INTEGER NOT NULL DEFAULT 0,
                        ""Notlar"" TEXT,
                        ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    )", connection);
                await createCmd.ExecuteNonQueryAsync();
                existingTables.Add("PuantajKayitlar");
                Console.WriteLine("PuantajKayitlar tablosu oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PuantajKayitlar tablosu oluşturulurken hata: {ex.Message}");
            }
        }

        // ========== Mega-migration (EbysEvrakKategoriler_Fix) eksik tabloları ==========

        // Sirketler tablosu (multi-tenant)
        if (!existingTables.Contains("Sirketler"))
        {
            try
            {
                await using var createCmd = new NpgsqlCommand(@"
                    CREATE TABLE ""Sirketler"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""SirketKodu"" VARCHAR(20) NOT NULL,
                        ""Unvan"" VARCHAR(250) NOT NULL,
                        ""KisaAd"" VARCHAR(100),
                        ""VergiDairesi"" VARCHAR(100),
                        ""VergiNo"" VARCHAR(11),
                        ""Adres"" VARCHAR(500),
                        ""Il"" VARCHAR(50),
                        ""Ilce"" VARCHAR(50),
                        ""PostaKodu"" VARCHAR(10),
                        ""Telefon"" VARCHAR(20),
                        ""Email"" VARCHAR(100),
                        ""WebSitesi"" VARCHAR(200),
                        ""LogoUrl"" VARCHAR(500),
                        ""Aktif"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ""ParaBirimi"" VARCHAR(5) NOT NULL DEFAULT 'TRY',
                        ""AyarlarJson"" TEXT,
                        ""LisansBitisTarihi"" TIMESTAMP WITHOUT TIME ZONE,
                        ""MaxKullaniciSayisi"" INTEGER NOT NULL DEFAULT 10,
                        ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    )", connection);
                await createCmd.ExecuteNonQueryAsync();
                existingTables.Add("Sirketler");

                // Unique index
                await using var idxCmd = new NpgsqlCommand(@"
                    CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Sirketler_SirketKodu"" ON ""Sirketler"" (""SirketKodu"")", connection);
                await idxCmd.ExecuteNonQueryAsync();

                Console.WriteLine("Sirketler tablosu oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sirketler tablosu oluşturulurken hata: {ex.Message}");
            }
        }

        // BildirimAyarlari tablosu
        if (!existingTables.Contains("BildirimAyarlari"))
        {
            try
            {
                await using var createCmd = new NpgsqlCommand(@"
                    CREATE TABLE ""BildirimAyarlari"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""KullaniciId"" INTEGER NOT NULL,
                        ""FaturaVadeUyarisi"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""EhliyetBitisUyarisi"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""SrcBelgesiUyarisi"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""PsikoteknikUyarisi"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""SaglikRaporuUyarisi"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""TrafikSigortaUyarisi"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""KaskoUyarisi"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""MuayeneUyarisi"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""DestekTalebiUyarisi"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""SistemBildirimleri"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""EpostaAlsin"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""EpostaAdresi"" TEXT,
                        ""SmsAlsin"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""SmsTelefon"" VARCHAR(20),
                        ""SmsVadeHatirlatma"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""SmsBelgeHatirlatma"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""VadeUyariGunSayisi"" INTEGER NOT NULL DEFAULT 7,
                        ""BelgeUyariGunSayisi"" INTEGER NOT NULL DEFAULT 30,
                        ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    )", connection);
                await createCmd.ExecuteNonQueryAsync();
                existingTables.Add("BildirimAyarlari");
                Console.WriteLine("BildirimAyarlari tablosu oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BildirimAyarlari tablosu oluşturulurken hata: {ex.Message}");
            }
        }

        // EpostaBildirimLoglari tablosu
        if (!existingTables.Contains("EpostaBildirimLoglari"))
        {
            try
            {
                await using var createCmd = new NpgsqlCommand(@"
                    CREATE TABLE ""EpostaBildirimLoglari"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""KullaniciId"" INTEGER NOT NULL,
                        ""EpostaAdresi"" VARCHAR(200) NOT NULL,
                        ""UyariSayisi"" INTEGER NOT NULL DEFAULT 0,
                        ""GonderimTarihi"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""Basarili"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""HataMesaji"" VARCHAR(500),
                        ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    )", connection);
                await createCmd.ExecuteNonQueryAsync();
                existingTables.Add("EpostaBildirimLoglari");
                Console.WriteLine("EpostaBildirimLoglari tablosu oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EpostaBildirimLoglari tablosu oluşturulurken hata: {ex.Message}");
            }
        }

        // WebhookEndpointler tablosu
        if (!existingTables.Contains("WebhookEndpointler"))
        {
            try
            {
                await using var createCmd = new NpgsqlCommand(@"
                    CREATE TABLE ""WebhookEndpointler"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""Ad"" VARCHAR(100) NOT NULL,
                        ""Aciklama"" VARCHAR(500),
                        ""Url"" VARCHAR(500) NOT NULL,
                        ""Secret"" VARCHAR(100),
                        ""Aktif"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ""MaxRetry"" INTEGER NOT NULL DEFAULT 3,
                        ""RetryDelaySaniye"" INTEGER NOT NULL DEFAULT 30,
                        ""OlayFiltresi"" TEXT,
                        ""HttpMethod"" TEXT NOT NULL DEFAULT 'POST',
                        ""Headers"" TEXT,
                        ""ToplamGonderim"" INTEGER NOT NULL DEFAULT 0,
                        ""BasariliGonderim"" INTEGER NOT NULL DEFAULT 0,
                        ""BasarisizGonderim"" INTEGER NOT NULL DEFAULT 0,
                        ""SonGonderimTarihi"" TIMESTAMP WITHOUT TIME ZONE,
                        ""SonBasariliTarih"" TIMESTAMP WITHOUT TIME ZONE,
                        ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    )", connection);
                await createCmd.ExecuteNonQueryAsync();
                existingTables.Add("WebhookEndpointler");
                Console.WriteLine("WebhookEndpointler tablosu oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebhookEndpointler tablosu oluşturulurken hata: {ex.Message}");
            }
        }

        // WebhookLoglar tablosu
        if (!existingTables.Contains("WebhookLoglar"))
        {
            try
            {
                await using var createCmd = new NpgsqlCommand(@"
                    CREATE TABLE ""WebhookLoglar"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""WebhookEndpointId"" INTEGER NOT NULL,
                        ""OlayTipi"" VARCHAR(100) NOT NULL,
                        ""Payload"" TEXT,
                        ""Durum"" INTEGER NOT NULL DEFAULT 0,
                        ""HttpStatusCode"" INTEGER NOT NULL DEFAULT 0,
                        ""ResponseBody"" TEXT,
                        ""GonderimTarihi"" TIMESTAMP WITHOUT TIME ZONE,
                        ""YanitTarihi"" TIMESTAMP WITHOUT TIME ZONE,
                        ""SureMilisaniye"" INTEGER NOT NULL DEFAULT 0,
                        ""RetryCount"" INTEGER NOT NULL DEFAULT 0,
                        ""HataMesaji"" TEXT,
                        ""IliskiliTablo"" TEXT,
                        ""IliskiliKayitId"" INTEGER,
                        ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    )", connection);
                await createCmd.ExecuteNonQueryAsync();
                existingTables.Add("WebhookLoglar");
                Console.WriteLine("WebhookLoglar tablosu oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebhookLoglar tablosu oluşturulurken hata: {ex.Message}");
            }
        }

        // AracBolgeler tablosu
        if (!existingTables.Contains("AracBolgeler"))
        {
            try
            {
                await using var createCmd = new NpgsqlCommand(@"
                    CREATE TABLE ""AracBolgeler"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""BolgeAdi"" TEXT NOT NULL,
                        ""Tip"" INTEGER NOT NULL DEFAULT 0,
                        ""MerkezLatitude"" DOUBLE PRECISION,
                        ""MerkezLongitude"" DOUBLE PRECISION,
                        ""YaricapMetre"" DOUBLE PRECISION,
                        ""PoligonKoordinatlari"" TEXT,
                        ""Renk"" TEXT,
                        ""GirisBildirimi"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""CikisBildirimi"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""Aktif"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ""Notlar"" TEXT,
                        ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    )", connection);
                await createCmd.ExecuteNonQueryAsync();
                existingTables.Add("AracBolgeler");
                Console.WriteLine("AracBolgeler tablosu oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AracBolgeler tablosu oluşturulurken hata: {ex.Message}");
            }
        }

        // AracBolgeAtamalar tablosu
        if (!existingTables.Contains("AracBolgeAtamalar"))
        {
            try
            {
                await using var createCmd = new NpgsqlCommand(@"
                    CREATE TABLE ""AracBolgeAtamalar"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""AracBolgeId"" INTEGER NOT NULL,
                        ""AracId"" INTEGER NOT NULL,
                        ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    )", connection);
                await createCmd.ExecuteNonQueryAsync();
                existingTables.Add("AracBolgeAtamalar");
                Console.WriteLine("AracBolgeAtamalar tablosu oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AracBolgeAtamalar tablosu oluşturulurken hata: {ex.Message}");
            }
        }

        // AracTakipCihazlar tablosu
        if (!existingTables.Contains("AracTakipCihazlar"))
        {
            try
            {
                await using var createCmd = new NpgsqlCommand(@"
                    CREATE TABLE ""AracTakipCihazlar"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""AracId"" INTEGER NOT NULL,
                        ""CihazId"" TEXT NOT NULL,
                        ""CihazMarka"" TEXT,
                        ""CihazModel"" TEXT,
                        ""SimKartNo"" TEXT,
                        ""Aktif"" BOOLEAN NOT NULL DEFAULT TRUE,
                        ""KurulumTarihi"" TIMESTAMP WITHOUT TIME ZONE,
                        ""SonIletisimZamani"" TIMESTAMP WITHOUT TIME ZONE,
                        ""BataryaSeviyesi"" INTEGER,
                        ""SinyalGucu"" INTEGER,
                        ""Notlar"" TEXT,
                        ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    )", connection);
                await createCmd.ExecuteNonQueryAsync();
                existingTables.Add("AracTakipCihazlar");
                Console.WriteLine("AracTakipCihazlar tablosu oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AracTakipCihazlar tablosu oluşturulurken hata: {ex.Message}");
            }
        }

        // AracKonumlar tablosu
        if (!existingTables.Contains("AracKonumlar"))
        {
            try
            {
                await using var createCmd = new NpgsqlCommand(@"
                    CREATE TABLE ""AracKonumlar"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""AracTakipCihazId"" INTEGER NOT NULL,
                        ""Latitude"" DOUBLE PRECISION NOT NULL,
                        ""Longitude"" DOUBLE PRECISION NOT NULL,
                        ""Hiz"" DOUBLE PRECISION,
                        ""Yon"" DOUBLE PRECISION,
                        ""Rakım"" DOUBLE PRECISION,
                        ""Hassasiyet"" DOUBLE PRECISION,
                        ""KayitZamani"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""KontakDurumu"" BOOLEAN,
                        ""MotorDurumu"" BOOLEAN,
                        ""YakitSeviyesi"" INTEGER,
                        ""Kilometre"" INTEGER,
                        ""Sicaklik"" DOUBLE PRECISION,
                        ""OlayTipi"" INTEGER NOT NULL DEFAULT 0,
                        ""Adres"" TEXT,
                        ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    )", connection);
                await createCmd.ExecuteNonQueryAsync();
                existingTables.Add("AracKonumlar");
                Console.WriteLine("AracKonumlar tablosu oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AracKonumlar tablosu oluşturulurken hata: {ex.Message}");
            }
        }

        // AracTakipAlarmlar tablosu
        if (!existingTables.Contains("AracTakipAlarmlar"))
        {
            try
            {
                await using var createCmd = new NpgsqlCommand(@"
                    CREATE TABLE ""AracTakipAlarmlar"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""AracTakipCihazId"" INTEGER NOT NULL,
                        ""AlarmTipi"" INTEGER NOT NULL DEFAULT 0,
                        ""AlarmZamani"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""Latitude"" DOUBLE PRECISION,
                        ""Longitude"" DOUBLE PRECISION,
                        ""Mesaj"" TEXT,
                        ""Deger"" DOUBLE PRECISION,
                        ""Okundu"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""Islendi"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""Notlar"" TEXT,
                        ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    )", connection);
                await createCmd.ExecuteNonQueryAsync();
                existingTables.Add("AracTakipAlarmlar");
                Console.WriteLine("AracTakipAlarmlar tablosu oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AracTakipAlarmlar tablosu oluşturulurken hata: {ex.Message}");
            }
        }

        // EbysAramaGecmisleri tablosu
        if (!existingTables.Contains("EbysAramaGecmisleri"))
        {
            try
            {
                await using var createCmd = new NpgsqlCommand(@"
                    CREATE TABLE ""EbysAramaGecmisleri"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""KullaniciId"" INTEGER NOT NULL,
                        ""AramaMetni"" VARCHAR(500) NOT NULL,
                        ""FiltreJson"" VARCHAR(2000),
                        ""SonucSayisi"" INTEGER NOT NULL DEFAULT 0,
                        ""AramaTarihi"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    )", connection);
                await createCmd.ExecuteNonQueryAsync();
                existingTables.Add("EbysAramaGecmisleri");
                Console.WriteLine("EbysAramaGecmisleri tablosu oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EbysAramaGecmisleri tablosu oluşturulurken hata: {ex.Message}");
            }
        }

        // EbysBelgeEmbeddingler tablosu
        if (!existingTables.Contains("EbysBelgeEmbeddingler"))
        {
            try
            {
                await using var createCmd = new NpgsqlCommand(@"
                    CREATE TABLE ""EbysBelgeEmbeddingler"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""Kaynak"" INTEGER NOT NULL DEFAULT 0,
                        ""KaynakId"" INTEGER NOT NULL,
                        ""DosyaId"" INTEGER,
                        ""Metin"" VARCHAR(8000) NOT NULL,
                        ""MetinOzet"" VARCHAR(500),
                        ""EmbeddingJson"" TEXT NOT NULL,
                        ""EmbeddingBoyutu"" INTEGER NOT NULL DEFAULT 0,
                        ""ModelAdi"" VARCHAR(100),
                        ""OlusturmaTarihi"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""GuncellemeTarihi"" TIMESTAMP WITHOUT TIME ZONE,
                        ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    )", connection);
                await createCmd.ExecuteNonQueryAsync();
                existingTables.Add("EbysBelgeEmbeddingler");
                Console.WriteLine("EbysBelgeEmbeddingler tablosu oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EbysBelgeEmbeddingler tablosu oluşturulurken hata: {ex.Message}");
            }
        }

        // EbysEvrakDosyaVersiyonlar tablosu
        if (!existingTables.Contains("EbysEvrakDosyaVersiyonlar"))
        {
            try
            {
                await using var createCmd = new NpgsqlCommand(@"
                    CREATE TABLE ""EbysEvrakDosyaVersiyonlar"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""EvrakDosyaId"" INTEGER NOT NULL,
                        ""VersiyonNo"" INTEGER NOT NULL DEFAULT 1,
                        ""DosyaAdi"" TEXT NOT NULL,
                        ""DosyaYolu"" TEXT NOT NULL,
                        ""DosyaTipi"" TEXT,
                        ""DosyaBoyutu"" BIGINT NOT NULL DEFAULT 0,
                        ""Aciklama"" TEXT,
                        ""DegisiklikNotu"" TEXT,
                        ""OlusturanKullaniciId"" INTEGER,
                        ""OlusturmaTarihi"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    )", connection);
                await createCmd.ExecuteNonQueryAsync();
                existingTables.Add("EbysEvrakDosyaVersiyonlar");
                Console.WriteLine("EbysEvrakDosyaVersiyonlar tablosu oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EbysEvrakDosyaVersiyonlar tablosu oluşturulurken hata: {ex.Message}");
            }
        }

        // EbysKayitliAramalar tablosu
        if (!existingTables.Contains("EbysKayitliAramalar"))
        {
            try
            {
                await using var createCmd = new NpgsqlCommand(@"
                    CREATE TABLE ""EbysKayitliAramalar"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""KullaniciId"" INTEGER NOT NULL,
                        ""AramaAdi"" VARCHAR(100) NOT NULL,
                        ""Aciklama"" VARCHAR(250),
                        ""FiltreJson"" VARCHAR(2000) NOT NULL,
                        ""BildirimAktif"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""SiraNo"" INTEGER NOT NULL DEFAULT 0,
                        ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    )", connection);
                await createCmd.ExecuteNonQueryAsync();
                existingTables.Add("EbysKayitliAramalar");
                Console.WriteLine("EbysKayitliAramalar tablosu oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EbysKayitliAramalar tablosu oluşturulurken hata: {ex.Message}");
            }
        }

        // PersonelOzlukEvrakVersiyonlar tablosu
        if (!existingTables.Contains("PersonelOzlukEvrakVersiyonlar"))
        {
            try
            {
                await using var createCmd = new NpgsqlCommand(@"
                    CREATE TABLE ""PersonelOzlukEvrakVersiyonlar"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""PersonelOzlukEvrakId"" INTEGER NOT NULL,
                        ""VersiyonNo"" INTEGER NOT NULL DEFAULT 1,
                        ""DosyaYolu"" TEXT,
                        ""DosyaAdi"" TEXT,
                        ""DosyaTipi"" TEXT,
                        ""DosyaBoyutu"" BIGINT,
                        ""Aciklama"" TEXT,
                        ""DegisiklikNotu"" TEXT,
                        ""OlusturanKullaniciId"" INTEGER,
                        ""OlusturmaTarihi"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    )", connection);
                await createCmd.ExecuteNonQueryAsync();
                existingTables.Add("PersonelOzlukEvrakVersiyonlar");
                Console.WriteLine("PersonelOzlukEvrakVersiyonlar tablosu oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PersonelOzlukEvrakVersiyonlar tablosu oluşturulurken hata: {ex.Message}");
            }
        }

        // AracEvrakDosyaVersiyonlar tablosu
        if (!existingTables.Contains("AracEvrakDosyaVersiyonlar"))
        {
            try
            {
                await using var createCmd = new NpgsqlCommand(@"
                    CREATE TABLE ""AracEvrakDosyaVersiyonlar"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""AracEvrakDosyaId"" INTEGER NOT NULL,
                        ""VersiyonNo"" INTEGER NOT NULL DEFAULT 1,
                        ""DosyaAdi"" TEXT NOT NULL,
                        ""DosyaYolu"" TEXT NOT NULL,
                        ""DosyaTipi"" TEXT,
                        ""DosyaBoyutu"" BIGINT NOT NULL DEFAULT 0,
                        ""Aciklama"" TEXT,
                        ""DegisiklikNotu"" TEXT,
                        ""OlusturanKullaniciId"" INTEGER,
                        ""OlusturmaTarihi"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    )", connection);
                await createCmd.ExecuteNonQueryAsync();
                existingTables.Add("AracEvrakDosyaVersiyonlar");
                Console.WriteLine("AracEvrakDosyaVersiyonlar tablosu oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AracEvrakDosyaVersiyonlar tablosu oluşturulurken hata: {ex.Message}");
            }
        }

        // SirketTransferLoglari tablosu
        if (!existingTables.Contains("SirketTransferLoglari"))
        {
            try
            {
                await using var createCmd = new NpgsqlCommand(@"
                    CREATE TABLE ""SirketTransferLoglari"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""EntityTuru"" VARCHAR(50) NOT NULL,
                        ""EntityId"" INTEGER NOT NULL,
                        ""EntityAciklama"" VARCHAR(500),
                        ""KaynakSirketId"" INTEGER NOT NULL,
                        ""HedefSirketId"" INTEGER NOT NULL,
                        ""KullaniciId"" INTEGER NOT NULL,
                        ""TransferTarihi"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""Durum"" INTEGER NOT NULL DEFAULT 0,
                        ""HataMesaji"" VARCHAR(2000),
                        ""IliskiliVerilerTransferEdildi"" BOOLEAN NOT NULL DEFAULT FALSE,
                        ""IliskiliEntitySayisi"" INTEGER NOT NULL DEFAULT 0,
                        ""Notlar"" VARCHAR(1000),
                        ""CreatedAt"" TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ""UpdatedAt"" TIMESTAMP WITHOUT TIME ZONE,
                        ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
                    )", connection);
                await createCmd.ExecuteNonQueryAsync();
                existingTables.Add("SirketTransferLoglari");
                Console.WriteLine("SirketTransferLoglari tablosu oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SirketTransferLoglari tablosu oluşturulurken hata: {ex.Message}");
            }
        }

        Console.WriteLine("Kritik tablolar kontrol edildi.");
    }
}
