using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Internado.Infrastructure.Models;

[Table("Usuarios", Schema = "sec")]
[Index("Correo", Name = "IX_Usuarios_Correo")]
[Index("Usuario1", Name = "IX_Usuarios_Usuario")]
[Index("Correo", Name = "UQ_Usuarios_Correo", IsUnique = true)]
[Index("Usuario1", Name = "UQ_Usuarios_Usuario", IsUnique = true)]
public partial class Usuario
{
    [Key]
    public int Id { get; set; }

    [StringLength(120)]
    public string Nombre { get; set; } = null!;

    [Column("Usuario")]
    [StringLength(60)]
    public string Usuario1 { get; set; } = null!;

    [StringLength(120)]
    public string Correo { get; set; } = null!;

    [MaxLength(256)]
    public byte[] HashContrasena { get; set; } = null!;

    public int RolId { get; set; }

    public bool Estado { get; set; }

    [Precision(0)]
    public DateTime FechaRegistro { get; set; }

    public int IntentosFallidos { get; set; }

    [Precision(0)]
    public DateTime? BloqueadoHastaUtc { get; set; }

    [InverseProperty("Medico")]
    public virtual ICollection<Consulta> Consulta { get; set; } = new List<Consulta>();

    [InverseProperty("Docente")]
    public virtual ICollection<Curso> Cursos { get; set; } = new List<Curso>();

    [InverseProperty("Docente")]
    public virtual ICollection<DocenteCurso> DocenteCursos { get; set; } = new List<DocenteCurso>();

    [InverseProperty("Usuario")]
    public virtual ICollection<MovimientosMedicamento> MovimientosMedicamentos { get; set; } = new List<MovimientosMedicamento>();

    [ForeignKey("RolId")]
    [InverseProperty("Usuarios")]
    public virtual Role Rol { get; set; } = null!;
}
