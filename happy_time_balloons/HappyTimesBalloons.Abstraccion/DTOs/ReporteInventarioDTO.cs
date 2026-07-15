using System.Collections.Generic;

namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class ReporteInventarioDTO
    {
        // Filtros aplicados
        public int? CategoriaId { get; set; }
        public string NombreCategoria { get; set; }
        public string EstadoStock { get; set; }

        // KPIs (sobre el subconjunto filtrado)
        public int TotalProductos { get; set; }
        public int ProductosStockBajo { get; set; }
        public int ProductosSinStock { get; set; }
        public decimal ValorTotalInventario { get; set; }

        public List<FilaInventarioReporteDTO> Items { get; set; } = new List<FilaInventarioReporteDTO>();
    }

    public class FilaInventarioReporteDTO
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; }
        public string Categoria { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public string EstadoStock { get; set; }
    }
}
