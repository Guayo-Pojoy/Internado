using Internado.Infrastructure.Data;
using Internado.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Internado.Web.Controllers;

[Authorize(Policy = "Docente")]
public class CalificacionesController : Controller
{
    private readonly InternadoDbContext _db;
    private readonly ILogger<CalificacionesController> _logger;

    public CalificacionesController(InternadoDbContext db, ILogger<CalificacionesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET: Listar cursos del docente
    public async Task<IActionResult> Index()
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

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

    // GET: Cargar calificaciones de un curso
    public async Task<IActionResult> CargarCalificaciones(int cursoId)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var curso = await _db.Cursos
            .Include(c => c.Calificaciones)
            .ThenInclude(cal => cal.Residente)
            .Include(c => c.AsignacionesDocentes)
            .Include(c => c.Matriculas)
            .ThenInclude(m => m.Residente)
            .ThenInclude(r => r.Grado)
            .FirstOrDefaultAsync(c => c.Id == cursoId);

        if (curso == null)
            return NotFound("Curso no encontrado.");

        // Validar que el docente está asignado a este curso
        var tieneAcceso = curso.AsignacionesDocentes.Any(ad => ad.DocenteId == usuarioId && ad.Activa);
        if (!tieneAcceso)
            return Unauthorized("No tienes permiso para este curso.");

        // Obtener residentes matriculados en este curso
        var residentesMatriculados = curso.Matriculas
            .Where(m => m.Activa)
            .Select(m => m.Residente)
            .Distinct()
            .ToList();

        ViewBag.Curso = curso;
        ViewBag.ResidentesMatriculados = residentesMatriculados;

        return View(curso);
    }

    // POST: Guardar calificación
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuardarCalificacion(int residenteId, int cursoId, decimal nota)
    {
        if (nota < 0 || nota > 100)
        {
            TempData["Error"] = "La nota debe estar entre 0 y 100.";
            return RedirectToAction("CargarCalificaciones", new { cursoId });
        }

        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var curso = await _db.Cursos
            .Include(c => c.AsignacionesDocentes)
            .Include(c => c.Matriculas)
            .FirstOrDefaultAsync(c => c.Id == cursoId);

        if (curso == null)
            return NotFound();

        // 1. Validar que el docente está asignado al curso
        var docente_valido = curso.AsignacionesDocentes.Any(ad => ad.DocenteId == usuarioId && ad.Activa);
        if (!docente_valido)
            return Unauthorized("No tienes permiso para este curso.");

        // 2. Validar que el residente está matriculado en este curso
        var matricula_valida = curso.Matriculas.Any(m => m.ResidenteId == residenteId && m.Activa);
        if (!matricula_valida)
        {
            TempData["Error"] = "El residente no está matriculado en este curso.";
            return RedirectToAction("CargarCalificaciones", new { cursoId });
        }

        var calificacion = await _db.Calificaciones
            .FirstOrDefaultAsync(c => c.ResidenteId == residenteId && c.CursoId == cursoId);

        if (calificacion == null)
        {
            calificacion = new Calificacione
            {
                ResidenteId = residenteId,
                CursoId = cursoId,
                Nota = nota,
                FechaRegistro = DateTime.UtcNow
            };
            _db.Calificaciones.Add(calificacion);
        }
        else
        {
            calificacion.Nota = nota;
            calificacion.FechaRegistro = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation($"Calificación: Residente {residenteId}, Curso {cursoId}, Nota {nota}");

        TempData["Success"] = "Calificación guardada correctamente.";
        return RedirectToAction("CargarCalificaciones", new { cursoId });
    }

    // GET: Ver calificaciones de un residente
    public async Task<IActionResult> VerCalificacionesResidente(int residenteId)
    {
        var residente = await _db.Residentes
            .Include(r => r.Calificaciones)
            .ThenInclude(c => c.Curso)
            .FirstOrDefaultAsync(r => r.Id == residenteId);

        if (residente == null)
            return NotFound();

        return View(residente);
    }
}
