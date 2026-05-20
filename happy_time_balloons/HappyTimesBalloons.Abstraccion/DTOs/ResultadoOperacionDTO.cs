namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public enum CodigoResultado
    {
        Exitoso,
        CredencialesInvalidas,
        CuentaBloqueada,
        EmailDuplicado,
        DatosInvalidos,
        Error
    }

    public class ResultadoOperacionDTO
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public CodigoResultado Codigo { get; set; }
        public UsuarioDTO Usuario { get; set; }

        public static ResultadoOperacionDTO Ok(string mensaje, UsuarioDTO usuario = null)
            => new ResultadoOperacionDTO { Exito = true, Mensaje = mensaje, Codigo = CodigoResultado.Exitoso, Usuario = usuario };

        public static ResultadoOperacionDTO Fallo(string mensaje, CodigoResultado codigo = CodigoResultado.Error)
            => new ResultadoOperacionDTO { Exito = false, Mensaje = mensaje, Codigo = codigo };
    }
}
