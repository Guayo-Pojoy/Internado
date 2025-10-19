using Internado.Infrastructure.Data;
using Internado.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Internado.Web.Controllers;

[Authorize(Policy = "AdminOnly")]
public class GradosController : Controller
{
    private readonly InternadoDbContext _db;
    private readonly ILogger<GradosController> _logger;

    public GradosController(InternadoDbContext db, ILogger<GradosController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET: Listar grados
    public async Task<IActionResult> Index()
    {
        var grados = await _db.Grados
            .Include(g => g.GradoCursos)
            .OrderBy(g => g.Nivel)
            .ToListAsync();

        return View(grados);
    }

    // GET: Crear grado
    public async Task<IActionResult> Create()
    {
        // Cargar todos los cursos disponibles para asignación
        var cursos = await _db.Cursos.OrderBy(c => c.Nombre).ToListAsync();
        ViewBag.Cursos = cursos;

        return View();
    }

    // POST: Guardar grado
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string nombre, int nivel, string? descripcion, int[] cursoIds)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            ModelState.AddModelError("nombre", "El nombre del grado es requerido.");
            var cursos = await _db.Cursos.OrderBy(c => c.Nombre).ToListAsync();
            ViewBag.Cursos = cursos;
            return View();
        }

        // Validar que el nivel sea positivo
        if (nivel < 1)
        {
            ModelState.AddModelError("nivel", "El nivel debe ser mayor a 0.");
            var cursos = await _db.Cursos.OrderBy(c => c.Nombre).ToListAsync();
            ViewBag.Cursos = cursos;
            return View();
        }

        // Validar que no existe grado con el mismo nombre
        var existe = await _db.Grados.AnyAsync(g => g.Nombre == nombre);
        if (existe)
        {
            ModelState.AddModelError("nombre", "Ya existe un grado con este nombre.");
            var cursos = await _db.Cursos.OrderBy(c => c.Nombre).ToListAsync();
            ViewBag.Cursos = cursos;
            return View();
        }

        var grado = new Grado
        {
            Nombre = nombre,
            Nivel = nivel,
            Descripcion = descripcion,
            Estado = "Activo",
            FechaCreacion = DateTime.UtcNow
        };

        _db.Grados.Add(grado);
        await _db.SaveChangesAsync();

        // Asignar los cursos seleccionados al grado recién creado
        if (cursoIds != null && cursoIds.Length > 0)
        {
            foreach (var cursoId in cursoIds)
            {
                var gradoCurso = new GradoCurso
                {
                    GradoId = grado.Id,
                    CursoId = cursoId,
                    Activa = true,
                    FechaAsignacion = DateTime.UtcNow
                };
                _db.GradoCursos.Add(gradoCurso);
            }
            await _db.SaveChangesAsync();
            _logger.LogInformation($"Grado creado: {nombre} con {cursoIds.Length} cursos asignados");
        }
        else
        {
            _logger.LogInformation($"Grado creado: {nombre} sin cursos asignados");
        }

        TempData["Success"] = "Grado creado correctamente.";
        return RedirectToAction("Index");
    }

    // GET: Editar grado
    public async Task<IActionResult> Edit(int id)
    {
        var grado = await _db.Grados
            .Include(g => g.GradoCursos)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (grado == null)
            return NotFound();

        return View(grado);
    }

    // POST: Actualizar grado
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string nombre, int nivel, string? descripcion)
    {
        var grado = await _db.Grados.FindAsync(id);

        if (grado == null)
            return NotFound();

        grado.Nombre = nombre;
        grado.Nivel = nivel;
        grado.Descripcion = descripcion;

        await _db.SaveChangesAsync();
        _logger.LogInformation($"Grado actualizado: {nombre}");

        TempData["Success"] = "Grado actualizado correctamente.";
        return RedirectToAction("Index");
    }

    // GET: Asignar cursos a grado
    public async Task<IActionResult> AsignarCursos(int id)
    {
        var grado = await _db.Grados
            .Include(g => g.GradoCursos)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (grado == null)
            return NotFound();

        var cursos = await _db.Cursos.ToListAsync();
        var cursosAsignados = grado.GradoCursos.Where(gc => gc.Activa).Select(gc => gc.CursoId).ToList();

        ViewBag.Cursos = cursos;
        ViewBag.CursosAsignados = cursosAsignados;

        return View(grado);
    }

    // POST: Guardar asignación de cursos
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuardarCursos(int id, int[] cursoIds)
    {
        var grado = await _db.Grados
            .Include(g => g.GradoCursos)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (grado == null)
            return NotFound();

        var cursosActuales = grado.GradoCursos.Where(gc => gc.Activa).Select(gc => gc.CursoId).ToList();
        var cursosNuevos = (cursoIds ?? Array.Empty<int>()).Except(cursosActuales).ToList();
        var cursosQuitar = cursosActuales.Except(cursoIds ?? Array.Empty<int>()).ToList();

        // Agregar nuevos cursos
        foreach (var cursoId in cursosNuevos)
        {
            var gradoCurso = new GradoCurso
            {
                GradoId = id,
                CursoId = cursoId,
                Activa = true,
                FechaAsignacion = DateTime.UtcNow
            };
            _db.GradoCursos.Add(gradoCurso);
        }

        // Desactivar cursos removidos
        foreach (var cursoId in cursosQuitar)
        {
            var gradoCurso = grado.GradoCursos.FirstOrDefault(gc => gc.CursoId == cursoId && gc.Activa);
            if (gradoCurso != null)
                gradoCurso.Activa = false;
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation($"Cursos asignados al grado {id}");

        TempData["Success"] = "Cursos asignados correctamente.";
        return RedirectToAction("Index");
    }

    // POST: Desactivar grado
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var grado = await _db.Grados.FindAsync(id);

        if (grado != null)
        {
            grado.Estado = "Inactivo";
            await _db.SaveChangesAsync();
            _logger.LogInformation($"Grado desactivado: {grado.Nombre}");
        }

        TempData["Success"] = "Grado desactivado correctamente.";
        return RedirectToAction("Index");
    }
}
