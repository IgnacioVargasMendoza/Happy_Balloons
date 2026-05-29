using System.ComponentModel.DataAnnotations;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class Verificar2FAViewModel
    {
        [Required(ErrorMessage = "El código de verificación es obligatorio.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "El código debe tener exactamente 6 dígitos.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "El código debe contener solo dígitos.")]
        [Display(Name = "Código de verificación")]
        public string Codigo { get; set; }

        // Email parcialmente enmascarado para mostrar en la vista (ej: na***@mail.com)
        public string EmailParcial { get; set; }

        // Mensaje de error para mostrar intentos restantes u otros avisos
        public string MensajeError { get; set; }
    }
}
