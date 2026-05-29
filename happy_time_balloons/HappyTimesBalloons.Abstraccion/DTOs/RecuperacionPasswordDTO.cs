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
        public string UsuarioId { get; set; }
    }
}
