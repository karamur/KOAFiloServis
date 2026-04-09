using CRMFiloServis.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;

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
                            Console.WriteLine("SQLite migration gecmisi mevcut tablo yapisina gore duzeltildi.");
                            return;
                        }
                    }
                }
                catch (Exception sqliteFixEx)
                {
                    Console.WriteLine($"SQLite migration duzeltme hatasi: {sqliteFixEx.Message}");
                }
            }

            if (context.Database.IsNpgsql())
            {
                try
                {
                    var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
                    migrationRecovered = await EnsurePostgreSqlMigrationHistoryAsync(context, pendingMigrations, ex);
                    if (migrationRecovered)
                    {
                        Console.WriteLine("PostgreSQL migration gecmisi mevcut tablo yapisina gore duzeltildi.");
                    }
                }
                catch (Exception pgFixEx)
                {
                    Console.WriteLine($"PostgreSQL migration duzeltme hatasi: {pgFixEx.Message}");
                }
            }

            if (migrationRecovered)
            {
                goto AfterMigrationHandling;
            }

            // Migration hatasi durumunda EnsureCreated dene
            Console.WriteLine($"Migration hatasi: {ex.Message}. EnsureCreated deneniyor...");
            
            try
            {
                await context.Database.EnsureCreatedAsync();
            }
            catch (Exception createEx)
            {
                Console.WriteLine($"EnsureCreated hatasi: {createEx.Message}");
                // Tablolar zaten var, devam et
            }
        }

AfterMigrationHandling:

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

        // Destek Talebi seed verilerini ekle
        await SeedDestekTalebiVerileriAsync(context);
    }

    private static string GetDefaultConnectionString(ApplicationDbContext context, IConfiguration configuration)
    {
        return context.Database.GetConnectionString()
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection bulunamadi.");
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

    private static async Task<bool> EnsurePostgreSqlMigrationHistoryAsync(ApplicationDbContext context, List<string> pendingMigrations, Exception migrationException)
    {
        const string addBudgetHedefMigrationId = "20260409091451_AddBudgetHedef";

        if (!pendingMigrations.Contains(addBudgetHedefMigrationId))
        {
            return false;
        }

        var errorText = migrationException.ToString();
        var looksLikeDuplicateSchemaObject = errorText.Contains("already exists", StringComparison.OrdinalIgnoreCase)
            || errorText.Contains("42701", StringComparison.OrdinalIgnoreCase)
            || errorText.Contains("42P07", StringComparison.OrdinalIgnoreCase)
            || errorText.Contains("42710", StringComparison.OrdinalIgnoreCase);

        if (!looksLikeDuplicateSchemaObject)
        {
            return false;
        }

        var connection = (NpgsqlConnection)context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;

        if (shouldClose)
        {
            await connection.OpenAsync();
        }

        try
        {
            await using var historyCreate = new NpgsqlCommand(@"
                CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
                    ""MigrationId"" character varying(150) NOT NULL,
                    ""ProductVersion"" character varying(32) NOT NULL,
                    CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY (""MigrationId"")
                );", connection);
            await historyCreate.ExecuteNonQueryAsync();

            await using var markerCmd = new NpgsqlCommand(@"
                SELECT EXISTS (
                    SELECT 1
                    FROM information_schema.columns
                    WHERE table_name = 'PuantajKayitlar' AND column_name = 'AitFirmaAdi'
                ) OR EXISTS (
                    SELECT 1
                    FROM information_schema.tables
                    WHERE table_name = 'BudgetHedefler'
                ) OR EXISTS (
                    SELECT 1
                    FROM information_schema.tables
                    WHERE table_name = 'CariHatirlatmalar'
                );", connection);

            var schemaLooksApplied = (bool)(await markerCmd.ExecuteScalarAsync() ?? false);
            if (!schemaLooksApplied)
            {
                return false;
            }

            await using var existsCmd = new NpgsqlCommand(@"
                SELECT EXISTS (
                    SELECT 1 FROM ""__EFMigrationsHistory"" WHERE ""MigrationId"" = @migrationId
                );", connection);
            existsCmd.Parameters.AddWithValue("migrationId", addBudgetHedefMigrationId);
            var alreadyRecorded = (bool)(await existsCmd.ExecuteScalarAsync() ?? false);
            if (alreadyRecorded)
            {
                return true;
            }

            await using var insertCmd = new NpgsqlCommand(@"
                INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
                VALUES (@migrationId, @productVersion);", connection);
            insertCmd.Parameters.AddWithValue("migrationId", addBudgetHedefMigrationId);
            insertCmd.Parameters.AddWithValue("productVersion", "10.0.5");
            await insertCmd.ExecuteNonQueryAsync();

            return true;
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
        catch
        {
            // Hata olursa EnsureCreated dene
            try
            {
                await context.Database.EnsureCreatedAsync();
            }
            catch
            {
                // Tablolar zaten var, devam et
            }
        }

        var dbProvider = context.Database.IsNpgsql() ? "PostgreSQL" : context.Database.IsSqlite() ? "SQLite" : "SQLite";
        await EnsureDestekModuluTablesAsync(context, dbProvider, null);
        await EnsureDestekModuluColumnsAsync(context, dbProvider, null);

        await SeedBudgetMasrafKalemleriAsync(context);

        // Destek Talebi seed verilerini ekle
        await SeedDestekTalebiVerileriAsync(context);
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
            );

            INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
            VALUES ('20260326224724_AracSasePlakaYapisi', '10.0.5')
            ON CONFLICT (""MigrationId"") DO NOTHING;");
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

        // BankaHesaplar tablosu
        if (!existingTables.Contains("BankaHesaplar"))
        {
            try
            {
                await using var createCmd = new NpgsqlCommand(@"
                    CREATE TABLE ""BankaHesaplar"" (
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
                existingTables.Add("BankaHesaplar");
                Console.WriteLine("BankaHesaplar tablosu oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BankaHesaplar tablosu oluşturulurken hata: {ex.Message}");
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

        Console.WriteLine("Kritik tablolar kontrol edildi.");
    }
}
