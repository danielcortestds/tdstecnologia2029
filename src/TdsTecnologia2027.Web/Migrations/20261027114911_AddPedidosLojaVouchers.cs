using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TdsTecnologia2027.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPedidosLojaVouchers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfiguracoesLoja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Handle = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    ApiKey = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracoesLoja", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pedidos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderNsu = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    NomeCliente = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    EmailCliente = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    TelefoneCliente = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    Total = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    InvoiceSlug = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    TransactionNsu = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    MetodoPagamento = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PagoEm = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vouchers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    CursoOnlineId = table.Column<int>(type: "INTEGER", nullable: false),
                    Validade = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UsosMaximos = table.Column<int>(type: "INTEGER", nullable: false),
                    UsosFeitos = table.Column<int>(type: "INTEGER", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    Observacao = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vouchers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vouchers_CursosOnline_CursoOnlineId",
                        column: x => x.CursoOnlineId,
                        principalTable: "CursosOnline",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PedidosItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PedidoId = table.Column<int>(type: "INTEGER", nullable: false),
                    CursoOnlineId = table.Column<int>(type: "INTEGER", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    Preco = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidosItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidosItens_CursosOnline_CursoOnlineId",
                        column: x => x.CursoOnlineId,
                        principalTable: "CursosOnline",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PedidosItens_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VouchersResgates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VoucherId = table.Column<int>(type: "INTEGER", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    AlunoId = table.Column<int>(type: "INTEGER", nullable: true),
                    ResgatadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VouchersResgates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VouchersResgates_Vouchers_VoucherId",
                        column: x => x.VoucherId,
                        principalTable: "Vouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_OrderNsu",
                table: "Pedidos",
                column: "OrderNsu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PedidosItens_CursoOnlineId",
                table: "PedidosItens",
                column: "CursoOnlineId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidosItens_PedidoId",
                table: "PedidosItens",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_Codigo",
                table: "Vouchers",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_CursoOnlineId",
                table: "Vouchers",
                column: "CursoOnlineId");

            migrationBuilder.CreateIndex(
                name: "IX_VouchersResgates_VoucherId",
                table: "VouchersResgates",
                column: "VoucherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracoesLoja");

            migrationBuilder.DropTable(
                name: "PedidosItens");

            migrationBuilder.DropTable(
                name: "VouchersResgates");

            migrationBuilder.DropTable(
                name: "Pedidos");

            migrationBuilder.DropTable(
                name: "Vouchers");
        }
    }
}
