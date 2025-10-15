using Internado.Application.Services;
using Internado.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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

        public ResidentesController(IResidenteService residenteService)
        {
            _residenteService = residenteService ?? throw new ArgumentNullException(nameof(residenteService));
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
        public IActionResult Create()
        {
            return View(new Residente { FechaIngreso = DateOnly.FromDateTime(DateTime.Today) });
        }

        // POST: Residentes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NombreCompleto,FechaNacimiento,DPI,Tutor,Estado,FechaIngreso,FechaEgreso")] Residente residente)
        {
            // Validación de negocio adicional
            if (residente.FechaEgreso.HasValue && residente.FechaEgreso < residente.FechaIngreso)
            {
                ModelState.AddModelError("FechaEgreso", "La fecha de egreso no puede ser anterior a la fecha de ingreso");
            }

            if (await _residenteService.ExisteDPIAsync(residente.DPI))
            {
                ModelState.AddModelError("DPI", "Ya existe un residente con este DPI");
            }

            if (ModelState.IsValid)
            {
                var resultado = await _residenteService.CrearAsync(residente);
                if (resultado)
                {
                    TempData["SuccessMessage"] = $"Residente '{residente.NombreCompleto}' creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al crear el residente. Verifica que el DPI no esté duplicado";
                }
            }

            return View(residente);
        }

        // GET: Residentes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var residente = await _residenteService.ObtenerPorIdAsync(id.Value);
            if (residente == null)
                return NotFound();

            return View(residente);
        }

        // POST: Residentes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombreCompleto,FechaNacimiento,DPI,Tutor,Estado,FechaIngreso,FechaEgreso")] Residente residente)
        {
            if (id != residente.Id)
                return NotFound();

            // Validaciones de negocio
            if (residente.FechaEgreso.HasValue && residente.FechaEgreso < residente.FechaIngreso)
            {
                ModelState.AddModelError("FechaEgreso", "La fecha de egreso no puede ser anterior a la fecha de ingreso");
            }

            if (await _residenteService.ExisteDPIAsync(residente.DPI, residente.Id))
            {
                ModelState.AddModelError("DPI", "Ya existe otro residente con este DPI");
            }

            if (ModelState.IsValid)
            {
                var resultado = await _residenteService.ActualizarAsync(residente);
                if (resultado)
                {
                    TempData["SuccessMessage"] = $"Residente '{residente.NombreCompleto}' actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al actualizar el residente";
                }
            }

            return View(residente);
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
    }
}