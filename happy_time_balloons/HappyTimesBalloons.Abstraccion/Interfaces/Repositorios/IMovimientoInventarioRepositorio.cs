using HappyTimesBalloons.Abstraccion.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Repositorios
{
    public interface IMovimientoInventarioRepositorio
    {
        Task<MovimientoInventarioDTO> RegistrarAsync(MovimientoInventarioDTO dto);
        Task<List<MovimientoInventarioDTO>> ObtenerPorProductoAsync(int productoId);
        Task<List<MovimientoInventarioDTO>> ObtenerUltimosAsync(int cantidad);
    }
}
