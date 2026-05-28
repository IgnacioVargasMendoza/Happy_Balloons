using HappyTimesBalloons.Abstraccion.Enums;
using System;

namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class BitacoraResumenDTO
    {
        public string NombreUsuario { get; set; }
        public TipoOperacion Accion { get; set; }
        public string TablaAfectada { get; set; }
        public DateTime FechaHoraUtc { get; set; }
        public string Detalle { get; set; }
    }
}
