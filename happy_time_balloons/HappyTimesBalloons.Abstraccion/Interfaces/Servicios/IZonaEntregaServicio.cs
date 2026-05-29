using HappyTimesBalloons.Abstraccion.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Servicios
{
    public interface IZonaEntregaServicio
    {
        Task<List<ZonaEntregaDTO>> ObtenerTodasAsync();
        Task<ZonaEntregaDTO> ObtenerPorIdAsync(int id);
    }
}
