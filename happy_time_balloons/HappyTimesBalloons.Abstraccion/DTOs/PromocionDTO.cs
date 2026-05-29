using System;

namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class PromocionDTO
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; }
        public decimal DescuentoPorcentaje { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public bool Activa { get; set; }

        public bool EstaVigente =>
            Activa && FechaInicio <= DateTime.UtcNow && FechaFin >= DateTime.UtcNow;
    }
}
