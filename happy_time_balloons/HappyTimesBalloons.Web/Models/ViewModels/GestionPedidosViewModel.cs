using System.Collections.Generic;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class GestionPedidosViewModel
    {
        public List<PedidoResumenViewModel> Pedidos { get; set; } = new List<PedidoResumenViewModel>();
        public string FiltroEstado { get; set; }
        public string FiltroBusqueda { get; set; }
        public SelectList EstadosDisponibles { get; set; }
    }
}
