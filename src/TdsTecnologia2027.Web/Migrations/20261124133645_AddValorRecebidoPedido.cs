using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TdsTecnologia2027.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddValorRecebidoPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ValorRecebido",
                table: "Pedidos",
                type: "decimal(10,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValorRecebido",
                table: "Pedidos");
        }
    }
}
