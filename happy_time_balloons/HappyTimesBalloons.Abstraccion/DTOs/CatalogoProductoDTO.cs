using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class CatalogoProductoDTO
    {
        public int ProductoId { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public decimal Precio { get; set; }

        public decimal? PrecioDescuento { get; set; }

        public string CategoriaNombre { get; set; }

        public string ImagenPrincipal { get; set; }

        public bool EsActivo { get; set; }

        public bool Disponible { get; set; }
    }
}
