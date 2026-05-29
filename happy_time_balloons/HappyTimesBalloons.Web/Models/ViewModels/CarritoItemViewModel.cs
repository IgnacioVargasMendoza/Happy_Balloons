using System;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    [Serializable]
    public class CarritoItemViewModel
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; }
        public string ImagenUrl { get; set; }
        public decimal Precio { get; set; }
        public decimal? PrecioDescuento { get; set; }
        public int Cantidad { get; set; }

        public decimal PrecioEfectivo => PrecioDescuento.HasValue && PrecioDescuento < Precio
            ? PrecioDescuento.Value : Precio;

        public decimal Subtotal => PrecioEfectivo * Cantidad;
    }
}
