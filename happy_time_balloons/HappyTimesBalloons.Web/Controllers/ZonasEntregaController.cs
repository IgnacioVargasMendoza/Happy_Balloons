using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using HappyTimesBalloons.Web.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ZonasEntregaController : Controller
    {
        private readonly IZonaEntregaServicio _zonaServicio;

        public ZonasEntregaController(IZonaEntregaServicio zonaServicio)
        {
            _zonaServicio = zonaServicio;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var zonas = await _zonaServicio.ObtenerTodasIncluyendoInactivasAsync();

            var modelo = new ZonasEntregaIndexViewModel
            {
                Zonas = zonas.Select(z => new ZonaEntregaViewModel
                {
                    Id = z.Id,
                    Nombre = z.Nombre,
                    Descripcion = z.Descripcion,
                    CostoEnvio = z.CostoEnvio,
                    EsDisponible = z.EsDisponible,
                    FechaCreacion = z.FechaCreacion
                }).ToList()
            };

            return View(modelo);
        }

        [HttpGet]
        public ActionResult Crear()
        {
            return View(new ZonaEntregaViewModel { EsDisponible = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Crear(ZonaEntregaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var dto = new ZonaEntregaDTO
            {
                Nombre = model.Nombre,
                Descripcion = model.Descripcion,
                CostoEnvio = model.CostoEnvio,
                EsDisponible = model.EsDisponible
            };

            var usuarioId = User.Identity.GetUserId();
            var nombreUsuario = User.Identity.Name;

            var resultado = await _zonaServicio.CrearAsync(dto, usuarioId, nombreUsuario);

            if (resultado.Exito)
            {
                TempData["Exito"] = resultado.Mensaje;
                return RedirectToAction("Index");
            }

            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Editar(int id)
        {
            var dto = await _zonaServicio.ObtenerPorIdAsync(id);
            if (dto == null)
            {
                return HttpNotFound();
            }

            var modelo = new ZonaEntregaViewModel
            {
                Id = dto.Id,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                CostoEnvio = dto.CostoEnvio,
                EsDisponible = dto.EsDisponible,
                FechaCreacion = dto.FechaCreacion
            };

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Editar(ZonaEntregaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var dto = new ZonaEntregaDTO
            {
                Id = model.Id,
                Nombre = model.Nombre,
                Descripcion = model.Descripcion,
                CostoEnvio = model.CostoEnvio,
                EsDisponible = model.EsDisponible
            };

            var usuarioId = User.Identity.GetUserId();
            var nombreUsuario = User.Identity.Name;

            var resultado = await _zonaServicio.ActualizarAsync(dto, usuarioId, nombreUsuario);

            if (resultado.Exito)
            {
                TempData["Exito"] = resultado.Mensaje;
                return RedirectToAction("Index");
            }

            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CambiarDisponibilidad(int id, bool esDisponible)
        {
            var usuarioId = User.Identity.GetUserId();
            var nombreUsuario = User.Identity.Name;

            var resultado = await _zonaServicio.CambiarDisponibilidadAsync(id, esDisponible, usuarioId, nombreUsuario);

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
