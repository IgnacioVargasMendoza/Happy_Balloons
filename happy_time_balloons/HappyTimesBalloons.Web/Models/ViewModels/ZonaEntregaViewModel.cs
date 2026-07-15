using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class ZonaEntregaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido.")]
        [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [MaxLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El costo de envío es requerido.")]
        [Range(0, 99999.99, ErrorMessage = "El costo debe ser un valor entre 0 y 99,999.99.")]
        [Display(Name = "Costo de envío")]
        public decimal CostoEnvio { get; set; }

        [Display(Name = "Disponible")]
        public bool EsDisponible { get; set; }

        [Display(Name = "Fecha de creación")]
        public DateTime FechaCreacion { get; set; }
    }

    public class ZonasEntregaIndexViewModel
    {
        public List<ZonaEntregaViewModel> Zonas { get; set; }
    }
}
