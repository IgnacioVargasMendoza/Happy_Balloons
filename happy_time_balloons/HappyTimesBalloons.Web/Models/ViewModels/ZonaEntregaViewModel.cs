namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class ZonaEntregaViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal CostoEnvio { get; set; }
        public bool EsDisponible { get; set; }
    }
}
