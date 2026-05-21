using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyTimesBalloons.AccesoADatos.Modelos
{
    [Table("ZonasEntrega")]
    public class ZonaEntrega
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; }

        [MaxLength(500)]
        public string Descripcion { get; set; }

        public decimal CostoEnvio { get; set; }

        public bool EsDisponible { get; set; }

        public DateTime FechaCreacion { get; set; }
    }
}
