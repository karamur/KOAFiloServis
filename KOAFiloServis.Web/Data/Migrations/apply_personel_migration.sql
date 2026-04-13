-- Cari tablosuna SoforId kolonu ekle
ALTER TABLE "Cariler" ADD COLUMN IF NOT EXISTS "SoforId" integer NULL;

-- SoforId için index oluþtur
CREATE INDEX IF NOT EXISTS "IX_Cariler_SoforId" ON "Cariler" ("SoforId");

-- Foreign key ekle
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Cariler_Soforler_SoforId') THEN
        ALTER TABLE "Cariler" ADD CONSTRAINT "FK_Cariler_Soforler_SoforId" 
        FOREIGN KEY ("SoforId") REFERENCES "Soforler" ("Id") ON DELETE SET NULL;
    END IF;
END $$;

-- KullaniciCariler tablosundaki unique index'i normal index'e çevir
DROP INDEX IF EXISTS "IX_KullaniciCariler_KullaniciId_CariId";
CREATE INDEX IF NOT EXISTS "IX_KullaniciCariler_KullaniciId_CariId" ON "KullaniciCariler" ("KullaniciId", "CariId");

-- Migration geçmiþini güncelle
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
SELECT '20260326214329_CariPersonelSoforIliski', '10.0.5'
WHERE NOT EXISTS (
    SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260326214329_CariPersonelSoforIliski'
);
