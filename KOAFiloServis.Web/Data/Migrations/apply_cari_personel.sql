-- Cari tablosuna SoforId kolonu ekle (zaten eklenmiþ olabilir)
ALTER TABLE "Cariler" ADD COLUMN IF NOT EXISTS "SoforId" integer;

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

-- Migration geçmiþini güncelle
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260326214329_CariPersonelSoforIliski', '10.0.5')
ON CONFLICT DO NOTHING;
