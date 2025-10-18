using Internado.Infrastructure.Data;
using Internado.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Internado.Web.Controllers;

[Authorize(Policy = "Medico")]
public class MedicoController : Controller
{
    private readonly InternadoDbContext _db;
    private readonly ILogger<MedicoController> _logger;

    public MedicoController(InternadoDbContext db, ILogger<MedicoController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ===== CONSULTAS =====

    // GET: Listar consultas
    public async Task<IActionResult> Consultas()
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var consultas = await _db.Consultas
            .Where(c => c.MedicoId == usuarioId)
            .Include(c => c.Residente)
            .OrderByDescending(c => c.Fecha)
            .ToListAsync();

        return View(consultas);
    }

    // GET: Nueva consulta
    public async Task<IActionResult> NuevaConsulta()
    {
        var residentes = await _db.Residentes.Where(r => r.Estado == "Activa").ToListAsync();
        ViewBag.Residentes = residentes;
        return View();
    }

    // POST: Guardar consulta
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuardarConsulta(int residenteId, string diagnostico,
        string? tratamiento, bool generarPaseSalida = false, string? motivoPase = null,
        DateTime? fechaSalida = null, DateTime? fechaRetorno = null)
    {
        if (string.IsNullOrWhiteSpace(diagnostico))
        {
            TempData["Error"] = "El diagnóstico es requerido.";
            return RedirectToAction("NuevaConsulta");
        }

        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Agregar información de pase de salida al tratamiento si aplica
        var tratamientoFinal = tratamiento ?? "";
        if (generarPaseSalida)
        {
            var paseSalida = $"\n[PASE DE SALIDA AUTORIZADO]\nMotivo: {motivoPase}\n" +
                           $"Fecha Salida: {fechaSalida?.ToString("dd/MM/yyyy HH:mm")}\n" +
                           $"Fecha Retorno: {fechaRetorno?.ToString("dd/MM/yyyy HH:mm")}";
            tratamientoFinal += paseSalida;
        }

        var consulta = new Consulta
        {
            ResidenteId = residenteId,
            MedicoId = usuarioId,
            Fecha = DateTime.UtcNow,
            Diagnostico = diagnostico,
            Tratamiento = tratamientoFinal
        };

        _db.Consultas.Add(consulta);
        await _db.SaveChangesAsync();
        _logger.LogInformation($"Consulta registrada para residente {residenteId}" +
            (generarPaseSalida ? " con pase de salida" : ""));

        TempData["Success"] = "Consulta registrada correctamente" +
            (generarPaseSalida ? " con pase de salida autorizado." : ".");
        return RedirectToAction("Consultas");
    }

    // GET: Ver historial médico de residente
    public async Task<IActionResult> HistorialResidente(int residenteId)
    {
        var residente = await _db.Residentes
            .Include(r => r.Consulta)
                .ThenInclude(c => c.Medico)
            .Include(r => r.HistorialMedicos)
            .FirstOrDefaultAsync(r => r.Id == residenteId);

        if (residente == null)
            return NotFound();

        return View(residente);
    }

    // GET: Imprimir pase de salida
    public async Task<IActionResult> ImprimirPaseSalida(long consultaId)
    {
        var consulta = await _db.Consultas
            .Include(c => c.Residente)
            .Include(c => c.Medico)
            .FirstOrDefaultAsync(c => c.Id == consultaId);

        if (consulta == null || !consulta.Tratamiento?.Contains("[PASE DE SALIDA AUTORIZADO]") == true)
            return NotFound("Pase de salida no encontrado");

        ViewBag.Consulta = consulta;
        return View();
    }

    // ===== MEDICAMENTOS =====

    // GET: Listar medicamentos
    public async Task<IActionResult> Medicamentos()
    {
        var medicamentos = await _db.Medicamentos.ToListAsync();
        return View(medicamentos);
    }

    // GET: Nuevo medicamento
    [Authorize(Policy = "AdminOnly")]
    public IActionResult NuevoMedicamento()
    {
        return View();
    }

    // POST: Crear medicamento
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CrearMedicamento(string nombre, string lote,
        DateOnly fechaVencimiento, int cantidad)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            TempData["Error"] = "El nombre del medicamento es requerido.";
            return RedirectToAction("Medicamentos");
        }

        // Validar que la cantidad no sea negativa
        if (cantidad < 0)
        {
            TempData["Error"] = "La cantidad no puede ser negativa.";
            return RedirectToAction("NuevoMedicamento");
        }

        // Validar que la fecha de vencimiento no sea pasada
        if (fechaVencimiento < DateOnly.FromDateTime(DateTime.Today))
        {
            TempData["Error"] = "La fecha de vencimiento no puede ser una fecha pasada.";
            return RedirectToAction("NuevoMedicamento");
        }

        var medicamento = new Medicamento
        {
            Nombre = nombre,
            Lote = lote,
            FechaVencimiento = fechaVencimiento,
            StockActual = cantidad,
            StockMinimo = 10
        };

        _db.Medicamentos.Add(medicamento);
        await _db.SaveChangesAsync();
        _logger.LogInformation($"Medicamento creado: {nombre}");

        TempData["Success"] = "Medicamento registrado correctamente.";
        return RedirectToAction("Medicamentos");
    }

    // POST: Dispensar medicamento
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DispensarMedicamento(int medicamentoId, int cantidad)
    {
        // Validar que la cantidad sea positiva
        if (cantidad <= 0)
        {
            TempData["Error"] = "La cantidad debe ser mayor a 0.";
            return RedirectToAction("Medicamentos");
        }

        var medicamento = await _db.Medicamentos.FindAsync(medicamentoId);

        if (medicamento == null || medicamento.StockActual < cantidad)
        {
            TempData["Error"] = "Stock insuficiente.";
            return RedirectToAction("Medicamentos");
        }

        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        medicamento.StockActual -= cantidad;

        var movimiento = new MovimientosMedicamento
        {
            MedicamentoId = medicamentoId,
            TipoMovimiento = "Salida",
            Cantidad = cantidad,
            UsuarioId = usuarioId,
            Fecha = DateTime.UtcNow
        };

        _db.MovimientosMedicamentos.Add(movimiento);
        await _db.SaveChangesAsync();
        _logger.LogInformation($"Medicamento dispensado: {medicamento.Nombre} x{cantidad}");

        TempData["Success"] = "Medicamento dispensado correctamente.";
        return RedirectToAction("Medicamentos");
    }
}
