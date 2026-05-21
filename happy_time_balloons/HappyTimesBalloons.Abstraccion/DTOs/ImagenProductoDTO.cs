namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class ImagenProductoDTO
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string RutaImagen { get; set; }
        public bool EsPrincipal { get; set; }
        public int Orden { get; set; }
    }
}
