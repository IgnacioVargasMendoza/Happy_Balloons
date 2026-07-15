using HappyTimesBalloons.Abstraccion.DTOs;
using System;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Servicios
{
    public interface IReporteVentasServicio
    {
        Task<ReporteVentasDTO> ObtenerReporteAsync(DateTime fechaInicio, DateTime fechaFin);
        byte[] GenerarCsvBytes(ReporteVentasDTO reporte);
    }
}
