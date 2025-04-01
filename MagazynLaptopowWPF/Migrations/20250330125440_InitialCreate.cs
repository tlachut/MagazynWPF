using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagazynLaptopowWPF.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Laptopy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Marka = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SystemOperacyjny = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    RozmiarEkranu = table.Column<double>(type: "REAL", nullable: true),
                    Ilosc = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Laptopy", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Laptopy_Marka",
                table: "Laptopy",
                column: "Marka");

            migrationBuilder.CreateIndex(
                name: "IX_Laptopy_Model",
                table: "Laptopy",
                column: "Model");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Laptopy");
        }
    }
}
