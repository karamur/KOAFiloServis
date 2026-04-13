-- Tüm bekleyen migration'larý geçmiþe ekle
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES 
    ('20260324174737_InitialCreate', '10.0.5'),
    ('20260324175248_Init', '10.0.5'),
    ('20260324195453_AddPiyasaArastirmaModule', '10.0.5'),
    ('20260325200834_AddResimUrlToPiyasaIlan', '10.0.5'),
    ('20260326090740_TekrarlayanOdeme', '10.0.5')
ON CONFLICT DO NOTHING;
