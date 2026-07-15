using System;

namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class ZonaEntregaDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal CostoEnvio { get; set; }
        public bool EsDisponible { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
