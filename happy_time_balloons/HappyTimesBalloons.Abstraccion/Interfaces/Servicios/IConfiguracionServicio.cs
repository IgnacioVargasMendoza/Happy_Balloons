using HappyTimesBalloons.Abstraccion.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Servicios
{
    public interface IConfiguracionServicio
    {
        Task<List<ConfiguracionSistemaDTO>> ObtenerTodasAsync();
        Task<ConfiguracionSistemaDTO> ObtenerPorIdAsync(int id);
        Task<ConfiguracionSistemaDTO> ObtenerPorClaveAsync(string clave);
        Task<ResultadoOperacionDTO> ActualizarAsync(ConfiguracionSistemaDTO dto, string usuarioId, string nombreUsuario);
        Task<int> ObtenerIntAsync(string clave, int valorDefault);
        Task<decimal> ObtenerDecimalAsync(string clave, decimal valorDefault);
        Task<string> ObtenerStringAsync(string clave, string valorDefault);
        Task<List<int>> ObtenerListaEnteroAsync(string clave, List<int> valorDefault);
    }
}
