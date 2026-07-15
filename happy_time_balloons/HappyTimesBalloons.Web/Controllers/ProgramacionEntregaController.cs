using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using HappyTimesBalloons.Web.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Controllers
{
    [Authorize(Roles = "Administrador,Operador")]
    public class ProgramacionEntregaController : Controller
    {
        private readonly IProgramacionEntregaServicio _servicio;
        private readonly IPedidoServicio _pedidoServicio;

        public ProgramacionEntregaController(
            IProgramacionEntregaServicio servicio,
            IPedidoServicio pedidoServicio)
        {
            _servicio = servicio;
            _pedidoServicio = pedidoServicio;
        }

        // Tarea 2: listado principal de programaciones
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var programaciones = await _servicio.ObtenerTodasAsync();

            var modelo = new ProgramacionEntregaIndexViewModel
            {
                Programaciones = programaciones
            };

            return View(modelo);
        }

        // Tarea 2 + 4: formulario de programación; la fecha y horarios se cargan aquí
        [HttpGet]
        public async Task<ActionResult> Programar(int pedidoId)
        {
            var pedido = await _pedidoServicio.ObtenerPorIdAsync(pedidoId);
            if (pedido == null)
            {
                return HttpNotFound();
            }

            var fechaMinima = DateTime.Today.AddDays(1);
            var fechaDefault = fechaMinima.DayOfWeek == DayOfWeek.Sunday
                ? fechaMinima.AddDays(1)
                : fechaMinima;

            var horariosDisponibles = await _servicio.ObtenerHorariosDisponiblesAsync(fechaDefault);

            var modelo = new ProgramacionEntregaFormViewModel
            {
                PedidoId = pedidoId,
                NumeroPedido = pedido.Numero,
                FechaEntrega = fechaDefault,
                FechaMinima = fechaMinima,
                FechaMaxima = DateTime.Today.AddDays(30),
                HorariosDisponibles = horariosDisponibles
            };

            return View(modelo);
        }

        // Tarea 2: endpoint AJAX para recargar horarios al cambiar la fecha
        [HttpGet]
        public async Task<ActionResult> HorariosDisponibles(string fecha)
        {
            if (!DateTime.TryParse(fecha, out var fechaParsed))
            {
                return Json(new { error = "Fecha inválida." }, JsonRequestBehavior.AllowGet);
            }

            if (!_servicio.EsFechaPermitida(fechaParsed, out var mensajeError))
            {
                return Json(new { error = mensajeError }, JsonRequestBehavior.AllowGet);
            }

            var horarios = await _servicio.ObtenerHorariosDisponiblesAsync(fechaParsed);

            var resultado = horarios.Select(h => new
            {
                horarioEntregaId = h.HorarioEntregaId,
                etiqueta = h.Etiqueta,
                horaInicio = h.HoraInicio,
                horaFin = h.HoraFin,
                cuposRestantes = h.CuposRestantes,
                tieneCupo = h.TieneCupo
            });

            return Json(resultado, JsonRequestBehavior.AllowGet);
        }

        // Tarea 3: registrar la programación de entrega
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Programar(ProgramacionEntregaFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.FechaMinima = DateTime.Today.AddDays(1);
                model.FechaMaxima = DateTime.Today.AddDays(30);
                model.HorariosDisponibles = await _servicio.ObtenerHorariosDisponiblesAsync(model.FechaEntrega);
                return View(model);
            }

            var dto = new ProgramacionEntregaDTO
            {
                PedidoId = model.PedidoId,
                HorarioEntregaId = model.HorarioEntregaId,
                FechaEntrega = model.FechaEntrega,
                Notas = model.Notas
            };

            var usuarioId = User.Identity.GetUserId();
            var nombreUsuario = User.Identity.Name;

            var resultado = await _servicio.RegistrarAsync(dto, usuarioId, nombreUsuario);

            if (resultado.Exito)
            {
                TempData["Exito"] = resultado.Mensaje;
                return RedirectToAction("Index");
            }

            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            model.FechaMinima = DateTime.Today.AddDays(1);
            model.FechaMaxima = DateTime.Today.AddDays(30);
            model.HorariosDisponibles = await _servicio.ObtenerHorariosDisponiblesAsync(model.FechaEntrega);
            return View(model);
        }

        // Tarea 5: cancelar libera el cupo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Cancelar(int id)
        {
            var usuarioId = User.Identity.GetUserId();
            var nombreUsuario = User.Identity.Name;

            var resultado = await _servicio.CancelarAsync(id, usuarioId, nombreUsuario);

            if (resultado.Exito)
            {
                TempData["Exito"] = resultado.Mensaje;
            }
            else
            {
                TempData["Error"] = resultado.Mensaje;
            }

            return RedirectToAction("Index");
        }
    }
}
