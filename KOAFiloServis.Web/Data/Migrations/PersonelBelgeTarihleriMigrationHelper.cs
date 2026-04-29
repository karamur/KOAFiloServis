using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KOAFiloServis.Web.Data.Migrations;

/// <summary>
/// Personeller tablosuna belge geçerlilik tarihi kolonlarını ekleyen migration helper
/// </summary>
public static class PersonelBelgeTarihleriMigrationHelper
{
    public static async Task ApplyPersonelBelgeTarihleriAsync(ApplicationDbContext context, ILogger logger)
    {
        try
        {
            var connection = context.Database.GetDbConnection();
            await connection.OpenAsync();

            var isPostgres = context.Database.ProviderName?.Contains("Npgsql") == true;

            string[] columns = ["KimlikGecerlilikTarihi", "AdliSicilGecerlilikTarihi", "SuruculCezaBarkodluBelgeTarihi"];

            foreach (var column in columns)
            {
                var checkSql = isPostgres
                    ? $"SELECT COUNT(*) FROM information_schema.columns WHERE table_name = 'Personeller' AND column_name = '{column}'"
                    : $"SELECT COUNT(*) FROM pragma_table_info('Personeller') WHERE name = '{column}'";

                using var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = checkSql;
                var exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0;

                if (!exists)
                {
                    logger.LogInformation("Personeller tablosuna {Column} kolonu ekleniyor...", column);

                    var alterSql = isPostgres
                        ? $"ALTER TABLE \"Personeller\" ADD COLUMN \"{column}\" timestamp without time zone NULL"
                        : $"ALTER TABLE Personeller ADD COLUMN {column} DATETIME NULL";

                    using var alterCmd = connection.CreateCommand();
                    alterCmd.CommandText = alterSql;
                    await alterCmd.ExecuteNonQueryAsync();

                    logger.LogInformation("{Column} kolonu başarıyla eklendi.", column);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "PersonelBelgeTarihleri kolonları eklenirken hata: {Message}", ex.Message);
        }
    }
}
