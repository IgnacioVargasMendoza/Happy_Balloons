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
        public ActionResult Index(string busqueda = "", int? categoriaId = null, string orden = "")
        {
            using (var ctx = new ApplicationDbContext())
            {
                var repositorio = new CatalogoProductoRepositorio(ctx);

                ICatalogoProductoServicio servicio =
                    new CatalogoProductoServicio(repositorio);

                var catalogo = servicio.ObtenerCatalogo(busqueda, categoriaId, orden);

                ViewBag.Categorias = servicio.ObtenerCategorias();
                ViewBag.Orden = orden;

                return View(catalogo);
            }
        }
        [HttpGet]
        public ActionResult Detalle(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("Index");
            }

            using (var ctx = new ApplicationDbContext())
            {
                var repositorio = new CatalogoProductoRepositorio(ctx);

                ICatalogoProductoServicio servicio =
                    new CatalogoProductoServicio(repositorio);

                var producto = servicio.ObtenerProductoCatalogoPorId(id.Value);

                if (producto == null)
                {
                    return HttpNotFound();
                }

                return View(producto);
            }
        }
    }

}