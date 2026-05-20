using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.AccesoADatos.Modelos;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

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

            context.SaveChanges();
        }
    }
}
