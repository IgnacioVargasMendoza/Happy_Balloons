using HappyTimesBalloons.Abstraccion.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Repositorios
{
    public interface ICategoriaRepositorio
    {
        Task<List<CategoriaDTO>> ObtenerTodasAsync(string busqueda = null);

        Task<CategoriaDTO> ObtenerPorIdAsync(int id);

        Task<CategoriaEstadisticasDTO> ObtenerEstadisticasAsync();

        Task<CategoriaDTO> CrearAsync(CategoriaDTO dto);

        Task<CategoriaDTO> ActualizarAsync(CategoriaDTO dto);

        Task<bool> ToggleEstadoAsync(int id);

        Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null);
    }
}