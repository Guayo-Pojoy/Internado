using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Internado.Infrastructure.Models;

[Table("Roles", Schema = "sec")]
[Index("NombreRol", Name = "UQ_Roles_Nombre", IsUnique = true)]
public partial class Role
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string NombreRol { get; set; } = null!;

    [StringLength(200)]
    public string? Descripcion { get; set; }

    [InverseProperty("Rol")]
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
