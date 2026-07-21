using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
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

        private readonly IPedidoServicio _pedidoServicio;
        private readonly IProductoServicio _productoServicio;
        private readonly IZonaEntregaServicio _zonaServicio;
        private readonly IAuditoriaServicio _auditoriaServicio;
        private readonly INotificacionPedidoServicio _notificacionServicio;

        public PedidoController(
            IPedidoServicio pedidoServicio,
            IProductoServicio productoServicio,
            IZonaEntregaServicio zonaServicio,
            IAuditoriaServicio auditoriaServicio,
            INotificacionPedidoServicio notificacionServicio)
        {
            _pedidoServicio = pedidoServicio;
            _productoServicio = productoServicio;
            _zonaServicio = zonaServicio;
            _auditoriaServicio = auditoriaServicio;
            _notificacionServicio = notificacionServicio;
        }

        // ═══════════════════════════════════════════════════════════════════
        // CARRITO
        // ═══════════════════════════════════════════════════════════════════

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Carrito()
        {
            var vm = new CarritoViewModel { Items = ObtenerCarrito() };
            return View(vm);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AgregarAlCarrito(int productoId, int cantidad)
        {
            if (cantidad < 1) cantidad = 1;

            var producto = await _productoServicio.ObtenerPorIdAsync(productoId);

            if (producto == null || !producto.EsActivo)
            {
                TempData["Error"] = "El producto no está disponible.";
                return RedirectToAction("Carrito");
            }

            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(i => i.ProductoId == productoId);

            if (item != null)
            {
                item.Cantidad = _pedidoServicio.AjustarCantidad(item.Cantidad + cantidad, producto.Stock);
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
                    Cantidad = _pedidoServicio.AjustarCantidad(cantidad, producto.Stock)
                });
            }

            GuardarCarrito(carrito);
            TempData["Exito"] = "Producto agregado al carrito.";
            return RedirectToAction("Carrito");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ActualizarCantidad(int productoId, int cantidad)
        {
            if (cantidad < 1) cantidad = 1;

            var producto = await _productoServicio.ObtenerPorIdAsync(productoId);
            if (producto == null || !producto.EsActivo)
            {
                TempData["Error"] = "El producto no está disponible.";
                return RedirectToAction("Carrito");
            }

            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(i => i.ProductoId == productoId);
            if (item != null)
                item.Cantidad = _pedidoServicio.AjustarCantidad(cantidad, producto.Stock);

            GuardarCarrito(carrito);
            return RedirectToAction("Carrito");
        }

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

            var zonas = await _zonaServicio.ObtenerTodasAsync();
            var vm = new CheckoutViewModel
            {
                ItemsCarrito = carrito,
                ZonasEntrega = zonas.Select(MapearZona).ToList()
            };

            if (vm.ZonasEntrega.Any())
            {
                vm.ZonaEntregaId = vm.ZonasEntrega.First().Id;
                vm.CostoEnvio = vm.ZonasEntrega.First().CostoEnvio;
            }

            return View(vm);
        }

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

            if (!ModelState.IsValid)
            {
                var zonas = await _zonaServicio.ObtenerTodasAsync();
                model.ItemsCarrito = carrito;
                model.ZonasEntrega = zonas.Select(MapearZona).ToList();
                return View("Checkout", model);
            }

            var checkout = new CheckoutDTO
            {
                ZonaEntregaId = model.ZonaEntregaId,
                DireccionEntrega = model.DireccionEntrega,
                MetodoPago = model.MetodoPago,
                NumeroReferencia = model.NumeroReferencia,
                Notas = model.Notas,
                Items = carrito.Select(i => new CheckoutItemDTO
                {
                    ProductoId = i.ProductoId,
                    Cantidad = i.Cantidad
                }).ToList()
            };

            var resultado = await _pedidoServicio.CrearPedidoAsync(User.Identity.GetUserId(), checkout);

            if (!resultado.Exito)
            {
                TempData["Error"] = resultado.Mensaje;
                var zonas = await _zonaServicio.ObtenerTodasAsync();
                model.ItemsCarrito = carrito;
                model.ZonasEntrega = zonas.Select(MapearZona).ToList();
                return View("Checkout", model);
            }

            await _auditoriaServicio.RegistrarAsync(
                User.Identity.Name, User.Identity.Name,
                TipoOperacion.Crear, "Pedidos",
                registroId: resultado.Datos,
                detalle: $"Pedido creado por {User.Identity.GetUserId()}",
                ip: Request.UserHostAddress);

            var pedidoCompleto = await _pedidoServicio.ObtenerPorIdAsync(resultado.Datos);
            if (pedidoCompleto != null)
            {
                var resultadoCorreo = await _notificacionServicio.EnviarConfirmacionAsync(
                    pedidoCompleto, User.Identity.Name);

                await _auditoriaServicio.RegistrarAsync(
                    User.Identity.Name, User.Identity.Name,
                    TipoOperacion.EnviarNotificacionPedido, "NotificacionCorreo",
                    registroId: resultado.Datos,
                    detalle: resultadoCorreo.Exito
                        ? $"Correo de confirmación enviado a {User.Identity.Name}"
                        : $"Fallo al enviar correo: {resultadoCorreo.Mensaje}",
                    ip: Request.UserHostAddress);
            }

            LimpiarCarrito();
            TempData["Exito"] = "¡Tu pedido fue confirmado exitosamente!";
            return RedirectToAction("MisPedidos");
        }

        // ═══════════════════════════════════════════════════════════════════
        // MIS PEDIDOS (Cliente)
        // ═══════════════════════════════════════════════════════════════════

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> MisPedidos()
        {
            var pedidos = await _pedidoServicio.ObtenerPorUsuarioAsync(User.Identity.GetUserId());

            var vm = new MisPedidosViewModel
            {
                Pedidos = pedidos.Select(p => new PedidoResumenViewModel
                {
                    Id = p.Id,
                    Numero = p.Numero,
                    NombreUsuario = p.NombreUsuario,
                    FechaPedido = p.FechaPedido,
                    EstadoPedido = p.EstadoPedido,
                    Total = p.Total,
                    CantidadItems = p.CantidadItems
                }).ToList()
            };

            return View(vm);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> Detalle(int id)
        {
            var pedido = await _pedidoServicio.ObtenerPorIdAsync(id);
            if (pedido == null) return HttpNotFound();

            var userId = User.Identity.GetUserId();
            bool esAdminOOperador = User.IsInRole("Administrador") || User.IsInRole("Operador");

            if (pedido.UserId != userId && !esAdminOOperador)
                return new HttpUnauthorizedResult();

            var vm = new PedidoDetalleViewModel
            {
                Pedido = pedido,
                Resumen = new PedidoResumenViewModel
                {
                    Id = pedido.Id,
                    Numero = pedido.Numero,
                    NombreUsuario = pedido.NombreUsuario,
                    FechaPedido = pedido.FechaPedido,
                    EstadoPedido = pedido.EstadoPedido,
                    Total = pedido.Total,
                    CantidadItems = pedido.CantidadItems
                }
            };

            return View(vm);
        }

        // ═══════════════════════════════════════════════════════════════════
        // GESTIÓN ADMIN / OPERADOR
        // ═══════════════════════════════════════════════════════════════════

        [HttpGet]
        [Authorize(Roles = "Administrador,Operador")]
        public async Task<ActionResult> Index(string filtroEstado, string filtroBusqueda)
        {
            EstadoPedido? estadoFiltro = null;
            if (!string.IsNullOrEmpty(filtroEstado) &&
                Enum.TryParse(filtroEstado, out EstadoPedido estadoParsed))
                estadoFiltro = estadoParsed;

            var pedidos = await _pedidoServicio.ObtenerTodosAsync(estadoFiltro, filtroBusqueda);

            var estadosItems = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Todos los estados" }
            };
            foreach (EstadoPedido est in Enum.GetValues(typeof(EstadoPedido)))
            {
                estadosItems.Add(new SelectListItem
                {
                    Value = est.ToString(),
                    Text = ObtenerTextoEstado(est),
                    Selected = estadoFiltro.HasValue && estadoFiltro.Value == est
                });
            }

            var vm = new GestionPedidosViewModel
            {
                FiltroEstado = filtroEstado,
                FiltroBusqueda = filtroBusqueda,
                EstadosDisponibles = new SelectList(estadosItems, "Value", "Text", filtroEstado),
                Pedidos = pedidos.Select(p => new PedidoResumenViewModel
                {
                    Id = p.Id,
                    Numero = p.Numero,
                    NombreUsuario = p.NombreUsuario,
                    FechaPedido = p.FechaPedido,
                    EstadoPedido = p.EstadoPedido,
                    Total = p.Total,
                    CantidadItems = p.CantidadItems
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Operador")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ActualizarEstado(int id, EstadoPedido estado)
        {
            var resultado = await _pedidoServicio.ActualizarEstadoAsync(id, estado);

            if (resultado.Exito)
            {
                await _auditoriaServicio.RegistrarAsync(
                    User.Identity.Name, User.Identity.Name,
                    TipoOperacion.Actualizar, "Pedidos",
                    registroId: id, detalle: $"Estado cambiado a {estado}",
                    ip: Request.UserHostAddress);
                TempData["Exito"] = resultado.Mensaje;
            }
            else
            {
                TempData["Error"] = resultado.Mensaje;
            }

            return RedirectToAction("Index");
        }

        // ═══════════════════════════════════════════════════════════════════
        // Helpers privados
        // ═══════════════════════════════════════════════════════════════════

        private List<CarritoItemViewModel> ObtenerCarrito() =>
            Session[SessionCarrito] as List<CarritoItemViewModel>
            ?? new List<CarritoItemViewModel>();

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

        private static ZonaEntregaViewModel MapearZona(ZonaEntregaDTO z) =>
            new ZonaEntregaViewModel
            {
                Id = z.Id,
                Nombre = z.Nombre,
                Descripcion = z.Descripcion,
                CostoEnvio = z.CostoEnvio,
                EsDisponible = z.EsDisponible
            };

        private static string ObtenerTextoEstado(EstadoPedido estado)
        {
            switch (estado)
            {
                case EstadoPedido.Pendiente: return "Pendiente";
                case EstadoPedido.Procesando: return "Procesando";
                case EstadoPedido.Enviado: return "Enviado";
                case EstadoPedido.Entregado: return "Entregado";
                case EstadoPedido.Cancelado:      return "Cancelado";
                case EstadoPedido.PagoPendiente: return "Pago pendiente";
                default:                         return estado.ToString();
            }
        }
    }
}
