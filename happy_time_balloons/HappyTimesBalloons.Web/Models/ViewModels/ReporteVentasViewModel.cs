using HappyTimesBalloons.Abstraccion.DTOs;
using System;
using System.ComponentModel.DataAnnotations;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class ReporteVentasViewModel
    {
        [Display(Name = "Fecha inicio")]
        [DataType(DataType.Date)]
        public DateTime FechaInicio { get; set; }

        [Display(Name = "Fecha fin")]
        [DataType(DataType.Date)]
        public DateTime FechaFin { get; set; }

        public ReporteVentasDTO Reporte { get; set; }

        public bool TieneDatos => Reporte != null && Reporte.TotalPedidos > 0;
        public bool FiltroAplicado { get; set; }
    }
}
