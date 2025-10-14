using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Internado.Infrastructure.Models;

[Table("HistorialAcademico", Schema = "aca")]
[Index("ResidenteId", "Anio", Name = "UQ_HA_Res_Anio", IsUnique = true)]
public partial class HistorialAcademico
{
    [Key]
    public int Id { get; set; }

    public int ResidenteId { get; set; }

    public int Anio { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal Promedio { get; set; }

    [StringLength(400)]
    public string? Observaciones { get; set; }

    [ForeignKey("ResidenteId")]
    [InverseProperty("HistorialAcademicos")]
    public virtual Residente Residente { get; set; } = null!;
}
