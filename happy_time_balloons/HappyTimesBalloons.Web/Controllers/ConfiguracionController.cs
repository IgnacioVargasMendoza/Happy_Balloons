using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using HappyTimesBalloons.Web.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Controllers
{
    [Authorize]
    public class ConfiguracionController : Controller
    {
        private readonly IAuthServicio _authServicio;

        public ConfiguracionController(IAuthServicio authServicio)
        {
            _authServicio = authServicio;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> EditarPerfil()
        {
            var usuarioId = User.Identity.GetUserId();
            var usuario = await _authServicio.ObtenerPorIdAsync(usuarioId);
            if (usuario == null)
                return RedirectToAction("Index");

            var vm = new EditarPerfilViewModel
            {
                Nombre = usuario.Nombre,
                Direccion = usuario.Direccion,
                Telefono = usuario.Telefono
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditarPerfil(EditarPerfilViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuarioId = User.Identity.GetUserId();
            var ip = Request.UserHostAddress;

            var resultado = await _authServicio.EditarPerfilAsync(
                new EditarPerfilDTO
                {
                    UsuarioId = usuarioId,
                    Nombre = model.Nombre,
                    Direccion = model.Direccion,
                    Telefono = model.Telefono
                },
                usuarioId, User.Identity.Name, ip);

            if (!resultado.Exito)
            {
                ModelState.AddModelError("", resultado.Mensaje);
                return View(model);
            }

            TempData["Mensaje"] = resultado.Mensaje;
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult CambiarContrasena()
        {
            return View(new CambiarContrasenaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CambiarContrasena(CambiarContrasenaViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuarioId = User.Identity.GetUserId();
            var ip = Request.UserHostAddress;

            var resultado = await _authServicio.CambiarContrasenaAsync(
                usuarioId, model.ContrasenaActual, model.NuevaContrasena, ip);

            if (!resultado.Exito)
            {
                ModelState.AddModelError("", resultado.Mensaje);
                return View(model);
            }

            TempData["Mensaje"] = resultado.Mensaje;
            return RedirectToAction("Index");
        }
    }
}
