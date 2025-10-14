using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Internado.Infrastructure.Data;
using Internado.Infrastructure.Models;

namespace Internado.Web.Controllers
{
    public class ResidentesController : Controller
    {
        private readonly InternadoDbContext _context;

        public ResidentesController(InternadoDbContext context)
        {
            _context = context;
        }

        // GET: Residentes
        public async Task<IActionResult> Index()
        {
            var data = await _context.Residentes
                .AsNoTracking()
                .OrderBy(r => r.Id)
                .ToListAsync();

            return View(data);
        }

        // GET: Residentes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var residente = await _context.Residentes
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (residente == null) return NotFound();

            return View(residente);
        }

        // GET: Residentes/Create
        public IActionResult Create()
        {
            ViewBag.Estados = EstadosSelectList();
            return View(new Residente());
        }

        // POST: Residentes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NombreCompleto,DPI,Tutor,FechaNacimiento,FechaIngreso,FechaEgreso,Estado")] Residente residente)
        {
            if (ModelState.IsValid)
            {
                _context.Add(residente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Estados = EstadosSelectList(residente.Estado);
            return View(residente);
        }

        // GET: Residentes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var residente = await _context.Residentes.FindAsync(id);
            if (residente == null) return NotFound();

            ViewBag.Estados = EstadosSelectList(residente.Estado);
            return View(residente);
        }

        // POST: Residentes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombreCompleto,DPI,Tutor,FechaNacimiento,FechaIngreso,FechaEgreso,Estado")] Residente residente)
        {
            if (id != residente.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(residente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ResidenteExists(residente.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Estados = EstadosSelectList(residente.Estado);
            return View(residente);
        }

        // GET: Residentes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var residente = await _context.Residentes
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (residente == null) return NotFound();

            return View(residente);
        }

        // POST: Residentes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var residente = await _context.Residentes.FindAsync(id);
            if (residente != null)
            {
                _context.Residentes.Remove(residente);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ResidenteExists(int id) =>
            _context.Residentes.Any(e => e.Id == id);

        private SelectList EstadosSelectList(string? selected = null)
        {
            var items = new[] { "Activa", "Egresada", "Suspendida" }
                .Select(x => new SelectListItem(x, x));
            return new SelectList(items, "Value", "Text", selected);
        }
    }
}
