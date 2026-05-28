using System;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class CategoriaViewModel
    {
        public int Id { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public bool EsActiva { get; set; }

        public DateTime FechaCreacion { get; set; }
    }
}