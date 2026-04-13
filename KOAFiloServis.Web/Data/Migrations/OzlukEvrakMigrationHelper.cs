using System.Data;
using Microsoft.EntityFrameworkCore;

namespace KOAFiloServis.Web.Data.Migrations;

public static class OzlukEvrakMigrationHelper
{
    public static async Task ApplyOzlukEvrakMigrationAsync(ApplicationDbContext context)
    {
        try
        {
            if (context.Database.IsNpgsql())
            {
                var sql = @"
CREATE TABLE IF NOT EXISTS ""OzlukEvrakTanimlari"" (
    ""Id"" SERIAL PRIMARY KEY,
    ""EvrakAdi"" VARCHAR(200) NOT NULL,
    ""Aciklama"" VARCHAR(500),
    ""Kategori"" INTEGER NOT NULL DEFAULT 1,
    ""Zorunlu"" BOOLEAN NOT NULL DEFAULT TRUE,
    ""SiraNo"" INTEGER NOT NULL DEFAULT 1,
    ""Aktif"" BOOLEAN NOT NULL DEFAULT TRUE,
    ""GecerliGorevler"" VARCHAR(50),
    ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ""UpdatedAt"" TIMESTAMP WITH TIME ZONE,
    ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS ""PersonelOzlukEvraklar"" (
    ""Id"" SERIAL PRIMARY KEY,
    ""SoforId"" INTEGER NOT NULL,
    ""EvrakTanimId"" INTEGER NOT NULL,
    ""Tamamlandi"" BOOLEAN NOT NULL DEFAULT FALSE,
    ""TamamlanmaTarihi"" TIMESTAMP WITH TIME ZONE,
    ""DosyaYolu"" VARCHAR(500),
    ""Aciklama"" VARCHAR(500),
    ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ""UpdatedAt"" TIMESTAMP WITH TIME ZONE,
    ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
    CONSTRAINT ""FK_PersonelOzlukEvraklar_Personeller"" FOREIGN KEY (""SoforId"") REFERENCES ""Personeller"" (""Id"") ON DELETE CASCADE,
    CONSTRAINT ""FK_PersonelOzlukEvraklar_OzlukEvrakTanimlari"" FOREIGN KEY (""EvrakTanimId"") REFERENCES ""OzlukEvrakTanimlari"" (""Id"") ON DELETE CASCADE
);";

                await context.Database.ExecuteSqlRawAsync(sql);
            }
            else if (context.Database.IsSqlite())
            {
                await EnsureSqliteSchemaAsync(context);
            }

            // Index'leri ayrı oluştur (hata vermemesi için try-catch)
            try
            {
                await context.Database.ExecuteSqlRawAsync(@"CREATE INDEX IF NOT EXISTS ""IX_PersonelOzlukEvraklar_SoforId"" ON ""PersonelOzlukEvraklar"" (""SoforId"")");
                await context.Database.ExecuteSqlRawAsync(@"CREATE INDEX IF NOT EXISTS ""IX_PersonelOzlukEvraklar_EvrakTanimId"" ON ""PersonelOzlukEvraklar"" (""EvrakTanimId"")");
                await context.Database.ExecuteSqlRawAsync(@"CREATE INDEX IF NOT EXISTS ""IX_OzlukEvrakTanimlari_Kategori"" ON ""OzlukEvrakTanimlari"" (""Kategori"")");
            }
            catch { /* Index zaten varsa hata vermesini engelle */ }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Özlük evrak migration hatası: {ex.Message}");
        }
    }

    private static async Task EnsureSqliteSchemaAsync(ApplicationDbContext context)
    {
        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;

        if (shouldClose)
        {
            await connection.OpenAsync();
        }

        try
        {
            if (await RequiresSqliteTableRebuildAsync(connection, "OzlukEvrakTanimlari"))
            {
                await ExecuteSqliteNonQueryAsync(connection, "DROP TABLE IF EXISTS \"PersonelOzlukEvraklar\"");
                await ExecuteSqliteNonQueryAsync(connection, "DROP TABLE IF EXISTS \"OzlukEvrakTanimlari\"");
            }

            await ExecuteSqliteNonQueryAsync(connection, @"
CREATE TABLE IF NOT EXISTS ""OzlukEvrakTanimlari"" (
    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_OzlukEvrakTanimlari"" PRIMARY KEY AUTOINCREMENT,
    ""EvrakAdi"" TEXT NOT NULL,
    ""Aciklama"" TEXT NULL,
    ""Kategori"" INTEGER NOT NULL DEFAULT 1,
    ""Zorunlu"" INTEGER NOT NULL DEFAULT 1,
    ""SiraNo"" INTEGER NOT NULL DEFAULT 1,
    ""Aktif"" INTEGER NOT NULL DEFAULT 1,
    ""GecerliGorevler"" TEXT NULL,
    ""CreatedAt"" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ""UpdatedAt"" TEXT NULL,
    ""IsDeleted"" INTEGER NOT NULL DEFAULT 0
)");

            await ExecuteSqliteNonQueryAsync(connection, @"
CREATE TABLE IF NOT EXISTS ""PersonelOzlukEvraklar"" (
    ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_PersonelOzlukEvraklar"" PRIMARY KEY AUTOINCREMENT,
    ""SoforId"" INTEGER NOT NULL,
    ""EvrakTanimId"" INTEGER NOT NULL,
    ""Tamamlandi"" INTEGER NOT NULL DEFAULT 0,
    ""TamamlanmaTarihi"" TEXT NULL,
    ""DosyaYolu"" TEXT NULL,
    ""Aciklama"" TEXT NULL,
    ""CreatedAt"" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ""UpdatedAt"" TEXT NULL,
    ""IsDeleted"" INTEGER NOT NULL DEFAULT 0,
    CONSTRAINT ""FK_PersonelOzlukEvraklar_Personeller"" FOREIGN KEY (""SoforId"") REFERENCES ""Personeller"" (""Id"") ON DELETE CASCADE,
    CONSTRAINT ""FK_PersonelOzlukEvraklar_OzlukEvrakTanimlari"" FOREIGN KEY (""EvrakTanimId"") REFERENCES ""OzlukEvrakTanimlari"" (""Id"") ON DELETE CASCADE
)");
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static async Task<bool> RequiresSqliteTableRebuildAsync(System.Data.Common.DbConnection connection, string tableName)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT type, pk FROM pragma_table_info('{tableName}') WHERE name = 'Id' LIMIT 1";

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return false;
        }

        var type = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
        var pk = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
        return pk != 1 || !string.Equals(type, "INTEGER", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task ExecuteSqliteNonQueryAsync(System.Data.Common.DbConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }
}
