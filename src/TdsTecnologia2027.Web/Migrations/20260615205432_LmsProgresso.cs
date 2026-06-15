using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TdsTecnologia2027.Web.Migrations
{
    /// <inheritdoc />
    public partial class LmsProgresso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProgressosLicao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlunoId = table.Column<int>(type: "INTEGER", nullable: false),
                    LicaoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    TempoAssistidoSegundos = table.Column<int>(type: "INTEGER", nullable: false),
                    UltimoAcesso = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressosLicao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgressosLicao_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgressosLicao_Licoes_LicaoId",
                        column: x => x.LicaoId,
                        principalTable: "Licoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProgressosLicao_AlunoId_LicaoId",
                table: "ProgressosLicao",
                columns: new[] { "AlunoId", "LicaoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgressosLicao_LicaoId",
                table: "ProgressosLicao",
                column: "LicaoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgressosLicao");
        }
    }
}
