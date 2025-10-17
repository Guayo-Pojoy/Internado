namespace Internado.Infrastructure.Models;

/// <summary>
/// Tabla intermediaria para relación muchos-a-muchos entre Docentes y Cursos
/// Permite que un docente tenga múltiples cursos y un curso múltiples docentes
/// </summary>
public partial class DocenteCurso
{
    public int Id { get; set; }

    /// <summary>
    /// FK al usuario (docente)
    /// </summary>
    public int DocenteId { get; set; }

    /// <summary>
    /// FK al curso
    /// </summary>
    public int CursoId { get; set; }

    /// <summary>
    /// Fecha de asignación
    /// </summary>
    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Si la asignación está activa
    /// </summary>
    public bool Activa { get; set; } = true;

    // Navegación
    public virtual Usuario Docente { get; set; } = null!;
    public virtual Curso Curso { get; set; } = null!;
}
