using System;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class PromocionViewModel
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

        public bool EstaVencida => FechaFin < DateTime.UtcNow;
    }
}
