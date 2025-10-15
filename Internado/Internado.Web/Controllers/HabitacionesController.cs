using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Internado.Infrastructure.Data;
using Internado.Infrastructure.Models;

namespace Internado.Web.Controllers
{
    [Authorize]
    public class HabitacionesController : Controller
    {
        private readonly InternadoDbContext _context;

        public HabitacionesController(InternadoDbContext context)
        {
            _context = context;
        }

        // GET: Habitaciones
        public async Task<IActionResult> Index()
        {
            var habitaciones = await _context.Habitaciones
                .AsNoTracking()
                .OrderBy(h => h.Numero)
                .ToListAsync();

            return View(habitaciones);
        }

        // GET: Habitaciones/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var habitacion = await _context.Habitaciones
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (habitacion == null) return NotFound();

            return View(habitacion);
        }

        // GET: Habitaciones/Create
        public IActionResult Create()
        {
            ViewBag.Tipos = TiposSelectList();
            ViewBag.Estados = EstadosSelectList();
            return View(new Habitacion { Capacidad = 2 });
        }

        // POST: Habitaciones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Numero,Capacidad,Tipo,Estado,Piso,Edificio,Observaciones")] Habitacion habitacion)
        {
            if (ModelState.IsValid)
            {
                var existe = await _context.Habitaciones.AnyAsync(h => h.Numero == habitacion.Numero);

                if (existe)
                {
                    ModelState.AddModelError("Numero", "Ya existe una habitacion con este numero");
                    ViewBag.Tipos = TiposSelectList(habitacion.Tipo);
                    ViewBag.Estados = EstadosSelectList(habitacion.Estado);
                    return View(habitacion);
                }

                _context.Add(habitacion);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Habitacion creada exitosamente";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Tipos = TiposSelectList(habitacion.Tipo);
            ViewBag.Estados = EstadosSelectList(habitacion.Estado);
            return View(habitacion);
        }

        // GET: Habitaciones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var habitacion = await _context.Habitaciones.FindAsync(id);
            if (habitacion == null) return NotFound();

            ViewBag.Tipos = TiposSelectList(habitacion.Tipo);
            ViewBag.Estados = EstadosSelectList(habitacion.Estado);
            return View(habitacion);
        }

        // POST: Habitaciones/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Numero,Capacidad,Tipo,Estado,Piso,Edificio,Observaciones")] Habitacion habitacion)
        {
            if (id != habitacion.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existe = await _context.Habitaciones.AnyAsync(h => h.Numero == habitacion.Numero && h.Id != habitacion.Id);

                    if (existe)
                    {
                        ModelState.AddModelError("Numero", "Ya existe otra habitacion con este numero");
                        ViewBag.Tipos = TiposSelectList(habitacion.Tipo);
                        ViewBag.Estados = EstadosSelectList(habitacion.Estado);
                        return View(habitacion);
                    }

                    _context.Update(habitacion);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Habitacion actualizada exitosamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HabitacionExists(habitacion.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Tipos = TiposSelectList(habitacion.Tipo);
            ViewBag.Estados = EstadosSelectList(habitacion.Estado);
            return View(habitacion);
        }

        // GET: Habitaciones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var habitacion = await _context.Habitaciones
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (habitacion == null) return NotFound();

            return View(habitacion);
        }

        // POST: Habitaciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var habitacion = await _context.Habitaciones.FindAsync(id);
            if (habitacion != null)
            {
                _context.Habitaciones.Remove(habitacion);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Habitacion eliminada exitosamente";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool HabitacionExists(int id) =>
            _context.Habitaciones.Any(e => e.Id == id);

        private SelectList TiposSelectList(string? selected = null)
        {
            var items = new[] { "Individual", "Doble", "Compartida", "Suite" }
                .Select(x => new SelectListItem(x, x));
            return new SelectList(items, "Value", "Text", selected);
        }

        private SelectList EstadosSelectList(string? selected = null)
        {
            var items = new[] { "Disponible", "Ocupada", "Mantenimiento", "Reservada" }
                .Select(x => new SelectListItem(x, x));
            return new SelectList(items, "Value", "Text", selected);
        }
    }
}
