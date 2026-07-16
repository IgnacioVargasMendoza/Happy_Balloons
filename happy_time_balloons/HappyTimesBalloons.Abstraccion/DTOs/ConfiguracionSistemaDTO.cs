using System;

namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class ConfiguracionSistemaDTO
    {
        public int Id { get; set; }
        public string Clave { get; set; }
        public string Valor { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaUltimaModificacion { get; set; }
        public string UsuarioUltimaModificacion { get; set; }
    }
}
