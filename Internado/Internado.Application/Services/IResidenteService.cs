using Internado.Infrastructure.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Internado.Application.Services
{
    /// <summary>
    /// Interfaz para operaciones de negocio relacionadas con Residentes
    /// </summary>
    public interface IResidenteService
    {
        Task<IEnumerable<Residente>> ObtenerTodosAsync();
        Task<IEnumerable<Residente>> BuscarAsync(string searchTerm);
        Task<Residente?> ObtenerPorIdAsync(int id);
        Task<bool> CrearAsync(Residente residente);
        Task<bool> ActualizarAsync(Residente residente);
        Task<bool> EliminarAsync(int id);
        Task<bool> ExisteDPIAsync(string dpi, int? excludeId = null);
        Task<Dictionary<string, int>> ObtenerEstadisticasAsync();
    }
}