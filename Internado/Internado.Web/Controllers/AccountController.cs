using System.Security.Claims;
using Internado.Application.Services;
using Internado.Infrastructure.Data;
using Internado.Infrastructure.Security;
using Internado.Web.Models.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Internado.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly InternadoDbContext _db;
        private readonly IPasswordHasher _hasher;
        private readonly IConfiguration _cfg;

        public AccountController(InternadoDbContext db, IPasswordHasher hasher, IConfiguration cfg)
        {
            _db = db;
            _hasher = hasher;
            _cfg = cfg;
        }

        // Helpers de reflexión: leer propiedad si existe
        private static T? GetProp<T>(object obj, string propName)
        {
            var p = obj.GetType().GetProperty(propName);
            if (p == null) return default;
            var val = p.GetValue(obj);
            return val is T t ? t : default;
        }

        private static string? GetUserNameFromEntity(object entity)
        {
            // Intenta varios nombres típicos generados por scaffold
            return GetProp<string>(entity, "Usuario")
                ?? GetProp<string>(entity, "Usuario1")
                ?? GetProp<string>(entity, "NombreUsuario")
                ?? GetProp<string>(entity, "Login")
                ?? GetProp<string>(entity, "Username");
        }

        private static string? GetHashBase64FromEntity(object entity)
        {
            // HashContrasena puede ser byte[] (varbinary) o string (nvarchar)
            var bytes = GetProp<byte[]>(entity, "HashContrasena");
            if (bytes != null) return Convert.ToBase64String(bytes);

            var str = GetProp<string>(entity, "HashContrasena");
            return str; // si ya viene como string/base64
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = await _db.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Usuario1 == model.UserName);

            var loginAttemptService = HttpContext.RequestServices.GetRequiredService<ILoginAttemptService>();
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            const int MAX_INTENTOS = 5;

            // VALIDAR: ¿Cuenta bloqueada?
            if (await loginAttemptService.EstaCuentaBloqueadaAsync(model.UserName))
            {
                ModelState.AddModelError("", "Cuenta bloqueada tras múltiples intentos fallidos. Intenta en 15 minutos.");
                await loginAttemptService.RegistrarIntentoFallidoAsync(model.UserName, clientIp);
                return View(model);
            }

            // VALIDAR: ¿Usuario existe?
            if (usuario == null)
            {
                await loginAttemptService.RegistrarIntentoFallidoAsync(model.UserName, clientIp);
                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
                return View(model);
            }

            // VALIDAR: ¿Usuario activo?
            if (!usuario.Estado)
            {
                await loginAttemptService.RegistrarIntentoFallidoAsync(model.UserName, clientIp);
                ModelState.AddModelError("", "Cuenta desactivada. Contacta al administrador.");
                return View(model);
            }

            // VALIDAR: ¿Contraseña correcta?
            var passwordHasher = HttpContext.RequestServices.GetRequiredService<IPasswordHasher>();
            var hashBase64 = Convert.ToBase64String(usuario.HashContrasena);
            if (!passwordHasher.VerifyFromBase64(model.Password, hashBase64))
            {
                await loginAttemptService.RegistrarIntentoFallidoAsync(model.UserName, clientIp);
                var intentosRestantes = MAX_INTENTOS - await loginAttemptService.ObtenerIntentosRecentesAsync(model.UserName);
                ModelState.AddModelError("", $"Contraseña incorrecta. Intentos restantes: {intentosRestantes}");
                return View(model);
            }

            // LOGIN EXITOSO
            await loginAttemptService.LimpiarIntentosAsync(model.UserName);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new(ClaimTypes.Name, usuario.Usuario1),
                new(ClaimTypes.Email, usuario.Correo),
                new(ClaimTypes.Role, usuario.Rol?.NombreRol ?? "Sin Rol")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = model.RememberMe });

            return RedirectToAction("Index", "Dashboard");
        }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Account");
    }
}
}


