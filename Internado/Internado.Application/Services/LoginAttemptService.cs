using Internado.Infrastructure.Data;
using Internado.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Internado.Application.Services;

public class LoginAttemptService : ILoginAttemptService
{
    private readonly InternadoDbContext _context;
    private const int MAX_INTENTOS = 5;
    private const int VENTANA_MINUTOS = 15;

    public LoginAttemptService(InternadoDbContext context)
    {
        _context = context;
    }

    public async Task RegistrarIntentoFallidoAsync(string usuario, string? ip = null)
    {
        var intento = new LoginAttempt
        {
            Usuario = usuario,
            DireccionIp = ip,
            TipoError = "CredencialesInvalidas",
            FechaIntento = DateTime.UtcNow
        };

        _context.LoginAttempts.Add(intento);
        await _context.SaveChangesAsync();
    }

    public async Task<int> ObtenerIntentosRecentesAsync(string usuario)
    {
        var hace15Minutos = DateTime.UtcNow.AddMinutes(-VENTANA_MINUTOS);
        return await _context.LoginAttempts
            .Where(x => x.Usuario == usuario && x.FechaIntento >= hace15Minutos)
            .CountAsync();
    }

    public async Task<bool> EstaCuentaBloqueadaAsync(string usuario)
    {
        var intentos = await ObtenerIntentosRecentesAsync(usuario);
        return intentos >= MAX_INTENTOS;
    }

    public async Task LimpiarIntentosAsync(string usuario)
    {
        var intentos = _context.LoginAttempts
            .Where(x => x.Usuario == usuario);
        _context.LoginAttempts.RemoveRange(intentos);
        await _context.SaveChangesAsync();
    }
}
