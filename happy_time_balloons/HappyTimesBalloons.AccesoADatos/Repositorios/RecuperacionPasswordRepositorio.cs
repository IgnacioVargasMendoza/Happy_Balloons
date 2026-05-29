using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.AccesoADatos.Modelos;
using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace HappyTimesBalloons.AccesoADatos.Repositorios
{
    public class RecuperacionPasswordRepositorio : IRecuperacionPasswordRepositorio
    {
        private readonly ApplicationDbContext _ctx;

        public RecuperacionPasswordRepositorio(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task GuardarTokenAsync(string usuarioId, string token, DateTime expiracion)
        {
            _ctx.RecuperacionTokens.Add(new RecuperacionToken
            {
                UsuarioId = usuarioId,
                Token = token,
                FechaExpiracion = expiracion,
                Usado = false,
                FechaCreacion = DateTime.UtcNow
            });
            await _ctx.SaveChangesAsync();
        }

        public async Task<TokenRecuperacionDTO> ObtenerTokenValidoAsync(string token)
        {
            var registro = await _ctx.RecuperacionTokens
                .FirstOrDefaultAsync(r =>
                    r.Token == token &&
                    !r.Usado &&
                    r.FechaExpiracion > DateTime.UtcNow);

            if (registro == null)
                return null;

            return new TokenRecuperacionDTO
            {
                Id = registro.Id,
                UsuarioId = registro.UsuarioId
            };
        }

        public async Task MarcarComoUsadoAsync(int tokenId)
        {
            var registro = await _ctx.RecuperacionTokens.FindAsync(tokenId);
            if (registro == null)
                return;

            registro.Usado = true;
            await _ctx.SaveChangesAsync();
        }
    }
}
