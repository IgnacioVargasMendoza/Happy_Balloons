using HappyTimesBalloons.Abstraccion.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Servicios
{
    public interface IProductoServicio
    {
        Task<List<ProductoDTO>> ObtenerTodosAsync(string busqueda = null, int? categoriaId = null);
        Task<ProductoDTO> ObtenerPorIdAsync(int id);
        Task<ResultadoOperacionDTO> CrearAsync(ProductoDTO dto);
        Task<ResultadoOperacionDTO> ActualizarAsync(ProductoDTO dto);
        Task<ResultadoOperacionDTO> ToggleEstadoAsync(int id);
        Task<ResultadoOperacionDTO> AgregarImagenAsync(ImagenProductoDTO dto);
        Task<ResultadoOperacionDTO> EliminarImagenAsync(int imagenId);
        Task<ResultadoOperacionDTO> EstablecerImagenPrincipalAsync(int imagenId, int productoId);
    }
}
