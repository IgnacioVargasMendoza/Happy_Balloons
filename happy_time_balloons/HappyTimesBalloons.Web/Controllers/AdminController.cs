using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using HappyTimesBalloons.Web.Models.ViewModels;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly IProductoServicio _productoServicio;
        private readonly ICategoriaServicio _categoriaServicio;
        private readonly IPedidoServicio _pedidoServicio;
        private readonly IAuditoriaServicio _auditoriaServicio;
        private readonly IInventarioServicio _inventarioServicio;

        public AdminController(
            IProductoServicio productoServicio,
            ICategoriaServicio categoriaServicio,
            IPedidoServicio pedidoServicio,
            IAuditoriaServicio auditoriaServicio,
            IInventarioServicio inventarioServicio)
        {
            _productoServicio = productoServicio;
            _categoriaServicio = categoriaServicio;
            _pedidoServicio = pedidoServicio;
            _auditoriaServicio = auditoriaServicio;
            _inventarioServicio = inventarioServicio;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var productoStats = await _productoServicio.ObtenerEstadisticasAsync();
            var categoriaStats = await _categoriaServicio.ObtenerEstadisticasAsync();
            var pedidoStats = await _pedidoServicio.ObtenerEstadisticasAsync();
            var inventarioKpis = await _inventarioServicio.ObtenerKpisAsync();
            var ventasDiarias = await _pedidoServicio.ObtenerVentasPorDiaAsync(7);
            var actividad = await _auditoriaServicio.ObtenerActividadRecienteAsync(10);

            var vm = new AdminDashboardViewModel
            {
                TotalProductos = productoStats.Total,
                ProductosActivos = productoStats.Activos,
                ProductosConBajoStock = productoStats.ConBajoStock,

                TotalCategorias = categoriaStats.Total,
                CategoriasActivas = categoriaStats.Activas,

                TotalPedidos = pedidoStats.Total,
                PedidosHoy = pedidoStats.PedidosHoy,
                VentasTotales = pedidoStats.VentasTotales,

                ProductosSinStock = inventarioKpis.ProductosSinStock,
                ProductosStockBajoInventario = inventarioKpis.ProductosStockBajo,
                ValorTotalInventario = inventarioKpis.ValorTotalInventario,

                VentasUltimos7Dias = ventasDiarias,

                ActividadReciente = actividad.Select(b => new BitacoraResumenViewModel
                {
                    NombreUsuario = b.NombreUsuario,
                    Accion = b.Accion.ToString(),
                    TablaAfectada = b.TablaAfectada,
                    FechaHoraUtc = b.FechaHoraUtc,
                    Detalle = b.Detalle
                }).ToList()
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<ActionResult> DatosJson()
        {
            var pedidoStats = await _pedidoServicio.ObtenerEstadisticasAsync();
            var productoStats = await _productoServicio.ObtenerEstadisticasAsync();
            var inventarioKpis = await _inventarioServicio.ObtenerKpisAsync();
            var ventasDiarias = await _pedidoServicio.ObtenerVentasPorDiaAsync(7);

            var datos = new
            {
                pedidosHoy = pedidoStats.PedidosHoy,
                totalPedidos = pedidoStats.Total,
                ventasTotales = pedidoStats.VentasTotales,
                productosConBajoStock = productoStats.ConBajoStock,
                productosSinStock = inventarioKpis.ProductosSinStock,
                valorTotalInventario = inventarioKpis.ValorTotalInventario,
                ventasDiarias = ventasDiarias.Select(v => new { v.Fecha, v.Total, v.Cantidad }).ToList()
            };

            return Content(JsonConvert.SerializeObject(datos), "application/json");
        }
    }
}
