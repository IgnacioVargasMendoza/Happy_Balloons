using HappyTimesBalloons.Abstraccion.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Repositorios
{
    public interface IRolRepositorio
    {
        Task<List<RolDTO>> ObtenerTodosAsync();
        Task<RolDTO> ObtenerPorIdAsync(string id);
        Task<bool> ExisteNombreAsync(string nombre);
        Task<string> CrearAsync(string nombre);
        Task<bool> ActualizarAsync(string id, string nuevoNombre);
        Task<bool> EliminarAsync(string id);
        Task<List<UsuarioConRolDTO>> ObtenerTodosLosUsuariosAsync();
        Task<bool> TieneRolAsync(string usuarioId, string rolNombre);
        Task<bool> AsignarRolAsync(string usuarioId, string rolNombre);
        Task<bool> RevocarRolAsync(string usuarioId, string rolNombre);
    }
}
