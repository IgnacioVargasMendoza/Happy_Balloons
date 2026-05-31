using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.AccesoADatos.Repositorios;
using HappyTimesBalloons.LogicaNegocio.Servicios;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Controllers
{
    public class CatalogoController : Controller
    {
        [HttpGet]
        public ActionResult Index(string busqueda = "", int? categoriaId = null)
        {
            using (var ctx = new ApplicationDbContext())
            {
                var repositorio = new CatalogoProductoRepositorio(ctx);

                ICatalogoProductoServicio servicio =
                    new CatalogoProductoServicio(repositorio);

                var catalogo = servicio.ObtenerCatalogo(busqueda, categoriaId);

                ViewBag.Categorias = servicio.ObtenerCategorias();

                return View(catalogo);
            }
        }

        [HttpGet]
        public ActionResult Detalle(int id)
        {
            using (var ctx = new ApplicationDbContext())
            {
                var repositorio = new CatalogoProductoRepositorio(ctx);

                ICatalogoProductoServicio servicio =
                    new CatalogoProductoServicio(repositorio);

                var producto = servicio.ObtenerProductoCatalogoPorId(id);

                if (producto == null)
                {
                    return HttpNotFound();
                }

                return View(producto);
            }
        }
    }
}