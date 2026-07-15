using HappyTimesBalloons.Abstraccion.DTOs;
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

        private const int DiasMinimosAdelante = 1;
        private const int DiasMaximosAdelante = 30;

        public ProgramacionEntregaServicio(
            IProgramacionEntregaRepositorio repo,
            IHorarioEntregaRepositorio horarioRepo)
        {
            _repo = repo;
            _horarioRepo = horarioRepo;
        }

        // Tarea 2: retorna los horarios con cupos reales para una fecha dada
        public Task<List<HorarioDisponibleDTO>> ObtenerHorariosDisponiblesAsync(DateTime fecha)
            => _horarioRepo.ObtenerDisponiblesParaFechaAsync(fecha);

        public Task<List<ProgramacionEntregaDTO>> ObtenerTodasAsync()
            => _repo.ObtenerTodasAsync();

        public Task<ProgramacionEntregaDTO> ObtenerPorIdAsync(int id)
            => _repo.ObtenerPorIdAsync(id);

        public Task<ProgramacionEntregaDTO> ObtenerPorPedidoIdAsync(int pedidoId)
            => _repo.ObtenerPorPedidoIdAsync(pedidoId);

        // Tarea 3 + 4: registrar con validación completa antes de persistir
        public async Task<ResultadoOperacionDTO> RegistrarAsync(
            ProgramacionEntregaDTO dto,
            string usuarioId,
            string nombreUsuario)
        {
            // Tarea 4: validar la fecha antes de continuar
            if (!EsFechaPermitida(dto.FechaEntrega, out var mensajeError))
            {
                return ResultadoOperacionDTO.Fallo(mensajeError);
            }

            // Verificar que el pedido no tenga ya una programación activa
            var existente = await _repo.ObtenerPorPedidoIdAsync(dto.PedidoId);
            if (existente != null)
            {
                return ResultadoOperacionDTO.Fallo(
                    "Este pedido ya tiene una entrega programada. Cancélala primero para reprogramar.");
            }

            // Tarea 5: verificar que el horario tenga cupo disponible
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

            return await _repo.RegistrarAsync(dto, usuarioId, nombreUsuario);
        }

        public Task<ResultadoOperacionDTO> CancelarAsync(int id, string usuarioId, string nombreUsuario)
            => _repo.CancelarAsync(id, usuarioId, nombreUsuario);

        // Tarea 4: reglas de negocio para fechas permitidas
        public bool EsFechaPermitida(DateTime fecha, out string mensajeError)
        {
            var hoy = DateTime.Today;
            var fechaSolo = fecha.Date;

            if (fechaSolo < hoy.AddDays(DiasMinimosAdelante))
            {
                mensajeError = "La fecha de entrega debe ser al menos mañana.";
                return false;
            }

            if (fechaSolo > hoy.AddDays(DiasMaximosAdelante))
            {
                mensajeError = $"La fecha de entrega no puede ser más de {DiasMaximosAdelante} días en el futuro.";
                return false;
            }

            if (fechaSolo.DayOfWeek == DayOfWeek.Sunday)
            {
                mensajeError = "No realizamos entregas los domingos. Por favor elige otro día.";
                return false;
            }

            mensajeError = null;
            return true;
        }
    }
}
