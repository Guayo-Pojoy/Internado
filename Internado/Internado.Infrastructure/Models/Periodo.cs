using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Internado.Infrastructure.Models;

[Table("Periodos", Schema = "aca")]
[Index("Anio", "Trimestre", Name = "UQ_Periodo", IsUnique = true)]
public partial class Periodo
{
    [Key]
    public int Id { get; set; }

    public int Anio { get; set; }

    public byte Trimestre { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }
}
