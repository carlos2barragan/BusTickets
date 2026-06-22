using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Busticket.Migrations
{
    /// <inheritdoc />
    public partial class AddTipoBusYNumAsientos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumAsientos",
                table: "Ruta",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoBus",
                table: "Ruta",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumAsientos",
                table: "Ruta");

            migrationBuilder.DropColumn(
                name: "TipoBus",
                table: "Ruta");
        }
    }
}
