using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Servicios
{
    public interface INotificacionPedidoServicio
    {
        Task<ResultadoOperacionDTO> EnviarConfirmacionAsync(PedidoDTO pedido, string emailDestinatario);
        Task<ResultadoOperacionDTO> EnviarCambioEstadoAsync(PedidoDTO pedido, string emailDestinatario, EstadoPedido nuevoEstado);
    }
}
