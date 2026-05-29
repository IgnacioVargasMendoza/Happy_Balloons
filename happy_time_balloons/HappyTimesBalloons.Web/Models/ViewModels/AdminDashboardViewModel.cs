using System;
using System.Collections.Generic;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalProductos { get; set; }
        public int ProductosActivos { get; set; }
        public int ProductosConBajoStock { get; set; }

        public int TotalCategorias { get; set; }
        public int CategoriasActivas { get; set; }

        public int TotalPedidos { get; set; }
        public int PedidosHoy { get; set; }
        public decimal VentasTotales { get; set; }

        public List<BitacoraResumenViewModel> ActividadReciente { get; set; }
            = new List<BitacoraResumenViewModel>();
    }

    public class BitacoraResumenViewModel
    {
        public string NombreUsuario { get; set; }
        public string Accion { get; set; }
        public string TablaAfectada { get; set; }
        public DateTime FechaHoraUtc { get; set; }
        public string Detalle { get; set; }
    }
}
