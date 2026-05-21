using HappyTimesBalloons.Abstraccion.DTOs;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Servicios
{
    public interface IAuthServicio
    {
        Task<ResultadoOperacionDTO> RegistrarAsync(RegistroDTO registro);
    }
}
