using System.Data;
using Microsoft.EntityFrameworkCore;

namespace KOAFiloServis.Web.Data.Migrations;

public static class FaturaGibDurumMigrationHelper
{
    public static async Task ApplyFaturaGibDurumAsync(ApplicationDbContext context)
    {
        try
        {
            const string tableName = "Faturalar";

            if (context.Database.IsNpgsql())
            {
                var sql = @"
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Faturalar' AND column_name = 'GibDurumu') THEN
        ALTER TABLE ""Faturalar"" ADD COLUMN ""GibDurumu"" integer NOT NULL DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Faturalar' AND column_name = 'GibGonderimTarihi') THEN
        ALTER TABLE ""Faturalar"" ADD COLUMN ""GibGonderimTarihi"" timestamp without time zone NULL;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Faturalar' AND column_name = 'GibDurumGuncellemeTarihi') THEN
        ALTER TABLE ""Faturalar"" ADD COLUMN ""GibDurumGuncellemeTarihi"" timestamp without time zone NULL;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Faturalar' AND column_name = 'GibDurumMesaji') THEN
        ALTER TABLE ""Faturalar"" ADD COLUMN ""GibDurumMesaji"" text NULL;
    END IF;
END $$;";

                await context.Database.ExecuteSqlRawAsync(sql);
                return;
            }

            if (context.Database.IsSqlite())
            {
                await EnsureSqliteColumnAsync(context, tableName, "GibDurumu", "INTEGER NOT NULL DEFAULT 0");
                await EnsureSqliteColumnAsync(context, tableName, "GibGonderimTarihi", "TEXT NULL");
                await EnsureSqliteColumnAsync(context, tableName, "GibDurumGuncellemeTarihi", "TEXT NULL");
                await EnsureSqliteColumnAsync(context, tableName, "GibDurumMesaji", "TEXT NULL");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatura GİB durum migration hatası: {ex.Message}");
        }
    }

    private static async Task EnsureSqliteColumnAsync(ApplicationDbContext context, string tableName, string columnName, string columnDefinition)
    {
        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;

        if (shouldClose)
        {
            await connection.OpenAsync();
        }

        try
        {
            await using var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = $"SELECT 1 FROM pragma_table_info('{tableName}') WHERE name = $columnName LIMIT 1";

            var parameter = checkCommand.CreateParameter();
            parameter.ParameterName = "$columnName";
            parameter.Value = columnName;
            checkCommand.Parameters.Add(parameter);

            var exists = await checkCommand.ExecuteScalarAsync() is not null;
            if (exists)
            {
                return;
            }

            await using var alterCommand = connection.CreateCommand();
            alterCommand.CommandText = $"ALTER TABLE \"{tableName}\" ADD COLUMN \"{columnName}\" {columnDefinition}";
            await alterCommand.ExecuteNonQueryAsync();
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
