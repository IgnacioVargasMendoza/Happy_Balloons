using HappyTimesBalloons.Abstraccion.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Repositorios
{
    public interface IProgramacionEntregaRepositorio
    {
        Task<List<ProgramacionEntregaDTO>> ObtenerTodasAsync();
        Task<ProgramacionEntregaDTO> ObtenerPorIdAsync(int id);
        Task<ProgramacionEntregaDTO> ObtenerPorPedidoIdAsync(int pedidoId);
        Task<int> ContarReservasPorHorarioYFechaAsync(int horarioEntregaId, DateTime fecha);
        Task<ResultadoOperacionDTO> RegistrarAsync(ProgramacionEntregaDTO dto, string usuarioId, string nombreUsuario);
        Task<ResultadoOperacionDTO> CancelarAsync(int id, string usuarioId, string nombreUsuario);
    }
}
