using HappyTimesBalloons.Abstraccion.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Repositorios
{
    public interface IProductoRepositorio
    {
        Task<List<ProductoDTO>> ObtenerTodosAsync(string busqueda = null, int? categoriaId = null);
        Task<ProductoDTO> ObtenerPorIdAsync(int id);
        Task<ProductoEstadisticasDTO> ObtenerEstadisticasAsync();
        Task<ImagenProductoDTO> ObtenerImagenPorIdAsync(int imagenId);
        Task<ProductoDTO> CrearAsync(ProductoDTO dto);
        Task<ProductoDTO> ActualizarAsync(ProductoDTO dto);
        Task<bool> ToggleEstadoAsync(int id);
        Task<ImagenProductoDTO> AgregarImagenAsync(ImagenProductoDTO dto);
        Task<bool> EliminarImagenAsync(int imagenId);
        Task<bool> EstablecerImagenPrincipalAsync(int imagenId, int productoId);
    }
}
