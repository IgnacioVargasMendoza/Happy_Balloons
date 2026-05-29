using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.LogicaNegocio.Servicios
{
    public class ZonaEntregaServicio : IZonaEntregaServicio
    {
        private readonly IZonaEntregaRepositorio _repo;

        public ZonaEntregaServicio(IZonaEntregaRepositorio repo)
        {
            _repo = repo;
        }

        public Task<List<ZonaEntregaDTO>> ObtenerTodasAsync() => _repo.ObtenerTodasAsync();

        public Task<ZonaEntregaDTO> ObtenerPorIdAsync(int id) => _repo.ObtenerPorIdAsync(id);
    }
}
