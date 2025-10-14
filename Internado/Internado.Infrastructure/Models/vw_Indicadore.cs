using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Internado.Infrastructure.Models;

[Keyless]
public partial class vw_Indicadore
{
    [Column(TypeName = "datetime")]
    public DateTime Corte { get; set; }

    public int? Activas { get; set; }

    public int? Egresadas { get; set; }

    public int? MedicamentosBajoStock { get; set; }

    public int? MedicamentosVencidos { get; set; }
}
