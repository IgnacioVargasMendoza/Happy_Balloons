using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.AccesoADatos.Inicializadores;
using System.Data.Entity;
using System.Web.Mvc;
using System.Web.Routing;

namespace HappyTimesBalloons.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Inicializar DB y sembrar roles/admin si no existen
            Database.SetInitializer(new DatabaseConfig());
            using (var ctx = new ApplicationDbContext())
            {
                ctx.Database.Initialize(false);
            }
        }
    }
}
