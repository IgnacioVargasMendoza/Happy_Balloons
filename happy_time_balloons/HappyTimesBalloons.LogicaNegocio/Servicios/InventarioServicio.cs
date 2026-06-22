using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.LogicaNegocio.Servicios
{
    public class InventarioServicio : IInventarioServicio
    {
        private readonly IInventarioRepositorio _repo;
        private readonly IBitacoraRepositorio _bitacora;

        public InventarioServicio(IInventarioRepositorio repo, IBitacoraRepositorio bitacora)
        {
            _repo = repo;
            _bitacora = bitacora;
        }

        public Task<List<InventarioDTO>> ObtenerTodosAsync(
            string busqueda = null,
            int? categoriaId = null,
            string estadoStock = "todos")
            => _repo.ObtenerTodosAsync(busqueda, categoriaId, estadoStock);

        public Task<InventarioKpisDTO> ObtenerKpisAsync()
            => _repo.ObtenerKpisAsync();

        public Task<InventarioDTO> ObtenerPorProductoIdAsync(int productoId)
            => _repo.ObtenerPorProductoIdAsync(productoId);

        public async Task<ResultadoOperacionDTO> ActualizarStockMinimoAsync(
            int inventarioId,
            int nuevoStockMinimo,
            string usuarioId,
            string nombreUsuario)
        {
            if (nuevoStockMinimo < 0)
            {
                return ResultadoOperacionDTO.Fallo(
                    "El stock mínimo no puede ser negativo.",
                    CodigoResultado.DatosInvalidos);
            }

            var actualizado = await _repo.ActualizarStockMinimoAsync(inventarioId, nuevoStockMinimo, usuarioId);

            if (!actualizado)
            {
                return ResultadoOperacionDTO.Fallo(
                    "No se encontró el registro de inventario.",
                    CodigoResultado.DatosInvalidos);
            }

            await _bitacora.GuardarAsync(new BitacoraEntradaDTO
            {
                UsuarioId = usuarioId,
                NombreUsuario = nombreUsuario,
                Accion = TipoOperacion.Actualizar,
                TablaAfectada = "Inventario",
                RegistroId = inventarioId,
                Detalle = $"Stock mínimo actualizado a {nuevoStockMinimo} unidades."
            });

            return ResultadoOperacionDTO.Ok("Stock mínimo actualizado correctamente.");
        }
    }
}
