using CRMFiloServis.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data;

namespace CRMFiloServis.Web.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext context, IConfiguration configuration)
    {
        var dbProvider = context.Database.IsNpgsql()
            ? "PostgreSQL"
            : context.Database.IsSqlite()
                ? "SQLite"
                : configuration.GetValue<string>("DatabaseProvider") ?? "SQLite";
        
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

        // Budget masraf kalemleri her zaman kontrol et
        await SeedBudgetMasrafKalemleriAsync(context);
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

        await SeedBudgetMasrafKalemleriAsync(context);
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
    /// PostgreSQL için eksik kolonları otomatik ekler
    /// </summary>
    private static async Task EnsurePostgreSqlMissingColumnsAsync(ApplicationDbContext context, IConfiguration configuration)
    {
        var connectionString = GetDefaultConnectionString(context, configuration);

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

            // Soforlar tablosu - Sıralama
            ("Soforlar", "SiralamaNo", "INTEGER", "0"),

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

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        foreach (var (table, column, type, defaultValue) in missingColumns)
        {
            try
            {
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
}
