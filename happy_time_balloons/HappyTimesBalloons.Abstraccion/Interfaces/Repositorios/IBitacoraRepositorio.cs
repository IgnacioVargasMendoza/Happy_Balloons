using HappyTimesBalloons.Abstraccion.DTOs;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Repositorios
{
    public interface IBitacoraRepositorio
    {
        Task GuardarAsync(BitacoraEntradaDTO entrada);
    }
}
