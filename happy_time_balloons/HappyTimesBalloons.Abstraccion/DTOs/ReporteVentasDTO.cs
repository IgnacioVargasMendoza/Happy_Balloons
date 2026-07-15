using System;
using System.Collections.Generic;

namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class ReporteVentasDTO
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        // KPIs
        public int TotalPedidos { get; set; }
        public decimal IngresosTotales { get; set; }
        public decimal TicketPromedio { get; set; }
        public int UnidadesVendidas { get; set; }

        public List<FilaPedidoReporteDTO> Pedidos { get; set; } = new List<FilaPedidoReporteDTO>();
        public List<LineaProductoReporteDTO> ProductosMasVendidos { get; set; } = new List<LineaProductoReporteDTO>();
    }

    public class FilaPedidoReporteDTO
    {
        public int Id { get; set; }
        public string Numero { get; set; }
        public DateTime FechaPedido { get; set; }
        public string NombreCliente { get; set; }
        public string MetodoPago { get; set; }
        public decimal Total { get; set; }
    }

    public class LineaProductoReporteDTO
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; }
        public string Categoria { get; set; }
        public int UnidadesVendidas { get; set; }
        public decimal Ingresos { get; set; }
    }
}
