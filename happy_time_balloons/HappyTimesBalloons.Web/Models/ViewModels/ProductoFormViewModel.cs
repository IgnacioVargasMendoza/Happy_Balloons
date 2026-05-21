using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class ProductoFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(200, ErrorMessage = "Máximo 200 caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, 99999999, ErrorMessage = "El precio debe ser mayor a cero.")]
        [Display(Name = "Precio (₡)")]
        public decimal Precio { get; set; }

        [Range(0, 99999999, ErrorMessage = "El precio con descuento no puede ser negativo.")]
        [Display(Name = "Precio con descuento (₡)")]
        public decimal? PrecioDescuento { get; set; }

        [Required(ErrorMessage = "El stock es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        [Display(Name = "Stock")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Selecciona una categoría.")]
        [Display(Name = "Categoría")]
        public int CategoriaId { get; set; }

        public bool EsActivo { get; set; } = true;

        public IEnumerable<SelectListItem> Categorias { get; set; }

        public List<ImagenProductoViewModel> ImagenesExistentes { get; set; }
            = new List<ImagenProductoViewModel>();
    }
}
