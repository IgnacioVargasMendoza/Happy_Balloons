using HappyTimesBalloons.Abstraccion.DTOs;
using System;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Repositorios
{
    public interface IReporteVentasRepositorio
    {
        Task<ReporteVentasDTO> ObtenerReporteAsync(DateTime fechaInicio, DateTime fechaFin);
    }
}
