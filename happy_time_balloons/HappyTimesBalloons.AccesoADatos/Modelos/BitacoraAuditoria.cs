using HappyTimesBalloons.Abstraccion.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyTimesBalloons.AccesoADatos.Modelos
{
    [Table("BitacoraAuditoria")]
    public class BitacoraAuditoria
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(128)]
        public string UsuarioId { get; set; }

        [MaxLength(256)]
        public string NombreUsuario { get; set; }

        public TipoOperacion Accion { get; set; }

        [Required, MaxLength(100)]
        public string TablaAfectada { get; set; }

        public int? RegistroId { get; set; }

        public DateTime FechaHoraUtc { get; set; }

        [MaxLength(45)]
        public string DireccionIp { get; set; }

        [MaxLength(1000)]
        public string Detalle { get; set; }
    }
}
