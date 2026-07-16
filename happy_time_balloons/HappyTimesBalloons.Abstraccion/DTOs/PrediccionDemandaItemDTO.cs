namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class PrediccionDemandaItemDTO
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; }
        public string Categoria { get; set; }
        public double CantidadPredichaProximoPeriodo { get; set; }
        public double PromedioHistorico { get; set; }
        public double Tendencia { get; set; }
        public int PeriodosAnalizados { get; set; }
        public bool DatosSuficientes { get; set; }
    }
}
