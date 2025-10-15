using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Internado.Domain.Entities
{
    /// <summary>
    /// Entidad que representa una habitación del internado
    /// </summary>
    [Table("Habitaciones", Schema = "aca")]
    public class Habitacion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El número de habitación es obligatorio")]
        [StringLength(10, ErrorMessage = "El número no puede exceder 10 caracteres")]
        [Display(Name = "Número de Habitación")]
        public string Numero { get; set; } = string.Empty;

        [Required(ErrorMessage = "El piso es obligatorio")]
        [Range(1, 10, ErrorMessage = "El piso debe estar entre 1 y 10")]
        [Display(Name = "Piso")]
        public int Piso { get; set; }

        [Required(ErrorMessage = "La capacidad es obligatoria")]
        [Range(1, 6, ErrorMessage = "La capacidad debe estar entre 1 y 6")]
        [Display(Name = "Capacidad")]
        public int Capacidad { get; set; }

        [Required(ErrorMessage = "El tipo es obligatorio")]
        [StringLength(20)]
        [RegularExpression("^(Individual|Doble|Compartida)$", ErrorMessage = "Tipo inválido")]
        [Display(Name = "Tipo")]
        public string Tipo { get; set; } = "Compartida";

        [Required(ErrorMessage = "El estado es obligatorio")]
        [StringLength(20)]
        [RegularExpression("^(Disponible|Ocupada|Mantenimiento)$", ErrorMessage = "Estado inválido")]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Disponible";

        [StringLength(500)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Propiedad calculada - ocupantes actuales
        [NotMapped]
        public int OcupantesActuales { get; set; }

        // Propiedad calculada - disponibilidad
        [NotMapped]
        public bool TieneCupo => Estado == "Disponible" && OcupantesActuales < Capacidad;

        // Propiedad calculada - porcentaje de ocupación
        [NotMapped]
        public double PorcentajeOcupacion => Capacidad > 0 ? (double)OcupantesActuales / Capacidad * 100 : 0;
    }
}