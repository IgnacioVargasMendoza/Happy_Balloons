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
            var movimientos = await _ctx.MovimientosInventario
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

            await ResolverNombresUsuarioAsync(movimientos);
            return movimientos;
        }

        public async Task<List<MovimientoInventarioDTO>> ObtenerUltimosAsync(int cantidad)
        {
            var movimientos = await _ctx.MovimientosInventario
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

            await ResolverNombresUsuarioAsync(movimientos);
            return movimientos;
        }

        private async Task ResolverNombresUsuarioAsync(List<MovimientoInventarioDTO> movimientos)
        {
            var ids = movimientos
                .Where(m => m.UsuarioId != null)
                .Select(m => m.UsuarioId)
                .Distinct()
                .ToList();

            if (!ids.Any())
                return;

            var emails = await _ctx.Users
                .Where(u => ids.Contains(u.Id))
                .Select(u => new { u.Id, u.Email })
                .ToDictionaryAsync(u => u.Id, u => u.Email);

            foreach (var m in movimientos)
            {
                if (m.UsuarioId != null && emails.TryGetValue(m.UsuarioId, out var email))
                {
                    m.NombreUsuario = email;
                }
                else
                {
                    m.NombreUsuario = m.UsuarioId;
                }
            }
        }
    }
}
