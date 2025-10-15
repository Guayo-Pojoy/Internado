using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Internado.Infrastructure.Models
{
    [Table("Residentes", Schema = "aca")]
    public partial class Residente
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string NombreCompleto { get; set; } = null!;

        [Required, StringLength(20)]
        public string DPI { get; set; } = null!;

        [Required, StringLength(120)]
        public string Tutor { get; set; } = null!;

        [Required, StringLength(20)]
        public string Estado { get; set; } = "Activa";

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de nacimiento")]
        public DateOnly FechaNacimiento { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de ingreso")]
        public DateOnly FechaIngreso { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de egreso")]
        public DateOnly? FechaEgreso { get; set; }

        [Display(Name = "Habitación")]
        public int? HabitacionId { get; set; }

        // ===== Navegación esperada por el DbContext =====
        public virtual ICollection<Asistencium> Asistencia { get; set; } = new List<Asistencium>();
        public virtual ICollection<Calificacione> Calificaciones { get; set; } = new List<Calificacione>();
        public virtual ICollection<Consulta> Consulta { get; set; } = new List<Consulta>();
        public virtual ICollection<HistorialAcademico> HistorialAcademicos { get; set; } = new List<HistorialAcademico>();
        public virtual ICollection<HistorialMedico> HistorialMedicos { get; set; } = new List<HistorialMedico>();
        public virtual Habitacion? Habitacion { get; set; }
    }
}