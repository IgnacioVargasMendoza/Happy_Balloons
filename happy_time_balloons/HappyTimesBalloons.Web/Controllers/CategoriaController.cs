using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using HappyTimesBalloons.Web.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class CategoriaController : Controller
    {
        private readonly ICategoriaServicio _categoriaServicio;
        private readonly IAuditoriaServicio _auditoriaServicio;

        public CategoriaController(ICategoriaServicio categoriaServicio, IAuditoriaServicio auditoriaServicio)
        {
            _categoriaServicio = categoriaServicio;
            _auditoriaServicio = auditoriaServicio;
        }

        [HttpGet]
        public async Task<ActionResult> Index(string busqueda)
        {
            var categorias = await _categoriaServicio.ObtenerTodasAsync(busqueda);

            var modelo = new CategoriaIndexViewModel
            {
                Busqueda = busqueda,
                Categorias = categorias.Select(c => new CategoriaViewModel
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    EsActiva = c.EsActiva,
                    FechaCreacion = c.FechaCreacion
                }).ToList()
            };

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Crear(CategoriaFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Revisa los datos ingresados.";
                return RedirectToAction("Index");
            }

            var resultado = await _categoriaServicio.CrearAsync(new CategoriaDTO
            {
                Nombre = model.Nombre,
                Descripcion = model.Descripcion
            });

            if (resultado.Exito)
            {
                await _auditoriaServicio.RegistrarAsync(
                    User.Identity.Name, User.Identity.Name,
                    TipoOperacion.Crear, "Categorias",
                    detalle: model.Nombre, ip: Request.UserHostAddress);
                TempData["Exito"] = resultado.Mensaje;
            }
            else
            {
                TempData["Error"] = resultado.Mensaje;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Editar(CategoriaFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Revisa los datos ingresados.";
                return RedirectToAction("Index");
            }

            var resultado = await _categoriaServicio.ActualizarAsync(new CategoriaDTO
            {
                Id = model.Id,
                Nombre = model.Nombre,
                Descripcion = model.Descripcion
            });

            if (resultado.Exito)
            {
                await _auditoriaServicio.RegistrarAsync(
                    User.Identity.Name, User.Identity.Name,
                    TipoOperacion.Actualizar, "Categorias",
                    registroId: model.Id, detalle: model.Nombre, ip: Request.UserHostAddress);
                TempData["Exito"] = resultado.Mensaje;
            }
            else
            {
                TempData["Error"] = resultado.Mensaje;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ToggleEstado(int id)
        {
            var resultado = await _categoriaServicio.ToggleEstadoAsync(id);

            if (resultado.Exito)
            {
                await _auditoriaServicio.RegistrarAsync(
                    User.Identity.Name, User.Identity.Name,
                    TipoOperacion.Actualizar, "Categorias",
                    registroId: id, detalle: "Cambio de estado", ip: Request.UserHostAddress);
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
