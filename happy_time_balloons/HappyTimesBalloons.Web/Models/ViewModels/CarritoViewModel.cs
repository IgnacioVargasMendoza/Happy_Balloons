using System.Collections.Generic;
using System.Linq;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class CarritoViewModel
    {
        public List<CarritoItemViewModel> Items { get; set; } = new List<CarritoItemViewModel>();

        public decimal Total => Items.Sum(i => i.Subtotal);
        public int CantidadTotal => Items.Sum(i => i.Cantidad);
        public bool EstaVacio => !Items.Any();
    }
}
