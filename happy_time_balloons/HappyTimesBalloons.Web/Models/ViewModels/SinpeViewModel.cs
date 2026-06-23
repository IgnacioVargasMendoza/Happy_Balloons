using HappyTimesBalloons.Abstraccion.DTOs;
using System.Collections.Generic;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class SinpeViewModel
    {
        public IList<PagoSinpeDTO> Pagos { get; set; }
    }

    public class SinpeDetalleViewModel
    {
        public PagoSinpeDTO Pago { get; set; }
    }
}
