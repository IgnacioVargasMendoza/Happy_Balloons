using HappyTimesBalloons.Abstraccion.DTOs;
using System;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Servicios
{
    public interface IRankingProductosServicio
    {
        Task<RankingProductosDTO> ObtenerRankingAsync(
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int? zonaId);
    }
}
