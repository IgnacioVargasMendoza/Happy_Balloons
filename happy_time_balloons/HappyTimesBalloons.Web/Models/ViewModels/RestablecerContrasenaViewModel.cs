using System.ComponentModel.DataAnnotations;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class RestablecerContrasenaViewModel
    {
        public string Token { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es requerida.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        [Display(Name = "Nueva contraseña")]
        public string NuevaContrasena { get; set; }

        [Required(ErrorMessage = "Confirma la nueva contraseña.")]
        [Compare("NuevaContrasena", ErrorMessage = "Las contraseñas no coinciden.")]
        [Display(Name = "Confirmar contraseña")]
        public string ConfirmarContrasena { get; set; }
    }
}
