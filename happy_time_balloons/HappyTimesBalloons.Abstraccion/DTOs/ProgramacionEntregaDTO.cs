using HappyTimesBalloons.Abstraccion.Enums;
using System;

namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class ProgramacionEntregaDTO
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public string NumeroPedido { get; set; }
        public int HorarioEntregaId { get; set; }
        public string EtiquetaHorario { get; set; }
        public DateTime FechaEntrega { get; set; }
        public EstadoProgramacionEntrega EstadoProgramacion { get; set; }
        public DateTime FechaProgramacion { get; set; }
        public string Notas { get; set; }
        public string UsuarioId { get; set; }
        public string NombreUsuario { get; set; }
    }
}
