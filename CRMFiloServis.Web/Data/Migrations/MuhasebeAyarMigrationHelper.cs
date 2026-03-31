using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Data.Migrations;

public static class MuhasebeAyarMigrationHelper
{
    public static async Task ApplyStokMasrafAyarlariAsync(ApplicationDbContext context)
    {
        var sql = @"
-- MuhasebeAyarlari tablosuna yeni sütunlar ekle
DO $$
BEGIN
    -- MalMasrafHesabi
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'MuhasebeAyarlari' AND column_name = 'MalMasrafHesabi') THEN
        ALTER TABLE ""MuhasebeAyarlari"" ADD COLUMN ""MalMasrafHesabi"" VARCHAR(50) DEFAULT '740.99.001';
    END IF;

    -- SarfMalzemeMasrafHesabi
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'MuhasebeAyarlari' AND column_name = 'SarfMalzemeMasrafHesabi') THEN
        ALTER TABLE ""MuhasebeAyarlari"" ADD COLUMN ""SarfMalzemeMasrafHesabi"" VARCHAR(50) DEFAULT '740.99.002';
    END IF;

    -- StokCikisHesabi
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'MuhasebeAyarlari' AND column_name = 'StokCikisHesabi') THEN
        ALTER TABLE ""MuhasebeAyarlari"" ADD COLUMN ""StokCikisHesabi"" VARCHAR(50) DEFAULT '153';
    END IF;

    -- StokMasrafAktarimiOtomatik
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'MuhasebeAyarlari' AND column_name = 'StokMasrafAktarimiOtomatik') THEN
        ALTER TABLE ""MuhasebeAyarlari"" ADD COLUMN ""StokMasrafAktarimiOtomatik"" BOOLEAN DEFAULT TRUE;
    END IF;
END $$;
";

        try
        {
            await context.Database.ExecuteSqlRawAsync(sql);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Muhasebe Ayar migration hatası: {ex.Message}");
        }
    }
}
