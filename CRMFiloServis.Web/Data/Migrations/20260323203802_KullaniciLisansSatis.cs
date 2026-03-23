using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CRMFiloServis.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class KullaniciLisansSatis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AracMarkalari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MarkaAdi = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Logo = table.Column<string>(type: "text", nullable: true),
                    SiraNo = table.Column<int>(type: "integer", nullable: false),
                    Aktif = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AracMarkalari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lisanslar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LisansAnahtari = table.Column<string>(type: "text", nullable: false),
                    Tur = table.Column<int>(type: "integer", nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FirmaAdi = table.Column<string>(type: "text", nullable: true),
                    YetkiliKisi = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Telefon = table.Column<string>(type: "text", nullable: true),
                    MakineKodu = table.Column<string>(type: "text", nullable: false),
                    MaxKullaniciSayisi = table.Column<int>(type: "integer", nullable: false),
                    ExcelExportIzni = table.Column<bool>(type: "boolean", nullable: false),
                    PdfExportIzni = table.Column<bool>(type: "boolean", nullable: false),
                    RaporlamaIzni = table.Column<bool>(type: "boolean", nullable: false),
                    YedeklemeIzni = table.Column<bool>(type: "boolean", nullable: false),
                    MuhasebeIzni = table.Column<bool>(type: "boolean", nullable: false),
                    SatisModuluIzni = table.Column<bool>(type: "boolean", nullable: false),
                    Imza = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lisanslar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roller",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RolAdi = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Aciklama = table.Column<string>(type: "text", nullable: true),
                    SistemRolu = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roller", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SatisPersonelleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonelKodu = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AdSoyad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Telefon = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Aktif = table.Column<bool>(type: "boolean", nullable: false),
                    KomisyonOrani = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    SabitKomisyon = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AylikSatisHedefi = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AylikAracHedefi = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatisPersonelleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AracModelleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MarkaId = table.Column<int>(type: "integer", nullable: false),
                    ModelAdi = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BaslangicYili = table.Column<int>(type: "integer", nullable: false),
                    BitisYili = table.Column<int>(type: "integer", nullable: true),
                    VarsayilanKasaTipi = table.Column<int>(type: "integer", nullable: false),
                    Aktif = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "Kullanicilar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KullaniciAdi = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SifreHash = table.Column<string>(type: "text", nullable: false),
                    AdSoyad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Telefon = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    SoforId = table.Column<int>(type: "integer", nullable: true),
                    RolId = table.Column<int>(type: "integer", nullable: false),
                    Aktif = table.Column<bool>(type: "boolean", nullable: false),
                    SonGirisTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BasarisizGirisSayisi = table.Column<int>(type: "integer", nullable: false),
                    Kilitli = table.Column<bool>(type: "boolean", nullable: false),
                    Tema = table.Column<string>(type: "text", nullable: false),
                    KompaktMod = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "RolYetkileri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RolId = table.Column<int>(type: "integer", nullable: false),
                    YetkiKodu = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Izin = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "AracIlanlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Plaka = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    Marka = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Model = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ModelYili = table.Column<int>(type: "integer", nullable: false),
                    Versiyon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Kilometre = table.Column<int>(type: "integer", nullable: false),
                    YakitTuru = table.Column<int>(type: "integer", nullable: false),
                    VitesTuru = table.Column<int>(type: "integer", nullable: false),
                    KasaTipi = table.Column<int>(type: "integer", nullable: false),
                    Renk = table.Column<int>(type: "integer", nullable: false),
                    Durum = table.Column<int>(type: "integer", nullable: false),
                    Boyali = table.Column<bool>(type: "boolean", nullable: false),
                    BoyaliParcaSayisi = table.Column<int>(type: "integer", nullable: false),
                    BoyaliParcalar = table.Column<string>(type: "text", nullable: true),
                    DegisenVar = table.Column<bool>(type: "boolean", nullable: false),
                    DegisenParcaSayisi = table.Column<int>(type: "integer", nullable: false),
                    DegisenParcalar = table.Column<string>(type: "text", nullable: true),
                    HasarKaydi = table.Column<bool>(type: "boolean", nullable: false),
                    HasarAciklama = table.Column<string>(type: "text", nullable: true),
                    TramerKaydi = table.Column<bool>(type: "boolean", nullable: false),
                    TramerTutari = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AlisFiyati = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SatisFiyati = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    KaskoDegeri = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PiyasaDegeriMin = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PiyasaDegeriMax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PiyasaDegeriOrtalama = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IlanDurum = table.Column<int>(type: "integer", nullable: false),
                    IlanTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SatisTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Aciklama = table.Column<string>(type: "text", nullable: true),
                    Notlar = table.Column<string>(type: "text", nullable: true),
                    Fotograflar = table.Column<string>(type: "text", nullable: true),
                    SahipCariId = table.Column<int>(type: "integer", nullable: true),
                    SatisPersoneliId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "AracSatislari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AracIlanId = table.Column<int>(type: "integer", nullable: false),
                    AliciCariId = table.Column<int>(type: "integer", nullable: true),
                    SatisPersoneliId = table.Column<int>(type: "integer", nullable: true),
                    SatisTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SatisFiyati = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    KomisyonTutari = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OdemeSekli = table.Column<int>(type: "integer", nullable: false),
                    Notlar = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AracIlanId = table.Column<int>(type: "integer", nullable: false),
                    Kaynak = table.Column<int>(type: "integer", nullable: false),
                    IlanUrl = table.Column<string>(type: "text", nullable: true),
                    IlanNo = table.Column<string>(type: "text", nullable: true),
                    Fiyat = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Sehir = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Ilce = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Kilometre = table.Column<int>(type: "integer", nullable: false),
                    Yil = table.Column<int>(type: "integer", nullable: false),
                    Durum = table.Column<string>(type: "text", nullable: true),
                    BoyaliParca = table.Column<int>(type: "integer", nullable: false),
                    DegisenParca = table.Column<int>(type: "integer", nullable: false),
                    TramerVar = table.Column<bool>(type: "boolean", nullable: false),
                    TramerTutari = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    TaramaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EkBilgiler = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "IX_AracMarkalari_MarkaAdi",
                table: "AracMarkalari",
                column: "MarkaAdi",
                unique: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AracModelleri");

            migrationBuilder.DropTable(
                name: "AracSatislari");

            migrationBuilder.DropTable(
                name: "Kullanicilar");

            migrationBuilder.DropTable(
                name: "Lisanslar");

            migrationBuilder.DropTable(
                name: "PiyasaIlanlari");

            migrationBuilder.DropTable(
                name: "RolYetkileri");

            migrationBuilder.DropTable(
                name: "AracMarkalari");

            migrationBuilder.DropTable(
                name: "AracIlanlari");

            migrationBuilder.DropTable(
                name: "Roller");

            migrationBuilder.DropTable(
                name: "SatisPersonelleri");
        }
    }
}
