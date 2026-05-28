using HappyTimesBalloons.Abstraccion.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Repositorios
{
    public interface IBitacoraRepositorio
    {
        Task GuardarAsync(BitacoraEntradaDTO entrada);
        Task<List<BitacoraResumenDTO>> ObtenerActividadRecienteAsync(int cantidad);
    }
}
