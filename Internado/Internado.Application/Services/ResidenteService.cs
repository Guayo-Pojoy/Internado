using Internado.Infrastructure.Models;
using Internado.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Internado.Application.Services
{
    /// <summary>
    /// Servicio de lógica de negocio para gestión de Residentes
    /// </summary>
    public class ResidenteService : IResidenteService
    {
        private readonly InternadoDbContext _context;

        public ResidenteService(InternadoDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Residente>> ObtenerTodosAsync()
        {
            return await _context.Residentes
                .OrderBy(r => r.NombreCompleto)
                .ToListAsync();
        }

        public async Task<IEnumerable<Residente>> BuscarAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await ObtenerTodosAsync();

            searchTerm = searchTerm.Trim().ToLower();

            return await _context.Residentes
                .Where(r => r.NombreCompleto.ToLower().Contains(searchTerm) ||
                           r.DPI.Contains(searchTerm))
                .OrderBy(r => r.NombreCompleto)
                .ToListAsync();
        }

        public async Task<Residente?> ObtenerPorIdAsync(int id)
        {
            return await _context.Residentes.FindAsync(id);
        }

        public async Task<bool> CrearAsync(Residente residente)
        {
            try
            {
                if (await ExisteDPIAsync(residente.DPI))
                    return false;

                if (residente.FechaEgreso.HasValue && residente.FechaEgreso < residente.FechaIngreso)
                    return false;

                _context.Residentes.Add(residente);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ActualizarAsync(Residente residente)
        {
            try
            {
                if (await ExisteDPIAsync(residente.DPI, residente.Id))
                    return false;

                if (residente.FechaEgreso.HasValue && residente.FechaEgreso < residente.FechaIngreso)
                    return false;

                _context.Entry(residente).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> EliminarAsync(int id)
        {
            try
            {
                var residente = await ObtenerPorIdAsync(id);
                if (residente == null)
                    return false;

                _context.Residentes.Remove(residente);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ExisteDPIAsync(string dpi, int? excludeId = null)
        {
            if (excludeId.HasValue)
            {
                return await _context.Residentes
                    .AnyAsync(r => r.DPI == dpi && r.Id != excludeId.Value);
            }

            return await _context.Residentes.AnyAsync(r => r.DPI == dpi);
        }

        public async Task<Dictionary<string, int>> ObtenerEstadisticasAsync()
        {
            var stats = new Dictionary<string, int>
            {
                ["Total"] = await _context.Residentes.CountAsync(),
                ["Activas"] = await _context.Residentes.CountAsync(r => r.Estado == "Activa"),
                ["Egresadas"] = await _context.Residentes.CountAsync(r => r.Estado == "Egresada"),
                ["Suspendidas"] = await _context.Residentes.CountAsync(r => r.Estado == "Suspendida")
            };

            return stats;
        }
    }
}