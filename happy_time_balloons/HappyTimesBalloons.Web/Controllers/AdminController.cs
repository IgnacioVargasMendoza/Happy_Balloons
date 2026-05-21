using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.Web.Models.ViewModels;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        // GET /Admin
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            using (var ctx = new ApplicationDbContext())
            {
                var productos = await ctx.Productos.ToListAsync();
                var categorias = await ctx.Categorias.ToListAsync();
                var actividad = await ctx.BitacoraAuditoria
                    .OrderByDescending(b => b.FechaHoraUtc)
                    .Take(10)
                    .ToListAsync();

                var vm = new AdminDashboardViewModel
                {
                    TotalProductos         = productos.Count,
                    ProductosActivos       = productos.Count(p => p.EsActivo),
                    ProductosConBajoStock  = productos.Count(p => p.Stock <= 5 && p.EsActivo),
                    TotalCategorias        = categorias.Count,
                    CategoriasActivas      = categorias.Count(c => c.EsActiva),

                    // TODO: usar IPedidoServicio cuando esté disponible
                    TotalPedidos  = 0,
                    PedidosHoy    = 0,
                    VentasTotales = 0,

                    ActividadReciente = actividad.Select(b => new BitacoraResumenViewModel
                    {
                        NombreUsuario  = b.NombreUsuario ?? "Sistema",
                        Accion         = b.Accion.ToString(),
                        TablaAfectada  = b.TablaAfectada,
                        FechaHoraUtc   = b.FechaHoraUtc,
                        Detalle        = b.Detalle
                    }).ToList()
                };

                return View(vm);
            }
        }
    }
}
