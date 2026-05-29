using System.Collections.Generic;

namespace HappyTimesBalloons.Web.Models.ViewModels
{
    public class CategoriaIndexViewModel
    {
        public CategoriaIndexViewModel()
        {
            Categorias = new List<CategoriaViewModel>();
        }

        public List<CategoriaViewModel> Categorias { get; set; }

        public string Busqueda { get; set; }
    }
}