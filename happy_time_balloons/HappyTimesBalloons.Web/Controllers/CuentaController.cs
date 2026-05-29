using HappyTimesBalloons.Abstraccion.DTOs;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using HappyTimesBalloons.AccesoADatos.Contexto;
using HappyTimesBalloons.Web.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
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
        private readonly IAuthServicio _authServicio;
        private readonly IRecuperacionPasswordServicio _recuperacionServicio;

        private IAuthenticationManager AuthManager
            => HttpContext.GetOwinContext().Authentication;

        // ApplicationUserManager requiere el contexto OWIN en runtime,
        // por lo que se instancia por demanda en lugar de inyectarse por constructor.
        private ApplicationUserManager GetUserManager()
            => ApplicationUserManager.Create(new ApplicationDbContext());

        public CuentaController(
            IAuthServicio authServicio,
            IRecuperacionPasswordServicio recuperacionServicio)
        {
            _authServicio = authServicio;
            _recuperacionServicio = recuperacionServicio;
        }

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
                        int intentos   = await userManager.GetAccessFailedCountAsync(usuario.Id);
                        int restantes  = userManager.MaxFailedAccessAttemptsBeforeLockout - intentos;
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

            var resultado = await _authServicio.RegistrarAsync(new RegistroDTO
            {
                Nombre    = model.Nombre,
                Email     = model.Email,
                Contrasena = model.Contrasena,
                Telefono  = model.Telefono,
                Direccion = model.Direccion
            });

            if (!resultado.Exito)
            {
                ModelState.AddModelError("", resultado.Mensaje);
                return View(model);
            }

            using (var userManager = GetUserManager())
            {
                var usuario  = await userManager.FindByEmailAsync(model.Email);
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

        [HttpGet]
        public ActionResult OlvideContrasena()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> OlvideContrasena(OlvideContrasenaViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var urlBase = Request.Url.GetLeftPart(UriPartial.Authority) +
                          Request.ApplicationPath.TrimEnd('/');

            var resultado = await _recuperacionServicio.SolicitarRecuperacionAsync(
                new RecuperacionPasswordDTO
                {
                    Email = model.Email,
                    UrlBase = urlBase
                });

            TempData["Email"] = model.Email;
            TempData["UrlReset"] = !string.IsNullOrEmpty(resultado.Mensaje) ? resultado.Mensaje : null;

            return RedirectToAction("ConfirmacionEnvio");
        }

        [HttpGet]
        public ActionResult ConfirmacionEnvio()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> RestablecerContrasena(string t)
        {
            if (string.IsNullOrWhiteSpace(t) || !await _recuperacionServicio.ValidarTokenAsync(t))
            {
                TempData["Error"] = "El enlace de recuperación es inválido o ha expirado.";
                return RedirectToAction("OlvideContrasena");
            }

            return View(new RestablecerContrasenaViewModel { Token = t });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RestablecerContrasena(RestablecerContrasenaViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var resultado = await _recuperacionServicio.RestablecerContrasenaAsync(
                new RestablecerPasswordDTO
                {
                    Token = model.Token,
                    NuevaContrasena = model.NuevaContrasena
                });

            if (!resultado.Exito)
            {
                ModelState.AddModelError("", resultado.Mensaje);
                return View(model);
            }

            return RedirectToAction("ConfirmacionReset");
        }

        [HttpGet]
        public ActionResult ConfirmacionReset()
        {
            return View();
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }
    }
}
