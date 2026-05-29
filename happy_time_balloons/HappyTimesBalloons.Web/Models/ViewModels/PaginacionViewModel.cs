using System.Web.Routing;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class PaginacionViewModel
    {
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public string Accion { get; set; }
        public string Controlador { get; set; }
        public RouteValueDictionary RouteValues { get; set; }

        public bool TienePaginaAnterior => PaginaActual > 1;
        public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;
    }
}
