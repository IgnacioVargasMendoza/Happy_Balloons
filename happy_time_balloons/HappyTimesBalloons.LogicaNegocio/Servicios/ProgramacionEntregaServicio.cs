using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.LogicaNegocio.Servicios
{
    public class ProgramacionEntregaServicio : IProgramacionEntregaServicio
    {
        private readonly IProgramacionEntregaRepositorio _repo;
        private readonly IHorarioEntregaRepositorio _horarioRepo;
        private readonly IConfiguracionServicio _configuracionServicio;
        private readonly IPedidoRepositorio _pedidoRepo;

        public ProgramacionEntregaServicio(
            IProgramacionEntregaRepositorio repo,
            IHorarioEntregaRepositorio horarioRepo,
            IConfiguracionServicio configuracionServicio,
            IPedidoRepositorio pedidoRepo)
        {
            _repo = repo;
            _horarioRepo = horarioRepo;
            _configuracionServicio = configuracionServicio;
            _pedidoRepo = pedidoRepo;
        }

        public Task<List<HorarioDisponibleDTO>> ObtenerHorariosDisponiblesAsync(DateTime fecha)
            => _horarioRepo.ObtenerDisponiblesParaFechaAsync(fecha);

        public Task<List<ProgramacionEntregaDTO>> ObtenerTodasAsync()
            => _repo.ObtenerTodasAsync();

        public Task<ProgramacionEntregaDTO> ObtenerPorIdAsync(int id)
            => _repo.ObtenerPorIdAsync(id);

        public Task<ProgramacionEntregaDTO> ObtenerPorPedidoIdAsync(int pedidoId)
            => _repo.ObtenerPorPedidoIdAsync(pedidoId);

        public async Task<ResultadoOperacionDTO> RegistrarAsync(
            ProgramacionEntregaDTO dto,
            string usuarioId,
            string nombreUsuario)
        {
            var validacion = await EsFechaPermitidaAsync(dto.FechaEntrega);
            if (!validacion.Exito)
            {
                return validacion;
            }

            var existente = await _repo.ObtenerPorPedidoIdAsync(dto.PedidoId);
            if (existente != null)
            {
                return ResultadoOperacionDTO.Fallo(
                    "Este pedido ya tiene una entrega programada. Cancélala primero para reprogramar.");
            }

            var reservas = await _repo.ContarReservasPorHorarioYFechaAsync(
                dto.HorarioEntregaId, dto.FechaEntrega);

            var horario = await _horarioRepo.ObtenerPorIdAsync(dto.HorarioEntregaId);
            if (horario == null)
            {
                return ResultadoOperacionDTO.Fallo("El horario de entrega seleccionado no existe.");
            }

            if (reservas >= horario.CapacidadMaxima)
            {
                return ResultadoOperacionDTO.Fallo(
                    $"El horario '{horario.Etiqueta}' no tiene cupos disponibles para esa fecha.");
            }

            var resultado = await _repo.RegistrarAsync(dto, usuarioId, nombreUsuario);
            if (resultado.Exito)
            {
                await _pedidoRepo.ActualizarEstadoAsync(dto.PedidoId, EstadoPedido.Procesando);
            }

            return resultado;
        }

        public async Task<ResultadoOperacionDTO> CancelarAsync(int id, string usuarioId, string nombreUsuario)
        {
            var programacion = await _repo.ObtenerPorIdAsync(id);
            if (programacion == null)
            {
                return ResultadoOperacionDTO.Fallo("La programación de entrega no existe.");
            }

            var resultado = await _repo.CancelarAsync(id, usuarioId, nombreUsuario);
            if (resultado.Exito)
            {
                await _pedidoRepo.ActualizarEstadoAsync(programacion.PedidoId, EstadoPedido.Procesando);
            }

            return resultado;
        }

        public async Task<ResultadoOperacionDTO> MarcarEntregadaAsync(int id, string usuarioId, string nombreUsuario)
        {
            var programacion = await _repo.ObtenerPorIdAsync(id);
            if (programacion == null)
            {
                return ResultadoOperacionDTO.Fallo("La programación de entrega no existe.");
            }

            var resultado = await _repo.MarcarEntregadaAsync(id, usuarioId, nombreUsuario);
            if (resultado.Exito)
            {
                await _pedidoRepo.ActualizarEstadoAsync(programacion.PedidoId, EstadoPedido.Entregado);
                return ResultadoOperacionDTO.Ok("Entrega completada. El pedido fue actualizado a Entregado.");
            }

            return resultado;
        }

        public async Task<ResultadoOperacionDTO> EsFechaPermitidaAsync(DateTime fecha)
        {
            var diasMinimos = await _configuracionServicio.ObtenerIntAsync("EntregaDiasMinimosAdelante", 1);
            var diasMaximos = await _configuracionServicio.ObtenerIntAsync("EntregaDiasMaximosAdelante", 30);
            var diasHabilitados = await _configuracionServicio.ObtenerListaEnteroAsync(
                "EntregaDiasHabilitados", new List<int> { 1, 2, 3, 4, 5, 6 });

            var hoy = DateTime.Today;
            var fechaSolo = fecha.Date;

            if (fechaSolo < hoy.AddDays(diasMinimos))
            {
                return ResultadoOperacionDTO.Fallo(
                    $"La fecha de entrega debe ser al menos {diasMinimos} día(s) desde hoy.");
            }

            if (fechaSolo > hoy.AddDays(diasMaximos))
            {
                return ResultadoOperacionDTO.Fallo(
                    $"La fecha de entrega no puede ser más de {diasMaximos} días en el futuro.");
            }

            if (!diasHabilitados.Contains((int)fechaSolo.DayOfWeek))
            {
                return ResultadoOperacionDTO.Fallo(
                    "No realizamos entregas ese día de la semana. Por favor elige otro día.");
            }

            return ResultadoOperacionDTO.Ok("Fecha permitida.");
        }
    }
}
