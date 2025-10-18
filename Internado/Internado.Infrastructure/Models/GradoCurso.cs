namespace Internado.Infrastructure.Models;

/// <summary>
/// Relación muchos a muchos entre Grados y Cursos
/// </summary>
public partial class GradoCurso
{
    public int Id { get; set; }

    /// <summary>
    /// ID del Grado
    /// </summary>
    public int GradoId { get; set; }

    /// <summary>
    /// ID del Curso
    /// </summary>
    public int CursoId { get; set; }

    /// <summary>
    /// Indica si la asignación está activa
    /// </summary>
    public bool Activa { get; set; } = true;

    /// <summary>
    /// Fecha en que se asignó el curso al grado
    /// </summary>
    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;

    // Navegación
    public virtual Grado Grado { get; set; } = null!;
    public virtual Curso Curso { get; set; } = null!;
}
