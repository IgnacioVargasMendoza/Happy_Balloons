using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.LogicaNegocio.Servicios
{
    public class PromocionServicio : IPromocionServicio
    {
        private readonly IPromocionRepositorio _repo;

        public PromocionServicio(IPromocionRepositorio repo)
        {
            _repo = repo;
        }

        public Task<List<PromocionDTO>> ObtenerTodasAsync() => _repo.ObtenerTodasAsync();

        public async Task<ResultadoOperacionDTO> CrearAsync(PromocionDTO dto)
        {
            if (dto.ProductoId <= 0)
                return ResultadoOperacionDTO.Fallo(
                    "Debe seleccionar un producto.", CodigoResultado.DatosInvalidos);

            if (dto.DescuentoPorcentaje <= 0 || dto.DescuentoPorcentaje > 100)
                return ResultadoOperacionDTO.Fallo(
                    "El descuento debe estar entre 1% y 100%.", CodigoResultado.DatosInvalidos);

            if (dto.FechaFin <= dto.FechaInicio)
                return ResultadoOperacionDTO.Fallo(
                    "La fecha de fin debe ser posterior a la fecha de inicio.", CodigoResultado.DatosInvalidos);

            if (dto.FechaFin.Date < DateTime.UtcNow.Date)
                return ResultadoOperacionDTO.Fallo(
                    "La fecha de fin no puede ser en el pasado.", CodigoResultado.DatosInvalidos);

            await _repo.CrearAsync(dto);
            return ResultadoOperacionDTO.Ok("Promoción creada exitosamente.");
        }

        public async Task<ResultadoOperacionDTO> EliminarAsync(int id)
        {
            var ok = await _repo.EliminarAsync(id);
            if (!ok)
                return ResultadoOperacionDTO.Fallo("Promoción no encontrada.", CodigoResultado.Error);

            return ResultadoOperacionDTO.Ok("Promoción eliminada exitosamente.");
        }
    }
}
