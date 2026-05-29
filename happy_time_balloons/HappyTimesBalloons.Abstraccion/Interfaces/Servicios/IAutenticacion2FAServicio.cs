using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Servicios
{
    public interface IAutenticacion2FAServicio
    {
        Task GenerarYEnviarCodigoAsync(string usuarioId, string email, string ip);
        Task<ResultadoVerificacion2FA> ValidarCodigoAsync(string usuarioId, string codigoIngresado, string ip);
        string EnmascararEmail(string email);
    }

    public enum ResultadoVerificacion2FA
    {
        Exitoso,
        CodigoIncorrecto,
        CodigoExpirado,
        MaximoIntentosAlcanzado,
        SinCodigoActivo
    }
}
