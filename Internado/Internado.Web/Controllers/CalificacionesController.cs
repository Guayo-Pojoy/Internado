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

        // Solo obtener cursos asignados al docente actual
        var cursos = await _db.Cursos
            .Where(c => c.AsignacionesDocentes.Any(ad => ad.DocenteId == usuarioId && ad.Activa))
            .Include(c => c.Calificaciones)
            .ToListAsync();

        return View(cursos);
    }

    // GET: Cargar calificaciones de un curso
    public async Task<IActionResult> CargarCalificaciones(int cursoId)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var curso = await _db.Cursos
            .Include(c => c.Calificaciones)
            .ThenInclude(cal => cal.Residente)
            .Include(c => c.AsignacionesDocentes)
            .FirstOrDefaultAsync(c => c.Id == cursoId);

        if (curso == null)
            return NotFound("Curso no encontrado.");

        // Validar que el docente está asignado a este curso
        var tieneAcceso = curso.AsignacionesDocentes.Any(ad => ad.DocenteId == usuarioId && ad.Activa);
        if (!tieneAcceso)
            return Unauthorized("No tienes permiso para este curso.");

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
            .FirstOrDefaultAsync(c => c.Id == cursoId);

        if (curso == null)
            return NotFound();

        // Validar acceso al curso
        var tieneAcceso = curso.AsignacionesDocentes.Any(ad => ad.DocenteId == usuarioId && ad.Activa);
        if (!tieneAcceso)
            return Unauthorized("No tienes permiso para este curso.");

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
        _logger.LogInformation($"Calificación guardada: Residente {residenteId}, Curso {cursoId}, Nota {nota}");

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
