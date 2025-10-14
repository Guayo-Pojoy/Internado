using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Internado.Infrastructure.Models;

[Table("Calificaciones", Schema = "aca")]
[Index("ResidenteId", "CursoId", Name = "IX_Calif_Res_Curso")]
public partial class Calificacione
{
    [Key]
    public long Id { get; set; }

    public int ResidenteId { get; set; }

    public int CursoId { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal Nota { get; set; }

    [Precision(0)]
    public DateTime FechaRegistro { get; set; }

    [ForeignKey("CursoId")]
    [InverseProperty("Calificaciones")]
    public virtual Curso Curso { get; set; } = null!;

    [ForeignKey("ResidenteId")]
    [InverseProperty("Calificaciones")]
    public virtual Residente Residente { get; set; } = null!;
}
