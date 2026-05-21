using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.AccesoADatos.Repositorios;
using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.LogicaNegocio.Servicios;
using HappyTimesBalloons.Web.Models.ViewModels;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public async Task<ActionResult> Index(string busqueda, int? categoriaId)
        {
            using (var ctx = new ApplicationDbContext())
            {
                var servicio = new ProductoServicio(new ProductoRepositorio(ctx));
                var dtos = await servicio.ObtenerTodosAsync(busqueda, categoriaId);

                var categorias = await ctx.Categorias
                    .Where(c => c.EsActiva)
                    .OrderBy(c => c.Nombre)
                    .ToListAsync();

                var vm = new CatalogoIndexViewModel
                {
                    Busqueda = busqueda,
                    CategoriaId = categoriaId,
                    Categorias = new SelectList(categorias, "Id", "Nombre", categoriaId),
                    Productos = dtos
                        .Where(p => p.EsActivo)
                        .Select(p => MapearProducto(p))
                        .ToList()
                };

                return View(vm);
            }
        }

        [HttpGet]
        public async Task<ActionResult> Detalle(int id)
        {
            using (var ctx = new ApplicationDbContext())
            {
                var servicio = new ProductoServicio(new ProductoRepositorio(ctx));
                var dto = await servicio.ObtenerPorIdAsync(id);

                if (dto == null || !dto.EsActivo)
                    return HttpNotFound();

                var relacionados = await servicio.ObtenerTodosAsync(null, dto.CategoriaId);

                var vm = new ProductoDetalleViewModel
                {
                    Producto = MapearProducto(dto),
                    ProductosRelacionados = relacionados
                        .Where(p => p.EsActivo && p.Id != id)
                        .Take(4)
                        .Select(p => MapearProducto(p))
                        .ToList()
                };

                return View(vm);
            }
        }

        private static ProductoViewModel MapearProducto(ProductoDTO p)
        {
            return new ProductoViewModel
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Precio = p.Precio,
                PrecioDescuento = p.PrecioDescuento,
                Stock = p.Stock,
                CategoriaId = p.CategoriaId,
                CategoriaNombre = p.CategoriaNombre,
                EsActivo = p.EsActivo,
                FechaCreacion = p.FechaCreacion,
                Imagenes = p.Imagenes.Select(i => new ImagenProductoViewModel
                {
                    Id = i.Id,
                    RutaImagen = i.RutaImagen,
                    EsPrincipal = i.EsPrincipal,
                    Orden = i.Orden
                }).ToList()
            };
        }
    }
}
