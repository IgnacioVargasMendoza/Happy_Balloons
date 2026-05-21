using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class CheckoutViewModel
    {
        public List<ZonaEntregaViewModel> ZonasEntrega { get; set; } = new List<ZonaEntregaViewModel>();

        [Required(ErrorMessage = "Selecciona una zona de entrega.")]
        [Display(Name = "Zona de entrega")]
        public int ZonaEntregaId { get; set; }

        [Required(ErrorMessage = "La dirección de entrega es obligatoria.")]
        [MaxLength(500)]
        [Display(Name = "Dirección de entrega")]
        public string DireccionEntrega { get; set; }

        [Required(ErrorMessage = "Selecciona un método de pago.")]
        [Display(Name = "Método de pago")]
        public string MetodoPago { get; set; }

        [Display(Name = "Número de referencia / datos de pago")]
        public string NumeroReferencia { get; set; }

        [Display(Name = "Notas adicionales")]
        public string Notas { get; set; }

        public List<CarritoItemViewModel> ItemsCarrito { get; set; } = new List<CarritoItemViewModel>();

        public decimal Subtotal => ItemsCarrito.Sum(i => i.Subtotal);

        public decimal CostoEnvio { get; set; }

        public decimal Total => Subtotal + CostoEnvio;
    }
}
