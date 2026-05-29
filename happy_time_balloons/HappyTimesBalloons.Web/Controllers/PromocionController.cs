using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using HappyTimesBalloons.Web.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class PromocionController : Controller
    {
        private readonly IPromocionServicio _promoServicio;
        private readonly IProductoServicio _productoServicio;

        public PromocionController(IPromocionServicio promoServicio, IProductoServicio productoServicio)
        {
            _promoServicio = promoServicio;
            _productoServicio = productoServicio;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var vm = await ConstruirVmAsync();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Crear(PromocionFormViewModel form)
        {
            if (!ModelState.IsValid)
            {
                var vm = await ConstruirVmAsync();
                form.Productos = vm.Formulario.Productos;
                vm.Formulario = form;
                return View("Index", vm);
            }

            var dto = new PromocionDTO
            {
                ProductoId = form.ProductoId,
                DescuentoPorcentaje = form.DescuentoPorcentaje,
                FechaInicio = form.FechaInicio,
                FechaFin = form.FechaFin
            };

            var resultado = await _promoServicio.CrearAsync(dto);

            if (resultado.Exito)
                TempData["Exito"] = resultado.Mensaje;
            else
                TempData["Error"] = resultado.Mensaje;

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Eliminar(int id)
        {
            var resultado = await _promoServicio.EliminarAsync(id);

            if (resultado.Exito)
                TempData["Exito"] = resultado.Mensaje;
            else
                TempData["Error"] = resultado.Mensaje;

            return RedirectToAction("Index");
        }

        private async Task<AdminPromocionesViewModel> ConstruirVmAsync()
        {
            var promociones = await _promoServicio.ObtenerTodasAsync();
            var productos = await _productoServicio.ObtenerTodosAsync(soloActivos: true);

            var productosItems = productos
                .OrderBy(p => p.Nombre)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Nombre
                });

            return new AdminPromocionesViewModel
            {
                Promociones = promociones.Select(p => new PromocionViewModel
                {
                    Id = p.Id,
                    ProductoId = p.ProductoId,
                    ProductoNombre = p.ProductoNombre,
                    DescuentoPorcentaje = p.DescuentoPorcentaje,
                    FechaInicio = p.FechaInicio,
                    FechaFin = p.FechaFin,
                    Activa = p.Activa
                }).ToList(),
                Formulario = new PromocionFormViewModel
                {
                    Productos = productosItems
                }
            };
        }
    }
}
