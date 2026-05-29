using System;

namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class TokenRecuperacionDTO
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public string UsuarioId { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public bool Usado { get; set; }
        public bool EsValido => !Usado && FechaExpiracion > DateTime.UtcNow;
    }
}
