using HappyTimesBalloons.Abstraccion.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Servicios
{
    public interface IMovimientoInventarioServicio
    {
        Task<ResultadoOperacionDTO> RegistrarMovimientoAsync(MovimientoInventarioDTO dto, string usuarioId, string nombreUsuario);
        Task<List<MovimientoInventarioDTO>> ObtenerHistorialPorProductoAsync(int productoId);
        Task<List<MovimientoInventarioDTO>> ObtenerUltimosMovimientosAsync(int cantidad = 20);
    }
}
