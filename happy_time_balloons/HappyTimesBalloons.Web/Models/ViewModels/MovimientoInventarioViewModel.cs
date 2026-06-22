using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class RegistrarMovimientoViewModel
    {
        [Required]
        public int ProductoId { get; set; }

        [Display(Name = "Producto")]
        public string ProductoNombre { get; set; }

        [Display(Name = "Stock actual")]
        public int StockActual { get; set; }

        [Required(ErrorMessage = "El tipo de movimiento es obligatorio.")]
        [Display(Name = "Tipo de movimiento")]
        public TipoMovimiento TipoMovimiento { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria.")]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "El motivo es obligatorio.")]
        [StringLength(500, ErrorMessage = "El motivo no puede superar 500 caracteres.")]
        [Display(Name = "Motivo")]
        public string Motivo { get; set; }
    }

    public class MovimientoInventarioItemViewModel
    {
        public int Id { get; set; }
        public TipoMovimiento TipoMovimiento { get; set; }
        public int Cantidad { get; set; }
        public int StockAnterior { get; set; }
        public int StockNuevo { get; set; }
        public string Motivo { get; set; }
        public string NombreUsuario { get; set; }
        public DateTime FechaMovimiento { get; set; }
    }

    public class MovimientoInventarioHistorialViewModel
    {
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; }
        public int StockActual { get; set; }
        public List<MovimientoInventarioItemViewModel> Movimientos { get; set; }
            = new List<MovimientoInventarioItemViewModel>();
    }
}
