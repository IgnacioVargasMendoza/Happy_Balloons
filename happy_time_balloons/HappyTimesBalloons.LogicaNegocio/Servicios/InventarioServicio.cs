using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.LogicaNegocio.Servicios
{
    public class InventarioServicio : IInventarioServicio
    {
        private readonly IInventarioRepositorio _repo;

        public InventarioServicio(IInventarioRepositorio repo)
        {
            _repo = repo;
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
    }
}
