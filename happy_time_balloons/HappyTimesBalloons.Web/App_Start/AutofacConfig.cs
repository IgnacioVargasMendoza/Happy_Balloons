using Autofac;
using Autofac.Integration.Mvc;
using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.AccesoADatos.Repositorios;
using HappyTimesBalloons.LogicaNegocio.Servicios;
using HappyTimesBalloons.Abstraccion.Interfaces.Repositorios;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.App_Start
{
    public static class AutofacConfig
    {
        public static void Register()
        {
            var builder = new ContainerBuilder();

            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            // DbContext — una instancia compartida por request HTTP
            builder.RegisterType<ApplicationDbContext>()
                   .AsSelf()
                   .InstancePerRequest();

            // Repositorios
            builder.RegisterType<CategoriaRepositorio>().As<ICategoriaRepositorio>().InstancePerRequest();
            builder.RegisterType<ProductoRepositorio>().As<IProductoRepositorio>().InstancePerRequest();
            builder.RegisterType<PedidoRepositorio>().As<IPedidoRepositorio>().InstancePerRequest();
            builder.RegisterType<ZonaEntregaRepositorio>().As<IZonaEntregaRepositorio>().InstancePerRequest();
            builder.RegisterType<BitacoraRepositorio>().As<IBitacoraRepositorio>().InstancePerRequest();
            builder.RegisterType<PromocionRepositorio>().As<IPromocionRepositorio>().InstancePerRequest();
            builder.RegisterType<RecuperacionPasswordRepositorio>().As<IRecuperacionPasswordRepositorio>().InstancePerRequest();
            builder.RegisterType<AuthRepositorio>().As<IAuthRepositorio>().InstancePerRequest();
            builder.RegisterType<CodigoVerificacion2FARepositorio>().As<ICodigoVerificacion2FARepositorio>().InstancePerRequest();
            builder.RegisterType<RolRepositorio>().As<IRolRepositorio>().InstancePerRequest();

            // Servicios
            builder.RegisterType<CategoriaServicio>().As<ICategoriaServicio>().InstancePerRequest();
            builder.RegisterType<ProductoServicio>().As<IProductoServicio>().InstancePerRequest();
            builder.RegisterType<PedidoServicio>().As<IPedidoServicio>().InstancePerRequest();
            builder.RegisterType<ZonaEntregaServicio>().As<IZonaEntregaServicio>().InstancePerRequest();
            builder.RegisterType<AuditoriaServicio>().As<IAuditoriaServicio>().InstancePerRequest();
            builder.RegisterType<AuthServicio>().As<IAuthServicio>().InstancePerRequest();
            builder.RegisterType<PromocionServicio>().As<IPromocionServicio>().InstancePerRequest();
            builder.RegisterType<RecuperacionPasswordServicio>().As<IRecuperacionPasswordServicio>().InstancePerRequest();
            builder.RegisterType<Autenticacion2FAServicio>().As<IAutenticacion2FAServicio>().InstancePerRequest();
            builder.RegisterType<RolServicio>().As<IRolServicio>().InstancePerRequest();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}
