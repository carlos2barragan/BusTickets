using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Busticket.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDatosPasajeroBoleto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentoPasajero",
                table: "Boleto",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelefonoPasajero",
                table: "Boleto",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentoPasajero",
                table: "Boleto");

            migrationBuilder.DropColumn(
                name: "TelefonoPasajero",
                table: "Boleto");
        }
    }
}
