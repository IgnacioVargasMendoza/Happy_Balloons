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
    public class RankingProductosRepositorio : IRankingProductosRepositorio
    {
        private readonly ApplicationDbContext _ctx;

        public RankingProductosRepositorio(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<List<RankingProductoItemDTO>> ObtenerItemsAsync(
            DateTime fechaInicio,
            DateTime fechaFin,
            int? zonaId)
        {
            var hasta = fechaFin.Date.AddDays(1);

            var query = _ctx.DetallesPedido
                .Include(dp => dp.Pedido)
                .Include(dp => dp.Producto.Categoria)
                .Where(dp =>
                    dp.Pedido.EstadoPedido != EstadoPedido.Cancelado &&
                    dp.Pedido.FechaPedido >= fechaInicio.Date &&
                    dp.Pedido.FechaPedido < hasta &&
                    (zonaId == null || dp.Pedido.ZonaEntregaId == zonaId));

            var detalles = await query.ToListAsync();

            var items = detalles
                .GroupBy(dp => new
                {
                    dp.ProductoId,
                    NombreProducto = dp.Producto.Nombre,
                    Categoria = dp.Producto.Categoria.Nombre
                })
                .Select(g => new RankingProductoItemDTO
                {
                    ProductoId = g.Key.ProductoId,
                    NombreProducto = g.Key.NombreProducto,
                    Categoria = g.Key.Categoria,
                    UnidadesVendidas = g.Sum(d => d.Cantidad),
                    Ingresos = g.Sum(d => d.Subtotal),
                    NumeroPedidos = g.Select(d => d.PedidoId).Distinct().Count()
                })
                .OrderByDescending(x => x.UnidadesVendidas)
                .ToList();

            return items;
        }
    }
}
