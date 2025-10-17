using Internado.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;

namespace Internado.Web.Controllers;

[Authorize(Policy = "Direccion")]
public class ReportesController : Controller
{
    private readonly InternadoDbContext _db;
    private readonly ILogger<ReportesController> _logger;

    public ReportesController(InternadoDbContext db, ILogger<ReportesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET: Panel de reportes
    public IActionResult Index()
    {
        return View();
    }

    // GET: Reporte de Asistencia
    public async Task<IActionResult> ReporteAsistencia(int? cursoId = null, DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        fechaInicio ??= DateTime.Today.AddMonths(-1);
        fechaFin ??= DateTime.Today;

        var fechaInicioOnly = DateOnly.FromDateTime(fechaInicio.Value);
        var fechaFinOnly = DateOnly.FromDateTime(fechaFin.Value);

        var query = _db.Asistencia
            .Include(a => a.Residente)
            .Include(a => a.Curso)
            .Where(a => a.Fecha >= fechaInicioOnly && a.Fecha <= fechaFinOnly);

        if (cursoId.HasValue)
            query = query.Where(a => a.CursoId == cursoId);

        var datos = await query.ToListAsync();

        // Agrupar por residente
        var resumen = datos
            .GroupBy(a => a.Residente)
            .Select(g => new
            {
                Residente = g.Key.NombreCompleto,
                Total = g.Count(),
                Presentes = g.Count(a => a.Estado == "Presente"),
                Ausentes = g.Count(a => a.Estado == "Ausente"),
                Tardes = g.Count(a => a.Estado == "Tarde"),
                PorcentajeAsistencia = g.Count() > 0 ? (g.Count(a => a.Estado == "Presente") * 100.0 / g.Count()) : 0
            })
            .OrderBy(r => r.Residente)
            .ToList();

        ViewBag.FechaInicio = fechaInicio;
        ViewBag.FechaFin = fechaFin;
        ViewBag.CursoId = cursoId;
        ViewBag.Cursos = await _db.Cursos.ToListAsync();

        return View(resumen);
    }

    // GET: Descargar Reporte Excel Asistencia
    public async Task<IActionResult> DescargarAsistenciaExcel(int? cursoId = null, DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        fechaInicio ??= DateTime.Today.AddMonths(-1);
        fechaFin ??= DateTime.Today;

        var fechaInicioOnly = DateOnly.FromDateTime(fechaInicio.Value);
        var fechaFinOnly = DateOnly.FromDateTime(fechaFin.Value);

        var query = _db.Asistencia
            .Include(a => a.Residente)
            .Include(a => a.Curso)
            .Where(a => a.Fecha >= fechaInicioOnly && a.Fecha <= fechaFinOnly);

        if (cursoId.HasValue)
            query = query.Where(a => a.CursoId == cursoId);

        var datos = await query.OrderByDescending(a => a.Fecha).ToListAsync();

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Asistencia");

            // Encabezados con estilo
            worksheet.Cell(1, 1).Value = "Fecha";
            worksheet.Cell(1, 2).Value = "Residente";
            worksheet.Cell(1, 3).Value = "Curso";
            worksheet.Cell(1, 4).Value = "Estado";
            worksheet.Cell(1, 5).Value = "Observación";

            // Estilo encabezados
            var headerRange = worksheet.Range(1, 1, 1, 5);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int row = 2;
            foreach (var item in datos)
            {
                worksheet.Cell(row, 1).Value = item.Fecha.ToString("dd/MM/yyyy");
                worksheet.Cell(row, 2).Value = item.Residente.NombreCompleto;
                worksheet.Cell(row, 3).Value = item.Curso.Nombre;
                worksheet.Cell(row, 4).Value = item.Estado;
                worksheet.Cell(row, 5).Value = item.Observacion ?? "";

                // Color según estado
                var estadoCell = worksheet.Cell(row, 4);
                if (item.Estado == "Presente")
                    estadoCell.Style.Fill.BackgroundColor = XLColor.LightGreen;
                else if (item.Estado == "Ausente")
                    estadoCell.Style.Fill.BackgroundColor = XLColor.LightPink;
                else if (item.Estado == "Tarde")
                    estadoCell.Style.Fill.BackgroundColor = XLColor.LightYellow;

                row++;
            }

            // Ajustar columnas
            worksheet.Columns().AdjustToContents();

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();
                return File(content,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Reporte_Asistencia_{DateTime.Now:yyyyMMdd}.xlsx");
            }
        }
    }

    // GET: Reporte de Calificaciones
    public async Task<IActionResult> ReporteCalificaciones(int? cursoId = null)
    {
        var query = _db.Calificaciones
            .Include(c => c.Residente)
            .Include(c => c.Curso)
            .AsQueryable();

        if (cursoId.HasValue)
            query = query.Where(c => c.CursoId == cursoId);

        var datos = await query.OrderBy(c => c.Residente.NombreCompleto).ToListAsync();

        ViewBag.CursoId = cursoId;
        ViewBag.Cursos = await _db.Cursos.ToListAsync();
        return View(datos);
    }

    // GET: Descargar Reporte Excel Calificaciones
    public async Task<IActionResult> DescargarCalificacionesExcel(int? cursoId = null)
    {
        var query = _db.Calificaciones
            .Include(c => c.Residente)
            .Include(c => c.Curso)
            .AsQueryable();

        if (cursoId.HasValue)
            query = query.Where(c => c.CursoId == cursoId);

        var datos = await query.OrderBy(c => c.Curso.Nombre)
            .ThenBy(c => c.Residente.NombreCompleto)
            .ToListAsync();

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Calificaciones");

            // Encabezados
            worksheet.Cell(1, 1).Value = "Residente";
            worksheet.Cell(1, 2).Value = "Curso";
            worksheet.Cell(1, 3).Value = "Nota";
            worksheet.Cell(1, 4).Value = "Fecha Registro";

            // Estilo encabezados
            var headerRange = worksheet.Range(1, 1, 1, 4);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int row = 2;
            foreach (var item in datos)
            {
                worksheet.Cell(row, 1).Value = item.Residente.NombreCompleto;
                worksheet.Cell(row, 2).Value = item.Curso.Nombre;
                worksheet.Cell(row, 3).Value = item.Nota;
                worksheet.Cell(row, 4).Value = item.FechaRegistro.ToString("dd/MM/yyyy");

                // Color según nota
                var notaCell = worksheet.Cell(row, 3);
                if (item.Nota >= 70)
                    notaCell.Style.Fill.BackgroundColor = XLColor.LightGreen;
                else if (item.Nota >= 60)
                    notaCell.Style.Fill.BackgroundColor = XLColor.LightYellow;
                else
                    notaCell.Style.Fill.BackgroundColor = XLColor.LightPink;

                row++;
            }

            // Ajustar columnas
            worksheet.Columns().AdjustToContents();

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();
                return File(content,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Reporte_Calificaciones_{DateTime.Now:yyyyMMdd}.xlsx");
            }
        }
    }
}
