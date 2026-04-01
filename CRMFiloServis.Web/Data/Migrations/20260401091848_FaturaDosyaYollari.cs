using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CRMFiloServis.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class FaturaDosyaYollari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MalMasrafHesabi",
                table: "MuhasebeAyarlari",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SarfMalzemeMasrafHesabi",
                table: "MuhasebeAyarlari",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StokCikisHesabi",
                table: "MuhasebeAyarlari",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "StokMasrafAktarimiOtomatik",
                table: "MuhasebeAyarlari",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PdfDosyaYolu",
                table: "Faturalar",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "XmlDosyaYolu",
                table: "Faturalar",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fax",
                table: "Cariler",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Il",
                table: "Cariler",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ilce",
                table: "Cariler",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostaKodu",
                table: "Cariler",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefon2",
                table: "Cariler",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebSitesi",
                table: "Cariler",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OzlukEvrakTanimlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EvrakAdi = table.Column<string>(type: "text", nullable: false),
                    Aciklama = table.Column<string>(type: "text", nullable: true),
                    Kategori = table.Column<int>(type: "integer", nullable: false),
                    Zorunlu = table.Column<bool>(type: "boolean", nullable: false),
                    SiraNo = table.Column<int>(type: "integer", nullable: false),
                    Aktif = table.Column<bool>(type: "boolean", nullable: false),
                    GecerliGorevler = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OzlukEvrakTanimlari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PersonelOzlukEvraklar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SoforId = table.Column<int>(type: "integer", nullable: false),
                    EvrakTanimId = table.Column<int>(type: "integer", nullable: false),
                    Tamamlandi = table.Column<bool>(type: "boolean", nullable: false),
                    TamamlanmaTarihi = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DosyaYolu = table.Column<string>(type: "text", nullable: true),
                    Aciklama = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonelOzlukEvraklar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonelOzlukEvraklar_OzlukEvrakTanimlari_EvrakTanimId",
                        column: x => x.EvrakTanimId,
                        principalTable: "OzlukEvrakTanimlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonelOzlukEvraklar_Soforler_SoforId",
                        column: x => x.SoforId,
                        principalTable: "Soforler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonelOzlukEvraklar_EvrakTanimId",
                table: "PersonelOzlukEvraklar",
                column: "EvrakTanimId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonelOzlukEvraklar_SoforId",
                table: "PersonelOzlukEvraklar",
                column: "SoforId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonelOzlukEvraklar");

            migrationBuilder.DropTable(
                name: "OzlukEvrakTanimlari");

            migrationBuilder.DropColumn(
                name: "MalMasrafHesabi",
                table: "MuhasebeAyarlari");

            migrationBuilder.DropColumn(
                name: "SarfMalzemeMasrafHesabi",
                table: "MuhasebeAyarlari");

            migrationBuilder.DropColumn(
                name: "StokCikisHesabi",
                table: "MuhasebeAyarlari");

            migrationBuilder.DropColumn(
                name: "StokMasrafAktarimiOtomatik",
                table: "MuhasebeAyarlari");

            migrationBuilder.DropColumn(
                name: "PdfDosyaYolu",
                table: "Faturalar");

            migrationBuilder.DropColumn(
                name: "XmlDosyaYolu",
                table: "Faturalar");

            migrationBuilder.DropColumn(
                name: "Fax",
                table: "Cariler");

            migrationBuilder.DropColumn(
                name: "Il",
                table: "Cariler");

            migrationBuilder.DropColumn(
                name: "Ilce",
                table: "Cariler");

            migrationBuilder.DropColumn(
                name: "PostaKodu",
                table: "Cariler");

            migrationBuilder.DropColumn(
                name: "Telefon2",
                table: "Cariler");

            migrationBuilder.DropColumn(
                name: "WebSitesi",
                table: "Cariler");
        }
    }
}
