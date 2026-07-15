using HappyTimesBalloons.Abstraccion.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Repositorios
{
    public interface IZonaEntregaRepositorio
    {
        Task<List<ZonaEntregaDTO>> ObtenerTodasAsync();
        Task<ZonaEntregaDTO> ObtenerPorIdAsync(int id);
        Task<List<ZonaEntregaDTO>> ObtenerTodasIncluyendoInactivasAsync();
        Task<ResultadoOperacionDTO> CrearAsync(ZonaEntregaDTO dto, string usuarioId, string nombreUsuario);
        Task<ResultadoOperacionDTO> ActualizarAsync(ZonaEntregaDTO dto, string usuarioId, string nombreUsuario);
        Task<ResultadoOperacionDTO> CambiarDisponibilidadAsync(int id, bool esDisponible, string usuarioId, string nombreUsuario);
        Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null);
    }
}
