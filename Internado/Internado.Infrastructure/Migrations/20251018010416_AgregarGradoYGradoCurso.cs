using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Internado.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarGradoYGradoCurso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GradoId",
                schema: "aca",
                table: "Residentes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Grados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Nivel = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Activo"),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grados", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GradoCursos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GradoId = table.Column<int>(type: "int", nullable: false),
                    CursoId = table.Column<int>(type: "int", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradoCursos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GradoCurso_Curso",
                        column: x => x.CursoId,
                        principalSchema: "aca",
                        principalTable: "Cursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GradoCurso_Grado",
                        column: x => x.GradoId,
                        principalTable: "Grados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Residentes_GradoId",
                schema: "aca",
                table: "Residentes",
                column: "GradoId");

            migrationBuilder.CreateIndex(
                name: "IX_GradoCursos_CursoId",
                table: "GradoCursos",
                column: "CursoId");

            migrationBuilder.CreateIndex(
                name: "IX_GradoCursos_GradoId_CursoId",
                table: "GradoCursos",
                columns: new[] { "GradoId", "CursoId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Residente_Grado",
                schema: "aca",
                table: "Residentes",
                column: "GradoId",
                principalTable: "Grados",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Residente_Grado",
                schema: "aca",
                table: "Residentes");

            migrationBuilder.DropTable(
                name: "GradoCursos");

            migrationBuilder.DropTable(
                name: "Grados");

            migrationBuilder.DropIndex(
                name: "IX_Residentes_GradoId",
                schema: "aca",
                table: "Residentes");

            migrationBuilder.DropColumn(
                name: "GradoId",
                schema: "aca",
                table: "Residentes");
        }
    }
}
