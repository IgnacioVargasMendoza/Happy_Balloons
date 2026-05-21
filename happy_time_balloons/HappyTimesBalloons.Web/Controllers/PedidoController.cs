using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.AccesoADatos.Repositorios;
using HappyTimesBalloons.LogicaNegocio.Servicios;
using HappyTimesBalloons.Web.Helpers;
using HappyTimesBalloons.Web.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Controllers
{
    public class PedidoController : Controller
    {
        private const string SessionCarrito = "Carrito";
        private const string SessionCarritoCount = "CarritoCount";

        // ═══════════════════════════════════════════════════════════════════
        // CARRITO
        // ═══════════════════════════════════════════════════════════════════

        // GET /Pedido/Carrito
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Carrito()
        {
            var vm = new CarritoViewModel { Items = ObtenerCarrito() };
            return View(vm);
        }

        // POST /Pedido/AgregarAlCarrito
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AgregarAlCarrito(int productoId, int cantidad)
        {
            if (cantidad < 1) cantidad = 1;

            using (var ctx = new ApplicationDbContext())
            {
                var servicio = new ProductoServicio(new ProductoRepositorio(ctx));
                var producto = await servicio.ObtenerPorIdAsync(productoId);

                if (producto == null || !producto.EsActivo)
                {
                    TempData["Error"] = "El producto no está disponible.";
                    return RedirectToAction("Carrito");
                }

                var carrito = ObtenerCarrito();
                var item = carrito.FirstOrDefault(i => i.ProductoId == productoId);

                if (item != null)
                {
                    item.Cantidad = System.Math.Min(item.Cantidad + cantidad, producto.Stock);
                }
                else
                {
                    string imagenUrl = producto.Imagenes
                        .Where(i => i.EsPrincipal).Select(i => i.RutaImagen).FirstOrDefault()
                        ?? producto.Imagenes.Select(i => i.RutaImagen).FirstOrDefault();

                    carrito.Add(new CarritoItemViewModel
                    {
                        ProductoId = producto.Id,
                        Nombre = producto.Nombre,
                        ImagenUrl = imagenUrl,
                        Precio = producto.Precio,
                        PrecioDescuento = producto.PrecioDescuento,
                        Cantidad = System.Math.Min(cantidad, producto.Stock)
                    });
                }

                GuardarCarrito(carrito);
            }

            TempData["Exito"] = "Producto agregado al carrito.";
            return RedirectToAction("Carrito");
        }

        // POST /Pedido/ActualizarCantidad
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ActualizarCantidad(int productoId, int cantidad)
        {
            if (cantidad < 1) cantidad = 1;

            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(i => i.ProductoId == productoId);
            if (item != null)
                item.Cantidad = cantidad;

            GuardarCarrito(carrito);
            return RedirectToAction("Carrito");
        }

        // POST /Pedido/QuitarDelCarrito
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult QuitarDelCarrito(int productoId)
        {
            var carrito = ObtenerCarrito();
            carrito.RemoveAll(i => i.ProductoId == productoId);
            GuardarCarrito(carrito);
            TempData["Exito"] = "Producto eliminado del carrito.";
            return RedirectToAction("Carrito");
        }

        // ═══════════════════════════════════════════════════════════════════
        // CHECKOUT
        // ═══════════════════════════════════════════════════════════════════

        // GET /Pedido/Checkout
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> Checkout()
        {
            var carrito = ObtenerCarrito();
            if (!carrito.Any())
            {
                TempData["Info"] = "Tu carrito está vacío. Agrega productos antes de continuar.";
                return RedirectToAction("Carrito");
            }

            using (var ctx = new ApplicationDbContext())
            {
                var zonaRepo = new ZonaEntregaRepositorio(ctx);
                var zonas = await zonaRepo.ObtenerTodasAsync();

                var vm = new CheckoutViewModel
                {
                    ItemsCarrito = carrito,
                    ZonasEntrega = zonas.Select(z => new ZonaEntregaViewModel
                    {
                        Id = z.Id,
                        Nombre = z.Nombre,
                        Descripcion = z.Descripcion,
                        CostoEnvio = z.CostoEnvio,
                        EsDisponible = z.EsDisponible
                    }).ToList()
                };

                // Pre-seleccionar primera zona
                if (vm.ZonasEntrega.Any())
                {
                    vm.ZonaEntregaId = vm.ZonasEntrega.First().Id;
                    vm.CostoEnvio = vm.ZonasEntrega.First().CostoEnvio;
                }

                return View(vm);
            }
        }

        // POST /Pedido/ConfirmarPedido
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmarPedido(CheckoutViewModel model)
        {
            var carrito = ObtenerCarrito();
            if (!carrito.Any())
            {
                TempData["Error"] = "El carrito está vacío.";
                return RedirectToAction("Carrito");
            }

            // Volver a cargar zonas si el modelo no es válido
            if (!ModelState.IsValid)
            {
                using (var ctx = new ApplicationDbContext())
                {
                    var zonaRepo = new ZonaEntregaRepositorio(ctx);
                    var zonas = await zonaRepo.ObtenerTodasAsync();
                    model.ItemsCarrito = carrito;
                    model.ZonasEntrega = zonas.Select(z => new ZonaEntregaViewModel
                    {
                        Id = z.Id,
                        Nombre = z.Nombre,
                        Descripcion = z.Descripcion,
                        CostoEnvio = z.CostoEnvio,
                        EsDisponible = z.EsDisponible
                    }).ToList();
                }
                return View("Checkout", model);
            }

            var userId = User.Identity.GetUserId();

            var checkout = new CheckoutDTO
            {
                ZonaEntregaId    = model.ZonaEntregaId,
                DireccionEntrega = model.DireccionEntrega,
                MetodoPago       = model.MetodoPago,
                NumeroReferencia = model.NumeroReferencia,
                Notas            = model.Notas,
                Items            = carrito.Select(i => new CheckoutItemDTO
                {
                    ProductoId = i.ProductoId,
                    Cantidad   = i.Cantidad
                }).ToList()
            };

            using (var ctx = new ApplicationDbContext())
            {
                var servicio = new PedidoServicio(
                    new PedidoRepositorio(ctx),
                    new ProductoRepositorio(ctx),
                    new ZonaEntregaRepositorio(ctx));

                var resultado = await servicio.CrearPedidoAsync(userId, checkout);

                if (!resultado.Exito)
                {
                    TempData["Error"] = resultado.Mensaje;
                    // Recargar zonas
                    var zonaRepo = new ZonaEntregaRepositorio(ctx);
                    var zonas = await zonaRepo.ObtenerTodasAsync();
                    model.ItemsCarrito = carrito;
                    model.ZonasEntrega = zonas.Select(z => new ZonaEntregaViewModel
                    {
                        Id = z.Id,
                        Nombre = z.Nombre,
                        Descripcion = z.Descripcion,
                        CostoEnvio = z.CostoEnvio,
                        EsDisponible = z.EsDisponible
                    }).ToList();
                    return View("Checkout", model);
                }

                // Auditoría
                await AuditoriaHelper.RegistrarAsync(
                    HttpContext, TipoOperacion.Crear, "Pedidos", resultado.Datos, $"Pedido creado por {userId}");

                // Limpiar carrito
                LimpiarCarrito();

                TempData["Exito"] = "¡Tu pedido fue confirmado exitosamente!";
                return RedirectToAction("MisPedidos");
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // MIS PEDIDOS (Cliente)
        // ═══════════════════════════════════════════════════════════════════

        // GET /Pedido/MisPedidos
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> MisPedidos()
        {
            var userId = User.Identity.GetUserId();

            using (var ctx = new ApplicationDbContext())
            {
                var servicio = new PedidoServicio(
                    new PedidoRepositorio(ctx),
                    new ProductoRepositorio(ctx),
                    new ZonaEntregaRepositorio(ctx));

                var pedidos = await servicio.ObtenerPorUsuarioAsync(userId);

                var vm = new MisPedidosViewModel
                {
                    Pedidos = pedidos.Select(p => new PedidoResumenViewModel
                    {
                        Id            = p.Id,
                        Numero        = p.Numero,
                        NombreUsuario = p.NombreUsuario,
                        FechaPedido   = p.FechaPedido,
                        EstadoPedido  = p.EstadoPedido,
                        Total         = p.Total,
                        CantidadItems = p.Detalles.Sum(d => d.Cantidad)
                    }).ToList()
                };

                return View(vm);
            }
        }

        // GET /Pedido/Detalle/5
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> Detalle(int id)
        {
            using (var ctx = new ApplicationDbContext())
            {
                var servicio = new PedidoServicio(
                    new PedidoRepositorio(ctx),
                    new ProductoRepositorio(ctx),
                    new ZonaEntregaRepositorio(ctx));

                var pedido = await servicio.ObtenerPorIdAsync(id);
                if (pedido == null) return HttpNotFound();

                // Verificar que el pedido pertenezca al usuario actual o sea admin/operador
                var userId = User.Identity.GetUserId();
                bool esAdminOOperador = User.IsInRole("Administrador") || User.IsInRole("Operador");

                if (pedido.UserId != userId && !esAdminOOperador)
                    return new HttpUnauthorizedResult();

                var vm = new PedidoDetalleViewModel
                {
                    Pedido = pedido,
                    Resumen = new PedidoResumenViewModel
                    {
                        Id            = pedido.Id,
                        Numero        = pedido.Numero,
                        NombreUsuario = pedido.NombreUsuario,
                        FechaPedido   = pedido.FechaPedido,
                        EstadoPedido  = pedido.EstadoPedido,
                        Total         = pedido.Total,
                        CantidadItems = pedido.Detalles.Sum(d => d.Cantidad)
                    }
                };

                return View(vm);
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // GESTIÓN ADMIN / OPERADOR
        // ═══════════════════════════════════════════════════════════════════

        // GET /Pedido/Index
        [HttpGet]
        [Authorize(Roles = "Administrador,Operador")]
        public async Task<ActionResult> Index(string filtroEstado, string filtroBusqueda)
        {
            EstadoPedido? estadoFiltro = null;
            if (!string.IsNullOrEmpty(filtroEstado) && Enum.TryParse(filtroEstado, out EstadoPedido estadoParsed))
                estadoFiltro = estadoParsed;

            using (var ctx = new ApplicationDbContext())
            {
                var servicio = new PedidoServicio(
                    new PedidoRepositorio(ctx),
                    new ProductoRepositorio(ctx),
                    new ZonaEntregaRepositorio(ctx));

                var pedidos = await servicio.ObtenerTodosAsync(estadoFiltro, filtroBusqueda);

                // SelectList de estados
                var estadosItems = new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "Todos los estados" }
                };
                foreach (EstadoPedido est in Enum.GetValues(typeof(EstadoPedido)))
                {
                    estadosItems.Add(new SelectListItem
                    {
                        Value = est.ToString(),
                        Text  = ObtenerTextoEstado(est),
                        Selected = estadoFiltro.HasValue && estadoFiltro.Value == est
                    });
                }

                var vm = new GestionPedidosViewModel
                {
                    FiltroEstado    = filtroEstado,
                    FiltroBusqueda  = filtroBusqueda,
                    EstadosDisponibles = new SelectList(estadosItems, "Value", "Text", filtroEstado),
                    Pedidos = pedidos.Select(p => new PedidoResumenViewModel
                    {
                        Id            = p.Id,
                        Numero        = p.Numero,
                        NombreUsuario = p.NombreUsuario,
                        FechaPedido   = p.FechaPedido,
                        EstadoPedido  = p.EstadoPedido,
                        Total         = p.Total,
                        CantidadItems = p.Detalles.Sum(d => d.Cantidad)
                    }).ToList()
                };

                return View(vm);
            }
        }

        // POST /Pedido/ActualizarEstado
        [HttpPost]
        [Authorize(Roles = "Administrador,Operador")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ActualizarEstado(int id, EstadoPedido estado)
        {
            using (var ctx = new ApplicationDbContext())
            {
                var servicio = new PedidoServicio(
                    new PedidoRepositorio(ctx),
                    new ProductoRepositorio(ctx),
                    new ZonaEntregaRepositorio(ctx));

                var resultado = await servicio.ActualizarEstadoAsync(id, estado);

                if (resultado.Exito)
                {
                    await AuditoriaHelper.RegistrarAsync(
                        HttpContext, TipoOperacion.Actualizar, "Pedidos", id,
                        $"Estado cambiado a {estado}");
                    TempData["Exito"] = resultado.Mensaje;
                }
                else
                {
                    TempData["Error"] = resultado.Mensaje;
                }
            }

            return RedirectToAction("Index");
        }

        // ═══════════════════════════════════════════════════════════════════
        // Helpers privados
        // ═══════════════════════════════════════════════════════════════════

        private List<CarritoItemViewModel> ObtenerCarrito()
        {
            return Session[SessionCarrito] as List<CarritoItemViewModel>
                ?? new List<CarritoItemViewModel>();
        }

        private void GuardarCarrito(List<CarritoItemViewModel> carrito)
        {
            Session[SessionCarrito] = carrito;
            Session[SessionCarritoCount] = carrito.Sum(i => i.Cantidad);
        }

        private void LimpiarCarrito()
        {
            Session[SessionCarrito] = new List<CarritoItemViewModel>();
            Session[SessionCarritoCount] = 0;
        }

        private static string ObtenerTextoEstado(EstadoPedido estado)
        {
            switch (estado)
            {
                case EstadoPedido.Pendiente:  return "Pendiente";
                case EstadoPedido.Procesando: return "Procesando";
                case EstadoPedido.Enviado:    return "Enviado";
                case EstadoPedido.Entregado:  return "Entregado";
                case EstadoPedido.Cancelado:  return "Cancelado";
                default:                      return estado.ToString();
            }
        }
    }
}
