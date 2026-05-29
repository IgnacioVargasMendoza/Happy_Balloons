using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.AccesoADatos.Modelos;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTimesBalloons.AccesoADatos.Repositorios
{
    public class PromocionRepositorio : IPromocionRepositorio
    {
        private readonly ApplicationDbContext _ctx;

        public PromocionRepositorio(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<List<PromocionDTO>> ObtenerTodasAsync()
        {
            return await _ctx.Promociones
                .Include(p => p.Producto)
                .OrderByDescending(p => p.FechaFin)
                .Select(p => new PromocionDTO
                {
                    Id = p.Id,
                    ProductoId = p.ProductoId,
                    ProductoNombre = p.Producto.Nombre,
                    DescuentoPorcentaje = p.DescuentoPorcentaje,
                    FechaInicio = p.FechaInicio,
                    FechaFin = p.FechaFin,
                    Activa = p.Activa
                })
                .ToListAsync();
        }

        public async Task<PromocionDTO> ObtenerPorIdAsync(int id)
        {
            var p = await _ctx.Promociones
                .Include(x => x.Producto)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (p == null) return null;

            return new PromocionDTO
            {
                Id = p.Id,
                ProductoId = p.ProductoId,
                ProductoNombre = p.Producto?.Nombre,
                DescuentoPorcentaje = p.DescuentoPorcentaje,
                FechaInicio = p.FechaInicio,
                FechaFin = p.FechaFin,
                Activa = p.Activa
            };
        }

        public async Task<PromocionDTO> CrearAsync(PromocionDTO dto)
        {
            var entidad = new Promocion
            {
                ProductoId = dto.ProductoId,
                DescuentoPorcentaje = dto.DescuentoPorcentaje,
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin,
                Activa = true
            };

            _ctx.Promociones.Add(entidad);
            await _ctx.SaveChangesAsync();

            dto.Id = entidad.Id;
            dto.Activa = entidad.Activa;
            return dto;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var entidad = await _ctx.Promociones.FindAsync(id);
            if (entidad == null) return false;

            _ctx.Promociones.Remove(entidad);
            await _ctx.SaveChangesAsync();
            return true;
        }
    }
}
