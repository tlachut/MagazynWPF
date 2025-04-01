using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagazynLaptopowWPF.Migrations
{
    /// <inheritdoc />
    public partial class DodanieKoduKreskowego : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KodKreskowy",
                table: "Laptopy",
                type: "TEXT",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KodKreskowy",
                table: "Laptopy");
        }
    }
}
