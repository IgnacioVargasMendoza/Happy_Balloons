namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class RankingProductoItemDTO
    {
        public int Posicion { get; set; }
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; }
        public string Categoria { get; set; }
        public int UnidadesVendidas { get; set; }
        public decimal Ingresos { get; set; }
        public int NumeroPedidos { get; set; }
        public decimal PorcentajeUnidades { get; set; }
    }
}
