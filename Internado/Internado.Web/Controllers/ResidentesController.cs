using Internado.Application.Services;
using Internado.Infrastructure.Data;
using Internado.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Internado.Web.Controllers
{
    /// <summary>
    /// Controlador para gestión de Residentes del internado
    /// </summary>
    [Authorize]
    public class ResidentesController : Controller
    {
        private readonly IResidenteService _residenteService;
        private readonly InternadoDbContext _db;
        private readonly ILogger<ResidentesController> _logger;

        public ResidentesController(IResidenteService residenteService, InternadoDbContext db, ILogger<ResidentesController> logger)
        {
            _residenteService = residenteService ?? throw new ArgumentNullException(nameof(residenteService));
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: Residentes
        public async Task<IActionResult> Index(string searchTerm)
        {
            var residentes = string.IsNullOrWhiteSpace(searchTerm)
                ? await _residenteService.ObtenerTodosAsync()
                : await _residenteService.BuscarAsync(searchTerm);

            ViewBag.SearchTerm = searchTerm;
            ViewBag.Estadisticas = await _residenteService.ObtenerEstadisticasAsync();

            return View(residentes);
        }

        // GET: Residentes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var residente = await _residenteService.ObtenerPorIdAsync(id.Value);
            if (residente == null)
                return NotFound();

            return View(residente);
        }

        // GET: Residentes/Create
        public async Task<IActionResult> Create()
        {
            var grados = await _db.Grados.Where(g => g.Estado == "Activo").ToListAsync();
            var habitaciones = await _db.Habitaciones.Where(h => h.Estado == "Disponible").ToListAsync();

            ViewBag.Grados = grados;
            ViewBag.Habitaciones = habitaciones;

            return View(new Residente { FechaIngreso = DateOnly.FromDateTime(DateTime.Today) });
        }

        // POST: Residentes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection collection)
        {
            try
            {
                var nombreCompleto = collection["NombreCompleto"].ToString();
                var dpi = collection["DPI"].ToString();
                var tutor = collection["Tutor"].ToString();
                var gradoId = int.Parse(collection["GradoId"].ToString() ?? "0");
                var habitacionId = int.Parse(collection["HabitacionId"].ToString() ?? "0");
                var fechaNacimiento = DateOnly.Parse(collection["FechaNacimiento"].ToString());
                var fechaIngreso = DateOnly.Parse(collection["FechaIngreso"].ToString());

                if (string.IsNullOrWhiteSpace(nombreCompleto) || string.IsNullOrWhiteSpace(dpi))
                {
                    TempData["Error"] = "Nombre y DPI son requeridos.";
                    return RedirectToAction(nameof(Create));
                }

                // Validar formato DPI (13 dígitos, sin espacios)
                dpi = dpi.Trim().Replace(" ", "").Replace("-", "");
                if (dpi.Length != 13 || !dpi.All(char.IsDigit))
                {
                    TempData["Error"] = "El DPI debe contener exactamente 13 dígitos numéricos sin espacios.";
                    return RedirectToAction(nameof(Create));
                }

                // Validar DPI único
                var existeDpi = await _db.Residentes.AnyAsync(r => r.DPI == dpi);
                if (existeDpi)
                {
                    TempData["Error"] = "El DPI ya está registrado.";
                    return RedirectToAction(nameof(Create));
                }

                var residente = new Residente
                {
                    NombreCompleto = nombreCompleto,
                    DPI = dpi,
                    Tutor = tutor,
                    Estado = "Activa",
                    GradoId = gradoId > 0 ? gradoId : null,
                    HabitacionId = habitacionId > 0 ? habitacionId : null,
                    FechaNacimiento = fechaNacimiento,
                    FechaIngreso = fechaIngreso
                };

                _db.Residentes.Add(residente);
                await _db.SaveChangesAsync();

                // AUTOMÁTICO: Si tiene grado asignado, matricularlo en los cursos del grado
                if (gradoId > 0)
                {
                    var cursosDelGrado = await _db.GradoCursos
                        .Where(gc => gc.GradoId == gradoId && gc.Activa)
                        .Select(gc => gc.CursoId)
                        .ToListAsync();

                    foreach (var cursoId in cursosDelGrado)
                    {
                        var matricula = new Matricula
                        {
                            ResidenteId = residente.Id,
                            CursoId = cursoId,
                            Periodo = ObtenerPeriodoActual(),
                            FechaMatricula = DateTime.UtcNow,
                            Activa = true
                        };
                        _db.Matriculas.Add(matricula);
                    }

                    await _db.SaveChangesAsync();
                    _logger.LogInformation($"Residente {nombreCompleto} matriculado en {cursosDelGrado.Count} cursos");
                }

                TempData["Success"] = "Residente creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al crear residente: {ex.Message}";
                return RedirectToAction(nameof(Create));
            }
        }

        // GET: Residentes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var residente = await _residenteService.ObtenerPorIdAsync(id.Value);
            if (residente == null)
                return NotFound();

            var grados = await _db.Grados.Where(g => g.Estado == "Activo").ToListAsync();
            var habitaciones = await _db.Habitaciones.Where(h => h.Estado == "Disponible" || h.Id == residente.HabitacionId).ToListAsync();

            ViewBag.Grados = grados;
            ViewBag.Habitaciones = habitaciones;

            return View(residente);
        }

        // POST: Residentes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormCollection collection)
        {
            var residente = await _db.Residentes
                .Include(r => r.Matriculas)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (residente == null)
                return NotFound();

            try
            {
                var gradoIdNuevo = int.Parse(collection["GradoId"].ToString() ?? "0");
                var gradoIdAnterior = residente.GradoId;

                residente.NombreCompleto = collection["NombreCompleto"].ToString();
                residente.Tutor = collection["Tutor"].ToString();
                residente.HabitacionId = int.Parse(collection["HabitacionId"].ToString() ?? "0");
                residente.GradoId = gradoIdNuevo > 0 ? gradoIdNuevo : null;

                if (!string.IsNullOrEmpty(collection["FechaNacimiento"]))
                    residente.FechaNacimiento = DateOnly.Parse(collection["FechaNacimiento"].ToString());
                if (!string.IsNullOrEmpty(collection["FechaIngreso"]))
                    residente.FechaIngreso = DateOnly.Parse(collection["FechaIngreso"].ToString());

                // SI CAMBIÓ DE GRADO
                if (gradoIdNuevo > 0 && gradoIdAnterior != gradoIdNuevo)
                {
                    // Obtener cursos del nuevo grado
                    var cursosDelGradoNuevo = await _db.GradoCursos
                        .Where(gc => gc.GradoId == gradoIdNuevo && gc.Activa)
                        .Select(gc => gc.CursoId)
                        .ToListAsync();

                    var periodo = ObtenerPeriodoActual();

                    // Eliminar matrículas antiguas (excepto las explícitas)
                    var matriculasAnteriores = residente.Matriculas
                        .Where(m => m.Activa && m.Periodo == periodo)
                        .ToList();

                    foreach (var matricula in matriculasAnteriores)
                        matricula.Activa = false;

                    // Crear nuevas matrículas basadas en el nuevo grado
                    foreach (var cursoId in cursosDelGradoNuevo)
                    {
                        var yaExiste = await _db.Matriculas
                            .AnyAsync(m => m.ResidenteId == id && m.CursoId == cursoId && m.Periodo == periodo);

                        if (!yaExiste)
                        {
                            var matricula = new Matricula
                            {
                                ResidenteId = id,
                                CursoId = cursoId,
                                Periodo = periodo,
                                FechaMatricula = DateTime.UtcNow,
                                Activa = true
                            };
                            _db.Matriculas.Add(matricula);
                        }
                    }

                    _logger.LogInformation($"Residente {residente.NombreCompleto} movido a grado {gradoIdNuevo}");
                }

                await _db.SaveChangesAsync();
                TempData["Success"] = "Residente actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al actualizar: {ex.Message}";
                return RedirectToAction(nameof(Edit), new { id });
            }
        }

        // GET: Residentes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var residente = await _residenteService.ObtenerPorIdAsync(id.Value);
            if (residente == null)
                return NotFound();

            return View(residente);
        }

        // POST: Residentes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var resultado = await _residenteService.EliminarAsync(id);
            if (resultado)
            {
                TempData["SuccessMessage"] = "Residente eliminado exitosamente";
            }
            else
            {
                TempData["ErrorMessage"] = "Error al eliminar el residente";
            }

            return RedirectToAction(nameof(Index));
        }

        private string ObtenerPeriodoActual()
        {
            var ahora = DateTime.Now;
            var periodo = ahora.Month <= 6 ? "1" : "2";
            return $"{ahora.Year}-{periodo}";
        }
    }
}