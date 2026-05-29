using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.AccesoADatos.Modelos;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTimesBalloons.AccesoADatos.Repositorios
{
    public class CodigoVerificacion2FARepositorio : ICodigoVerificacion2FARepositorio
    {
        private readonly ApplicationDbContext _ctx;

        public CodigoVerificacion2FARepositorio(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<CodigoVerificacion2FADTO> ObtenerCodigoActivoAsync(string usuarioId)
        {
            var ahora = DateTime.UtcNow;
            var registro = await _ctx.CodigosVerificacion2FA
                .FirstOrDefaultAsync(c =>
                    c.UsuarioId == usuarioId &&
                    !c.Utilizado &&
                    c.FechaExpiracion > ahora);

            if (registro == null)
                return null;

            return new CodigoVerificacion2FADTO
            {
                Id = registro.Id,
                UsuarioId = registro.UsuarioId,
                FechaCreacion = registro.FechaCreacion,
                FechaExpiracion = registro.FechaExpiracion,
                Utilizado = registro.Utilizado,
                IntentosFallidos = registro.IntentosFallidos
            };
        }

        public async Task CrearCodigoAsync(CodigoVerificacion2FADTO dto, string codigoHasheado)
        {
            var entidad = new CodigoVerificacion2FA
            {
                UsuarioId = dto.UsuarioId,
                CodigoHash = codigoHasheado,
                FechaCreacion = dto.FechaCreacion,
                FechaExpiracion = dto.FechaExpiracion,
                Utilizado = false,
                IntentosFallidos = 0,
                IpSolicitud = null
            };
            _ctx.CodigosVerificacion2FA.Add(entidad);
            await _ctx.SaveChangesAsync();
        }

        public async Task InvalidarCodigosAnterioresAsync(string usuarioId)
        {
            var codigos = await _ctx.CodigosVerificacion2FA
                .Where(c => c.UsuarioId == usuarioId && !c.Utilizado)
                .ToListAsync();

            foreach (var codigo in codigos)
                codigo.Utilizado = true;

            if (codigos.Count > 0)
                await _ctx.SaveChangesAsync();
        }

        public async Task<string> ObtenerHashAsync(int id)
        {
            var registro = await _ctx.CodigosVerificacion2FA.FindAsync(id);
            return registro?.CodigoHash;
        }

        public async Task MarcarComoUsadoAsync(int id)
        {
            var registro = await _ctx.CodigosVerificacion2FA.FindAsync(id);
            if (registro != null)
            {
                registro.Utilizado = true;
                await _ctx.SaveChangesAsync();
            }
        }

        public async Task IncrementarIntentosAsync(int id)
        {
            var registro = await _ctx.CodigosVerificacion2FA.FindAsync(id);
            if (registro != null)
            {
                registro.IntentosFallidos++;
                await _ctx.SaveChangesAsync();
            }
        }
    }
}
