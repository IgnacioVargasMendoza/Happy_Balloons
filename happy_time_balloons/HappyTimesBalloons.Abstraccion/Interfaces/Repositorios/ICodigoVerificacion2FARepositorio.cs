using HappyTimesBalloons.Abstraccion.DTOs;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Repositorios
{
    public interface ICodigoVerificacion2FARepositorio
    {
        Task<CodigoVerificacion2FADTO> ObtenerCodigoActivoAsync(string usuarioId);
        Task<string> ObtenerHashAsync(int id);
        Task CrearCodigoAsync(CodigoVerificacion2FADTO dto, string codigoHasheado);
        Task InvalidarCodigosAnterioresAsync(string usuarioId);
        Task MarcarComoUsadoAsync(int id);
        Task IncrementarIntentosAsync(int id);
    }
}
