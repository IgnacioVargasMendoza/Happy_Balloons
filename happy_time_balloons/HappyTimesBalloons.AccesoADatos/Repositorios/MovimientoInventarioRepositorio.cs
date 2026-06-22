using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.AccesoADatos.Modelos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTimesBalloons.AccesoADatos.Repositorios
{
    public class MovimientoInventarioRepositorio : IMovimientoInventarioRepositorio
    {
        private readonly ApplicationDbContext _ctx;

        public MovimientoInventarioRepositorio(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<MovimientoInventarioDTO> RegistrarAsync(MovimientoInventarioDTO dto)
        {
            var entidad = new MovimientoInventario
            {
                ProductoId = dto.ProductoId,
                TipoMovimiento = dto.TipoMovimiento,
                Cantidad = dto.Cantidad,
                StockAnterior = dto.StockAnterior,
                StockNuevo = dto.StockNuevo,
                Motivo = dto.Motivo,
                UsuarioId = dto.UsuarioId,
                FechaMovimiento = dto.FechaMovimiento
            };

            _ctx.MovimientosInventario.Add(entidad);
            await _ctx.SaveChangesAsync();

            dto.Id = entidad.Id;
            return dto;
        }

        public async Task<List<MovimientoInventarioDTO>> ObtenerPorProductoAsync(int productoId)
        {
            return await _ctx.MovimientosInventario
                .Where(m => m.ProductoId == productoId)
                .OrderByDescending(m => m.FechaMovimiento)
                .Select(m => new MovimientoInventarioDTO
                {
                    Id = m.Id,
                    ProductoId = m.ProductoId,
                    ProductoNombre = m.Producto.Nombre,
                    TipoMovimiento = m.TipoMovimiento,
                    Cantidad = m.Cantidad,
                    StockAnterior = m.StockAnterior,
                    StockNuevo = m.StockNuevo,
                    Motivo = m.Motivo,
                    UsuarioId = m.UsuarioId,
                    FechaMovimiento = m.FechaMovimiento
                })
                .ToListAsync();
        }

        public async Task<List<MovimientoInventarioDTO>> ObtenerUltimosAsync(int cantidad)
        {
            return await _ctx.MovimientosInventario
                .OrderByDescending(m => m.FechaMovimiento)
                .Take(cantidad)
                .Select(m => new MovimientoInventarioDTO
                {
                    Id = m.Id,
                    ProductoId = m.ProductoId,
                    ProductoNombre = m.Producto.Nombre,
                    TipoMovimiento = m.TipoMovimiento,
                    Cantidad = m.Cantidad,
                    StockAnterior = m.StockAnterior,
                    StockNuevo = m.StockNuevo,
                    Motivo = m.Motivo,
                    UsuarioId = m.UsuarioId,
                    FechaMovimiento = m.FechaMovimiento
                })
                .ToListAsync();
        }
    }
}
