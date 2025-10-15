using Internado.Application.Services;
using Internado.Infrastructure.Data;
using Internado.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

// EF Core (DbContext hacia InternadoDB)
builder.Services.AddDbContext<InternadoDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// DI: Password Hasher (BCrypt)
builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

// DI: Servicios de aplicación
builder.Services.AddScoped<IResidenteService, ResidenteService>();

// Autenticación por cookies (seguro para app interna)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // En prod con HTTPS: Always
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(
            builder.Configuration.GetValue<int>("Security:CookieExpireMinutes", 30));
    });

// Autorización por roles (políticas)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Administrador"));
    options.AddPolicy("Docente",   p => p.RequireRole("Docente","Administrador"));
    options.AddPolicy("Medico",    p => p.RequireRole("Medico","Administrador"));
    options.AddPolicy("Direccion", p => p.RequireRole("Direccion","Administrador"));
});

// MVC + Swagger
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<Internado.Infrastructure.Data.InternadoDbContext>();
    var hasher = services.GetRequiredService<Internado.Infrastructure.Security.IPasswordHasher>();

    // Helper de reflexión seguro
    static string? GetStringProp(object obj, string name)
    {
        var p = obj.GetType().GetProperty(name);
        if (p == null) return null;
        var val = p.GetValue(obj);
        return val?.ToString();
    }

    var userNames   = new[] { "Usuario", "Usuario1", "Login", "NombreUsuario", "UserName" };
    var roleNames   = new[] { "NombreRol", "Nombre", "RoleName" };

    // Traer a memoria (evitamos EF.Property en SQL)
    var usuarios = db.Usuarios.AsNoTracking().ToList();
    var adminExists = usuarios.Any(u => userNames.Any(n => string.Equals(GetStringProp(u, n), "admin", StringComparison.OrdinalIgnoreCase)));

    var roles = db.Roles.AsNoTracking().ToList();
    var rolAdmin = roles.FirstOrDefault(r => roleNames.Any(n => string.Equals(GetStringProp(r, n), "Administrador", StringComparison.OrdinalIgnoreCase)));

    if (!adminExists)
    {
        // crear instancia dinámica del tipo Usuarios
        var entityType = db.GetType().GetProperty("Usuarios")!
            .PropertyType.GenericTypeArguments[0];
        var admin = Activator.CreateInstance(entityType)!;

        entityType.GetProperty("Nombre")?.SetValue(admin, "Administrador del sistema");
        entityType.GetProperty("Correo")?.SetValue(admin, "admin@internado.local");
        // Estado true si existe (bool/bit)
        var pEstado = entityType.GetProperty("Estado");
        if (pEstado != null && (pEstado.PropertyType == typeof(bool) || Nullable.GetUnderlyingType(pEstado.PropertyType) == typeof(bool)))
            pEstado.SetValue(admin, true);

        // setear user en la primera propiedad que exista
        foreach (var n in userNames)
        {
            var p = entityType.GetProperty(n);
            if (p != null) { p.SetValue(admin, "admin"); break; }
        }

        // rol si hay FK RolId
        var pRolId = entityType.GetProperty("RolId");
        if (pRolId != null && rolAdmin != null)
        {
            var pId = rolAdmin.GetType().GetProperty("Id");
            if (pId != null)
            {
                var valId = pId.GetValue(rolAdmin);
                pRolId.SetValue(admin, valId);
            }
        }

        // contraseña
        var hash = hasher.HashToBase64("Admin123");
        var pHash = entityType.GetProperty("HashContrasena");
        if (pHash != null)
        {
            if (pHash.PropertyType == typeof(byte[]))
                pHash.SetValue(admin, Convert.FromBase64String(hash));
            else
                pHash.SetValue(admin, hash);
        }

        db.Add(admin);
        db.SaveChanges();
        Console.WriteLine("✅ Usuario admin creado (Usuario: admin / Contraseña: Admin123)");
    }
}

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Ruta por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();