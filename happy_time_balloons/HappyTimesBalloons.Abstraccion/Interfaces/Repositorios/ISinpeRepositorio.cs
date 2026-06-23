using HappyTimesBalloons.Abstraccion.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Repositorios
{
    public interface ISinpeRepositorio
    {
        Task<PagoSinpeDTO> RegistrarAsync(PagoSinpeDTO dto);
        Task<bool> ExisteComprobanteAsync(string numeroComprobante);
        Task<IList<PagoSinpeDTO>> ObtenerTodosAsync();
        Task<PagoSinpeDTO> ObtenerPorIdAsync(int id);
    }
}
