using System.Collections.Generic;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class AdminPromocionesViewModel
    {
        public List<PromocionViewModel> Promociones { get; set; } = new List<PromocionViewModel>();
        public PromocionFormViewModel Formulario { get; set; }
    }
}
