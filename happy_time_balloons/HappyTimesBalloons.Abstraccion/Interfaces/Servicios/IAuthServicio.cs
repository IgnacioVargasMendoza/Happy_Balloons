using HappyTimesBalloons.Abstraccion.DTOs;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Servicios
{
    public interface IAuthServicio
    {
        Task<ResultadoOperacionDTO> RegistrarAsync(RegistroDTO registro);
        Task<ResultadoLoginDTO> ValidarCredencialesAsync(string email, string password);
        Task<UsuarioDTO> ObtenerPorIdAsync(string userId);
        Task<ResultadoOperacionDTO> EditarPerfilAsync(EditarPerfilDTO dto, string adminId, string adminNombre, string ip);
        Task<ResultadoOperacionDTO> CambiarContrasenaAsync(string userId, string contrasenaActual, string nuevaContrasena, string ip);
    }
}
