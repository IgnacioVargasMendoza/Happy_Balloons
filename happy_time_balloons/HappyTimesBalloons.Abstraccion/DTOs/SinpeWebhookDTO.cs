namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class SinpeWebhookDTO
    {
        public string NumeroComprobante { get; set; }
        public decimal Monto { get; set; }
        public string NombreTitular { get; set; }
        public string TelefonoDestino { get; set; }
        public string TokenSeguridad { get; set; }
    }
}
