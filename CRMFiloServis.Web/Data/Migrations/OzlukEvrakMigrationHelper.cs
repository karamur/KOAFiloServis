using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Data.Migrations;

public static class OzlukEvrakMigrationHelper
{
    public static async Task ApplyOzlukEvrakMigrationAsync(ApplicationDbContext context)
    {
        var sql = @"
-- Evrak Tanımları Tablosu
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

-- Personel Özlük Evrakları Tablosu
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
    CONSTRAINT ""FK_PersonelOzlukEvraklar_Soforler"" FOREIGN KEY (""SoforId"") REFERENCES ""Soforler"" (""Id"") ON DELETE CASCADE,
    CONSTRAINT ""FK_PersonelOzlukEvraklar_OzlukEvrakTanimlari"" FOREIGN KEY (""EvrakTanimId"") REFERENCES ""OzlukEvrakTanimlari"" (""Id"") ON DELETE CASCADE
);
";

        try
        {
            await context.Database.ExecuteSqlRawAsync(sql);

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
}
