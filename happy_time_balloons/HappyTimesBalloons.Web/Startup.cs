using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(HappyTimesBalloons.Web.Startup))]
namespace HappyTimesBalloons.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
