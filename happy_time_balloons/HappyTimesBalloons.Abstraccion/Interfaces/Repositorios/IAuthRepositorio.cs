using HappyTimesBalloons.Abstraccion.DTOs;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Repositorios
{
    public interface IAuthRepositorio
    {
        Task<UsuarioDTO> BuscarPorEmailAsync(string email);
        Task<ResultadoOperacionDTO> CrearCuentaAsync(RegistroDTO registro);
        Task AsignarRolAsync(string userId, string rol);
        Task<bool> EstaBloquedoAsync(string userId);
        Task<bool> VerificarPasswordAsync(string userId, string password);
        Task RegistrarIntentoFallidoAsync(string userId);
        Task<bool> SeBloqueoAsync(string userId);
        Task<int> ObtenerIntentosRestantesAsync(string userId);
        Task ResetearIntentosAsync(string userId);
        Task<bool> EsEnRolAsync(string userId, string rol);
    }
}
