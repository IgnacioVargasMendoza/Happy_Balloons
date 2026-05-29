using System.Collections.Generic;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class ProductoIndexViewModel
    {
        public List<ProductoViewModel> Productos { get; set; }
        public string Busqueda { get; set; }
        public int? CategoriaId { get; set; }
        public IEnumerable<SelectListItem> Categorias { get; set; }
    }
}
