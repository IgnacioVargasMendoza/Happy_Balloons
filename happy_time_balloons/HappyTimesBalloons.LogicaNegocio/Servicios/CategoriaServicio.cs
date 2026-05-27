using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.LogicaNegocio.Servicios
{
    public class CategoriaServicio : ICategoriaServicio
    {
        private readonly ICategoriaRepositorio _repo;

        public CategoriaServicio(ICategoriaRepositorio repo)
        {
            _repo = repo;
        }

        public Task<List<CategoriaDTO>> ObtenerTodasAsync(string busqueda = null)
        {
            return _repo.ObtenerTodasAsync(busqueda);
        }

        public Task<CategoriaDTO> ObtenerPorIdAsync(int id)
        {
            return _repo.ObtenerPorIdAsync(id);
        }

        public async Task<ResultadoOperacionDTO> CrearAsync(CategoriaDTO dto)
        {
            if (dto == null)
            {
                return ResultadoOperacionDTO.Fallo(
                    "Los datos de la categoría son obligatorios.",
                    CodigoResultado.DatosInvalidos);
            }

            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                return ResultadoOperacionDTO.Fallo(
                    "El nombre de la categoría es obligatorio.",
                    CodigoResultado.DatosInvalidos);
            }

            if (dto.Nombre.Trim().Length > 200)
            {
                return ResultadoOperacionDTO.Fallo(
                    "El nombre no puede superar los 200 caracteres.",
                    CodigoResultado.DatosInvalidos);
            }

            if (!string.IsNullOrWhiteSpace(dto.Descripcion) && dto.Descripcion.Trim().Length > 1000)
            {
                return ResultadoOperacionDTO.Fallo(
                    "La descripción no puede superar los 1000 caracteres.",
                    CodigoResultado.DatosInvalidos);
            }

            if (await _repo.ExisteNombreAsync(dto.Nombre))
            {
                return ResultadoOperacionDTO.Fallo(
                    "Ya existe una categoría con ese nombre.",
                    CodigoResultado.DatosInvalidos);
            }

            await _repo.CrearAsync(dto);

            return ResultadoOperacionDTO.Ok("Categoría creada exitosamente.");
        }

        public async Task<ResultadoOperacionDTO> ActualizarAsync(CategoriaDTO dto)
        {
            if (dto == null)
            {
                return ResultadoOperacionDTO.Fallo(
                    "Los datos de la categoría son obligatorios.",
                    CodigoResultado.DatosInvalidos);
            }

            if (dto.Id <= 0)
            {
                return ResultadoOperacionDTO.Fallo(
                    "La categoría seleccionada no es válida.",
                    CodigoResultado.DatosInvalidos);
            }

            if (string.IsNullOrWhiteSpace(dto.Nombre))
            {
                return ResultadoOperacionDTO.Fallo(
                    "El nombre de la categoría es obligatorio.",
                    CodigoResultado.DatosInvalidos);
            }

            if (dto.Nombre.Trim().Length > 200)
            {
                return ResultadoOperacionDTO.Fallo(
                    "El nombre no puede superar los 200 caracteres.",
                    CodigoResultado.DatosInvalidos);
            }

            if (!string.IsNullOrWhiteSpace(dto.Descripcion) && dto.Descripcion.Trim().Length > 1000)
            {
                return ResultadoOperacionDTO.Fallo(
                    "La descripción no puede superar los 1000 caracteres.",
                    CodigoResultado.DatosInvalidos);
            }

            if (await _repo.ExisteNombreAsync(dto.Nombre, dto.Id))
            {
                return ResultadoOperacionDTO.Fallo(
                    "Ya existe otra categoría con ese nombre.",
                    CodigoResultado.DatosInvalidos);
            }

            var actualizada = await _repo.ActualizarAsync(dto);

            if (actualizada == null)
            {
                return ResultadoOperacionDTO.Fallo(
                    "Categoría no encontrada.",
                    CodigoResultado.Error);
            }

            return ResultadoOperacionDTO.Ok("Categoría actualizada exitosamente.");
        }

        public async Task<ResultadoOperacionDTO> ToggleEstadoAsync(int id)
        {
            if (id <= 0)
            {
                return ResultadoOperacionDTO.Fallo(
                    "La categoría seleccionada no es válida.",
                    CodigoResultado.DatosInvalidos);
            }

            var resultado = await _repo.ToggleEstadoAsync(id);

            if (!resultado)
            {
                return ResultadoOperacionDTO.Fallo(
                    "Categoría no encontrada.",
                    CodigoResultado.Error);
            }

            return ResultadoOperacionDTO.Ok("Estado de la categoría actualizado.");
        }
    }
}