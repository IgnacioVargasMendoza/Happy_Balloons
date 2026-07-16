using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTimesBalloons.LogicaNegocio.Servicios
{
    public class ConfiguracionServicio : IConfiguracionServicio
    {
        private readonly IConfiguracionRepositorio _repo;

        public ConfiguracionServicio(IConfiguracionRepositorio repo)
        {
            _repo = repo;
        }

        public Task<List<ConfiguracionSistemaDTO>> ObtenerTodasAsync() => _repo.ObtenerTodasAsync();

        public Task<ConfiguracionSistemaDTO> ObtenerPorIdAsync(int id) => _repo.ObtenerPorIdAsync(id);

        public Task<ConfiguracionSistemaDTO> ObtenerPorClaveAsync(string clave) => _repo.ObtenerPorClaveAsync(clave);

        public Task<ResultadoOperacionDTO> ActualizarAsync(ConfiguracionSistemaDTO dto, string usuarioId, string nombreUsuario)
            => _repo.ActualizarAsync(dto, usuarioId, nombreUsuario);

        public async Task<int> ObtenerIntAsync(string clave, int valorDefault)
        {
            var dto = await _repo.ObtenerPorClaveAsync(clave);
            if (dto == null || string.IsNullOrWhiteSpace(dto.Valor))
            {
                return valorDefault;
            }

            return int.TryParse(dto.Valor, out var resultado) ? resultado : valorDefault;
        }

        public async Task<decimal> ObtenerDecimalAsync(string clave, decimal valorDefault)
        {
            var dto = await _repo.ObtenerPorClaveAsync(clave);
            if (dto == null || string.IsNullOrWhiteSpace(dto.Valor))
            {
                return valorDefault;
            }

            return decimal.TryParse(dto.Valor, out var resultado) ? resultado : valorDefault;
        }

        public async Task<string> ObtenerStringAsync(string clave, string valorDefault)
        {
            var dto = await _repo.ObtenerPorClaveAsync(clave);
            if (dto == null || string.IsNullOrWhiteSpace(dto.Valor))
            {
                return valorDefault;
            }

            return dto.Valor;
        }

        public async Task<List<int>> ObtenerListaEnteroAsync(string clave, List<int> valorDefault)
        {
            var dto = await _repo.ObtenerPorClaveAsync(clave);
            if (dto == null || string.IsNullOrWhiteSpace(dto.Valor))
            {
                return valorDefault;
            }

            try
            {
                var lista = dto.Valor
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => int.Parse(s.Trim()))
                    .ToList();

                return lista.Count > 0 ? lista : valorDefault;
            }
            catch (FormatException)
            {
                return valorDefault;
            }
        }
    }
}
