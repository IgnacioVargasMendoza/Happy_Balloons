using System.Collections.Generic;

namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class PrediccionDemandaDetalleDTO
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; }
        public string Categoria { get; set; }
        public List<PeriodoVentaDTO> HistorialPeriodos { get; set; }
        public double CantidadPredichaProximoPeriodo { get; set; }
        public double PromedioHistorico { get; set; }
        public double Tendencia { get; set; }
        public bool DatosSuficientes { get; set; }
        public string MensajeValidacion { get; set; }
    }
}
