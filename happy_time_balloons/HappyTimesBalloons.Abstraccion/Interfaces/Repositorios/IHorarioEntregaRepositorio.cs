using HappyTimesBalloons.Abstraccion.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Repositorios
{
    public interface IHorarioEntregaRepositorio
    {
        Task<List<HorarioEntregaDTO>> ObtenerTodosAsync();
        Task<HorarioEntregaDTO> ObtenerPorIdAsync(int id);
        Task<List<HorarioDisponibleDTO>> ObtenerDisponiblesParaFechaAsync(DateTime fecha);
    }
}
