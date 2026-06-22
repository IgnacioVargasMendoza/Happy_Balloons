using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using HappyTimesBalloons.Web.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Controllers
{
    [Authorize(Roles = "Administrador,Operador")]
    public class InventarioController : Controller
    {
        private readonly IInventarioServicio _inventarioServicio;
        private readonly ICategoriaServicio _categoriaServicio;

        public InventarioController(
            IInventarioServicio inventarioServicio,
            ICategoriaServicio categoriaServicio)
        {
            _inventarioServicio = inventarioServicio;
            _categoriaServicio = categoriaServicio;
        }

        [HttpGet]
        public async Task<ActionResult> Index(string busqueda, int? categoriaId, string estadoStock)
        {
            if (string.IsNullOrEmpty(estadoStock))
                estadoStock = "todos";

            var items = await _inventarioServicio.ObtenerTodosAsync(busqueda, categoriaId, estadoStock);
            var kpis = await _inventarioServicio.ObtenerKpisAsync();
            var categorias = (await _categoriaServicio.ObtenerTodasAsync(soloActivas: true))
                .OrderBy(c => c.Nombre).ToList();

            var vm = new InventarioIndexViewModel
            {
                Busqueda = busqueda,
                CategoriaId = categoriaId,
                EstadoStock = estadoStock,
                Categorias = new SelectList(categorias, "Id", "Nombre", categoriaId),
                Kpis = new InventarioKpisViewModel
                {
                    TotalProductos = kpis.TotalProductos,
                    ProductosStockBajo = kpis.ProductosStockBajo,
                    ProductosSinStock = kpis.ProductosSinStock,
                    ValorTotalInventario = kpis.ValorTotalInventario,
                    AlertasReabastecimiento = kpis.AlertasReabastecimiento
                        .Select(MapearItem)
                        .ToList()
                },
                Items = items.Select(MapearItem).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditarStockMinimo(int id, int stockMinimo)
        {
            var resultado = await _inventarioServicio.ActualizarStockMinimoAsync(
                id, stockMinimo, User.Identity.GetUserId(), User.Identity.Name);

            if (!resultado.Exito)
                TempData["Error"] = resultado.Mensaje;
            else
                TempData["Exito"] = resultado.Mensaje;

            return RedirectToAction("Index");
        }

        private static InventarioItemViewModel MapearItem(
            HappyTimesBalloons.Abstraccion.DTOs.InventarioDTO dto)
        {
            return new InventarioItemViewModel
            {
                Id = dto.Id,
                ProductoId = dto.ProductoId,
                ProductoNombre = dto.ProductoNombre,
                CategoriaNombre = dto.CategoriaNombre,
                StockActual = dto.StockActual,
                StockMinimo = dto.StockMinimo,
                FechaUltimaActualizacion = dto.FechaUltimaActualizacion
            };
        }
    }
}
