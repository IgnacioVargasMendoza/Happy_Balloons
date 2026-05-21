using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Repositorios
{
    public interface IPedidoRepositorio
    {
        Task<PedidoDTO> CrearAsync(PedidoDTO dto, IList<CheckoutItemDTO> items);
        Task<PedidoDTO> ObtenerPorIdAsync(int id);
        Task<List<PedidoDTO>> ObtenerPorUsuarioAsync(string userId);
        Task<List<PedidoDTO>> ObtenerTodosAsync(EstadoPedido? filtroEstado = null, string busqueda = null);
        Task<bool> ActualizarEstadoAsync(int id, EstadoPedido nuevoEstado);
        Task<int> ContarPorEstadoAsync(EstadoPedido estado);
    }
}
