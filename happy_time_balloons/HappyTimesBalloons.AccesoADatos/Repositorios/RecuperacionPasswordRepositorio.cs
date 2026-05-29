using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.AccesoADatos.Modelos;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace HappyTimesBalloons.AccesoADatos.Repositorios
{
    public class RecuperacionPasswordRepositorio : IRecuperacionPasswordRepositorio, IDisposable
    {
        private readonly ApplicationDbContext _ctx;
        private readonly UserManager<ApplicationUser> _userManager;

        public RecuperacionPasswordRepositorio(ApplicationDbContext ctx)
        {
            _ctx = ctx;
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(ctx));
        }

        public async Task<bool> ExisteEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email) != null;
        }

        public async Task<string> ObtenerUsuarioIdPorEmailAsync(string email)
        {
            var usuario = await _userManager.FindByEmailAsync(email);
            return usuario?.Id;
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
            var ahora = DateTime.UtcNow;
            var registro = await _ctx.RecuperacionTokens
                .FirstOrDefaultAsync(t => t.Token == token && !t.Usado && t.FechaExpiracion > ahora);

            if (registro == null) return null;

            return new TokenRecuperacionDTO
            {
                Id = registro.Id,
                Token = registro.Token,
                UsuarioId = registro.UsuarioId,
                FechaExpiracion = registro.FechaExpiracion,
                Usado = registro.Usado
            };
        }

        public async Task MarcarComoUsadoAsync(int tokenId)
        {
            var registro = await _ctx.RecuperacionTokens.FindAsync(tokenId);
            if (registro != null)
            {
                registro.Usado = true;
                await _ctx.SaveChangesAsync();
            }
        }

        public async Task<ResultadoOperacionDTO> RestablecerPasswordAsync(string usuarioId, string nuevaContrasena)
        {
            var removeResult = await _userManager.RemovePasswordAsync(usuarioId);
            if (!removeResult.Succeeded)
                return ResultadoOperacionDTO.Fallo(string.Join(" ", removeResult.Errors));

            var addResult = await _userManager.AddPasswordAsync(usuarioId, nuevaContrasena);
            if (!addResult.Succeeded)
                return ResultadoOperacionDTO.Fallo(string.Join(" ", addResult.Errors));

            return ResultadoOperacionDTO.Ok("Contraseña actualizada exitosamente.");
        }

        public void Dispose()
        {
            _userManager?.Dispose();
        }
    }
}
