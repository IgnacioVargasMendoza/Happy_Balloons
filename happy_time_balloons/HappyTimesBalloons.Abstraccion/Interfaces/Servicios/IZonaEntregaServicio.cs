using HappyTimesBalloons.Abstraccion.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Servicios
{
    public interface IZonaEntregaServicio
    {
        Task<List<ZonaEntregaDTO>> ObtenerTodasAsync();
        Task<ZonaEntregaDTO> ObtenerPorIdAsync(int id);
        Task<List<ZonaEntregaDTO>> ObtenerTodasIncluyendoInactivasAsync();
        Task<ResultadoOperacionDTO> CrearAsync(ZonaEntregaDTO dto, string usuarioId, string nombreUsuario);
        Task<ResultadoOperacionDTO> ActualizarAsync(ZonaEntregaDTO dto, string usuarioId, string nombreUsuario);
        Task<ResultadoOperacionDTO> CambiarDisponibilidadAsync(int id, bool esDisponible, string usuarioId, string nombreUsuario);
    }
}
