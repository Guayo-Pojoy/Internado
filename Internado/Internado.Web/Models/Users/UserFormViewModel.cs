using System.ComponentModel.DataAnnotations;

namespace Internado.Web.Models.Users
{
    public class UserFormViewModel
    {
        public int? Id { get; set; }

        [Required, StringLength(120)]
        public string Nombre { get; set; } = "";

        [Required, StringLength(60), Display(Name="Usuario")]
        public string Usuario { get; set; } = "";

        [Required, EmailAddress, StringLength(120)]
        public string Correo { get; set; } = "";

        [Display(Name="Rol")]
        public int RolId { get; set; }

        [Display(Name="Activo")]
        public bool Estado { get; set; } = true;

        [DataType(DataType.Password)]
        [Display(Name="Contraseña")]
        public string? Password { get; set; }
    }
}
