using HappyTimesBalloons.Abstraccion.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Servicios
{
    public interface ICategoriaServicio
    {
        Task<List<CategoriaDTO>> ObtenerTodasAsync(string busqueda = null);
        Task<CategoriaDTO> ObtenerPorIdAsync(int id);
        Task<ResultadoOperacionDTO> CrearAsync(CategoriaDTO dto);
        Task<ResultadoOperacionDTO> ActualizarAsync(CategoriaDTO dto);
        Task<ResultadoOperacionDTO> ToggleEstadoAsync(int id);
    }
}
