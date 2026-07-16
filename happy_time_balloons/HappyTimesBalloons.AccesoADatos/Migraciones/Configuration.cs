using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.AccesoADatos.Modelos;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Collections.Generic;

namespace HappyTimesBalloons.AccesoADatos.Migraciones
{
    public sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            MigrationsDirectory = "Migraciones";
        }

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

            // Horarios de entrega semilla
            if (!context.HorariosEntrega.Any())
            {
                context.HorariosEntrega.AddOrUpdate(h => h.Etiqueta,
                    new HorarioEntrega { Etiqueta = "Mañana", HoraInicio = "08:00", HoraFin = "12:00", CapacidadMaxima = 10, EsActivo = true },
                    new HorarioEntrega { Etiqueta = "Tarde",  HoraInicio = "12:00", HoraFin = "17:00", CapacidadMaxima = 10, EsActivo = true },
                    new HorarioEntrega { Etiqueta = "Noche",  HoraInicio = "17:00", HoraFin = "20:00", CapacidadMaxima = 5,  EsActivo = true }
                );
            }

            // Configuraciones del sistema — reglas operativas
            var claves = new[]
            {
                new { Clave = "EntregaDiasMinimosAdelante",    Valor = "1",        Descripcion = "Mínimo de días de anticipación para programar una entrega" },
                new { Clave = "EntregaDiasMaximosAdelante",    Valor = "30",       Descripcion = "Máximo de días en el futuro para programar una entrega" },
                new { Clave = "EntregaDiasHabilitados",        Valor = "1,2,3,4,5,6", Descripcion = "Días de entrega habilitados (DayOfWeek: 0=Dom, 1=Lun … 6=Sáb)" },
                new { Clave = "PedidoHorasMaximasCancelacion", Valor = "24",       Descripcion = "Horas desde la creación del pedido en las que el cliente puede cancelar" },
                new { Clave = "PedidoMontoMinimo",             Valor = "0",        Descripcion = "Monto mínimo en colones para confirmar un pedido (0 = sin mínimo)" },
                new { Clave = "AtencionHoraCorte",             Valor = "17:00",    Descripcion = "Hora límite para aceptar pedidos nuevos (HH:mm, 24h)" }
            };

            foreach (var k in claves)
            {
                if (!context.ConfiguracionesSistema.Any(c => c.Clave == k.Clave))
                {
                    context.ConfiguracionesSistema.Add(new ConfiguracionSistema
                    {
                        Clave = k.Clave,
                        Valor = k.Valor,
                        Descripcion = k.Descripcion,
                        FechaUltimaModificacion = DateTime.UtcNow,
                        UsuarioUltimaModificacion = "Sistema"
                    });
                }
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
                context.SaveChanges();
            }

            context.SaveChanges();
        }
    }
}
