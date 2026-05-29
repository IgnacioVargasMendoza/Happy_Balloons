using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.AccesoADatos.Migraciones;
using HappyTimesBalloons.Web.App_Start;
using System.Data.Entity;
using System.Web.Mvc;
using System.Web.Routing;

namespace HappyTimesBalloons.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_BeginRequest()
        {
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.Charset = "utf-8";
        }

        protected void Application_Start()
        {
            AutofacConfig.Register();

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            Database.SetInitializer(
                new MigrateDatabaseToLatestVersion<ApplicationDbContext, Configuration>());
            using (var ctx = new ApplicationDbContext())
            {
                ctx.Database.Initialize(force: true);
            }
        }
    }
}
