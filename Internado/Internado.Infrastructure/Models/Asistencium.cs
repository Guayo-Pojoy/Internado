using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Internado.Infrastructure.Models;

[Table("Asistencia", Schema = "aca")]
[Index("CursoId", "Fecha", Name = "IX_Asistencia_Curso_Fecha")]
[Index("ResidenteId", "Fecha", "CursoId", Name = "UQ_Asistencia", IsUnique = true)]
public partial class Asistencium
{
    [Key]
    public long Id { get; set; }

    public int ResidenteId { get; set; }

    public DateOnly Fecha { get; set; }

    [StringLength(12)]
    public string Estado { get; set; } = null!;

    public int CursoId { get; set; }

    [StringLength(250)]
    public string? Observacion { get; set; }

    [ForeignKey("CursoId")]
    [InverseProperty("Asistencia")]
    public virtual Curso Curso { get; set; } = null!;

    [ForeignKey("ResidenteId")]
    [InverseProperty("Asistencia")]
    public virtual Residente Residente { get; set; } = null!;
}
