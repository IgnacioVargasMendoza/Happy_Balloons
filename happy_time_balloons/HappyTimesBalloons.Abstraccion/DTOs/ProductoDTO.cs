using System;
using System.Collections.Generic;

namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class ProductoDTO
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
        public List<ImagenProductoDTO> Imagenes { get; set; } = new List<ImagenProductoDTO>();
    }
}
