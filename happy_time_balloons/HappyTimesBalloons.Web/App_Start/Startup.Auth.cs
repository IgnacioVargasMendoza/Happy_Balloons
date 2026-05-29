using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;

namespace HappyTimesBalloons.Web
{
    // Startup.Auth simplificado: solo configura la cookie de autenticación.
    // No registra factories OWIN para UserManager/SignInManager, eliminando
    // la dependencia en Microsoft.AspNet.Identity.Owin (GHSA-25c8-p796-jg6r).
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Cuenta/Login"),
                Provider = new CookieAuthenticationProvider()
            });
        }
    }
}
