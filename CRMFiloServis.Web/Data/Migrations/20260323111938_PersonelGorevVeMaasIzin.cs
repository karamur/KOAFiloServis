using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CRMFiloServis.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class PersonelGorevVeMaasIzin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankaAdi",
                table: "Soforler",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BrutMaas",
                table: "Soforler",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Departman",
                table: "Soforler",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gorev",
                table: "Soforler",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IBAN",
                table: "Soforler",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "IstenAyrilmaTarihi",
                table: "Soforler",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NetMaas",
                table: "Soforler",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Pozisyon",
                table: "Soforler",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PsikoteknikGecerlilikTarihi",
                table: "Soforler",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SaglikRaporuGecerlilikTarihi",
                table: "Soforler",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SrcBelgesiGecerlilikTarihi",
                table: "Soforler",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AylikChecklistler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Yil = table.Column<int>(type: "integer", nullable: false),
                    Ay = table.Column<int>(type: "integer", nullable: false),
                    ChecklistTipi = table.Column<int>(type: "integer", nullable: false),
                    SoforId = table.Column<int>(type: "integer", nullable: true),
                    AracId = table.Column<int>(type: "integer", nullable: true),
                    GuzergahId = table.Column<int>(type: "integer", nullable: true),
                    KontrolTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    KontrolEden = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GenelDurum = table.Column<int>(type: "integer", nullable: false),
                    Notlar = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "PersonelIzinHaklari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SoforId = table.Column<int>(type: "integer", nullable: false),
                    Yil = table.Column<int>(type: "integer", nullable: false),
                    YillikIzinHakki = table.Column<int>(type: "integer", nullable: false),
                    KullanilanIzin = table.Column<int>(type: "integer", nullable: false),
                    DevirenIzin = table.Column<int>(type: "integer", nullable: false),
                    Notlar = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SoforId = table.Column<int>(type: "integer", nullable: false),
                    IzinTipi = table.Column<int>(type: "integer", nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Durum = table.Column<int>(type: "integer", nullable: false),
                    OnaylayanKisi = table.Column<string>(type: "text", nullable: true),
                    OnayTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RedNedeni = table.Column<string>(type: "text", nullable: true),
                    Aciklama = table.Column<string>(type: "text", nullable: true),
                    Notlar = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SoforId = table.Column<int>(type: "integer", nullable: false),
                    Yil = table.Column<int>(type: "integer", nullable: false),
                    Ay = table.Column<int>(type: "integer", nullable: false),
                    BrutMaas = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NetMaas = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SGKIsciPayi = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SGKIsverenPayi = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    GelirVergisi = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DamgaVergisi = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IssizlikPrimi = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Prim = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Ikramiye = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Yemek = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Yol = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Mesai = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DigerEklemeler = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Avans = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IcraTakibi = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DigerKesintiler = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OdemeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OdemeDurum = table.Column<int>(type: "integer", nullable: false),
                    OdemeAciklama = table.Column<string>(type: "text", nullable: true),
                    CalismaGunu = table.Column<int>(type: "integer", nullable: false),
                    IzinliGun = table.Column<int>(type: "integer", nullable: false),
                    RaporluGun = table.Column<int>(type: "integer", nullable: false),
                    DevamsizlikGun = table.Column<int>(type: "integer", nullable: false),
                    Notlar = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "ChecklistKalemleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AylikChecklistId = table.Column<int>(type: "integer", nullable: false),
                    KalemAdi = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Durum = table.Column<int>(type: "integer", nullable: false),
                    SonGecerlilikTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    KontrolTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Aciklama = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SiraNo = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "IX_AylikChecklistler_Yil_Ay_ChecklistTipi_SoforId_AracId_Guzer~",
                table: "AylikChecklistler",
                columns: new[] { "Yil", "Ay", "ChecklistTipi", "SoforId", "AracId", "GuzergahId" });

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistKalemleri_AylikChecklistId",
                table: "ChecklistKalemleri",
                column: "AylikChecklistId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChecklistKalemleri");

            migrationBuilder.DropTable(
                name: "PersonelIzinHaklari");

            migrationBuilder.DropTable(
                name: "PersonelIzinleri");

            migrationBuilder.DropTable(
                name: "PersonelMaaslari");

            migrationBuilder.DropTable(
                name: "AylikChecklistler");

            migrationBuilder.DropColumn(
                name: "BankaAdi",
                table: "Soforler");

            migrationBuilder.DropColumn(
                name: "BrutMaas",
                table: "Soforler");

            migrationBuilder.DropColumn(
                name: "Departman",
                table: "Soforler");

            migrationBuilder.DropColumn(
                name: "Gorev",
                table: "Soforler");

            migrationBuilder.DropColumn(
                name: "IBAN",
                table: "Soforler");

            migrationBuilder.DropColumn(
                name: "IstenAyrilmaTarihi",
                table: "Soforler");

            migrationBuilder.DropColumn(
                name: "NetMaas",
                table: "Soforler");

            migrationBuilder.DropColumn(
                name: "Pozisyon",
                table: "Soforler");

            migrationBuilder.DropColumn(
                name: "PsikoteknikGecerlilikTarihi",
                table: "Soforler");

            migrationBuilder.DropColumn(
                name: "SaglikRaporuGecerlilikTarihi",
                table: "Soforler");

            migrationBuilder.DropColumn(
                name: "SrcBelgesiGecerlilikTarihi",
                table: "Soforler");
        }
    }
}
