using System.Collections.Generic;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class CatalogoIndexViewModel
    {
        public List<ProductoViewModel> Productos { get; set; } = new List<ProductoViewModel>();
        public IEnumerable<SelectListItem> Categorias { get; set; } = new List<SelectListItem>();
        public string Busqueda { get; set; }
        public int? CategoriaId { get; set; }
        public PaginacionViewModel Paginacion { get; set; }
    }
}
