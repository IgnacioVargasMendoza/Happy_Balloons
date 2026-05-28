using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using HappyTimesBalloons.Web.Models.ViewModels;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _ctx;
        private readonly IPedidoServicio _pedidoServicio;

        public AdminController(ApplicationDbContext ctx, IPedidoServicio pedidoServicio)
        {
            _ctx = ctx;
            _pedidoServicio = pedidoServicio;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var productos = await _ctx.Productos.ToListAsync();
            var categorias = await _ctx.Categorias.ToListAsync();
            var actividad = await _ctx.BitacoraAuditoria
                .OrderByDescending(b => b.FechaHoraUtc)
                .Take(10)
                .ToListAsync();

            var pedidos = await _pedidoServicio.ObtenerTodosAsync();
            var hoy = System.DateTime.UtcNow.Date;

            var vm = new AdminDashboardViewModel
            {
                TotalProductos        = productos.Count,
                ProductosActivos      = productos.Count(p => p.EsActivo),
                ProductosConBajoStock = productos.Count(p => p.Stock <= 5 && p.EsActivo),
                TotalCategorias       = categorias.Count,
                CategoriasActivas     = categorias.Count(c => c.EsActiva),

                TotalPedidos  = pedidos.Count,
                PedidosHoy    = pedidos.Count(p => p.FechaPedido.Date == hoy),
                VentasTotales = pedidos.Sum(p => p.Total),

                ActividadReciente = actividad.Select(b => new BitacoraResumenViewModel
                {
                    NombreUsuario = b.NombreUsuario ?? "Sistema",
                    Accion        = b.Accion.ToString(),
                    TablaAfectada = b.TablaAfectada,
                    FechaHoraUtc  = b.FechaHoraUtc,
                    Detalle       = b.Detalle
                }).ToList()
            };

            return View(vm);
        }
    }
}
