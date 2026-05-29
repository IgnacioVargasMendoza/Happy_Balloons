using HappyTimesBalloons.Abstraccion.DTOs;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Servicios
{
    public interface IRecuperacionPasswordServicio
    {
        Task<ResultadoOperacionDTO<string>> SolicitarRecuperacionAsync(string email);
        Task<TokenRecuperacionDTO> ValidarTokenAsync(string token);
        Task<ResultadoOperacionDTO> RestablecerContrasenaAsync(string token, string nuevaContrasena);
    }
}
