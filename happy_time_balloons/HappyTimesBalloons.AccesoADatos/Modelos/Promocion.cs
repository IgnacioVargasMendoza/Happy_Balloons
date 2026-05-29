using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyTimesBalloons.AccesoADatos.Modelos
{
    [Table("Promociones")]
    public class Promocion
    {
        [Key]
        public int Id { get; set; }

        public int ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public virtual Producto Producto { get; set; }

        public decimal DescuentoPorcentaje { get; set; }

        public DateTime FechaInicio { get; set; }

        public DateTime FechaFin { get; set; }

        public bool Activa { get; set; }
    }
}
