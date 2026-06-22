using System;
using System.ComponentModel.DataAnnotations;

namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class InventarioDTO
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; }
        public string CategoriaNombre { get; set; }

        [Required(ErrorMessage = "El stock actual es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock actual no puede ser negativo.")]
        public int StockActual { get; set; }

        [Required(ErrorMessage = "El stock mínimo es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo.")]
        public int StockMinimo { get; set; }

        public DateTime FechaUltimaActualizacion { get; set; }
        public string UsuarioUltimaActualizacionId { get; set; }
    }
}
