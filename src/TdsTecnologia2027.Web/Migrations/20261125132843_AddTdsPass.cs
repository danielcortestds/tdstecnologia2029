using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TdsTecnologia2027.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddTdsPass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CursoOnlineId",
                table: "PedidosItens",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "PlanoAssinaturaId",
                table: "PedidosItens",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Planos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 600, nullable: true),
                    Preco = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    PrecoDe = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    DuracaoMeses = table.Column<int>(type: "INTEGER", nullable: false),
                    BonusMeses = table.Column<int>(type: "INTEGER", nullable: false),
                    BonusAtivo = table.Column<bool>(type: "INTEGER", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Planos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Assinaturas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlunoId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlanoAssinaturaId = table.Column<int>(type: "INTEGER", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataFim = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    PedidoId = table.Column<int>(type: "INTEGER", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assinaturas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assinaturas_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assinaturas_Planos_PlanoAssinaturaId",
                        column: x => x.PlanoAssinaturaId,
                        principalTable: "Planos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PedidosItens_PlanoAssinaturaId",
                table: "PedidosItens",
                column: "PlanoAssinaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_Assinaturas_AlunoId",
                table: "Assinaturas",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_Assinaturas_PlanoAssinaturaId",
                table: "Assinaturas",
                column: "PlanoAssinaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_Planos_Slug",
                table: "Planos",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PedidosItens_Planos_PlanoAssinaturaId",
                table: "PedidosItens",
                column: "PlanoAssinaturaId",
                principalTable: "Planos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PedidosItens_Planos_PlanoAssinaturaId",
                table: "PedidosItens");

            migrationBuilder.DropTable(
                name: "Assinaturas");

            migrationBuilder.DropTable(
                name: "Planos");

            migrationBuilder.DropIndex(
                name: "IX_PedidosItens_PlanoAssinaturaId",
                table: "PedidosItens");

            migrationBuilder.DropColumn(
                name: "PlanoAssinaturaId",
                table: "PedidosItens");

            migrationBuilder.AlterColumn<int>(
                name: "CursoOnlineId",
                table: "PedidosItens",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
