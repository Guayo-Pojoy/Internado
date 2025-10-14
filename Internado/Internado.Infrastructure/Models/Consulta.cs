using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Internado.Infrastructure.Models;

[Table("Consultas", Schema = "med")]
[Index("MedicoId", "Fecha", Name = "IX_Consultas_Medico_Fecha")]
[Index("ResidenteId", "Fecha", Name = "IX_Consultas_Res_Fecha")]
public partial class Consulta
{
    [Key]
    public long Id { get; set; }

    public int ResidenteId { get; set; }

    [Precision(0)]
    public DateTime Fecha { get; set; }

    [StringLength(400)]
    public string Diagnostico { get; set; } = null!;

    [StringLength(400)]
    public string? Tratamiento { get; set; }

    public int MedicoId { get; set; }

    [ForeignKey("MedicoId")]
    [InverseProperty("Consulta")]
    public virtual Usuario Medico { get; set; } = null!;

    [ForeignKey("ResidenteId")]
    [InverseProperty("Consulta")]
    public virtual Residente Residente { get; set; } = null!;
}
