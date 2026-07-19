using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Repositorios
{
    public interface IPrediccionDemandaRepositorio
    {
        Task<List<PeriodoVentaDTO>> ObtenerHistorialVentasPorProductoAsync(int productoId, TipoPeriodo tipoPeriodo);
        Task<List<int>> ObtenerIdsProductosConVentasAsync(TipoPeriodo tipoPeriodo);
        Task<string> ObtenerNombreProductoAsync(int productoId);
        Task<string> ObtenerCategoriaProductoAsync(int productoId);
    }
}
