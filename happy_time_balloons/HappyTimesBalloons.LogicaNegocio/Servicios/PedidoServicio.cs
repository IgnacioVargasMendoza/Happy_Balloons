using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTimesBalloons.LogicaNegocio.Servicios
{
    public class PedidoServicio : IPedidoServicio
    {
        private readonly IPedidoRepositorio _pedidoRepo;
        private readonly IProductoRepositorio _productoRepo;
        private readonly IZonaEntregaRepositorio _zonaRepo;

        public PedidoServicio(
            IPedidoRepositorio pedidoRepo,
            IProductoRepositorio productoRepo,
            IZonaEntregaRepositorio zonaRepo)
        {
            _pedidoRepo = pedidoRepo;
            _productoRepo = productoRepo;
            _zonaRepo = zonaRepo;
        }

        public async Task<ResultadoOperacionDTO<int>> CrearPedidoAsync(string userId, CheckoutDTO checkout)
        {
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(userId))
                return ResultadoOperacionDTO<int>.Fallo("Usuario no identificado.", CodigoResultado.DatosInvalidos);

            if (checkout.Items == null || !checkout.Items.Any())
                return ResultadoOperacionDTO<int>.Fallo("El carrito está vacío.", CodigoResultado.DatosInvalidos);

            if (string.IsNullOrWhiteSpace(checkout.DireccionEntrega))
                return ResultadoOperacionDTO<int>.Fallo("La dirección de entrega es obligatoria.", CodigoResultado.DatosInvalidos);

            if (string.IsNullOrWhiteSpace(checkout.MetodoPago))
                return ResultadoOperacionDTO<int>.Fallo("El método de pago es obligatorio.", CodigoResultado.DatosInvalidos);

            // Verificar zona de entrega
            var zona = await _zonaRepo.ObtenerPorIdAsync(checkout.ZonaEntregaId);
            if (zona == null || !zona.EsDisponible)
                return ResultadoOperacionDTO<int>.Fallo("La zona de entrega seleccionada no está disponible.", CodigoResultado.DatosInvalidos);

            // Verificar stock de cada producto y calcular subtotales
            decimal subtotal = 0m;
            foreach (var item in checkout.Items)
            {
                var producto = await _productoRepo.ObtenerPorIdAsync(item.ProductoId);
                if (producto == null || !producto.EsActivo)
                    return ResultadoOperacionDTO<int>.Fallo(
                        $"El producto con Id {item.ProductoId} no está disponible.", CodigoResultado.DatosInvalidos);

                if (producto.Stock < item.Cantidad)
                    return ResultadoOperacionDTO<int>.Fallo(
                        $"Stock insuficiente para '{producto.Nombre}'. Disponible: {producto.Stock}.", CodigoResultado.DatosInvalidos);

                decimal precio = producto.PrecioDescuento.HasValue && producto.PrecioDescuento < producto.Precio
                    ? producto.PrecioDescuento.Value
                    : producto.Precio;

                subtotal += precio * item.Cantidad;
            }

            decimal costoEnvio = zona.CostoEnvio;
            decimal total = subtotal + costoEnvio;

            var dto = new PedidoDTO
            {
                UserId = userId,
                MetodoPago = checkout.MetodoPago,
                NumeroReferencia = checkout.NumeroReferencia,
                ZonaEntregaId = checkout.ZonaEntregaId,
                DireccionEntrega = checkout.DireccionEntrega,
                Subtotal = subtotal,
                CostoEnvio = costoEnvio,
                Total = total,
                Notas = checkout.Notas
            };

            try
            {
                var pedidoCreado = await _pedidoRepo.CrearAsync(dto, checkout.Items);
                return ResultadoOperacionDTO<int>.Ok("Pedido creado exitosamente.", pedidoCreado.Id);
            }
            catch (InvalidOperationException ex)
            {
                return ResultadoOperacionDTO<int>.Fallo(ex.Message, CodigoResultado.Error);
            }
        }

        public Task<PedidoDTO> ObtenerPorIdAsync(int id)
            => _pedidoRepo.ObtenerPorIdAsync(id);

        public Task<List<PedidoDTO>> ObtenerPorUsuarioAsync(string userId)
            => _pedidoRepo.ObtenerPorUsuarioAsync(userId);

        public Task<List<PedidoDTO>> ObtenerTodosAsync(EstadoPedido? filtroEstado = null, string busqueda = null)
            => _pedidoRepo.ObtenerTodosAsync(filtroEstado, busqueda);

        public async Task<ResultadoOperacionDTO> ActualizarEstadoAsync(int id, EstadoPedido nuevoEstado)
        {
            var pedido = await _pedidoRepo.ObtenerPorIdAsync(id);
            if (pedido == null)
                return ResultadoOperacionDTO.Fallo("Pedido no encontrado.", CodigoResultado.Error);

            if (pedido.EstadoPedido == nuevoEstado)
                return ResultadoOperacionDTO.Fallo("El pedido ya se encuentra en ese estado.", CodigoResultado.DatosInvalidos);

            if (!TransicionValida(pedido.EstadoPedido, nuevoEstado))
                return ResultadoOperacionDTO.Fallo(
                    $"No se puede cambiar el estado de '{pedido.EstadoPedido}' a '{nuevoEstado}'.",
                    CodigoResultado.DatosInvalidos);

            var ok = await _pedidoRepo.ActualizarEstadoAsync(id, nuevoEstado);
            return ok
                ? ResultadoOperacionDTO.Ok("Estado del pedido actualizado.")
                : ResultadoOperacionDTO.Fallo("No se pudo actualizar el estado.", CodigoResultado.Error);
        }

        public static bool TransicionValida(EstadoPedido actual, EstadoPedido nuevo)
        {
            switch (actual)
            {
                case EstadoPedido.PagoPendiente:
                    return nuevo == EstadoPedido.Pendiente || nuevo == EstadoPedido.Cancelado;
                case EstadoPedido.Pendiente:
                    return nuevo == EstadoPedido.Procesando || nuevo == EstadoPedido.Cancelado;
                case EstadoPedido.Procesando:
                    return nuevo == EstadoPedido.Enviado || nuevo == EstadoPedido.Cancelado;
                case EstadoPedido.Enviado:
                    return nuevo == EstadoPedido.Entregado || nuevo == EstadoPedido.Cancelado;
                default:
                    return false;
            }
        }

        public Task<PedidoEstadisticasDTO> ObtenerEstadisticasAsync()
            => _pedidoRepo.ObtenerEstadisticasAsync();

        public Task<List<VentaDiariaDTO>> ObtenerVentasPorDiaAsync(int dias)
            => _pedidoRepo.ObtenerVentasPorDiaAsync(dias);

        public int AjustarCantidad(int cantidadSolicitada, int stockDisponible)
            => Math.Min(cantidadSolicitada > 0 ? cantidadSolicitada : 1, stockDisponible);
    }
}
