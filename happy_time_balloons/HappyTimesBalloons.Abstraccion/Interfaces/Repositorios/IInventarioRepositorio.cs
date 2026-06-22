using HappyTimesBalloons.Abstraccion.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Repositorios
{
    public interface IInventarioRepositorio
    {
        Task<List<InventarioDTO>> ObtenerTodosAsync(string busqueda = null, int? categoriaId = null, string estadoStock = "todos");
        Task<InventarioKpisDTO> ObtenerKpisAsync();
        Task<InventarioDTO> ObtenerPorProductoIdAsync(int productoId);
        Task<int?> ObtenerStockActualAsync(int productoId);
        Task ActualizarStockAsync(int productoId, int nuevoStock, string usuarioId);
        Task<bool> ActualizarStockMinimoAsync(int inventarioId, int nuevoStockMinimo, string usuarioId);
    }
}
