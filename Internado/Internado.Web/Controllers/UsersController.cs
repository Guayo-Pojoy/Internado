using System.Reflection;
using Internado.Infrastructure.Data;
using Internado.Infrastructure.Security;
using Internado.Web.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Internado.Web.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class UsersController : Controller
    {
        private readonly InternadoDbContext _db;
        private readonly IPasswordHasher _hasher;

        public UsersController(InternadoDbContext db, IPasswordHasher hasher)
        {
            _db = db; _hasher = hasher;
        }

        // ====== Helpers de reflexión (para tolerar nombres distintos del scaffold) ======
        private static PropertyInfo? P(object o, string name) => o.GetType().GetProperty(name);
        private static T? Get<T>(object o, string name)
        {
            var p = P(o, name); if (p == null) return default;
            var v = p.GetValue(o); return v is T t ? t : default;
        }
        private static void Set(object o, string name, object? value)
        {
            var p = P(o, name); if (p != null) p.SetValue(o, value);
        }

        private static string? GetUserName(object u) =>
            Get<string>(u, "Usuario") ?? Get<string>(u, "Usuario1") ?? Get<string>(u, "NombreUsuario") ?? Get<string>(u, "Login");
        private static string? GetEmail(object u) =>
            Get<string>(u, "Correo") ?? Get<string>(u, "Email");
        private static int GetId(object u) => Get<int?>(u, "Id") ?? 0;
        private static string? GetHashBase64(object u)
        {
            var b = Get<byte[]>(u, "HashContrasena");
            if (b != null) return Convert.ToBase64String(b);
            return Get<string>(u, "HashContrasena");
        }
        private static void SetHashFromBase64(object u, string base64)
        {
            var p = P(u, "HashContrasena");
            if (p == null) return;
            if (p.PropertyType == typeof(byte[]))
                p.SetValue(u, Convert.FromBase64String(base64));
            else
                p.SetValue(u, base64);
        }

        // DTO para roles (evita el problema de las tuplas Item1/Item2)
        private sealed class RoleOption
        {
            public int Id { get; }
            public string Texto { get; }
            public RoleOption(int id, string texto) { Id = id; Texto = texto; }
        }

        private async Task<List<RoleOption>> GetRolesAsync()
        {
            var roles = await _db.Roles.AsNoTracking().ToListAsync();
            var list = new List<RoleOption>();
            foreach (var r in roles)
            {
                var id   = Get<int?>(r, "Id") ?? 0;
                var name = Get<string>(r, "NombreRol") ?? Get<string>(r, "Nombre") ?? "Rol";
                list.Add(new RoleOption(id, name));
            }
            return list.OrderBy(x => x.Texto).ToList();
        }

        private void FillViewBagsForForm(UserFormViewModel vm, List<RoleOption> roles)
        {
            ViewBag.Roles = roles.Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem {
                Value = x.Id.ToString(),
                Text  = x.Texto,
                Selected = x.Id == vm.RolId
            }).ToList();
        }

        // ================== Index ==================
        public async Task<IActionResult> Index()
        {
            var qry = _db.Usuarios.AsQueryable();
            try { qry = qry.Include("Rol"); } catch { }
            var list = await qry.AsNoTracking()
                                .OrderBy(u => GetUserName(u) ?? Get<string>(u,"Nombre"))
                                .ToListAsync();
            return View(list);
        }

        // ================== Create ==================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new UserFormViewModel();
            FillViewBagsForForm(vm, await GetRolesAsync());
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserFormViewModel vm)
        {
            FillViewBagsForForm(vm, await GetRolesAsync());
            if (!ModelState.IsValid) return View(vm);

            var users = await _db.Usuarios.AsNoTracking().ToListAsync();
            var dup = users.Any(u =>
                string.Equals(GetUserName(u) ?? "", vm.Usuario, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(GetEmail(u) ?? "",    vm.Correo,  StringComparison.OrdinalIgnoreCase));
            if (dup)
            {
                ModelState.AddModelError("", "Usuario o correo ya existe.");
                return View(vm);
            }

            // Crear instancia del entity del DbSet Usuarios
            var entityType = _db.GetType()
                                .GetProperty("Usuarios")!
                                .PropertyType
                                .GenericTypeArguments[0];
            var entity = Activator.CreateInstance(entityType)!;

            Set(entity, "Nombre", vm.Nombre);
            Set(entity, "Estado", vm.Estado);
            Set(entity, "Correo", vm.Correo);

            if      (P(entity,"Usuario")  != null) Set(entity, "Usuario",  vm.Usuario);
            else if (P(entity,"Usuario1") != null) Set(entity, "Usuario1", vm.Usuario);
            else if (P(entity,"Login")    != null) Set(entity, "Login",    vm.Usuario);

            if (P(entity,"RolId") != null) Set(entity, "RolId", vm.RolId);
            if (P(entity,"FechaRegistro") != null) Set(entity, "FechaRegistro", DateTime.Now);

            if (!string.IsNullOrWhiteSpace(vm.Password))
            {
                var hash64 = _hasher.HashToBase64(vm.Password);
                SetHashFromBase64(entity, hash64);
            }

            _db.Add(entity);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ================== Edit ==================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var u = await _db.Usuarios.FindAsync(id);
            if (u == null) return NotFound();

            var vm = new UserFormViewModel {
                Id      = id,
                Nombre  = Get<string>(u,"Nombre") ?? "",
                Usuario = GetUserName(u) ?? "",
                Correo  = GetEmail(u) ?? "",
                Estado  = Get<bool?>(u,"Estado") ?? true,
                RolId   = Get<int?>(u,"RolId") ?? 0
            };
            FillViewBagsForForm(vm, await GetRolesAsync());
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserFormViewModel vm)
        {
            FillViewBagsForForm(vm, await GetRolesAsync());
            if (!ModelState.IsValid) return View(vm);

            var u = await _db.Usuarios.FindAsync(vm.Id);
            if (u == null) return NotFound();

            var others = await _db.Usuarios.AsNoTracking().Where(x => GetId(x) != vm.Id).ToListAsync();
            var dup = others.Any(x =>
                string.Equals(GetUserName(x) ?? "", vm.Usuario, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(GetEmail(x) ?? "",    vm.Correo,  StringComparison.OrdinalIgnoreCase));
            if (dup)
            {
                ModelState.AddModelError("", "Usuario o correo ya existe.");
                return View(vm);
            }

            Set(u, "Nombre", vm.Nombre);
            Set(u, "Estado", vm.Estado);
            Set(u, "Correo", vm.Correo);

            if      (P(u,"Usuario")  != null) Set(u, "Usuario",  vm.Usuario);
            else if (P(u,"Usuario1") != null) Set(u, "Usuario1", vm.Usuario);
            else if (P(u,"Login")    != null) Set(u, "Login",    vm.Usuario);

            if (P(u,"RolId") != null) Set(u, "RolId", vm.RolId);

            if (!string.IsNullOrWhiteSpace(vm.Password))
            {
                var hash64 = _hasher.HashToBase64(vm.Password);
                SetHashFromBase64(u, hash64);
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ================== Delete ==================
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var u = await _db.Usuarios.FindAsync(id);
            if (u != null)
            {
                _db.Remove(u);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
