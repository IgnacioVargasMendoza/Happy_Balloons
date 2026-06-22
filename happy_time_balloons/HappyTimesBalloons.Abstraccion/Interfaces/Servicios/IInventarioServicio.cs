using HappyTimesBalloons.Abstraccion.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Servicios
{
    public interface IInventarioServicio
    {
        Task<List<InventarioDTO>> ObtenerTodosAsync(string busqueda = null, int? categoriaId = null, string estadoStock = "todos");
        Task<InventarioKpisDTO> ObtenerKpisAsync();
        Task<InventarioDTO> ObtenerPorProductoIdAsync(int productoId);
    }
}
