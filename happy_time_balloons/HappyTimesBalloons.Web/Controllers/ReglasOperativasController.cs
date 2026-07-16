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
    public class ReglasOperativasController : Controller
    {
        private readonly IConfiguracionServicio _configuracionServicio;

        public ReglasOperativasController(IConfiguracionServicio configuracionServicio)
        {
            _configuracionServicio = configuracionServicio;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var configuraciones = await _configuracionServicio.ObtenerTodasAsync();

            var modelo = new ReglasOperativasIndexViewModel
            {
                Configuraciones = configuraciones.Select(c => new ConfiguracionItemViewModel
                {
                    Id = c.Id,
                    Clave = c.Clave,
                    Valor = c.Valor,
                    Descripcion = c.Descripcion,
                    FechaUltimaModificacion = c.FechaUltimaModificacion,
                    UsuarioUltimaModificacion = c.UsuarioUltimaModificacion
                }).ToList()
            };

            return View(modelo);
        }

        [HttpGet]
        public async Task<ActionResult> Editar(int id)
        {
            var dto = await _configuracionServicio.ObtenerPorIdAsync(id);

            if (dto == null)
            {
                return HttpNotFound();
            }

            var modelo = new ConfiguracionEditarViewModel
            {
                Id = dto.Id,
                Clave = dto.Clave,
                Valor = dto.Valor,
                Descripcion = dto.Descripcion,
                FechaUltimaModificacion = dto.FechaUltimaModificacion,
                UsuarioUltimaModificacion = dto.UsuarioUltimaModificacion
            };

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Editar(ConfiguracionEditarViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var dto = new ConfiguracionSistemaDTO
            {
                Id = model.Id,
                Clave = model.Clave,
                Valor = model.Valor
            };

            var usuarioId = User.Identity.GetUserId();
            var nombreUsuario = User.Identity.Name;

            var resultado = await _configuracionServicio.ActualizarAsync(dto, usuarioId, nombreUsuario);

            if (resultado.Exito)
            {
                TempData["Exito"] = resultado.Mensaje;
                return RedirectToAction("Index");
            }

            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(model);
        }
    }
}
