using Internado.Infrastructure.Models;

namespace Internado.Application.Services;

public interface ILoginAttemptService
{
    Task RegistrarIntentoFallidoAsync(string usuario, string? ip = null);
    Task<int> ObtenerIntentosRecentesAsync(string usuario);
    Task<bool> EstaCuentaBloqueadaAsync(string usuario);
    Task LimpiarIntentosAsync(string usuario);
}
