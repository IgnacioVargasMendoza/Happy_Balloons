using System.Collections.Generic;

namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class InventarioKpisDTO
    {
        public int TotalProductos { get; set; }
        public int ProductosStockBajo { get; set; }
        public int ProductosSinStock { get; set; }
        public decimal ValorTotalInventario { get; set; }
        public List<InventarioDTO> AlertasReabastecimiento { get; set; } = new List<InventarioDTO>();
    }
}
