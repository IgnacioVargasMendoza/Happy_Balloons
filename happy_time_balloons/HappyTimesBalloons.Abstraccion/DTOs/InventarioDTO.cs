using System;

namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class InventarioDTO
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; }
        public string CategoriaNombre { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public DateTime FechaUltimaActualizacion { get; set; }
        public string UsuarioUltimaActualizacionId { get; set; }
    }
}
