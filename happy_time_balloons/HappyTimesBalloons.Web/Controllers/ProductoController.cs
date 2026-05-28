using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using HappyTimesBalloons.Web.Helpers;
using HappyTimesBalloons.Web.Models.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Controllers
{
    [Authorize(Roles = "Administrador,Operador")]
    public class ProductoController : Controller
    {
        private static readonly string[] _extensionesPermitidas =
            { ".jpg", ".jpeg", ".png", ".webp" };
        private const int _maxTamanoBytes = 5 * 1024 * 1024;

        private readonly IProductoServicio _productoServicio;
        private readonly ICategoriaServicio _categoriaServicio;

        public ProductoController(
            IProductoServicio productoServicio,
            ICategoriaServicio categoriaServicio)
        {
            _productoServicio  = productoServicio;
            _categoriaServicio = categoriaServicio;
        }

        [HttpGet]
        public async Task<ActionResult> Index(string busqueda, int? categoriaId)
        {
            var productos = await _productoServicio.ObtenerTodosAsync(busqueda, categoriaId);
            var categorias = (await _categoriaServicio.ObtenerTodasAsync())
                .Where(c => c.EsActiva).OrderBy(c => c.Nombre).ToList();

            var vm = new ProductoIndexViewModel
            {
                Busqueda = busqueda,
                CategoriaId = categoriaId,
                Categorias = new SelectList(categorias, "Id", "Nombre", categoriaId),
                Productos = productos.Select(p => new ProductoViewModel
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    PrecioDescuento = p.PrecioDescuento,
                    Stock = p.Stock,
                    CategoriaId = p.CategoriaId,
                    CategoriaNombre = p.CategoriaNombre,
                    EsActivo = p.EsActivo,
                    FechaCreacion = p.FechaCreacion,
                    Imagenes = p.Imagenes.Select(i => new ImagenProductoViewModel
                    {
                        Id = i.Id,
                        RutaImagen = i.RutaImagen,
                        EsPrincipal = i.EsPrincipal,
                        Orden = i.Orden
                    }).ToList()
                }).ToList()
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<ActionResult> Crear()
        {
            return View(await ConstruirFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Crear(ProductoFormViewModel model, HttpPostedFileBase[] imagenes)
        {
            if (!ModelState.IsValid)
                return View(await ConstruirFormViewModel(model));

            var dto = new ProductoDTO
            {
                Nombre = model.Nombre,
                Descripcion = model.Descripcion,
                Precio = model.Precio,
                PrecioDescuento = model.PrecioDescuento,
                Stock = model.Stock,
                CategoriaId = model.CategoriaId
            };

            var resultado = await _productoServicio.CrearAsync(dto);
            if (!resultado.Exito)
            {
                TempData["Error"] = resultado.Mensaje;
                return View(await ConstruirFormViewModel(model));
            }

            await ProcesarImagenes(dto.Id, imagenes);
            await AuditoriaHelper.RegistrarAsync(
                HttpContext, TipoOperacion.Crear, "Productos", dto.Id, model.Nombre);
            TempData["Exito"] = resultado.Mensaje;

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> Editar(int id)
        {
            var producto = await _productoServicio.ObtenerPorIdAsync(id);
            if (producto == null) return HttpNotFound();

            var vm = await ConstruirFormViewModel();
            vm.Id = producto.Id;
            vm.Nombre = producto.Nombre;
            vm.Descripcion = producto.Descripcion;
            vm.Precio = producto.Precio;
            vm.PrecioDescuento = producto.PrecioDescuento;
            vm.Stock = producto.Stock;
            vm.CategoriaId = producto.CategoriaId;
            vm.EsActivo = producto.EsActivo;
            vm.ImagenesExistentes = producto.Imagenes.Select(i => new ImagenProductoViewModel
            {
                Id = i.Id,
                RutaImagen = i.RutaImagen,
                EsPrincipal = i.EsPrincipal,
                Orden = i.Orden
            }).ToList();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Editar(ProductoFormViewModel model, HttpPostedFileBase[] imagenes)
        {
            if (!ModelState.IsValid)
                return View(await ConstruirFormViewModel(model));

            var resultado = await _productoServicio.ActualizarAsync(new ProductoDTO
            {
                Id = model.Id,
                Nombre = model.Nombre,
                Descripcion = model.Descripcion,
                Precio = model.Precio,
                PrecioDescuento = model.PrecioDescuento,
                Stock = model.Stock,
                CategoriaId = model.CategoriaId
            });

            if (!resultado.Exito)
            {
                TempData["Error"] = resultado.Mensaje;
                return View(await ConstruirFormViewModel(model));
            }

            await ProcesarImagenes(model.Id, imagenes);
            await AuditoriaHelper.RegistrarAsync(
                HttpContext, TipoOperacion.Actualizar, "Productos", model.Id, model.Nombre);
            TempData["Exito"] = resultado.Mensaje;

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ToggleEstado(int id)
        {
            var resultado = await _productoServicio.ToggleEstadoAsync(id);

            if (resultado.Exito)
            {
                await AuditoriaHelper.RegistrarAsync(
                    HttpContext, TipoOperacion.Actualizar, "Productos", id, "ToggleEstado");
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
        public async Task<ActionResult> EliminarImagen(int imagenId, int productoId)
        {
            var imagen = await _productoServicio.ObtenerImagenPorIdAsync(imagenId);
            if (imagen != null)
            {
                string rutaFisica = Server.MapPath(imagen.RutaImagen);
                if (System.IO.File.Exists(rutaFisica))
                    System.IO.File.Delete(rutaFisica);
            }

            await _productoServicio.EliminarImagenAsync(imagenId);
            await AuditoriaHelper.RegistrarAsync(
                HttpContext, TipoOperacion.Eliminar, "ImagenesProducto", imagenId);

            TempData["Exito"] = "Imagen eliminada.";
            return RedirectToAction("Editar", new { id = productoId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EstablecerPrincipal(int imagenId, int productoId)
        {
            await _productoServicio.EstablecerImagenPrincipalAsync(imagenId, productoId);
            TempData["Exito"] = "Imagen principal actualizada.";
            return RedirectToAction("Editar", new { id = productoId });
        }

        private async Task<ProductoFormViewModel> ConstruirFormViewModel(
            ProductoFormViewModel modelo = null)
        {
            var categorias = (await _categoriaServicio.ObtenerTodasAsync())
                .Where(c => c.EsActiva).OrderBy(c => c.Nombre).ToList();

            if (modelo == null)
                modelo = new ProductoFormViewModel();

            modelo.Categorias = new SelectList(categorias, "Id", "Nombre", modelo.CategoriaId);
            return modelo;
        }

        private async Task ProcesarImagenes(int productoId, HttpPostedFileBase[] archivos)
        {
            if (archivos == null) return;

            string carpeta = Server.MapPath("~/Content/Uploads/Productos/");
            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

            foreach (var archivo in archivos)
            {
                if (archivo == null || archivo.ContentLength == 0) continue;

                string ext = Path.GetExtension(archivo.FileName)?.ToLower();
                if (!_extensionesPermitidas.Contains(ext)) continue;
                if (archivo.ContentLength > _maxTamanoBytes) continue;

                string nombreArchivo = $"p{productoId}_{Guid.NewGuid():N}{ext}";
                string rutaFisica = Path.Combine(carpeta, nombreArchivo);
                archivo.SaveAs(rutaFisica);

                await _productoServicio.AgregarImagenAsync(new ImagenProductoDTO
                {
                    ProductoId = productoId,
                    RutaImagen = $"/Content/Uploads/Productos/{nombreArchivo}",
                    EsPrincipal = false
                });
            }
        }
    }
}
