using System;

namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class CodigoVerificacion2FADTO
    {
        public int Id { get; set; }
        public string UsuarioId { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public bool Utilizado { get; set; }
        public int IntentosFallidos { get; set; }
    }
}
