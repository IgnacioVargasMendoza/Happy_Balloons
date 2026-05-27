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
    [AllowAnonymous]
    public class CuentaController : Controller
    {
        private IAuthenticationManager AuthManager
            => HttpContext.GetOwinContext().Authentication;

        private ApplicationUserManager GetUserManager()
            => ApplicationUserManager.Create(new ApplicationDbContext());

        private IAuthServicio GetAuthServicio()
            => new AuthServicio(new ApplicationDbContext());

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model);

            using (var userManager = GetUserManager())
            {
                var usuario = await userManager.FindByEmailAsync(model.Email);

                if (usuario == null)
                {
                    ModelState.AddModelError("", "Credenciales incorrectas.");
                    return View(model);
                }

                if (await userManager.IsLockedOutAsync(usuario.Id))
                {
                    ModelState.AddModelError("", "Cuenta bloqueada temporalmente por múltiples intentos fallidos.");
                    return View(model);
                }

                bool passwordValida = await userManager.CheckPasswordAsync(usuario, model.Contrasena);
                if (!passwordValida)
                {
                    await userManager.AccessFailedAsync(usuario.Id);

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

                await userManager.ResetAccessFailedCountAsync(usuario.Id);

                var identidad = await userManager.CreateIdentityAsync(
                    usuario, DefaultAuthenticationTypes.ApplicationCookie);

                AuthManager.SignIn(
                    new AuthenticationProperties { IsPersistent = model.Recordarme },
                    identidad);

                return RedirectToLocal(returnUrl);
            }
        }

        [HttpGet]
        public ActionResult Registro()
        {
            return View();
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            AuthManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }
    }
}
