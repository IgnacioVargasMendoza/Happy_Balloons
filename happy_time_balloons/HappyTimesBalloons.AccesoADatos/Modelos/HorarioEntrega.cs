using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyTimesBalloons.AccesoADatos.Modelos
{
    [Table("HorariosEntrega")]
    public class HorarioEntrega
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Etiqueta { get; set; }

        [Required, MaxLength(10)]
        public string HoraInicio { get; set; }

        [Required, MaxLength(10)]
        public string HoraFin { get; set; }

        public int CapacidadMaxima { get; set; }

        public bool EsActivo { get; set; }
    }
}
