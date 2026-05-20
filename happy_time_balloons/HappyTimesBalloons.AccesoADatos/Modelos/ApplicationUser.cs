using Microsoft.AspNet.Identity.EntityFramework;

namespace HappyTimesBalloons.AccesoADatos.Modelos
{
    public class ApplicationUser : IdentityUser
    {
        public string Nombre { get; set; }
        public string Direccion { get; set; }
    }
}
