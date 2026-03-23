using CRMFiloServis.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Firma Modulu
    public DbSet<Firma> Firmalar { get; set; }

    // Cari Modulu
    public DbSet<Cari> Cariler { get; set; }

    // Filo Servis Modulu
    public DbSet<Sofor> Soforler { get; set; }
    public DbSet<Arac> Araclar { get; set; }
    public DbSet<Guzergah> Guzergahlar { get; set; }
    public DbSet<MasrafKalemi> MasrafKalemleri { get; set; }
    public DbSet<AracMasraf> AracMasraflari { get; set; }
    public DbSet<ServisCalisma> ServisCalismalari { get; set; }

    // Fatura Modulu
    public DbSet<Fatura> Faturalar { get; set; }
    public DbSet<FaturaKalem> FaturaKalemleri { get; set; }

    // Banka/Kasa Modulu
    public DbSet<BankaHesap> BankaHesaplari { get; set; }
    public DbSet<BankaKasaHareket> BankaKasaHareketleri { get; set; }
    public DbSet<OdemeEslestirme> OdemeEslestirmeleri { get; set; }

    // Checklist Modulu
    public DbSet<AylikChecklist> AylikChecklistler { get; set; }
    public DbSet<ChecklistKalem> ChecklistKalemleri { get; set; }

    // Personel Maas/Izin Modulu
    public DbSet<PersonelMaas> PersonelMaaslari { get; set; }
    public DbSet<PersonelIzin> PersonelIzinleri { get; set; }
    public DbSet<PersonelIzinHakki> PersonelIzinHaklari { get; set; }

    // Butce Modulu
    public DbSet<BudgetOdeme> BudgetOdemeler { get; set; }
    public DbSet<BudgetMasrafKalemi> BudgetMasrafKalemleri { get; set; }

    // Muhasebe Modulu
    public DbSet<MuhasebeHesap> MuhasebeHesaplari { get; set; }
    public DbSet<MuhasebeFis> MuhasebeFisleri { get; set; }
    public DbSet<MuhasebeFisKalem> MuhasebeFisKalemleri { get; set; }
    public DbSet<MuhasebeDonem> MuhasebeDonemleri { get; set; }

    // Kullanici ve Lisans Modulu
    public DbSet<Lisans> Lisanslar { get; set; }
    public DbSet<Kullanici> Kullanicilar { get; set; }
    public DbSet<Rol> Roller { get; set; }
    public DbSet<RolYetki> RolYetkileri { get; set; }

    // Satis Modulu
    public DbSet<SatisPersoneli> SatisPersonelleri { get; set; }
    public DbSet<AracIlan> AracIlanlari { get; set; }
    public DbSet<PiyasaIlan> PiyasaIlanlari { get; set; }
    public DbSet<AracSatis> AracSatislari { get; set; }
    public DbSet<AracMarka> AracMarkalari { get; set; }
    public DbSet<AracModelTanim> AracModelleri { get; set; }

    // Sistem Modulu
    public DbSet<AktiviteLog> AktiviteLoglar { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Firma
        modelBuilder.Entity<Firma>(entity =>
        {
            entity.HasIndex(e => e.FirmaKodu).IsUnique();
            entity.Property(e => e.FirmaKodu).HasMaxLength(50);
            entity.Property(e => e.FirmaAdi).HasMaxLength(250);
            entity.Property(e => e.VergiNo).HasMaxLength(11);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Cari
        modelBuilder.Entity<Cari>(entity =>
        {
            entity.HasIndex(e => e.CariKodu).IsUnique();
            entity.Property(e => e.CariKodu).HasMaxLength(50);
            entity.Property(e => e.Unvan).HasMaxLength(250);
            entity.Property(e => e.VergiNo).HasMaxLength(20);
            entity.Property(e => e.Telefon).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Þoför
        modelBuilder.Entity<Sofor>(entity =>
        {
            entity.HasIndex(e => e.SoforKodu).IsUnique();
            entity.Property(e => e.SoforKodu).HasMaxLength(50);
            entity.Property(e => e.Ad).HasMaxLength(100);
            entity.Property(e => e.Soyad).HasMaxLength(100);
            entity.Property(e => e.TcKimlikNo).HasMaxLength(11);
            entity.Property(e => e.Telefon).HasMaxLength(20);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Araç
        modelBuilder.Entity<Arac>(entity =>
        {
            entity.HasIndex(e => e.Plaka).IsUnique();
            entity.Property(e => e.Plaka).HasMaxLength(15);
            entity.Property(e => e.Marka).HasMaxLength(50);
            entity.Property(e => e.Model).HasMaxLength(50);
            entity.Property(e => e.GunlukKiraBedeli).HasPrecision(18, 2);
            entity.Property(e => e.AylikKiraBedeli).HasPrecision(18, 2);
            entity.Property(e => e.SeferBasinaKiraBedeli).HasPrecision(18, 2);
            entity.Property(e => e.KomisyonOrani).HasPrecision(5, 2);
            entity.Property(e => e.SabitKomisyonTutari).HasPrecision(18, 2);
            
            entity.HasOne(e => e.KiralikCari)
                .WithMany()
                .HasForeignKey(e => e.KiralikCariId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.KomisyoncuCari)
                .WithMany()
                .HasForeignKey(e => e.KomisyoncuCariId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Güzergah
        modelBuilder.Entity<Guzergah>(entity =>
        {
            entity.HasIndex(e => e.GuzergahKodu).IsUnique();
            entity.Property(e => e.GuzergahKodu).HasMaxLength(50);
            entity.Property(e => e.GuzergahAdi).HasMaxLength(200);
            entity.Property(e => e.BirimFiyat).HasPrecision(18, 2);
            entity.Property(e => e.Mesafe).HasPrecision(10, 2);
            entity.HasOne(e => e.Cari)
                .WithMany(c => c.Guzergahlar)
                .HasForeignKey(e => e.CariId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Masraf Kalemi
        modelBuilder.Entity<MasrafKalemi>(entity =>
        {
            entity.HasIndex(e => e.MasrafKodu).IsUnique();
            entity.Property(e => e.MasrafKodu).HasMaxLength(50);
            entity.Property(e => e.MasrafAdi).HasMaxLength(200);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Araç Masraf
        modelBuilder.Entity<AracMasraf>(entity =>
        {
            entity.Property(e => e.Tutar).HasPrecision(18, 2);
            entity.HasOne(e => e.Arac)
                .WithMany(a => a.Masraflar)
                .HasForeignKey(e => e.AracId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.MasrafKalemi)
                .WithMany(m => m.AracMasraflari)
                .HasForeignKey(e => e.MasrafKalemiId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Guzergah)
                .WithMany(g => g.AracMasraflari)
                .HasForeignKey(e => e.GuzergahId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.ServisCalisma)
                .WithMany(s => s.ArizaMasraflari)
                .HasForeignKey(e => e.ServisCalismaId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Servis Çalýþma
        modelBuilder.Entity<ServisCalisma>(entity =>
        {
            entity.Property(e => e.Fiyat).HasPrecision(18, 2);
            entity.HasOne(e => e.Arac)
                .WithMany(a => a.ServisCalismalari)
                .HasForeignKey(e => e.AracId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Sofor)
                .WithMany(s => s.ServisCalismalari)
                .HasForeignKey(e => e.SoforId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Guzergah)
                .WithMany(g => g.ServisCalismalari)
                .HasForeignKey(e => e.GuzergahId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Fatura
        modelBuilder.Entity<Fatura>(entity =>
        {
            entity.HasIndex(e => e.FaturaNo).IsUnique();
            entity.Property(e => e.FaturaNo).HasMaxLength(50);
            entity.Property(e => e.AraToplam).HasPrecision(18, 2);
            entity.Property(e => e.KdvOrani).HasPrecision(5, 2);
            entity.Property(e => e.KdvTutar).HasPrecision(18, 2);
            entity.Property(e => e.GenelToplam).HasPrecision(18, 2);
            entity.Property(e => e.OdenenTutar).HasPrecision(18, 2);
            entity.HasOne(e => e.Cari)
                .WithMany(c => c.Faturalar)
                .HasForeignKey(e => e.CariId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Fatura Kalem
        modelBuilder.Entity<FaturaKalem>(entity =>
        {
            entity.Property(e => e.BirimFiyat).HasPrecision(18, 2);
            entity.Property(e => e.KdvOrani).HasPrecision(5, 2);
            entity.Property(e => e.KdvTutar).HasPrecision(18, 2);
            entity.Property(e => e.ToplamTutar).HasPrecision(18, 2);
            entity.Property(e => e.Miktar).HasPrecision(18, 4);
            entity.Property(e => e.Birim).HasMaxLength(20);
            entity.HasOne(e => e.Fatura)
                .WithMany(f => f.FaturaKalemleri)
                .HasForeignKey(e => e.FaturaId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Banka Hesap
        modelBuilder.Entity<BankaHesap>(entity =>
        {
            entity.HasIndex(e => e.HesapKodu).IsUnique();
            entity.Property(e => e.HesapKodu).HasMaxLength(50);
            entity.Property(e => e.HesapAdi).HasMaxLength(200);
            entity.Property(e => e.BankaAdi).HasMaxLength(100);
            entity.Property(e => e.Iban).HasMaxLength(34);
            entity.Property(e => e.ParaBirimi).HasMaxLength(3);
            entity.Property(e => e.AcilisBakiye).HasPrecision(18, 2);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Banka/Kasa Hareket
        modelBuilder.Entity<BankaKasaHareket>(entity =>
        {
            entity.HasIndex(e => e.IslemNo).IsUnique();
            entity.Property(e => e.IslemNo).HasMaxLength(50);
            entity.Property(e => e.Tutar).HasPrecision(18, 2);
            entity.HasOne(e => e.BankaHesap)
                .WithMany(b => b.Hareketler)
                .HasForeignKey(e => e.BankaHesapId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Cari)
                .WithMany(c => c.BankaKasaHareketler)
                .HasForeignKey(e => e.CariId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Ödeme Eþleþtirme
        modelBuilder.Entity<OdemeEslestirme>(entity =>
        {
            entity.Property(e => e.EslestirilenTutar).HasPrecision(18, 2);
            entity.HasOne(e => e.Fatura)
                .WithMany(f => f.OdemeEslestirmeleri)
                .HasForeignKey(e => e.FaturaId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.BankaKasaHareket)
                .WithMany(b => b.OdemeEslestirmeleri)
                .HasForeignKey(e => e.BankaKasaHareketId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // AylikChecklist
        modelBuilder.Entity<AylikChecklist>(entity =>
        {
            entity.HasIndex(e => new { e.Yil, e.Ay, e.ChecklistTipi, e.SoforId, e.AracId, e.GuzergahId });
            entity.Property(e => e.KontrolEden).HasMaxLength(100);
            
            entity.HasOne(e => e.Sofor)
                .WithMany()
                .HasForeignKey(e => e.SoforId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Arac)
                .WithMany()
                .HasForeignKey(e => e.AracId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Guzergah)
                .WithMany()
                .HasForeignKey(e => e.GuzergahId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Checklist Kalem
        modelBuilder.Entity<ChecklistKalem>(entity =>
        {
            entity.Property(e => e.KalemAdi).HasMaxLength(200);
            entity.Property(e => e.Aciklama).HasMaxLength(500);
            entity.HasOne(e => e.AylikChecklist)
                .WithMany(ac => ac.Kalemler)
                .HasForeignKey(e => e.AylikChecklistId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Personel Maaþ
        modelBuilder.Entity<PersonelMaas>(entity =>
        {
            entity.HasIndex(e => new { e.SoforId, e.Yil, e.Ay }).IsUnique();
            entity.Property(e => e.BrutMaas).HasPrecision(18, 2);
            entity.Property(e => e.NetMaas).HasPrecision(18, 2);
            entity.Property(e => e.SGKIsciPayi).HasPrecision(18, 2);
            entity.Property(e => e.SGKIsverenPayi).HasPrecision(18, 2);
            entity.Property(e => e.GelirVergisi).HasPrecision(18, 2);
            entity.Property(e => e.DamgaVergisi).HasPrecision(18, 2);
            entity.Property(e => e.IssizlikPrimi).HasPrecision(18, 2);
            entity.Property(e => e.Prim).HasPrecision(18, 2);
            entity.Property(e => e.Ikramiye).HasPrecision(18, 2);
            entity.Property(e => e.Yemek).HasPrecision(18, 2);
            entity.Property(e => e.Yol).HasPrecision(18, 2);
            entity.Property(e => e.Mesai).HasPrecision(18, 2);
            entity.Property(e => e.DigerEklemeler).HasPrecision(18, 2);
            entity.Property(e => e.Avans).HasPrecision(18, 2);
            entity.Property(e => e.IcraTakibi).HasPrecision(18, 2);
            entity.Property(e => e.DigerKesintiler).HasPrecision(18, 2);
            
            entity.HasOne(e => e.Sofor)
                .WithMany(s => s.Maaslar)
                .HasForeignKey(e => e.SoforId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Personel Ýzin
        modelBuilder.Entity<PersonelIzin>(entity =>
        {
            entity.HasOne(e => e.Sofor)
                .WithMany(s => s.Izinler)
                .HasForeignKey(e => e.SoforId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Personel Ýzin Hakký
        modelBuilder.Entity<PersonelIzinHakki>(entity =>
        {
            entity.HasIndex(e => new { e.SoforId, e.Yil }).IsUnique();
            entity.HasOne(e => e.Sofor)
                .WithMany(s => s.IzinHaklari)
                .HasForeignKey(e => e.SoforId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Budget Ödeme
        modelBuilder.Entity<BudgetOdeme>(entity =>
        {
            entity.HasIndex(e => new { e.OdemeYil, e.OdemeAy, e.MasrafKalemi });
            entity.HasIndex(e => e.TaksitGrupId);
            entity.Property(e => e.MasrafKalemi).HasMaxLength(200);
            entity.Property(e => e.Aciklama).HasMaxLength(500);
            entity.Property(e => e.Notlar).HasMaxLength(1000);
            entity.Property(e => e.Miktar).HasPrecision(18, 2);
            entity.Property(e => e.Durum).HasConversion<int>();
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Budget Masraf Kalemi
        modelBuilder.Entity<BudgetMasrafKalemi>(entity =>
        {
            entity.HasIndex(e => e.KalemAdi);
            entity.Property(e => e.KalemAdi).HasMaxLength(200);
            entity.Property(e => e.Kategori).HasMaxLength(100);
            entity.Property(e => e.Renk).HasMaxLength(20);
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Muhasebe Hesap
        modelBuilder.Entity<MuhasebeHesap>(entity =>
        {
            entity.HasIndex(e => e.HesapKodu).IsUnique();
            entity.Property(e => e.HesapKodu).HasMaxLength(10);
            entity.Property(e => e.HesapAdi).HasMaxLength(200);
            entity.HasOne(e => e.UstHesap)
                .WithMany()
                .HasForeignKey(e => e.UstHesapId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Muhasebe Fis
        modelBuilder.Entity<MuhasebeFis>(entity =>
        {
            entity.HasIndex(e => e.FisNo).IsUnique();
            entity.Property(e => e.FisNo).HasMaxLength(50);
            entity.Property(e => e.ToplamBorc).HasPrecision(18, 2);
            entity.Property(e => e.ToplamAlacak).HasPrecision(18, 2);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Muhasebe Fis Kalem
        modelBuilder.Entity<MuhasebeFisKalem>(entity =>
        {
            entity.Property(e => e.Borc).HasPrecision(18, 2);
            entity.Property(e => e.Alacak).HasPrecision(18, 2);
            entity.HasOne(e => e.Fis)
                .WithMany(f => f.Kalemler)
                .HasForeignKey(e => e.FisId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Hesap)
                .WithMany(h => h.FisKalemleri)
                .HasForeignKey(e => e.HesapId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Cari)
                .WithMany()
                .HasForeignKey(e => e.CariId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Muhasebe Donem
        modelBuilder.Entity<MuhasebeDonem>(entity =>
        {
            entity.HasIndex(e => new { e.Yil, e.Ay }).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Aktivite Log
        modelBuilder.Entity<AktiviteLog>(entity =>
        {
            entity.HasIndex(e => e.IslemZamani);
            entity.HasIndex(e => new { e.Modul, e.IslemTipi });
            entity.Property(e => e.IslemTipi).HasMaxLength(50);
            entity.Property(e => e.Modul).HasMaxLength(100);
            entity.Property(e => e.EntityTipi).HasMaxLength(100);
            entity.Property(e => e.EntityAdi).HasMaxLength(500);
            entity.Property(e => e.Aciklama).HasMaxLength(1000);
            entity.Property(e => e.KullaniciAdi).HasMaxLength(100);
            entity.Property(e => e.IpAdresi).HasMaxLength(50);
            entity.Property(e => e.Tarayici).HasMaxLength(500);
            // Log tablosunda soft delete yok
        });

        // Kullanici
        modelBuilder.Entity<Kullanici>(entity =>
        {
            entity.HasIndex(e => e.KullaniciAdi).IsUnique();
            entity.Property(e => e.KullaniciAdi).HasMaxLength(50);
            entity.Property(e => e.AdSoyad).HasMaxLength(100);
            entity.HasOne(e => e.Rol)
                .WithMany(r => r.Kullanicilar)
                .HasForeignKey(e => e.RolId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Sofor)
                .WithMany()
                .HasForeignKey(e => e.SoforId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Rol
        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasIndex(e => e.RolAdi).IsUnique();
            entity.Property(e => e.RolAdi).HasMaxLength(50);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // RolYetki
        modelBuilder.Entity<RolYetki>(entity =>
        {
            entity.HasIndex(e => new { e.RolId, e.YetkiKodu }).IsUnique();
            entity.Property(e => e.YetkiKodu).HasMaxLength(100);
            entity.HasOne(e => e.Rol)
                .WithMany(r => r.Yetkiler)
                .HasForeignKey(e => e.RolId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Lisans
        modelBuilder.Entity<Lisans>(entity =>
        {
            entity.HasIndex(e => e.LisansAnahtari).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // SatisPersoneli
        modelBuilder.Entity<SatisPersoneli>(entity =>
        {
            entity.HasIndex(e => e.PersonelKodu).IsUnique();
            entity.Property(e => e.PersonelKodu).HasMaxLength(50);
            entity.Property(e => e.AdSoyad).HasMaxLength(100);
            entity.Property(e => e.KomisyonOrani).HasPrecision(5, 2);
            entity.Property(e => e.SabitKomisyon).HasPrecision(18, 2);
            entity.Property(e => e.AylikSatisHedefi).HasPrecision(18, 2);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // AracIlan
        modelBuilder.Entity<AracIlan>(entity =>
        {
            entity.HasIndex(e => e.Plaka);
            entity.Property(e => e.Plaka).HasMaxLength(15);
            entity.Property(e => e.Marka).HasMaxLength(50);
            entity.Property(e => e.Model).HasMaxLength(50);
            entity.Property(e => e.AlisFiyati).HasPrecision(18, 2);
            entity.Property(e => e.SatisFiyati).HasPrecision(18, 2);
            entity.Property(e => e.KaskoDegeri).HasPrecision(18, 2);
            entity.Property(e => e.PiyasaDegeriMin).HasPrecision(18, 2);
            entity.Property(e => e.PiyasaDegeriMax).HasPrecision(18, 2);
            entity.Property(e => e.PiyasaDegeriOrtalama).HasPrecision(18, 2);
            entity.Property(e => e.TramerTutari).HasPrecision(18, 2);
            entity.HasOne(e => e.SahipCari)
                .WithMany()
                .HasForeignKey(e => e.SahipCariId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.SatisPersoneli)
                .WithMany(p => p.Ilanlar)
                .HasForeignKey(e => e.SatisPersoneliId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // PiyasaIlan
        modelBuilder.Entity<PiyasaIlan>(entity =>
        {
            entity.Property(e => e.Fiyat).HasPrecision(18, 2);
            entity.Property(e => e.TramerTutari).HasPrecision(18, 2);
            entity.HasOne(e => e.AracIlan)
                .WithMany(a => a.PiyasaIlanlari)
                .HasForeignKey(e => e.AracIlanId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // AracSatis
        modelBuilder.Entity<AracSatis>(entity =>
        {
            entity.Property(e => e.SatisFiyati).HasPrecision(18, 2);
            entity.Property(e => e.KomisyonTutari).HasPrecision(18, 2);
            entity.HasOne(e => e.AracIlan)
                .WithMany()
                .HasForeignKey(e => e.AracIlanId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.AliciCari)
                .WithMany()
                .HasForeignKey(e => e.AliciCariId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.SatisPersoneli)
                .WithMany(p => p.Satislar)
                .HasForeignKey(e => e.SatisPersoneliId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // AracMarka
        modelBuilder.Entity<AracMarka>(entity =>
        {
            entity.HasIndex(e => e.MarkaAdi).IsUnique();
            entity.Property(e => e.MarkaAdi).HasMaxLength(50);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // AracModelTanim
        modelBuilder.Entity<AracModelTanim>(entity =>
        {
            entity.Property(e => e.ModelAdi).HasMaxLength(50);
            entity.HasOne(e => e.Marka)
                .WithMany(m => m.Modeller)
                .HasForeignKey(e => e.MarkaId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    public override int SaveChanges()
    {
        ConvertDatesToUtc();
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ConvertDatesToUtc();
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }

    private void ConvertDatesToUtc()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            foreach (var property in entry.Properties)
            {
                if (property.CurrentValue is DateTime dateTime)
                {
                    if (dateTime.Kind != DateTimeKind.Utc)
                    {
                        property.CurrentValue = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                    }
                }
                else if (property.CurrentValue is DateTime?)
                {
                    var nullableDateTime = (DateTime?)property.CurrentValue;
                    if (nullableDateTime.HasValue && nullableDateTime.Value.Kind != DateTimeKind.Utc)
                    {
                        property.CurrentValue = DateTime.SpecifyKind(nullableDateTime.Value, DateTimeKind.Utc);
                    }
                }
            }
        }
    }
}
