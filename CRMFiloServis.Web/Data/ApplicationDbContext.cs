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
    public DbSet<AracPlaka> AracPlakalar { get; set; }
    public DbSet<Guzergah> Guzergahlar { get; set; }
    public DbSet<MasrafKalemi> MasrafKalemleri { get; set; }
    public DbSet<AracMasraf> AracMasraflari { get; set; }
    public DbSet<ServisCalisma> ServisCalismalari { get; set; }
    public DbSet<AracEvrak> AracEvraklari { get; set; }
    public DbSet<AracEvrakDosya> AracEvrakDosyalari { get; set; }

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
    public DbSet<TekrarlayanOdeme> TekrarlayanOdemeler { get; set; }

    // Muhasebe Modulu
    public DbSet<MuhasebeHesap> MuhasebeHesaplari { get; set; }
    public DbSet<MuhasebeFis> MuhasebeFisleri { get; set; }
    public DbSet<MuhasebeFisKalem> MuhasebeFisKalemleri { get; set; }
    public DbSet<MuhasebeDonem> MuhasebeDonemleri { get; set; }
    public DbSet<MuhasebeAyar> MuhasebeAyarlari { get; set; }
    public DbSet<KostMerkezi> KostMerkezleri { get; set; }
    public DbSet<MuhasebeProje> MuhasebeProjeler { get; set; }

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

    // Aylik Odeme Modulu
    public DbSet<AylikOdemePlani> AylikOdemePlanlari { get; set; }
    public DbSet<AylikOdemeGerceklesen> AylikOdemeGerceklesenler { get; set; }

    // Kiralama ve Servis Takip Modulu
    public DbSet<KiralamaArac> KiralamaAraclar { get; set; }
    public DbSet<ServisCalismaKiralama> ServisCalismaKiralamalar { get; set; }

    // Musteri Kiralama Modulu
    public DbSet<MusteriKiralama> MusteriKiralamalar { get; set; }

    // Puantaj Modulu
    public DbSet<PersonelPuantaj> PersonelPuantajlar { get; set; }
    public DbSet<GunlukPuantaj> GunlukPuantajlar { get; set; }

    // Filo Komisyon ve Araç Operasyon Puantaj Modülü
    public DbSet<FiloGuzergahEslestirme> FiloGuzergahEslestirmeleri { get; set; }
    public DbSet<FiloGunlukPuantaj> FiloGunlukPuantajlar { get; set; }

    // Piyasa Arastirma Modulu
    public DbSet<AracPiyasaArastirma> PiyasaArastirmalar { get; set; }
    public DbSet<PiyasaArastirmaIlan> PiyasaArastirmaIlanlar { get; set; }
    public DbSet<AracMarkaModel> AracMarkaModeller { get; set; }
    public DbSet<PiyasaKaynak> PiyasaKaynaklar { get; set; }

    // Filo Operasyon Modülü (Komisyonculuk, Araç Alım/Satım, Plaka Dönüşüm)
    public DbSet<KomisyonculukIs> KomisyonculukIsler { get; set; }
    public DbSet<KomisyonculukIsAtama> KomisyonculukIsAtamalar { get; set; }
    public DbSet<AracAlimSatim> AracAlimSatimlar { get; set; }
    public DbSet<PlakaDonusum> PlakaDonusumler { get; set; }
    public DbSet<AracOperasyonDurum> AracOperasyonDurumlari { get; set; }

    // CRM Modulu
    public DbSet<Bildirim> Bildirimler { get; set; }
    public DbSet<Mesaj> Mesajlar { get; set; }
    public DbSet<EmailAyar> EmailAyarlari { get; set; }
    public DbSet<WhatsAppAyar> WhatsAppAyarlari { get; set; }
    public DbSet<Hatirlatici> Hatirlaticilar { get; set; }
    public DbSet<KullaniciCari> KullaniciCariler { get; set; }
    public DbSet<DashboardWidget> DashboardWidgetlar { get; set; }
    public DbSet<CariIletisimNot> CariIletisimNotlar { get; set; }
    public DbSet<CariHatirlatma> CariHatirlatmalar { get; set; }

    // WhatsApp Iletisim Modulu
    public DbSet<WhatsAppKisi> WhatsAppKisiler { get; set; }
    public DbSet<WhatsAppGrup> WhatsAppGruplar { get; set; }
    public DbSet<WhatsAppGrupUye> WhatsAppGrupUyeler { get; set; }
    public DbSet<WhatsAppMesaj> WhatsAppMesajlar { get; set; }
    public DbSet<WhatsAppSablon> WhatsAppSablonlar { get; set; }

    // Stok/Envanter Modulu
    public DbSet<StokKarti> StokKartlari { get; set; }
    public DbSet<StokKategori> StokKategoriler { get; set; }
    public DbSet<StokHareket> StokHareketler { get; set; }
    public DbSet<AracIslem> AracIslemler { get; set; }
    public DbSet<ServisKaydi> ServisKayitlari { get; set; }
    public DbSet<ServisParca> ServisParcalar { get; set; }

    // Personel Özlük Evrak Modülü
    public DbSet<OzlukEvrakTanim> OzlukEvrakTanimlari { get; set; }
    public DbSet<PersonelOzlukEvrak> PersonelOzlukEvraklar { get; set; }

    // Fatura Şablon Modülü
    public DbSet<FaturaSablon> FaturaSablonlari { get; set; }

    // Personel Finans Modülü (Avans ve Borç Takip)
    public DbSet<PersonelAvans> PersonelAvanslar { get; set; }
    public DbSet<PersonelBorc> PersonelBorclar { get; set; }
    public DbSet<PersonelAvansMahsup> PersonelAvansMahsuplar { get; set; }
    public DbSet<PersonelBorcOdeme> PersonelBorcOdemeler { get; set; }
    public DbSet<PersonelFinansAyar> PersonelFinansAyarlar { get; set; }

    // Bordro Modülü
    public DbSet<Bordro> Bordrolar { get; set; }
    public DbSet<BordroDetay> BordroDetaylar { get; set; }
    public DbSet<BordroOdeme> BordroOdemeler { get; set; }
    public DbSet<BordroAyar> BordroAyarlar { get; set; }

    // Araç İlan Yayın ve Kullanıcı Tercihleri Modülü
    public DbSet<IlanPlatformu> IlanPlatformlari { get; set; }
    public DbSet<AracIlanYayin> AracIlanYayinlar { get; set; }
    public DbSet<AracIlanIcerik> AracIlanIcerikleri { get; set; }
    public DbSet<KullaniciTercihi> KullaniciTercihleri { get; set; }
    public DbSet<KullaniciSonIslem> KullaniciSonIslemler { get; set; }

    // Puantaj/Hakedis Modülü (Excel Import destekli)
    public DbSet<PuantajKayit> PuantajKayitlar { get; set; }
    public DbSet<PuantajExcelImport> PuantajExcelImportlar { get; set; }
    public DbSet<PuantajEslestirmeOneri> PuantajEslestirmeOnerileri { get; set; }

    // Proforma Fatura Modülü
    public DbSet<ProformaFatura> ProformaFaturalar { get; set; }
    public DbSet<ProformaFaturaKalem> ProformaFaturaKalemler { get; set; }

    // İhale Hazırlık Modülü
    public DbSet<IhaleProje> IhaleProjeleri { get; set; }
    public DbSet<IhaleGuzergahKalem> IhaleGuzergahKalemleri { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        var isSqlite = Database.IsSqlite();

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
            // CariKodu unique - SQLite filter desteklemiyor, PostgreSQL destekliyor
            if (isSqlite)
            {
                entity.HasIndex(e => e.CariKodu).IsUnique();
            }
            else
            {
                entity.HasIndex(e => e.CariKodu)
                    .IsUnique()
                    .HasFilter("\"IsDeleted\" = false");
            }
            
            entity.Property(e => e.CariKodu).HasMaxLength(50);
            entity.Property(e => e.Unvan).HasMaxLength(250);
            entity.Property(e => e.VergiNo).HasMaxLength(20);
            entity.Property(e => e.TcKimlikNo).HasMaxLength(11);
            entity.Property(e => e.Telefon).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            
            // Personel (Sofor) iliskisi
            entity.HasOne(e => e.Sofor)
                .WithMany()
                .HasForeignKey(e => e.SoforId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Şoför
        modelBuilder.Entity<Sofor>(entity =>
        {
            entity.ToTable("Personeller");
            entity.HasIndex(e => e.SoforKodu).IsUnique();
            entity.Property(e => e.SoforKodu).HasMaxLength(50);
            entity.Property(e => e.Ad).HasMaxLength(100);
            entity.Property(e => e.Soyad).HasMaxLength(100);
            entity.Property(e => e.TcKimlikNo).HasMaxLength(11);
            entity.Property(e => e.Telefon).HasMaxLength(20);
            entity.Property(e => e.BrutMaas).HasPrecision(18, 2);
            entity.Property(e => e.CalismaMiktari).HasPrecision(18, 2);
            entity.Property(e => e.BirimUcret).HasPrecision(18, 2);
            entity.Property(e => e.ResmiNetMaas).HasPrecision(18, 2);
            entity.Property(e => e.DigerMaas).HasPrecision(18, 2);
            entity.Property(e => e.NetMaas).HasPrecision(18, 2);
            entity.Property(e => e.TopluMaas).HasPrecision(18, 2);
            entity.Property(e => e.SgkMaasi).HasPrecision(18, 2);
            entity.Property(e => e.SGKBordroDahilMi).HasDefaultValue(false);
            entity.Property(e => e.BordroTipiPersonel).HasDefaultValue(PersonelBordroTipi.Yok);
            entity.Ignore(e => e.EkOdeme);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Araç
        modelBuilder.Entity<Arac>(entity =>
        {
            // Şase numarası unique
            if (isSqlite)
            {
                entity.HasIndex(e => e.SaseNo).IsUnique();
            }
            else
            {
                entity.HasIndex(e => e.SaseNo)
                    .IsUnique()
                    .HasFilter("\"IsDeleted\" = false");
            }
            
            entity.Property(e => e.SaseNo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.AktifPlaka).HasMaxLength(15);
            entity.Property(e => e.Marka).HasMaxLength(50);
            entity.Property(e => e.Model).HasMaxLength(50);
            entity.Property(e => e.MotorNo).HasMaxLength(50);
            entity.Property(e => e.Renk).HasMaxLength(30);
            entity.Property(e => e.GunlukKiraBedeli).HasPrecision(18, 2);
            entity.Property(e => e.AylikKiraBedeli).HasPrecision(18, 2);
            entity.Property(e => e.SeferBasinaKiraBedeli).HasPrecision(18, 2);
            entity.Property(e => e.KomisyonOrani).HasPrecision(5, 2);
            entity.Property(e => e.SabitKomisyonTutari).HasPrecision(18, 2);
            entity.Property(e => e.SatisFiyati).HasPrecision(18, 2);
            
            entity.HasOne(e => e.KiralikCari)
                .WithMany()
                .HasForeignKey(e => e.KiralikCariId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.KomisyoncuCari)
                .WithMany()
                .HasForeignKey(e => e.KomisyoncuCariId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // PlakaGecmisi navigation'ı AracPlaka entity'sinde tanımlanıyor
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
        
        // Araç Plaka Geçmişi
        modelBuilder.Entity<AracPlaka>(entity =>
        {
            entity.Property(e => e.Plaka).HasMaxLength(15).IsRequired();
            entity.Property(e => e.Aciklama).HasMaxLength(500);
            entity.Property(e => e.IslemTutari).HasPrecision(18, 2);
            
            entity.HasOne(e => e.Arac)
                .WithMany(a => a.PlakaGecmisi)
                .HasForeignKey(e => e.AracId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Cari)
                .WithMany()
                .HasForeignKey(e => e.CariId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Aynı anda aynı plaka farklı araçta aktif olamaz
            entity.HasIndex(e => new { e.Plaka, e.CikisTarihi })
                .HasFilter("\"CikisTarihi\" IS NULL AND \"IsDeleted\" = false");
                
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
            entity.HasOne(e => e.Sofor)
                .WithMany()
                .HasForeignKey(e => e.SoforId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Cari)
                .WithMany()
                .HasForeignKey(e => e.CariId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.MuhasebeFis)
                .WithMany()
                .HasForeignKey(e => e.MuhasebeFisId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Servis Çalışma
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
            entity.Property(e => e.TevkifatOrani).HasPrecision(5, 2);
            entity.Property(e => e.TevkifatTutar).HasPrecision(18, 2);
            entity.Property(e => e.TevkifatKodu).HasMaxLength(20);
            entity.HasOne(e => e.Cari)
                .WithMany(c => c.Faturalar)
                .HasForeignKey(e => e.CariId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Arac)
                .WithMany()
                .HasForeignKey(e => e.AracId)
                .OnDelete(DeleteBehavior.SetNull);
            // Firmalar arası fatura - Karşı firma ilişkisi
            entity.HasOne(e => e.KarsiFirma)
                .WithMany()
                .HasForeignKey(e => e.KarsiFirmaId)
                .OnDelete(DeleteBehavior.SetNull);
            // Firmalar arası fatura eşleştirme ilişkisi
            entity.HasOne(e => e.EslesenFatura)
                .WithMany()
                .HasForeignKey(e => e.EslesenFaturaId)
                .OnDelete(DeleteBehavior.SetNull);
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
            entity.Property(e => e.IskontoOrani).HasPrecision(5, 2);
            entity.Property(e => e.IskontoTutar).HasPrecision(18, 2);
            entity.Property(e => e.TevkifatOrani).HasPrecision(5, 2);
            entity.Property(e => e.TevkifatTutar).HasPrecision(18, 2);
            entity.Property(e => e.Birim).HasMaxLength(20);
            entity.Property(e => e.UrunKodu).HasMaxLength(50);
            entity.HasOne(e => e.Fatura)
                .WithMany(f => f.FaturaKalemleri)
                .HasForeignKey(e => e.FaturaId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.MuhasebeHesap)
                .WithMany()
                .HasForeignKey(e => e.MuhasebeHesapId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Arac)
                .WithMany()
                .HasForeignKey(e => e.AracId)
                .OnDelete(DeleteBehavior.SetNull);
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

        // Ödeme Eşleştirme
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

        // Personel Maaş
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

        // Personel İzin
        modelBuilder.Entity<PersonelIzin>(entity =>
        {
            entity.HasOne(e => e.Sofor)
                .WithMany(s => s.Izinler)
                .HasForeignKey(e => e.SoforId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Personel İzin Hakkı
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

        // Tekrarlayan Odeme
        modelBuilder.Entity<TekrarlayanOdeme>(entity =>
        {
            entity.HasIndex(e => e.MasrafKalemi);
            entity.Property(e => e.OdemeAdi).HasMaxLength(200);
            entity.Property(e => e.MasrafKalemi).HasMaxLength(200);
            entity.Property(e => e.Aciklama).HasMaxLength(500);
            entity.Property(e => e.Notlar).HasMaxLength(1000);
            entity.Property(e => e.Tutar).HasPrecision(18, 2);
            entity.Property(e => e.Renk).HasMaxLength(20);
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.Property(e => e.Periyod).HasConversion<int>();
            entity.HasOne(e => e.Firma)
                .WithMany()
                .HasForeignKey(e => e.FirmaId)
                .OnDelete(DeleteBehavior.SetNull);
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

        // ===== FİLO OPERASYON MODÜLÜ =====

        // KomisyonculukIs
        modelBuilder.Entity<KomisyonculukIs>(entity =>
        {
            entity.HasIndex(e => e.IsKodu).IsUnique();
            entity.Property(e => e.IsKodu).HasMaxLength(50);
            entity.Property(e => e.IsAciklamasi).HasMaxLength(200);
            entity.Property(e => e.BirimFiyat).HasPrecision(18, 2);
            entity.Property(e => e.ToplamTutar).HasPrecision(18, 2);
            entity.HasOne(e => e.MusteriCari)
                .WithMany()
                .HasForeignKey(e => e.MusteriCariId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.AlinanIsFatura)
                .WithMany()
                .HasForeignKey(e => e.AlinanIsFaturaId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // KomisyonculukIsAtama
        modelBuilder.Entity<KomisyonculukIsAtama>(entity =>
        {
            entity.Property(e => e.DisAracPlaka).HasMaxLength(15);
            entity.Property(e => e.DisSoforAdSoyad).HasMaxLength(100);
            entity.Property(e => e.DisSoforTelefon).HasMaxLength(20);
            entity.Property(e => e.AracKiraBedeli).HasPrecision(18, 2);
            entity.Property(e => e.SoforMaliyeti).HasPrecision(18, 2);
            entity.Property(e => e.YakitMaliyeti).HasPrecision(18, 2);
            entity.Property(e => e.OtoyolMaliyeti).HasPrecision(18, 2);
            entity.Property(e => e.DigerMasraflar).HasPrecision(18, 2);
            entity.HasOne(e => e.KomisyonculukIs)
                .WithMany(i => i.Atamalar)
                .HasForeignKey(e => e.KomisyonculukIsId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Arac)
                .WithMany()
                .HasForeignKey(e => e.AracId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.TedarikciCari)
                .WithMany()
                .HasForeignKey(e => e.TedarikciCariId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Sofor)
                .WithMany()
                .HasForeignKey(e => e.SoforId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.VerilenIsFatura)
                .WithMany()
                .HasForeignKey(e => e.VerilenIsFaturaId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // AracAlimSatim
        modelBuilder.Entity<AracAlimSatim>(entity =>
        {
            entity.HasIndex(e => new { e.AracId, e.IslemTarihi });
            entity.Property(e => e.KarsiTarafAdSoyad).HasMaxLength(100);
            entity.Property(e => e.KarsiTarafTcKimlik).HasMaxLength(11);
            entity.Property(e => e.KarsiTarafTelefon).HasMaxLength(20);
            entity.Property(e => e.IslemTutari).HasPrecision(18, 2);
            entity.Property(e => e.KDVTutari).HasPrecision(18, 2);
            entity.Property(e => e.ToplamTutar).HasPrecision(18, 2);
            entity.Property(e => e.NoterAdi).HasMaxLength(100);
            entity.Property(e => e.NoterYevmiyeNo).HasMaxLength(50);
            entity.Property(e => e.OdenenTutar).HasPrecision(18, 2);
            entity.HasOne(e => e.Arac)
                .WithMany()
                .HasForeignKey(e => e.AracId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.KarsiTarafCari)
                .WithMany()
                .HasForeignKey(e => e.KarsiTarafCariId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Fatura)
                .WithMany()
                .HasForeignKey(e => e.FaturaId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // PlakaDonusum
        modelBuilder.Entity<PlakaDonusum>(entity =>
        {
            entity.HasIndex(e => new { e.AracId, e.EskiPlaka });
            entity.Property(e => e.EskiPlaka).HasMaxLength(15);
            entity.Property(e => e.YeniPlaka).HasMaxLength(15);
            entity.Property(e => e.PlakaBedeliMasrafi).HasPrecision(18, 2);
            entity.Property(e => e.EmnivetHarci).HasPrecision(18, 2);
            entity.Property(e => e.NoterMasrafi).HasPrecision(18, 2);
            entity.Property(e => e.DigerMasraflar).HasPrecision(18, 2);
            entity.Property(e => e.PlakaSatisBedeli).HasPrecision(18, 2);
            entity.HasOne(e => e.Arac)
                .WithMany()
                .HasForeignKey(e => e.AracId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.PlakaSatisCarisi)
                .WithMany()
                .HasForeignKey(e => e.PlakaSatisCarisiId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // AracOperasyonDurum
        modelBuilder.Entity<AracOperasyonDurum>(entity =>
        {
            entity.HasIndex(e => new { e.AracId, e.Yil, e.Ay }).IsUnique();
            entity.Property(e => e.BrutGelir).HasPrecision(18, 2);
            entity.Property(e => e.KomisyonKesintisi).HasPrecision(18, 2);
            entity.Property(e => e.YakitGideri).HasPrecision(18, 2);
            entity.Property(e => e.SoforMaliyeti).HasPrecision(18, 2);
            entity.Property(e => e.KiraBedeli).HasPrecision(18, 2);
            entity.Property(e => e.BakimOnarimGideri).HasPrecision(18, 2);
            entity.Property(e => e.SigortaGideri).HasPrecision(18, 2);
            entity.Property(e => e.VergiGideri).HasPrecision(18, 2);
            entity.Property(e => e.OtoyolGideri).HasPrecision(18, 2);
            entity.Property(e => e.DigerGiderler).HasPrecision(18, 2);
            entity.HasOne(e => e.Arac)
                .WithMany()
                .HasForeignKey(e => e.AracId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ===== ARAÇ İLAN YAYIN VE KULLANICI TERCİHLERİ MODÜLÜ =====

        // IlanPlatformu (arabam, sahibinden, letgo vb.)
        modelBuilder.Entity<IlanPlatformu>(entity =>
        {
            entity.HasIndex(e => e.PlatformAdi).IsUnique();
            entity.Property(e => e.PlatformAdi).HasMaxLength(50);
            entity.Property(e => e.WebSiteUrl).HasMaxLength(100);
            entity.Property(e => e.ApiUrl).HasMaxLength(100);
            entity.Property(e => e.ApiKey).HasMaxLength(200);
            entity.Property(e => e.ApiSecret).HasMaxLength(100);
            entity.Property(e => e.KullaniciAdi).HasMaxLength(100);
            entity.Property(e => e.Sifre).HasMaxLength(100);
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.Property(e => e.Renk).HasMaxLength(20);
            entity.Property(e => e.Notlar).HasMaxLength(500);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // AracIlanYayin - hangi araç hangi platformda yayında
        modelBuilder.Entity<AracIlanYayin>(entity =>
        {
            entity.HasIndex(e => new { e.AracId, e.PlatformId }).IsUnique().HasFilter("\"IsDeleted\" = false");
            entity.Property(e => e.PlatformIlanNo).HasMaxLength(100);
            entity.Property(e => e.PlatformIlanUrl).HasMaxLength(500);
            entity.Property(e => e.YayinFiyati).HasPrecision(18, 2);
            entity.Property(e => e.FiyatAciklama).HasMaxLength(50);
            entity.Property(e => e.OneCikarmaBedeli).HasPrecision(18, 2);
            entity.Property(e => e.Notlar).HasMaxLength(500);
            entity.HasOne(e => e.Arac)
                .WithMany()
                .HasForeignKey(e => e.AracId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Platform)
                .WithMany(p => p.Yayinlar)
                .HasForeignKey(e => e.PlatformId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.YayinlayanKullanici)
                .WithMany()
                .HasForeignKey(e => e.YayinlayanKullaniciId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // AracIlanIcerik - ilan içeriği (başlık, açıklama, fotoğraflar)
        modelBuilder.Entity<AracIlanIcerik>(entity =>
        {
            entity.HasIndex(e => new { e.AracId, e.PlatformId });
            entity.Property(e => e.IlanBasligi).HasMaxLength(200);
            entity.Property(e => e.MetaBaslik).HasMaxLength(200);
            entity.Property(e => e.MetaAciklama).HasMaxLength(500);
            entity.Property(e => e.AnahtarKelimeler).HasMaxLength(200);
            entity.HasOne(e => e.Arac)
                .WithMany()
                .HasForeignKey(e => e.AracId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Platform)
                .WithMany()
                .HasForeignKey(e => e.PlatformId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // KullaniciTercihi - varsayılan anasayfa, tema, bildirimler
        modelBuilder.Entity<KullaniciTercihi>(entity =>
        {
            entity.HasIndex(e => e.KullaniciId).IsUnique();
            entity.Property(e => e.VarsayilanAnasayfa).HasMaxLength(100);
            entity.Property(e => e.Tema).HasMaxLength(20);
            entity.Property(e => e.SidebarDurum).HasMaxLength(20);
            entity.Property(e => e.VarsayilanSiralama).HasMaxLength(2000);
            entity.Property(e => e.AnasayfaWidgetSirasi).HasMaxLength(2000);
            entity.Property(e => e.DigerTercihler).HasMaxLength(4000);
            entity.HasOne(e => e.Kullanici)
                .WithMany()
                .HasForeignKey(e => e.KullaniciId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // KullaniciSonIslem - son erişilen sayfalar
        modelBuilder.Entity<KullaniciSonIslem>(entity =>
        {
            entity.HasIndex(e => new { e.KullaniciId, e.SayfaYolu });
            entity.Property(e => e.SayfaYolu).HasMaxLength(200);
            entity.Property(e => e.SayfaBasligi).HasMaxLength(200);
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.HasOne(e => e.Kullanici)
                .WithMany()
                .HasForeignKey(e => e.KullaniciId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ===== PUANTAJ/HAKEDİS MODÜLÜ KONFIGURASYONLARI =====

        // PuantajKayit - Excel import ve manuel giriş
        modelBuilder.Entity<PuantajKayit>(entity =>
        {
            entity.HasIndex(e => new { e.Yil, e.Ay, e.GuzergahId, e.AracId });
            entity.HasIndex(e => new { e.Yil, e.Ay, e.KurumCariId });
            entity.Property(e => e.KurumAdi).HasMaxLength(200);
            entity.Property(e => e.GuzergahAdi).HasMaxLength(200);
            entity.Property(e => e.Plaka).HasMaxLength(20);
            entity.Property(e => e.SoforAdi).HasMaxLength(100);
            entity.Property(e => e.SoforTelefon).HasMaxLength(20);
            entity.Property(e => e.FaturaKesiciAdi).HasMaxLength(200);
            entity.Property(e => e.FaturaKesiciTelefon).HasMaxLength(20);
            entity.Property(e => e.GelirFaturaNo).HasMaxLength(50);
            entity.Property(e => e.GiderFaturaNo).HasMaxLength(50);
            entity.Property(e => e.OnaylayanKullanici).HasMaxLength(100);
            entity.Property(e => e.Notlar).HasMaxLength(1000);

            // Decimal precision
            entity.Property(e => e.Gun).HasPrecision(10, 2);
            entity.Property(e => e.BirimGelir).HasPrecision(18, 2);
            entity.Property(e => e.ToplamGelir).HasPrecision(18, 2);
            entity.Property(e => e.GelirKdvTutari).HasPrecision(18, 2);
            entity.Property(e => e.GelirToplam).HasPrecision(18, 2);
            entity.Property(e => e.BirimGider).HasPrecision(18, 2);
            entity.Property(e => e.ToplamGider).HasPrecision(18, 2);
            entity.Property(e => e.GiderKdv20Tutari).HasPrecision(18, 2);
            entity.Property(e => e.GiderKdv10Tutari).HasPrecision(18, 2);
            entity.Property(e => e.GiderKesinti).HasPrecision(18, 2);
            entity.Property(e => e.Odenecek).HasPrecision(18, 2);
            entity.Property(e => e.GelirOdenenTutar).HasPrecision(18, 2);
            entity.Property(e => e.GiderOdenenTutar).HasPrecision(18, 2);

            // İlişkiler
            entity.HasOne(e => e.KurumCari)
                .WithMany()
                .HasForeignKey(e => e.KurumCariId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Guzergah)
                .WithMany()
                .HasForeignKey(e => e.GuzergahId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Arac)
                .WithMany()
                .HasForeignKey(e => e.AracId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Sofor)
                .WithMany()
                .HasForeignKey(e => e.SoforId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.OdemeYapilacakCari)
                .WithMany()
                .HasForeignKey(e => e.OdemeYapilacakCariId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.FaturaKesiciCari)
                .WithMany()
                .HasForeignKey(e => e.FaturaKesiciCariId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // PuantajExcelImport - import batch kaydı
        modelBuilder.Entity<PuantajExcelImport>(entity =>
        {
            entity.HasIndex(e => new { e.Yil, e.Ay });
            entity.Property(e => e.DosyaAdi).HasMaxLength(200);
            entity.Property(e => e.ImportEdenKullanici).HasMaxLength(100);
            entity.Property(e => e.HataMesaji).HasMaxLength(2000);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // PuantajEslestirmeOneri - import eşleştirme önerileri
        modelBuilder.Entity<PuantajEslestirmeOneri>(entity =>
        {
            entity.HasIndex(e => new { e.ExcelImportId, e.Tip, e.ExcelDeger });
            entity.Property(e => e.ExcelDeger).HasMaxLength(200);
            entity.Property(e => e.OnerilenAd).HasMaxLength(200);
            entity.HasOne(e => e.ExcelImport)
                .WithMany()
                .HasForeignKey(e => e.ExcelImportId)
                .OnDelete(DeleteBehavior.Cascade);
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

        // AracEvrak
        modelBuilder.Entity<AracEvrak>(entity =>
        {
            entity.HasIndex(e => new { e.AracId, e.EvrakKategorisi });
            entity.Property(e => e.EvrakKategorisi).HasMaxLength(100);
            entity.Property(e => e.EvrakAdi).HasMaxLength(200);
            entity.Property(e => e.Aciklama).HasMaxLength(500);
            entity.Property(e => e.SigortaSirketi).HasMaxLength(100);
            entity.Property(e => e.PoliceNo).HasMaxLength(100);
            entity.Property(e => e.Tutar).HasPrecision(18, 2);
            entity.HasOne(e => e.Arac)
                .WithMany()
                .HasForeignKey(e => e.AracId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // AracEvrakDosya
        modelBuilder.Entity<AracEvrakDosya>(entity =>
        {
            entity.Property(e => e.DosyaAdi).HasMaxLength(255);
            entity.Property(e => e.DosyaYolu).HasMaxLength(500);
            entity.Property(e => e.DosyaTipi).HasMaxLength(20);
            entity.Property(e => e.Aciklama).HasMaxLength(500);
            entity.HasOne(e => e.AracEvrak)
                .WithMany(e => e.Dosyalar)
                .HasForeignKey(e => e.AracEvrakId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ===== CRM MODULU KONFIGURASYONLARI =====

        // Bildirim
        modelBuilder.Entity<Bildirim>(entity =>
        {
            entity.HasIndex(e => new { e.KullaniciId, e.Okundu });
            entity.Property(e => e.Baslik).HasMaxLength(200);
            entity.Property(e => e.Icerik).HasMaxLength(1000);
            entity.Property(e => e.IliskiliTablo).HasMaxLength(50);
            entity.Property(e => e.Link).HasMaxLength(200);
            entity.HasOne(e => e.Kullanici)
                .WithMany(k => k.Bildirimler)
                .HasForeignKey(e => e.KullaniciId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Mesaj
        modelBuilder.Entity<Mesaj>(entity =>
        {
            entity.HasIndex(e => new { e.AliciId, e.Okundu });
            entity.HasIndex(e => e.GonderenId);
            entity.Property(e => e.Konu).HasMaxLength(200);
            entity.Property(e => e.DisAlici).HasMaxLength(100);
            entity.Property(e => e.DisGonderimId).HasMaxLength(100);
            entity.HasOne(e => e.Gonderen)
                .WithMany(k => k.GonderilenMesajlar)
                .HasForeignKey(e => e.GonderenId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Alici)
                .WithMany(k => k.AlinanMesajlar)
                .HasForeignKey(e => e.AliciId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.UstMesaj)
                .WithMany(m => m.Yanitlar)
                .HasForeignKey(e => e.UstMesajId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // EmailAyar
        modelBuilder.Entity<EmailAyar>(entity =>
        {
            entity.Property(e => e.SmtpSunucu).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Sifre).HasMaxLength(100);
            entity.Property(e => e.GonderenAdi).HasMaxLength(100);
            entity.Property(e => e.ImapSunucu).HasMaxLength(100);
            entity.Property(e => e.GelenKlasoru).HasMaxLength(100);
            entity.HasOne(e => e.Kullanici)
                .WithMany()
                .HasForeignKey(e => e.KullaniciId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // WhatsAppAyar
        modelBuilder.Entity<WhatsAppAyar>(entity =>
        {
            entity.Property(e => e.Telefon).HasMaxLength(20);
            entity.Property(e => e.ApiKey).HasMaxLength(500);
            entity.Property(e => e.WebhookUrl).HasMaxLength(200);
            entity.Property(e => e.HizliSablonlarJson).HasMaxLength(4000);
            entity.HasOne(e => e.Kullanici)
                .WithMany()
                .HasForeignKey(e => e.KullaniciId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Hatirlatici
        modelBuilder.Entity<Hatirlatici>(entity =>
        {
            entity.HasIndex(e => new { e.KullaniciId, e.BaslangicTarihi });
            entity.Property(e => e.Baslik).HasMaxLength(200);
            entity.Property(e => e.Aciklama).HasMaxLength(1000);
            entity.Property(e => e.IliskiliTablo).HasMaxLength(50);
            entity.Property(e => e.Renk).HasMaxLength(20);
            entity.HasOne(e => e.Kullanici)
                .WithMany(k => k.Hatirlaticilar)
                .HasForeignKey(e => e.KullaniciId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Cari)
                .WithMany(c => c.Hatirlaticilar)
                .HasForeignKey(e => e.CariId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // WhatsApp Modelleri
        modelBuilder.Entity<WhatsAppKisi>(entity =>
        {
            entity.HasIndex(e => e.Telefon).IsUnique().HasFilter("\"IsDeleted\" = false");
            entity.HasOne(e => e.Cari)
                .WithMany()
                .HasForeignKey(e => e.CariId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<WhatsAppGrupUye>(entity =>
        {
            entity.HasIndex(e => new { e.GrupId, e.KisiId }).IsUnique().HasFilter("\"IsDeleted\" = false");
            
            entity.HasOne(e => e.Grup)
                .WithMany(g => g.Uyeler)
                .HasForeignKey(e => e.GrupId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Kisi)
                .WithMany(k => k.Gruplari)
                .HasForeignKey(e => e.KisiId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<WhatsAppMesaj>(entity =>
        {
            entity.HasOne(e => e.Gonderen)
                .WithMany()
                .HasForeignKey(e => e.GonderenId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Kisi)
                .WithMany()
                .HasForeignKey(e => e.KisiId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Grup)
                .WithMany()
                .HasForeignKey(e => e.GrupId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ===== STOK/ENVANTER MODULU KONFIGURASYONLARI =====

        // StokKarti
        modelBuilder.Entity<StokKarti>(entity =>
        {
            entity.HasIndex(e => e.StokKodu).IsUnique();
            entity.Property(e => e.StokKodu).HasMaxLength(50);
            entity.Property(e => e.StokAdi).HasMaxLength(200);
            entity.Property(e => e.Barkod).HasMaxLength(50);
            entity.Property(e => e.Birim).HasMaxLength(20);
            entity.Property(e => e.AlisFiyati).HasPrecision(18, 2);
            entity.Property(e => e.SatisFiyati).HasPrecision(18, 2);
            entity.Property(e => e.KdvOrani).HasPrecision(5, 2);
            entity.Property(e => e.MinStokMiktari).HasPrecision(18, 4);
            entity.Property(e => e.MaksStokMiktari).HasPrecision(18, 4);
            entity.Property(e => e.MevcutStok).HasPrecision(18, 4);
            entity.HasOne(e => e.Kategori)
                .WithMany(k => k.StokKartlari)
                .HasForeignKey(e => e.KategoriId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.VarsayilanTedarikci)
                .WithMany()
                .HasForeignKey(e => e.VarsayilanTedarikciId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.MuhasebeHesap)
                .WithMany()
                .HasForeignKey(e => e.MuhasebeHesapId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // StokKategori
        modelBuilder.Entity<StokKategori>(entity =>
        {
            entity.Property(e => e.KategoriAdi).HasMaxLength(100);
            entity.Property(e => e.Renk).HasMaxLength(20);
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.HasOne(e => e.UstKategori)
                .WithMany(k => k.AltKategoriler)
                .HasForeignKey(e => e.UstKategoriId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // StokHareket
        modelBuilder.Entity<StokHareket>(entity =>
        {
            entity.HasIndex(e => new { e.StokKartiId, e.IslemTarihi });
            entity.Property(e => e.BelgeNo).HasMaxLength(50);
            entity.Property(e => e.Miktar).HasPrecision(18, 4);
            entity.Property(e => e.BirimFiyat).HasPrecision(18, 2);
            entity.HasOne(e => e.StokKarti)
                .WithMany(s => s.Hareketler)
                .HasForeignKey(e => e.StokKartiId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Fatura)
                .WithMany()
                .HasForeignKey(e => e.FaturaId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.FaturaKalem)
                .WithMany()
                .HasForeignKey(e => e.FaturaKalemId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Cari)
                .WithMany()
                .HasForeignKey(e => e.CariId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Arac)
                .WithMany()
                .HasForeignKey(e => e.AracId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.AracMasraf)
                .WithMany()
                .HasForeignKey(e => e.AracMasrafId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // AracIslem (Araç Alış/Satış)
        modelBuilder.Entity<AracIslem>(entity =>
        {
            entity.HasIndex(e => new { e.AracId, e.IslemTarihi });
            entity.Property(e => e.Tutar).HasPrecision(18, 2);
            entity.Property(e => e.KdvOrani).HasPrecision(5, 2);
            entity.Property(e => e.KdvTutar).HasPrecision(18, 2);
            entity.Property(e => e.ToplamTutar).HasPrecision(18, 2);
            entity.Property(e => e.NoterId).HasMaxLength(50);
            entity.HasOne(e => e.Arac)
                .WithMany()
                .HasForeignKey(e => e.AracId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Cari)
                .WithMany()
                .HasForeignKey(e => e.CariId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Fatura)
                .WithMany()
                .HasForeignKey(e => e.FaturaId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.StokHareket)
                .WithMany()
                .HasForeignKey(e => e.StokHareketId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ServisKaydi
        modelBuilder.Entity<ServisKaydi>(entity =>
        {
            entity.HasIndex(e => new { e.AracId, e.ServisTarihi });
            entity.Property(e => e.ServisAdi).HasMaxLength(200);
            entity.Property(e => e.IscilikTutari).HasPrecision(18, 2);
            entity.Property(e => e.ParcaTutari).HasPrecision(18, 2);
            entity.Property(e => e.KdvOrani).HasPrecision(5, 2);
            entity.Property(e => e.KdvTutar).HasPrecision(18, 2);
            entity.Property(e => e.ToplamTutar).HasPrecision(18, 2);
            entity.HasOne(e => e.Arac)
                .WithMany()
                .HasForeignKey(e => e.AracId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ServisciCari)
                .WithMany()
                .HasForeignKey(e => e.ServisciCariId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Fatura)
                .WithMany()
                .HasForeignKey(e => e.FaturaId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.AracMasraf)
                .WithMany()
                .HasForeignKey(e => e.AracMasrafId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.StokHareket)
                .WithMany()
                .HasForeignKey(e => e.StokHareketId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ServisParca
        modelBuilder.Entity<ServisParca>(entity =>
        {
            entity.Property(e => e.ParcaAdi).HasMaxLength(200);
            entity.Property(e => e.Birim).HasMaxLength(20);
            entity.Property(e => e.Miktar).HasPrecision(18, 4);
            entity.Property(e => e.BirimFiyat).HasPrecision(18, 2);
            entity.HasOne(e => e.ServisKaydi)
                .WithMany(s => s.Parcalar)
                .HasForeignKey(e => e.ServisKaydiId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.StokKarti)
                .WithMany()
                .HasForeignKey(e => e.StokKartiId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<AylikOdemePlani>()
            .HasQueryFilter(e => !e.IsDeleted && (e.Firma == null || !e.Firma.IsDeleted));

        modelBuilder.Entity<AylikOdemeGerceklesen>()
            .HasQueryFilter(e => !e.IsDeleted && (e.Firma == null || !e.Firma.IsDeleted));

        modelBuilder.Entity<BordroDetay>()
            .HasQueryFilter(e => !e.IsDeleted && !e.Personel.IsDeleted);

        modelBuilder.Entity<DashboardWidget>()
            .HasQueryFilter(e => !e.IsDeleted && !e.Kullanici.IsDeleted);

        modelBuilder.Entity<FiloGuzergahEslestirme>()
            .HasQueryFilter(e => !e.IsDeleted && (e.Arac == null || !e.Arac.IsDeleted));

        modelBuilder.Entity<FiloGunlukPuantaj>()
            .HasQueryFilter(e => !e.IsDeleted && (e.Arac == null || !e.Arac.IsDeleted));

        modelBuilder.Entity<KiralamaArac>()
            .HasQueryFilter(e => !e.IsDeleted && (e.Firma == null || !e.Firma.IsDeleted));

        modelBuilder.Entity<KullaniciCari>()
            .HasQueryFilter(e => !e.IsDeleted && !e.Cari.IsDeleted);

        modelBuilder.Entity<PersonelAvans>()
            .HasQueryFilter(e => !e.IsDeleted && !e.Personel.IsDeleted);

        modelBuilder.Entity<PersonelBorc>()
            .HasQueryFilter(e => !e.IsDeleted && !e.Personel.IsDeleted);

        modelBuilder.Entity<PersonelOzlukEvrak>()
            .HasQueryFilter(e => !e.IsDeleted && !e.Sofor.IsDeleted);

        modelBuilder.Entity<PersonelPuantaj>()
            .HasQueryFilter(e => !e.IsDeleted && (e.Firma == null || !e.Firma.IsDeleted));

        modelBuilder.Entity<ServisCalismaKiralama>()
            .HasQueryFilter(e => !e.IsDeleted && (e.Firma == null || !e.Firma.IsDeleted));

        modelBuilder.Entity<BordroOdeme>()
            .HasQueryFilter(e => !e.IsDeleted && !e.BordroDetay.IsDeleted);

        modelBuilder.Entity<GunlukPuantaj>()
            .HasQueryFilter(e => !e.IsDeleted && (e.PersonelPuantaj == null || !e.PersonelPuantaj.IsDeleted));

        modelBuilder.Entity<PersonelAvansMahsup>()
            .HasQueryFilter(e => !e.IsDeleted && !e.Avans.IsDeleted);

        modelBuilder.Entity<PersonelBorcOdeme>()
            .HasQueryFilter(e => !e.IsDeleted && !e.Borc.IsDeleted);

        // Proforma Fatura
        modelBuilder.Entity<ProformaFatura>(entity =>
        {
            entity.HasIndex(e => e.ProformaNo).IsUnique();
            entity.Property(e => e.ProformaNo).HasMaxLength(50);
            entity.Property(e => e.AraToplam).HasPrecision(18, 2);
            entity.Property(e => e.IskontoTutar).HasPrecision(18, 2);
            entity.Property(e => e.IskontoOrani).HasPrecision(5, 2);
            entity.Property(e => e.KdvOrani).HasPrecision(5, 2);
            entity.Property(e => e.KdvTutar).HasPrecision(18, 2);
            entity.Property(e => e.GenelToplam).HasPrecision(18, 2);
            entity.Property(e => e.OdemeKosulu).HasMaxLength(100);
            entity.Property(e => e.TeslimKosulu).HasMaxLength(100);
            entity.Property(e => e.IlgiliKisi).HasMaxLength(100);
            entity.Property(e => e.Telefon).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.HasOne(e => e.Cari)
                .WithMany()
                .HasForeignKey(e => e.CariId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Firma)
                .WithMany()
                .HasForeignKey(e => e.FirmaId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Fatura)
                .WithMany()
                .HasForeignKey(e => e.FaturaId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<ProformaFaturaKalem>(entity =>
        {
            entity.Property(e => e.UrunAdi).HasMaxLength(250);
            entity.Property(e => e.UrunKodu).HasMaxLength(50);
            entity.Property(e => e.Birim).HasMaxLength(20);
            entity.Property(e => e.Miktar).HasPrecision(18, 4);
            entity.Property(e => e.BirimFiyat).HasPrecision(18, 2);
            entity.Property(e => e.IskontoOrani).HasPrecision(5, 2);
            entity.Property(e => e.IskontoTutar).HasPrecision(18, 2);
            entity.Property(e => e.KdvOrani).HasPrecision(5, 2);
            entity.Property(e => e.KdvTutar).HasPrecision(18, 2);
            entity.Property(e => e.AraToplam).HasPrecision(18, 2);
            entity.Property(e => e.NetTutar).HasPrecision(18, 2);
            entity.Property(e => e.ToplamTutar).HasPrecision(18, 2);
            entity.HasOne(e => e.ProformaFatura)
                .WithMany(p => p.Kalemler)
                .HasForeignKey(e => e.ProformaFaturaId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.StokKarti)
                .WithMany()
                .HasForeignKey(e => e.StokKartiId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    public override int SaveChanges()
    {
        ConvertDatesToUtc();
        UpdateTimestamps();
        GenerateAuditLogs();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ConvertDatesToUtc();
        UpdateTimestamps();
        GenerateAuditLogs();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void GenerateAuditLogs()
    {
        var modifiedEntities = ChangeTracker.Entries()
            .Where(x => (x.State == EntityState.Added || x.State == EntityState.Modified || x.State == EntityState.Deleted) && !(x.Entity is AktiviteLog))
            .ToList();

        foreach (var entry in modifiedEntities)
        {
            var entityType = entry.Entity.GetType();
            // Skip logging for system/identity entities if needed, e.g.
            if (entityType.Name.Contains("AktiviteLog") || entityType.Name.Contains("Log")) continue;

            var log = new AktiviteLog
            {
                IslemZamani = DateTime.UtcNow,
                IslemTipi = entry.State.ToString(),
                EntityTipi = entityType.Name,
                Modul = "Genel" // Default, could be mapped based on type
            };

            try
            {
                var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
                if (idProperty != null && idProperty.CurrentValue != null && entry.State != EntityState.Added)
                {
                    log.EntityId = (int?)Convert.ChangeType(idProperty.CurrentValue, typeof(int));
                }

                if (entry.State == EntityState.Modified)
                {
                    var originalValues = new System.Collections.Generic.Dictionary<string, object?>();
                    var currentValues = new System.Collections.Generic.Dictionary<string, object?>();

                    foreach (var property in entry.Properties)
                    {
                        if (property.IsModified)
                        {
                            originalValues[property.Metadata.Name] = property.OriginalValue;
                            currentValues[property.Metadata.Name] = property.CurrentValue;
                        }
                    }

                    log.EskiDeger = System.Text.Json.JsonSerializer.Serialize(originalValues);
                    log.YeniDeger = System.Text.Json.JsonSerializer.Serialize(currentValues);
                    log.Aciklama = $"{entityType.Name} kaydı güncellendi.";
                }
                else if (entry.State == EntityState.Added)
                {
                    log.Aciklama = $"{entityType.Name} kaydı eklendi.";
                    // We don't have the ID yet for Added entities, it will be generated after SaveChanges.
                    // A proper implementation would run a second pass after SaveChanges.
                }
                else if (entry.State == EntityState.Deleted)
                {
                    log.Aciklama = $"{entityType.Name} kaydı silindi.";
                }

                AktiviteLoglar.Add(log);
            }
            catch { /* Ignore logging errors */ }
        }
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
