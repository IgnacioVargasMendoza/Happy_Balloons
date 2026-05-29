using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTimesBalloons.Abstraccion.Interfaces.Servicios
{
    public interface IPedidoServicio
    {
        Task<ResultadoOperacionDTO<int>> CrearPedidoAsync(string userId, CheckoutDTO checkout);
        Task<PedidoDTO> ObtenerPorIdAsync(int id);
        Task<List<PedidoDTO>> ObtenerPorUsuarioAsync(string userId);
        Task<List<PedidoDTO>> ObtenerTodosAsync(EstadoPedido? filtroEstado = null, string busqueda = null);
        Task<ResultadoOperacionDTO> ActualizarEstadoAsync(int id, EstadoPedido nuevoEstado);
        Task<PedidoEstadisticasDTO> ObtenerEstadisticasAsync();
        int AjustarCantidad(int cantidadSolicitada, int stockDisponible);
    }
}
