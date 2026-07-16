using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using System.Collections.Generic;
using System.Linq;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class PrediccionDemandaViewModel
    {
        public List<PrediccionDemandaItemDTO> Predicciones { get; set; }
        public TipoPeriodo TipoPeriodoSeleccionado { get; set; }
        public string NombrePeriodo { get; set; }
        public int TotalProductos => Predicciones?.Count ?? 0;
        public int ProductosConDatosSuficientes => Predicciones?.Count(p => p.DatosSuficientes) ?? 0;
    }
}
