using HappyTimesBalloons.Abstraccion.DTOs;
using System.Collections.Generic;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class ProgramacionEntregaIndexViewModel
    {
        public List<PedidoDTO> PedidosSinProgramar { get; set; }
            = new List<PedidoDTO>();

        public List<ProgramacionEntregaDTO> ProgramacionesActivas { get; set; }
            = new List<ProgramacionEntregaDTO>();

        public List<ProgramacionEntregaDTO> ProgramacionesHistorial { get; set; }
            = new List<ProgramacionEntregaDTO>();
    }
}
