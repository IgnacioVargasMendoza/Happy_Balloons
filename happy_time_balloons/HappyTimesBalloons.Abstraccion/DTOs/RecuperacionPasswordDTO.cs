using System;

namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class RecuperacionPasswordDTO
    {
        public string Email { get; set; }
        public string UrlBase { get; set; }
    }

    public class RestablecerPasswordDTO
    {
        public string Token { get; set; }
        public string NuevaContrasena { get; set; }
    }

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
