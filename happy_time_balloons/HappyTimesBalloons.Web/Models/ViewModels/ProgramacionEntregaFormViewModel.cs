using HappyTimesBalloons.Abstraccion.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class ProgramacionEntregaFormViewModel
    {
        public int PedidoId { get; set; }

        public string NumeroPedido { get; set; }

        [Required(ErrorMessage = "Selecciona una fecha de entrega.")]
        [DataType(DataType.Date)]
        public DateTime FechaEntrega { get; set; }

        [Required(ErrorMessage = "Selecciona un horario de entrega.")]
        public int HorarioEntregaId { get; set; }

        [MaxLength(1000)]
        public string Notas { get; set; }

        public DateTime FechaMinima { get; set; }
        public DateTime FechaMaxima { get; set; }

        public List<HorarioDisponibleDTO> HorariosDisponibles { get; set; }
            = new List<HorarioDisponibleDTO>();
    }
}
