using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.LogicaNegocio.Servicios;
using HappyTimesBalloons.Web.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HappyTimesBalloons.Web.Controllers
{
    // Sign-in implementado con IAuthenticationManager (Microsoft.Owin.Security),
    // sin dependencia en Microsoft.AspNet.Identity.Owin (GHSA-25c8-p796-jg6r).
    [AllowAnonymous]
    public class CuentaController : Controller
    {
        private IAuthenticationManager AuthManager
            => HttpContext.GetOwinContext().Authentication;

        private ApplicationUserManager GetUserManager()
            => ApplicationUserManager.Create(new ApplicationDbContext());

        private IAuthServicio GetAuthServicio()
            => new AuthServicio(new ApplicationDbContext());

        // GET /Cuenta/Login
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST /Cuenta/Login — HU-AUT-001: todos los escenarios
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model);

            using (var userManager = GetUserManager())
            {
                var usuario = await userManager.FindByEmailAsync(model.Email);

                // Escenario 2: usuario no existe → misma respuesta que contraseña incorrecta
                if (usuario == null)
                {
                    ModelState.AddModelError("", "Credenciales incorrectas.");
                    return View(model);
                }

                // Escenario 3: cuenta bloqueada
                if (await userManager.IsLockedOutAsync(usuario.Id))
                {
                    ModelState.AddModelError("", "Cuenta bloqueada temporalmente por múltiples intentos fallidos.");
                    return View(model);
                }

                // Verificar contraseña
                bool passwordValida = await userManager.CheckPasswordAsync(usuario, model.Contrasena);
                if (!passwordValida)
                {
                    await userManager.AccessFailedAsync(usuario.Id);

                    // Re-leer estado tras incremento
                    if (await userManager.IsLockedOutAsync(usuario.Id))
                    {
                        ModelState.AddModelError("", "Cuenta bloqueada por múltiples intentos fallidos.");
                    }
                    else
                    {
                        int intentos = await userManager.GetAccessFailedCountAsync(usuario.Id);
                        int restantes = userManager.MaxFailedAccessAttemptsBeforeLockout - intentos;
                        ModelState.AddModelError("",
                            restantes > 0
                                ? $"Contraseña incorrecta. Te quedan {restantes} intento(s)."
                                : "Credenciales incorrectas.");
                    }
                    return View(model);
                }

                // Escenario 1: login exitoso
                await userManager.ResetAccessFailedCountAsync(usuario.Id);

                var identidad = await userManager.CreateIdentityAsync(
                    usuario, DefaultAuthenticationTypes.ApplicationCookie);

                AuthManager.SignIn(
                    new AuthenticationProperties { IsPersistent = model.Recordarme },
                    identidad);

                return RedirectToLocal(returnUrl);
            }
        }

        // GET /Cuenta/Registro
        [HttpGet]
        public ActionResult Registro()
        {
            return View();
        }

        // POST /Cuenta/Registro — HU-CLI-001: registro de cliente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Registro(RegistroViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var dto = new RegistroDTO
            {
                Nombre = model.Nombre,
                Email = model.Email,
                Contrasena = model.Contrasena,
                Telefono = model.Telefono,
                Direccion = model.Direccion
            };

            var resultado = await GetAuthServicio().RegistrarAsync(dto);

            if (!resultado.Exito)
            {
                ModelState.AddModelError("", resultado.Mensaje);
                return View(model);
            }

            // Autenticar automáticamente al nuevo usuario
            using (var userManager = GetUserManager())
            {
                var usuario = await userManager.FindByEmailAsync(model.Email);
                var identidad = await userManager.CreateIdentityAsync(
                    usuario, DefaultAuthenticationTypes.ApplicationCookie);
                AuthManager.SignIn(
                    new AuthenticationProperties { IsPersistent = false },
                    identidad);
            }

            return RedirectToAction("Index", "Home");
        }

        // POST /Cuenta/Logout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            AuthManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login");
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }
    }
}
