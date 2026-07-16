using HappyTimesBalloons.Abstraccion.DTOs;
using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class RankingProductosViewModel
    {
        [Display(Name = "Fecha inicio")]
        [DataType(DataType.Date)]
        public DateTime FechaInicio { get; set; }

        [Display(Name = "Fecha fin")]
        [DataType(DataType.Date)]
        public DateTime FechaFin { get; set; }

        public int? ZonaId { get; set; }

        public SelectList Zonas { get; set; }

        public RankingProductosDTO Ranking { get; set; }

        public bool FiltroAplicado { get; set; }

        public bool TieneDatos => Ranking != null && Ranking.Items.Count > 0;
    }
}
