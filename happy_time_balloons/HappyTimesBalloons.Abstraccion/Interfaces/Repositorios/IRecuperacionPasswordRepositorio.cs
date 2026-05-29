using HappyTimesBalloons.Abstraccion.DTOs;
using System;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Repositorios
{
    public interface IRecuperacionPasswordRepositorio
    {
        Task<bool> ExisteEmailAsync(string email);
        Task<string> ObtenerUsuarioIdPorEmailAsync(string email);
        Task GuardarTokenAsync(string usuarioId, string token, DateTime expiracion);
        Task<TokenRecuperacionDTO> ObtenerTokenValidoAsync(string token);
        Task MarcarComoUsadoAsync(int tokenId);
        Task<ResultadoOperacionDTO> RestablecerPasswordAsync(string usuarioId, string nuevaContrasena);
    }
}
