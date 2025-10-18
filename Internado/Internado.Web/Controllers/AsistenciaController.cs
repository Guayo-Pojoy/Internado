using Internado.Infrastructure.Data;
using Internado.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Internado.Web.Controllers;

[Authorize(Policy = "Docente")]
public class AsistenciaController : Controller
{
    private readonly InternadoDbContext _db;
    private readonly ILogger<AsistenciaController> _logger;

    public AsistenciaController(InternadoDbContext db, ILogger<AsistenciaController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET: Seleccionar curso para registrar asistencia
    public async Task<IActionResult> Index()
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Obtener grados con sus cursos asignados al docente
        var grados = await _db.Grados
            .Where(g => g.Estado == "Activo")
            .Include(g => g.GradoCursos)
            .ThenInclude(gc => gc.Curso)
            .ThenInclude(c => c.AsignacionesDocentes)
            .ToListAsync();

        // Filtrar solo grados que tienen cursos asignados a este docente
        var gradosConCursos = grados
            .Where(g => g.GradoCursos.Any(gc =>
                gc.Activa &&
                gc.Curso.AsignacionesDocentes.Any(ad => ad.DocenteId == usuarioId && ad.Activa)))
            .ToList();

        ViewBag.Grados = gradosConCursos;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerCursosPorGrado(int gradoId)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var cursos = await _db.GradoCursos
            .Where(gc => gc.GradoId == gradoId && gc.Activa)
            .Include(gc => gc.Curso)
            .ThenInclude(c => c.AsignacionesDocentes)
            .Select(gc => gc.Curso)
            .Where(c => c.AsignacionesDocentes.Any(ad => ad.DocenteId == usuarioId && ad.Activa))
            .ToListAsync();

        return Json(cursos.Select(c => new { id = c.Id, nombre = c.Nombre }));
    }

    // GET: Formulario de asistencia
    public async Task<IActionResult> RegistrarAsistencia(int cursoId, DateTime? fecha = null)
    {
        fecha ??= DateTime.Today;
        var fechaOnly = DateOnly.FromDateTime(fecha.Value);

        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var curso = await _db.Cursos
            .Include(c => c.Asistencia)
            .Include(c => c.AsignacionesDocentes)
            .Include(c => c.Matriculas)
            .ThenInclude(m => m.Residente)
            .ThenInclude(r => r.Grado)
            .FirstOrDefaultAsync(c => c.Id == cursoId);

        if (curso == null)
            return NotFound();

        // Validar que el docente está asignado
        var tieneAcceso = curso.AsignacionesDocentes.Any(ad => ad.DocenteId == usuarioId && ad.Activa);
        if (!tieneAcceso)
            return Unauthorized("No tienes permiso para este curso.");

        // Obtener residentes matriculados en el curso
        var residentesMatriculados = curso.Matriculas
            .Where(m => m.Activa)
            .Select(m => m.Residente)
            .Distinct()
            .OrderBy(r => r.NombreCompleto)
            .ToList();

        var asistenciasDelDia = curso.Asistencia
            .Where(a => a.Fecha == fechaOnly)
            .ToDictionary(a => a.ResidenteId, a => a.Estado);

        var observacionesDelDia = curso.Asistencia
            .Where(a => a.Fecha == fechaOnly)
            .ToDictionary(a => a.ResidenteId, a => a.Observacion ?? "");

        ViewBag.Curso = curso;
        ViewBag.Fecha = fecha;
        ViewBag.Asistencias = asistenciasDelDia;
        ViewBag.Observaciones = observacionesDelDia;

        return View(residentesMatriculados);
    }

    // POST: Guardar asistencia en lote
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuardarAsistencia(int cursoId, DateTime fecha,
        Dictionary<string, string> asistencia, Dictionary<string, string>? excusa = null)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var curso = await _db.Cursos
            .Include(c => c.AsignacionesDocentes)
            .Include(c => c.Matriculas)
            .FirstOrDefaultAsync(c => c.Id == cursoId);

        if (curso == null)
            return Unauthorized();

        // 1. Validar acceso del docente
        var tieneAcceso = curso.AsignacionesDocentes.Any(ad => ad.DocenteId == usuarioId && ad.Activa);
        if (!tieneAcceso)
            return Unauthorized("No tienes permiso para este curso.");

        var fechaOnly = DateOnly.FromDateTime(fecha);
        var estados = new[] { "Presente", "Ausente", "Tarde" };

        foreach (var item in asistencia)
        {
            if (int.TryParse(item.Key.Replace("residente_", ""), out var residenteId) &&
                estados.Contains(item.Value))
            {
                // 2. Validar que el residente está matriculado
                var matricula_valida = curso.Matriculas.Any(m => m.ResidenteId == residenteId && m.Activa);
                if (!matricula_valida)
                    continue; // Ignorar si no está matriculado

                // Obtener la excusa si existe
                var observacion = excusa?.TryGetValue(item.Key, out var obs) == true ? obs?.Trim() : null;
                if (string.IsNullOrWhiteSpace(observacion))
                    observacion = null;

                var existente = await _db.Asistencia
                    .FirstOrDefaultAsync(a => a.ResidenteId == residenteId &&
                                               a.CursoId == cursoId &&
                                               a.Fecha == fechaOnly);

                if (existente == null)
                {
                    var nuevaAsistencia = new Asistencium
                    {
                        ResidenteId = residenteId,
                        CursoId = cursoId,
                        Fecha = fechaOnly,
                        Estado = item.Value,
                        Observacion = observacion
                    };
                    _db.Asistencia.Add(nuevaAsistencia);
                }
                else
                {
                    existente.Estado = item.Value;
                    existente.Observacion = observacion;
                }
            }
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation($"Asistencia guardada: Curso {cursoId}, Fecha {fecha}");

        TempData["Success"] = "Asistencia registrada correctamente.";
        return RedirectToAction("RegistrarAsistencia", new { cursoId, fecha });
    }

    // GET: Ver historial de asistencia de un residente
    public async Task<IActionResult> HistorialResidente(int residenteId)
    {
        var residente = await _db.Residentes
            .Include(r => r.Asistencia)
            .ThenInclude(a => a.Curso)
            .FirstOrDefaultAsync(r => r.Id == residenteId);

        if (residente == null)
            return NotFound();

        return View(residente);
    }
}
