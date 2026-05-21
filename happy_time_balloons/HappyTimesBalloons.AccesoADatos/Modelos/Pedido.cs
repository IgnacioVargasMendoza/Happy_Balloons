using HappyTimesBalloons.Abstraccion.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyTimesBalloons.AccesoADatos.Modelos
{
    [Table("Pedidos")]
    public class Pedido
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string Numero { get; set; }

        [Required, MaxLength(128)]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser Usuario { get; set; }

        public DateTime FechaPedido { get; set; }

        public EstadoPedido EstadoPedido { get; set; }

        [Required, MaxLength(20)]
        public string MetodoPago { get; set; }

        [MaxLength(100)]
        public string NumeroReferencia { get; set; }

        public int ZonaEntregaId { get; set; }

        [ForeignKey("ZonaEntregaId")]
        public virtual ZonaEntrega ZonaEntrega { get; set; }

        [Required, MaxLength(500)]
        public string DireccionEntrega { get; set; }

        public decimal Total { get; set; }

        public decimal Subtotal { get; set; }

        public decimal CostoEnvio { get; set; }

        [MaxLength(1000)]
        public string Notas { get; set; }

        public virtual ICollection<DetallePedido> DetallesPedido { get; set; }
            = new List<DetallePedido>();
    }
}
