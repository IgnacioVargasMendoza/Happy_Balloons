namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class HorarioEntregaDTO
    {
        public int Id { get; set; }
        public string Etiqueta { get; set; }
        public string HoraInicio { get; set; }
        public string HoraFin { get; set; }
        public int CapacidadMaxima { get; set; }
        public bool EsActivo { get; set; }
    }
}
