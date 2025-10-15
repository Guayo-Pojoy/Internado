using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Internado.Infrastructure.Models
{
    public partial class Habitacion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El numero de habitacion es obligatorio")]
        [StringLength(20, ErrorMessage = "El numero no puede exceder 20 caracteres")]
        [Display(Name = "Numero")]
        public string Numero { get; set; } = null!;

        [Required(ErrorMessage = "La capacidad es obligatoria")]
        [Range(1, 10, ErrorMessage = "La capacidad debe estar entre 1 y 10 personas")]
        public int Capacidad { get; set; }

        [Required(ErrorMessage = "El tipo es obligatorio")]
        [StringLength(30, ErrorMessage = "El tipo no puede exceder 30 caracteres")]
        public string Tipo { get; set; } = null!;

        [Required(ErrorMessage = "El estado es obligatorio")]
        [StringLength(30, ErrorMessage = "El estado no puede exceder 30 caracteres")]
        public string Estado { get; set; } = "Disponible";

        [StringLength(50)]
        public string? Piso { get; set; }

        [StringLength(50)]
        public string? Edificio { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }

        // Navegacion - Relacion con Residentes
        public virtual ICollection<Residente>? Residentes { get; set; }
    }
}
