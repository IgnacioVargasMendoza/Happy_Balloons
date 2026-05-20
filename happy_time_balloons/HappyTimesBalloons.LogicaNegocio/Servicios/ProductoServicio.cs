using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.LogicaNegocio.Servicios
{
    public class ProductoServicio : IProductoServicio
    {
        private readonly IProductoRepositorio _repo;

        public ProductoServicio(IProductoRepositorio repo)
        {
            _repo = repo;
        }

        public Task<List<ProductoDTO>> ObtenerTodosAsync(string busqueda = null, int? categoriaId = null)
            => _repo.ObtenerTodosAsync(busqueda, categoriaId);

        public Task<ProductoDTO> ObtenerPorIdAsync(int id)
            => _repo.ObtenerPorIdAsync(id);

        public async Task<ResultadoOperacionDTO> CrearAsync(ProductoDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return ResultadoOperacionDTO.Fallo(
                    "El nombre del producto es obligatorio.", CodigoResultado.DatosInvalidos);

            if (dto.Precio <= 0)
                return ResultadoOperacionDTO.Fallo(
                    "El precio debe ser mayor a cero.", CodigoResultado.DatosInvalidos);

            if (dto.Stock < 0)
                return ResultadoOperacionDTO.Fallo(
                    "El stock no puede ser negativo.", CodigoResultado.DatosInvalidos);

            if (dto.PrecioDescuento.HasValue && dto.PrecioDescuento >= dto.Precio)
                return ResultadoOperacionDTO.Fallo(
                    "El precio con descuento debe ser menor al precio base.", CodigoResultado.DatosInvalidos);

            await _repo.CrearAsync(dto);
            return ResultadoOperacionDTO.Ok("Producto creado exitosamente.");
        }

        public async Task<ResultadoOperacionDTO> ActualizarAsync(ProductoDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return ResultadoOperacionDTO.Fallo(
                    "El nombre del producto es obligatorio.", CodigoResultado.DatosInvalidos);

            if (dto.Precio <= 0)
                return ResultadoOperacionDTO.Fallo(
                    "El precio debe ser mayor a cero.", CodigoResultado.DatosInvalidos);

            if (dto.PrecioDescuento.HasValue && dto.PrecioDescuento >= dto.Precio)
                return ResultadoOperacionDTO.Fallo(
                    "El precio con descuento debe ser menor al precio base.", CodigoResultado.DatosInvalidos);

            var actualizado = await _repo.ActualizarAsync(dto);
            if (actualizado == null)
                return ResultadoOperacionDTO.Fallo("Producto no encontrado.", CodigoResultado.Error);

            return ResultadoOperacionDTO.Ok("Producto actualizado exitosamente.");
        }

        public async Task<ResultadoOperacionDTO> ToggleEstadoAsync(int id)
        {
            var ok = await _repo.ToggleEstadoAsync(id);
            if (!ok)
                return ResultadoOperacionDTO.Fallo("Producto no encontrado.", CodigoResultado.Error);
            return ResultadoOperacionDTO.Ok("Estado del producto actualizado.");
        }

        public async Task<ResultadoOperacionDTO> AgregarImagenAsync(ImagenProductoDTO dto)
        {
            await _repo.AgregarImagenAsync(dto);
            return ResultadoOperacionDTO.Ok("Imagen agregada.");
        }

        public async Task<ResultadoOperacionDTO> EliminarImagenAsync(int imagenId)
        {
            var ok = await _repo.EliminarImagenAsync(imagenId);
            if (!ok)
                return ResultadoOperacionDTO.Fallo("Imagen no encontrada.", CodigoResultado.Error);
            return ResultadoOperacionDTO.Ok("Imagen eliminada.");
        }

        public async Task<ResultadoOperacionDTO> EstablecerImagenPrincipalAsync(int imagenId, int productoId)
        {
            await _repo.EstablecerImagenPrincipalAsync(imagenId, productoId);
            return ResultadoOperacionDTO.Ok("Imagen principal actualizada.");
        }
    }
}
