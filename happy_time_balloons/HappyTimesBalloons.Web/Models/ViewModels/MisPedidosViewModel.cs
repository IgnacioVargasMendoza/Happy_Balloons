using System.Collections.Generic;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class MisPedidosViewModel
    {
        public List<PedidoResumenViewModel> Pedidos { get; set; } = new List<PedidoResumenViewModel>();
    }
}
