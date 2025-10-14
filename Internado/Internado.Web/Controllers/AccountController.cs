using System.Security.Claims;
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

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Traemos usuarios a memoria (evitamos referenciar propiedades que no existen en EF)
            var users = await _db.Usuarios.Include("Rol").ToListAsync();
            var user = users.FirstOrDefault(u =>
            {
                var uname = GetUserNameFromEntity(u) ?? "";
                var mail  = GetProp<string>(u, "Correo") ?? "";
                return string.Equals(uname, vm.UserName, StringComparison.OrdinalIgnoreCase)
                       || string.Equals(mail, vm.UserName, StringComparison.OrdinalIgnoreCase);
            });

            if (user == null)
            {
                ModelState.AddModelError("", "Usuario no encontrado.");
                return View(vm);
            }

            // Estado (si existe)
            var activo = GetProp<bool?>(user, "Estado");
            if (activo.HasValue && !activo.Value)
            {
                ModelState.AddModelError("", "Usuario inactivo.");
                return View(vm);
            }

            // Verificación de contraseña (funciona con byte[] o string)
            var hashBase64 = GetHashBase64FromEntity(user);
            if (string.IsNullOrWhiteSpace(hashBase64) || !_hasher.VerifyFromBase64(vm.Password, hashBase64))
            {
                ModelState.AddModelError("", "Credenciales inválidas.");
                return View(vm);
            }

            // Datos para Claims
            var id       = GetProp<int?>(user, "Id")?.ToString() ?? "";
            var nombre   = GetProp<string>(user, "Nombre") ?? (GetUserNameFromEntity(user) ?? "");
            var correo   = GetProp<string>(user, "Correo") ?? "";
            var rolNombre= GetProp<object>(user, "Rol") is {} rolObj
                           ? (GetProp<string>(rolObj, "NombreRol") ?? "Usuario")
                           : "Usuario";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, id),
                new Claim(ClaimTypes.Name, nombre),
                new Claim(ClaimTypes.Email, correo),
                new Claim(ClaimTypes.Role, rolNombre)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return !string.IsNullOrEmpty(vm.ReturnUrl) ? Redirect(vm.ReturnUrl) : RedirectToAction("Index", "Home");
        }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Account");
    }
}
}


