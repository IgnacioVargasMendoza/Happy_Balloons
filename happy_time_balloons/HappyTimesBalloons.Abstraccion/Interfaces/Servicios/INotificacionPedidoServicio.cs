using HappyTimesBalloons.Abstraccion.DTOs;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Servicios
{
    public interface INotificacionPedidoServicio
    {
        Task<ResultadoOperacionDTO> EnviarConfirmacionAsync(PedidoDTO pedido, string emailDestinatario);
    }
}
