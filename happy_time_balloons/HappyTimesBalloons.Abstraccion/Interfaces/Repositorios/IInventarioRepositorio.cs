using HappyTimesBalloons.Abstraccion.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Repositorios
{
    public interface IInventarioRepositorio
    {
        Task<List<InventarioDTO>> ObtenerTodosAsync(string busqueda = null, int? categoriaId = null, string estadoStock = "todos");
        Task<InventarioKpisDTO> ObtenerKpisAsync();
    }
}
