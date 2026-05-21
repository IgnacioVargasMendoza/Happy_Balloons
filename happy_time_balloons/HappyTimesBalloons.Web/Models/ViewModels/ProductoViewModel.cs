using System;
using System.Collections.Generic;
using System.Linq;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class ProductoViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public decimal? PrecioDescuento { get; set; }
        public int Stock { get; set; }
        public int CategoriaId { get; set; }
        public string CategoriaNombre { get; set; }
        public bool EsActivo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public List<ImagenProductoViewModel> Imagenes { get; set; } = new List<ImagenProductoViewModel>();

        public string ImagenPrincipal =>
            Imagenes.FirstOrDefault(i => i.EsPrincipal)?.RutaImagen
            ?? Imagenes.FirstOrDefault()?.RutaImagen;

        public bool TieneDescuento => PrecioDescuento.HasValue && PrecioDescuento < Precio;
    }
}
