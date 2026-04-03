using CRMFiloServis.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                Console.WriteLine($"Bekleyen migration sayisi: {pendingMigrations.Count()}");
                await context.Database.MigrateAsync();
                Console.WriteLine("Migration'lar basariyla uygulandi.");
            }
        }
        catch (Exception ex)
        {
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

        // PiyasaKaynaklar tablosunu oluştur
        await EnsurePiyasaKaynaklarTableAsync(context, dbProvider, configuration);

        // TekrarlayanOdemeler tablosunu oluştur
        await EnsureTekrarlayanOdemelerTableAsync(context, dbProvider, configuration);

        // Roller tablosuna Renk kolonu ekle (yoksa)
        await EnsureRollerRenkColumnAsync(context, dbProvider, configuration);

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
                await using var connection = context.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

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
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Roller Renk kolonu ekleme hatası: {ex.Message}");
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
}
