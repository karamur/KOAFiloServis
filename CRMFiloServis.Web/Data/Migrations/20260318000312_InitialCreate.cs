using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CRMFiloServis.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Araclar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Plaka = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    Marka = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Model = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ModelYili = table.Column<int>(type: "integer", nullable: true),
                    SaseNo = table.Column<string>(type: "text", nullable: true),
                    MotorNo = table.Column<string>(type: "text", nullable: true),
                    Renk = table.Column<string>(type: "text", nullable: true),
                    KoltukSayisi = table.Column<int>(type: "integer", nullable: false),
                    AracTipi = table.Column<int>(type: "integer", nullable: false),
                    TrafikSigortaBitisTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    KaskoBitisTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MuayeneBitisTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    KmDurumu = table.Column<int>(type: "integer", nullable: true),
                    Aktif = table.Column<bool>(type: "boolean", nullable: false),
                    Notlar = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Araclar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BankaHesaplari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HesapKodu = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HesapAdi = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    HesapTipi = table.Column<int>(type: "integer", nullable: false),
                    BankaAdi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SubeAdi = table.Column<string>(type: "text", nullable: true),
                    SubeKodu = table.Column<string>(type: "text", nullable: true),
                    HesapNo = table.Column<string>(type: "text", nullable: true),
                    Iban = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: true),
                    ParaBirimi = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    AcilisBakiye = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Aktif = table.Column<bool>(type: "boolean", nullable: false),
                    Notlar = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankaHesaplari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cariler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CariKodu = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Unvan = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    CariTipi = table.Column<int>(type: "integer", nullable: false),
                    VergiDairesi = table.Column<string>(type: "text", nullable: true),
                    VergiNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Adres = table.Column<string>(type: "text", nullable: true),
                    Telefon = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    YetkiliKisi = table.Column<string>(type: "text", nullable: true),
                    Notlar = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cariler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MasrafKalemleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MasrafKodu = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MasrafAdi = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Kategori = table.Column<int>(type: "integer", nullable: false),
                    Aktif = table.Column<bool>(type: "boolean", nullable: false),
                    Notlar = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasrafKalemleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Soforler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SoforKodu = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Ad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Soyad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TcKimlikNo = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    Telefon = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Adres = table.Column<string>(type: "text", nullable: true),
                    EhliyetNo = table.Column<string>(type: "text", nullable: true),
                    EhliyetGecerlilikTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IseBaslamaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Aktif = table.Column<bool>(type: "boolean", nullable: false),
                    Notlar = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Soforler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BankaKasaHareketleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IslemNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IslemTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HareketTipi = table.Column<int>(type: "integer", nullable: false),
                    Tutar = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Aciklama = table.Column<string>(type: "text", nullable: true),
                    BelgeNo = table.Column<string>(type: "text", nullable: true),
                    IslemKaynak = table.Column<int>(type: "integer", nullable: false),
                    BankaHesapId = table.Column<int>(type: "integer", nullable: false),
                    CariId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FaturaNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FaturaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VadeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FaturaTipi = table.Column<int>(type: "integer", nullable: false),
                    Durum = table.Column<int>(type: "integer", nullable: false),
                    AraToplam = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    KdvOrani = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    KdvTutar = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    GenelToplam = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OdenenTutar = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Aciklama = table.Column<string>(type: "text", nullable: true),
                    Notlar = table.Column<string>(type: "text", nullable: true),
                    CariId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "Guzergahlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuzergahKodu = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GuzergahAdi = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BaslangicNoktasi = table.Column<string>(type: "text", nullable: true),
                    BitisNoktasi = table.Column<string>(type: "text", nullable: true),
                    BirimFiyat = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Mesafe = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    TahminiSure = table.Column<int>(type: "integer", nullable: true),
                    Aktif = table.Column<bool>(type: "boolean", nullable: false),
                    Notlar = table.Column<string>(type: "text", nullable: true),
                    CariId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "FaturaKalemleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SiraNo = table.Column<int>(type: "integer", nullable: false),
                    Aciklama = table.Column<string>(type: "text", nullable: false),
                    Miktar = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Birim = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    KdvOrani = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    KdvTutar = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ToplamTutar = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FaturaId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EslestirmeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EslestirilenTutar = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Aciklama = table.Column<string>(type: "text", nullable: true),
                    FaturaId = table.Column<int>(type: "integer", nullable: false),
                    BankaKasaHareketId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "ServisCalismalari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CalismaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ServisTuru = table.Column<int>(type: "integer", nullable: false),
                    Fiyat = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    KmBaslangic = table.Column<int>(type: "integer", nullable: true),
                    KmBitis = table.Column<int>(type: "integer", nullable: true),
                    BaslangicSaati = table.Column<TimeSpan>(type: "interval", nullable: true),
                    BitisSaati = table.Column<TimeSpan>(type: "interval", nullable: true),
                    ArizaOlduMu = table.Column<bool>(type: "boolean", nullable: false),
                    ArizaAciklamasi = table.Column<string>(type: "text", nullable: true),
                    Durum = table.Column<int>(type: "integer", nullable: false),
                    Notlar = table.Column<string>(type: "text", nullable: true),
                    AracId = table.Column<int>(type: "integer", nullable: false),
                    SoforId = table.Column<int>(type: "integer", nullable: false),
                    GuzergahId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "AracMasraflari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MasrafTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Tutar = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Aciklama = table.Column<string>(type: "text", nullable: true),
                    BelgeNo = table.Column<string>(type: "text", nullable: true),
                    ArizaKaynaklimi = table.Column<bool>(type: "boolean", nullable: false),
                    AracId = table.Column<int>(type: "integer", nullable: false),
                    MasrafKalemiId = table.Column<int>(type: "integer", nullable: false),
                    GuzergahId = table.Column<int>(type: "integer", nullable: true),
                    ServisCalismaId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_Araclar_Plaka",
                table: "Araclar",
                column: "Plaka",
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
                name: "IX_Cariler_CariKodu",
                table: "Cariler",
                column: "CariKodu",
                unique: true);

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
                name: "IX_Guzergahlar_CariId",
                table: "Guzergahlar",
                column: "CariId");

            migrationBuilder.CreateIndex(
                name: "IX_Guzergahlar_GuzergahKodu",
                table: "Guzergahlar",
                column: "GuzergahKodu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MasrafKalemleri_MasrafKodu",
                table: "MasrafKalemleri",
                column: "MasrafKodu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OdemeEslestirmeleri_BankaKasaHareketId",
                table: "OdemeEslestirmeleri",
                column: "BankaKasaHareketId");

            migrationBuilder.CreateIndex(
                name: "IX_OdemeEslestirmeleri_FaturaId",
                table: "OdemeEslestirmeleri",
                column: "FaturaId");

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
                name: "AracMasraflari");

            migrationBuilder.DropTable(
                name: "FaturaKalemleri");

            migrationBuilder.DropTable(
                name: "OdemeEslestirmeleri");

            migrationBuilder.DropTable(
                name: "MasrafKalemleri");

            migrationBuilder.DropTable(
                name: "ServisCalismalari");

            migrationBuilder.DropTable(
                name: "BankaKasaHareketleri");

            migrationBuilder.DropTable(
                name: "Faturalar");

            migrationBuilder.DropTable(
                name: "Araclar");

            migrationBuilder.DropTable(
                name: "Guzergahlar");

            migrationBuilder.DropTable(
                name: "Soforler");

            migrationBuilder.DropTable(
                name: "BankaHesaplari");

            migrationBuilder.DropTable(
                name: "Cariler");
        }
    }
}
