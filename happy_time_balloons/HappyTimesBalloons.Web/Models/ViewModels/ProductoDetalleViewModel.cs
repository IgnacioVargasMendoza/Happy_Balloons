using System.Collections.Generic;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class ProductoDetalleViewModel
    {
        public ProductoViewModel Producto { get; set; }
        public List<ProductoViewModel> ProductosRelacionados { get; set; } = new List<ProductoViewModel>();
    }
}
