using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMFiloServis.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AktiviteLoglar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IslemZamani = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IslemTipi = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Modul = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EntityTipi = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    EntityId = table.Column<int>(type: "INTEGER", nullable: true),
                    EntityAdi = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    EskiDeger = table.Column<string>(type: "TEXT", nullable: true),
                    YeniDeger = table.Column<string>(type: "TEXT", nullable: true),
                    KullaniciAdi = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IpAdresi = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Tarayici = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Seviye = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AktiviteLoglar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AracMarkalari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MarkaAdi = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Logo = table.Column<string>(type: "TEXT", nullable: true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AracMarkalari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BankaHesaplari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HesapKodu = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    HesapAdi = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    HesapTipi = table.Column<int>(type: "INTEGER", nullable: false),
                    BankaAdi = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SubeAdi = table.Column<string>(type: "TEXT", nullable: true),
                    SubeKodu = table.Column<string>(type: "TEXT", nullable: true),
                    HesapNo = table.Column<string>(type: "TEXT", nullable: true),
                    Iban = table.Column<string>(type: "TEXT", maxLength: 34, nullable: true),
                    ParaBirimi = table.Column<string>(type: "TEXT", maxLength: 3, nullable: true),
                    AcilisBakiye = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notlar = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankaHesaplari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BudgetMasrafKalemleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KalemAdi = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Kategori = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Renk = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Icon = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetMasrafKalemleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Firmalar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirmaKodu = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    FirmaAdi = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    UnvanTam = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    VergiNo = table.Column<string>(type: "TEXT", maxLength: 11, nullable: true),
                    VergiDairesi = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Adres = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Il = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Ilce = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Telefon = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    WebSite = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Logo = table.Column<string>(type: "TEXT", nullable: true),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false),
                    VarsayilanFirma = table.Column<bool>(type: "INTEGER", nullable: false),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifDonemYil = table.Column<int>(type: "INTEGER", nullable: false),
                    AktifDonemAy = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Firmalar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lisanslar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LisansAnahtari = table.Column<string>(type: "TEXT", nullable: false),
                    Tur = table.Column<int>(type: "INTEGER", nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FirmaAdi = table.Column<string>(type: "TEXT", nullable: true),
                    YetkiliKisi = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Telefon = table.Column<string>(type: "TEXT", nullable: true),
                    MakineKodu = table.Column<string>(type: "TEXT", nullable: false),
                    MaxKullaniciSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    ExcelExportIzni = table.Column<bool>(type: "INTEGER", nullable: false),
                    PdfExportIzni = table.Column<bool>(type: "INTEGER", nullable: false),
                    RaporlamaIzni = table.Column<bool>(type: "INTEGER", nullable: false),
                    YedeklemeIzni = table.Column<bool>(type: "INTEGER", nullable: false),
                    MuhasebeIzni = table.Column<bool>(type: "INTEGER", nullable: false),
                    SatisModuluIzni = table.Column<bool>(type: "INTEGER", nullable: false),
                    Imza = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lisanslar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MasrafKalemleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MasrafKodu = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MasrafAdi = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Kategori = table.Column<int>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notlar = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasrafKalemleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MuhasebeDonemleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Yil = table.Column<int>(type: "INTEGER", nullable: false),
                    Ay = table.Column<int>(type: "INTEGER", nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Durum = table.Column<int>(type: "INTEGER", nullable: false),
                    KapanisTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuhasebeDonemleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MuhasebeFisleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FisNo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    FisTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FisTipi = table.Column<int>(type: "INTEGER", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    ToplamBorc = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ToplamAlacak = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Durum = table.Column<int>(type: "INTEGER", nullable: false),
                    Kaynak = table.Column<int>(type: "INTEGER", nullable: false),
                    KaynakId = table.Column<int>(type: "INTEGER", nullable: true),
                    KaynakTip = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuhasebeFisleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MuhasebeHesaplari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HesapKodu = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    HesapAdi = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    HesapTuru = table.Column<int>(type: "INTEGER", nullable: false),
                    HesapGrubu = table.Column<int>(type: "INTEGER", nullable: false),
                    UstHesapId = table.Column<int>(type: "INTEGER", nullable: true),
                    AltHesapVar = table.Column<bool>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false),
                    SistemHesabi = table.Column<bool>(type: "INTEGER", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuhasebeHesaplari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MuhasebeHesaplari_MuhasebeHesaplari_UstHesapId",
                        column: x => x.UstHesapId,
                        principalTable: "MuhasebeHesaplari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Roller",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RolAdi = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    SistemRolu = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roller", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SatisPersonelleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PersonelKodu = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    AdSoyad = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Telefon = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false),
                    KomisyonOrani = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    SabitKomisyon = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    AylikSatisHedefi = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    AylikAracHedefi = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatisPersonelleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Soforler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SoforKodu = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Ad = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Soyad = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TcKimlikNo = table.Column<string>(type: "TEXT", maxLength: 11, nullable: true),
                    Telefon = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Adres = table.Column<string>(type: "TEXT", nullable: true),
                    Gorev = table.Column<int>(type: "INTEGER", nullable: false),
                    Departman = table.Column<string>(type: "TEXT", nullable: true),
                    Pozisyon = table.Column<string>(type: "TEXT", nullable: true),
                    EhliyetNo = table.Column<string>(type: "TEXT", nullable: true),
                    EhliyetGecerlilikTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SrcBelgesiGecerlilikTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PsikoteknikGecerlilikTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SaglikRaporuGecerlilikTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IseBaslamaTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IstenAyrilmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notlar = table.Column<string>(type: "TEXT", nullable: true),
                    BrutMaas = table.Column<decimal>(type: "TEXT", nullable: false),
                    NetMaas = table.Column<decimal>(type: "TEXT", nullable: false),
                    BankaAdi = table.Column<string>(type: "TEXT", nullable: true),
                    IBAN = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Soforler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AracModelleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MarkaId = table.Column<int>(type: "INTEGER", nullable: false),
                    ModelAdi = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    BaslangicYili = table.Column<int>(type: "INTEGER", nullable: false),
                    BitisYili = table.Column<int>(type: "INTEGER", nullable: true),
                    VarsayilanKasaTipi = table.Column<int>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AracModelleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AracModelleri_AracMarkalari_MarkaId",
                        column: x => x.MarkaId,
                        principalTable: "AracMarkalari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cariler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CariKodu = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Unvan = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    CariTipi = table.Column<int>(type: "INTEGER", nullable: false),
                    VergiDairesi = table.Column<string>(type: "TEXT", nullable: true),
                    VergiNo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    TcKimlikNo = table.Column<string>(type: "TEXT", maxLength: 11, nullable: true),
                    Adres = table.Column<string>(type: "TEXT", nullable: true),
                    Telefon = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    YetkiliKisi = table.Column<string>(type: "TEXT", nullable: true),
                    Notlar = table.Column<string>(type: "TEXT", nullable: true),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false),
                    Borc = table.Column<decimal>(type: "TEXT", nullable: false),
                    Alacak = table.Column<decimal>(type: "TEXT", nullable: false),
                    MuhasebeHesapId = table.Column<int>(type: "INTEGER", nullable: true),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cariler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cariler_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Cariler_MuhasebeHesaplari_MuhasebeHesapId",
                        column: x => x.MuhasebeHesapId,
                        principalTable: "MuhasebeHesaplari",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RolYetkileri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RolId = table.Column<int>(type: "INTEGER", nullable: false),
                    YetkiKodu = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Izin = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolYetkileri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolYetkileri_Roller_RolId",
                        column: x => x.RolId,
                        principalTable: "Roller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Kullanicilar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KullaniciAdi = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SifreHash = table.Column<string>(type: "TEXT", nullable: false),
                    AdSoyad = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Telefon = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    SoforId = table.Column<int>(type: "INTEGER", nullable: true),
                    RolId = table.Column<int>(type: "INTEGER", nullable: false),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false),
                    SonGirisTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    BasarisizGirisSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    Kilitli = table.Column<bool>(type: "INTEGER", nullable: false),
                    Tema = table.Column<string>(type: "TEXT", nullable: false),
                    KompaktMod = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanicilar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kullanicilar_Roller_RolId",
                        column: x => x.RolId,
                        principalTable: "Roller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Kullanicilar_Soforler_SoforId",
                        column: x => x.SoforId,
                        principalTable: "Soforler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PersonelIzinHaklari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SoforId = table.Column<int>(type: "INTEGER", nullable: false),
                    Yil = table.Column<int>(type: "INTEGER", nullable: false),
                    YillikIzinHakki = table.Column<int>(type: "INTEGER", nullable: false),
                    KullanilanIzin = table.Column<int>(type: "INTEGER", nullable: false),
                    DevirenIzin = table.Column<int>(type: "INTEGER", nullable: false),
                    Notlar = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonelIzinHaklari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonelIzinHaklari_Soforler_SoforId",
                        column: x => x.SoforId,
                        principalTable: "Soforler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PersonelIzinleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SoforId = table.Column<int>(type: "INTEGER", nullable: false),
                    IzinTipi = table.Column<int>(type: "INTEGER", nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Durum = table.Column<int>(type: "INTEGER", nullable: false),
                    OnaylayanKisi = table.Column<string>(type: "TEXT", nullable: true),
                    OnayTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RedNedeni = table.Column<string>(type: "TEXT", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    Notlar = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonelIzinleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonelIzinleri_Soforler_SoforId",
                        column: x => x.SoforId,
                        principalTable: "Soforler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PersonelMaaslari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SoforId = table.Column<int>(type: "INTEGER", nullable: false),
                    Yil = table.Column<int>(type: "INTEGER", nullable: false),
                    Ay = table.Column<int>(type: "INTEGER", nullable: false),
                    BrutMaas = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    NetMaas = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    SGKIsciPayi = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    SGKIsverenPayi = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    GelirVergisi = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    DamgaVergisi = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    IssizlikPrimi = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Prim = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Ikramiye = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Yemek = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Yol = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Mesai = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    DigerEklemeler = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Avans = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    IcraTakibi = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    DigerKesintiler = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    OdemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OdemeDurum = table.Column<int>(type: "INTEGER", nullable: false),
                    OdemeAciklama = table.Column<string>(type: "TEXT", nullable: true),
                    CalismaGunu = table.Column<int>(type: "INTEGER", nullable: false),
                    IzinliGun = table.Column<int>(type: "INTEGER", nullable: false),
                    RaporluGun = table.Column<int>(type: "INTEGER", nullable: false),
                    DevamsizlikGun = table.Column<int>(type: "INTEGER", nullable: false),
                    Notlar = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonelMaaslari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonelMaaslari_Soforler_SoforId",
                        column: x => x.SoforId,
                        principalTable: "Soforler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PersonelPuantajlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: false),
                    PersonelId = table.Column<int>(type: "INTEGER", nullable: false),
                    Yil = table.Column<int>(type: "INTEGER", nullable: false),
                    Ay = table.Column<int>(type: "INTEGER", nullable: false),
                    CalisilanGun = table.Column<int>(type: "INTEGER", nullable: false),
                    FazlaMesaiSaat = table.Column<decimal>(type: "TEXT", nullable: false),
                    IzinGunu = table.Column<int>(type: "INTEGER", nullable: false),
                    MazeretGunu = table.Column<int>(type: "INTEGER", nullable: false),
                    BrutMaas = table.Column<decimal>(type: "TEXT", nullable: false),
                    YemekUcreti = table.Column<decimal>(type: "TEXT", nullable: false),
                    YolUcreti = table.Column<decimal>(type: "TEXT", nullable: false),
                    Prim = table.Column<decimal>(type: "TEXT", nullable: false),
                    DigerOdeme = table.Column<decimal>(type: "TEXT", nullable: false),
                    SgkKesinti = table.Column<decimal>(type: "TEXT", nullable: false),
                    GelirVergisi = table.Column<decimal>(type: "TEXT", nullable: false),
                    DamgaVergisi = table.Column<decimal>(type: "TEXT", nullable: false),
                    DigerKesinti = table.Column<decimal>(type: "TEXT", nullable: false),
                    NetOdeme = table.Column<decimal>(type: "TEXT", nullable: false),
                    OdemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Odendi = table.Column<bool>(type: "INTEGER", nullable: false),
                    BankaHesapNo = table.Column<string>(type: "TEXT", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonelPuantajlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonelPuantajlar_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonelPuantajlar_Soforler_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "Soforler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AracIlanlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Plaka = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false),
                    Marka = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Model = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ModelYili = table.Column<int>(type: "INTEGER", nullable: false),
                    Versiyon = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Kilometre = table.Column<int>(type: "INTEGER", nullable: false),
                    YakitTuru = table.Column<int>(type: "INTEGER", nullable: false),
                    VitesTuru = table.Column<int>(type: "INTEGER", nullable: false),
                    KasaTipi = table.Column<int>(type: "INTEGER", nullable: false),
                    Renk = table.Column<int>(type: "INTEGER", nullable: false),
                    Durum = table.Column<int>(type: "INTEGER", nullable: false),
                    Boyali = table.Column<bool>(type: "INTEGER", nullable: false),
                    BoyaliParcaSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    BoyaliParcalar = table.Column<string>(type: "TEXT", nullable: true),
                    DegisenVar = table.Column<bool>(type: "INTEGER", nullable: false),
                    DegisenParcaSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    DegisenParcalar = table.Column<string>(type: "TEXT", nullable: true),
                    HasarKaydi = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasarAciklama = table.Column<string>(type: "TEXT", nullable: true),
                    TramerKaydi = table.Column<bool>(type: "INTEGER", nullable: false),
                    TramerTutari = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    AlisFiyati = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    SatisFiyati = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    KaskoDegeri = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    PiyasaDegeriMin = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    PiyasaDegeriMax = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    PiyasaDegeriOrtalama = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    IlanDurum = table.Column<int>(type: "INTEGER", nullable: false),
                    IlanTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SatisTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    Notlar = table.Column<string>(type: "TEXT", nullable: true),
                    Fotograflar = table.Column<string>(type: "TEXT", nullable: true),
                    SahipCariId = table.Column<int>(type: "INTEGER", nullable: true),
                    SatisPersoneliId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AracIlanlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AracIlanlari_Cariler_SahipCariId",
                        column: x => x.SahipCariId,
                        principalTable: "Cariler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AracIlanlari_SatisPersonelleri_SatisPersoneliId",
                        column: x => x.SatisPersoneliId,
                        principalTable: "SatisPersonelleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Araclar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Plaka = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false),
                    Marka = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Model = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ModelYili = table.Column<int>(type: "INTEGER", nullable: true),
                    SaseNo = table.Column<string>(type: "TEXT", nullable: true),
                    MotorNo = table.Column<string>(type: "TEXT", nullable: true),
                    Renk = table.Column<string>(type: "TEXT", nullable: true),
                    KoltukSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    AracTipi = table.Column<int>(type: "INTEGER", nullable: false),
                    SahiplikTipi = table.Column<int>(type: "INTEGER", nullable: false),
                    KiralikCariId = table.Column<int>(type: "INTEGER", nullable: true),
                    GunlukKiraBedeli = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    AylikKiraBedeli = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    SeferBasinaKiraBedeli = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    KiraHesaplamaTipi = table.Column<int>(type: "INTEGER", nullable: true),
                    KomisyonVar = table.Column<bool>(type: "INTEGER", nullable: false),
                    KomisyoncuCariId = table.Column<int>(type: "INTEGER", nullable: true),
                    KomisyonOrani = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: true),
                    SabitKomisyonTutari = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    KomisyonHesaplamaTipi = table.Column<int>(type: "INTEGER", nullable: true),
                    TrafikSigortaBitisTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    KaskoBitisTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MuayeneBitisTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    KmDurumu = table.Column<int>(type: "INTEGER", nullable: true),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notlar = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Araclar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Araclar_Cariler_KiralikCariId",
                        column: x => x.KiralikCariId,
                        principalTable: "Cariler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Araclar_Cariler_KomisyoncuCariId",
                        column: x => x.KomisyoncuCariId,
                        principalTable: "Cariler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AylikOdemePlanlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: false),
                    OdemeAdi = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Turu = table.Column<int>(type: "INTEGER", nullable: false),
                    AylikTutar = table.Column<decimal>(type: "TEXT", nullable: false),
                    OdemeGunu = table.Column<int>(type: "INTEGER", nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OtomatikKayitOlustur = table.Column<bool>(type: "INTEGER", nullable: false),
                    CariId = table.Column<int>(type: "INTEGER", nullable: true),
                    BankaHesapId = table.Column<int>(type: "INTEGER", nullable: true),
                    MasrafKalemiId = table.Column<int>(type: "INTEGER", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AylikOdemePlanlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AylikOdemePlanlari_BankaHesaplari_BankaHesapId",
                        column: x => x.BankaHesapId,
                        principalTable: "BankaHesaplari",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AylikOdemePlanlari_Cariler_CariId",
                        column: x => x.CariId,
                        principalTable: "Cariler",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AylikOdemePlanlari_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AylikOdemePlanlari_MasrafKalemleri_MasrafKalemiId",
                        column: x => x.MasrafKalemiId,
                        principalTable: "MasrafKalemleri",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BankaKasaHareketleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IslemNo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IslemTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HareketTipi = table.Column<int>(type: "INTEGER", nullable: false),
                    Tutar = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    BelgeNo = table.Column<string>(type: "TEXT", nullable: true),
                    IslemKaynak = table.Column<int>(type: "INTEGER", nullable: false),
                    BankaHesapId = table.Column<int>(type: "INTEGER", nullable: false),
                    CariId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankaKasaHareketleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankaKasaHareketleri_BankaHesaplari_BankaHesapId",
                        column: x => x.BankaHesapId,
                        principalTable: "BankaHesaplari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BankaKasaHareketleri_Cariler_CariId",
                        column: x => x.CariId,
                        principalTable: "Cariler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Faturalar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FaturaNo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    FaturaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    VadeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FaturaTipi = table.Column<int>(type: "INTEGER", nullable: false),
                    Durum = table.Column<int>(type: "INTEGER", nullable: false),
                    EFaturaTipi = table.Column<int>(type: "INTEGER", nullable: false),
                    FaturaYonu = table.Column<int>(type: "INTEGER", nullable: false),
                    EttnNo = table.Column<string>(type: "TEXT", nullable: true),
                    GibKodu = table.Column<string>(type: "TEXT", nullable: true),
                    GibOnayTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ImportKaynak = table.Column<string>(type: "TEXT", nullable: true),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: true),
                    AraToplam = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    IskontoTutar = table.Column<decimal>(type: "TEXT", nullable: false),
                    KdvOrani = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    KdvTutar = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    GenelToplam = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    OdenenTutar = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    Notlar = table.Column<string>(type: "TEXT", nullable: true),
                    CariId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faturalar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Faturalar_Cariler_CariId",
                        column: x => x.CariId,
                        principalTable: "Cariler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Faturalar_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Guzergahlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GuzergahKodu = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    GuzergahAdi = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    BaslangicNoktasi = table.Column<string>(type: "TEXT", nullable: true),
                    BitisNoktasi = table.Column<string>(type: "TEXT", nullable: true),
                    BirimFiyat = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Mesafe = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true),
                    TahminiSure = table.Column<int>(type: "INTEGER", nullable: true),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notlar = table.Column<string>(type: "TEXT", nullable: true),
                    CariId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guzergahlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Guzergahlar_Cariler_CariId",
                        column: x => x.CariId,
                        principalTable: "Cariler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KiralamaAraclar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: false),
                    KiralayıcıCariId = table.Column<int>(type: "INTEGER", nullable: false),
                    Plaka = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false),
                    Marka = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Model = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ModelYili = table.Column<int>(type: "INTEGER", nullable: true),
                    AracTipi = table.Column<int>(type: "INTEGER", nullable: false),
                    KoltukSayisi = table.Column<int>(type: "INTEGER", nullable: true),
                    KiralamaBaslangic = table.Column<DateTime>(type: "TEXT", nullable: false),
                    KiralamaBitis = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GunlukKiraBedeli = table.Column<decimal>(type: "TEXT", nullable: true),
                    SeferBasinaKiraBedeli = table.Column<decimal>(type: "TEXT", nullable: true),
                    AylikKiraBedeli = table.Column<decimal>(type: "TEXT", nullable: true),
                    KomisyonOrani = table.Column<decimal>(type: "TEXT", nullable: true),
                    SabitKomisyonTutari = table.Column<decimal>(type: "TEXT", nullable: true),
                    SozlesmeNo = table.Column<string>(type: "TEXT", nullable: true),
                    Notlar = table.Column<string>(type: "TEXT", nullable: true),
                    Aktif = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KiralamaAraclar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KiralamaAraclar_Cariler_KiralayıcıCariId",
                        column: x => x.KiralayıcıCariId,
                        principalTable: "Cariler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KiralamaAraclar_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MuhasebeFisKalemleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FisId = table.Column<int>(type: "INTEGER", nullable: false),
                    HesapId = table.Column<int>(type: "INTEGER", nullable: false),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    Borc = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Alacak = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    CariId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuhasebeFisKalemleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MuhasebeFisKalemleri_Cariler_CariId",
                        column: x => x.CariId,
                        principalTable: "Cariler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MuhasebeFisKalemleri_MuhasebeFisleri_FisId",
                        column: x => x.FisId,
                        principalTable: "MuhasebeFisleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MuhasebeFisKalemleri_MuhasebeHesaplari_HesapId",
                        column: x => x.HesapId,
                        principalTable: "MuhasebeHesaplari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AracSatislari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AracIlanId = table.Column<int>(type: "INTEGER", nullable: false),
                    AliciCariId = table.Column<int>(type: "INTEGER", nullable: true),
                    SatisPersoneliId = table.Column<int>(type: "INTEGER", nullable: true),
                    SatisTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SatisFiyati = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    KomisyonTutari = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    OdemeSekli = table.Column<int>(type: "INTEGER", nullable: false),
                    Notlar = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AracSatislari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AracSatislari_AracIlanlari_AracIlanId",
                        column: x => x.AracIlanId,
                        principalTable: "AracIlanlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AracSatislari_Cariler_AliciCariId",
                        column: x => x.AliciCariId,
                        principalTable: "Cariler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AracSatislari_SatisPersonelleri_SatisPersoneliId",
                        column: x => x.SatisPersoneliId,
                        principalTable: "SatisPersonelleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PiyasaIlanlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AracIlanId = table.Column<int>(type: "INTEGER", nullable: false),
                    Kaynak = table.Column<int>(type: "INTEGER", nullable: false),
                    IlanUrl = table.Column<string>(type: "TEXT", nullable: true),
                    IlanNo = table.Column<string>(type: "TEXT", nullable: true),
                    Fiyat = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Sehir = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Ilce = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Kilometre = table.Column<int>(type: "INTEGER", nullable: false),
                    Yil = table.Column<int>(type: "INTEGER", nullable: false),
                    Durum = table.Column<string>(type: "TEXT", nullable: true),
                    BoyaliParca = table.Column<int>(type: "INTEGER", nullable: false),
                    DegisenParca = table.Column<int>(type: "INTEGER", nullable: false),
                    TramerVar = table.Column<bool>(type: "INTEGER", nullable: false),
                    TramerTutari = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    TaramaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EkBilgiler = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PiyasaIlanlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PiyasaIlanlari_AracIlanlari_AracIlanId",
                        column: x => x.AracIlanId,
                        principalTable: "AracIlanlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AracEvraklari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AracId = table.Column<int>(type: "INTEGER", nullable: false),
                    EvrakKategorisi = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EvrakAdi = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    BaslangicTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    BitisTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    HatirlatmaTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Tutar = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    SigortaSirketi = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PoliceNo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Durum = table.Column<int>(type: "INTEGER", nullable: false),
                    HatirlatmaAktif = table.Column<bool>(type: "INTEGER", nullable: false),
                    HatirlatmaGunOnce = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AracEvraklari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AracEvraklari_Araclar_AracId",
                        column: x => x.AracId,
                        principalTable: "Araclar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AylikOdemeGerceklesenler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AylikOdemePlaniId = table.Column<int>(type: "INTEGER", nullable: false),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Yil = table.Column<int>(type: "INTEGER", nullable: false),
                    Ay = table.Column<int>(type: "INTEGER", nullable: false),
                    PlanlananTutar = table.Column<decimal>(type: "TEXT", nullable: false),
                    OdenenTutar = table.Column<decimal>(type: "TEXT", nullable: false),
                    OdemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    BankaKasaHareketId = table.Column<int>(type: "INTEGER", nullable: true),
                    Durum = table.Column<int>(type: "INTEGER", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AylikOdemeGerceklesenler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AylikOdemeGerceklesenler_AylikOdemePlanlari_AylikOdemePlaniId",
                        column: x => x.AylikOdemePlaniId,
                        principalTable: "AylikOdemePlanlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AylikOdemeGerceklesenler_BankaKasaHareketleri_BankaKasaHareketId",
                        column: x => x.BankaKasaHareketId,
                        principalTable: "BankaKasaHareketleri",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AylikOdemeGerceklesenler_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BudgetOdemeler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OdemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OdemeAy = table.Column<int>(type: "INTEGER", nullable: false),
                    OdemeYil = table.Column<int>(type: "INTEGER", nullable: false),
                    MasrafKalemi = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Miktar = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: true),
                    TaksitliMi = table.Column<bool>(type: "INTEGER", nullable: false),
                    ToplamTaksitSayisi = table.Column<int>(type: "INTEGER", nullable: false),
                    KacinciTaksit = table.Column<int>(type: "INTEGER", nullable: false),
                    TaksitGrupId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TaksitBaslangicAy = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TaksitBitisAy = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Durum = table.Column<int>(type: "INTEGER", nullable: false),
                    Notlar = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    GercekOdemeTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OdemeYapildigiHesapId = table.Column<int>(type: "INTEGER", nullable: true),
                    OdenenTutar = table.Column<decimal>(type: "TEXT", nullable: true),
                    OdemeNotu = table.Column<string>(type: "TEXT", nullable: true),
                    BankaKasaHareketId = table.Column<int>(type: "INTEGER", nullable: true),
                    FaturaId = table.Column<int>(type: "INTEGER", nullable: true),
                    FaturaIleKapatildi = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetOdemeler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetOdemeler_BankaHesaplari_OdemeYapildigiHesapId",
                        column: x => x.OdemeYapildigiHesapId,
                        principalTable: "BankaHesaplari",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BudgetOdemeler_Faturalar_FaturaId",
                        column: x => x.FaturaId,
                        principalTable: "Faturalar",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BudgetOdemeler_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FaturaKalemleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: false),
                    Miktar = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    Birim = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    KdvOrani = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    KdvTutar = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ToplamTutar = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    FaturaId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaturaKalemleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaturaKalemleri_Faturalar_FaturaId",
                        column: x => x.FaturaId,
                        principalTable: "Faturalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OdemeEslestirmeleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EslestirmeTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EslestirilenTutar = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    FaturaId = table.Column<int>(type: "INTEGER", nullable: false),
                    BankaKasaHareketId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OdemeEslestirmeleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OdemeEslestirmeleri_BankaKasaHareketleri_BankaKasaHareketId",
                        column: x => x.BankaKasaHareketId,
                        principalTable: "BankaKasaHareketleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OdemeEslestirmeleri_Faturalar_FaturaId",
                        column: x => x.FaturaId,
                        principalTable: "Faturalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AylikChecklistler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Yil = table.Column<int>(type: "INTEGER", nullable: false),
                    Ay = table.Column<int>(type: "INTEGER", nullable: false),
                    ChecklistTipi = table.Column<int>(type: "INTEGER", nullable: false),
                    SoforId = table.Column<int>(type: "INTEGER", nullable: true),
                    AracId = table.Column<int>(type: "INTEGER", nullable: true),
                    GuzergahId = table.Column<int>(type: "INTEGER", nullable: true),
                    KontrolTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    KontrolEden = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    GenelDurum = table.Column<int>(type: "INTEGER", nullable: false),
                    Notlar = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AylikChecklistler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AylikChecklistler_Araclar_AracId",
                        column: x => x.AracId,
                        principalTable: "Araclar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AylikChecklistler_Guzergahlar_GuzergahId",
                        column: x => x.GuzergahId,
                        principalTable: "Guzergahlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AylikChecklistler_Soforler_SoforId",
                        column: x => x.SoforId,
                        principalTable: "Soforler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServisCalismalari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CalismaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ServisTuru = table.Column<int>(type: "INTEGER", nullable: false),
                    Fiyat = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    KmBaslangic = table.Column<int>(type: "INTEGER", nullable: true),
                    KmBitis = table.Column<int>(type: "INTEGER", nullable: true),
                    BaslangicSaati = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    BitisSaati = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    ArizaOlduMu = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArizaAciklamasi = table.Column<string>(type: "TEXT", nullable: true),
                    Durum = table.Column<int>(type: "INTEGER", nullable: false),
                    Notlar = table.Column<string>(type: "TEXT", nullable: true),
                    AracId = table.Column<int>(type: "INTEGER", nullable: false),
                    SoforId = table.Column<int>(type: "INTEGER", nullable: false),
                    GuzergahId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServisCalismalari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServisCalismalari_Araclar_AracId",
                        column: x => x.AracId,
                        principalTable: "Araclar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServisCalismalari_Guzergahlar_GuzergahId",
                        column: x => x.GuzergahId,
                        principalTable: "Guzergahlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServisCalismalari_Soforler_SoforId",
                        column: x => x.SoforId,
                        principalTable: "Soforler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServisCalismaKiralamalar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: false),
                    CalismaTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ServisTuru = table.Column<int>(type: "INTEGER", nullable: false),
                    AracSahiplikTuru = table.Column<int>(type: "INTEGER", nullable: false),
                    AracId = table.Column<int>(type: "INTEGER", nullable: true),
                    KiralamaAracId = table.Column<int>(type: "INTEGER", nullable: true),
                    SoforId = table.Column<int>(type: "INTEGER", nullable: false),
                    GuzergahId = table.Column<int>(type: "INTEGER", nullable: false),
                    MusteriFirmaId = table.Column<int>(type: "INTEGER", nullable: true),
                    CalismaBedeli = table.Column<decimal>(type: "TEXT", nullable: true),
                    AracKiraBedeli = table.Column<decimal>(type: "TEXT", nullable: true),
                    KomisyonTutari = table.Column<decimal>(type: "TEXT", nullable: true),
                    NetKazanc = table.Column<decimal>(type: "TEXT", nullable: true),
                    KmBaslangic = table.Column<int>(type: "INTEGER", nullable: true),
                    KmBitis = table.Column<int>(type: "INTEGER", nullable: true),
                    ToplamKm = table.Column<int>(type: "INTEGER", nullable: true),
                    BaslangicSaati = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    BitisSaati = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    ArizaOlduMu = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArizaAciklamasi = table.Column<string>(type: "TEXT", nullable: true),
                    Durum = table.Column<int>(type: "INTEGER", nullable: false),
                    Notlar = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServisCalismaKiralamalar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServisCalismaKiralamalar_Araclar_AracId",
                        column: x => x.AracId,
                        principalTable: "Araclar",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServisCalismaKiralamalar_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServisCalismaKiralamalar_Firmalar_MusteriFirmaId",
                        column: x => x.MusteriFirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServisCalismaKiralamalar_Guzergahlar_GuzergahId",
                        column: x => x.GuzergahId,
                        principalTable: "Guzergahlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServisCalismaKiralamalar_KiralamaAraclar_KiralamaAracId",
                        column: x => x.KiralamaAracId,
                        principalTable: "KiralamaAraclar",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServisCalismaKiralamalar_Soforler_SoforId",
                        column: x => x.SoforId,
                        principalTable: "Soforler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AracEvrakDosyalari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AracEvrakId = table.Column<int>(type: "INTEGER", nullable: false),
                    DosyaAdi = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    DosyaYolu = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    DosyaTipi = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    DosyaBoyutu = table.Column<long>(type: "INTEGER", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AracEvrakDosyalari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AracEvrakDosyalari_AracEvraklari_AracEvrakId",
                        column: x => x.AracEvrakId,
                        principalTable: "AracEvraklari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistKalemleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AylikChecklistId = table.Column<int>(type: "INTEGER", nullable: false),
                    KalemAdi = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Durum = table.Column<int>(type: "INTEGER", nullable: false),
                    SonGecerlilikTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    KontrolTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Aciklama = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    SiraNo = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistKalemleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistKalemleri_AylikChecklistler_AylikChecklistId",
                        column: x => x.AylikChecklistId,
                        principalTable: "AylikChecklistler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AracMasraflari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MasrafTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Tutar = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    BelgeNo = table.Column<string>(type: "TEXT", nullable: true),
                    ArizaKaynaklimi = table.Column<bool>(type: "INTEGER", nullable: false),
                    AracId = table.Column<int>(type: "INTEGER", nullable: false),
                    MasrafKalemiId = table.Column<int>(type: "INTEGER", nullable: false),
                    GuzergahId = table.Column<int>(type: "INTEGER", nullable: true),
                    ServisCalismaId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AracMasraflari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AracMasraflari_Araclar_AracId",
                        column: x => x.AracId,
                        principalTable: "Araclar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AracMasraflari_Guzergahlar_GuzergahId",
                        column: x => x.GuzergahId,
                        principalTable: "Guzergahlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AracMasraflari_MasrafKalemleri_MasrafKalemiId",
                        column: x => x.MasrafKalemiId,
                        principalTable: "MasrafKalemleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AracMasraflari_ServisCalismalari_ServisCalismaId",
                        column: x => x.ServisCalismaId,
                        principalTable: "ServisCalismalari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "GunlukPuantajlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PersonelPuantajId = table.Column<int>(type: "INTEGER", nullable: false),
                    Tarih = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Calisti = table.Column<bool>(type: "INTEGER", nullable: false),
                    FazlaMesaiSaat = table.Column<decimal>(type: "TEXT", nullable: true),
                    Izinli = table.Column<bool>(type: "INTEGER", nullable: false),
                    Mazeret = table.Column<bool>(type: "INTEGER", nullable: false),
                    ServisCalismaId = table.Column<int>(type: "INTEGER", nullable: true),
                    Notlar = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GunlukPuantajlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GunlukPuantajlar_PersonelPuantajlar_PersonelPuantajId",
                        column: x => x.PersonelPuantajId,
                        principalTable: "PersonelPuantajlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GunlukPuantajlar_ServisCalismalari_ServisCalismaId",
                        column: x => x.ServisCalismaId,
                        principalTable: "ServisCalismalari",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AktiviteLoglar_IslemZamani",
                table: "AktiviteLoglar",
                column: "IslemZamani");

            migrationBuilder.CreateIndex(
                name: "IX_AktiviteLoglar_Modul_IslemTipi",
                table: "AktiviteLoglar",
                columns: new[] { "Modul", "IslemTipi" });

            migrationBuilder.CreateIndex(
                name: "IX_AracEvrakDosyalari_AracEvrakId",
                table: "AracEvrakDosyalari",
                column: "AracEvrakId");

            migrationBuilder.CreateIndex(
                name: "IX_AracEvraklari_AracId_EvrakKategorisi",
                table: "AracEvraklari",
                columns: new[] { "AracId", "EvrakKategorisi" });

            migrationBuilder.CreateIndex(
                name: "IX_AracIlanlari_Plaka",
                table: "AracIlanlari",
                column: "Plaka");

            migrationBuilder.CreateIndex(
                name: "IX_AracIlanlari_SahipCariId",
                table: "AracIlanlari",
                column: "SahipCariId");

            migrationBuilder.CreateIndex(
                name: "IX_AracIlanlari_SatisPersoneliId",
                table: "AracIlanlari",
                column: "SatisPersoneliId");

            migrationBuilder.CreateIndex(
                name: "IX_Araclar_KiralikCariId",
                table: "Araclar",
                column: "KiralikCariId");

            migrationBuilder.CreateIndex(
                name: "IX_Araclar_KomisyoncuCariId",
                table: "Araclar",
                column: "KomisyoncuCariId");

            migrationBuilder.CreateIndex(
                name: "IX_Araclar_Plaka",
                table: "Araclar",
                column: "Plaka",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AracMarkalari_MarkaAdi",
                table: "AracMarkalari",
                column: "MarkaAdi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AracMasraflari_AracId",
                table: "AracMasraflari",
                column: "AracId");

            migrationBuilder.CreateIndex(
                name: "IX_AracMasraflari_GuzergahId",
                table: "AracMasraflari",
                column: "GuzergahId");

            migrationBuilder.CreateIndex(
                name: "IX_AracMasraflari_MasrafKalemiId",
                table: "AracMasraflari",
                column: "MasrafKalemiId");

            migrationBuilder.CreateIndex(
                name: "IX_AracMasraflari_ServisCalismaId",
                table: "AracMasraflari",
                column: "ServisCalismaId");

            migrationBuilder.CreateIndex(
                name: "IX_AracModelleri_MarkaId",
                table: "AracModelleri",
                column: "MarkaId");

            migrationBuilder.CreateIndex(
                name: "IX_AracSatislari_AliciCariId",
                table: "AracSatislari",
                column: "AliciCariId");

            migrationBuilder.CreateIndex(
                name: "IX_AracSatislari_AracIlanId",
                table: "AracSatislari",
                column: "AracIlanId");

            migrationBuilder.CreateIndex(
                name: "IX_AracSatislari_SatisPersoneliId",
                table: "AracSatislari",
                column: "SatisPersoneliId");

            migrationBuilder.CreateIndex(
                name: "IX_AylikChecklistler_AracId",
                table: "AylikChecklistler",
                column: "AracId");

            migrationBuilder.CreateIndex(
                name: "IX_AylikChecklistler_GuzergahId",
                table: "AylikChecklistler",
                column: "GuzergahId");

            migrationBuilder.CreateIndex(
                name: "IX_AylikChecklistler_SoforId",
                table: "AylikChecklistler",
                column: "SoforId");

            migrationBuilder.CreateIndex(
                name: "IX_AylikChecklistler_Yil_Ay_ChecklistTipi_SoforId_AracId_GuzergahId",
                table: "AylikChecklistler",
                columns: new[] { "Yil", "Ay", "ChecklistTipi", "SoforId", "AracId", "GuzergahId" });

            migrationBuilder.CreateIndex(
                name: "IX_AylikOdemeGerceklesenler_AylikOdemePlaniId",
                table: "AylikOdemeGerceklesenler",
                column: "AylikOdemePlaniId");

            migrationBuilder.CreateIndex(
                name: "IX_AylikOdemeGerceklesenler_BankaKasaHareketId",
                table: "AylikOdemeGerceklesenler",
                column: "BankaKasaHareketId");

            migrationBuilder.CreateIndex(
                name: "IX_AylikOdemeGerceklesenler_FirmaId",
                table: "AylikOdemeGerceklesenler",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_AylikOdemePlanlari_BankaHesapId",
                table: "AylikOdemePlanlari",
                column: "BankaHesapId");

            migrationBuilder.CreateIndex(
                name: "IX_AylikOdemePlanlari_CariId",
                table: "AylikOdemePlanlari",
                column: "CariId");

            migrationBuilder.CreateIndex(
                name: "IX_AylikOdemePlanlari_FirmaId",
                table: "AylikOdemePlanlari",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_AylikOdemePlanlari_MasrafKalemiId",
                table: "AylikOdemePlanlari",
                column: "MasrafKalemiId");

            migrationBuilder.CreateIndex(
                name: "IX_BankaHesaplari_HesapKodu",
                table: "BankaHesaplari",
                column: "HesapKodu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankaKasaHareketleri_BankaHesapId",
                table: "BankaKasaHareketleri",
                column: "BankaHesapId");

            migrationBuilder.CreateIndex(
                name: "IX_BankaKasaHareketleri_CariId",
                table: "BankaKasaHareketleri",
                column: "CariId");

            migrationBuilder.CreateIndex(
                name: "IX_BankaKasaHareketleri_IslemNo",
                table: "BankaKasaHareketleri",
                column: "IslemNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BudgetMasrafKalemleri_KalemAdi",
                table: "BudgetMasrafKalemleri",
                column: "KalemAdi");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetOdemeler_FaturaId",
                table: "BudgetOdemeler",
                column: "FaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetOdemeler_FirmaId",
                table: "BudgetOdemeler",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetOdemeler_OdemeYapildigiHesapId",
                table: "BudgetOdemeler",
                column: "OdemeYapildigiHesapId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetOdemeler_OdemeYil_OdemeAy_MasrafKalemi",
                table: "BudgetOdemeler",
                columns: new[] { "OdemeYil", "OdemeAy", "MasrafKalemi" });

            migrationBuilder.CreateIndex(
                name: "IX_BudgetOdemeler_TaksitGrupId",
                table: "BudgetOdemeler",
                column: "TaksitGrupId");

            migrationBuilder.CreateIndex(
                name: "IX_Cariler_CariKodu",
                table: "Cariler",
                column: "CariKodu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cariler_FirmaId",
                table: "Cariler",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_Cariler_MuhasebeHesapId",
                table: "Cariler",
                column: "MuhasebeHesapId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistKalemleri_AylikChecklistId",
                table: "ChecklistKalemleri",
                column: "AylikChecklistId");

            migrationBuilder.CreateIndex(
                name: "IX_FaturaKalemleri_FaturaId",
                table: "FaturaKalemleri",
                column: "FaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_Faturalar_CariId",
                table: "Faturalar",
                column: "CariId");

            migrationBuilder.CreateIndex(
                name: "IX_Faturalar_FaturaNo",
                table: "Faturalar",
                column: "FaturaNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Faturalar_FirmaId",
                table: "Faturalar",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_Firmalar_FirmaKodu",
                table: "Firmalar",
                column: "FirmaKodu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GunlukPuantajlar_PersonelPuantajId",
                table: "GunlukPuantajlar",
                column: "PersonelPuantajId");

            migrationBuilder.CreateIndex(
                name: "IX_GunlukPuantajlar_ServisCalismaId",
                table: "GunlukPuantajlar",
                column: "ServisCalismaId");

            migrationBuilder.CreateIndex(
                name: "IX_Guzergahlar_CariId",
                table: "Guzergahlar",
                column: "CariId");

            migrationBuilder.CreateIndex(
                name: "IX_Guzergahlar_GuzergahKodu",
                table: "Guzergahlar",
                column: "GuzergahKodu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KiralamaAraclar_FirmaId",
                table: "KiralamaAraclar",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_KiralamaAraclar_KiralayıcıCariId",
                table: "KiralamaAraclar",
                column: "KiralayıcıCariId");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_KullaniciAdi",
                table: "Kullanicilar",
                column: "KullaniciAdi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_RolId",
                table: "Kullanicilar",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_SoforId",
                table: "Kullanicilar",
                column: "SoforId");

            migrationBuilder.CreateIndex(
                name: "IX_Lisanslar_LisansAnahtari",
                table: "Lisanslar",
                column: "LisansAnahtari",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MasrafKalemleri_MasrafKodu",
                table: "MasrafKalemleri",
                column: "MasrafKodu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MuhasebeDonemleri_Yil_Ay",
                table: "MuhasebeDonemleri",
                columns: new[] { "Yil", "Ay" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MuhasebeFisKalemleri_CariId",
                table: "MuhasebeFisKalemleri",
                column: "CariId");

            migrationBuilder.CreateIndex(
                name: "IX_MuhasebeFisKalemleri_FisId",
                table: "MuhasebeFisKalemleri",
                column: "FisId");

            migrationBuilder.CreateIndex(
                name: "IX_MuhasebeFisKalemleri_HesapId",
                table: "MuhasebeFisKalemleri",
                column: "HesapId");

            migrationBuilder.CreateIndex(
                name: "IX_MuhasebeFisleri_FisNo",
                table: "MuhasebeFisleri",
                column: "FisNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MuhasebeHesaplari_HesapKodu",
                table: "MuhasebeHesaplari",
                column: "HesapKodu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MuhasebeHesaplari_UstHesapId",
                table: "MuhasebeHesaplari",
                column: "UstHesapId");

            migrationBuilder.CreateIndex(
                name: "IX_OdemeEslestirmeleri_BankaKasaHareketId",
                table: "OdemeEslestirmeleri",
                column: "BankaKasaHareketId");

            migrationBuilder.CreateIndex(
                name: "IX_OdemeEslestirmeleri_FaturaId",
                table: "OdemeEslestirmeleri",
                column: "FaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonelIzinHaklari_SoforId_Yil",
                table: "PersonelIzinHaklari",
                columns: new[] { "SoforId", "Yil" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonelIzinleri_SoforId",
                table: "PersonelIzinleri",
                column: "SoforId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonelMaaslari_SoforId_Yil_Ay",
                table: "PersonelMaaslari",
                columns: new[] { "SoforId", "Yil", "Ay" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonelPuantajlar_FirmaId",
                table: "PersonelPuantajlar",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonelPuantajlar_PersonelId",
                table: "PersonelPuantajlar",
                column: "PersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_PiyasaIlanlari_AracIlanId",
                table: "PiyasaIlanlari",
                column: "AracIlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Roller_RolAdi",
                table: "Roller",
                column: "RolAdi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolYetkileri_RolId_YetkiKodu",
                table: "RolYetkileri",
                columns: new[] { "RolId", "YetkiKodu" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SatisPersonelleri_PersonelKodu",
                table: "SatisPersonelleri",
                column: "PersonelKodu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServisCalismaKiralamalar_AracId",
                table: "ServisCalismaKiralamalar",
                column: "AracId");

            migrationBuilder.CreateIndex(
                name: "IX_ServisCalismaKiralamalar_FirmaId",
                table: "ServisCalismaKiralamalar",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_ServisCalismaKiralamalar_GuzergahId",
                table: "ServisCalismaKiralamalar",
                column: "GuzergahId");

            migrationBuilder.CreateIndex(
                name: "IX_ServisCalismaKiralamalar_KiralamaAracId",
                table: "ServisCalismaKiralamalar",
                column: "KiralamaAracId");

            migrationBuilder.CreateIndex(
                name: "IX_ServisCalismaKiralamalar_MusteriFirmaId",
                table: "ServisCalismaKiralamalar",
                column: "MusteriFirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_ServisCalismaKiralamalar_SoforId",
                table: "ServisCalismaKiralamalar",
                column: "SoforId");

            migrationBuilder.CreateIndex(
                name: "IX_ServisCalismalari_AracId",
                table: "ServisCalismalari",
                column: "AracId");

            migrationBuilder.CreateIndex(
                name: "IX_ServisCalismalari_GuzergahId",
                table: "ServisCalismalari",
                column: "GuzergahId");

            migrationBuilder.CreateIndex(
                name: "IX_ServisCalismalari_SoforId",
                table: "ServisCalismalari",
                column: "SoforId");

            migrationBuilder.CreateIndex(
                name: "IX_Soforler_SoforKodu",
                table: "Soforler",
                column: "SoforKodu",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AktiviteLoglar");

            migrationBuilder.DropTable(
                name: "AracEvrakDosyalari");

            migrationBuilder.DropTable(
                name: "AracMasraflari");

            migrationBuilder.DropTable(
                name: "AracModelleri");

            migrationBuilder.DropTable(
                name: "AracSatislari");

            migrationBuilder.DropTable(
                name: "AylikOdemeGerceklesenler");

            migrationBuilder.DropTable(
                name: "BudgetMasrafKalemleri");

            migrationBuilder.DropTable(
                name: "BudgetOdemeler");

            migrationBuilder.DropTable(
                name: "ChecklistKalemleri");

            migrationBuilder.DropTable(
                name: "FaturaKalemleri");

            migrationBuilder.DropTable(
                name: "GunlukPuantajlar");

            migrationBuilder.DropTable(
                name: "Kullanicilar");

            migrationBuilder.DropTable(
                name: "Lisanslar");

            migrationBuilder.DropTable(
                name: "MuhasebeDonemleri");

            migrationBuilder.DropTable(
                name: "MuhasebeFisKalemleri");

            migrationBuilder.DropTable(
                name: "OdemeEslestirmeleri");

            migrationBuilder.DropTable(
                name: "PersonelIzinHaklari");

            migrationBuilder.DropTable(
                name: "PersonelIzinleri");

            migrationBuilder.DropTable(
                name: "PersonelMaaslari");

            migrationBuilder.DropTable(
                name: "PiyasaIlanlari");

            migrationBuilder.DropTable(
                name: "RolYetkileri");

            migrationBuilder.DropTable(
                name: "ServisCalismaKiralamalar");

            migrationBuilder.DropTable(
                name: "AracEvraklari");

            migrationBuilder.DropTable(
                name: "AracMarkalari");

            migrationBuilder.DropTable(
                name: "AylikOdemePlanlari");

            migrationBuilder.DropTable(
                name: "AylikChecklistler");

            migrationBuilder.DropTable(
                name: "PersonelPuantajlar");

            migrationBuilder.DropTable(
                name: "ServisCalismalari");

            migrationBuilder.DropTable(
                name: "MuhasebeFisleri");

            migrationBuilder.DropTable(
                name: "BankaKasaHareketleri");

            migrationBuilder.DropTable(
                name: "Faturalar");

            migrationBuilder.DropTable(
                name: "AracIlanlari");

            migrationBuilder.DropTable(
                name: "Roller");

            migrationBuilder.DropTable(
                name: "KiralamaAraclar");

            migrationBuilder.DropTable(
                name: "MasrafKalemleri");

            migrationBuilder.DropTable(
                name: "Araclar");

            migrationBuilder.DropTable(
                name: "Guzergahlar");

            migrationBuilder.DropTable(
                name: "Soforler");

            migrationBuilder.DropTable(
                name: "BankaHesaplari");

            migrationBuilder.DropTable(
                name: "SatisPersonelleri");

            migrationBuilder.DropTable(
                name: "Cariler");

            migrationBuilder.DropTable(
                name: "Firmalar");

            migrationBuilder.DropTable(
                name: "MuhasebeHesaplari");
        }
    }
}
