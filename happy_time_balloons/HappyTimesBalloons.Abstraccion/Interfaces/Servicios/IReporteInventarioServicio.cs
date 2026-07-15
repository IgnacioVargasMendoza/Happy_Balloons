using HappyTimesBalloons.Abstraccion.DTOs;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Servicios
{
    public interface IReporteInventarioServicio
    {
        Task<ReporteInventarioDTO> ObtenerReporteAsync(int? categoriaId = null, string estadoStock = "todos");
        byte[] GenerarCsvBytes(ReporteInventarioDTO reporte);
    }
}
