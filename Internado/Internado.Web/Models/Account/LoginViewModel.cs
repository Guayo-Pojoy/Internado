using System.ComponentModel.DataAnnotations;

namespace Internado.Web.Models.Account
{
    public class LoginViewModel
    {
        [Required, Display(Name = "Usuario")]
        public string UserName { get; set; } = "";

        [Required, DataType(DataType.Password), Display(Name = "Contraseña")]
        public string Password { get; set; } = "";

        [Display(Name = "Recordarme")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
