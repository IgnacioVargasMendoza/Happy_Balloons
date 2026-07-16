using HappyTimesBalloons.Abstraccion.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Repositorios
{
    public interface IRankingProductosRepositorio
    {
        Task<List<RankingProductoItemDTO>> ObtenerItemsAsync(
            DateTime fechaInicio,
            DateTime fechaFin,
            int? zonaId);
    }
}
