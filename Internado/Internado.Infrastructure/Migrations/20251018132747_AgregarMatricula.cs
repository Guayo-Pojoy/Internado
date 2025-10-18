using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Internado.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarMatricula : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cursos_Docente",
                schema: "aca",
                table: "Cursos");

            migrationBuilder.DropIndex(
                name: "IX_Cursos_DocenteId",
                schema: "aca",
                table: "Cursos");

            migrationBuilder.DropColumn(
                name: "DocenteId",
                schema: "aca",
                table: "Cursos");

            migrationBuilder.CreateTable(
                name: "Matriculas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResidenteId = table.Column<int>(type: "int", nullable: false),
                    CursoId = table.Column<int>(type: "int", nullable: false),
                    Periodo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FechaMatricula = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    Activa = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Razon = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matriculas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matricula_Curso",
                        column: x => x.CursoId,
                        principalSchema: "aca",
                        principalTable: "Cursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Matricula_Residente",
                        column: x => x.ResidenteId,
                        principalSchema: "aca",
                        principalTable: "Residentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Matriculas_CursoId",
                table: "Matriculas",
                column: "CursoId");

            migrationBuilder.CreateIndex(
                name: "IX_Matriculas_ResidenteId_CursoId_Periodo",
                table: "Matriculas",
                columns: new[] { "ResidenteId", "CursoId", "Periodo" },
                unique: true,
                filter: "[Periodo] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Matriculas");

            migrationBuilder.AddColumn<int>(
                name: "DocenteId",
                schema: "aca",
                table: "Cursos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_DocenteId",
                schema: "aca",
                table: "Cursos",
                column: "DocenteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cursos_Docente",
                schema: "aca",
                table: "Cursos",
                column: "DocenteId",
                principalSchema: "sec",
                principalTable: "Usuarios",
                principalColumn: "Id");
        }
    }
}
