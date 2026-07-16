using HappyTimesBalloons.Abstraccion.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Repositorios
{
    public interface IConfiguracionRepositorio
    {
        Task<List<ConfiguracionSistemaDTO>> ObtenerTodasAsync();
        Task<ConfiguracionSistemaDTO> ObtenerPorIdAsync(int id);
        Task<ConfiguracionSistemaDTO> ObtenerPorClaveAsync(string clave);
        Task<ResultadoOperacionDTO> ActualizarAsync(ConfiguracionSistemaDTO dto, string usuarioId, string nombreUsuario);
    }
}
