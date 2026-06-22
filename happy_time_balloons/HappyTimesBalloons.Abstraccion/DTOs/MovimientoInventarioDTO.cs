using HappyTimesBalloons.Abstraccion.Enums;
using System;

namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class MovimientoInventarioDTO
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; }
        public TipoMovimiento TipoMovimiento { get; set; }
        public int Cantidad { get; set; }
        public int StockAnterior { get; set; }
        public int StockNuevo { get; set; }
        public string Motivo { get; set; }
        public string UsuarioId { get; set; }
        public string NombreUsuario { get; set; }
        public DateTime FechaMovimiento { get; set; }
    }
}
