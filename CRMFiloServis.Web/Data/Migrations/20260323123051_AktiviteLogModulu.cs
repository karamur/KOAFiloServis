using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CRMFiloServis.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AktiviteLogModulu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AktiviteLoglar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IslemZamani = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IslemTipi = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Modul = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityTipi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EntityId = table.Column<int>(type: "integer", nullable: true),
                    EntityAdi = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Aciklama = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EskiDeger = table.Column<string>(type: "text", nullable: true),
                    YeniDeger = table.Column<string>(type: "text", nullable: true),
                    KullaniciAdi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IpAdresi = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Tarayici = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Seviye = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AktiviteLoglar", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AktiviteLoglar_IslemZamani",
                table: "AktiviteLoglar",
                column: "IslemZamani");

            migrationBuilder.CreateIndex(
                name: "IX_AktiviteLoglar_Modul_IslemTipi",
                table: "AktiviteLoglar",
                columns: new[] { "Modul", "IslemTipi" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AktiviteLoglar");
        }
    }
}
