using CRMFiloServis.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRMFiloServis.Web.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Cari Modülü
    public DbSet<Cari> Cariler { get; set; }

    // Filo Servis Modülü
    public DbSet<Sofor> Soforler { get; set; }
    public DbSet<Arac> Araclar { get; set; }
    public DbSet<Guzergah> Guzergahlar { get; set; }
    public DbSet<MasrafKalemi> MasrafKalemleri { get; set; }
    public DbSet<AracMasraf> AracMasraflari { get; set; }
    public DbSet<ServisCalisma> ServisCalismalari { get; set; }

    // Fatura Modülü
    public DbSet<Fatura> Faturalar { get; set; }
    public DbSet<FaturaKalem> FaturaKalemleri { get; set; }

    // Banka/Kasa Modülü
    public DbSet<BankaHesap> BankaHesaplari { get; set; }
    public DbSet<BankaKasaHareket> BankaKasaHareketleri { get; set; }
    public DbSet<OdemeEslestirme> OdemeEslestirmeleri { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
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
}
