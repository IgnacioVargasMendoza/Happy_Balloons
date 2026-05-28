using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using HappyTimesBalloons.Web.Models.ViewModels;
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

        public AdminController(
            IProductoServicio productoServicio,
            ICategoriaServicio categoriaServicio,
            IPedidoServicio pedidoServicio,
            IAuditoriaServicio auditoriaServicio)
        {
            _productoServicio = productoServicio;
            _categoriaServicio = categoriaServicio;
            _pedidoServicio = pedidoServicio;
            _auditoriaServicio = auditoriaServicio;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var productoStats = await _productoServicio.ObtenerEstadisticasAsync();
            var categoriaStats = await _categoriaServicio.ObtenerEstadisticasAsync();
            var actividad = await _auditoriaServicio.ObtenerActividadRecienteAsync(10);
            var pedidos = await _pedidoServicio.ObtenerTodosAsync();
            var hoy = System.DateTime.UtcNow.Date;

            var vm = new AdminDashboardViewModel
            {
                TotalProductos = productoStats.Total,
                ProductosActivos = productoStats.Activos,
                ProductosConBajoStock = productoStats.ConBajoStock,

                TotalCategorias = categoriaStats.Total,
                CategoriasActivas = categoriaStats.Activas,

                TotalPedidos = pedidos.Count,
                PedidosHoy = pedidos.Count(p => p.FechaPedido.Date == hoy),
                VentasTotales = pedidos.Sum(p => p.Total),

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
    }
}
