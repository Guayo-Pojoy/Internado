using System;

namespace Internado.Infrastructure.Models;

public partial class LoginAttempt
{
    public int Id { get; set; }
    public string Usuario { get; set; } = null!;
    public string? DireccionIp { get; set; }
    public string TipoError { get; set; } = "CredencialesInvalidas";
    public DateTime FechaIntento { get; set; } = DateTime.UtcNow;
    public int? UsuarioId { get; set; }
    public virtual Usuario? UsuarioNavigation { get; set; }
}
