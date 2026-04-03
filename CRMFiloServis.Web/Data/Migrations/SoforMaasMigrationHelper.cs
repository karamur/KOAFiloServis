using System.Data;
using CRMFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Data.Migrations;

public static class SoforMaasMigrationHelper
{
    public static async Task ApplySoforMaasAlanlariAsync(ApplicationDbContext context)
    {
        try
        {
            if (context.Database.IsNpgsql())
            {
                var sql = @"
                    DO $$
                    BEGIN
                        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Soforler' AND column_name = 'ArgePersoneli') THEN
                            ALTER TABLE ""Soforler"" ADD COLUMN ""ArgePersoneli"" boolean NOT NULL DEFAULT FALSE;
                        END IF;
                        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Soforler' AND column_name = 'TopluMaas') THEN
                            ALTER TABLE ""Soforler"" ADD COLUMN ""TopluMaas"" numeric(18,2) NOT NULL DEFAULT 0;
                        END IF;
                        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Soforler' AND column_name = 'SgkMaasi') THEN
                            ALTER TABLE ""Soforler"" ADD COLUMN ""SgkMaasi"" numeric(18,2) NOT NULL DEFAULT 0;
                        END IF;
                        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Soforler' AND column_name = 'ResmiNetMaas') THEN
                            ALTER TABLE ""Soforler"" ADD COLUMN ""ResmiNetMaas"" numeric(18,2) NOT NULL DEFAULT 0;
                        END IF;
                        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Soforler' AND column_name = 'DigerMaas') THEN
                            ALTER TABLE ""Soforler"" ADD COLUMN ""DigerMaas"" numeric(18,2) NOT NULL DEFAULT 0;
                        END IF;
                        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Soforler' AND column_name = 'SGKBordroDahilMi') THEN
                            ALTER TABLE ""Soforler"" ADD COLUMN ""SGKBordroDahilMi"" boolean NOT NULL DEFAULT FALSE;
                        END IF;
                        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Soforler' AND column_name = 'BordroTipiPersonel') THEN
                            ALTER TABLE ""Soforler"" ADD COLUMN ""BordroTipiPersonel"" integer NOT NULL DEFAULT 0;
                        END IF;
                        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Soforler' AND column_name = 'SgkCikisTarihi') THEN
                            ALTER TABLE ""Soforler"" ADD COLUMN ""SgkCikisTarihi"" timestamp without time zone NULL;
                        END IF;
                    END $$;
                ";

                await context.Database.ExecuteSqlRawAsync(sql);
                await context.Database.ExecuteSqlRawAsync(@"UPDATE ""Soforler"" SET ""ResmiNetMaas"" = ""NetMaas"" WHERE COALESCE(""ResmiNetMaas"", 0) = 0 AND COALESCE(""DigerMaas"", 0) = 0 AND COALESCE(""NetMaas"", 0) > 0");
                // Mevcut ArgePersoneli = true olanları SGKBordroDahilMi = true, BordroTipiPersonel = 2 (Arge) yap
                await context.Database.ExecuteSqlRawAsync(@"UPDATE ""Soforler"" SET ""SGKBordroDahilMi"" = TRUE, ""BordroTipiPersonel"" = 2 WHERE ""ArgePersoneli"" = TRUE AND ""SGKBordroDahilMi"" = FALSE");
                return;
            }

            if (context.Database.IsSqlite())
            {
                await EnsureSqliteColumnAsync(context, "ArgePersoneli", "INTEGER NOT NULL DEFAULT 0");
                await EnsureSqliteColumnAsync(context, "TopluMaas", "TEXT NOT NULL DEFAULT '0'");
                await EnsureSqliteColumnAsync(context, "SgkMaasi", "TEXT NOT NULL DEFAULT '0'");
                await EnsureSqliteColumnAsync(context, "ResmiNetMaas", "TEXT NOT NULL DEFAULT '0'");
                await EnsureSqliteColumnAsync(context, "DigerMaas", "TEXT NOT NULL DEFAULT '0'");
                await EnsureSqliteColumnAsync(context, "SgkCikisTarihi", "TEXT NULL");
                await context.Database.ExecuteSqlRawAsync(@"UPDATE ""Soforler"" SET ""ResmiNetMaas"" = ""NetMaas"" WHERE IFNULL(""ResmiNetMaas"", '0') = '0' AND IFNULL(""DigerMaas"", '0') = '0' AND IFNULL(""NetMaas"", '0') <> '0'");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Şoför maaş alanları migration hatası: {ex.Message}");
        }
    }

    private static async Task EnsureSqliteColumnAsync(ApplicationDbContext context, string columnName, string columnDefinition)
    {
        await using var connection = context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = "SELECT 1 FROM pragma_table_info('Soforler') WHERE name = $columnName LIMIT 1";

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
        alterCommand.CommandText = $"ALTER TABLE \"Soforler\" ADD COLUMN \"{columnName}\" {columnDefinition}";
        await alterCommand.ExecuteNonQueryAsync();
    }
}
