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

        public Task<List<ZonaEntregaDTO>> ObtenerTodasIncluyendoInactivasAsync()
            => _repo.ObtenerTodasIncluyendoInactivasAsync();

        public async Task<ResultadoOperacionDTO> CrearAsync(ZonaEntregaDTO dto, string usuarioId, string nombreUsuario)
        {
            var nombreExiste = await _repo.ExisteNombreAsync(dto.Nombre);
            if (nombreExiste)
            {
                return ResultadoOperacionDTO.Fallo("Ya existe una zona de entrega con ese nombre.");
            }

            return await _repo.CrearAsync(dto, usuarioId, nombreUsuario);
        }

        public async Task<ResultadoOperacionDTO> ActualizarAsync(ZonaEntregaDTO dto, string usuarioId, string nombreUsuario)
        {
            var nombreExiste = await _repo.ExisteNombreAsync(dto.Nombre, dto.Id);
            if (nombreExiste)
            {
                return ResultadoOperacionDTO.Fallo("Ya existe una zona de entrega con ese nombre.");
            }

            return await _repo.ActualizarAsync(dto, usuarioId, nombreUsuario);
        }

        public Task<ResultadoOperacionDTO> CambiarDisponibilidadAsync(int id, bool esDisponible, string usuarioId, string nombreUsuario)
            => _repo.CambiarDisponibilidadAsync(id, esDisponible, usuarioId, nombreUsuario);
    }
}
