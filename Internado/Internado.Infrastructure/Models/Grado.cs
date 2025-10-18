namespace Internado.Infrastructure.Models;

/// <summary>
/// Representa un grado académico (1er año, 2do año, etc.)
/// </summary>
public partial class Grado
{
    public int Id { get; set; }

    /// <summary>
    /// Nombre del grado (Ej: "1er Año", "2do Año")
    /// </summary>
    public string Nombre { get; set; } = null!;

    /// <summary>
    /// Nivel educativo
    /// </summary>
    public int Nivel { get; set; }

    /// <summary>
    /// Descripción del grado
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Estado: Activo, Inactivo
    /// </summary>
    public string Estado { get; set; } = "Activo";

    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Navegación
    public virtual ICollection<Residente> Residentes { get; set; } = new List<Residente>();
    public virtual ICollection<GradoCurso> GradoCursos { get; set; } = new List<GradoCurso>();
}
