using HappyTimesBalloons.Abstraccion.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Servicios
{
    public interface IRolServicio
    {
        Task<List<RolDTO>> ObtenerTodosAsync();
        Task<RolDTO> ObtenerPorIdAsync(string id);
        Task<ResultadoOperacionDTO<string>> CrearAsync(string nombre, string adminId, string adminNombre, string ip);
        Task<ResultadoOperacionDTO> ActualizarAsync(string id, string nuevoNombre, string adminId, string adminNombre, string ip);
        Task<ResultadoOperacionDTO> EliminarAsync(string id, string adminId, string adminNombre, string ip);
        Task<List<UsuarioConRolDTO>> ObtenerTodosLosUsuariosAsync();
        Task<ResultadoOperacionDTO> AsignarRolAsync(string usuarioId, string rolNombre, string adminId, string adminNombre, string ip);
        Task<ResultadoOperacionDTO> RevocarRolAsync(string usuarioId, string rolNombre, string adminId, string adminNombre, string ip);
    }
}
