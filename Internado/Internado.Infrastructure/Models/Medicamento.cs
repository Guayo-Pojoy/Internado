using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Internado.Infrastructure.Models;

[Table("Medicamentos", Schema = "med")]
[Index("FechaVencimiento", Name = "IX_Medicamentos_Vencimiento")]
public partial class Medicamento
{
    [Key]
    public int Id { get; set; }

    [StringLength(120)]
    public string Nombre { get; set; } = null!;

    [StringLength(50)]
    public string Lote { get; set; } = null!;

    public DateOnly FechaVencimiento { get; set; }

    public int StockActual { get; set; }

    public int StockMinimo { get; set; }

    [InverseProperty("Medicamento")]
    public virtual ICollection<MovimientosMedicamento> MovimientosMedicamentos { get; set; } = new List<MovimientosMedicamento>();
}
