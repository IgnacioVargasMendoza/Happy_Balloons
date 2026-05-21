using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyTimesBalloons.AccesoADatos.Modelos
{
    [Table("DetallesPedido")]
    public class DetallePedido
    {
        [Key]
        public int Id { get; set; }

        public int PedidoId { get; set; }

        [ForeignKey("PedidoId")]
        public virtual Pedido Pedido { get; set; }

        public int ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public virtual Producto Producto { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }
    }
}
