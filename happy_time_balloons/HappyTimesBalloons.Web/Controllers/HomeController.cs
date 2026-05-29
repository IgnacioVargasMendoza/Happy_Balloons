using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using HappyTimesBalloons.Web.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductoServicio _productoServicio;
        private readonly ICategoriaServicio _categoriaServicio;

        public HomeController(IProductoServicio productoServicio, ICategoriaServicio categoriaServicio)
        {
            _productoServicio = productoServicio;
            _categoriaServicio = categoriaServicio;
        }

        [HttpGet]
        public async Task<ActionResult> Index(string busqueda, int? categoriaId)
        {
            var dtos = await _productoServicio.ObtenerTodosAsync(busqueda, categoriaId);
            var categoriasDtos = await _categoriaServicio.ObtenerTodasAsync();

            var vm = new CatalogoIndexViewModel
            {
                Busqueda = busqueda,
                CategoriaId = categoriaId,
                Categorias = new SelectList(
                    categoriasDtos.Where(c => c.EsActiva).OrderBy(c => c.Nombre).ToList(),
                    "Id", "Nombre", categoriaId),
                Productos = dtos
                    .Where(p => p.EsActivo)
                    .Select(p => MapearProducto(p))
                    .ToList()
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<ActionResult> Detalle(int id)
        {
            var dto = await _productoServicio.ObtenerPorIdAsync(id);

            if (dto == null || !dto.EsActivo)
                return HttpNotFound();

            var relacionados = await _productoServicio.ObtenerTodosAsync(null, dto.CategoriaId);

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
                TienePromocion = p.TienePromocion,
                PromocionFin = p.PromocionFin,
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
