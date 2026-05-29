using HappyTimesBalloons.Abstraccion.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Repositorios
{
    public interface IPromocionRepositorio
    {
        Task<List<PromocionDTO>> ObtenerTodasAsync();
        Task<PromocionDTO> ObtenerPorIdAsync(int id);
        Task<PromocionDTO> CrearAsync(PromocionDTO dto);
        Task<bool> EliminarAsync(int id);
    }
}
