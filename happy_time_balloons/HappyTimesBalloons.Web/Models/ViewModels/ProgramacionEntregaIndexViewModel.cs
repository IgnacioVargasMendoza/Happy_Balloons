using HappyTimesBalloons.Abstraccion.DTOs;
using System.Collections.Generic;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class ProgramacionEntregaIndexViewModel
    {
        public List<ProgramacionEntregaDTO> Programaciones { get; set; }
            = new List<ProgramacionEntregaDTO>();
    }
}
