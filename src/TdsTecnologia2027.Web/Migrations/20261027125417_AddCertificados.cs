using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TdsTecnologia2027.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogosCertificado",
                table: "CursosOnline",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Certificados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlunoId = table.Column<int>(type: "INTEGER", nullable: false),
                    CursoOnlineId = table.Column<int>(type: "INTEGER", nullable: false),
                    CodigoValidacao = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    NomeAluno = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    CursoNome = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    CargaHoraria = table.Column<int>(type: "INTEGER", nullable: false),
                    PeriodoExecucao = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    PercentualNaEmissao = table.Column<int>(type: "INTEGER", nullable: false),
                    EmitidoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certificados_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Certificados_CursosOnline_CursoOnlineId",
                        column: x => x.CursoOnlineId,
                        principalTable: "CursosOnline",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Certificados_AlunoId_CursoOnlineId",
                table: "Certificados",
                columns: new[] { "AlunoId", "CursoOnlineId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificados_CodigoValidacao",
                table: "Certificados",
                column: "CodigoValidacao",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificados_CursoOnlineId",
                table: "Certificados",
                column: "CursoOnlineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Certificados");

            migrationBuilder.DropColumn(
                name: "LogosCertificado",
                table: "CursosOnline");
        }
    }
}
