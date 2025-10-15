using Internado.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Internado.Web.Controllers
{
    /// <summary>
    /// Controlador para el Dashboard principal con estadísticas y gráficos
    /// </summary>
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IResidenteService _residenteService;

        public DashboardController(IResidenteService residenteService)
        {
            _residenteService = residenteService ?? throw new ArgumentNullException(nameof(residenteService));
        }

        // GET: Dashboard
        public async Task<IActionResult> Index()
        {
            try
            {
                // Obtener todas las estadísticas
                var estadisticas = await _residenteService.ObtenerEstadisticasAsync();
                var residentes = (await _residenteService.ObtenerTodosAsync()).ToList();

                // Estadísticas por mes de ingreso (últimos 6 meses)
                var ingresosPorMes = residentes
                    .Where(r => r.FechaIngreso >= DateOnly.FromDateTime(DateTime.Now.AddMonths(-6)))
                    .GroupBy(r => new { r.FechaIngreso.Year, r.FechaIngreso.Month })
                    .Select(g => new
                    {
                        Mes = $"{g.Key.Month:00}/{g.Key.Year}",
                        Cantidad = g.Count(),
                        Order = g.Key.Year * 100 + g.Key.Month
                    })
                    .OrderBy(x => x.Order)
                    .ToList();

                // Si no hay datos, crear datos vacíos para los últimos 6 meses
                if (!ingresosPorMes.Any())
                {
                    var now = DateTime.Now;
                    for (int i = 5; i >= 0; i--)
                    {
                        var fecha = now.AddMonths(-i);
                        ingresosPorMes.Add(new
                        {
                            Mes = $"{fecha.Month:00}/{fecha.Year}",
                            Cantidad = 0,
                            Order = fecha.Year * 100 + fecha.Month
                        });
                    }
                }

                // Estadísticas de edad
                var hoy = DateOnly.FromDateTime(DateTime.Today);
                var edades = residentes
                    .Select(r => hoy.Year - r.FechaNacimiento.Year - 
                        (hoy.DayOfYear < r.FechaNacimiento.DayOfYear ? 1 : 0))
                    .ToList();

                var rangoEdades = new
                {
                    Menores15 = edades.Count(e => e < 15),
                    Entre15y17 = edades.Count(e => e >= 15 && e <= 17),
                    Mayores17 = edades.Count(e => e > 17)
                };

                // Pasar datos a la vista
                ViewBag.Estadisticas = estadisticas;
                ViewBag.IngresosPorMes = ingresosPorMes;
                ViewBag.RangoEdades = rangoEdades;
                ViewBag.TotalResidentes = residentes.Count;
                ViewBag.PromedioEdad = edades.Any() ? Math.Round(edades.Average(), 1) : 0;

                // Residentes recientes
                var residentesRecientes = residentes
                    .OrderByDescending(r => r.FechaIngreso)
                    .Take(5)
                    .ToList();

                ViewBag.ResidentesRecientes = residentesRecientes;

                return View();
            }
            catch (Exception ex)
            {
                // Log del error
                TempData["ErrorMessage"] = "Error al cargar el dashboard: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // API endpoint para obtener datos en JSON (para gráficos dinámicos)
        [HttpGet]
        public async Task<IActionResult> GetEstadisticas()
        {
            var estadisticas = await _residenteService.ObtenerEstadisticasAsync();
            return Json(estadisticas);
        }
    }
}