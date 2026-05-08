using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KOAFiloServis.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAracMasrafIdToBankaKasaHareket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AracMasrafId",
                table: "BankaKasaHareketleri",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LastikSezonAyarlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SirketId = table.Column<int>(type: "integer", nullable: true),
                    Ad = table.Column<string>(type: "text", nullable: false),
                    SezonTipi = table.Column<int>(type: "integer", nullable: false),
                    BaslangicAyi = table.Column<int>(type: "integer", nullable: false),
                    BaslangicGunu = table.Column<int>(type: "integer", nullable: false),
                    BitisAyi = table.Column<int>(type: "integer", nullable: false),
                    BitisGunu = table.Column<int>(type: "integer", nullable: false),
                    UyariOncesiGun = table.Column<int>(type: "integer", nullable: false),
                    Notlar = table.Column<string>(type: "text", nullable: true),
                    Aktif = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LastikSezonAyarlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LastikSezonAyarlari_Sirketler_SirketId",
                        column: x => x.SirketId,
                        principalTable: "Sirketler",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LastikSezonAyarlari_SirketId",
                table: "LastikSezonAyarlari",
                column: "SirketId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LastikSezonAyarlari");

            migrationBuilder.DropColumn(
                name: "AracMasrafId",
                table: "BankaKasaHareketleri");
        }
    }
}
