using System.Security.Claims;

namespace Internado.Web.Middleware;

public class RoleAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public RoleAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        var userRole = context.User?.FindFirst(ClaimTypes.Role)?.Value ?? "";

        // Rutas públicas o comunes que todos los usuarios autenticados pueden acceder
        var rutasComunes = new List<string>
        {
            "/account/logout",
            "/dashboard",
            "/home",
            "/",
            "/lib",      // archivos estáticos
            "/css",      // archivos estáticos
            "/js",       // archivos estáticos
            "/favicon"   // favicon
        };

        // Reglas de autorización por rol
        var permisosRol = new Dictionary<string, List<string>>
        {
            { "Docente", new List<string> { "/calificaciones", "/asistencia" } },
            { "Medico", new List<string> { "/medico" } },
            { "Direccion", new List<string> { "/residentes", "/habitaciones", "/reportes" } }
            // Administrador tiene acceso total (no necesita estar en el diccionario)
        };

        if (context.User?.Identity?.IsAuthenticated ?? false)
        {
            // Verificar si es una ruta común permitida para todos
            var esRutaComun = rutasComunes.Any(ruta => path.StartsWith(ruta));
            if (esRutaComun)
            {
                await _next(context);
                return;
            }

            // Administrador tiene acceso total a TODOS los módulos
            if (userRole == "Administrador")
            {
                await _next(context);
                return;
            }

            // Para otros roles, verificar permisos específicos
            var tieneAcceso = false;
            if (permisosRol.TryGetValue(userRole, out var rutas))
            {
                tieneAcceso = rutas.Any(ruta => path.StartsWith(ruta));
            }

            if (!tieneAcceso)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Acceso denegado. No tienes permiso para acceder a este módulo.");
                return;
            }
        }

        await _next(context);
    }
}
