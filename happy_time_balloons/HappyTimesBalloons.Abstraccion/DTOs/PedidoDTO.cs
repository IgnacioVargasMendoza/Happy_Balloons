using HappyTimesBalloons.Abstraccion.Enums;
using System;
using System.Collections.Generic;

namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class PedidoDTO
    {
        public int Id { get; set; }
        public string Numero { get; set; }
        public string UserId { get; set; }
        public string NombreUsuario { get; set; }
        public DateTime FechaPedido { get; set; }
        public EstadoPedido EstadoPedido { get; set; }
        public string MetodoPago { get; set; }
        public string NumeroReferencia { get; set; }
        public int ZonaEntregaId { get; set; }
        public string NombreZona { get; set; }
        public string DireccionEntrega { get; set; }
        public decimal Total { get; set; }
        public decimal Subtotal { get; set; }
        public decimal CostoEnvio { get; set; }
        public string Notas { get; set; }
        public IList<DetallePedidoDTO> Detalles { get; set; } = new List<DetallePedidoDTO>();
    }
}
