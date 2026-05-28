using System.ComponentModel.DataAnnotations;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class CategoriaFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(200, ErrorMessage = "Máximo 200 caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [MaxLength(1000, ErrorMessage = "Máximo 1000 caracteres.")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }
    }
}