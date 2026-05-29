using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using HappyTimesBalloons.Web.Infrastructure;
using HappyTimesBalloons.Web.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Controllers
{
    [AutorizarRol(Roles = "Administrador")]
    public class RolController : Controller
    {
        private readonly IRolServicio _rolServicio;

        public RolController(IRolServicio rolServicio)
        {
            _rolServicio = rolServicio;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var roles = await _rolServicio.ObtenerTodosAsync();
            var vm = new RolIndexViewModel
            {
                Roles = roles.Select(r => new RolItemViewModel
                {
                    Id = r.Id,
                    Nombre = r.Nombre,
                    CantidadUsuarios = r.CantidadUsuarios
                }).ToList()
            };
            return View(vm);
        }

        [HttpGet]
        public ActionResult Crear()
        {
            return View(new RolFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Crear(RolFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var adminId = User.Identity.GetUserId();
            var adminNombre = User.Identity.Name;
            var ip = Request.UserHostAddress;

            var resultado = await _rolServicio.CrearAsync(model.Nombre, adminId, adminNombre, ip);

            if (!resultado.Exito)
            {
                ModelState.AddModelError("", resultado.Mensaje);
                return View(model);
            }

            TempData["Mensaje"] = resultado.Mensaje;
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> Editar(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction("Index");

            var rol = await _rolServicio.ObtenerPorIdAsync(id);
            if (rol == null)
                return HttpNotFound();

            return View(new RolFormViewModel { Id = rol.Id, Nombre = rol.Nombre });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Editar(RolFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var adminId = User.Identity.GetUserId();
            var adminNombre = User.Identity.Name;
            var ip = Request.UserHostAddress;

            var resultado = await _rolServicio.ActualizarAsync(model.Id, model.Nombre, adminId, adminNombre, ip);

            if (!resultado.Exito)
            {
                ModelState.AddModelError("", resultado.Mensaje);
                return View(model);
            }

            TempData["Mensaje"] = resultado.Mensaje;
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Eliminar(string id)
        {
            var adminId = User.Identity.GetUserId();
            var adminNombre = User.Identity.Name;
            var ip = Request.UserHostAddress;

            var resultado = await _rolServicio.EliminarAsync(id, adminId, adminNombre, ip);

            TempData[resultado.Exito ? "Mensaje" : "Error"] = resultado.Mensaje;
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> Usuarios()
        {
            var usuarios = await _rolServicio.ObtenerTodosLosUsuariosAsync();
            var roles = await _rolServicio.ObtenerTodosAsync();

            var vm = new UsuariosRolViewModel
            {
                Usuarios = usuarios.Select(u => new UsuarioRolItemViewModel
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Email = u.Email,
                    Roles = u.Roles.ToList()
                }).ToList(),
                RolesDisponibles = roles.Select(r => new RolItemViewModel
                {
                    Id = r.Id,
                    Nombre = r.Nombre,
                    CantidadUsuarios = r.CantidadUsuarios
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AsignarRol(string usuarioId, string rolNombre)
        {
            var adminId = User.Identity.GetUserId();
            var adminNombre = User.Identity.Name;
            var ip = Request.UserHostAddress;

            var resultado = await _rolServicio.AsignarRolAsync(usuarioId, rolNombre, adminId, adminNombre, ip);

            TempData[resultado.Exito ? "Mensaje" : "Error"] = resultado.Mensaje;
            return RedirectToAction("Usuarios");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RevocarRol(string usuarioId, string rolNombre)
        {
            var adminId = User.Identity.GetUserId();
            var adminNombre = User.Identity.Name;
            var ip = Request.UserHostAddress;

            var resultado = await _rolServicio.RevocarRolAsync(usuarioId, rolNombre, adminId, adminNombre, ip);

            TempData[resultado.Exito ? "Mensaje" : "Error"] = resultado.Mensaje;
            return RedirectToAction("Usuarios");
        }
    }
}
