using Internado.Infrastructure.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Internado.Application.Services
{
    public interface IHabitacionService
    {
        Task<IEnumerable<Habitacion>> ObtenerTodosAsync();
        Task<IEnumerable<Habitacion>> BuscarAsync(string searchTerm);
        Task<IEnumerable<Habitacion>> ObtenerDisponiblesAsync();
        Task<Habitacion?> ObtenerPorIdAsync(int id);
        Task<Habitacion?> ObtenerConResidentesAsync(int id);
        Task<bool> CrearAsync(Habitacion habitacion);
        Task<bool> ActualizarAsync(Habitacion habitacion);
        Task<bool> EliminarAsync(int id);
        Task<bool> ExisteNumeroAsync(string numero, int? excludeId = null);
        Task<Dictionary<string, int>> ObtenerEstadisticasAsync();
        Task<Dictionary<string, object>> ObtenerEstadisticasDetalladasAsync();
    }
}