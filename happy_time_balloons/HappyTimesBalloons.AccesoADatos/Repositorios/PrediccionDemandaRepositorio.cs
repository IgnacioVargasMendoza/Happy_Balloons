using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.AccesoADatos.Contexto;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTimesBalloons.AccesoADatos.Repositorios
{
    public class PrediccionDemandaRepositorio : IPrediccionDemandaRepositorio
    {
        private readonly ApplicationDbContext _ctx;

        public PrediccionDemandaRepositorio(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<List<PeriodoVentaDTO>> ObtenerHistorialVentasPorProductoAsync(
            int productoId,
            TipoPeriodo tipoPeriodo)
        {
            var estadosValidos = new[]
            {
                HappyTimesBalloons.Abstraccion.Enums.EstadoPedido.Entregado
            };

            var baseQuery = _ctx.DetallesPedido
                .Where(dp =>
                    dp.ProductoId == productoId &&
                    estadosValidos.Contains(dp.Pedido.EstadoPedido));

            List<PeriodoVentaDTO> periodos;

            if (tipoPeriodo == TipoPeriodo.Semanal)
            {
                var agrupados = await baseQuery
                    .GroupBy(dp => new
                    {
                        Anio = SqlFunctions.DatePart("year", dp.Pedido.FechaPedido),
                        Periodo = SqlFunctions.DatePart("week", dp.Pedido.FechaPedido)
                    })
                    .Select(g => new
                    {
                        g.Key.Anio,
                        g.Key.Periodo,
                        Total = (double)g.Sum(dp => dp.Cantidad)
                    })
                    .OrderBy(g => g.Anio)
                    .ThenBy(g => g.Periodo)
                    .ToListAsync();

                periodos = agrupados
                    .Select(x => new PeriodoVentaDTO
                    {
                        Anio = x.Anio.GetValueOrDefault(),
                        NumeroPeriodo = x.Periodo.GetValueOrDefault(),
                        CantidadVendida = x.Total
                    })
                    .ToList();
            }
            else if (tipoPeriodo == TipoPeriodo.Mensual)
            {
                var agrupados = await baseQuery
                    .GroupBy(dp => new
                    {
                        Anio = SqlFunctions.DatePart("year", dp.Pedido.FechaPedido),
                        Periodo = SqlFunctions.DatePart("month", dp.Pedido.FechaPedido)
                    })
                    .Select(g => new
                    {
                        g.Key.Anio,
                        g.Key.Periodo,
                        Total = (double)g.Sum(dp => dp.Cantidad)
                    })
                    .OrderBy(g => g.Anio)
                    .ThenBy(g => g.Periodo)
                    .ToListAsync();

                periodos = agrupados
                    .Select(x => new PeriodoVentaDTO
                    {
                        Anio = x.Anio.GetValueOrDefault(),
                        NumeroPeriodo = x.Periodo.GetValueOrDefault(),
                        CantidadVendida = x.Total
                    })
                    .ToList();
            }
            else
            {
                var agrupados = await baseQuery
                    .GroupBy(dp => new
                    {
                        Anio = SqlFunctions.DatePart("year", dp.Pedido.FechaPedido),
                        Periodo = SqlFunctions.DatePart("quarter", dp.Pedido.FechaPedido)
                    })
                    .Select(g => new
                    {
                        g.Key.Anio,
                        g.Key.Periodo,
                        Total = (double)g.Sum(dp => dp.Cantidad)
                    })
                    .OrderBy(g => g.Anio)
                    .ThenBy(g => g.Periodo)
                    .ToListAsync();

                periodos = agrupados
                    .Select(x => new PeriodoVentaDTO
                    {
                        Anio = x.Anio.GetValueOrDefault(),
                        NumeroPeriodo = x.Periodo.GetValueOrDefault(),
                        CantidadVendida = x.Total
                    })
                    .ToList();
            }

            return periodos;
        }

        public async Task<List<int>> ObtenerIdsProductosConVentasAsync(TipoPeriodo tipoPeriodo)
        {
            var estadosValidos = new[]
            {
                HappyTimesBalloons.Abstraccion.Enums.EstadoPedido.Entregado
            };

            var fechaCorte = ObtenerFechaCorte(tipoPeriodo);

            return await _ctx.DetallesPedido
                .Where(dp =>
                    estadosValidos.Contains(dp.Pedido.EstadoPedido) &&
                    dp.Pedido.FechaPedido >= fechaCorte)
                .Select(dp => dp.ProductoId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<string> ObtenerNombreProductoAsync(int productoId)
        {
            return await _ctx.Productos
                .Where(p => p.Id == productoId)
                .Select(p => p.Nombre)
                .FirstOrDefaultAsync();
        }

        public async Task<string> ObtenerCategoriaProductoAsync(int productoId)
        {
            return await _ctx.Productos
                .Where(p => p.Id == productoId)
                .Select(p => p.Categoria.Nombre)
                .FirstOrDefaultAsync();
        }

        private static DateTime ObtenerFechaCorte(TipoPeriodo tipoPeriodo)
        {
            var hoy = DateTime.Today;
            switch (tipoPeriodo)
            {
                case TipoPeriodo.Semanal:
                    return hoy.AddDays(-7 * 12);
                case TipoPeriodo.Trimestral:
                    return hoy.AddMonths(-3 * 12);
                default:
                    return hoy.AddMonths(-12);
            }
        }

    }
}
