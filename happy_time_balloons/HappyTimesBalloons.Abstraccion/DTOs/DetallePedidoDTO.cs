namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class DetallePedidoDTO
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; }
        public string ImagenUrl { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
