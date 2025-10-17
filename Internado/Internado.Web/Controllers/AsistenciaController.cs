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
        var cursos = await _db.Cursos
            .Where(c => c.DocenteId == usuarioId)
            .ToListAsync();

        return View(cursos);
    }

    // GET: Formulario de asistencia
    public async Task<IActionResult> RegistrarAsistencia(int cursoId, DateTime? fecha = null)
    {
        fecha ??= DateTime.Today;
        var fechaOnly = DateOnly.FromDateTime(fecha.Value);

        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var curso = await _db.Cursos
            .Include(c => c.Asistencia)
            .FirstOrDefaultAsync(c => c.Id == cursoId && c.DocenteId == usuarioId);

        if (curso == null)
            return NotFound();

        var residentes = await _db.Residentes
            .Where(r => r.Estado == "Activa")
            .ToListAsync();

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

        return View(residentes);
    }

    // POST: Guardar asistencia en lote
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuardarAsistencia(int cursoId, DateTime fecha,
        Dictionary<string, string> asistencia, Dictionary<string, string>? excusa = null)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var curso = await _db.Cursos.FirstOrDefaultAsync(c => c.Id == cursoId && c.DocenteId == usuarioId);

        if (curso == null)
            return Unauthorized();

        var fechaOnly = DateOnly.FromDateTime(fecha);
        var estados = new[] { "Presente", "Ausente", "Tarde" };

        foreach (var item in asistencia)
        {
            if (int.TryParse(item.Key.Replace("residente_", ""), out var residenteId) &&
                estados.Contains(item.Value))
            {
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
        _logger.LogInformation($"Asistencia guardada para curso {cursoId} en fecha {fecha}");

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
