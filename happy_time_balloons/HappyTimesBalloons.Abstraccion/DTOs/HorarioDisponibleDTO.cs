namespace HappyTimesBalloons.Abstraccion.DTOs
{
    public class HorarioDisponibleDTO
    {
        public int HorarioEntregaId { get; set; }
        public string Etiqueta { get; set; }
        public string HoraInicio { get; set; }
        public string HoraFin { get; set; }
        public int CapacidadMaxima { get; set; }
        public int ReservasActuales { get; set; }
        public int CuposRestantes => CapacidadMaxima - ReservasActuales;
        public bool TieneCupo => CuposRestantes > 0;
    }
}
