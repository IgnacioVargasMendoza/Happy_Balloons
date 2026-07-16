using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class ReglasOperativasIndexViewModel
    {
        public List<ConfiguracionItemViewModel> Configuraciones { get; set; }
    }

    public class ConfiguracionItemViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Clave")]
        public string Clave { get; set; }

        [Display(Name = "Valor actual")]
        public string Valor { get; set; }

        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Display(Name = "Última modificación")]
        public DateTime FechaUltimaModificacion { get; set; }

        [Display(Name = "Modificado por")]
        public string UsuarioUltimaModificacion { get; set; }
    }

    public class ConfiguracionEditarViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Clave")]
        public string Clave { get; set; }

        [Required(ErrorMessage = "El valor es obligatorio.")]
        [StringLength(500, ErrorMessage = "El valor no puede superar 500 caracteres.")]
        [Display(Name = "Valor")]
        public string Valor { get; set; }

        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Display(Name = "Última modificación")]
        public DateTime FechaUltimaModificacion { get; set; }

        [Display(Name = "Modificado por")]
        public string UsuarioUltimaModificacion { get; set; }
    }
}
