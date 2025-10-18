using Internado.Infrastructure.Data;
using Internado.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Internado.Web.Controllers;

[Authorize]
public class CursosController : Controller
{
    private readonly InternadoDbContext _db;
    private readonly ILogger<CursosController> _logger;

    public CursosController(InternadoDbContext db, ILogger<CursosController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET: Listar cursos (Admin y Docentes ven sus cursos)
    [Authorize(Policy = "Docente")]
    public async Task<IActionResult> Index()
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

        IQueryable<Curso> cursos;

        if (userRole == "Administrador")
        {
            // Admin ve todos los cursos
            cursos = _db.Cursos
                .Include(c => c.AsignacionesDocentes)
                    .ThenInclude(ad => ad.Docente);
        }
        else
        {
            // Docente solo ve sus cursos asignados
            cursos = _db.Cursos
                .Where(c => c.AsignacionesDocentes.Any(ad => ad.DocenteId == usuarioId && ad.Activa))
                .Include(c => c.AsignacionesDocentes)
                    .ThenInclude(ad => ad.Docente);
        }

        return View(await cursos.ToListAsync());
    }

    // GET: Crear nuevo curso (Solo Admin)
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create()
    {
        var docentes = await _db.Usuarios
            .Include(u => u.Rol)
            .Where(u => u.Rol.NombreRol == "Docente" && u.Estado)
            .ToListAsync();

        ViewBag.Docentes = docentes;
        return View();
    }

    // POST: Guardar nuevo curso (Solo Admin)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create(string nombre, int docentePrincipalId, int[] docenteIds)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            ModelState.AddModelError("nombre", "El nombre del curso es requerido.");
            var docentes = await _db.Usuarios
                .Include(u => u.Rol)
                .Where(u => u.Rol.NombreRol == "Docente" && u.Estado)
                .ToListAsync();
            ViewBag.Docentes = docentes;
            return View();
        }

        if (docentePrincipalId == 0)
        {
            ModelState.AddModelError("docentePrincipalId", "Debe seleccionar un docente principal.");
            var docentes = await _db.Usuarios
                .Include(u => u.Rol)
                .Where(u => u.Rol.NombreRol == "Docente" && u.Estado)
                .ToListAsync();
            ViewBag.Docentes = docentes;
            return View();
        }

        var curso = new Curso
        {
            Nombre = nombre
        };

        _db.Cursos.Add(curso);
        await _db.SaveChangesAsync();

        // Asignar docente principal
        var asignacionPrincipal = new DocenteCurso
        {
            DocenteId = docentePrincipalId,
            CursoId = curso.Id,
            FechaAsignacion = DateTime.UtcNow,
            Activa = true
        };
        _db.DocenteCursos.Add(asignacionPrincipal);

        // Asignar docentes colaboradores
        if (docenteIds != null && docenteIds.Length > 0)
        {
            foreach (var docenteId in docenteIds)
            {
                // Evitar duplicar el docente principal
                if (docenteId == docentePrincipalId)
                    continue;

                var asignacion = new DocenteCurso
                {
                    DocenteId = docenteId,
                    CursoId = curso.Id,
                    FechaAsignacion = DateTime.UtcNow,
                    Activa = true
                };
                _db.DocenteCursos.Add(asignacion);
            }
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation($"Curso creado: {nombre}");
        TempData["Success"] = "Curso creado correctamente.";
        return RedirectToAction("Index");
    }

    // GET: Editar curso (Solo Admin)
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Edit(int id)
    {
        var curso = await _db.Cursos
            .Include(c => c.AsignacionesDocentes)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (curso == null)
            return NotFound();

        var docentes = await _db.Usuarios
            .Include(u => u.Rol)
            .Where(u => u.Rol.NombreRol == "Docente" && u.Estado)
            .ToListAsync();

        ViewBag.Docentes = docentes;
        ViewBag.DocentesAsignados = curso.AsignacionesDocentes.Where(ad => ad.Activa).Select(ad => ad.DocenteId).ToList();

        return View(curso);
    }

    // POST: Actualizar curso y asignaciones (Solo Admin)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Edit(int id, string nombre, int docentePrincipalId, int[] docenteIds)
    {
        var curso = await _db.Cursos
            .Include(c => c.AsignacionesDocentes)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (curso == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            ModelState.AddModelError("nombre", "El nombre del curso es requerido.");
            var docentes = await _db.Usuarios
                .Include(u => u.Rol)
                .Where(u => u.Rol.NombreRol == "Docente" && u.Estado)
                .ToListAsync();
            ViewBag.Docentes = docentes;
            ViewBag.DocentesAsignados = curso.AsignacionesDocentes.Where(ad => ad.Activa).Select(ad => ad.DocenteId).ToList();
            return View(curso);
        }

        curso.Nombre = nombre;

        // Obtener asignaciones actuales
        var asignacionesActuales = curso.AsignacionesDocentes.Where(ad => ad.Activa).ToList();
        var docentesActuales = asignacionesActuales.Select(ad => ad.DocenteId).ToList();

        // Agregar docente principal si no está
        var docentesSeleccionados = new List<int>();
        docentesSeleccionados.Add(docentePrincipalId);
        if (docenteIds != null && docenteIds.Length > 0)
        {
            docentesSeleccionados.AddRange(docenteIds.Where(id => id != docentePrincipalId));
        }

        // Docentes a agregar
        var docentesNuevos = docentesSeleccionados
            .Except(docentesActuales)
            .ToList();

        foreach (var docenteId in docentesNuevos)
        {
            var asignacion = new DocenteCurso
            {
                DocenteId = docenteId,
                CursoId = curso.Id,
                FechaAsignacion = DateTime.UtcNow,
                Activa = true
            };
            _db.DocenteCursos.Add(asignacion);
        }

        // Docentes a quitar
        var docentesQuitar = docentesActuales
            .Except(docentesSeleccionados)
            .ToList();

        foreach (var docenteId in docentesQuitar)
        {
            var asignacion = asignacionesActuales.First(ad => ad.DocenteId == docenteId);
            asignacion.Activa = false;
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation($"Curso actualizado: {nombre}");
        TempData["Success"] = "Curso actualizado correctamente.";
        return RedirectToAction("Index");
    }

    // GET: Ver detalles del curso
    [Authorize(Policy = "Docente")]
    public async Task<IActionResult> Details(int id)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

        var curso = await _db.Cursos
            .Include(c => c.AsignacionesDocentes)
                .ThenInclude(ad => ad.Docente)
            .Include(c => c.Calificaciones)
            .Include(c => c.Asistencia)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (curso == null)
            return NotFound();

        // Validar acceso
        if (userRole != "Administrador" &&
            !curso.AsignacionesDocentes.Any(ad => ad.DocenteId == usuarioId && ad.Activa))
        {
            return Forbid();
        }

        return View(curso);
    }

    // POST: Eliminar curso (Solo Admin)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        var curso = await _db.Cursos.FindAsync(id);

        if (curso != null)
        {
            _db.Cursos.Remove(curso);
            await _db.SaveChangesAsync();
            _logger.LogInformation($"Curso eliminado: {curso.Nombre}");
        }

        TempData["Success"] = "Curso eliminado correctamente.";
        return RedirectToAction("Index");
    }
}
