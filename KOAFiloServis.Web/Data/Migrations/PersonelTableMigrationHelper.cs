using System.Data;
using Microsoft.EntityFrameworkCore;

namespace KOAFiloServis.Web.Data.Migrations;

public static class PersonelTableMigrationHelper
{
    public static async Task ApplyPersonelTableMigrationAsync(ApplicationDbContext context)
    {
        try
        {
            if (context.Database.IsNpgsql())
            {
                var sql = @"
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Soforler')
       AND NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Personeller') THEN
        ALTER TABLE ""Soforler"" RENAME TO ""Personeller"";
    END IF;
END $$;";

                await context.Database.ExecuteSqlRawAsync(sql);
                return;
            }

            if (context.Database.IsSqlite())
            {
                var connection = context.Database.GetDbConnection();
                var shouldClose = connection.State != ConnectionState.Open;

                if (shouldClose)
                {
                    await connection.OpenAsync();
                }

                try
                {
                    var hasSoforler = await TableExistsAsync(connection, "Soforler");
                    var hasPersoneller = await TableExistsAsync(connection, "Personeller");

                    if (hasSoforler && !hasPersoneller)
                    {
                        await using var command = connection.CreateCommand();
                        command.CommandText = "ALTER TABLE \"Soforler\" RENAME TO \"Personeller\"";
                        await command.ExecuteNonQueryAsync();
                    }

                    if (await TableExistsAsync(connection, "Personeller") &&
                        !await ColumnExistsAsync(connection, "Personeller", "SiralamaNo"))
                    {
                        await using var addColumnCommand = connection.CreateCommand();
                        addColumnCommand.CommandText = "ALTER TABLE \"Personeller\" ADD COLUMN \"SiralamaNo\" INTEGER NOT NULL DEFAULT 0";
                        await addColumnCommand.ExecuteNonQueryAsync();
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
            Console.WriteLine($"Personel tablo migration hatası: {ex.Message}");
        }
    }

    private static async Task<bool> TableExistsAsync(System.Data.Common.DbConnection connection, string tableName)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = $tableName LIMIT 1";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "$tableName";
        parameter.Value = tableName;
        command.Parameters.Add(parameter);

        return await command.ExecuteScalarAsync() is not null;
    }

    private static async Task<bool> ColumnExistsAsync(System.Data.Common.DbConnection connection, string tableName, string columnName)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT 1 FROM pragma_table_info('{tableName}') WHERE name = $columnName LIMIT 1";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "$columnName";
        parameter.Value = columnName;
        command.Parameters.Add(parameter);

        return await command.ExecuteScalarAsync() is not null;
    }
}
