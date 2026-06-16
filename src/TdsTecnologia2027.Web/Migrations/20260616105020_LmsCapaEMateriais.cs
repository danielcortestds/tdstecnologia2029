using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TdsTecnologia2027.Web.Migrations
{
    /// <inheritdoc />
    public partial class LmsCapaEMateriais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CorCapa",
                table: "CursosOnline",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IconeCapa",
                table: "CursosOnline",
                type: "TEXT",
                maxLength: 40,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MateriaisApoio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CursoOnlineId = table.Column<int>(type: "INTEGER", nullable: false),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    NomeArquivo = table.Column<string>(type: "TEXT", maxLength: 260, nullable: false),
                    ArquivoArmazenado = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    TamanhoBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MateriaisApoio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MateriaisApoio_CursosOnline_CursoOnlineId",
                        column: x => x.CursoOnlineId,
                        principalTable: "CursosOnline",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MateriaisApoio_CursoOnlineId",
                table: "MateriaisApoio",
                column: "CursoOnlineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MateriaisApoio");

            migrationBuilder.DropColumn(
                name: "CorCapa",
                table: "CursosOnline");

            migrationBuilder.DropColumn(
                name: "IconeCapa",
                table: "CursosOnline");
        }
    }
}
