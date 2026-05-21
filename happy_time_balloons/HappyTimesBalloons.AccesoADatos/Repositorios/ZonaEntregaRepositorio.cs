using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.AccesoADatos.Contexto;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTimesBalloons.AccesoADatos.Repositorios
{
    public class ZonaEntregaRepositorio : IZonaEntregaRepositorio
    {
        private readonly ApplicationDbContext _ctx;

        public ZonaEntregaRepositorio(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<List<ZonaEntregaDTO>> ObtenerTodasAsync()
        {
            return await _ctx.ZonasEntrega
                .Where(z => z.EsDisponible)
                .OrderBy(z => z.CostoEnvio)
                .Select(z => new ZonaEntregaDTO
                {
                    Id = z.Id,
                    Nombre = z.Nombre,
                    Descripcion = z.Descripcion,
                    CostoEnvio = z.CostoEnvio,
                    EsDisponible = z.EsDisponible
                })
                .ToListAsync();
        }

        public async Task<ZonaEntregaDTO> ObtenerPorIdAsync(int id)
        {
            var z = await _ctx.ZonasEntrega.FindAsync(id);
            if (z == null) return null;

            return new ZonaEntregaDTO
            {
                Id = z.Id,
                Nombre = z.Nombre,
                Descripcion = z.Descripcion,
                CostoEnvio = z.CostoEnvio,
                EsDisponible = z.EsDisponible
            };
        }
    }
}
