using CRMFiloServis.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Firma
        if (!await context.Firmalar.AnyAsync())
        {
            var firma = new Firma
            {
                FirmaAdi = "Ana Firma",
                VergiDairesi = "Merkez VD",
                VergiNo = "1234567890",
                Adres = "Ýstanbul, Türkiye",
                Telefon = "+90 212 000 00 00",
                Email = "info@firma.com",
                VarsayilanFirma = true,
                Aktif = true,
                AktifDonemYil = DateTime.Today.Year,
                AktifDonemAy = DateTime.Today.Month,
                CreatedAt = DateTime.UtcNow
            };
            context.Firmalar.Add(firma);
            await context.SaveChangesAsync();
        }

        // Roller
        if (!await context.Roller.AnyAsync())
        {
            var roller = new List<Rol>
            {
                new Rol { RolAdi = "Admin", Aciklama = "Sistem yöneticisi", CreatedAt = DateTime.UtcNow },
                new Rol { RolAdi = "Muhasebe", Aciklama = "Muhasebe personeli", CreatedAt = DateTime.UtcNow },
                new Rol { RolAdi = "Kullanici", Aciklama = "Standart kullanýcý", CreatedAt = DateTime.UtcNow }
            };
            context.Roller.AddRange(roller);
            await context.SaveChangesAsync();
        }

        // Kullanici (Admin)
        if (!await context.Kullanicilar.AnyAsync())
        {
            var adminRol = await context.Roller.FirstAsync(r => r.RolAdi == "Admin");
            var admin = new Kullanici
            {
                KullaniciAdi = "admin",
                SifreHash = "admin123", // Production'da düzgün hash'lenmiþ olmalý
                AdSoyad = "Sistem Yöneticisi",
                Email = "admin@firma.com",
                RolId = adminRol.Id,
                Aktif = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Kullanicilar.Add(admin);
            await context.SaveChangesAsync();
        }

        // Muhasebe Hesap Planý (Tek Düzen Hesap Planý)
        if (!await context.MuhasebeHesaplari.AnyAsync())
        {
            var hesaplar = new List<MuhasebeHesap>
            {
                // 1XX - DÖNEN VARLIKLAR
                new MuhasebeHesap { HesapKodu = "100", HesapAdi = "Kasa", HesapTuru = HesapTuru.Aktif, HesapGrubu = HesapGrubu.DonenVarliklar, AltHesapVar = true, Aktif = true, CreatedAt = DateTime.UtcNow },
                new MuhasebeHesap { HesapKodu = "100.01", HesapAdi = "TL Kasa", HesapTuru = HesapTuru.Aktif, HesapGrubu = HesapGrubu.DonenVarliklar, Aktif = true, CreatedAt = DateTime.UtcNow },
                new MuhasebeHesap { HesapKodu = "100.02", HesapAdi = "Döviz Kasa", HesapTuru = HesapTuru.Aktif, HesapGrubu = HesapGrubu.DonenVarliklar, Aktif = true, CreatedAt = DateTime.UtcNow },

                new MuhasebeHesap { HesapKodu = "102", HesapAdi = "Bankalar", HesapTuru = HesapTuru.Aktif, HesapGrubu = HesapGrubu.DonenVarliklar, AltHesapVar = true, Aktif = true, CreatedAt = DateTime.UtcNow },
                new MuhasebeHesap { HesapKodu = "102.01", HesapAdi = "TL Banka", HesapTuru = HesapTuru.Aktif, HesapGrubu = HesapGrubu.DonenVarliklar, Aktif = true, CreatedAt = DateTime.UtcNow },

                new MuhasebeHesap { HesapKodu = "120", HesapAdi = "Alýcýlar", HesapTuru = HesapTuru.Aktif, HesapGrubu = HesapGrubu.DonenVarliklar, AltHesapVar = true, Aktif = true, CreatedAt = DateTime.UtcNow },
                new MuhasebeHesap { HesapKodu = "120.01", HesapAdi = "Müþteriler", HesapTuru = HesapTuru.Aktif, HesapGrubu = HesapGrubu.DonenVarliklar, AltHesapVar = true, Aktif = true, CreatedAt = DateTime.UtcNow },

                new MuhasebeHesap { HesapKodu = "153", HesapAdi = "Ticari Mallar", HesapTuru = HesapTuru.Aktif, HesapGrubu = HesapGrubu.DonenVarliklar, Aktif = true, CreatedAt = DateTime.UtcNow },

                // 2XX - DURAN VARLIKLAR
                new MuhasebeHesap { HesapKodu = "253", HesapAdi = "Taþýtlar", HesapTuru = HesapTuru.Aktif, HesapGrubu = HesapGrubu.DuranVarliklar, AltHesapVar = true, Aktif = true, CreatedAt = DateTime.UtcNow },
                new MuhasebeHesap { HesapKodu = "253.01", HesapAdi = "Araçlar", HesapTuru = HesapTuru.Aktif, HesapGrubu = HesapGrubu.DuranVarliklar, Aktif = true, CreatedAt = DateTime.UtcNow },

                new MuhasebeHesap { HesapKodu = "257", HesapAdi = "Birikmiþ Amortismanlar", HesapTuru = HesapTuru.Aktif, HesapGrubu = HesapGrubu.DuranVarliklar, AltHesapVar = true, Aktif = true, CreatedAt = DateTime.UtcNow },

                // 3XX - KISA VADELÝ YABANCI KAYNAKLAR
                new MuhasebeHesap { HesapKodu = "320", HesapAdi = "Satýcýlar", HesapTuru = HesapTuru.Pasif, HesapGrubu = HesapGrubu.KisaVadeliYabanciKaynaklar, AltHesapVar = true, Aktif = true, CreatedAt = DateTime.UtcNow },
                new MuhasebeHesap { HesapKodu = "320.01", HesapAdi = "Tedarikçiler", HesapTuru = HesapTuru.Pasif, HesapGrubu = HesapGrubu.KisaVadeliYabanciKaynaklar, AltHesapVar = true, Aktif = true, CreatedAt = DateTime.UtcNow },

                new MuhasebeHesap { HesapKodu = "360", HesapAdi = "Ödenecek Vergi ve Fonlar", HesapTuru = HesapTuru.Pasif, HesapGrubu = HesapGrubu.KisaVadeliYabanciKaynaklar, AltHesapVar = true, Aktif = true, CreatedAt = DateTime.UtcNow },
                new MuhasebeHesap { HesapKodu = "360.01", HesapAdi = "KDV Borcu", HesapTuru = HesapTuru.Pasif, HesapGrubu = HesapGrubu.KisaVadeliYabanciKaynaklar, Aktif = true, CreatedAt = DateTime.UtcNow },

                new MuhasebeHesap { HesapKodu = "361", HesapAdi = "Ödenecek Sosyal Güv. Kes.", HesapTuru = HesapTuru.Pasif, HesapGrubu = HesapGrubu.KisaVadeliYabanciKaynaklar, Aktif = true, CreatedAt = DateTime.UtcNow },

                new MuhasebeHesap { HesapKodu = "370", HesapAdi = "Dönem Karý Vergi ve Diðer Yasal Yük. Karþýlýðý", HesapTuru = HesapTuru.Pasif, HesapGrubu = HesapGrubu.KisaVadeliYabanciKaynaklar, Aktif = true, CreatedAt = DateTime.UtcNow },

                // 5XX - ÖZKAYNAK
                new MuhasebeHesap { HesapKodu = "500", HesapAdi = "Sermaye", HesapTuru = HesapTuru.Pasif, HesapGrubu = HesapGrubu.Ozkaynaklar, Aktif = true, CreatedAt = DateTime.UtcNow },

                new MuhasebeHesap { HesapKodu = "590", HesapAdi = "Dönem Net Karý/Zararý", HesapTuru = HesapTuru.Pasif, HesapGrubu = HesapGrubu.Ozkaynaklar, Aktif = true, CreatedAt = DateTime.UtcNow },

                // 6XX - GELÝR HESAPLARI
                new MuhasebeHesap { HesapKodu = "600", HesapAdi = "Yurt Ýçi Satýþlar", HesapTuru = HesapTuru.Gelir, HesapGrubu = HesapGrubu.GelirTablosu, AltHesapVar = true, Aktif = true, CreatedAt = DateTime.UtcNow },
                new MuhasebeHesap { HesapKodu = "600.01", HesapAdi = "Servis Gelirleri", HesapTuru = HesapTuru.Gelir, HesapGrubu = HesapGrubu.GelirTablosu, Aktif = true, CreatedAt = DateTime.UtcNow },

                new MuhasebeHesap { HesapKodu = "602", HesapAdi = "Diðer Gelirler", HesapTuru = HesapTuru.Gelir, HesapGrubu = HesapGrubu.GelirTablosu, Aktif = true, CreatedAt = DateTime.UtcNow },

                // 7XX - GÝDER HESAPLARI
                new MuhasebeHesap { HesapKodu = "710", HesapAdi = "Direkt Ýlk Madde ve Malzeme Giderleri", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari, Aktif = true, CreatedAt = DateTime.UtcNow },

                new MuhasebeHesap { HesapKodu = "720", HesapAdi = "Direkt Ýþçilik Giderleri", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari, AltHesapVar = true, Aktif = true, CreatedAt = DateTime.UtcNow },
                new MuhasebeHesap { HesapKodu = "720.01", HesapAdi = "Þoför Maaþlarý", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari, Aktif = true, CreatedAt = DateTime.UtcNow },

                new MuhasebeHesap { HesapKodu = "730", HesapAdi = "Genel Üretim Giderleri", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari, AltHesapVar = true, Aktif = true, CreatedAt = DateTime.UtcNow },
                new MuhasebeHesap { HesapKodu = "730.01", HesapAdi = "Yakýt Giderleri", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari, Aktif = true, CreatedAt = DateTime.UtcNow },
                new MuhasebeHesap { HesapKodu = "730.02", HesapAdi = "Araç Bakým Onarým", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari, Aktif = true, CreatedAt = DateTime.UtcNow },
                new MuhasebeHesap { HesapKodu = "730.03", HesapAdi = "Sigorta Giderleri", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari, Aktif = true, CreatedAt = DateTime.UtcNow },

                new MuhasebeHesap { HesapKodu = "750", HesapAdi = "Araþtýrma ve Geliþtirme Giderleri", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari, Aktif = true, CreatedAt = DateTime.UtcNow },

                new MuhasebeHesap { HesapKodu = "760", HesapAdi = "Pazarlama Satýþ ve Daðýtým Giderleri", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari, AltHesapVar = true, Aktif = true, CreatedAt = DateTime.UtcNow },
                new MuhasebeHesap { HesapKodu = "760.01", HesapAdi = "Reklam Giderleri", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari, Aktif = true, CreatedAt = DateTime.UtcNow },

                new MuhasebeHesap { HesapKodu = "770", HesapAdi = "Genel Yönetim Giderleri", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari, AltHesapVar = true, Aktif = true, CreatedAt = DateTime.UtcNow },
                new MuhasebeHesap { HesapKodu = "770.01", HesapAdi = "Kira Giderleri", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari, Aktif = true, CreatedAt = DateTime.UtcNow },
                new MuhasebeHesap { HesapKodu = "770.02", HesapAdi = "Elektrik Su Giderleri", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari, Aktif = true, CreatedAt = DateTime.UtcNow },
                new MuhasebeHesap { HesapKodu = "770.03", HesapAdi = "Haberleþme Giderleri", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari, Aktif = true, CreatedAt = DateTime.UtcNow },
                new MuhasebeHesap { HesapKodu = "770.04", HesapAdi = "Kýrtasiye Giderleri", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari, Aktif = true, CreatedAt = DateTime.UtcNow },

                new MuhasebeHesap { HesapKodu = "780", HesapAdi = "Finansman Giderleri", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari, AltHesapVar = true, Aktif = true, CreatedAt = DateTime.UtcNow },
                new MuhasebeHesap { HesapKodu = "780.01", HesapAdi = "Kredi Faiz Giderleri", HesapTuru = HesapTuru.Gider, HesapGrubu = HesapGrubu.MaliyetHesaplari, Aktif = true, CreatedAt = DateTime.UtcNow }
            };

            context.MuhasebeHesaplari.AddRange(hesaplar);
            await context.SaveChangesAsync();

            // Parent-child iliþkilerini güncelle
            foreach (var hesap in hesaplar.Where(h => h.HesapKodu.Contains(".")))
            {
                var parentKod = hesap.HesapKodu.Substring(0, hesap.HesapKodu.LastIndexOf('.'));
                var parent = await context.MuhasebeHesaplari.FirstOrDefaultAsync(h => h.HesapKodu == parentKod);
                if (parent != null)
                {
                    hesap.UstHesapId = parent.Id;
                }
            }
            await context.SaveChangesAsync();
        }

        // Bütçe Masraf Kalemleri
        if (!await context.BudgetMasrafKalemleri.AnyAsync())
        {
            var masrafKalemleri = new List<BudgetMasrafKalemi>
            {
                new BudgetMasrafKalemi { KalemAdi = "Yakýt", Aktif = true, CreatedAt = DateTime.UtcNow },
                new BudgetMasrafKalemi { KalemAdi = "Araç Bakým/Onarým", Aktif = true, CreatedAt = DateTime.UtcNow },
                new BudgetMasrafKalemi { KalemAdi = "Araç Sigorta", Aktif = true, CreatedAt = DateTime.UtcNow },
                new BudgetMasrafKalemi { KalemAdi = "MTV", Aktif = true, CreatedAt = DateTime.UtcNow },
                new BudgetMasrafKalemi { KalemAdi = "Þoför Maaþlarý", Aktif = true, CreatedAt = DateTime.UtcNow },
                new BudgetMasrafKalemi { KalemAdi = "Kira", Aktif = true, CreatedAt = DateTime.UtcNow },
                new BudgetMasrafKalemi { KalemAdi = "Elektrik", Aktif = true, CreatedAt = DateTime.UtcNow },
                new BudgetMasrafKalemi { KalemAdi = "Su", Aktif = true, CreatedAt = DateTime.UtcNow },
                new BudgetMasrafKalemi { KalemAdi = "Doðalgaz", Aktif = true, CreatedAt = DateTime.UtcNow },
                new BudgetMasrafKalemi { KalemAdi = "Ýnternet", Aktif = true, CreatedAt = DateTime.UtcNow },
                new BudgetMasrafKalemi { KalemAdi = "Telefon", Aktif = true, CreatedAt = DateTime.UtcNow },
                new BudgetMasrafKalemi { KalemAdi = "Kýrtasiye", Aktif = true, CreatedAt = DateTime.UtcNow },
                new BudgetMasrafKalemi { KalemAdi = "Temizlik", Aktif = true, CreatedAt = DateTime.UtcNow },
                new BudgetMasrafKalemi { KalemAdi = "Muhasebe/Danýþmanlýk", Aktif = true, CreatedAt = DateTime.UtcNow },
                new BudgetMasrafKalemi { KalemAdi = "Reklam/Pazarlama", Aktif = true, CreatedAt = DateTime.UtcNow },
                new BudgetMasrafKalemi { KalemAdi = "Kredi Taksiti", Aktif = true, CreatedAt = DateTime.UtcNow },
                new BudgetMasrafKalemi { KalemAdi = "Vergi/SGK Ödemeleri", Aktif = true, CreatedAt = DateTime.UtcNow },
                new BudgetMasrafKalemi { KalemAdi = "Diðer", Aktif = true, CreatedAt = DateTime.UtcNow }
            };

            context.BudgetMasrafKalemleri.AddRange(masrafKalemleri);
            await context.SaveChangesAsync();
        }

        // Araç Markalarý
        if (!await context.AracMarkalari.AnyAsync())
        {
            var markalar = new List<AracMarka>
            {
                new AracMarka { MarkaAdi = "Mercedes-Benz", Aktif = true, CreatedAt = DateTime.UtcNow },
                new AracMarka { MarkaAdi = "Ford", Aktif = true, CreatedAt = DateTime.UtcNow },
                new AracMarka { MarkaAdi = "Volkswagen", Aktif = true, CreatedAt = DateTime.UtcNow },
                new AracMarka { MarkaAdi = "Fiat", Aktif = true, CreatedAt = DateTime.UtcNow },
                new AracMarka { MarkaAdi = "Hyundai", Aktif = true, CreatedAt = DateTime.UtcNow },
                new AracMarka { MarkaAdi = "Iveco", Aktif = true, CreatedAt = DateTime.UtcNow },
                new AracMarka { MarkaAdi = "BMC", Aktif = true, CreatedAt = DateTime.UtcNow },
                new AracMarka { MarkaAdi = "Otokar", Aktif = true, CreatedAt = DateTime.UtcNow }
            };

            context.AracMarkalari.AddRange(markalar);
            await context.SaveChangesAsync();
        }

        // Araç Modelleri
        if (!await context.AracModelleri.AnyAsync())
        {
            var mercedes = await context.AracMarkalari.FirstAsync(m => m.MarkaAdi == "Mercedes-Benz");
            var ford = await context.AracMarkalari.FirstAsync(m => m.MarkaAdi == "Ford");
            var vw = await context.AracMarkalari.FirstAsync(m => m.MarkaAdi == "Volkswagen");

            var modeller = new List<AracModelTanim>
            {
                new AracModelTanim { MarkaId = mercedes.Id, ModelAdi = "Sprinter", Aktif = true, CreatedAt = DateTime.UtcNow },
                new AracModelTanim { MarkaId = mercedes.Id, ModelAdi = "Tourismo", Aktif = true, CreatedAt = DateTime.UtcNow },
                new AracModelTanim { MarkaId = ford.Id, ModelAdi = "Transit", Aktif = true, CreatedAt = DateTime.UtcNow },
                new AracModelTanim { MarkaId = ford.Id, ModelAdi = "Transit Custom", Aktif = true, CreatedAt = DateTime.UtcNow },
                new AracModelTanim { MarkaId = vw.Id, ModelAdi = "Crafter", Aktif = true, CreatedAt = DateTime.UtcNow },
                new AracModelTanim { MarkaId = vw.Id, ModelAdi = "Caravelle", Aktif = true, CreatedAt = DateTime.UtcNow }
            };

            context.AracModelleri.AddRange(modeller);
            await context.SaveChangesAsync();
        }

        // Banka Hesaplarý
        if (!await context.BankaHesaplari.AnyAsync())
        {
            var firma = await context.Firmalar.FirstAsync();
            
            var hesaplar = new List<BankaHesap>
            {
                new BankaHesap 
                { 
                    HesapKodu = "KASA01",
                    HesapAdi = "Nakit Kasa", 
                    HesapTipi = HesapTipi.Kasa,
                    AcilisBakiye = 0,
                    Aktif = true,
                    CreatedAt = DateTime.UtcNow 
                },
                new BankaHesap 
                { 
                    HesapKodu = "BANKA01",
                    HesapAdi = "Ziraat Bankasý Ticari Hesap", 
                    HesapTipi = HesapTipi.VadesizHesap,
                    BankaAdi = "Ziraat Bankasý",
                    SubeKodu = "001",
                    HesapNo = "1234567890",
                    Iban = "TR000000000000000000000000",
                    AcilisBakiye = 0,
                    Aktif = true,
                    CreatedAt = DateTime.UtcNow 
                }
            };
            
            context.BankaHesaplari.AddRange(hesaplar);
            await context.SaveChangesAsync();
        }

        // Gelen Faturalarý E-Fatura olarak güncelle
        await UpdateGelenFaturalarToEFaturaAsync(context);

        Console.WriteLine("? Seed verileri baþarýyla eklendi!");
    }

    /// <summary>
    /// Tüm gelen faturalarý E-Fatura olarak günceller
    /// </summary>
    public static async Task UpdateGelenFaturalarToEFaturaAsync(ApplicationDbContext context)
    {
        var gelenFaturalar = await context.Faturalar
            .Where(f => f.FaturaYonu == FaturaYonu.Gelen && f.EFaturaTipi != EFaturaTipi.EFatura)
            .ToListAsync();

        if (gelenFaturalar.Any())
        {
            foreach (var fatura in gelenFaturalar)
            {
                fatura.EFaturaTipi = EFaturaTipi.EFatura;
                fatura.UpdatedAt = DateTime.Now;
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"? {gelenFaturalar.Count} adet gelen fatura E-Fatura olarak güncellendi!");
        }
    }
}
