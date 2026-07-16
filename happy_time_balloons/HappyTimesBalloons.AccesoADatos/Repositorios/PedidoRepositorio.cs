using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.AccesoADatos.Modelos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTimesBalloons.AccesoADatos.Repositorios
{
    public class PedidoRepositorio : IPedidoRepositorio
    {
        private readonly ApplicationDbContext _ctx;

        public PedidoRepositorio(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<PedidoDTO> CrearAsync(PedidoDTO dto, IList<CheckoutItemDTO> items)
        {
            var pedido = new Pedido
            {
                Numero = "TEMP",
                UserId = dto.UserId,
                FechaPedido = DateTime.UtcNow,
                EstadoPedido = dto.MetodoPago == "SINPE" ? EstadoPedido.PagoPendiente : EstadoPedido.Pendiente,
                MetodoPago = dto.MetodoPago,
                NumeroReferencia = dto.NumeroReferencia,
                ZonaEntregaId = dto.ZonaEntregaId,
                DireccionEntrega = dto.DireccionEntrega,
                Subtotal = dto.Subtotal,
                CostoEnvio = dto.CostoEnvio,
                Total = dto.Total,
                Notas = dto.Notas
            };

            foreach (var item in items)
            {
                var producto = await _ctx.Productos.FindAsync(item.ProductoId);
                if (producto == null) continue;

                decimal precio = producto.PrecioDescuento.HasValue && producto.PrecioDescuento < producto.Precio
                    ? producto.PrecioDescuento.Value
                    : producto.Precio;

                pedido.DetallesPedido.Add(new DetallePedido
                {
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = precio,
                    Subtotal = precio * item.Cantidad
                });
            }

            _ctx.Pedidos.Add(pedido);

            // Un solo SaveChanges: Pedido + Detalles + trigger en la misma transacción (ACID)
            try
            {
                await _ctx.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var sqlEx = ex.InnerException?.InnerException as SqlException
                            ?? ex.InnerException as SqlException;
                string mensaje = sqlEx != null
                    ? sqlEx.Message
                    : "No se pudo crear el pedido. Verifique el stock disponible.";
                throw new InvalidOperationException(mensaje, ex);
            }

            // Número definitivo: requiere el Id generado por la BD
            pedido.Numero = $"PED-{pedido.FechaPedido.Year}-{pedido.Id:D4}";
            await _ctx.SaveChangesAsync();

            dto.Id = pedido.Id;
            dto.Numero = pedido.Numero;
            dto.FechaPedido = pedido.FechaPedido;
            dto.EstadoPedido = pedido.EstadoPedido;
            return dto;
        }

        public async Task<PedidoDTO> ObtenerPorIdAsync(int id)
        {
            var p = await _ctx.Pedidos
                .Include(x => x.ZonaEntrega)
                .Include(x => x.DetallesPedido.Select(d => d.Producto.Imagenes))
                .Include(x => x.Usuario)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (p == null) return null;

            return MapearDTO(p);
        }

        public async Task<List<PedidoDTO>> ObtenerPorUsuarioAsync(string userId)
        {
            return await _ctx.Pedidos
                .Include(x => x.ZonaEntrega)
                .Include(x => x.DetallesPedido.Select(d => d.Producto.Imagenes))
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.FechaPedido)
                .Select(x => new PedidoDTO
                {
                    Id             = x.Id,
                    Numero         = x.Numero,
                    UserId         = x.UserId,
                    NombreUsuario  = x.Usuario.Nombre ?? x.Usuario.Email,
                    FechaPedido    = x.FechaPedido,
                    EstadoPedido   = x.EstadoPedido,
                    MetodoPago     = x.MetodoPago,
                    NumeroReferencia = x.NumeroReferencia,
                    ZonaEntregaId  = x.ZonaEntregaId,
                    NombreZona     = x.ZonaEntrega.Nombre,
                    DireccionEntrega = x.DireccionEntrega,
                    Subtotal       = x.Subtotal,
                    CostoEnvio     = x.CostoEnvio,
                    Total          = x.Total,
                    Notas          = x.Notas,
                    Detalles       = x.DetallesPedido.Select(d => new DetallePedidoDTO
                    {
                        Id             = d.Id,
                        PedidoId       = d.PedidoId,
                        ProductoId     = d.ProductoId,
                        NombreProducto = d.Producto.Nombre,
                        ImagenUrl      = d.Producto.Imagenes
                            .Where(i => i.EsPrincipal)
                            .Select(i => i.RutaImagen)
                            .FirstOrDefault(),
                        Cantidad       = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal       = d.Subtotal
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<List<PedidoDTO>> ObtenerTodosAsync(EstadoPedido? filtroEstado = null, string busqueda = null)
        {
            var query = _ctx.Pedidos
                .Include(x => x.ZonaEntrega)
                .Include(x => x.DetallesPedido.Select(d => d.Producto.Imagenes))
                .Include(x => x.Usuario)
                .AsQueryable();

            if (filtroEstado.HasValue)
                query = query.Where(x => x.EstadoPedido == filtroEstado.Value);

            if (!string.IsNullOrWhiteSpace(busqueda))
                query = query.Where(x =>
                    x.Numero.Contains(busqueda) ||
                    x.Usuario.Nombre.Contains(busqueda) ||
                    x.Usuario.Email.Contains(busqueda));

            return await query
                .OrderByDescending(x => x.FechaPedido)
                .Select(x => new PedidoDTO
                {
                    Id             = x.Id,
                    Numero         = x.Numero,
                    UserId         = x.UserId,
                    NombreUsuario  = x.Usuario.Nombre ?? x.Usuario.Email,
                    FechaPedido    = x.FechaPedido,
                    EstadoPedido   = x.EstadoPedido,
                    MetodoPago     = x.MetodoPago,
                    NumeroReferencia = x.NumeroReferencia,
                    ZonaEntregaId  = x.ZonaEntregaId,
                    NombreZona     = x.ZonaEntrega.Nombre,
                    DireccionEntrega = x.DireccionEntrega,
                    Subtotal       = x.Subtotal,
                    CostoEnvio     = x.CostoEnvio,
                    Total          = x.Total,
                    Notas          = x.Notas,
                    Detalles       = x.DetallesPedido.Select(d => new DetallePedidoDTO
                    {
                        Id             = d.Id,
                        PedidoId       = d.PedidoId,
                        ProductoId     = d.ProductoId,
                        NombreProducto = d.Producto.Nombre,
                        ImagenUrl      = d.Producto.Imagenes
                            .Where(i => i.EsPrincipal)
                            .Select(i => i.RutaImagen)
                            .FirstOrDefault(),
                        Cantidad       = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal       = d.Subtotal
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<bool> ActualizarEstadoAsync(int id, EstadoPedido nuevoEstado)
        {
            var pedido = await _ctx.Pedidos.FindAsync(id);
            if (pedido == null) return false;

            pedido.EstadoPedido = nuevoEstado;
            await _ctx.SaveChangesAsync();
            return true;
        }

        public async Task<int> ContarPorEstadoAsync(EstadoPedido estado)
        {
            return await _ctx.Pedidos.CountAsync(x => x.EstadoPedido == estado);
        }

        public async Task<PedidoEstadisticasDTO> ObtenerEstadisticasAsync()
        {
            var hoy = DateTime.UtcNow.Date;
            var manana = hoy.AddDays(1);

            var total = await _ctx.Pedidos.CountAsync();
            var pedidosHoy = await _ctx.Pedidos.CountAsync(p => p.FechaPedido >= hoy && p.FechaPedido < manana);
            var ventasTotales = await _ctx.Pedidos.SumAsync(p => (decimal?)p.Total) ?? 0m;

            return new PedidoEstadisticasDTO
            {
                Total = total,
                PedidosHoy = pedidosHoy,
                VentasTotales = ventasTotales
            };
        }

        public async Task<List<VentaDiariaDTO>> ObtenerVentasPorDiaAsync(int dias)
        {
            var desde = DateTime.UtcNow.Date.AddDays(-(dias - 1));
            var hasta = DateTime.UtcNow.Date.AddDays(1);

            var datos = await _ctx.Pedidos
                .Where(p => p.FechaPedido >= desde && p.FechaPedido < hasta)
                .Select(p => new { FechaDia = DbFunctions.TruncateTime(p.FechaPedido), p.Total })
                .GroupBy(p => p.FechaDia)
                .Select(g => new { Fecha = g.Key, Total = g.Sum(x => x.Total), Cantidad = g.Count() })
                .OrderBy(x => x.Fecha)
                .ToListAsync();

            var resultado = new List<VentaDiariaDTO>();
            for (int i = 0; i < dias; i++)
            {
                var fecha = desde.AddDays(i);
                var entrada = datos.FirstOrDefault(d => d.Fecha == fecha);
                resultado.Add(new VentaDiariaDTO
                {
                    Fecha = fecha.ToString("dd/MM"),
                    Total = entrada != null ? entrada.Total : 0m,
                    Cantidad = entrada != null ? entrada.Cantidad : 0
                });
            }
            return resultado;
        }

        public async Task<PedidoDTO> BuscarPorSinpeAsync(string numeroComprobante, decimal monto)
        {
            var pedido = await _ctx.Pedidos
                .Include(x => x.ZonaEntrega)
                .Include(x => x.Usuario)
                .Where(x =>
                    x.EstadoPedido == EstadoPedido.PagoPendiente &&
                    x.MetodoPago == "SINPE" &&
                    (x.NumeroReferencia == numeroComprobante || x.Total == monto))
                .OrderByDescending(x => x.FechaPedido)
                .FirstOrDefaultAsync();

            return pedido == null ? null : MapearDTO(pedido);
        }

        // ── Helpers privados ─────────────────────────────────────────────────

        private static PedidoDTO MapearDTO(Pedido p)
        {
            return new PedidoDTO
            {
                Id             = p.Id,
                Numero         = p.Numero,
                UserId         = p.UserId,
                NombreUsuario  = p.Usuario?.Nombre ?? p.Usuario?.Email,
                FechaPedido    = p.FechaPedido,
                EstadoPedido   = p.EstadoPedido,
                MetodoPago     = p.MetodoPago,
                NumeroReferencia = p.NumeroReferencia,
                ZonaEntregaId  = p.ZonaEntregaId,
                NombreZona     = p.ZonaEntrega?.Nombre,
                DireccionEntrega = p.DireccionEntrega,
                Subtotal       = p.Subtotal,
                CostoEnvio     = p.CostoEnvio,
                Total          = p.Total,
                Notas          = p.Notas,
                Detalles       = p.DetallesPedido?.Select(d => new DetallePedidoDTO
                {
                    Id             = d.Id,
                    PedidoId       = d.PedidoId,
                    ProductoId     = d.ProductoId,
                    NombreProducto = d.Producto?.Nombre,
                    ImagenUrl      = d.Producto?.Imagenes
                        ?.Where(i => i.EsPrincipal)
                        .Select(i => i.RutaImagen)
                        .FirstOrDefault(),
                    Cantidad       = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal       = d.Subtotal
                }).ToList() ?? new List<DetallePedidoDTO>()
            };
        }
    }
}
