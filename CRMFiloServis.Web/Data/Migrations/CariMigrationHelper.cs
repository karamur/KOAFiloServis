using Microsoft.EntityFrameworkCore;
using CRMFiloServis.Web.Data;

namespace CRMFiloServis.Web.Data.Migrations;

public static class CariMigrationHelper
{
    public static async Task ApplyCariAlanGenisletmeAsync(ApplicationDbContext context)
    {
        var sql = @"
            DO $$ 
            BEGIN 
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Cariler' AND column_name = 'Il') THEN
                    ALTER TABLE ""Cariler"" ADD COLUMN ""Il"" TEXT NULL;
                END IF;
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Cariler' AND column_name = 'Ilce') THEN
                    ALTER TABLE ""Cariler"" ADD COLUMN ""Ilce"" TEXT NULL;
                END IF;
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Cariler' AND column_name = 'PostaKodu') THEN
                    ALTER TABLE ""Cariler"" ADD COLUMN ""PostaKodu"" TEXT NULL;
                END IF;
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Cariler' AND column_name = 'Telefon2') THEN
                    ALTER TABLE ""Cariler"" ADD COLUMN ""Telefon2"" TEXT NULL;
                END IF;
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Cariler' AND column_name = 'Fax') THEN
                    ALTER TABLE ""Cariler"" ADD COLUMN ""Fax"" TEXT NULL;
                END IF;
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Cariler' AND column_name = 'WebSitesi') THEN
                    ALTER TABLE ""Cariler"" ADD COLUMN ""WebSitesi"" TEXT NULL;
                END IF;
            END $$;
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
    }
}
