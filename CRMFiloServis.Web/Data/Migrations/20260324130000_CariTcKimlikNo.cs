using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRMFiloServis.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class CariTcKimlikNo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TcKimlikNo",
                table: "Cariler",
                type: "character varying(11)",
                maxLength: 11,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TcKimlikNo",
                table: "Cariler");
        }
    }
}
