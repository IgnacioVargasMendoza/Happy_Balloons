using HappyTimesBalloons.Abstraccion.Enums;
using System;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class PedidoResumenViewModel
    {
        public int Id { get; set; }
        public string Numero { get; set; }
        public string NombreUsuario { get; set; }
        public DateTime FechaPedido { get; set; }
        public EstadoPedido EstadoPedido { get; set; }
        public decimal Total { get; set; }
        public int CantidadItems { get; set; }

        public string EstadoTexto
        {
            get
            {
                switch (EstadoPedido)
                {
                    case EstadoPedido.Pendiente:  return "Pendiente";
                    case EstadoPedido.Procesando: return "Procesando";
                    case EstadoPedido.Enviado:    return "Enviado";
                    case EstadoPedido.Entregado:  return "Entregado";
                    case EstadoPedido.Cancelado:      return "Cancelado";
                    case EstadoPedido.PagoPendiente: return "Pago pendiente";
                    default:                         return EstadoPedido.ToString();
                }
            }
        }

        public string EstadoCss
        {
            get
            {
                switch (EstadoPedido)
                {
                    case EstadoPedido.Pendiente:     return "bg-warning text-dark";
                    case EstadoPedido.Procesando:    return "bg-info text-dark";
                    case EstadoPedido.Enviado:       return "bg-primary";
                    case EstadoPedido.Entregado:     return "bg-success";
                    case EstadoPedido.Cancelado:     return "bg-danger";
                    case EstadoPedido.PagoPendiente: return "bg-dark";
                    default:                         return "bg-secondary";
                }
            }
        }
    }
}
