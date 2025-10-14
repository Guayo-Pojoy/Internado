using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Internado.Infrastructure.Models;

[Table("AuditoriaAccesos", Schema = "sec")]
[Index("FechaUtc", Name = "IX_Auditoria_Fecha")]
[Index("UsuarioId", Name = "IX_Auditoria_Usuario")]
public partial class AuditoriaAcceso
{
    [Key]
    public long Id { get; set; }

    public int? UsuarioId { get; set; }

    [Precision(0)]
    public DateTime FechaUtc { get; set; }

    [StringLength(45)]
    public string? IP { get; set; }

    [StringLength(50)]
    public string Accion { get; set; } = null!;

    [StringLength(20)]
    public string Resultado { get; set; } = null!;
}
