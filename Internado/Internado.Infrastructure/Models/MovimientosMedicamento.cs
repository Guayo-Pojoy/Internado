using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Internado.Infrastructure.Models;

[Table("MovimientosMedicamentos", Schema = "med")]
[Index("MedicamentoId", "Fecha", Name = "IX_MM_Med_Fecha")]
public partial class MovimientosMedicamento
{
    [Key]
    public long Id { get; set; }

    public int MedicamentoId { get; set; }

    [StringLength(10)]
    public string TipoMovimiento { get; set; } = null!;

    public int Cantidad { get; set; }

    [Precision(0)]
    public DateTime Fecha { get; set; }

    public int UsuarioId { get; set; }

    [ForeignKey("MedicamentoId")]
    [InverseProperty("MovimientosMedicamentos")]
    public virtual Medicamento Medicamento { get; set; } = null!;

    [ForeignKey("UsuarioId")]
    [InverseProperty("MovimientosMedicamentos")]
    public virtual Usuario Usuario { get; set; } = null!;
}
