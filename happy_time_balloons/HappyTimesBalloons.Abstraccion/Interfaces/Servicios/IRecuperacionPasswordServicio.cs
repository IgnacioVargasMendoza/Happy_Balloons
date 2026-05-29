using HappyTimesBalloons.Abstraccion.DTOs;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Servicios
{
    public interface IRecuperacionPasswordServicio
    {
        Task<ResultadoOperacionDTO> SolicitarRecuperacionAsync(RecuperacionPasswordDTO solicitud);
        Task<bool> ValidarTokenAsync(string token);
        Task<ResultadoOperacionDTO> RestablecerContrasenaAsync(RestablecerPasswordDTO datos);
    }
}
