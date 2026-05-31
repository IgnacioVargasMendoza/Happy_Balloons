using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyTimesBalloons.AccesoADatos.Modelos
{
    [Table("Inventario")]
    public class Inventario
    {
        [Key]
        public int Id { get; set; }

        public int ProductoId { get; set; }

        public virtual Producto Producto { get; set; }

        public int StockActual { get; set; }

        public int StockMinimo { get; set; }

        public DateTime FechaUltimaActualizacion { get; set; }

        [MaxLength(128)]
        public string UsuarioUltimaActualizacionId { get; set; }
    }
}
