namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class ResultadoLoginDTO
    {
        public bool Exito { get; set; }
        public bool CuentaBloqueada { get; set; }
        public int IntentosRestantes { get; set; }
        public string Mensaje { get; set; }
        public string UsuarioId { get; set; }
        public string NombreUsuario { get; set; }
        public string Email { get; set; }
        public bool EsAdmin { get; set; }
    }
}
