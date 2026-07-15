using HappyTimesBalloons.Abstraccion.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyTimesBalloons.AccesoADatos.Modelos
{
    [Table("ProgramacionesEntrega")]
    public class ProgramacionEntrega
    {
        [Key]
        public int Id { get; set; }

        public int PedidoId { get; set; }

        [ForeignKey("PedidoId")]
        public virtual Pedido Pedido { get; set; }

        public int HorarioEntregaId { get; set; }

        [ForeignKey("HorarioEntregaId")]
        public virtual HorarioEntrega HorarioEntrega { get; set; }

        public DateTime FechaEntrega { get; set; }

        public EstadoProgramacionEntrega EstadoProgramacion { get; set; }

        public DateTime FechaProgramacion { get; set; }

        [MaxLength(1000)]
        public string Notas { get; set; }

        [Required, MaxLength(128)]
        public string UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual ApplicationUser Usuario { get; set; }
    }
}
