using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.AccesoADatos.Repositorios;
using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using HappyTimesBalloons.LogicaNegocio.Servicios;
using HappyTimesBalloons.Web.Helpers;
using HappyTimesBalloons.Web.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class CategoriaController : Controller
    {
        // GET /Categoria
        [HttpGet]
        public async Task<ActionResult> Index(string busqueda)
        {
            using (var ctx = new ApplicationDbContext())
            {
                var servicio = new CategoriaServicio(new CategoriaRepositorio(ctx));
                var categorias = await servicio.ObtenerTodasAsync(busqueda);

                var vm = new CategoriaIndexViewModel
                {
                    Categorias = categorias.Select(c => new CategoriaViewModel
                    {
                        Id = c.Id,
                        Nombre = c.Nombre,
                        Descripcion = c.Descripcion,
                        EsActiva = c.EsActiva,
                        FechaCreacion = c.FechaCreacion
                    }).ToList(),
                    Busqueda = busqueda
                };

                return View(vm);
            }
        }

        // POST /Categoria/Crear — HU-CAT-002
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Crear(CategoriaFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Revisa los datos ingresados.";
                return RedirectToAction("Index");
            }

            using (var ctx = new ApplicationDbContext())
            {
                var servicio = new CategoriaServicio(new CategoriaRepositorio(ctx));
                var resultado = await servicio.CrearAsync(new CategoriaDTO
                {
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion
                });

                if (resultado.Exito)
                {
                    await AuditoriaHelper.RegistrarAsync(
                        HttpContext, TipoOperacion.Crear, "Categorias", detalle: model.Nombre);
                    TempData["Exito"] = resultado.Mensaje;
                }
                else
                {
                    TempData["Error"] = resultado.Mensaje;
                }
            }

            return RedirectToAction("Index");
        }

        // POST /Categoria/Editar — HU-CAT-003
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Editar(CategoriaFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Revisa los datos ingresados.";
                return RedirectToAction("Index");
            }

            using (var ctx = new ApplicationDbContext())
            {
                var servicio = new CategoriaServicio(new CategoriaRepositorio(ctx));
                var resultado = await servicio.ActualizarAsync(new CategoriaDTO
                {
                    Id = model.Id,
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion
                });

                if (resultado.Exito)
                {
                    await AuditoriaHelper.RegistrarAsync(
                        HttpContext, TipoOperacion.Actualizar, "Categorias", model.Id, model.Nombre);
                    TempData["Exito"] = resultado.Mensaje;
                }
                else
                {
                    TempData["Error"] = resultado.Mensaje;
                }
            }

            return RedirectToAction("Index");
        }

        // POST /Categoria/ToggleEstado/5 — HU-CAT-004
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ToggleEstado(int id)
        {
            using (var ctx = new ApplicationDbContext())
            {
                var servicio = new CategoriaServicio(new CategoriaRepositorio(ctx));
                var resultado = await servicio.ToggleEstadoAsync(id);

                if (resultado.Exito)
                {
                    await AuditoriaHelper.RegistrarAsync(
                        HttpContext, TipoOperacion.Actualizar, "Categorias", id, "ToggleEstado");
                    TempData["Exito"] = resultado.Mensaje;
                }
                else
                {
                    TempData["Error"] = resultado.Mensaje;
                }
            }

            return RedirectToAction("Index");
        }
    }
}
