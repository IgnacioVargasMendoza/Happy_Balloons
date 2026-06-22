using HappyTimesBalloons.Abstraccion.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyTimesBalloons.AccesoADatos.Modelos
{
    [Table("MovimientosInventario")]
    public class MovimientoInventario
    {
        [Key]
        public int Id { get; set; }

        public int ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public virtual Producto Producto { get; set; }

        public TipoMovimiento TipoMovimiento { get; set; }

        public int Cantidad { get; set; }

        public int StockAnterior { get; set; }

        public int StockNuevo { get; set; }

        [Required, MaxLength(500)]
        public string Motivo { get; set; }

        [Required, MaxLength(128)]
        public string UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual ApplicationUser Usuario { get; set; }

        public DateTime FechaMovimiento { get; set; }
    }
}
