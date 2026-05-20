using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyTimesBalloons.AccesoADatos.Modelos
{
    [Table("ImagenesProducto")]
    public class ImagenProducto
    {
        [Key]
        public int Id { get; set; }

        public int ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public virtual Producto Producto { get; set; }

        [Required, MaxLength(500)]
        public string RutaImagen { get; set; }

        public bool EsPrincipal { get; set; }

        public int Orden { get; set; }
    }
}
