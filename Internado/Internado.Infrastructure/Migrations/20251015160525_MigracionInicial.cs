using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Internado.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MigracionInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "aca");

            migrationBuilder.EnsureSchema(
                name: "sec");

            migrationBuilder.EnsureSchema(
                name: "med");

            migrationBuilder.CreateTable(
                name: "AuditoriaAccesos",
                schema: "sec",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    FechaUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    IP = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    Accion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Resultado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditoriaAccesos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Habitaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Capacidad = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Piso = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Edificio = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Habitaciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Medicamentos",
                schema: "med",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Lote = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaVencimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    StockActual = table.Column<int>(type: "int", nullable: false),
                    StockMinimo = table.Column<int>(type: "int", nullable: false, defaultValue: 10)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicamentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Periodos",
                schema: "aca",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Anio = table.Column<int>(type: "int", nullable: false),
                    Trimestre = table.Column<byte>(type: "tinyint", nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Periodos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "sec",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreRol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Residentes",
                schema: "aca",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreCompleto = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    DPI = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Tutor = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaNacimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaIngreso = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaEgreso = table.Column<DateOnly>(type: "date", nullable: true),
                    HabitacionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Residentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Residentes_Habitaciones_HabitacionId",
                        column: x => x.HabitacionId,
                        principalTable: "Habitaciones",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                schema: "sec",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    HashContrasena = table.Column<byte[]>(type: "varbinary(256)", maxLength: 256, nullable: false),
                    RolId = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    IntentosFallidos = table.Column<int>(type: "int", nullable: false),
                    BloqueadoHastaUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Roles",
                        column: x => x.RolId,
                        principalSchema: "sec",
                        principalTable: "Roles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HistorialAcademico",
                schema: "aca",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResidenteId = table.Column<int>(type: "int", nullable: false),
                    Anio = table.Column<int>(type: "int", nullable: false),
                    Promedio = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialAcademico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HA_Res",
                        column: x => x.ResidenteId,
                        principalSchema: "aca",
                        principalTable: "Residentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistorialMedico",
                schema: "med",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResidenteId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    Diagnostico = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Tratamiento = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialMedico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HM_Res",
                        column: x => x.ResidenteId,
                        principalSchema: "aca",
                        principalTable: "Residentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Consultas",
                schema: "med",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResidenteId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    Diagnostico = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Tratamiento = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    MedicoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consultas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cons_Med",
                        column: x => x.MedicoId,
                        principalSchema: "sec",
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Cons_Res",
                        column: x => x.ResidenteId,
                        principalSchema: "aca",
                        principalTable: "Residentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cursos",
                schema: "aca",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DocenteId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cursos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cursos_Docente",
                        column: x => x.DocenteId,
                        principalSchema: "sec",
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MovimientosMedicamentos",
                schema: "med",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicamentoId = table.Column<int>(type: "int", nullable: false),
                    TipoMovimiento = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosMed", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MM_Med",
                        column: x => x.MedicamentoId,
                        principalSchema: "med",
                        principalTable: "Medicamentos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MM_Usr",
                        column: x => x.UsuarioId,
                        principalSchema: "sec",
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Asistencia",
                schema: "aca",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResidenteId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    CursoId = table.Column<int>(type: "int", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asistencia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asistencia_Curso",
                        column: x => x.CursoId,
                        principalSchema: "aca",
                        principalTable: "Cursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Asistencia_Res",
                        column: x => x.ResidenteId,
                        principalSchema: "aca",
                        principalTable: "Residentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Calificaciones",
                schema: "aca",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResidenteId = table.Column<int>(type: "int", nullable: false),
                    CursoId = table.Column<int>(type: "int", nullable: false),
                    Nota = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calificaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Calif_Curso",
                        column: x => x.CursoId,
                        principalSchema: "aca",
                        principalTable: "Cursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Calif_Res",
                        column: x => x.ResidenteId,
                        principalSchema: "aca",
                        principalTable: "Residentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Asistencia_Curso_Fecha",
                schema: "aca",
                table: "Asistencia",
                columns: new[] { "CursoId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "UQ_Asistencia",
                schema: "aca",
                table: "Asistencia",
                columns: new[] { "ResidenteId", "Fecha", "CursoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Auditoria_Fecha",
                schema: "sec",
                table: "AuditoriaAccesos",
                column: "FechaUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Auditoria_Usuario",
                schema: "sec",
                table: "AuditoriaAccesos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Calif_Res_Curso",
                schema: "aca",
                table: "Calificaciones",
                columns: new[] { "ResidenteId", "CursoId" });

            migrationBuilder.CreateIndex(
                name: "IX_Calificaciones_CursoId",
                schema: "aca",
                table: "Calificaciones",
                column: "CursoId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultas_Medico_Fecha",
                schema: "med",
                table: "Consultas",
                columns: new[] { "MedicoId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_Consultas_Res_Fecha",
                schema: "med",
                table: "Consultas",
                columns: new[] { "ResidenteId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_DocenteId",
                schema: "aca",
                table: "Cursos",
                column: "DocenteId");

            migrationBuilder.CreateIndex(
                name: "UQ_HA_Res_Anio",
                schema: "aca",
                table: "HistorialAcademico",
                columns: new[] { "ResidenteId", "Anio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HM_Res_Fecha",
                schema: "med",
                table: "HistorialMedico",
                columns: new[] { "ResidenteId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_Medicamentos_Vencimiento",
                schema: "med",
                table: "Medicamentos",
                column: "FechaVencimiento");

            migrationBuilder.CreateIndex(
                name: "IX_MM_Med_Fecha",
                schema: "med",
                table: "MovimientosMedicamentos",
                columns: new[] { "MedicamentoId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosMedicamentos_UsuarioId",
                schema: "med",
                table: "MovimientosMedicamentos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "UQ_Periodo",
                schema: "aca",
                table: "Periodos",
                columns: new[] { "Anio", "Trimestre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Residentes_HabitacionId",
                schema: "aca",
                table: "Residentes",
                column: "HabitacionId");

            migrationBuilder.CreateIndex(
                name: "UQ_Roles_Nombre",
                schema: "sec",
                table: "Roles",
                column: "NombreRol",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Correo",
                schema: "sec",
                table: "Usuarios",
                column: "Correo");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_RolId",
                schema: "sec",
                table: "Usuarios",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Usuario",
                schema: "sec",
                table: "Usuarios",
                column: "Usuario");

            migrationBuilder.CreateIndex(
                name: "UQ_Usuarios_Correo",
                schema: "sec",
                table: "Usuarios",
                column: "Correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Usuarios_Usuario",
                schema: "sec",
                table: "Usuarios",
                column: "Usuario",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Asistencia",
                schema: "aca");

            migrationBuilder.DropTable(
                name: "AuditoriaAccesos",
                schema: "sec");

            migrationBuilder.DropTable(
                name: "Calificaciones",
                schema: "aca");

            migrationBuilder.DropTable(
                name: "Consultas",
                schema: "med");

            migrationBuilder.DropTable(
                name: "HistorialAcademico",
                schema: "aca");

            migrationBuilder.DropTable(
                name: "HistorialMedico",
                schema: "med");

            migrationBuilder.DropTable(
                name: "MovimientosMedicamentos",
                schema: "med");

            migrationBuilder.DropTable(
                name: "Periodos",
                schema: "aca");

            migrationBuilder.DropTable(
                name: "Cursos",
                schema: "aca");

            migrationBuilder.DropTable(
                name: "Residentes",
                schema: "aca");

            migrationBuilder.DropTable(
                name: "Medicamentos",
                schema: "med");

            migrationBuilder.DropTable(
                name: "Usuarios",
                schema: "sec");

            migrationBuilder.DropTable(
                name: "Habitaciones");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "sec");
        }
    }
}
