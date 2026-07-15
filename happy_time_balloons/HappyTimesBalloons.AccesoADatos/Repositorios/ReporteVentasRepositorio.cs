using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.AccesoADatos.Contexto;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTimesBalloons.AccesoADatos.Repositorios
{
    public class ReporteVentasRepositorio : IReporteVentasRepositorio
    {
        private readonly ApplicationDbContext _ctx;

        public ReporteVentasRepositorio(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<ReporteVentasDTO> ObtenerReporteAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            // fechaFin apunta al último instante del día
            var hasta = fechaFin.Date.AddDays(1);

            var pedidos = await _ctx.Pedidos
                .Include(p => p.Usuario)
                .Include(p => p.DetallesPedido.Select(d => d.Producto.Categoria))
                .Where(p =>
                    p.EstadoPedido == EstadoPedido.Entregado &&
                    p.FechaPedido >= fechaInicio.Date &&
                    p.FechaPedido < hasta)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();

            var filasPedidos = pedidos.Select(p => new FilaPedidoReporteDTO
            {
                Id = p.Id,
                Numero = p.Numero,
                FechaPedido = p.FechaPedido,
                NombreCliente = p.Usuario != null ? (p.Usuario.Nombre ?? p.Usuario.Email) : "—",
                MetodoPago = p.MetodoPago,
                Total = p.Total
            }).ToList();

            var detalles = pedidos.SelectMany(p => p.DetallesPedido).ToList();

            var productosMasVendidos = detalles
                .GroupBy(d => new { d.ProductoId, NombreProducto = d.Producto.Nombre, Categoria = d.Producto.Categoria.Nombre })
                .Select(g => new LineaProductoReporteDTO
                {
                    ProductoId = g.Key.ProductoId,
                    NombreProducto = g.Key.NombreProducto,
                    Categoria = g.Key.Categoria,
                    UnidadesVendidas = g.Sum(d => d.Cantidad),
                    Ingresos = g.Sum(d => d.Subtotal)
                })
                .OrderByDescending(x => x.UnidadesVendidas)
                .ToList();

            int totalUnidades = detalles.Sum(d => d.Cantidad);
            decimal ingresosTotales = pedidos.Sum(p => p.Total);

            return new ReporteVentasDTO
            {
                FechaInicio = fechaInicio.Date,
                FechaFin = fechaFin.Date,
                TotalPedidos = pedidos.Count,
                IngresosTotales = ingresosTotales,
                TicketPromedio = pedidos.Count > 0 ? ingresosTotales / pedidos.Count : 0m,
                UnidadesVendidas = totalUnidades,
                Pedidos = filasPedidos,
                ProductosMasVendidos = productosMasVendidos
            };
        }
    }
}
