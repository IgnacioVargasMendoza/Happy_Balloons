using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyTimesBalloons.AccesoADatos.Modelos
{
    [Table("CodigoVerificacion2FA")]
    public class CodigoVerificacion2FA
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(128)]
        public string UsuarioId { get; set; }

        [Required]
        [MaxLength(64)]
        public string CodigoHash { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime FechaExpiracion { get; set; }

        public bool Utilizado { get; set; }

        public int IntentosFallidos { get; set; }

        [MaxLength(45)]
        public string IpSolicitud { get; set; }
    }
}
