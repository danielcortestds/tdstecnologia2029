using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TdsTecnologia2027.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddFabricanteCurso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Fabricante",
                table: "CursosOnline",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fabricante",
                table: "CursosOnline");
        }
    }
}
