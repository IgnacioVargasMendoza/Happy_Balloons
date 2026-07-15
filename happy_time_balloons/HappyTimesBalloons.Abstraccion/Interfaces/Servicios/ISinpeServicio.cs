using HappyTimesBalloons.Abstraccion.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Servicios
{
    public interface ISinpeServicio
    {
        Task<ResultadoOperacionDTO> ProcesarWebhookAsync(SinpeWebhookDTO webhook, string ipOrigen);
        Task<IList<PagoSinpeDTO>> ObtenerPagosAsync();
        Task<PagoSinpeDTO> ObtenerPorIdAsync(int id);
    }
}
