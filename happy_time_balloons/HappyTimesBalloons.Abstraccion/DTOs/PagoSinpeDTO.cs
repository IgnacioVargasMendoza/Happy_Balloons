using HappyTimesBalloons.Abstraccion.Enums;
using System;

namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class PagoSinpeDTO
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public string NumeroPedido { get; set; }
        public string NumeroComprobante { get; set; }
        public decimal Monto { get; set; }
        public string NombreTitular { get; set; }
        public string TelefonoDestino { get; set; }
        public EstadoPagoSinpe EstadoPago { get; set; }
        public string MotivoRechazo { get; set; }
        public DateTime FechaRecepcion { get; set; }
        public DateTime? FechaProcesamiento { get; set; }
    }
}
