using HappyTimesBalloons.Abstraccion.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Servicios
{
    public interface IProgramacionEntregaServicio
    {
        Task<List<HorarioDisponibleDTO>> ObtenerHorariosDisponiblesAsync(DateTime fecha);
        Task<List<ProgramacionEntregaDTO>> ObtenerTodasAsync();
        Task<ProgramacionEntregaDTO> ObtenerPorIdAsync(int id);
        Task<ProgramacionEntregaDTO> ObtenerPorPedidoIdAsync(int pedidoId);
        Task<ResultadoOperacionDTO> RegistrarAsync(ProgramacionEntregaDTO dto, string usuarioId, string nombreUsuario);
        Task<ResultadoOperacionDTO> CancelarAsync(int id, string usuarioId, string nombreUsuario);
        bool EsFechaPermitida(DateTime fecha, out string mensajeError);
    }
}
