using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HappyTimesBalloons.AccesoADatos.Modelos
{
    [Table("Productos")]
    public class Producto
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

       // [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        //[Column(TypeName = "decimal(18,2)")]
        public decimal? PrecioDescuento { get; set; }

        public int Stock { get; set; }

        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public virtual Categoria Categoria { get; set; }

        public bool EsActivo { get; set; }

        public DateTime FechaCreacion { get; set; }

        public virtual ICollection<ImagenProducto> Imagenes { get; set; }
            = new List<ImagenProducto>();
    }
}
