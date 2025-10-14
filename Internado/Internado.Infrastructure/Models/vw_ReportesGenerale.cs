using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Internado.Infrastructure.Models;

[Keyless]
public partial class vw_ReportesGenerale
{
    public int ResidenteId { get; set; }

    [StringLength(150)]
    public string NombreCompleto { get; set; } = null!;

    [StringLength(15)]
    public string Estado { get; set; } = null!;

    public int? Edad { get; set; }

    public int? Faltas { get; set; }

    [Column(TypeName = "decimal(38, 6)")]
    public decimal? Promedio { get; set; }

    [Precision(0)]
    public DateTime? UltimaConsulta { get; set; }
}
