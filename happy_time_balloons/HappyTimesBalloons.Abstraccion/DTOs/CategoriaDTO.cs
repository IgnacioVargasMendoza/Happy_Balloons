using System;

namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class CategoriaDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public bool EsActiva { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
