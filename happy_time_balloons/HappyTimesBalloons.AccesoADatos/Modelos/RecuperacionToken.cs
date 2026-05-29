using System;

namespace HappyTimesBalloons.AccesoADatos.Modelos
{
    public class RecuperacionToken
    {
        public int Id { get; set; }
        public string UsuarioId { get; set; }
        public string Token { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public bool Usado { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
