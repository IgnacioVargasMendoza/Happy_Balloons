using HappyTimesBalloons.Abstraccion.DTOs;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class ReporteInventarioViewModel
    {
        public int? CategoriaId { get; set; }
        public string EstadoStock { get; set; }
        public SelectList Categorias { get; set; }

        public ReporteInventarioDTO Reporte { get; set; }

        public bool TieneDatos => Reporte != null && Reporte.TotalProductos > 0;
        public bool FiltroAplicado { get; set; }
    }
}
