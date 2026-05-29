namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class BitacoraEntradaDTO
    {
        public string UsuarioId { get; set; }
        public string NombreUsuario { get; set; }
        public HappyTimesBalloons.Abstraccion.Enums.TipoOperacion Accion { get; set; }
        public string TablaAfectada { get; set; }
        public int? RegistroId { get; set; }
        public string Detalle { get; set; }
        public string DireccionIp { get; set; }
    }
}
