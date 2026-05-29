using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.AccesoADatos.Modelos;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;

namespace HappyTimesBalloons.Web
{
    // No depende de Microsoft.AspNet.Identity.Owin (GHSA-25c8-p796-jg6r).
    // Se instancia directamente desde ApplicationDbContext sin factories OWIN.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store) : base(store) { }

        public static ApplicationUserManager Create(ApplicationDbContext ctx)
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(ctx));

            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            manager.PasswordValidator = new PasswordValidator { RequiredLength = 6 };

            // HU-AUT-001: bloqueo tras 3 intentos fallidos durante 30 min
            // TODO: leer desde ConfiguracionSistema (módulo CFG)
            manager.UserLockoutEnabledByDefault = true;
            manager.MaxFailedAccessAttemptsBeforeLockout = 3;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(30);

            return manager;
        }
    }
}
