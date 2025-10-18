namespace Internado.Infrastructure.Models;

/// <summary>
/// Registra la matrícula de un residente en un curso específico
/// Permite excepciones: residentes fuera del grado pero inscritos en cursos
/// </summary>
public partial class Matricula
{
    public int Id { get; set; }

    /// <summary>
    /// FK al residente
    /// </summary>
    public int ResidenteId { get; set; }

    /// <summary>
    /// FK al curso
    /// </summary>
    public int CursoId { get; set; }

    /// <summary>
    /// Período académico (Ej: "2025-1")
    /// </summary>
    public string? Periodo { get; set; }

    /// <summary>
    /// Fecha de inscripción
    /// </summary>
    public DateTime FechaMatricula { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Si la matrícula está activa
    /// </summary>
    public bool Activa { get; set; } = true;

    /// <summary>
    /// Razón de exclusión si aplica
    /// </summary>
    public string? Razon { get; set; }

    // Navegación
    public virtual Residente Residente { get; set; } = null!;
    public virtual Curso Curso { get; set; } = null!;
}
