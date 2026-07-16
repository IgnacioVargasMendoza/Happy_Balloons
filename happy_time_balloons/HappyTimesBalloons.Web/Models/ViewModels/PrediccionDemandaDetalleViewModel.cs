using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class PrediccionDemandaDetalleViewModel
    {
        public PrediccionDemandaDetalleDTO Detalle { get; set; }
        public TipoPeriodo TipoPeriodoSeleccionado { get; set; }
        public string NombrePeriodo { get; set; }
    }
}
