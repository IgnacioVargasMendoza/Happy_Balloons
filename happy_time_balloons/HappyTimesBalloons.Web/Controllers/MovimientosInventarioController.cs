using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using HappyTimesBalloons.Web.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Controllers
{
    [Authorize(Roles = "Administrador,Operador")]
    public class MovimientosInventarioController : Controller
    {
        private readonly IMovimientoInventarioServicio _movimientoServicio;
        private readonly IInventarioServicio _inventarioServicio;

        public MovimientosInventarioController(
            IMovimientoInventarioServicio movimientoServicio,
            IInventarioServicio inventarioServicio)
        {
            _movimientoServicio = movimientoServicio;
            _inventarioServicio = inventarioServicio;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Registrar(RegistrarMovimientoViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Por favor completa todos los campos requeridos.";
                return RedirectToAction("Index", "Inventario");
            }

            var dto = new MovimientoInventarioDTO
            {
                ProductoId = vm.ProductoId,
                TipoMovimiento = vm.TipoMovimiento,
                Cantidad = vm.Cantidad,
                Motivo = vm.Motivo
            };

            var resultado = await _movimientoServicio.RegistrarMovimientoAsync(dto, User.Identity.GetUserId());

            if (!resultado.Exito)
            {
                TempData["Error"] = resultado.Mensaje;
                return RedirectToAction("Index", "Inventario");
            }

            TempData["Exito"] = resultado.Mensaje;
            return RedirectToAction("Index", "Inventario");
        }

        [HttpGet]
        public async Task<ActionResult> Historial(int productoId)
        {
            var inventario = await _inventarioServicio.ObtenerPorProductoIdAsync(productoId);

            if (inventario == null)
            {
                TempData["Error"] = "No se encontró el producto en inventario.";
                return RedirectToAction("Index", "Inventario");
            }

            var movimientos = await _movimientoServicio.ObtenerHistorialPorProductoAsync(productoId);

            var vm = new MovimientoInventarioHistorialViewModel
            {
                ProductoId = productoId,
                ProductoNombre = inventario.ProductoNombre,
                StockActual = inventario.StockActual,
                Movimientos = movimientos.Select(m => new MovimientoInventarioItemViewModel
                {
                    Id = m.Id,
                    TipoMovimiento = m.TipoMovimiento,
                    Cantidad = m.Cantidad,
                    StockAnterior = m.StockAnterior,
                    StockNuevo = m.StockNuevo,
                    Motivo = m.Motivo,
                    UsuarioId = m.UsuarioId,
                    FechaMovimiento = m.FechaMovimiento
                }).ToList()
            };

            return View(vm);
        }
    }
}
