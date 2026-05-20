using System.ComponentModel.DataAnnotations;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class CategoriaFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [MaxLength(500, ErrorMessage = "Máximo 500 caracteres.")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }
    }
}
