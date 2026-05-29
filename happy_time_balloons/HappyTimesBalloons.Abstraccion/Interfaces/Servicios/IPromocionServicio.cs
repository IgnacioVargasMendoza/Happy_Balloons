using HappyTimesBalloons.Abstraccion.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Servicios
{
    public interface IPromocionServicio
    {
        Task<List<PromocionDTO>> ObtenerTodasAsync();
        Task<ResultadoOperacionDTO> CrearAsync(PromocionDTO dto);
        Task<ResultadoOperacionDTO> EliminarAsync(int id);
    }
}
