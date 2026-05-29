using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.AccesoADatos.Modelos;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Data.Entity;
using System.Linq;

namespace HappyTimesBalloons.AccesoADatos.Inicializadores
{
    public class DatabaseConfig : CreateDatabaseIfNotExists<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            var roleStore = new RoleStore<IdentityRole>(context);
            var roleManager = new RoleManager<IdentityRole>(roleStore);

            string[] roles = { "Administrador", "Operador", "Cliente" };
            foreach (var rol in roles)
            {
                if (!roleManager.RoleExists(rol))
                    roleManager.Create(new IdentityRole(rol));
            }

            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);
            userManager.UserLockoutEnabledByDefault = true;
            userManager.MaxFailedAccessAttemptsBeforeLockout = 3;

            const string adminEmail = "admin@happytimes.com";
            if (userManager.FindByEmail(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Nombre = "Administrador Principal",
                    EmailConfirmed = true,
                    LockoutEnabled = true
                };
                userManager.Create(admin, "Admin@123456");
                userManager.AddToRole(admin.Id, "Administrador");
            }

            // Zonas de entrega semilla
            if (!context.ZonasEntrega.Any())
            {
                var zonas = new[]
                {
                    new ZonaEntrega { Nombre = "San José Centro",  Descripcion = "Zona central de San José",              CostoEnvio = 1000m, EsDisponible = true, FechaCreacion = DateTime.UtcNow },
                    new ZonaEntrega { Nombre = "San José Norte",   Descripcion = "Tibás, Moravia, Goicoechea",             CostoEnvio = 1500m, EsDisponible = true, FechaCreacion = DateTime.UtcNow },
                    new ZonaEntrega { Nombre = "San José Sur",     Descripcion = "Desamparados, Aserrí, Acosta",           CostoEnvio = 1500m, EsDisponible = true, FechaCreacion = DateTime.UtcNow },
                    new ZonaEntrega { Nombre = "San José Oeste",   Descripcion = "Escazú, Santa Ana, Mora",                CostoEnvio = 2000m, EsDisponible = true, FechaCreacion = DateTime.UtcNow },
                    new ZonaEntrega { Nombre = "San José Este",    Descripcion = "Curridabat, La Unión, Cartago",          CostoEnvio = 2000m, EsDisponible = true, FechaCreacion = DateTime.UtcNow },
                    new ZonaEntrega { Nombre = "Heredia",          Descripcion = "Heredia central y cantones cercanos",    CostoEnvio = 2500m, EsDisponible = true, FechaCreacion = DateTime.UtcNow },
                    new ZonaEntrega { Nombre = "Alajuela",         Descripcion = "Alajuela central y Grecia",              CostoEnvio = 3000m, EsDisponible = true, FechaCreacion = DateTime.UtcNow },
                    new ZonaEntrega { Nombre = "Cartago",          Descripcion = "Cartago central y Paraíso",              CostoEnvio = 3000m, EsDisponible = true, FechaCreacion = DateTime.UtcNow },
                };
                context.ZonasEntrega.AddRange(zonas);
            }

            context.SaveChanges();
        }
    }
}
