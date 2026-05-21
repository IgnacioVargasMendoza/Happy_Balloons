using System.Collections.Generic;

namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class CheckoutItemDTO
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }

    public class CheckoutDTO
    {
        public int ZonaEntregaId { get; set; }
        public string DireccionEntrega { get; set; }
        public string MetodoPago { get; set; }
        public string NumeroReferencia { get; set; }
        public string Notas { get; set; }
        public IList<CheckoutItemDTO> Items { get; set; } = new List<CheckoutItemDTO>();
    }
}
