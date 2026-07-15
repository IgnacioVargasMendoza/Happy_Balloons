using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using HappyTimesBalloons.Web.Models.ViewModels;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Controllers
{
    public class SinpeController : Controller
    {
        private readonly ISinpeServicio _sinpeServicio;
        private readonly IAuditoriaServicio _auditoriaServicio;

        public SinpeController(ISinpeServicio sinpeServicio, IAuditoriaServicio auditoriaServicio)
        {
            _sinpeServicio = sinpeServicio;
            _auditoriaServicio = auditoriaServicio;
        }

        [HttpPost]
        public async Task<JsonResult> RecibirWebhook(SinpeWebhookDTO dto)
        {
            var ip = Request.UserHostAddress;
            var resultado = await _sinpeServicio.ProcesarWebhookAsync(dto, ip);
            return Json(new { ok = resultado.Exito, mensaje = resultado.Mensaje });
        }

        [Authorize(Roles = "Administrador,Operador")]
        public async Task<ActionResult> Index()
        {
            var pagos = await _sinpeServicio.ObtenerPagosAsync();
            var vm = new SinpeViewModel { Pagos = pagos };
            return View(vm);
        }

        [Authorize(Roles = "Administrador,Operador")]
        public async Task<ActionResult> Detalle(int id)
        {
            var pago = await _sinpeServicio.ObtenerPorIdAsync(id);
            if (pago == null)
            {
                return HttpNotFound();
            }

            var vm = new SinpeDetalleViewModel { Pago = pago };
            return PartialView("_DetallePago", vm);
        }
    }
}
