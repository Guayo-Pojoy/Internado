using Internado.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Internado.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly InternadoDbContext _db;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(InternadoDbContext db, ILogger<DashboardController> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

        return userRole switch
        {
            "Administrador" => await DashboardAdmin(),
            "Docente" => await DashboardDocente(),
            "Medico" => await DashboardMedico(),
            "Direccion" => await DashboardDireccion(),
            _ => NotFound()
        };
    }

    // DASHBOARD ADMINISTRADOR
    private async Task<IActionResult> DashboardAdmin()
    {
        var totalUsuarios = await _db.Usuarios.CountAsync(u => u.Estado);
        var totalResidentes = await _db.Residentes.CountAsync(r => r.Estado == "Activo");
        var totalHabitaciones = await _db.Habitaciones.CountAsync();
        var totalCursos = await _db.Cursos.CountAsync();
        var totalDocentes = await _db.Usuarios.CountAsync(u => u.Rol.NombreRol == "Docente" && u.Estado);

        var habitacionesOcupadas = await _db.Habitaciones.CountAsync(h => h.Estado == "Ocupada");
        var habitacionesDisponibles = await _db.Habitaciones.CountAsync(h => h.Estado == "Disponible");
        var habitacionesMantenimiento = await _db.Habitaciones.CountAsync(h => h.Estado == "Mantenimiento");

        var medicamentosStock = await _db.Medicamentos
            .Where(m => m.StockActual < m.StockMinimo)
            .CountAsync();

        ViewBag.TotalUsuarios = totalUsuarios;
        ViewBag.TotalResidentes = totalResidentes;
        ViewBag.TotalHabitaciones = totalHabitaciones;
        ViewBag.TotalCursos = totalCursos;
        ViewBag.TotalDocentes = totalDocentes;
        ViewBag.HabitacionesOcupadas = habitacionesOcupadas;
        ViewBag.HabitacionesDisponibles = habitacionesDisponibles;
        ViewBag.HabitacionesMantenimiento = habitacionesMantenimiento;
        ViewBag.MedicamentosStockBajo = medicamentosStock;
        ViewBag.Rol = "Administrador";

        return View("DashboardAdmin");
    }

    // DASHBOARD DOCENTE
    private async Task<IActionResult> DashboardDocente()
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Cursos asignados al docente
        var cursos = await _db.Cursos
            .Where(c => c.AsignacionesDocentes.Any(ad => ad.DocenteId == usuarioId && ad.Activa))
            .Include(c => c.Calificaciones)
            .Include(c => c.Asistencia)
            .Include(c => c.Matriculas)
            .ToListAsync();

        var totalCursos = cursos.Count;
        var totalEstudiantes = cursos.SelectMany(c => c.Matriculas.Where(m => m.Activa)).Select(m => m.ResidenteId).Distinct().Count();

        // Métricas por curso
        var metricasCursos = cursos.Select(c => new
        {
            c.Id,
            c.Nombre,
            EstudiantesMatriculados = c.Matriculas.Count(m => m.Activa),
            CalificacionesRegistradas = c.Calificaciones.Count(),
            PromedioNotas = c.Calificaciones.Any() ? c.Calificaciones.Average(cal => cal.Nota) : 0m,
            RegistrosAsistencia = c.Asistencia.Count(),
            PorcentajePresencia = c.Asistencia.Any() ?
                Math.Round((double)c.Asistencia.Count(a => a.Estado == "Presente") / c.Asistencia.Count() * 100, 2) : 0
        }).ToList();

        // Asistencia promedio del último mes
        var haceTreintaDias = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
        var asistenciaReciente = await _db.Asistencia
            .Where(a => a.Fecha >= haceTreintaDias &&
                        a.Curso.AsignacionesDocentes.Any(ad => ad.DocenteId == usuarioId && ad.Activa))
            .GroupBy(a => a.Estado)
            .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
            .ToListAsync();

        ViewBag.TotalCursos = totalCursos;
        ViewBag.TotalEstudiantes = totalEstudiantes;
        ViewBag.MetricasCursos = metricasCursos;
        ViewBag.AsistenciaReciente = asistenciaReciente;
        ViewBag.Rol = "Docente";

        return View("DashboardDocente");
    }

    // DASHBOARD MÉDICO
    private async Task<IActionResult> DashboardMedico()
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Consultas del médico
        var totalConsultas = await _db.Consultas
            .CountAsync(c => c.MedicoId == usuarioId);

        var consultasEstasMes = await _db.Consultas
            .Where(c => c.MedicoId == usuarioId &&
                        c.Fecha.Year == DateTime.Now.Year &&
                        c.Fecha.Month == DateTime.Now.Month)
            .CountAsync();

        // Medicamentos
        var medicamentosTotal = await _db.Medicamentos.CountAsync();
        var medicamentosStockBajo = await _db.Medicamentos
            .Where(m => m.StockActual < m.StockMinimo)
            .CountAsync();

        var medicamentosProximos30Dias = await _db.Medicamentos
            .Where(m => m.StockActual <= m.StockMinimo + 10)
            .CountAsync();

        // Movimientos de medicamentos este mes
        var movimientosEsteMes = await _db.MovimientosMedicamentos
            .Where(m => m.Fecha.Year == DateTime.Now.Year &&
                        m.Fecha.Month == DateTime.Now.Month)
            .GroupBy(m => m.TipoMovimiento)
            .Select(g => new { Tipo = g.Key, Cantidad = g.Count() })
            .ToListAsync();

        // Top 5 medicamentos más dispensados
        var medicamentosMasUsados = await _db.MovimientosMedicamentos
            .Where(m => m.TipoMovimiento == "Salida" && m.Fecha >= DateTime.Today.AddDays(-30))
            .GroupBy(m => m.Medicamento.Nombre)
            .Select(g => new { Medicamento = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
            .OrderByDescending(x => x.Cantidad)
            .Take(5)
            .ToListAsync();

        // Diagnósticos más frecuentes este mes
        var diagnosticosFrequentes = await _db.Consultas
            .Where(c => c.Fecha >= DateTime.Today.AddDays(-30))
            .GroupBy(c => c.Diagnostico)
            .Select(g => new { Diagnostico = g.Key, Cantidad = g.Count() })
            .OrderByDescending(x => x.Cantidad)
            .Take(5)
            .ToListAsync();

        ViewBag.TotalConsultas = totalConsultas;
        ViewBag.ConsultasEsteMes = consultasEstasMes;
        ViewBag.MedicamentosTotal = medicamentosTotal;
        ViewBag.MedicamentosStockBajo = medicamentosStockBajo;
        ViewBag.MedicamentosProximos = medicamentosProximos30Dias;
        ViewBag.MovimientosEsteMes = movimientosEsteMes;
        ViewBag.MedicamentosMasUsados = medicamentosMasUsados;
        ViewBag.DiagnosticosFrequentes = diagnosticosFrequentes;
        ViewBag.Rol = "Medico";

        return View("DashboardMedico");
    }

    // DASHBOARD DIRECCIÓN
    private async Task<IActionResult> DashboardDireccion()
    {
        // Residentes
        var totalResidentes = await _db.Residentes.CountAsync(r => r.Estado == "Activo");
        var residentesPorGrado = await _db.Grados
            .Include(g => g.Residentes)
            .Where(g => g.Estado == "Activo")
            .Select(g => new { Grado = g.Nombre, Cantidad = g.Residentes.Count(r => r.Estado == "Activo") })
            .OrderBy(x => x.Grado)
            .ToListAsync();

        // Habitaciones
        var habitacionesOcupadas = await _db.Habitaciones.CountAsync(h => h.Estado == "Ocupada");
        var habitacionesDisponibles = await _db.Habitaciones.CountAsync(h => h.Estado == "Disponible");
        var habitacionesMantenimiento = await _db.Habitaciones.CountAsync(h => h.Estado == "Mantenimiento");
        var tasaOcupacion = await _db.Habitaciones.CountAsync();
        var porcentajeOcupacion = tasaOcupacion > 0 ? Math.Round((double)habitacionesOcupadas / tasaOcupacion * 100, 2) : 0;

        // Asistencia general (últimos 30 días)
        var haceTreintaDias = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
        var asistenciaGeneral = await _db.Asistencia
            .Where(a => a.Fecha >= haceTreintaDias)
            .GroupBy(a => a.Estado)
            .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
            .ToListAsync();

        // Residentes con baja asistencia (< 80% en último mes)
        var residentesBajaAsistencia = (await _db.Residentes
            .Where(r => r.Estado == "Activo")
            .Select(r => new
            {
                r.NombreCompleto,
                Grado = r.Grado.Nombre,
                TotalRegistros = r.Asistencia.Count(a => a.Fecha >= haceTreintaDias),
                Presentes = r.Asistencia.Count(a => a.Fecha >= haceTreintaDias && a.Estado == "Presente")
            })
            .ToListAsync())
            .Where(x => x.TotalRegistros > 0 && ((double)x.Presentes / x.TotalRegistros < 0.8))
            .OrderByDescending(x => x.TotalRegistros)
            .Take(10)
            .ToList();

        // Cursos
        var totalCursos = await _db.Cursos.CountAsync();

        ViewBag.TotalResidentes = totalResidentes;
        ViewBag.ResidentesPorGrado = residentesPorGrado;
        ViewBag.HabitacionesOcupadas = habitacionesOcupadas;
        ViewBag.HabitacionesDisponibles = habitacionesDisponibles;
        ViewBag.HabitacionesMantenimiento = habitacionesMantenimiento;
        ViewBag.PorcentajeOcupacion = porcentajeOcupacion;
        ViewBag.AsistenciaGeneral = asistenciaGeneral;
        ViewBag.ResidentesBajaAsistencia = residentesBajaAsistencia;
        ViewBag.TotalCursos = totalCursos;
        ViewBag.Rol = "Direccion";

        return View("DashboardDireccion");
    }
}
