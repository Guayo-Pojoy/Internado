using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Internado.Infrastructure.Models;

[Table("HistorialMedico", Schema = "med")]
[Index("ResidenteId", "Fecha", Name = "IX_HM_Res_Fecha")]
public partial class HistorialMedico
{
    [Key]
    public int Id { get; set; }

    public int ResidenteId { get; set; }

    [Precision(0)]
    public DateTime Fecha { get; set; }

    [StringLength(400)]
    public string Diagnostico { get; set; } = null!;

    [StringLength(400)]
    public string? Tratamiento { get; set; }

    [ForeignKey("ResidenteId")]
    [InverseProperty("HistorialMedicos")]
    public virtual Residente Residente { get; set; } = null!;
}
