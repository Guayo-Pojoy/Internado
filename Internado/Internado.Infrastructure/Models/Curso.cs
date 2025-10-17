using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Internado.Infrastructure.Models;

[Table("Cursos", Schema = "aca")]
public partial class Curso
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    public int DocenteId { get; set; }

    [InverseProperty("Curso")]
    public virtual ICollection<Asistencium> Asistencia { get; set; } = new List<Asistencium>();

    [InverseProperty("Curso")]
    public virtual ICollection<DocenteCurso> AsignacionesDocentes { get; set; } = new List<DocenteCurso>();

    [InverseProperty("Curso")]
    public virtual ICollection<Calificacione> Calificaciones { get; set; } = new List<Calificacione>();

    [ForeignKey("DocenteId")]
    [InverseProperty("Cursos")]
    public virtual Usuario Docente { get; set; } = null!;
}
