using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Servicios
{
    public interface IPrediccionDemandaServicio
    {
        Task<List<PrediccionDemandaItemDTO>> ObtenerPrediccionesAsync(TipoPeriodo tipoPeriodo);
        Task<PrediccionDemandaDetalleDTO> ObtenerDetallePrediccionAsync(int productoId, TipoPeriodo tipoPeriodo);
    }
}
