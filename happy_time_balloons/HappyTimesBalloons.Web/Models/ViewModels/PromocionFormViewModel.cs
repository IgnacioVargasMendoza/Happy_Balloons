using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class PromocionFormViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un producto.")]
        [Display(Name = "Producto")]
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "El descuento es obligatorio.")]
        [Range(1, 100, ErrorMessage = "El descuento debe estar entre 1% y 100%.")]
        [Display(Name = "Descuento (%)")]
        public decimal DescuentoPorcentaje { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        [Display(Name = "Fecha de inicio")]
        [DataType(DataType.Date)]
        public DateTime FechaInicio { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        [Display(Name = "Fecha de fin")]
        [DataType(DataType.Date)]
        public DateTime FechaFin { get; set; } = DateTime.Today.AddDays(7);

        public IEnumerable<SelectListItem> Productos { get; set; }
    }
}
