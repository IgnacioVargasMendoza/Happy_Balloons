using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyTimesBalloons.AccesoADatos.Modelos
{
    [Table("ConfiguracionesSistema")]
    public class ConfiguracionSistema
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        [Index("IX_ConfiguracionesSistema_Clave", IsUnique = true)]
        public string Clave { get; set; }

        [Required, MaxLength(500)]
        public string Valor { get; set; }

        [MaxLength(300)]
        public string Descripcion { get; set; }

        public DateTime FechaUltimaModificacion { get; set; }

        [MaxLength(128)]
        public string UsuarioUltimaModificacion { get; set; }
    }
}
