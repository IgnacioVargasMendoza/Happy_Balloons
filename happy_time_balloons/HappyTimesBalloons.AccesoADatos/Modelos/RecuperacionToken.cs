using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyTimesBalloons.AccesoADatos.Modelos
{
    [Table("RecuperacionTokens")]
    public class RecuperacionToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; }

        [Required]
        public string Token { get; set; }

        public DateTime FechaExpiracion { get; set; }

        public bool Usado { get; set; }

        public DateTime FechaCreacion { get; set; }
    }
}
