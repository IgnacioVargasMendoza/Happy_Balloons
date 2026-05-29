using System.ComponentModel.DataAnnotations;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class RegistroViewModel
    {
        [Required(ErrorMessage = "El nombre es requerido.")]
        [StringLength(100)]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El correo es requerido.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; }

        [Required(ErrorMessage = "Debes confirmar tu contraseña.")]
        [DataType(DataType.Password)]
        [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden.")]
        [Display(Name = "Confirmar contraseña")]
        public string ConfirmarContrasena { get; set; }

        [Required(ErrorMessage = "El teléfono es requerido.")]
        [Phone(ErrorMessage = "Formato de teléfono inválido.")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [Display(Name = "Dirección (opcional)")]
        public string Direccion { get; set; }
    }
}
