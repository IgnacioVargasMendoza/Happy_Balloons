using HappyTimesBalloons.Abstraccion.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyTimesBalloons.AccesoADatos.Modelos
{
    [Table("PagosSinpe")]
    public class PagoSinpe
    {
        public int Id { get; set; }

        public int PedidoId { get; set; }

        [ForeignKey("PedidoId")]
        public virtual Pedido Pedido { get; set; }

        [Required, MaxLength(100)]
        public string NumeroComprobante { get; set; }

        public decimal Monto { get; set; }

        [MaxLength(200)]
        public string NombreTitular { get; set; }

        [MaxLength(20)]
        public string TelefonoDestino { get; set; }

        public EstadoPagoSinpe EstadoPago { get; set; }

        [MaxLength(500)]
        public string MotivoRechazo { get; set; }

        public DateTime FechaRecepcion { get; set; }

        public DateTime? FechaProcesamiento { get; set; }
    }
}
