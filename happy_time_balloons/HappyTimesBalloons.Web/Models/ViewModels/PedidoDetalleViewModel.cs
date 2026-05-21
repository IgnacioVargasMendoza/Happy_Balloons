using HappyTimesBalloons.Abstraccion.DTOs;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class PedidoDetalleViewModel
    {
        public PedidoDTO Pedido { get; set; }
        public PedidoResumenViewModel Resumen { get; set; }
    }
}
