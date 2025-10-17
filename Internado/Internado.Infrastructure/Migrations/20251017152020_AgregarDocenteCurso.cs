using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Internado.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDocenteCurso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocenteCursos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocenteId = table.Column<int>(type: "int", nullable: false),
                    CursoId = table.Column<int>(type: "int", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    Activa = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocenteCursos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocenteCurso_Curso",
                        column: x => x.CursoId,
                        principalSchema: "aca",
                        principalTable: "Cursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocenteCurso_Usuario",
                        column: x => x.DocenteId,
                        principalSchema: "sec",
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocenteCursos_CursoId",
                table: "DocenteCursos",
                column: "CursoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocenteCursos_DocenteId_CursoId",
                table: "DocenteCursos",
                columns: new[] { "DocenteId", "CursoId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocenteCursos");
        }
    }
}
